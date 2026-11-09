using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Prepara os assets que a Phase 1 consome: fatia o tileset em tiles de
    /// 16x16, configura as camadas de parallax, cria os Tile assets e desenha o
    /// placeholder do Scrap.
    ///
    /// Tudo o que e gerado vai pra Assets/Generated, que pode ser apagada a
    /// qualquer momento: rodar o gerador de novo reconstroi.
    /// </summary>
    public static class Phase01Assets
    {
        public const int TileSize = 16;
        public const int PixelsPerUnit = 16;

        public const string TilesetPath =
            "Assets/Cave Platformer Tileset/Enviroment sprite sheets/cave_tileset.png";

        /// <summary>
        /// Parede de caverna, usada no fundo da sala. Grade de 3x7 tiles de
        /// 16x16; as celulas solidas (sem transparencia) sao a linha 6 inteira
        /// e a (2,5) — ver Phase01Layout.BackgroundTileName.
        /// </summary>
        public const string BackgroundTilesetPath =
            "Assets/Cave Platformer Tileset/Enviroment sprite sheets/bg_cave_greenish_16x16.png";

        /// <summary>Caixotes, restos de caixote e cerca. Grade de 6x3.</summary>
        public const string ObjectsPath =
            "Assets/Cave Platformer Tileset/Enviroment sprite sheets/objects_misc.png";

        /// <summary>Pedras e mato seco. Grade de 5x6.</summary>
        public const string BackgroundObjectsPath =
            "Assets/Cave Platformer Tileset/Enviroment sprite sheets/bg_objects.png";

        /// <summary>Pedacos prontos de parede com um buraco no meio, ja com textura.</summary>
        public const string WindowsPath =
            "Assets/Cave Platformer Tileset/Enviroment sprite sheets/bg_cave_greenish_other.png";

        /// <summary>Pedacos maiores de parede com buraco, de cor chapada.</summary>
        public const string GreatWindowsPath =
            "Assets/Cave Platformer Tileset/Enviroment sprite sheets/parallax_cave_holes_greenish.png";

        /// <summary>
        /// Duas faixas de 704x112 com o recorte de uma serra de caverna: a de
        /// cima azulada, a de baixo esverdeada. Viram as silhuetas dos geodos.
        /// </summary>
        public const string SilhouettePath =
            "Assets/Cave Platformer Tileset/Enviroment sprite sheets/parallax_cave_tops.png";

        public const string SilhouetteSpriteName = "silhouette_greenish";

        public const string TilePrefix = "cave";
        public const string BackgroundTilePrefix = "bgcave";
        public const string ObjectsTilePrefix = "misc";
        public const string BackgroundObjectsTilePrefix = "bgobj";

        public const string BackgroundFolder =
            "Assets/Cave Platformer Tileset/Example stage layers";

        public const string GeneratedRoot = "Assets/Generated";
        public const string TilesFolder = GeneratedRoot + "/Tiles";
        public const string SpritesFolder = GeneratedRoot + "/Sprites";

        public const string ScrapSpritePath = SpritesFolder + "/scrap_placeholder.png";
        public const string RockCorePath = SpritesFolder + "/" + Phase01Layout.CoreTileName + ".png";
        public const string FogPuffPath = SpritesFolder + "/fog_puff.png";

        public const string FogMaterialPath = GeneratedRoot + "/OxideFog.mat";
        public const string UnlitMaterialPath = GeneratedRoot + "/SpriteUnlit.mat";

        // ------------------------------------------------------------------
        // Sorting layers
        // ------------------------------------------------------------------

        public const string SortingParallax = "Parallax";
        public const string SortingTerrain = "Terrain";
        public const string SortingActors = "Actors";
        public const string SortingForeground = "Foreground";

        /// <summary>
        /// As camadas da cena, do fundo pra frente. A ordem aqui e a ordem de
        /// renderizacao: quem vem antes fica atras.
        ///
        /// Com isso o "Order in Layer" volta a ser um numero pequeno e local
        /// dentro de cada camada, em vez de um intervalo de -100 a 60 disputado
        /// por todo mundo na Default.
        /// </summary>
        public static readonly string[] SortingLayersInOrder =
        {
            SortingParallax,
            SortingTerrain,
            SortingActors,
            SortingForeground,
        };

        /// <summary>
        /// Nome do sprite de um tile. A linha e contada a partir do TOPO da
        /// imagem, que e como voce le a textura no Sprite Editor.
        ///
        /// A regra mora no Phase01Layout, que compila sem UnityEditor pra dar
        /// pra validar o mapa fora do editor. Aqui so reencaminhamos, pra
        /// existir um nome so de verdade.
        /// </summary>
        public static string TileName(int column, int rowFromTop)
        {
            return Phase01Layout.TileName(column, rowFromTop);
        }

        public static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets", "Generated");
            CreateFolderIfMissing(GeneratedRoot, "Tiles");
            CreateFolderIfMissing(GeneratedRoot, "Sprites");
            CreateFolderIfMissing("Assets", "Scenes");
        }

        private static void CreateFolderIfMissing(string parent, string child)
        {
            string full = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(full))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        /// <summary>
        /// Garante que a layer existe e devolve o indice dela. Se nao houver
        /// slot livre, cai na Default, que ainda funciona pro ground check.
        /// </summary>
        public static int EnsureLayer(string layerName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return 0;

            var tagManager = new SerializedObject(assets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null) return 0;

            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName) return i;
            }

            // Os indices 0-7 sao reservados pela Unity.
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(slot.stringValue))
                {
                    slot.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                    return i;
                }
            }

            Debug.LogWarning($"[Phase 1] Sem slot livre pra layer '{layerName}'. Usando Default.");
            return 0;
        }

        /// <summary>
        /// Garante que as sorting layers da cena existem, na ordem certa.
        ///
        /// Elas sao inseridas logo depois da Default, e nao no fim da lista, pra
        /// ficarem agrupadas e na ordem entre si. As layers que vieram com o
        /// pack de dungeon (Bg, Bg2, Scene2, Scene, Floor) ficam intactas — nao
        /// mexo nelas porque prefabs do pack podem depender.
        /// </summary>
        public static void EnsureSortingLayers()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning("[Phase 1] Nao consegui abrir o TagManager pra criar as sorting layers.");
                return;
            }

            var tagManager = new SerializedObject(assets[0]);
            SerializedProperty layers = tagManager.FindProperty("m_SortingLayers");
            if (layers == null)
            {
                Debug.LogWarning("[Phase 1] TagManager sem m_SortingLayers.");
                return;
            }

            // Index 0 e a Default; entramos logo atras dela.
            int insertAt = 1;
            bool changed = false;

            foreach (string name in SortingLayersInOrder)
            {
                if (SortingLayerIndex(layers, name) >= 0) continue;

                layers.InsertArrayElementAtIndex(insertAt);
                SerializedProperty element = layers.GetArrayElementAtIndex(insertAt);
                element.FindPropertyRelative("name").stringValue = name;
                element.FindPropertyRelative("uniqueID").intValue = UniqueSortingId(layers, name);
                element.FindPropertyRelative("locked").boolValue = false;

                insertAt++;
                changed = true;
            }

            if (!changed) return;

            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Phase 1] Sorting layers criadas: {string.Join(", ", SortingLayersInOrder)}.");
        }

        /// <summary>
        /// Confere que as sorting layers existem mesmo.
        ///
        /// Vale a checagem porque atribuir sortingLayerName a um nome
        /// inexistente nao da erro: a Unity cai calada de volta pra Default, e
        /// voce so descobre olhando a cena e achando tudo empilhado errado.
        /// </summary>
        public static bool VerifySortingLayers()
        {
            var missing = new List<string>();

            foreach (string name in SortingLayersInOrder)
            {
                bool found = false;

                foreach (SortingLayer layer in SortingLayer.layers)
                {
                    if (layer.name != name) continue;

                    found = true;
                    break;
                }

                if (!found) missing.Add(name);
            }

            if (missing.Count == 0) return true;

            Debug.LogError($"[Phase 1] Estas sorting layers nao existem: {string.Join(", ", missing)}. " +
                           "Crie na mao em Project Settings > Tags and Layers > Sorting Layers, " +
                           "na ordem " + string.Join(" / ", SortingLayersInOrder) +
                           ", senao tudo cai na Default e a ordem de desenho sai errada.");
            return false;
        }

        private static int SortingLayerIndex(SerializedProperty layers, string name)
        {
            for (int i = 0; i < layers.arraySize; i++)
            {
                SerializedProperty element = layers.GetArrayElementAtIndex(i);
                SerializedProperty nameProperty = element.FindPropertyRelative("name");

                if (nameProperty != null && nameProperty.stringValue == name) return i;
            }

            return -1;
        }

        /// <summary>
        /// Id derivado do nome, pra regerar dar sempre o mesmo. Se bater com um
        /// id que ja existe, anda ate achar um livre.
        /// </summary>
        private static int UniqueSortingId(SerializedProperty layers, string name)
        {
            int candidate = 0;
            unchecked
            {
                foreach (char c in name) candidate = candidate * 31 + c;
            }

            candidate &= 0x7FFFFFFF;
            if (candidate == 0) candidate = 1;

            while (SortingIdTaken(layers, candidate))
            {
                candidate = (candidate + 1) & 0x7FFFFFFF;
                if (candidate == 0) candidate = 1;
            }

            return candidate;
        }

        private static bool SortingIdTaken(SerializedProperty layers, int id)
        {
            for (int i = 0; i < layers.arraySize; i++)
            {
                SerializedProperty element = layers.GetArrayElementAtIndex(i);
                SerializedProperty idProperty = element.FindPropertyRelative("uniqueID");

                if (idProperty != null && idProperty.intValue == id) return true;
            }

            return false;
        }

        /// <summary>
        /// Fatia cave_tileset.png numa grade de 16x16 com nomes previsiveis.
        /// Refatiar com os mesmos nomes mantem as referencias dos Tile assets.
        /// </summary>
        public static bool SliceTileset()
        {
            return SliceGrid(TilesetPath, TilePrefix);
        }

        /// <summary>Fatia a textura de parede de caverna usada no fundo da sala.</summary>
        public static bool SliceBackgroundTileset()
        {
            return SliceGrid(BackgroundTilesetPath, BackgroundTilePrefix);
        }

        /// <summary>Fatia as duas folhas de objetos: sucata, pedras e mato seco.</summary>
        public static bool SliceObjects()
        {
            return SliceGrid(ObjectsPath, ObjectsTilePrefix)
                && SliceGrid(BackgroundObjectsPath, BackgroundObjectsTilePrefix);
        }

        /// <summary>
        /// Fatia uma textura numa grade de 16x16 com nomes previsiveis
        /// (prefixo_cX_rY). Refatiar com os mesmos nomes mantem as referencias
        /// dos Tile assets que ja existem.
        /// </summary>
        internal static bool SliceGrid(string path, string prefix)
        {
            Texture2D texture = PrepareSheet(path);
            if (texture == null) return false;

            int columns = texture.width / TileSize;
            int rows = texture.height / TileSize;

            var slices = new List<(string Name, RectInt Area)>();

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    slices.Add((
                        GridTileName(prefix, column, row),
                        new RectInt(column * TileSize, row * TileSize, TileSize, TileSize)));
                }
            }

            ApplySlices(path, texture.height, slices, SpriteAlignment.Center);

            Debug.Log($"[Phase 1] {prefix}: fatiado {columns}x{rows} = {slices.Count} tiles.");
            return true;
        }

        /// <summary>
        /// Recorta os pedacos de parede com buraco e a faixa de silhueta. Eles
        /// nao cabem numa grade, entao cada um tem o seu retangulo, medido a
        /// partir do canto de cima, a esquerda da textura.
        /// </summary>
        public static bool SliceBackdropPieces()
        {
            return SliceRects(WindowsPath, new[]
                {
                    (Phase01Layout.WindowTall, new RectInt(0, 0, 64, 80)),
                    (Phase01Layout.WindowWide, new RectInt(80, 16, 80, 64)),
                    (Phase01Layout.WindowTunnel, new RectInt(0, 96, 80, 64)),
                })
                && SliceRects(GreatWindowsPath, new[]
                {
                    (Phase01Layout.WindowGreat, new RectInt(112, 176, 192, 96)),
                })
                && SliceRects(SilhouettePath, new[]
                {
                    (SilhouetteSpriteName, new RectInt(0, 112, 704, 112)),
                });
        }

        /// <summary>
        /// Fatia uma textura em retangulos avulsos, com o pivo no canto de
        /// baixo, a esquerda — assim a posicao do objeto e a celula do canto.
        /// </summary>
        private static bool SliceRects(string path, (string Name, RectInt Area)[] slices)
        {
            Texture2D texture = PrepareSheet(path);
            if (texture == null) return false;

            ApplySlices(path, texture.height, slices, SpriteAlignment.BottomLeft);
            return true;
        }

        /// <summary>Configura a textura como folha de sprites de pixel art e a devolve.</summary>
        private static Texture2D PrepareSheet(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[Phase 1] Nao achei a textura em {path}.");
                return null;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                Debug.LogError($"[Phase 1] Nao consegui carregar a textura {path}.");
            }

            return texture;
        }

        private static void ApplySlices(string path, int textureHeight,
            IEnumerable<(string Name, RectInt Area)> slices, SpriteAlignment alignment)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);

            var factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            var rects = new List<SpriteRect>();
            var pairs = new List<SpriteNameFileIdPair>();

            foreach ((string name, RectInt area) in slices)
            {
                var spriteRect = new SpriteRect
                {
                    name = name,
                    spriteID = GUID.Generate(),
                    // O Rect da textura tem origem embaixo; a area e medida de cima.
                    rect = new Rect(
                        area.x,
                        textureHeight - area.y - area.height,
                        area.width,
                        area.height),
                    alignment = alignment,
                    pivot = alignment == SpriteAlignment.BottomLeft
                        ? Vector2.zero
                        : new Vector2(0.5f, 0.5f),
                };

                rects.Add(spriteRect);
                pairs.Add(new SpriteNameFileIdPair(spriteRect.name, spriteRect.spriteID));
            }

            provider.SetSpriteRects(rects.ToArray());

            var nameIdProvider = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameIdProvider?.SetNameFileIdPairs(pairs);

            provider.Apply();
            importer.SaveAndReimport();
        }

        /// <summary>Um sprite de uma folha fatiada, pelo nome.</summary>
        public static Sprite LoadSprite(string path, string name)
        {
            Sprite sprite = AssetDatabase
                .LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .FirstOrDefault(candidate => candidate.name == name);

            if (sprite == null)
            {
                Debug.LogWarning($"[Phase 1] Sprite '{name}' nao existe em {path}.");
            }

            return sprite;
        }

        public static string GridTileName(string prefix, int column, int rowFromTop)
        {
            return $"{prefix}_c{column}_r{rowFromTop}";
        }

        /// <summary>
        /// Deixa uma camada de parallax pronta pro SpriteRenderer em modo Tiled:
        /// Full Rect no mesh e Repeat no wrap, senao a repeticao aparece com
        /// costura ou simplesmente nao funciona.
        /// </summary>
        public static Sprite ConfigureBackgroundSprite(string fileName)
        {
            string path = $"{BackgroundFolder}/{fileName}";

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[Phase 1] Camada de fundo nao encontrada: {path}.");
                return null;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Repeat;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>
        /// Cria um Tile asset por sprite do tileset. Todos com collider Grid:
        /// isso so tem efeito no tilemap que carrega o TilemapCollider2D, entao
        /// as camadas decorativas continuam atravessaveis.
        /// </summary>
        public static Dictionary<string, TileBase> BuildTiles()
        {
            var tiles = new Dictionary<string, TileBase>();

            AddTilesFrom(TilesetPath, tiles);
            AddTilesFrom(BackgroundTilesetPath, tiles);
            AddTilesFrom(ObjectsPath, tiles);
            AddTilesFrom(BackgroundObjectsPath, tiles);
            AddTilesFrom(CreateRockCore(), tiles);

            AssetDatabase.SaveAssets();
            Debug.Log($"[Phase 1] {tiles.Count} Tile assets prontos em {TilesFolder}.");
            return tiles;
        }

        internal static void AddTilesFrom(string texturePath, Dictionary<string, TileBase> tiles)
        {
            var sprites = AssetDatabase
                .LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .ToArray();

            foreach (Sprite sprite in sprites)
            {
                string assetPath = $"{TilesFolder}/{sprite.name}.asset";
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);

                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;
                    tile.colliderType = Tile.ColliderType.Grid;
                    AssetDatabase.CreateAsset(tile, assetPath);
                }
                else
                {
                    tile.sprite = sprite;
                    tile.colliderType = Tile.ColliderType.Grid;
                    EditorUtility.SetDirty(tile);
                }

                tiles[sprite.name] = tile;
            }
        }

        /// <summary>
        /// O miolo da rocha: um tile todo da cor escura pra onde as bordas do
        /// tileset esmaecem. Devolve o caminho, pra virar Tile junto com os outros.
        /// </summary>
        private static string CreateRockCore()
        {
            if (System.IO.File.Exists(RockCorePath)) return RockCorePath;

            var core = new Color32(27, 21, 35, 255);
            var pixels = new Color32[TileSize * TileSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = core;

            WriteTexture(RockCorePath, TileSize, TileSize, pixels);

            var importer = AssetImporter.GetAtPath(RockCorePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = PixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return RockCorePath;
        }

        /// <summary>
        /// Um borrao redondo e macio. Os packs nao tem nevoa, entao ela e feita
        /// de varios destes, grandes e quase transparentes, se arrastando.
        /// </summary>
        private static Texture2D CreateFogPuff()
        {
            if (!System.IO.File.Exists(FogPuffPath))
            {
                const int size = 64;
                var pixels = new Color32[size * size];
                float center = (size - 1) * 0.5f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float distance = new Vector2(x - center, y - center).magnitude / center;
                        float alpha = Mathf.Clamp01(1f - distance);

                        // Ao quadrado: a borda some devagar, sem aro visivel.
                        pixels[y * size + x] = new Color(1f, 1f, 1f, alpha * alpha);
                    }
                }

                WriteTexture(FogPuffPath, size, size, pixels);

                var importer = AssetImporter.GetAtPath(FogPuffPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.alphaIsTransparency = true;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.SaveAndReimport();
                }
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(FogPuffPath);
        }

        private static void WriteTexture(string path, int width, int height, Color32[] pixels)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();

            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// Material da nevoa e da poeira. Sem luz de proposito: o Oxide brilha
        /// sozinho, e o que faz ele aparecer numa sala escura.
        /// </summary>
        public static Material GetOrCreateFogMaterial()
        {
            Material material = GetOrCreateUnlitMaterial(FogMaterialPath);
            if (material == null) return null;

            material.mainTexture = CreateFogPuff();
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        /// <summary>
        /// Material sem luz pro que fica la no fundo: as silhuetas tem que ter
        /// exatamente a cor escolhida, nao a que as luzes da sala derem.
        /// </summary>
        public static Material GetOrCreateUnlitMaterial()
        {
            return GetOrCreateUnlitMaterial(UnlitMaterialPath);
        }

        private static Material GetOrCreateUnlitMaterial(string path)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Debug.LogWarning("[Phase 1] Nao achei um shader de sprite sem luz. " +
                                 "A nevoa e as silhuetas vao usar o material padrao.");
                return null;
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssets();
            return material;
        }

        /// <summary>
        /// Desenha o placeholder do Scrap: 16x20, corpo gordinho de ferro
        /// oxidado e dois olhos grandes. Nao e a arte final, so uma silhueta
        /// legivel pra voce sentir o tamanho e o peso do movimento.
        /// Trocar depois e arrastar o sprite novo no SpriteRenderer.
        /// </summary>
        public static Sprite CreateScrapPlaceholder()
        {
            const int width = 16;
            const int height = 20;

            var rust = new Color32(0x8C, 0x5A, 0x3C, 0xFF);
            var rustDark = new Color32(0x5E, 0x39, 0x26, 0xFF);
            var rustLight = new Color32(0xB5, 0x7B, 0x54, 0xFF);
            var eye = new Color32(0xF2, 0xEC, 0xDC, 0xFF);
            var pupil = new Color32(0x20, 0x18, 0x14, 0xFF);
            var clear = new Color32(0, 0, 0, 0);

            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            // Corpo: um oval mais largo embaixo do que em cima, pra ler como gordinho.
            const float centerX = 7.5f;
            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                float halfWidth = Mathf.Lerp(6.8f, 5.2f, t * t);
                if (y < 2) halfWidth = 6.2f;

                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Abs(x - centerX);
                    if (dx > halfWidth) continue;

                    bool edge = dx > halfWidth - 1.2f || y == 0 || y == height - 1;
                    bool highlight = !edge && y > height - 7 && dx < 2.5f;

                    pixels[y * width + x] = edge ? rustDark : (highlight ? rustLight : rust);
                }
            }

            // Olhos grandes: 3x3 cada, com pupila. O que faz parecer curioso.
            DrawEye(pixels, width, 4, 11, eye, pupil);
            DrawEye(pixels, width, 9, 11, eye, pupil);

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
            };
            texture.SetPixels32(pixels);
            texture.Apply();

            System.IO.File.WriteAllBytes(ScrapSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(ScrapSpritePath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(ScrapSpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = PixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;

                // Pivo nos pes: facilita posicionar em cima de um tile.
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0f);
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(ScrapSpritePath);
        }

        private static void DrawEye(Color32[] pixels, int width, int originX, int originY,
            Color32 eye, Color32 pupil)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    int index = (originY + y) * width + originX + x;
                    if (index < 0 || index >= pixels.Length) continue;

                    pixels[index] = (x == 1 && y == 1) ? pupil : eye;
                }
            }
        }
    }
}
