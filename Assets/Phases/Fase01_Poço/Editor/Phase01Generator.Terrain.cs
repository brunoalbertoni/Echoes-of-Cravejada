using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase01Generator
    {
        // ------------------------------------------------------------------
        // Terreno
        // ------------------------------------------------------------------

        private static void BuildTilemaps(Dictionary<string, TileBase> tiles, int groundLayer)
        {
            var gridObject = new GameObject("Grid");
            var grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            Tilemap background = CreateTilemap(gridObject.transform, "TM_Background",
                Phase01Assets.SortingTerrain, 0);
            background.color = BackWallTint;

            // Minerio e mato encostados na parede do fundo: esmaecidos pra
            // lerem como fundo, e nao como algo em que da pra pisar.
            Tilemap backProps = CreateTilemap(gridObject.transform, "TM_BackProps",
                Phase01Assets.SortingTerrain, 5);
            backProps.color = new Color(0.50f, 0.62f, 0.60f);

            Tilemap ground = CreateTilemap(gridObject.transform, "TM_Ground",
                Phase01Assets.SortingTerrain, 10);

            // A sucata, as pedras e as cercas que nao se tocam: atras do chao e
            // das plataformas (ordem menor que a do TM_Ground) e apagadas.
            Tilemap decoration = CreateTilemap(gridObject.transform, "TM_Decoration",
                Phase01Assets.SortingTerrain, 9);
            decoration.color = DecorTint;

            // O que e do proprio terreno e fica por cima dele, com a cor cheia:
            // os espinhos (machucam) e os veios de minerio (incrustados na
            // rocha; atras do chao, sumiriam).
            Tilemap surface = CreateTilemap(gridObject.transform, "TM_Surface",
                Phase01Assets.SortingTerrain, 11);

            GameObject groundObject = ground.gameObject;
            groundObject.layer = groundLayer;

            var body = groundObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;

            var tilemapCollider = groundObject.AddComponent<TilemapCollider2D>();
            var composite = groundObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

            // Fundir tudo num collider so evita que o Scrap prenda nas juntas
            // entre tiles vizinhos, que e o bug classico de platformer com tilemap.
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            // Refaz o colisor quando a cena comeca: carregada no meio do jogo
            // ela chegava sem chao.
            groundObject.AddComponent<GroundColliderRefresh>();

            PaintRoom(background, backProps, ground, decoration, surface, tiles);

            // As plataformas baixas da arena: o Scrap passa por baixo delas.
            CreateThinSteps(ground, surface, Phase01Layout.ThinSteps);

            FinishTilemap(background);
            FinishTilemap(backProps);
            FinishTilemap(ground);
            FinishTilemap(decoration);
            FinishTilemap(surface);

            // O collider so existe depois que ha tiles, entao a geometria e
            // gerada agora, e nao na hora em que o componente foi adicionado.
            composite.GenerateGeometry();
        }

        /// <summary>
        /// Consolida o que foi pintado e garante que va parar no disco.
        ///
        /// SetTile mexe na copia em memoria do tilemap; sem marcar o componente
        /// como sujo, salvar a cena pode gravar um tilemap vazio — que foi
        /// exatamente o que aconteceu aqui. O log de contagem fica pra dizer, da
        /// proxima vez, se o problema foi na pintura ou na gravacao.
        /// </summary>
        internal static void FinishTilemap(Tilemap tilemap)
        {
            tilemap.RefreshAllTiles();
            tilemap.CompressBounds();

            EditorUtility.SetDirty(tilemap);
            EditorUtility.SetDirty(tilemap.gameObject);

            BoundsInt bounds = tilemap.cellBounds;

            // Conta celulas preenchidas, e nao GetUsedTilesCount(): aquela
            // devolve quantos tipos DIFERENTES de tile o mapa usa, o que aqui da
            // meia duzia e parece falha quando na verdade esta tudo certo.
            int filled = 0;
            foreach (Vector3Int cell in bounds.allPositionsWithin)
            {
                if (tilemap.HasTile(cell)) filled++;
            }

            if (filled == 0)
            {
                Debug.LogError($"[Phase 1] {tilemap.name} ficou sem nenhuma celula preenchida. " +
                               "Se nao apareceu aviso de tile faltando acima, o problema esta na " +
                               "pintura, e nao nos nomes dos tiles.");
                return;
            }

            Debug.Log($"[Phase 1] {tilemap.name}: {filled} celulas preenchidas " +
                      $"({tilemap.GetUsedTilesCount()} tipos de tile), " +
                      $"bounds {bounds.size.x}x{bounds.size.y} a partir de ({bounds.xMin}, {bounds.yMin}).");
        }

        internal static Tilemap CreateTilemap(Transform parent, string name,
            string sortingLayer, int sortingOrder)
        {
            var tilemapObject = new GameObject(name);
            tilemapObject.transform.SetParent(parent, false);

            var tilemap = tilemapObject.AddComponent<Tilemap>();
            var renderer = tilemapObject.AddComponent<TilemapRenderer>();
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = sortingOrder;

            return tilemap;
        }

        /// <summary>
        /// Pinta todos os tilemaps de uma vez so.
        ///
        /// Monta a sala inteira em buffers e aplica cada um com um unico
        /// SetTilesBlock, em vez de uns milhares de SetTile avulsos. Alem de ser
        /// muito mais rapido, e a forma que a Unity recomenda pra preencher
        /// area grande, e ela grava de imediato — chamada a chamada, a escrita
        /// pode nao ter sido consolidada na hora de salvar a cena.
        /// </summary>
        private static void PaintRoom(Tilemap background, Tilemap backProps, Tilemap ground,
            Tilemap decoration, Tilemap surface, Dictionary<string, TileBase> tiles)
        {
            int width = Phase01Layout.Width;
            int height = Phase01Layout.Height;

            var backgroundBuffer = new TileBase[width * height];
            var backPropsBuffer = new TileBase[width * height];
            var groundBuffer = new TileBase[width * height];
            var decorationBuffer = new TileBase[width * height];
            var surfaceBuffer = new TileBase[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = Index(x, y, width);

                    // A parede de caverna atras de tudo, preenchendo a sala
                    // inteira menos as janelas. E o que faz o fundo terminar
                    // onde a sala termina, em vez de vazar pelos lados.
                    backgroundBuffer[index] = Resolve(tiles, Phase01Layout.BackgroundTileName(x, y));

                    // O terreno: o layout ja devolve a borda, a quina ou o
                    // miolo certo pra vizinhanca de cada celula.
                    groundBuffer[index] = Resolve(tiles, Phase01Layout.GroundTileName(x, y));
                }
            }

            // Os espinhos de sucata: quem machuca e o gatilho criado em
            // BuildSpikes; aqui e so o desenho.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!Phase01Layout.IsSpike(x, y)) continue;
                    surfaceBuffer[Index(x, y, width)] = Resolve(tiles, Phase01Layout.SpikeTileName(x, y));
                }
            }

            foreach (Phase01Layout.Prop prop in Phase01Layout.Props)
            {
                if (prop.X < 0 || prop.X >= width || prop.Y < 0 || prop.Y >= height) continue;

                TileBase[] buffer = Phase01Layout.IsRock(prop.X, prop.Y) ? surfaceBuffer : prop.Layer switch
                {
                    // O que ficava na frente do Scrap vai pro fundo: nao se
                    // toca, entao nao pode parecer que esta no caminho.
                    Phase01Layout.PropLayer.Back => backPropsBuffer,
                    Phase01Layout.PropLayer.Front => backPropsBuffer,
                    _ => decorationBuffer,
                };

                buffer[Index(prop.X, prop.Y, width)] = Resolve(tiles, prop.Tile);
            }

            var bounds = new BoundsInt(0, 0, 0, width, height, 1);
            background.SetTilesBlock(bounds, backgroundBuffer);
            backProps.SetTilesBlock(bounds, backPropsBuffer);
            ground.SetTilesBlock(bounds, groundBuffer);
            decoration.SetTilesBlock(bounds, decorationBuffer);
            surface.SetTilesBlock(bounds, surfaceBuffer);
        }

        /// <summary>
        /// Posicao no buffer do SetTilesBlock: x corre primeiro, depois y.
        /// </summary>
        private static int Index(int x, int y, int width)
        {
            return x + y * width;
        }

        internal static TileBase Resolve(Dictionary<string, TileBase> tiles, string name)
        {
            // Nome nulo e celula vazia de proposito: ar, ou uma janela no fundo.
            if (name == null) return null;

            if (tiles.TryGetValue(name, out TileBase tile)) return tile;

            Debug.LogWarning($"[Phase 1] Tile '{name}' nao existe no tileset fatiado.");
            return null;
        }

        // ------------------------------------------------------------------
        // O que se ve pelas janelas
        // ------------------------------------------------------------------

        private static void BuildBackdrop(Material unlitMaterial)
        {
            var root = new GameObject("Backdrop");

            // Os pedacos de parede com buraco ocupam o lugar que o
            // TM_Background deixou vazio, na mesma cor dele.
            for (int i = 0; i < Phase01Layout.Windows.Length; i++)
            {
                Phase01Layout.Window window = Phase01Layout.Windows[i];

                string sheet = window.Sprite == Phase01Layout.WindowGreat
                    ? Phase01Assets.GreatWindowsPath
                    : Phase01Assets.WindowsPath;

                var windowObject = new GameObject($"Window_{i + 1}_{window.Sprite}");
                windowObject.transform.SetParent(root.transform, false);
                windowObject.transform.position = new Vector3(window.X, window.Y, 0f);

                var renderer = windowObject.AddComponent<SpriteRenderer>();
                renderer.sprite = Phase01Assets.LoadSprite(sheet, window.Sprite);
                renderer.sortingLayerName = Phase01Assets.SortingTerrain;
                renderer.sortingOrder = 1;
                renderer.color = new Color(
                    BackWallTint.r * window.Shade,
                    BackWallTint.g * window.Shade,
                    BackWallTint.b * window.Shade);
            }

            // Os geodos colossais: a mesma faixa de serra duas vezes, em dobro
            // de tamanho. A de longe e mais clara, quase sumindo no clarao; a
            // de perto e quase preta. Cada uma fica na altura de um grupo de
            // janelas e so anda na horizontal, senao sairia de tras delas.
            Sprite silhouette = Phase01Assets.LoadSprite(
                Phase01Assets.SilhouettePath, Phase01Assets.SilhouetteSpriteName);

            CreateSilhouette(root.transform, "Geodes_Far_High", silhouette, unlitMaterial,
                new Vector3(-26f, 17f, 0f), false, new Color(0.20f, 0.32f, 0.27f), 0, 0.30f);
            CreateSilhouette(root.transform, "Geodes_Near_High", silhouette, unlitMaterial,
                new Vector3(-18f, 15f, 0f), true, new Color(0.10f, 0.15f, 0.14f), 1, 0.55f);
            CreateSilhouette(root.transform, "Geodes_Far_Low", silhouette, unlitMaterial,
                new Vector3(-30f, 3f, 0f), true, new Color(0.20f, 0.32f, 0.27f), 0, 0.30f);
            CreateSilhouette(root.transform, "Geodes_Near_Low", silhouette, unlitMaterial,
                new Vector3(-14f, 1f, 0f), false, new Color(0.10f, 0.15f, 0.14f), 1, 0.55f);

            // As da janela grande da arena, que fica mais alta que a do poco.
            // Elas comecam bem a oeste da janela porque o parallax as empurra
            // pra leste conforme a camera anda ate la: a de longe uns 29 tiles,
            // a de perto uns 19.
            CreateSilhouette(root.transform, "Geodes_Far_Arena", silhouette, unlitMaterial,
                new Vector3(0f, 21f, 0f), true, new Color(0.20f, 0.32f, 0.27f), 0, 0.30f);
            CreateSilhouette(root.transform, "Geodes_Near_Arena", silhouette, unlitMaterial,
                new Vector3(10f, 19f, 0f), false, new Color(0.10f, 0.15f, 0.14f), 1, 0.55f);
        }

        private static void CreateSilhouette(Transform parent, string name, Sprite sprite,
            Material unlitMaterial, Vector3 position, bool flipX, Color color, int sortingOrder,
            float parallaxFactor)
        {
            if (sprite == null) return;

            var layerObject = new GameObject(name);
            layerObject.transform.SetParent(parent, false);
            layerObject.transform.position = position;
            layerObject.transform.localScale = new Vector3(2f, 2f, 1f);

            var renderer = layerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.flipX = flipX;
            renderer.color = color;
            renderer.sortingLayerName = Phase01Assets.SortingParallax;
            renderer.sortingOrder = sortingOrder;
            if (unlitMaterial != null) renderer.sharedMaterial = unlitMaterial;

            var parallax = layerObject.AddComponent<ParallaxLayer>();

            var serialized = new SerializedObject(parallax);
            serialized.FindProperty("fatorParalaxe").floatValue = parallaxFactor;
            serialized.FindProperty("verticalmenteSeguir").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
