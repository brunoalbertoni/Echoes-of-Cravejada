using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Monta a cena da Phase 5 — o palacio do Rei de Prata — do zero, a partir
    /// de Phase05Layout: o Corredor Real (sem inimigos, so o luxo), a bancada
    /// e Crisol no fim dele, a Sala do Trono com o Rei (as duas fases e a
    /// cutscene da transformacao), o Altar do Coracao e o final
    /// desenhado (O Grande Polimento).
    ///
    /// Menu:      Echoes > Gerar Phase 5 (Silver Palace)
    ///            Echoes > Validar layout da Phase 5
    /// Batchmode: -executeMethod EchoesOfCravejada.EditorTools.Phase05Generator.GenerateFromCommandLine
    /// </summary>
    public static class Phase05Generator
    {
        public const string ScenePath = "Assets/Phases/" + Phase05Layout.SceneName + "/Scene/" + Phase05Layout.SceneName + ".unity";
        private const string GroundLayerName = "Ground";
        private const string MarblePath = Phase02Assets.BrassFolder + "/marble_tiles.png";
        private const string MarblePrefix = "marble";

        private const string KingRegalSheet = "king_regal_sheet";
        private const string KingCorruptSheet = "king_corrupt_sheet";
        private const string KingWreckSheet = "d5_king_wreck_sheet";
        private const string StairDoor = "palace_stair_door";
        private const string Facet = "king_facet";
        private const string FacetCorrupt = "king_facet_corrupt";
        private const string Puff = "king_puff";
        private const string Glint = "king_glint";

        private const string CartoonKing = "king";
        private const string CartoonPolish = "polish";
        private const string CartoonCrown = "crown_end";

        private static readonly Color Night = new Color(0.07f, 0.07f, 0.11f);
        private static readonly Color LampGlow = new Color(1f, 0.92f, 0.78f);
        private static readonly Color OxideGreen = new Color(0.36f, 0.95f, 0.60f);

        private static int _groundLayer;
        private static Material _unlit;

        [MenuItem("Echoes/Gerar Phase 5 (Silver Palace)", false, 60)]
        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        [MenuItem("Echoes/Validar layout da Phase 5", false, 61)]
        public static void ValidateLayout()
        {
            List<string> problems = Phase05Layout.Validate();
            if (problems.Count == 0) Debug.Log("[Phase 5] Layout OK: da chegada ao Rei, e do Rei ao Altar.");
            foreach (string problem in problems) Debug.LogWarning($"[Phase 5] {problem}");
        }

        public static void GenerateFromCommandLine() => Build();

        private static void Build()
        {
            Phase01Assets.EnsureFolders();
            _groundLayer = Phase01Assets.EnsureLayer(GroundLayerName);
            Phase01Assets.EnsureSortingLayers();
            Phase01Assets.VerifySortingLayers();

            foreach (string problem in Phase05Layout.Validate()) Debug.LogWarning($"[Phase 5] {problem}");

            if (!Phase01Assets.SliceTileset() || !Phase01Assets.SliceBackgroundTileset() || !Phase01Assets.SliceObjects())
            {
                Debug.LogError("[Phase 5] Abortado: nao consegui fatiar as folhas do pack de caverna.");
                return;
            }

            if (!Phase02Assets.EnsureArt(new[] { 4 }) || !Phase02Assets.EnsureMechanicsArt())
            {
                Debug.LogError("[Phase 5] Abortado: nao consegui preparar a arte de latao.");
                return;
            }

            if (!EnsureArt())
            {
                Debug.LogError("[Phase 5] Abortado: nao consegui fatiar o tileset de marmore.");
                return;
            }

            Dictionary<string, TileBase> tiles = Phase02Assets.BuildTiles();
            Phase01Assets.AddTilesFrom(MarblePath, tiles);

            Sprite scrapSprite = Phase01Assets.CreateScrapPlaceholder();
            Sprite polishedSprite = Phase02Assets.LoadSprite(Phase02Assets.ScrapPolished);
            _unlit = Phase01Assets.GetOrCreateUnlitMaterial();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildTilemaps(tiles);
            Light2D global = BuildLighting(out List<Light2D> oxideLights);
            BuildProps();
            BuildMarkers();

            var travel = new GameObject("Travel");
            SceneKit.CreateSpawnPoint(travel.transform, Phase05Layout.FromDistrict4SpawnId, Phase05Layout.SpawnTile);

            Transform scrap = Phase01Generator.BuildScrap(scrapSprite, polishedSprite, _groundLayer, Phase05Layout.SpawnTile);
            Phase01Generator.BuildCamera(scrap, Phase05Layout.Width, Phase05Layout.Height, Night);
            scrap.GetComponent<ScrapDash>().SetUnlockedFromStart(true);
            scrap.GetComponent<ScrapAppearance>().SetPolishedFromStart(true);
            scrap.gameObject.AddComponent<AbilityGrant>().Configure(true, true, true);
            scrap.gameObject.AddComponent<PurificationGrant>();

            var root = new GameObject("Mechanics").transform;
            BuildWorkbenches(Child(root, "Workbenches"));
            BuildCrisol(root);
            BuildLore(Child(root, "Lore"));

            (int sx, int sy, int sh) = Phase05Layout.ArenaSeal;
            Phase01Generator.CreateArenaSeal(root, _groundLayer, new RectInt(sx, sy, 1, sh));

            BuildKing(Child(root, "ThroneRoom"), global, oxideLights);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneKit.AddSceneToBuildSettings(ScenePath);

            Debug.Log($"[Phase 5] Cena gerada: {ScenePath}. {Phase05Layout.Rooms.Count} salas, sem inimigos: o corredor, a bancada, o Rei de Prata e o final.");
        }

        private static Transform Child(Transform parent, string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static SpriteRenderer Sprite(GameObject target, string spriteName, string layer, int order)
        {
            var renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = Phase02Assets.LoadSprite(spriteName);
            renderer.sortingLayerName = layer;
            renderer.sortingOrder = order;
            return renderer;
        }

        // ------------------------------------------------------------------
        // Arte
        // ------------------------------------------------------------------

        private static bool EnsureArt()
        {
            var feet = new Vector2(0.5f, 0f);
            var top = new Vector2(0.5f, 1f);
            var middle = new Vector2(0.5f, 0.5f);

            Phase02Assets.WriteCanvas(MarblePath, MarbleArt.Tiles());

            foreach ((string name, System.Func<PixelCanvas> draw, bool hanging, bool tiled) in PalaceArt.Sprites())
            {
                // O trono a 24 px por tile: o Rei sentado nele fica do tamanho certo (ver KingArt.SeatHeight).
                int ppu = name.StartsWith("palace_throne") ? KingArt.ThronePixelsPerUnit
                    : name == "palace_crown" ? LootArt.PixelsPerUnit : Phase01Assets.PixelsPerUnit;
                Phase02Assets.WriteSprite(name, draw(), tiled ? Vector2.zero : (hanging ? top : feet), tiled, ppu);
            }

            // O luxo do corredor: colunas, janeloes, quadros, lustres, lambri e friso.
            foreach ((string name, System.Func<PixelCanvas> draw, bool hanging, bool tiled) in PalaceDecorArt.Sprites())
            {
                Phase02Assets.WriteSprite(name, draw(), tiled ? Vector2.zero : (hanging ? top : feet), tiled);
            }

            // A porta da Grande Escadaria, por onde o Scrap chega (a mesma do alto da escada, nos Ateliers).
            Phase02Assets.WriteSprite(StairDoor, StairArt.Doorway(), feet);

            Phase02Assets.WriteSprite(Facet, KingArt.Facet(false), middle);
            Phase02Assets.WriteSprite(FacetCorrupt, KingArt.Facet(true), middle);
            Phase02Assets.WriteSprite(Puff, KingArt.Puff(0), middle);
            Phase02Assets.WriteSprite(Glint, KingArt.Glint(), middle);

            // O Coracao de Quartzo, visto pelo janelao da cratera atras do trono.
            Phase02Assets.WriteSprite(HeartWindowClean, QuartzHeartArt.Window(false), feet);
            Phase02Assets.WriteSprite(HeartWindowCorrupt, QuartzHeartArt.Window(true), feet);
            Phase02Assets.WriteSprite(HeartVeins, QuartzHeartArt.Veins(), feet);

            // O Rei, no traco dos outros chefes: duas folhas com os mesmos quadros, 32 px por tile.
            var kingPivot = new Vector2(KingArt.PivotX, KingArt.PivotY);
            Phase02Assets.WriteSheet(KingRegalSheet, KingArt.Frames(false), kingPivot, KingArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(KingCorruptSheet, KingArt.Frames(true), kingPivot, KingArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(KingWreckSheet, KingWreckArt.Frames(), new Vector2(KingWreckArt.PivotX, KingWreckArt.PivotY), KingWreckArt.PixelsPerUnit);


            // As cutscenes desenhadas: a transformacao e o final.
            CartoonAssets.EnsureSet(CartoonKing, FinaleCartoonArt.Transformation());
            CartoonAssets.EnsureSet(CartoonPolish, FinaleCartoonArt.EndingA());
            CartoonAssets.EnsureSet(CartoonCrown, FinaleCartoonArt.EndingB());

            return Phase01Assets.SliceGrid(MarblePath, MarblePrefix);
        }

        // ------------------------------------------------------------------
        // Terreno, luz e cenario
        // ------------------------------------------------------------------

        private static void BuildTilemaps(Dictionary<string, TileBase> tiles)
        {
            var gridObject = new GameObject("Grid");
            gridObject.AddComponent<Grid>().cellSize = Vector3.one;
            Transform root = gridObject.transform;

            Tilemap back = Phase01Generator.CreateTilemap(root, "TM_Background", Phase01Assets.SortingTerrain, 0);
            back.color = new Color(0.58f, 0.60f, 0.72f);
            Tilemap ground = Phase01Generator.CreateTilemap(root, "TM_Ground", Phase01Assets.SortingTerrain, 10);
            ground.color = new Color(0.96f, 0.97f, 1f);

            GameObject groundObject = ground.gameObject;
            groundObject.layer = _groundLayer;
            groundObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            var tilemapCollider = groundObject.AddComponent<TilemapCollider2D>();
            var composite = groundObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            groundObject.AddComponent<GroundColliderRefresh>();

            int width = Phase05Layout.Width;
            int height = Phase05Layout.Height;
            var backBuffer = new TileBase[width * height];
            var groundBuffer = new TileBase[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    backBuffer[x + y * width] = Phase01Generator.Resolve(tiles, Phase05Layout.BackgroundTileName(x, y));
                    groundBuffer[x + y * width] = Phase01Generator.Resolve(tiles, Phase05Layout.GroundTileName(x, y));
                }
            }

            var bounds = new BoundsInt(0, 0, 0, width, height, 1);
            back.SetTilesBlock(bounds, backBuffer);
            ground.SetTilesBlock(bounds, groundBuffer);
            Phase01Generator.FinishTilemap(back);
            Phase01Generator.FinishTilemap(ground);
            composite.GenerateGeometry();
        }

        private static Light2D BuildLighting(out List<Light2D> oxide)
        {
            var root = new GameObject("Lighting");
            var globalObject = new GameObject("GlobalLight2D");
            globalObject.transform.SetParent(root.transform, false);

            var global = globalObject.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.color = new Color(0.86f, 0.86f, 0.98f);
            global.intensity = 0.55f;

            oxide = new List<Light2D>();
            int lamps = 0;
            foreach ((float x, float y, float radius, bool green) in Phase05Layout.Lamps)
            {
                if (green)
                {
                    Light2D glow = Phase01Generator.CreatePointLight(root.transform, $"OxideGlow_{oxide.Count + 1}", (x, y), OxideGreen, 0.8f, radius);
                    glow.gameObject.AddComponent<GlowPulse>().Configure(glow, null, 2.4f, oxide.Count * 0.2f);
                    oxide.Add(glow);
                }
                else
                {
                    Phase01Generator.CreatePointLight(root.transform, $"Lamp_{++lamps}", (x, y), LampGlow, 0.8f, radius);
                }
            }

            return global;
        }

        /// <summary>O palacio por cima do terreno. O trono e os espelhos da sala sao do Rei; a Coluna, do Altar.</summary>
        private static void BuildProps()
        {
            var root = new GameObject("PalaceDressing");

            foreach (Phase05Layout.Strip strip in Phase05Layout.Strips)
            {
                var stripObject = new GameObject($"{strip.Sprite}_{strip.X0}_{strip.Y:0.#}");
                stripObject.transform.SetParent(root.transform, false);
                stripObject.transform.position = new Vector3(strip.X0, strip.Y, 0f);
                SpriteRenderer renderer = Sprite(stripObject, strip.Sprite, Phase01Assets.SortingTerrain, strip.Order);
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = new Vector2(strip.X1 - strip.X0 + 1, strip.Height);
            }

            foreach (Phase05Layout.Prop prop in Phase05Layout.Props)
            {
                if (prop.Sprite == "palace_conduit") continue;

                var propObject = new GameObject($"{prop.Sprite}_{prop.X:0.#}_{prop.Y:0.#}");
                propObject.transform.SetParent(root.transform, false);
                propObject.transform.position = new Vector3(prop.X, prop.Y, 0f);
                Sprite(propObject, prop.Sprite, Phase01Assets.SortingTerrain, prop.Order);

                if (prop.Hanging) propObject.AddComponent<Sway>().Configure(1.5f, 4.5f, (prop.X * 0.37f) % 1f);
            }

            foreach ((int x0, int x1, int y) in Phase05Layout.Carpets)
            {
                var carpet = new GameObject($"Carpet_{x0}_{y}");
                carpet.transform.SetParent(root.transform, false);
                carpet.transform.position = new Vector3(x0, y - 0.05f, 0f);
                SpriteRenderer renderer = Sprite(carpet, "palace_carpet", Phase01Assets.SortingTerrain, 22);
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = new Vector2(x1 - x0 + 1, 5f / 16f);
            }
        }

        private static void BuildMarkers()
        {
            var root = new GameObject("Markers");
            var groups = new Dictionary<string, Transform>();
            foreach (Phase05Layout.Marker marker in Phase05Layout.Markers)
            {
                if (!groups.TryGetValue(marker.Group, out Transform group))
                {
                    group = Child(root.transform, marker.Group);
                    groups[marker.Group] = group;
                }

                var markerObject = new GameObject(marker.Name);
                markerObject.transform.SetParent(group, false);
                markerObject.transform.position = new Vector3(marker.X + 0.5f, marker.Y, 0f);
            }
        }

        // ------------------------------------------------------------------
        // A bancada, Crisol e as leituras
        // ------------------------------------------------------------------

        private static void BuildWorkbenches(Transform parent)
        {
            foreach ((string name, int x, int y) in Phase05Layout.Workbenches)
            {
                var benchObject = new GameObject(name);
                benchObject.transform.SetParent(parent, false);
                benchObject.transform.position = new Vector3(x, y, 0f);
                Sprite(benchObject, Phase02Assets.Workbench, Phase01Assets.SortingTerrain, 21);

                var area = benchObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(3.6f, 2.5f);
                area.offset = new Vector2(1.5f, 1.25f);

                var rest = new GameObject("RestPoint");
                rest.transform.SetParent(benchObject.transform, false);
                rest.transform.position = new Vector3(x + 1.5f, y, 0f);
                benchObject.AddComponent<Workbench>().Configure(rest.transform);
            }
        }

        /// <summary>
        /// O que se le com E no palacio: os retratos da nobreza no corredor.
        /// A arte (no mapa e na leitura) vem do LoreArt.Phase05.
        /// </summary>
        private static void BuildLore(Transform parent)
        {
            LoreArtAssets.Write(LoreArt.Phase05.Values);

            foreach (Phase05Layout.LoreSpot spot in Phase05Layout.Lore)
            {
                var loreObject = new GameObject(spot.Name);
                loreObject.transform.SetParent(parent, false);
                loreObject.transform.position = new Vector3(spot.X, spot.Y, 0f);

                var area = loreObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.6f, 2.5f);
                area.offset = new Vector2(0f, 1.25f);

                var artObject = new GameObject("Art");
                artObject.transform.SetParent(loreObject.transform, false);
                artObject.AddComponent<SpriteRenderer>();

                var readable = loreObject.AddComponent<Npc>();
                readable.ConfigureReadable(spot.Title, spot.Lines);

                if (LoreArt.Phase05.TryGetValue(spot.Name, out LoreArt.Piece piece))
                {
                    LoreArtAssets.Apply(piece, artObject, readable, Phase01Assets.SortingTerrain);
                }
            }
        }

        private static void BuildCrisol(Transform parent)
        {
            (int X, int Y) tile = Phase05Layout.CrisolTile;
            var crisolObject = new GameObject("Npc_Crisol");
            crisolObject.transform.SetParent(parent, false);
            crisolObject.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);
            Phase02Assets.AddCrisol(crisolObject, -4);

            var area = crisolObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(3.2f, 2.6f);
            area.offset = new Vector2(0f, 1.3f);

            var crisol = crisolObject.AddComponent<Crisol>();
            crisol.Configure("Crisol", Phase05Layout.CrisolLines);
            crisol.ConfigureGivesRuby(false);
        }

        private static Gate BuildGate(Transform parent, string name, (int X, int Y, int Height) cells)
        {
            var gateObject = new GameObject(name) { layer = _groundLayer };
            gateObject.transform.SetParent(parent, false);
            gateObject.transform.position = new Vector3(cells.X, cells.Y, 0f);

            SpriteRenderer bars = Sprite(gateObject, Phase02Assets.GateBars, Phase01Assets.SortingTerrain, 15);
            bars.drawMode = SpriteDrawMode.Tiled;
            bars.size = new Vector2(1f, cells.Height);
            bars.color = new Color(0.92f, 0.94f, 1f);

            var wall = gateObject.AddComponent<BoxCollider2D>();
            wall.size = new Vector2(1f, cells.Height);
            wall.offset = new Vector2(0.5f, cells.Height * 0.5f);

            var gate = gateObject.AddComponent<Gate>();
            gate.Configure(cells.Height + 1f, Vector2.up);
            return gate;
        }

        // ------------------------------------------------------------------
        // O Rei de Prata
        // ------------------------------------------------------------------

        private const string HeartWindowClean = "palace_heart_window_0";
        private const string HeartWindowCorrupt = "palace_heart_window_1";
        private const string HeartVeins = "palace_heart_veins";

        /// <summary>
        /// O janelao atras do trono: a cratera, com o Coracao de Quartzo la no
        /// fundo, e os veios que sobem dele ate o trono. So cenario; quem o faz
        /// pulsar e esverdear junto com a luta e o QuartzHeartView.
        /// </summary>
        private static void BuildHeartWindow(Transform parent, SilverKing king, float x, float y)
        {
            var root = new GameObject("QuartzHeartWindow");
            root.transform.SetParent(parent, false);

            GameObject clean = Prop(root.transform, HeartWindowClean, x, y, 1);
            GameObject corrupt = Prop(root.transform, HeartWindowCorrupt, x, y, 1);
            GameObject veins = Prop(root.transform, HeartVeins, x, y, 2);

            // As duas por cima uma da outra: o verde entra por transparencia.
            corrupt.GetComponent<SpriteRenderer>().sortingOrder = 1;
            corrupt.transform.position += new Vector3(0f, 0f, -0.01f);
            if (_unlit != null) veins.GetComponent<SpriteRenderer>().sharedMaterial = _unlit;

            // O Coracao fica a uns 9,8 tiles acima do pe do janelao (164 px de 320, de cima pra baixo).
            float heartY = y + (QuartzHeartArt.Size - 164f) / Phase01Assets.PixelsPerUnit;
            Light2D glow = Phase01Generator.CreatePointLight(root.transform, "HeartLight", (x, heartY), new Color(0.78f, 0.90f, 1f), 0.5f, 7f);

            root.AddComponent<QuartzHeartView>().Configure(clean.GetComponent<SpriteRenderer>(), corrupt.GetComponent<SpriteRenderer>(),
                veins.GetComponent<SpriteRenderer>(), glow, king);
        }

        private static GameObject Prop(Transform parent, string sprite, float x, float y, int order, string layer = null)
        {
            var propObject = new GameObject(sprite);
            propObject.transform.SetParent(parent, false);
            propObject.transform.position = new Vector3(x, y, 0f);
            Sprite(propObject, sprite, layer ?? Phase01Assets.SortingTerrain, order);
            return propObject;
        }

        private static KingBeam Beam(Transform parent, string name)
        {
            var beamObject = new GameObject(name);
            beamObject.transform.SetParent(parent, false);

            var line = beamObject.AddComponent<LineRenderer>();
            line.sharedMaterial = _unlit;
            line.numCapVertices = 3;
            line.useWorldSpace = true;
            line.sortingLayerName = Phase01Assets.SortingActors;
            line.sortingOrder = 10;
            line.positionCount = 0;

            return beamObject.AddComponent<KingBeam>();
        }

        /// <summary>
        /// A Sala do Trono: o trono no fundo, os espelhos (inteiros na fase 1,
        /// quebrados na 2), os cacos que viram escudo contra o sopro, e o Rei,
        /// de pe diante do trono, com as duas folhas, os raios, o sopro, a
        /// cutscene da transformacao e a coroa que cai.
        /// </summary>
        private static void BuildKing(Transform parent, Light2D global, List<Light2D> oxideLights)
        {
            (float kx, float ky) = Phase05Layout.KingTile;
            (float tx, float ty) = Phase05Layout.ThroneTile;
            (int ax0, int _, int ax1, int _) = Phase05Layout.ThroneRoom;
            (float minX, float maxX) = Phase05Layout.KingRange;
            (int wx, int wy, int ww, int wh) = Phase05Layout.KingWake;

            // O que muda na sala entre uma fase e outra.
            var phaseOne = new List<GameObject>();
            var phaseTwo = new List<GameObject>();

            // O trono na frente do janelao da cratera (1) e dos veios (2).
            phaseOne.Add(Prop(parent, "palace_throne", tx, ty, 3));
            phaseTwo.Add(Prop(parent, "palace_throne_broken", tx, ty, 3));

            foreach (float x in Phase05Layout.ThroneMirrors)
            {
                phaseOne.Add(Prop(parent, "palace_mirror", x, ky, 2));
                phaseTwo.Add(Prop(parent, "palace_mirror_broken", x, ky, 2));
            }

            // Os cacos de espelho que viram escudo contra o sopro.
            var covers = new List<Transform>();
            foreach (float x in Phase05Layout.BreathCovers)
            {
                GameObject shard = Prop(parent, "palace_mirror_shard", x, ky, 23);
                phaseTwo.Add(shard);
                covers.Add(shard.transform);
            }

            foreach (Light2D light in oxideLights) phaseTwo.Add(light.gameObject);

            // O Rei.
            var kingObject = new GameObject("SilverKing");
            kingObject.transform.SetParent(parent, false);
            kingObject.transform.position = new Vector3(kx, ky, 0f);

            Sprite[] regal = Phase02Assets.LoadSheet(KingRegalSheet, KingArt.FrameCount);
            Sprite[] corrupt = Phase02Assets.LoadSheet(KingCorruptSheet, KingArt.FrameCount);

            SpriteRenderer body = kingObject.AddComponent<SpriteRenderer>();
            body.sprite = regal.Length > 0 ? regal[KingArt.Seated] : null;
            body.sortingLayerName = Phase01Assets.SortingActors;
            body.sortingOrder = -2;
            body.flipX = true;

            kingObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

            // Do tamanho do rei desenhado: do chao ate a coroa.
            float tall = KingArt.StandingHeight - 0.6f;
            var area = kingObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(2.1f, tall);
            area.offset = new Vector2(0f, tall * 0.5f);

            var king = kingObject.AddComponent<SilverKing>();
            BuildHeartWindow(parent, king, tx, ty);

            // Os golpes.
            KingBeam beamA = Beam(parent, "ScepterBeam_A");
            KingBeam beamB = Beam(parent, "ScepterBeam_B");

            var breathObject = new GameObject("OxideBreath");
            breathObject.transform.SetParent(parent, false);
            var breath = breathObject.AddComponent<OxideBreath>();
            breath.Configure(ax0, ax1 + 1f, Phase05Layout.ArenaFloorY, Phase02Assets.LoadSprite(Puff), covers.ToArray());

            // A cutscene da transformacao.
            var cartoonObject = new GameObject("KingTransformation");
            cartoonObject.transform.SetParent(parent, false);
            var cartoon = cartoonObject.AddComponent<PurificationCartoon>();
            cartoon.Configure(CartoonAssets.LoadSet(CartoonKing, FinaleCartoonArt.Transformation()));

            // A coroa caida, que aparece quando ele tomba de vez: uma das duas escolhas do fim.
            (float crx, float cry) = Phase05Layout.CrownTile;
            GameObject crown = Prop(parent, "palace_crown", crx, cry, 24, Phase01Assets.SortingActors);
            crown.SetActive(false);

            (float sx, float sy) = KingArt.ScepterTip;
            (float ux, float uy) = KingArt.SkywardTip;
            (float cx, float cy) = KingArt.CrownPoint;
            (float mx, float my) = KingArt.Mouth;

            king.Configure(minX, maxX, Phase05Layout.ArenaFloorY, Phase05Layout.ArenaCeilingY,
                new Rect(wx, wy, ww, wh), new Vector2(sx, sy), new Vector2(ux, uy), new Vector2(cx, cy), new Vector2(mx, my), KingArt.SweepReach);
            king.ConfigureAttacks(beamA, beamB, breath,
                Phase02Assets.LoadSprite(Facet), Phase02Assets.LoadSprite(FacetCorrupt), Phase02Assets.LoadSprite(Glint));
            king.ConfigureStage(phaseOne.ToArray(), phaseTwo.ToArray(), global, cartoon, null, crown);
            king.ConfigureFrames(regal, corrupt);
            king.ConfigurePassThrough(Phase05Layout.ArenaLedges.Select(l => new Rect(l.X0, l.Y, l.X1 - l.X0 + 1, 1f)).ToArray());

            // Deitado de vez, ele vira o loop da carcaca: a armadura rachada e o Oxide indo embora.
            king.ConfigureWreck(WreckAssets.Build(parent, "SilverKingWreck",
                Phase02Assets.LoadSheet(KingWreckSheet, KingWreckArt.FrameCount), kingObject.transform.position, -2));

            // A luz da coroa e a do peito, que vao junto com ele.
            (float hx, float hy) = KingArt.Chest;
            Phase01Generator.CreatePointLight(kingObject.transform, "CrownGlow", (kx, ky + cy), new Color(1f, 0.96f, 0.85f), 0.8f, 5f);
            Phase01Generator.CreatePointLight(kingObject.transform, "HeartGlow", (kx + hx, ky + hy), OxideGreen, 0.5f, 4f);

            BuildFinale(parent, global, crown, tx, ty);
        }

        // ------------------------------------------------------------------
        // O final, na propria Sala do Trono
        // ------------------------------------------------------------------

        /// <summary>
        /// O Crisol que chega correndo depois do Rei, e as duas escolhas: a
        /// Coluna atras do trono (o nucleo, a cura, o Scrap se apaga) e a coroa
        /// caida (as 925 facetas quebradas, a luz volta, o Scrap fica).
        /// </summary>
        private static void BuildFinale(Transform parent, Light2D global, GameObject crown, float tx, float ty)
        {
            float floor = Phase05Layout.FloorY;

            // Os dois fins.
            FinaleDirector column = Director(parent, "Finale_Column", CartoonPolish, FinaleCartoonArt.EndingA(), global, new Vector2(tx, ty),
                Phase05Layout.EndingColumnTitle, Phase05Layout.EndingColumnStory, new Color(1f, 0.97f, 0.88f));
            FinaleDirector crowned = Director(parent, "Finale_Crown", CartoonCrown, FinaleCartoonArt.EndingB(), global, new Vector2(crown.transform.position.x, floor),
                Phase05Layout.EndingCrownTitle, Phase05Layout.EndingCrownStory, new Color(0.86f, 0.95f, 1f));

            // A Coluna: o janelao atras do trono, de alto a baixo.
            var columnObject = new GameObject("Choice_Column");
            columnObject.transform.SetParent(parent, false);
            columnObject.transform.position = new Vector3(tx, ty, 0f);
            var columnArea = columnObject.AddComponent<BoxCollider2D>();
            columnArea.isTrigger = true;
            columnArea.size = new Vector2(3f, 5f);
            columnArea.offset = new Vector2(0f, 2.5f);
            Light2D columnGlow = Phase01Generator.CreatePointLight(columnObject.transform, "Glow", (tx, ty + 4f), new Color(0.9f, 0.96f, 1f), 0f, 6f);
            columnObject.AddComponent<FinaleChoice>().Configure(column, null, columnGlow);

            // A coroa: o desenho dela pulsa junto.
            var crownArea = crown.AddComponent<BoxCollider2D>();
            crownArea.isTrigger = true;
            crownArea.size = new Vector2(1.8f, 1.4f);
            crownArea.offset = new Vector2(0f, 0.7f);
            Light2D crownGlow = Phase01Generator.CreatePointLight(crown.transform, "Glow",
                (crown.transform.position.x, floor + 0.8f), new Color(1f, 0.95f, 0.8f), 0f, 3.5f);
            crown.AddComponent<FinaleChoice>().Configure(crowned, crown.GetComponent<SpriteRenderer>(), crownGlow);

            // O Crisol que vem correndo.
            (float from, float to) = Phase05Layout.FinaleCrisolRun;
            var crisolObject = new GameObject("Finale_Crisol");
            crisolObject.transform.SetParent(parent, false);
            crisolObject.transform.position = new Vector3(from, floor, 0f);
            SpriteRenderer body = Phase02Assets.AddCrisol(crisolObject, -3);
            crisolObject.AddComponent<FinaleCrisol>().Configure(body, GameObject.Find("Npc_Crisol"), from, to, floor,
                Phase05Layout.FinaleCrisolLines);
        }

        private static FinaleDirector Director(Transform parent, string name, string set, PurificationCartoonArt.Shot[] shots,
            Light2D global, Vector2 focus, string title, string story, Color tint)
        {
            var cartoonObject = new GameObject(name + "_Cartoon");
            cartoonObject.transform.SetParent(parent, false);
            var cartoon = cartoonObject.AddComponent<PurificationCartoon>();
            cartoon.Configure(CartoonAssets.LoadSet(set, shots));

            var directorObject = new GameObject(name);
            directorObject.transform.SetParent(parent, false);
            var director = directorObject.AddComponent<FinaleDirector>();
            director.Configure(cartoon, null, null, global, focus);
            director.ConfigureEnding(title, story, false, tint);
            return director;
        }
    }
}
