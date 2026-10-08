using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Monta a cena da Phase 1 do zero, a partir do desenho em Phase01Layout.
    ///
    /// Rodar de novo refaz a cena inteira e descarta o que voce tiver ajustado
    /// nela na mao — entao ajuste o layout no codigo enquanto estiver iterando,
    /// e so comece a mexer na cena quando o formato estiver bom.
    ///
    /// Menu:      Echoes > Gerar Phase 1 (Scrap Yard)
    ///            Echoes > Validar layout da Phase 1
    /// Batchmode: -executeMethod EchoesOfCravejada.EditorTools.Phase01Generator.GenerateFromCommandLine
    /// </summary>
    public static class Phase01Generator
    {
        public const string ScenePath = "Assets/Scenes/Phase01_ScrapYard.unity";
        private const string GroundLayerName = "Ground";

        // As camadas de paisagem bg_0X_*.png do pack continuam de fora de
        // proposito: sao cenas de exterior completas (ceu com lua, horizonte,
        // props espalhados) e, esticadas pra cobrir uma sala fechada, vazam
        // pelas paredes e se confundem com as plataformas.
        //
        // O fundo e a parede de caverna ATRAS da sala, pintada no TM_Background
        // e terminando onde a sala termina. O que ha de profundidade aparece
        // so pelas janelas dessa parede: la atras ficam as silhuetas dos geodos,
        // em parallax, contra o clarao verde do Oxide.

        /// <summary>
        /// A cor do que se ve pelas janelas: o clarao do Oxide ao longe. E tambem
        /// o fundo da camera, entao se alguma camada falhar a tela fica verde
        /// escura, e nao preta.
        /// </summary>
        private static readonly Color OxideHaze = new Color(0.06f, 0.15f, 0.12f);

        /// <summary>Verde-cobre do Oxide, usado nas luzes e na nevoa.</summary>
        private static readonly Color OxideGreen = new Color(0.36f, 0.95f, 0.60f);

        /// <summary>
        /// Escurece a parede do fundo pra ela recuar em relacao ao terreno. E o
        /// que separa "onde eu piso" de "o que esta atras", sem contorno nenhum.
        /// </summary>
        private static readonly Color BackWallTint = new Color(0.45f, 0.55f, 0.52f);

        [MenuItem("Echoes/Gerar Phase 1 (Scrap Yard)", false, 10)]
        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[Phase 1] Geracao cancelada.");
                return;
            }

            Build();
        }

        /// <summary>
        /// Confere que o mapa e navegavel sem precisar entrar no jogo.
        /// Vale rodar toda vez que voce mexer nas plataformas do Phase01Layout.
        /// </summary>
        [MenuItem("Echoes/Validar layout da Phase 1", false, 11)]
        public static void ValidateLayout()
        {
            List<string> problems = Phase01Layout.Validate();

            if (problems.Count == 0)
            {
                Debug.Log("[Phase 1] Layout OK: da pra andar pela sala inteira a partir de onde " +
                          "o Scrap nasce e chegar embaixo da boca do poco.");
                return;
            }

            foreach (string problem in problems)
            {
                Debug.LogWarning($"[Phase 1] {problem}");
            }

            Debug.LogError($"[Phase 1] {problems.Count} problema(s) no layout. Veja os avisos acima.");
        }

        /// <summary>Ponto de entrada pro modo batch, sem nenhum dialogo.</summary>
        public static void GenerateFromCommandLine()
        {
            Build();
        }

        private static void Build()
        {
            Phase01Assets.EnsureFolders();
            int groundLayer = Phase01Assets.EnsureLayer(GroundLayerName);

            // Precisa vir antes de montar a cena: atribuir sortingLayerName a um
            // nome que nao existe faz a Unity cair calada de volta pra Default.
            Phase01Assets.EnsureSortingLayers();
            Phase01Assets.VerifySortingLayers();

            // Um mapa quebrado vira uma cena que nao da pra jogar, entao o
            // aviso vem antes de gastar tempo gerando.
            foreach (string problem in Phase01Layout.Validate())
            {
                Debug.LogWarning($"[Phase 1] {problem}");
            }

            if (!Phase01Assets.SliceTileset())
            {
                Debug.LogError("[Phase 1] Abortado: nao consegui fatiar o tileset.");
                return;
            }

            if (!Phase01Assets.SliceBackgroundTileset())
            {
                Debug.LogError("[Phase 1] Abortado: nao consegui fatiar a parede de fundo.");
                return;
            }

            if (!Phase01Assets.SliceObjects())
            {
                Debug.LogError("[Phase 1] Abortado: nao consegui fatiar as folhas de objetos.");
                return;
            }

            if (!Phase01Assets.SliceBackdropPieces())
            {
                Debug.LogError("[Phase 1] Abortado: nao consegui recortar as janelas e as silhuetas do fundo.");
                return;
            }

            Dictionary<string, TileBase> tiles = Phase01Assets.BuildTiles();
            if (tiles.Count == 0)
            {
                Debug.LogError("[Phase 1] Abortado: nenhum tile foi criado.");
                return;
            }

            if (!Phase02Assets.EnsureMechanicsArt())
            {
                Debug.LogError("[Phase 1] Abortado: nao consegui preparar a arte das mecanicas.");
                return;
            }

            Sprite scrapSprite = Phase01Assets.CreateScrapPlaceholder();
            Sprite polishedSprite = Phase02Assets.LoadSprite(Phase02Assets.ScrapPolished);
            Material fogMaterial = Phase01Assets.GetOrCreateFogMaterial();
            Material unlitMaterial = Phase01Assets.GetOrCreateUnlitMaterial();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildTilemaps(tiles, groundLayer);
            BuildBackdrop(unlitMaterial);
            Transform scrap = BuildScrap(scrapSprite, polishedSprite, groundLayer, Phase01Layout.SpawnTile);
            BuildCamera(scrap, Phase01Layout.Width, Phase01Layout.Height, OxideHaze);
            BuildLighting();
            BuildFog(fogMaterial);
            BuildDust(fogMaterial);
            BuildMarkers();
            BuildTravel();
            BuildMechanics(groundLayer);
            BuildBossAndPurification(groundLayer, fogMaterial);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            Debug.Log($"[Phase 1] Cena gerada: {ScenePath}. " +
                      $"Mapa {Phase01Layout.Width}x{Phase01Layout.Height} (poco + arena), " +
                      $"{Phase01Layout.Props.Length} objetos de cenario, " +
                      $"{Phase01Layout.Windows.Length} janelas.");
        }

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

            // A decoracao fica na frente do chao, mas a camada Terrain inteira
            // fica atras de Actors — entao a sucata e os veios de minerio ficam
            // atras do Scrap, e nao colados na frente dele.
            Tilemap decoration = CreateTilemap(gridObject.transform, "TM_Decoration",
                Phase01Assets.SortingTerrain, 20);

            // O que passa na frente do Scrap fica quase em silhueta, senao
            // disputa a atencao com ele.
            Tilemap foreground = CreateTilemap(gridObject.transform, "TM_Foreground",
                Phase01Assets.SortingForeground, 0);
            foreground.color = new Color(0.30f, 0.36f, 0.36f);

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

            PaintRoom(background, backProps, ground, decoration, foreground, tiles);

            FinishTilemap(background);
            FinishTilemap(backProps);
            FinishTilemap(ground);
            FinishTilemap(decoration);
            FinishTilemap(foreground);

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
            Tilemap decoration, Tilemap foreground, Dictionary<string, TileBase> tiles)
        {
            int width = Phase01Layout.Width;
            int height = Phase01Layout.Height;

            var backgroundBuffer = new TileBase[width * height];
            var backPropsBuffer = new TileBase[width * height];
            var groundBuffer = new TileBase[width * height];
            var decorationBuffer = new TileBase[width * height];
            var foregroundBuffer = new TileBase[width * height];

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
                    decorationBuffer[Index(x, y, width)] = Resolve(tiles, Phase01Layout.SpikeTileName(x, y));
                }
            }

            foreach (Phase01Layout.Prop prop in Phase01Layout.Props)
            {
                if (prop.X < 0 || prop.X >= width || prop.Y < 0 || prop.Y >= height) continue;

                TileBase[] buffer = prop.Layer switch
                {
                    Phase01Layout.PropLayer.Back => backPropsBuffer,
                    Phase01Layout.PropLayer.Front => foregroundBuffer,
                    _ => decorationBuffer,
                };

                buffer[Index(prop.X, prop.Y, width)] = Resolve(tiles, prop.Tile);
            }

            var bounds = new BoundsInt(0, 0, 0, width, height, 1);
            background.SetTilesBlock(bounds, backgroundBuffer);
            backProps.SetTilesBlock(bounds, backPropsBuffer);
            ground.SetTilesBlock(bounds, groundBuffer);
            decoration.SetTilesBlock(bounds, decorationBuffer);
            foreground.SetTilesBlock(bounds, foregroundBuffer);
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
            serialized.FindProperty("parallaxFactor").floatValue = parallaxFactor;
            serialized.FindProperty("followVertically").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // ------------------------------------------------------------------
        // Protagonista
        // ------------------------------------------------------------------

        /// <summary>
        /// Monta o Scrap. E o mesmo em todas as fases: o que muda e so onde
        /// ele nasce.
        /// </summary>
        internal static Transform BuildScrap(Sprite sprite, Sprite polishedSprite, int groundLayer,
            (int X, int Y) spawn)
        {
            var scrap = new GameObject("Scrap");
            scrap.transform.position = new Vector3(spawn.X + 0.5f, spawn.Y, 0f);

            // Os quadros de verdade do personagem. Os dois sprites recebidos sao
            // os placeholders antigos, e so entram se a arte nova nao existir.
            Sprite[] scrapFrames = Phase02Assets.HeroFrames(false);
            Sprite[] polishedFrames = Phase02Assets.HeroFrames(true);

            if (scrapFrames[0] != null) sprite = scrapFrames[0];
            if (polishedFrames[0] != null) polishedSprite = polishedFrames[0];

            var renderer = scrap.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = Phase01Assets.SortingActors;
            renderer.sortingOrder = 0;

            var body = scrap.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = scrap.AddComponent<CapsuleCollider2D>();
            collider.direction = CapsuleDirection2D.Vertical;
            collider.size = new Vector2(0.8f, 1.2f);
            collider.offset = new Vector2(0f, 0.6f);
            collider.sharedMaterial = GetOrCreateFrictionlessMaterial();

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(scrap.transform, false);
            groundCheck.transform.localPosition = new Vector3(0f, 0.06f, 0f);

            var controller = scrap.AddComponent<ScrapController>();

            // Os campos do controller sao privados com [SerializeField], entao
            // a ligacao e feita pelo SerializedObject.
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
            serialized.FindProperty("groundLayers").intValue = 1 << groundLayer;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // O que ele carrega de uma cena pra outra: a aparencia (sucata ou
            // polido), a chegada no ponto certo e a volta pro chao seguro
            // quando cai no acido.
            // Os olhos dele iluminam em volta. Sem isso ele some nos trechos
            // escuros: a ferrugem do corpo tem quase a cor da sombra.
            CreatePointLight(scrap.transform, "EyeGlow", (spawn.X + 0.5f, spawn.Y + 1.15f),
                new Color(1f, 0.90f, 0.70f), 0.75f, 4.5f);

            scrap.AddComponent<ScrapAppearance>().Configure(sprite, polishedSprite);
            scrap.AddComponent<ScrapRespawn>();
            scrap.AddComponent<SceneArrival>();
            scrap.AddComponent<ScrapHealth>().Configure(Phase02Assets.LoadSprite(Phase02Assets.OldScrap));

            // O risco do cinzel: um sprite filho, sem luz, que so aparece na
            // hora do golpe.
            var slashObject = new GameObject("ChiselSlash");
            slashObject.transform.SetParent(scrap.transform, false);

            var slash = slashObject.AddComponent<SpriteRenderer>();
            slash.sprite = Phase02Assets.LoadSprite(Phase02Assets.ChiselSlash);
            slash.sortingLayerName = Phase01Assets.SortingActors;
            slash.sortingOrder = 5;

            Material unlit = Phase01Assets.GetOrCreateUnlitMaterial();
            if (unlit != null) slash.sharedMaterial = unlit;
            slash.enabled = false;

            scrap.AddComponent<ScrapChisel>().Configure(slash);

            // O Steam Dash ja vai junto, desligado: quem liga e o Rusty Scrapper, ao cair.
            scrap.AddComponent<ScrapDash>();

            // Por ultimo, porque le o estado de todos os outros.
            scrap.AddComponent<ScrapAnimator>().Configure(scrapFrames, polishedFrames);

            return scrap.transform;
        }

        /// <summary>
        /// Sem atrito: senao o Scrap gruda na lateral das paredes ao encostar
        /// no ar. Toda a velocidade horizontal vem do controller.
        /// </summary>
        private static PhysicsMaterial2D GetOrCreateFrictionlessMaterial()
        {
            const string path = Phase01Assets.GeneratedRoot + "/ScrapFrictionless.physicsMaterial2D";

            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (material != null) return material;

            material = new PhysicsMaterial2D("ScrapFrictionless")
            {
                friction = 0f,
                bounciness = 0f,
            };

            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssets();
            return material;
        }

        // ------------------------------------------------------------------
        // Camera, luz e poeira
        // ------------------------------------------------------------------

        internal static void BuildCamera(Transform scrap, int mapWidth, int mapHeight, Color background)
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.nearClipPlane = -100f;
            camera.farClipPlane = 100f;

            // 480x270 a 16 PPU mostra ~17 tiles de altura: o Scrap fica pequeno
            // no quadro o bastante pra a sala respirar.
            var pixelPerfect = cameraObject.AddComponent<PixelPerfectCamera>();
            pixelPerfect.assetsPPU = Phase01Assets.PixelsPerUnit;
            pixelPerfect.refResolutionX = 480;
            pixelPerfect.refResolutionY = 270;

            camera.orthographicSize = 270f / (2f * Phase01Assets.PixelsPerUnit);

            cameraObject.AddComponent<AudioListener>();

            cameraObject.transform.position = new Vector3(
                scrap.position.x, scrap.position.y + 2f, -10f);

            var follow = cameraObject.AddComponent<ScrapCameraFollow>();
            follow.Configure(scrap, new Rect(0f, 0f, mapWidth, mapHeight));

            cameraObject.AddComponent<GameHud>();
        }

        private static void BuildLighting()
        {
            var root = new GameObject("Lighting");

            var globalObject = new GameObject("GlobalLight2D");
            globalObject.transform.SetParent(root.transform, false);

            // Fraca, fria e esverdeada: a sala e escura e doente, e o que da
            // pra ver vem dos pontos de luz. Subir a intensidade daqui e o
            // jeito mais rapido de clarear tudo se ficar dificil de enxergar.
            var global = globalObject.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.color = new Color(0.62f, 0.80f, 0.78f);
            global.intensity = 0.5f;

            // A unica luz quente do poco, em volta de onde o Scrap acorda: ele
            // tem que ser o ponto mais vivo da tela.
            CreatePointLight(root.transform, "WakeGlow", Phase01Layout.WakeGlowPosition,
                new Color(1f, 0.80f, 0.55f), 0.8f, 6f);

            // O lampiao da bancada: a unica luz quente e parada dos andares de cima.
            for (int i = 0; i < Phase01Layout.LampGlows.Length; i++)
            {
                (float X, float Y, float Radius) lamp = Phase01Layout.LampGlows[i];
                CreatePointLight(root.transform, $"LampGlow_{i + 1}", (lamp.X, lamp.Y),
                    new Color(1f, 0.82f, 0.55f), 0.9f, lamp.Radius);
            }

            // A luz fria descendo pela chamine: cai em cima do patamar do topo
            // e mostra pra onde e a subida antes de voce conseguir chegar la.
            CreatePointLight(root.transform, "ChimneyLight", Phase01Layout.ChimneyLightPosition,
                new Color(0.62f, 0.85f, 0.95f), 0.9f, 11f);

            // Uma brasa cor de ferrugem na boca do tunel da arena. Fraca de
            // proposito: e um aviso, nao uma lanterna.
            CreatePointLight(root.transform, "LairGlow", Phase01Layout.LairGlowPosition,
                new Color(1f, 0.42f, 0.22f), 0.55f, 5f);

            // E o Oxide, verde-cobre, no chao e atras das janelas.
            for (int i = 0; i < Phase01Layout.OxideGlows.Length; i++)
            {
                (float X, float Y, float Radius) glow = Phase01Layout.OxideGlows[i];
                CreatePointLight(root.transform, $"OxideGlow_{i + 1}", (glow.X, glow.Y),
                    OxideGreen, 0.7f, glow.Radius);
            }
        }

        internal static Light2D CreatePointLight(Transform parent, string name, (float X, float Y) position,
            Color color, float intensity, float outerRadius)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.position = new Vector3(position.X, position.Y, 0f);

            var light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.falloffIntensity = 0.7f;

            // O raio nao tem setter publico nesta versao da URP, entao vai
            // pelos campos serializados.
            var serialized = new SerializedObject(light);
            SetFloatIfPresent(serialized, "m_PointLightOuterRadius", outerRadius);
            SetFloatIfPresent(serialized, "m_PointLightInnerRadius", outerRadius * 0.25f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return light;
        }

        private static void SetFloatIfPresent(SerializedObject serialized, string path, float value)
        {
            SerializedProperty property = serialized.FindProperty(path);
            if (property != null) property.floatValue = value;
        }

        /// <summary>
        /// A nevoa do Oxide, em faixas deitadas — quais e onde, o layout diz.
        ///
        /// Ela so aparece com o jogo rodando (ou com o objeto selecionado na
        /// cena): sao particulas, e o editor nao simula o que nao esta em foco.
        /// </summary>
        private static void BuildFog(Material material)
        {
            var root = new GameObject("OxideFog");

            foreach (Phase01Layout.FogBank bank in Phase01Layout.FogBanks)
            {
                CreateFogBank(root.transform, bank, material);
            }
        }

        private static void CreateFogBank(Transform parent, Phase01Layout.FogBank bank, Material material)
        {
            var fogObject = new GameObject(bank.Name);
            fogObject.transform.SetParent(parent, false);
            fogObject.transform.position = new Vector3(bank.X, bank.Y, 0f);

            var particles = fogObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            // Senao a sala comeca limpa e a nevoa leva meio minuto pra chegar.
            main.prewarm = true;
            main.startLifetime = 22f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(5f, 9f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(OxideGreen.r, OxideGreen.g, OxideGreen.b, bank.Alpha);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 200;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = bank.Rate;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(bank.Width, bank.Thickness, 1f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.04f, 0.04f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            // Cada borrao aparece e some devagar, senao da pra ver eles
            // nascendo e morrendo.
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.25f),
                    new GradientAlphaKey(1f, 0.75f),
                    new GradientAlphaKey(0f, 1f),
                });

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fade);

            // Atras do terreno a nevoa some dentro da rocha, que e o certo; na
            // frente ela passa por cima do Scrap e do chao.
            var renderer = fogObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = bank.InFront
                ? Phase01Assets.SortingForeground
                : Phase01Assets.SortingTerrain;
            renderer.sortingOrder = bank.InFront ? 5 : 4;
            if (material != null) renderer.sharedMaterial = material;

            particles.Stop();
        }

        // A poeira cai com uma velocidade inicial e vai acelerando.
        private const float DustGravityModifier = 0.02f;
        private const float DustSlowestFall = 0.2f;
        private const float DustFastestFall = 0.5f;

        /// <summary>Metal dust: poeira de minerio descendo devagar, sala por sala.</summary>
        private static void BuildDust(Material material)
        {
            var root = new GameObject("MetalDust");

            foreach ((string name, float x, float width, float top, float fall, float rate)
                     in Phase01Layout.DustFalls)
            {
                var dustObject = new GameObject(name);
                dustObject.transform.SetParent(root.transform, false);
                dustObject.transform.position = new Vector3(x, top, 0f);

                var particles = dustObject.AddComponent<ParticleSystem>();

                ParticleSystem.MainModule main = particles.main;
                main.loop = true;
                main.prewarm = true;
                // O grao morre mais ou menos na altura do chao. Vivendo mais que
                // isso ele atravessa o piso e aparece caindo por dentro da rocha.
                main.startLifetime = DustLifetime(fall);
                main.startSpeed = 0f;
                main.startSize = 0.12f;
                // Clara e esverdeada, pra aparecer contra o fundo escuro.
                main.startColor = new Color(0.70f, 0.85f, 0.75f, 0.55f);
                main.gravityModifier = DustGravityModifier;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 400;

                ParticleSystem.EmissionModule emission = particles.emission;
                emission.rateOverTime = rate;

                ParticleSystem.ShapeModule shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(width, 1f, 1f);

                ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.World;
                velocity.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
                velocity.y = new ParticleSystem.MinMaxCurve(-DustFastestFall, -DustSlowestFall);
                velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

                var renderer = dustObject.GetComponent<ParticleSystemRenderer>();
                renderer.sortingLayerName = Phase01Assets.SortingForeground;
                renderer.sortingOrder = 10;
                if (material != null) renderer.sharedMaterial = material;

                particles.Stop();
            }
        }

        /// <summary>
        /// Quanto tempo um grao de poeira leva pra cair tantos tiles, na
        /// velocidade media: resolve d = v*t + a*t²/2 pra t.
        /// </summary>
        private static float DustLifetime(float fall)
        {
            float speed = (DustSlowestFall + DustFastestFall) * 0.5f;
            float acceleration = Mathf.Abs(Physics.gravity.y) * DustGravityModifier;

            return (-speed + Mathf.Sqrt(speed * speed + 2f * acceleration * fall)) / acceleration;
        }

        /// <summary>
        /// Objetos vazios marcando os lugares que as mecanicas vao precisar.
        /// Nao fazem nada sozinhos: sao so pontos com nome pra ligar depois.
        /// </summary>
        /// <summary>
        /// Os pontos de chegada desta cena. A ida pro District 2 fica depois do
        /// abismo e e montada junto com o boss, que e quem a destranca.
        /// </summary>
        private static void BuildTravel()
        {
            var root = new GameObject("Travel");

            SceneKit.CreateSpawnPoint(root.transform, Phase01Layout.StartSpawnId, Phase01Layout.SpawnTile);
            SceneKit.CreateSpawnPoint(root.transform, Phase01Layout.FromDistrict2SpawnId, Phase01Layout.ReturnTile);
        }

        // ------------------------------------------------------------------
        // O mini boss, a saida e a purificacao
        // ------------------------------------------------------------------

        /// <summary>
        /// Monta o Rusty Scrapper, a grade da saida da arena, a passagem pro
        /// District 2 (que so aparece depois) e o AreaPurifier que amarra tudo
        /// o que muda quando ele cai.
        ///
        /// Vem por ultimo no Build porque procura pelo nome as luzes, a nevoa,
        /// os tilemaps e a camera que os outros passos criaram.
        /// </summary>
        private static void BuildBossAndPurification(int groundLayer, Material steamMaterial)
        {
            var root = new GameObject("Boss");

            // --- A grade da saida ---
            (int X, int Y, int Height) gateCells = Phase01Layout.ExitGate;

            var gateObject = new GameObject("Gate_ArenaExit") { layer = groundLayer };
            gateObject.transform.SetParent(root.transform, false);
            gateObject.transform.position = new Vector3(gateCells.X, gateCells.Y, 0f);

            var bars = gateObject.AddComponent<SpriteRenderer>();
            bars.sprite = Phase02Assets.LoadSprite(Phase02Assets.GateBars);
            bars.drawMode = SpriteDrawMode.Tiled;
            bars.size = new Vector2(1f, gateCells.Height);
            bars.sortingLayerName = Phase01Assets.SortingTerrain;
            bars.sortingOrder = 15;

            var wall = gateObject.AddComponent<BoxCollider2D>();
            wall.size = new Vector2(1f, gateCells.Height);
            wall.offset = new Vector2(0.5f, gateCells.Height * 0.5f);

            var exitGate = gateObject.AddComponent<Gate>();
            exitGate.Configure(gateCells.Height + 1f);

            // --- A passagem pro District 2: vapor e saida, desligados ate a purificacao ---
            var passage = new GameObject("District2Passage");
            passage.transform.SetParent(root.transform, false);

            LevelMap.Lift vent = Phase01Layout.District2Vent;
            var ventCells = new RectInt(vent.X0, vent.Bottom, vent.X1 - vent.X0 + 1, vent.Top - vent.Bottom + 1);

            SceneKit.CreateSteamVent(passage.transform, "Lift_ToDistrict2", ventCells, SteamVent.Mode.Lift, 1,
                0f, 0f, steamMaterial);

            var nozzle = new GameObject("Nozzle");
            nozzle.transform.SetParent(passage.transform, false);
            nozzle.transform.position = new Vector3(vent.X0, vent.Bottom - 1, 0f);

            var nozzleSprite = nozzle.AddComponent<SpriteRenderer>();
            nozzleSprite.sprite = Phase02Assets.LoadSprite(Phase02Assets.NozzleName(ventCells.width));
            nozzleSprite.sortingLayerName = Phase01Assets.SortingTerrain;
            nozzleSprite.sortingOrder = 21;

            (int X, int Y, int Width, int Height) exit = Phase01Layout.District2Exit;
            SceneKit.CreateSceneExit(passage.transform, "Exit_ToDistrict2",
                new RectInt(exit.X, exit.Y, exit.Width, exit.Height),
                Phase02Layout.SceneName, Phase02Layout.FromDistrict1SpawnId);

            // --- Os lampioes que so acendem depois ---
            GameObject lighting = GameObject.Find("Lighting");
            var corrupted = new List<Light2D>();
            var purified = new List<Light2D>();
            Light2D global = null;

            if (lighting != null)
            {
                foreach (Light2D light in lighting.GetComponentsInChildren<Light2D>())
                {
                    if (light.lightType == Light2D.LightType.Global) global = light;
                    else if (light.name.StartsWith("OxideGlow") || light.name == "LairGlow") corrupted.Add(light);
                }

                // Um lampiao quente onde havia cada brilho de Oxide. O
                // AreaPurifier os apaga ao comecar e acende na purificacao.
                for (int i = 0; i < Phase01Layout.OxideGlows.Length; i++)
                {
                    (float X, float Y, float Radius) glow = Phase01Layout.OxideGlows[i];
                    purified.Add(CreatePointLight(lighting.transform, $"PurifiedLamp_{i + 1}", (glow.X, glow.Y),
                        new Color(1f, 0.82f, 0.55f), 0.8f, glow.Radius));
                }
            }

            GameObject fogRoot = GameObject.Find("OxideFog");
            ParticleSystem[] fog = fogRoot != null
                ? fogRoot.GetComponentsInChildren<ParticleSystem>()
                : new ParticleSystem[0];

            var tilemaps = new List<Tilemap>();
            GameObject background = GameObject.Find("TM_Background");
            if (background != null) tilemaps.Add(background.GetComponent<Tilemap>());

            GameObject cameraObject = GameObject.Find("Main Camera");
            GameObject enemies = GameObject.Find("Enemies");

            // --- O boss ---
            (int X, int Y, int Width, int Height) arena = Phase01Layout.ArenaBounds;
            (int X, int Y) spawn = Phase01Layout.BossSpawnTile;

            var bossObject = new GameObject("RustyScrapper");
            bossObject.transform.SetParent(root.transform, false);
            bossObject.transform.position = new Vector3(spawn.X + 6.5f, spawn.Y, 0f);

            var bossSprite = bossObject.AddComponent<SpriteRenderer>();
            bossSprite.sprite = Phase02Assets.LoadSprite(Phase02Assets.RustyScrapper);
            bossSprite.sortingLayerName = Phase01Assets.SortingActors;
            bossSprite.sortingOrder = -2;

            // Gatilho, como os outros inimigos: encostar machuca, mas ele nao
            // empurra o Scrap pra dentro da parede.
            var bossBody = bossObject.AddComponent<BoxCollider2D>();
            bossBody.isTrigger = true;
            bossBody.size = new Vector2(2.2f, 2.6f);
            bossBody.offset = new Vector2(0f, 1.4f);

            var rigidbody = bossObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;

            // A nuvem de Oxide da metade da luta: uma baforada so, sem dano.
            var cloudObject = new GameObject("OxideCloud");
            cloudObject.transform.SetParent(bossObject.transform, false);
            cloudObject.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            var cloud = cloudObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = cloud.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 1.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startColor = new Color(OxideGreen.r, OxideGreen.g, OxideGreen.b, 0.35f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 120;

            ParticleSystem.EmissionModule emission = cloud.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

            ParticleSystem.ShapeModule shape = cloud.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;

            ParticleSystem.ColorOverLifetimeModule fade = cloud.colorOverLifetime;
            fade.enabled = true;
            fade.color = new ParticleSystem.MinMaxGradient(SceneKit.FadeInOut(0.1f, 0.5f));

            var cloudRenderer = cloudObject.GetComponent<ParticleSystemRenderer>();
            cloudRenderer.sortingLayerName = Phase01Assets.SortingForeground;
            cloudRenderer.sortingOrder = 8;
            if (steamMaterial != null) cloudRenderer.sharedMaterial = steamMaterial;

            cloud.Stop();

            // --- O que muda quando ele cai ---
            var purifierObject = new GameObject("AreaPurifier");
            purifierObject.transform.SetParent(root.transform, false);

            var purifier = purifierObject.AddComponent<AreaPurifier>();
            purifier.Configure(global, corrupted.ToArray(), purified.ToArray(), fog, tilemaps.ToArray(),
                cameraObject != null ? cameraObject.GetComponent<Camera>() : null,
                new[] { passage },
                enemies != null ? new[] { enemies } : new GameObject[0],
                new[] { exitGate });

            // Ele anda so no fosso, e acorda quando o Scrap pisa em qualquer
            // lugar da arena.
            bossObject.AddComponent<RustyScrapper>().Configure(
                52.8f + 0.9f, 74f - 0.9f - 0.8f, spawn.Y,
                new Rect(arena.X, arena.Y, arena.Width, 10f),
                Phase02Assets.LoadSprite(Phase02Assets.ScrapChunk),
                Phase02Assets.LoadSprite(Phase02Assets.ShockWave),
                Phase02Assets.LoadSprite(Phase02Assets.Shard),
                Phase02Assets.LoadSprite(Phase02Assets.DashModule),
                cloud, exitGate, purifier);
        }

        // ------------------------------------------------------------------
        // Mecanicas: espinhos, inimigos, alavanca, portao e bancada
        // ------------------------------------------------------------------

        private static void BuildMechanics(int groundLayer)
        {
            var root = new GameObject("Mechanics");

            BuildSpikes(root.transform);
            BuildCrawlers(root.transform);
            BuildLeverAndGate(root.transform, groundLayer);
            BuildWorkbench(root.transform);
            BuildNpcs(root.transform);
        }

        private static void BuildNpcs(Transform parent)
        {
            var root = new GameObject("Npcs");
            root.transform.SetParent(parent, false);

            foreach (Phase01Layout.NpcSpot spot in Phase01Layout.Npcs)
            {
                var npcObject = new GameObject("Npc_" + spot.Name);
                npcObject.transform.SetParent(root.transform, false);
                npcObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);

                var renderer = npcObject.AddComponent<SpriteRenderer>();
                renderer.sprite = Phase02Assets.LoadSprite(Phase02Assets.SkeletonName(spot.Variant));
                renderer.sortingLayerName = Phase01Assets.SortingActors;
                renderer.sortingOrder = -4;

                // A area em volta onde da pra conversar.
                var area = npcObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(3f, 2.5f);
                area.offset = new Vector2(0f, 1.25f);

                npcObject.AddComponent<Npc>().Configure(spot.Name, spot.Lines);

                // Uma luz fraca em cima dele, pra dar pra achar no escuro.
                CreatePointLight(npcObject.transform, "Glow", (spot.X + 0.5f, spot.Y + 1f),
                    new Color(0.80f, 0.95f, 0.90f), 0.55f, 3.5f);
            }
        }

        /// <summary>
        /// Um gatilho de dano por fileira de espinhos. Os espinhos em si sao
        /// tiles, pintados junto com o resto do cenario.
        /// </summary>
        private static void BuildSpikes(Transform parent)
        {
            var root = new GameObject("Spikes");
            root.transform.SetParent(parent, false);

            for (int y = 0; y < Phase01Layout.Height; y++)
            {
                int start = -1;

                for (int x = 0; x <= Phase01Layout.Width; x++)
                {
                    bool spike = x < Phase01Layout.Width && Phase01Layout.IsSpike(x, y);

                    if (spike && start < 0) start = x;
                    if (spike || start < 0) continue;

                    int width = x - start;

                    var spikes = new GameObject($"Spikes_{start}_{y}");
                    spikes.transform.SetParent(root.transform, false);

                    // As pontas ocupam a metade de baixo do tile; o gatilho tambem.
                    spikes.transform.position = new Vector3(start + width * 0.5f, y + 0.35f, 0f);

                    var trigger = spikes.AddComponent<BoxCollider2D>();
                    trigger.isTrigger = true;
                    trigger.size = new Vector2(width - 0.3f, 0.7f);

                    spikes.AddComponent<Hazard>();
                    start = -1;
                }
            }
        }

        private static void BuildCrawlers(Transform parent)
        {
            var root = new GameObject("Enemies");
            root.transform.SetParent(parent, false);

            Sprite[] walk = Phase02Assets.CrawlerWalkSprites();
            Sprite shard = Phase02Assets.LoadSprite(Phase02Assets.Shard);

            foreach (Phase01Layout.Patrol patrol in Phase01Layout.Crawlers)
            {
                var crawler = new GameObject(patrol.Name);
                crawler.transform.SetParent(root.transform, false);
                crawler.transform.position = new Vector3((patrol.X0 + patrol.X1 + 1) * 0.5f, patrol.Y + 0.5f, 0f);

                var renderer = crawler.AddComponent<SpriteRenderer>();
                renderer.sprite = walk.Length > 0 ? walk[0] : null;
                renderer.sortingLayerName = Phase01Assets.SortingActors;
                renderer.sortingOrder = -3;

                // Gatilho, e nao parede: o Scrap atravessa e apanha, em vez de
                // ser empurrado pra dentro do chao.
                var trigger = crawler.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(0.8f, 0.6f);
                trigger.offset = new Vector2(0f, -0.2f);

                var body = crawler.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;

                crawler.AddComponent<Damageable>().Configure(2, 3, shard);
                crawler.AddComponent<CrawlerEnemy>().Configure(patrol.X0 + 0.5f, patrol.X1 + 0.5f, walk);
            }
        }

        private static void BuildLeverAndGate(Transform parent, int groundLayer)
        {
            (int X, int Y, int Height) gateCells = Phase01Layout.Gate;

            // O portao: uma parede de grade, na layer do chao pra barrar o Scrap.
            var gateObject = new GameObject("Gate_Lever") { layer = groundLayer };
            gateObject.transform.SetParent(parent, false);
            gateObject.transform.position = new Vector3(gateCells.X, gateCells.Y, 0f);

            var bars = gateObject.AddComponent<SpriteRenderer>();
            bars.sprite = Phase02Assets.LoadSprite(Phase02Assets.GateBars);
            bars.drawMode = SpriteDrawMode.Tiled;
            bars.size = new Vector2(1f, gateCells.Height);
            bars.sortingLayerName = Phase01Assets.SortingTerrain;
            bars.sortingOrder = 15;

            var wall = gateObject.AddComponent<BoxCollider2D>();
            wall.size = new Vector2(1f, gateCells.Height);
            wall.offset = new Vector2(0.5f, gateCells.Height * 0.5f);

            var gate = gateObject.AddComponent<Gate>();
            gate.Configure(gateCells.Height);

            // A alavanca, e a area em volta onde da pra raspar.
            (int X, int Y) leverTile = Phase01Layout.LeverTile;

            var leverObject = new GameObject("RustyLever");
            leverObject.transform.SetParent(parent, false);
            leverObject.transform.position = new Vector3(leverTile.X + 0.5f, leverTile.Y, 0f);

            var leverSprite = leverObject.AddComponent<SpriteRenderer>();
            leverSprite.sprite = Phase02Assets.LoadSprite(Phase02Assets.LeverRusty);
            leverSprite.sortingLayerName = Phase01Assets.SortingTerrain;
            leverSprite.sortingOrder = 21;

            var area = leverObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(2.6f, 2.5f);
            area.offset = new Vector2(0f, 1.25f);

            leverObject.AddComponent<RustyLever>().Configure(gate, leverSprite,
                Phase02Assets.LoadSprite(Phase02Assets.LeverClean));
        }

        private static void BuildWorkbench(Transform parent)
        {
            (int X, int Y) tile = Phase01Layout.WorkbenchTile;

            var benchObject = new GameObject("Workbench");
            benchObject.transform.SetParent(parent, false);
            benchObject.transform.position = new Vector3(tile.X, tile.Y, 0f);

            var renderer = benchObject.AddComponent<SpriteRenderer>();
            renderer.sprite = Phase02Assets.LoadSprite(Phase02Assets.Workbench);
            renderer.sortingLayerName = Phase01Assets.SortingTerrain;
            renderer.sortingOrder = 21;

            // A bancada tem 3x2 tiles; o gatilho cobre ela e um pouco em volta.
            var area = benchObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(3.6f, 2.5f);
            area.offset = new Vector2(1.5f, 1.25f);

            var rest = new GameObject("RestPoint");
            rest.transform.SetParent(benchObject.transform, false);
            rest.transform.position = new Vector3(tile.X + 1.5f, tile.Y, 0f);

            benchObject.AddComponent<Workbench>().Configure(rest.transform);
        }

        private static void BuildMarkers()
        {
            CreateMarker(null, "SpawnPoint", Phase01Layout.SpawnTile);

            var arena = new GameObject("BossArena");
            (int X, int Y, int Width, int Height) bounds = Phase01Layout.ArenaBounds;
            arena.transform.position = new Vector3(
                bounds.X + bounds.Width * 0.5f,
                bounds.Y + bounds.Height * 0.5f,
                0f);

            CreateMarker(arena.transform, "BossSpawn", Phase01Layout.BossSpawnTile);
            CreateMarker(arena.transform, "Gate_Entry", Phase01Layout.EntryGateTile);
            CreateMarker(arena.transform, "Gate_Exit", Phase01Layout.ExitGateTile);
        }

        /// <summary>Um ponto no meio da celula, na altura do chao dela.</summary>
        private static void CreateMarker(Transform parent, string name, (int X, int Y) tile)
        {
            var marker = new GameObject(name);
            marker.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);
            if (parent != null) marker.transform.SetParent(parent, true);
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            if (scenes.Exists(entry => entry.path == path)) return;

            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
