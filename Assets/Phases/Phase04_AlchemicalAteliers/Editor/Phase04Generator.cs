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
    /// Monta a cena da Phase 4 do zero, a partir do desenho em Phase04Layout:
    /// o laboratorio inteiro, os corrompidos animados, os reagentes e o Grande
    /// Alambique, o Alquimista de Mercurio e o palco do anfiteatro, que
    /// purifica o distrito.
    ///
    /// Rodar de novo refaz a cena inteira e descarta o que voce tiver ajustado
    /// nela na mao — entao ajuste o layout no codigo enquanto estiver iterando.
    ///
    /// Menu:      Echoes > Gerar Phase 4 (Alchemical Ateliers)
    ///            Echoes > Validar layout da Phase 4
    /// Batchmode: -executeMethod EchoesOfCravejada.EditorTools.Phase04Generator.GenerateFromCommandLine
    /// </summary>
    public static class Phase04Generator
    {
        public const string ScenePath = "Assets/Phases/" + Phase04Layout.SceneName + "/Scene/" + Phase04Layout.SceneName + ".unity";
        private const string GroundLayerName = "Ground";

        private const string MarblePath = Phase02Assets.BrassFolder + "/marble_tiles.png";
        private const string MarblePrefix = "marble";

        // Os nomes dos sprites desta fase, na mesma pasta dos de latao.
        private const string Mercury = "mercury_body";
        private const string Glass = "glass_pane";
        private const string SilverPlate = "silver_plate";
        private const string MarbleBlock = "marble_block";
        private const string Circle = "transmutation_circle";
        private const string Emitter = "light_emitter";
        private const string Drop = "mercury_drop";
        private const string Flask = "oxide_flask";
        private const string Cabinet = "reagent_cabinet";
        private const string AlchemistSheet = "alchemist_sheet";
        private const string StairDoor = "stair_palace_door";
        private static string StairFlightName(int steps) => "stair_flight_" + steps;
        private static string StairLandingName(int tiles) => "stair_landing_" + tiles;
        private const string SlimeSheet = "d4_slime_sheet";
        private const string ThrowerSheet = "d4_thrower_sheet";
        private const string ExperimentSheet = "d4_experiment_sheet";
        private const string GlassSentinelSheet = "d4_glass_sentinel_sheet";
        private const string SilverSentinelSheet = "d4_silver_sentinel_sheet";
        private const string BellShadow = "bell_shadow";
        private const string Bead = "mercury_bead";
        private const string Whip = "mercury_whip";

        private static string FrameName(string prefix, int frame) => $"{prefix}_{frame}";
        private static string WindowName(int variant) => "stained_window_" + variant;
        private static string BellName(int cracks) => "bell_jar_" + cracks;

        /// <summary>O fundo da camera: azul-acinzentado escuro, frio.</summary>
        private static readonly Color Gloom = new Color(0.06f, 0.07f, 0.11f);

        private static readonly Color BackTint = new Color(0.42f, 0.45f, 0.56f);
        private static readonly Color OxideGreen = new Color(0.36f, 0.95f, 0.60f);
        private static readonly Color LampGlow = new Color(0.86f, 0.90f, 1f);
        private static readonly Color RuneDim = new Color(0.50f, 0.55f, 0.68f);

        private static Material _unlit;
        private static Material _steam;
        private static int _groundLayer;

        // O que uma parte da montagem deixa pra outra.
        private static readonly Dictionary<string, GameObject> Transmutables = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, Gate> BuiltGates = new Dictionary<string, Gate>();
        private static readonly List<Light2D> OxideLights = new List<Light2D>();

        [MenuItem("Echoes/Gerar Phase 4 (Alchemical Ateliers)", false, 50)]
        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[Phase 4] Geracao cancelada.");
                return;
            }

            Build();
        }

        /// <summary>Confere que o mapa e navegavel sem precisar entrar no jogo.</summary>
        [MenuItem("Echoes/Validar layout da Phase 4", false, 51)]
        public static void ValidateLayout()
        {
            List<string> problems = Phase04Layout.Validate();

            if (problems.Count == 0)
            {
                Debug.Log("[Phase 4] Layout OK: as tres alas, o alambique, o Grande Laboratorio, a fuga e o palco estao ao alcance na ordem certa.");
            }
            else
            {
                foreach (string problem in problems) Debug.LogWarning($"[Phase 4] {problem}");
                Debug.LogError($"[Phase 4] {problems.Count} problema(s) no layout. Veja os avisos acima.");
            }

            foreach (string floor in Phase04Layout.UnreachableFloors())
            {
                Debug.Log($"[Phase 4] Chao sem acesso (pode ser so o patamar de um arremessador): {floor}");
            }
        }

        /// <summary>Ponto de entrada pro modo batch, sem nenhum dialogo.</summary>
        public static void GenerateFromCommandLine()
        {
            Build();
        }

        private static void Build()
        {
            Phase01Assets.EnsureFolders();
            _groundLayer = Phase01Assets.EnsureLayer(GroundLayerName);

            Phase01Assets.EnsureSortingLayers();
            Phase01Assets.VerifySortingLayers();

            foreach (string problem in Phase04Layout.Validate())
            {
                Debug.LogWarning($"[Phase 4] {problem}");
            }

            if (!Phase01Assets.SliceTileset() || !Phase01Assets.SliceBackgroundTileset() || !Phase01Assets.SliceObjects())
            {
                Debug.LogError("[Phase 4] Abortado: nao consegui fatiar as folhas do pack de caverna.");
                return;
            }

            // A bancada, Crisol, o portao e o elevador vem da arte de latao.
            if (!Phase02Assets.EnsureArt(new[] { 4 }) || !Phase02Assets.EnsureMechanicsArt())
            {
                Debug.LogError("[Phase 4] Abortado: nao consegui preparar a arte de latao.");
                return;
            }

            if (!EnsureArt())
            {
                Debug.LogError("[Phase 4] Abortado: nao consegui fatiar o tileset de marmore.");
                return;
            }

            Dictionary<string, TileBase> tiles = Phase02Assets.BuildTiles();
            Phase01Assets.AddTilesFrom(MarblePath, tiles);

            if (tiles.Count == 0)
            {
                Debug.LogError("[Phase 4] Abortado: nenhum tile foi criado.");
                return;
            }

            Sprite scrapSprite = Phase01Assets.CreateScrapPlaceholder();
            Sprite polishedSprite = Phase02Assets.LoadSprite(Phase02Assets.ScrapPolished);
            _steam = Phase01Assets.GetOrCreateFogMaterial();
            _unlit = Phase01Assets.GetOrCreateUnlitMaterial();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Transmutables.Clear();
            BuiltGates.Clear();
            OxideLights.Clear();

            BuildTilemaps(tiles);
            BuildLighting();
            BuildProps();
            BuildMarkers();

            var travel = new GameObject("Travel");
            SceneKit.CreateSpawnPoint(travel.transform, Phase04Layout.FromDistrict3SpawnId, Phase04Layout.SpawnTile);

            Transform scrap = Phase01Generator.BuildScrap(scrapSprite, polishedSprite, _groundLayer, Phase04Layout.SpawnTile);
            Phase01Generator.BuildCamera(scrap, Phase04Layout.Width, Phase04Layout.Height, Gloom);

            // O mapa desta fase conta com tudo o que ele ganhou ate aqui, entao
            // o Scrap ja chega com as tres — mesmo dando Play direto na cena.
            scrap.GetComponent<ScrapDash>().SetUnlockedFromStart(true);
            scrap.GetComponent<ScrapAppearance>().SetPolishedFromStart(true);
            scrap.gameObject.AddComponent<AbilityGrant>().Configure(true, true, true);

            var root = new GameObject("Mechanics").transform;

            BuildSpikes(Child(root, "Spikes"));
            BuildPools(Child(root, "Mercury"));
            BuildBlocks(Child(root, "Transmutables"));
            BuildVapor(Child(root, "Vapor"));
            BuildCircles(Child(root, "TransmutationCircles"));
            BuildBeams(Child(root, "LightBeams"));
            BuildDrips(Child(root, "MercuryDrips"));
            BuildGates(Child(root, "Gates"));
            BuildReagents(Child(root, "Reagents"));
            BuildAlembic(root);
            BuildSecrets(Child(root, "Secrets"));
            BuildGrandStair(Child(root, "GrandStair"));

            Transform enemies = Child(root, "Enemies");
            BuildFoes(enemies);

            BuildWorkbenches(Child(root, "Workbenches"));
            GameObject crisol = BuildCrisolAt(root, "Npc_Crisol", Phase04Layout.CrisolTile, Phase04Layout.CrisolLines);
            GameObject cabinet = BuildLore(Child(root, "Lore"));

            // Por ultimo: a purificacao precisa de tudo o que ela mexe, e o
            // chefe e o palco precisam dela.
            AreaPurifier purifier = BuildPurification(root, enemies.gameObject, crisol, cabinet);
            BuildBoss(root);
            BuildCeremony(root, purifier);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneKit.AddSceneToBuildSettings(ScenePath);

            Debug.Log($"[Phase 4] Cena gerada: {ScenePath}. " +
                      $"Mapa {Phase04Layout.Width}x{Phase04Layout.Height}, {Phase04Layout.Rooms.Count} salas, " +
                      $"{Phase04Layout.Foes.Count} inimigos, {Phase04Layout.Lore.Count} leituras, {Phase04Layout.Props.Count} pecas de laboratorio, " +
                      "e o Alquimista de Mercurio.");
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

        private static Sprite[] LoadFrames(string prefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++) frames[i] = Phase02Assets.LoadSprite(FrameName(prefix, i));
            return frames;
        }

        /// <summary>Um bloco solido de celulas, com um sprite repetido por cima.</summary>
        private static GameObject SolidBlock(Transform parent, string name, Phase04Layout.Cells area, string spriteName, int order)
        {
            var blockObject = new GameObject(name) { layer = _groundLayer };
            blockObject.transform.SetParent(parent, false);
            blockObject.transform.position = new Vector3(area.X0, area.Y0, 0f);

            SpriteRenderer plate = Sprite(blockObject, spriteName, Phase01Assets.SortingTerrain, order);
            plate.drawMode = SpriteDrawMode.Tiled;
            plate.size = new Vector2(area.Width, area.Height);

            var collider = blockObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(area.Width, area.Height);
            collider.offset = new Vector2(area.Width * 0.5f, area.Height * 0.5f);

            return blockObject;
        }

        // ------------------------------------------------------------------
        // Arte
        // ------------------------------------------------------------------

        private static bool EnsureArt()
        {
            var feet = new Vector2(0.5f, 0f);
            var top = new Vector2(0.5f, 1f);
            var middle = new Vector2(0.5f, 0.5f);
            var corner = Vector2.zero;

            Phase02Assets.WriteCanvas(MarblePath, MarbleArt.Tiles());

            Phase02Assets.WriteSprite(Mercury, MarbleArt.Mercury(), corner, true);
            Phase02Assets.WriteSprite(Glass, MarbleArt.Glass(), corner, true);
            Phase02Assets.WriteSprite(SilverPlate, MarbleArt.SilverPlate(), corner, true);
            Phase02Assets.WriteSprite(MarbleBlock, MarbleArt.MarbleBlock(), corner, true);
            Phase02Assets.WriteSprite(Circle, MarbleArt.Circle(), feet);
            Phase02Assets.WriteSprite(Emitter, MarbleArt.Emitter(), middle);
            Phase02Assets.WriteSprite(Drop, MarbleArt.Drop(), middle);
            Phase02Assets.WriteSprite(Flask, MarbleArt.Flask(), middle);
            Phase02Assets.WriteSprite(Cabinet, MarbleArt.Cabinet(), feet);
            for (int variant = 0; variant < 2; variant++) Phase02Assets.WriteSprite(WindowName(variant), MarbleArt.Window(variant), feet);

            // O laboratorio.
            foreach ((string name, System.Func<PixelCanvas> draw, bool hanging) in LabArt.Sprites())
            {
                Phase02Assets.WriteSprite(name, draw(), hanging ? top : feet);
            }

            for (int lit = 0; lit <= 3; lit++) Phase02Assets.WriteSprite(LabArt.GreatAlembicName(lit), LabArt.GreatAlembic(lit), feet);
            for (int i = 0; i < 3; i++) Phase02Assets.WriteSprite(LabArt.ReagentName(i), LabArt.Reagent(i), middle);
            for (int cracks = 0; cracks <= 3; cracks++) Phase02Assets.WriteSprite(BellName(cracks), LabArt.BellJar(cracks), feet);
            Phase02Assets.WriteSprite(BellShadow, LabArt.BellShadow(), middle);
            Phase02Assets.WriteSprite(Bead, LabArt.MercuryBead(), middle);
            Phase02Assets.WriteSprite(Whip, LabArt.WhipSegment(), middle, true);

            // A Grande Escadaria: os lances, os patamares e a porta do palacio.
            foreach (int steps in Phase04Layout.Flights.Select(f => f.Steps).Distinct())
            {
                Phase02Assets.WriteSprite(StairFlightName(steps), StairArt.Flight(steps), corner);
            }

            foreach (int tiles in Phase04Layout.Landings.Select(l => l.X1 - l.X0 + 1).Distinct())
            {
                Phase02Assets.WriteSprite(StairLandingName(tiles), StairArt.Landing(tiles), corner);
            }

            Phase02Assets.WriteSprite(StairDoor, StairArt.Doorway(), feet);

            // A Assistente, que aparece na plateia depois da purificacao.
            for (int f = 0; f < LabFoeArt.IdleFrames; f++)
            {
                Phase02Assets.WriteSprite(FrameName("assistant", f), LabFoeArt.Assistant(f), feet);
            }

            // Os corrompidos, no detalhe das outras fases: uma folha cada, 32 px
            // por tile, com todas as poses e a morte. A sentinela tem duas: a de
            // vidro e a de prata.
            Phase02Assets.WriteSheet(SlimeSheet, SlimeArt.Frames(), new Vector2(0.5f, SlimeArt.PivotY), SlimeArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(ThrowerSheet, ThrowerArt.Frames(), new Vector2(ThrowerArt.PivotX, ThrowerArt.PivotY), ThrowerArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(ExperimentSheet, ExperimentArt.Frames(),
                new Vector2(ExperimentArt.PivotX, ExperimentArt.PivotY), ExperimentArt.PixelsPerUnit);
            var sentinelPivot = new Vector2(GlassSentinelArt.PivotX, GlassSentinelArt.PivotY);
            Phase02Assets.WriteSheet(GlassSentinelSheet, GlassSentinelArt.Frames(false), sentinelPivot, GlassSentinelArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(SilverSentinelSheet, GlassSentinelArt.Frames(true), sentinelPivot, GlassSentinelArt.PixelsPerUnit);

            // O que se le: o desenho do mapa e o grande, da leitura.
            LoreArtAssets.Write(LoreArt.Phase04.Values);

            // O chefe, no detalhe dos chefes: uma folha, 32 px por tile.
            Phase02Assets.WriteSheet(AlchemistSheet, AlchemistArt.Frames(),
                new Vector2(AlchemistArt.PivotX, AlchemistArt.PivotY), AlchemistArt.PixelsPerUnit);

            return Phase01Assets.SliceGrid(MarblePath, MarblePrefix);
        }

        // ------------------------------------------------------------------
        // Terreno
        // ------------------------------------------------------------------

        private static void BuildTilemaps(Dictionary<string, TileBase> tiles)
        {
            var gridObject = new GameObject("Grid");
            var grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            Transform root = gridObject.transform;
            string terrain = Phase01Assets.SortingTerrain;

            Tilemap back = Phase01Generator.CreateTilemap(root, "TM_Background", terrain, 0);
            back.color = BackTint;

            Tilemap ground = Phase01Generator.CreateTilemap(root, "TM_Ground", terrain, 10);

            Tilemap decoration = Phase01Generator.CreateTilemap(root, "TM_Decoration", terrain, 20);
            decoration.color = new Color(0.85f, 0.90f, 1f);

            GameObject groundObject = ground.gameObject;
            groundObject.layer = _groundLayer;

            var body = groundObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;

            var tilemapCollider = groundObject.AddComponent<TilemapCollider2D>();
            var composite = groundObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            // Refaz o colisor quando a cena comeca: carregada no meio do jogo
            // ela chegava sem chao.
            groundObject.AddComponent<GroundColliderRefresh>();

            int width = Phase04Layout.Width;
            int height = Phase04Layout.Height;

            var backBuffer = new TileBase[width * height];
            var groundBuffer = new TileBase[width * height];
            var decorationBuffer = new TileBase[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = x + y * width;

                    backBuffer[index] = Phase01Generator.Resolve(tiles, Phase04Layout.BackgroundTileName(x, y));
                    groundBuffer[index] = Phase01Generator.Resolve(tiles, Phase04Layout.GroundTileName(x, y));

                    if (Phase04Layout.IsSpike(x, y))
                    {
                        decorationBuffer[index] = Phase01Generator.Resolve(tiles, Phase04Layout.SpikeTileName(x, y));
                    }
                }
            }

            var bounds = new BoundsInt(0, 0, 0, width, height, 1);
            back.SetTilesBlock(bounds, backBuffer);
            ground.SetTilesBlock(bounds, groundBuffer);
            decoration.SetTilesBlock(bounds, decorationBuffer);

            foreach (Tilemap tilemap in new[] { back, ground, decoration })
            {
                Phase01Generator.FinishTilemap(tilemap);
            }

            composite.GenerateGeometry();
        }

        // ------------------------------------------------------------------
        // Luz e cenario
        // ------------------------------------------------------------------

        private static void BuildLighting()
        {
            var root = new GameObject("Lighting");

            var globalObject = new GameObject("GlobalLight2D");
            globalObject.transform.SetParent(root.transform, false);

            // Fria e baixa: marmore no escuro, lustres apagados. O que acende a
            // sala sao os lampioes de laboratorio e o verde dos frascos.
            var global = globalObject.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.color = new Color(0.72f, 0.80f, 1f);
            global.intensity = 0.40f;

            int lamps = 0;

            foreach ((float x, float y, float radius, bool green) in Phase04Layout.Lamps)
            {
                if (green)
                {
                    Light2D glow = Phase01Generator.CreatePointLight(root.transform, $"OxideGlow_{OxideLights.Count + 1}",
                        (x, y), OxideGreen, 0.75f, radius);
                    glow.gameObject.AddComponent<GlowPulse>().Configure(glow, null, 2.6f + OxideLights.Count % 3 * 0.4f, OxideLights.Count * 0.17f);
                    OxideLights.Add(glow);
                }
                else
                {
                    Phase01Generator.CreatePointLight(root.transform, $"Lamp_{++lamps}", (x, y), LampGlow, 0.75f, radius);
                }
            }
        }

        /// <summary>O laboratorio por cima do terreno: vidraria, tanques, mobilia, vitrais. Atras de tudo o que se mexe.</summary>
        private static void BuildProps()
        {
            var root = new GameObject("LabDressing");

            foreach (Phase04Layout.Prop prop in Phase04Layout.Props)
            {
                // O Grande Alambique e da porta dele (BuildAlembic).
                if (prop.Sprite == "lab_great_alembic") continue;

                var propObject = new GameObject($"{prop.Sprite}_{prop.X:0.#}_{prop.Y:0.#}");
                propObject.transform.SetParent(root.transform, false);

                if (prop.Sprite == "elevator")
                {
                    // A cabine do elevador do District 3, com os trilhos subindo ate o teto.
                    propObject.transform.position = new Vector3(prop.X - 1f, prop.Y - 4f / Phase01Assets.PixelsPerUnit, 0f);
                    Sprite(propObject, Phase02Assets.ElevatorCage, Phase01Assets.SortingActors, -4);

                    var rails = new GameObject("Rails");
                    rails.transform.SetParent(propObject.transform, false);
                    rails.transform.localPosition = new Vector3(0f, 3f, 0f);
                    SpriteRenderer rail = Sprite(rails, Phase02Assets.ElevatorRail, Phase01Assets.SortingTerrain, 4);
                    rail.drawMode = SpriteDrawMode.Tiled;
                    rail.size = new Vector2(3f, 7f);
                    continue;
                }

                propObject.transform.position = new Vector3(prop.X, prop.Y, 0f);
                Sprite(propObject, prop.Sprite, Phase01Assets.SortingTerrain, prop.Order);

                // O lustre balanca devagar, preso no teto.
                if (prop.Sprite == "lab_chandelier") propObject.AddComponent<Sway>().Configure(1.5f, 4.5f, (prop.X * 0.37f) % 1f);
            }

            foreach ((int variant, int x, int y) in Phase04Layout.Windows)
            {
                var window = new GameObject($"window_{x}_{y}");
                window.transform.SetParent(root.transform, false);
                window.transform.position = new Vector3(x + 1f, y, 0f);
                Sprite(window, WindowName(variant), Phase01Assets.SortingTerrain, 2);
            }
        }

        /// <summary>Objetos vazios marcando os lugares do layout, agrupados por sala.</summary>
        private static void BuildMarkers()
        {
            var root = new GameObject("Markers");
            var groups = new Dictionary<string, Transform>();

            foreach (Phase04Layout.Marker marker in Phase04Layout.Markers)
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
        // Espinhos, mercurio e transmutacao
        // ------------------------------------------------------------------

        private static void BuildSpikes(Transform parent)
        {
            for (int y = 0; y < Phase04Layout.Height; y++)
            {
                int start = -1;

                for (int x = 0; x <= Phase04Layout.Width; x++)
                {
                    bool spike = x < Phase04Layout.Width && Phase04Layout.IsSpike(x, y);

                    if (spike && start < 0) start = x;
                    if (spike || start < 0) continue;

                    int width = x - start;

                    var spikes = new GameObject($"Spikes_{start}_{y}");
                    spikes.transform.SetParent(parent, false);
                    spikes.transform.position = new Vector3(start + width * 0.5f, y + 0.35f, 0f);

                    var trigger = spikes.AddComponent<BoxCollider2D>();
                    trigger.isTrigger = true;
                    trigger.size = new Vector2(width - 0.3f, 0.6f);

                    spikes.AddComponent<Hazard>();
                    start = -1;
                }
            }
        }

        private static void AddSurface(GameObject pool, Phase04Layout.Cells area, float current)
        {
            // A faixa logo acima da superficie: e ela que avisa o Scrap de que
            // ele esta pisando em mercurio.
            var surfaceObject = new GameObject("Surface");
            surfaceObject.transform.SetParent(pool.transform, false);
            surfaceObject.transform.localPosition = new Vector3(area.Width * 0.5f, area.Height + 0.3f, 0f);

            var trigger = surfaceObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(area.Width, 0.6f);

            surfaceObject.AddComponent<MercurySurface>().Configure(current);
        }

        private static void BuildPools(Transform parent)
        {
            foreach (Phase04Layout.Pool pool in Phase04Layout.Pools)
            {
                GameObject poolObject = SolidBlock(parent, pool.Name, pool.Area, Mercury, 11);

                // O corrompido e esverdeado: da pra saber antes de pisar.
                if (!Mathf.Approximately(pool.Current, 0f)) poolObject.GetComponent<SpriteRenderer>().color = new Color(0.70f, 1f, 0.78f);

                AddSurface(poolObject, pool.Area, pool.Current);
                Transmutables[pool.Name] = poolObject;
            }
        }

        private static void BuildDrips(Transform parent)
        {
            Sprite drop = Phase02Assets.LoadSprite(Drop);

            for (int i = 0; i < Phase04Layout.Drips.Count; i++)
            {
                (int x, int y, int fall) = Phase04Layout.Drips[i];

                var dripObject = new GameObject($"MercuryDrip_{i + 1}");
                dripObject.transform.SetParent(parent, false);
                dripObject.transform.position = new Vector3(x + 0.5f, y + 0.9f, 0f);

                // Cada uma no seu ritmo, e nenhuma comecando junto.
                dripObject.AddComponent<MercuryDrip>().Configure(
                    2.2f + LevelMap.Pick(x, y, 5) * 0.25f, LevelMap.Pick(x, y + 1, 20) * 0.1f, fall + 0.9f, drop);
            }
        }

        private static void BuildBlocks(Transform parent)
        {
            foreach (Phase04Layout.Block block in Phase04Layout.Blocks)
            {
                string sprite = block.Kind == Phase04Layout.BlockKind.Glass ? Glass
                    : block.Kind == Phase04Layout.BlockKind.Silver ? SilverPlate
                    : MarbleBlock;

                Transmutables[block.Name] = SolidBlock(parent, block.Name, block.Area, sprite, 12);
            }
        }

        private static void BuildVapor(Transform parent)
        {
            foreach ((string name, Phase04Layout.Cells area) in Phase04Layout.VaporLifts)
            {
                SteamVent vent = SceneKit.CreateSteamVent(parent, name,
                    new RectInt(area.X0, area.Y0, area.Width, area.Height),
                    SteamVent.Mode.Lift, 1, 0f, 0f, _steam);

                Transmutables[name] = vent.gameObject;
            }
        }

        private static GameObject[] Lookup(string[] names)
        {
            return names.Where(Transmutables.ContainsKey).Select(name => Transmutables[name]).ToArray();
        }

        private static SpriteRenderer Rune(GameObject target, float scale = 1f)
        {
            // A gravura fica rente ao chao e tem luz propria: e pra ser achada.
            SpriteRenderer rune = Sprite(target, Circle, Phase01Assets.SortingTerrain, 21);
            rune.color = RuneDim;
            target.transform.localScale = Vector3.one * scale;
            if (_unlit != null) rune.sharedMaterial = _unlit;
            return rune;
        }

        private static void BuildCircles(Transform parent)
        {
            foreach (Phase04Layout.Circle circle in Phase04Layout.Circles)
            {
                var circleObject = new GameObject(circle.Name);
                circleObject.transform.SetParent(parent, false);
                circleObject.transform.position = new Vector3(circle.X + 0.5f, circle.Y, 0f);

                SpriteRenderer rune = Rune(circleObject);

                var area = circleObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.6f, 2.2f);
                area.offset = new Vector2(0f, 1.1f);

                circleObject.AddComponent<TransmutationCircle>()
                    .Configure(circle.Effect, Lookup(circle.Vanish), Lookup(circle.Appear), rune);
            }
        }

        // ------------------------------------------------------------------
        // Os raios da Optica
        // ------------------------------------------------------------------

        /// <summary>Os raios-perigo: ligam e desligam em ritmo, e so queimam o Scrap.</summary>
        private static void BuildBeams(Transform parent)
        {
            foreach (Phase04Layout.Beam beam in Phase04Layout.Beams)
            {
                var emitterObject = new GameObject(beam.Name);
                emitterObject.transform.SetParent(parent, false);
                emitterObject.transform.position = new Vector3(beam.X + 0.5f, beam.Y + 0.5f, 0f);

                var lanternObject = new GameObject("Lantern");
                lanternObject.transform.SetParent(emitterObject.transform, false);
                SpriteRenderer lantern = Sprite(lanternObject, Emitter, Phase01Assets.SortingTerrain, 22);

                // O desenho olha pra direita: gira pro lado do raio.
                float angle = Mathf.Atan2(beam.DirY, beam.DirX) * Mathf.Rad2Deg;
                if (beam.DirX < 0) lantern.flipX = true;
                else lanternObject.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                var line = emitterObject.AddComponent<LineRenderer>();
                line.sharedMaterial = _unlit;
                line.startColor = new Color(1f, 0.96f, 0.62f, 0.95f);
                line.endColor = new Color(1f, 0.96f, 0.62f, 0.95f);
                line.startWidth = 0.16f;
                line.endWidth = 0.16f;
                line.numCapVertices = 2;
                line.numCornerVertices = 2;
                line.useWorldSpace = true;
                line.sortingLayerName = Phase01Assets.SortingActors;
                line.sortingOrder = 4;
                line.positionCount = 0;

                var light = emitterObject.AddComponent<LightBeam>();
                light.Configure(new Vector2(beam.DirX, beam.DirY));
                light.ConfigurePulse(beam.On, beam.Off, beam.Offset, false);

                Phase01Generator.CreatePointLight(emitterObject.transform, "Glow",
                    (beam.X + 0.5f, beam.Y + 0.5f), new Color(1f, 0.95f, 0.65f), 0.8f, 4f);
            }
        }

        // ------------------------------------------------------------------
        // Portoes, reagentes e o Grande Alambique
        // ------------------------------------------------------------------

        private static void BuildGates(Transform parent)
        {
            foreach (Phase04Layout.GateSpec spec in Phase04Layout.Gates)
            {
                var gateObject = new GameObject(spec.Name) { layer = _groundLayer };
                gateObject.transform.SetParent(parent, false);
                gateObject.transform.position = new Vector3(spec.X, spec.Y, 0f);

                SpriteRenderer bars = Sprite(gateObject, Phase02Assets.GateBars, Phase01Assets.SortingTerrain, 15);
                bars.drawMode = SpriteDrawMode.Tiled;
                bars.size = new Vector2(spec.Width, spec.Height);
                bars.color = new Color(0.85f, 0.90f, 1f);

                var wall = gateObject.AddComponent<BoxCollider2D>();
                wall.size = new Vector2(spec.Width, spec.Height);
                wall.offset = new Vector2(spec.Width * 0.5f, spec.Height * 0.5f);

                var gate = gateObject.AddComponent<Gate>();
                float travel = spec.MoveX != 0 ? spec.Width + 1f : spec.Height + 1f;
                gate.Configure(travel, new Vector2(spec.MoveX, spec.MoveY));
                BuiltGates[spec.Name] = gate;
            }
        }

        private static void BuildReagents(Transform parent)
        {
            foreach (Phase04Layout.Reagent reagent in Phase04Layout.Reagents)
            {
                var pickup = new GameObject(reagent.Id);
                pickup.transform.SetParent(parent, false);
                pickup.transform.position = new Vector3(reagent.X + 0.5f, reagent.Y + 1f, 0f);

                Sprite(pickup, LabArt.ReagentName(reagent.Index), Phase01Assets.SortingActors, -1);

                var trigger = pickup.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = 0.7f;

                pickup.AddComponent<KeyItemPickup>().Configure(reagent.Id, reagent.Name + " obtido",
                    Phase04Layout.ReagentPrefix, Phase04Layout.Reagents.Count);
                pickup.AddComponent<Sway>();

                Phase01Generator.CreatePointLight(pickup.transform, "Glow",
                    (reagent.X + 0.5f, reagent.Y + 1f), new Color(1f, 0.96f, 0.85f), 0.7f, 3.5f);
            }
        }

        /// <summary>O Grande Alambique: o desenho que acende encaixe por encaixe e a area onde se entrega.</summary>
        private static void BuildAlembic(Transform parent)
        {
            Phase04Layout.Prop prop = Phase04Layout.Props.Find(p => p.Sprite == "lab_great_alembic");
            (int ax, int ay) = Phase04Layout.AlembicTile;

            var alembicObject = new GameObject("GreatAlembic");
            alembicObject.transform.SetParent(parent, false);
            alembicObject.transform.position = new Vector3(prop.Sprite != null ? prop.X : ax + 1f, prop.Sprite != null ? prop.Y : ay, 0f);

            SpriteRenderer body = Sprite(alembicObject, LabArt.GreatAlembicName(0), Phase01Assets.SortingTerrain, 3);

            var areaObject = new GameObject("Sockets");
            areaObject.transform.SetParent(alembicObject.transform, false);
            areaObject.transform.position = new Vector3(ax + 0.5f, ay, 0f);

            var area = areaObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(5f, 3f);
            area.offset = new Vector2(0f, 1.5f);

            var stages = new Sprite[4];
            for (int lit = 0; lit <= 3; lit++) stages[lit] = Phase02Assets.LoadSprite(LabArt.GreatAlembicName(lit));

            BuiltGates.TryGetValue(Phase04Layout.AlembicGate, out Gate gate);
            areaObject.AddComponent<AlembicDoor>().Configure(
                Phase04Layout.Reagents.Select(r => r.Id).ToArray(),
                Phase04Layout.Reagents.Select(r => r.Name).ToArray(),
                gate, body, stages);

            Phase01Generator.CreatePointLight(alembicObject.transform, "Glow",
                (ax + 0.5f, ay + 3f), new Color(0.90f, 0.94f, 1f), 0.6f, 5f);
        }

        private static void BuildSecrets(Transform parent)
        {
            // O que o vapor da Destilaria alcanca: um reforco de chassi.
            (int X, int Y) secret = Phase04Layout.SecretChassisTile;

            var pickup = new GameObject("Secret_Chassis");
            pickup.transform.SetParent(parent, false);
            pickup.transform.position = new Vector3(secret.X + 0.5f, secret.Y + 1f, 0f);

            SpriteRenderer module = Sprite(pickup, Phase02Assets.DashModule, Phase01Assets.SortingActors, -1);
            module.color = new Color(1f, 0.85f, 0.45f);

            var trigger = pickup.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            pickup.AddComponent<RewardPickup>().Configure(RewardPickup.Kind.Chassis, "D4_Secret_Chassis");
        }

        /// <summary>
        /// A Grande Escadaria ate o palacio: os meios-degraus (caixas de meio
        /// tile, na camada do chao, que o Scrap sobe andando), o desenho dos
        /// lances e dos patamares e, no alto, a porta do palacio, que e a
        /// saida pro District 5.
        /// </summary>
        private static void BuildGrandStair(Transform parent)
        {
            var stair = new GameObject("GrandStair") { layer = _groundLayer };
            stair.transform.SetParent(parent, false);

            foreach ((int x, int y) in Phase04Layout.HalfSteps)
            {
                var step = stair.AddComponent<BoxCollider2D>();
                step.size = new Vector2(1f, 0.5f);
                step.offset = new Vector2(x + 0.5f, y + 0.25f);
            }

            foreach ((int x0, int steps, int floor) in Phase04Layout.Flights)
            {
                var flight = new GameObject($"Flight_{x0}");
                flight.transform.SetParent(stair.transform, false);
                flight.transform.position = new Vector3(x0, floor, 0f);
                Sprite(flight, StairFlightName(steps), Phase01Assets.SortingTerrain, 12);
            }

            foreach ((int x0, int x1, int y) in Phase04Layout.Landings)
            {
                var landing = new GameObject($"Landing_{x0}");
                landing.transform.SetParent(stair.transform, false);
                landing.transform.position = new Vector3(x0, y, 0f);
                Sprite(landing, StairLandingName(x1 - x0 + 1), Phase01Assets.SortingTerrain, 12);
            }

            // A porta do palacio, no alto, com a luz fria vazando.
            (int dx, int dy, int dw, int dh) = Phase04Layout.PalaceDoor;
            var door = new GameObject("PalaceDoor");
            door.transform.SetParent(stair.transform, false);
            door.transform.position = new Vector3(dx + dw * 0.5f, dy, 0f);
            Sprite(door, StairDoor, Phase01Assets.SortingTerrain, 3);
            Phase01Generator.CreatePointLight(door.transform, "PalaceLight", (dx + dw * 0.5f, dy + 2f), new Color(0.88f, 0.92f, 1f), 0.9f, 6f);

            SceneKit.CreateSceneExit(parent, "Exit_ToDistrict5", new RectInt(dx, dy, dw, dh),
                Phase04Layout.NextSceneName, Phase04Layout.ToDistrict5SpawnId);
        }

        // ------------------------------------------------------------------
        // Os corrompidos
        // ------------------------------------------------------------------

        private static void BuildFoes(Transform parent)
        {
            Sprite shard = Phase02Assets.LoadSprite(Phase02Assets.Shard);
            Sprite[] slime = Phase02Assets.LoadSheet(SlimeSheet, SlimeArt.FrameCount);
            Sprite[] thrower = Phase02Assets.LoadSheet(ThrowerSheet, ThrowerArt.FrameCount);
            Sprite[] experiment = Phase02Assets.LoadSheet(ExperimentSheet, ExperimentArt.FrameCount);
            Sprite[] glass = Phase02Assets.LoadSheet(GlassSentinelSheet, GlassSentinelArt.FrameCount);
            Sprite[] silver = Phase02Assets.LoadSheet(SilverSentinelSheet, GlassSentinelArt.FrameCount);

            foreach (Phase04Layout.Foe spot in Phase04Layout.Foes)
            {
                var foe = new GameObject(spot.Name);
                foe.transform.SetParent(parent, false);
                foe.transform.position = new Vector3(spot.HomeX + 0.5f, spot.Y, 0f);

                var body = foe.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;

                // Gatilho, e nao parede: o Scrap atravessa e apanha.
                var trigger = foe.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;

                float minX = spot.X0 + 0.5f;
                float maxX = spot.X1 + 0.5f;

                switch (spot.Kind)
                {
                    case Phase04Layout.FoeKind.Slime:
                        Body(foe, slime, QuicksilverSlime.Crawl);
                        trigger.size = new Vector2(1.2f, 0.85f);
                        trigger.offset = new Vector2(0f, 0.42f);

                        foe.AddComponent<Damageable>().Configure(2, 3, shard);
                        foe.AddComponent<QuicksilverSlime>().Configure(minX, maxX, slime);
                        break;

                    case Phase04Layout.FoeKind.FlaskThrower:
                        Body(foe, thrower, FlaskThrower.Idle);
                        trigger.size = new Vector2(0.8f, 1.5f);
                        trigger.offset = new Vector2(0f, 0.75f);

                        foe.AddComponent<Damageable>().Configure(2, 4, shard);
                        foe.AddComponent<FlaskThrower>().Configure(Phase02Assets.LoadSprite(Flask),
                            Phase02Assets.LoadSprite(Phase02Assets.AcidSurface), thrower, ThrowerArt.HandHeight);
                        break;

                    case Phase04Layout.FoeKind.FailedExperiment:
                        Body(foe, experiment, FailedExperiment.Idle);
                        trigger.size = new Vector2(1.2f, 1.8f);
                        trigger.offset = new Vector2(0f, 0.9f);

                        foe.AddComponent<Damageable>().Configure(5, 6, shard);
                        foe.AddComponent<FailedExperiment>().Configure(minX, maxX, experiment);
                        break;

                    case Phase04Layout.FoeKind.Sentinel:
                        Body(foe, glass, TransmutedSentinel.Idle);
                        trigger.size = new Vector2(0.9f, 1.8f);
                        trigger.offset = new Vector2(0f, 0.9f);

                        foe.AddComponent<Damageable>().Configure(4, 5, shard);
                        foe.AddComponent<TransmutedSentinel>().Configure(minX, maxX, glass, silver);
                        break;
                }
            }
        }

        /// <summary>O corpo de um corrompido: o primeiro quadro da folha, na camada dos atores.</summary>
        private static SpriteRenderer Body(GameObject target, Sprite[] frames, int firstFrame)
        {
            SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = frames.Length > firstFrame ? frames[firstFrame] : null;
            renderer.sortingLayerName = Phase01Assets.SortingActors;
            renderer.sortingOrder = -3;
            return renderer;
        }

        // ------------------------------------------------------------------
        // Bancadas, Crisol, a Assistente e as leituras
        // ------------------------------------------------------------------

        private static void BuildWorkbenches(Transform parent)
        {
            foreach ((string name, int x, int y) in Phase04Layout.Workbenches)
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

        private static GameObject BuildCrisolAt(Transform parent, string name, (int X, int Y) tile, string[] lines)
        {
            var crisolObject = new GameObject(name);
            crisolObject.transform.SetParent(parent, false);
            crisolObject.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);

            Phase02Assets.AddCrisol(crisolObject, -4);

            var area = crisolObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(3.2f, 2.6f);
            area.offset = new Vector2(0f, 1.3f);

            var crisol = crisolObject.AddComponent<Crisol>();
            crisol.Configure("Crisol", lines);

            // O rubi foi no District 2. Aqui ele so conversa e vende.
            crisol.ConfigureGivesRuby(false);
            return crisolObject;
        }

        /// <summary>
        /// O que da pra ler com E, cada um com a sua arte (LoreArt.Phase04): o
        /// desenho do mapa e o grande, da leitura. A Assistente tambem entra
        /// aqui — ela so fala de dentro do armario. Devolve o armario dela, que
        /// some na purificacao.
        /// </summary>
        private static GameObject BuildLore(Transform parent)
        {
            GameObject cabinet = null;

            foreach (Phase04Layout.LoreSpot spot in Phase04Layout.Lore)
            {
                var loreObject = new GameObject(spot.Name);
                loreObject.transform.SetParent(parent, false);
                loreObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);

                var area = loreObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.6f, 2.5f);
                area.offset = new Vector2(0f, 1.25f);

                var artObject = new GameObject("Art");
                artObject.transform.SetParent(loreObject.transform, false);

                var npc = loreObject.AddComponent<Npc>();

                if (spot.Kind == "assistant")
                {
                    // O armario de reagentes, de porta entreaberta: ela fala la de dentro.
                    Sprite(artObject, Cabinet, Phase01Assets.SortingTerrain, 6);
                    npc.Configure(spot.Title, spot.Lines);
                    cabinet = loreObject;
                }
                else
                {
                    npc.ConfigureReadable(spot.Title, spot.Lines);

                    if (LoreArt.Phase04.TryGetValue(spot.Name, out LoreArt.Piece own))
                    {
                        LoreArtAssets.Apply(own, artObject, npc, Phase01Assets.SortingTerrain);
                    }
                    else
                    {
                        Debug.LogWarning($"[Phase 4] {spot.Name} nao tem arte propria no LoreArt.Phase04.");
                    }
                }

                // Uma luz fraca, pra dar pra achar.
                Phase01Generator.CreatePointLight(loreObject.transform, "Glow",
                    (spot.X + 0.5f, spot.Y + 1.6f), new Color(0.95f, 0.92f, 0.80f), 0.45f, 3.5f);
            }

            return cabinet;
        }

        // ------------------------------------------------------------------
        // A purificacao
        // ------------------------------------------------------------------

        /// <summary>
        /// A purificacao do District 4: parte do palco do anfiteatro. O verde
        /// dos frascos apaga, os corrompidos somem, a Assistente sai do armario
        /// e senta na arquibancada, e Crisol vem assistir.
        /// </summary>
        private static AreaPurifier BuildPurification(Transform parent, GameObject enemies, GameObject crisol, GameObject cabinet)
        {
            GameObject lighting = GameObject.Find("Lighting");
            GameObject globalObject = GameObject.Find("GlobalLight2D");
            GameObject cameraObject = GameObject.Find("Main Camera");
            GameObject groundObject = GameObject.Find("TM_Ground");

            Transform audience = Child(parent, "Audience");
            var lamps = new List<Light2D>();

            foreach (Phase04Layout.LoreSpot spot in Phase04Layout.Purified)
            {
                if (spot.Kind == "crisol")
                {
                    BuildCrisolAt(audience, spot.Name, (spot.X, spot.Y), spot.Lines);
                }
                else
                {
                    var npcObject = new GameObject(spot.Name);
                    npcObject.transform.SetParent(audience, false);
                    npcObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);

                    Sprite(npcObject, FrameName("assistant", 0), Phase01Assets.SortingActors, -4);
                    npcObject.AddComponent<SpriteLoop>().Configure(LoadFrames("assistant", LabFoeArt.IdleFrames), 1.5f, -1);

                    var area = npcObject.AddComponent<BoxCollider2D>();
                    area.isTrigger = true;
                    area.size = new Vector2(2.6f, 2.5f);
                    area.offset = new Vector2(0f, 1.25f);

                    npcObject.AddComponent<Npc>().Configure(spot.Title, spot.Lines);
                }

                if (lighting != null)
                {
                    lamps.Add(Phase01Generator.CreatePointLight(lighting.transform, "PurifiedLamp_" + lamps.Count,
                        (spot.X + 0.5f, spot.Y + 3f), LampGlow, 0.8f, 7f));
                }
            }

            var purifierObject = new GameObject("AreaPurifier");
            purifierObject.transform.SetParent(parent, false);

            var vanish = new List<GameObject> { enemies, crisol };
            if (cabinet != null) vanish.Add(cabinet);

            var purifier = purifierObject.AddComponent<AreaPurifier>();
            purifier.Configure(
                globalObject != null ? globalObject.GetComponent<Light2D>() : null,
                OxideLights.ToArray(), lamps.ToArray(), new ParticleSystem[0],
                groundObject != null ? new[] { groundObject.GetComponent<Tilemap>() } : new Tilemap[0],
                cameraObject != null ? cameraObject.GetComponent<Camera>() : null,
                new[] { audience.gameObject }, vanish.ToArray(), new Gate[0]);

            // Prata limpa: a luz fica branca e clara, sem o verde.
            purifier.ConfigureLook(new Color(0.90f, 0.93f, 1f), 0.7f,
                new Color(0.96f, 0.97f, 1f), new Color(0.08f, 0.09f, 0.13f));

            return purifier;
        }

        // ------------------------------------------------------------------
        // O Alquimista e o palco
        // ------------------------------------------------------------------

        private static void BuildBoss(Transform parent)
        {
            (int bx, int by) = Phase04Layout.BossTile;
            (int X, int Y, int Width, int Height) wake = Phase04Layout.BossWake;
            Phase04Layout.Cells arena = Phase04Layout.ArenaRoom;
            Transform root = Child(parent, "GrandLaboratory");

            // Os circulos dos estrados.
            var circles = new List<ArenaCircle>();
            foreach ((string name, int x, int y) in Phase04Layout.ArenaCircles)
            {
                var circleObject = new GameObject(name);
                circleObject.transform.SetParent(root, false);
                circleObject.transform.position = new Vector3(x + 0.5f, y, 0f);

                SpriteRenderer rune = Rune(circleObject);

                var area = circleObject.AddComponent<BoxCollider2D>();
                area.isTrigger = true;
                area.size = new Vector2(2.4f, 2.2f);
                area.offset = new Vector2(0f, 1.1f);

                var circle = circleObject.AddComponent<ArenaCircle>();
                circle.Configure(rune, 1.1f);
                circles.Add(circle);

                Phase01Generator.CreatePointLight(circleObject.transform, "Glow",
                    (x + 0.5f, y + 0.3f), new Color(0.80f, 1f, 1f), 0.4f, 2.5f);
            }

            // O chao que vira mercurio na segunda metade.
            var flood = new List<GameObject>();
            for (int i = 0; i < Phase04Layout.ArenaFloodPools.Count; i++)
            {
                Phase04Layout.Cells cells = Phase04Layout.ArenaFloodPools[i];

                var poolObject = new GameObject($"Flood_{i + 1}");
                poolObject.transform.SetParent(root, false);
                poolObject.transform.position = new Vector3(cells.X0, cells.Y0 + 0.9f, 0f);

                SpriteRenderer sheet = Sprite(poolObject, Mercury, Phase01Assets.SortingTerrain, 12);
                sheet.drawMode = SpriteDrawMode.Tiled;
                sheet.size = new Vector2(cells.Width, 0.5f);

                AddSurface(poolObject, new Phase04Layout.Cells(cells.X0, cells.Y0, cells.X1, cells.Y0), 0f);
                poolObject.transform.Find("Surface").localPosition = new Vector3(cells.Width * 0.5f, 0.35f, 0f);
                flood.Add(poolObject);
            }

            // A campanula: o vidro (com as paredes de quando prende o Scrap) e a sombra.
            var jarObject = new GameObject("BellJar") { layer = _groundLayer };
            jarObject.transform.SetParent(root, false);
            SpriteRenderer glass = Sprite(jarObject, BellName(0), Phase01Assets.SortingActors, 6);

            var jarArea = jarObject.AddComponent<BoxCollider2D>();
            jarArea.isTrigger = true;
            jarArea.size = new Vector2(2.4f, 2.7f);
            jarArea.offset = new Vector2(0f, 1.35f);

            var walls = new List<Collider2D>();
            foreach ((float ox, float oy, float w, float h) in new[] { (-1.2f, 1.35f, 0.2f, 2.7f), (1.2f, 1.35f, 0.2f, 2.7f), (0f, 2.65f, 2.4f, 0.2f) })
            {
                var wall = new GameObject("Wall") { layer = _groundLayer };
                wall.transform.SetParent(jarObject.transform, false);
                var box = wall.AddComponent<BoxCollider2D>();
                box.size = new Vector2(w, h);
                box.offset = new Vector2(ox, oy);
                walls.Add(box);
            }

            var shadowObject = new GameObject("BellShadow");
            shadowObject.transform.SetParent(root, false);
            SpriteRenderer shadow = Sprite(shadowObject, BellShadow, Phase01Assets.SortingTerrain, 22);

            var cracks = new Sprite[4];
            for (int c = 0; c <= 3; c++) cracks[c] = Phase02Assets.LoadSprite(BellName(c));

            var jar = jarObject.AddComponent<BellJar>();
            jar.Configure(Phase04Layout.ArenaCeilingY, Phase04Layout.ArenaFloorY, glass, cracks, shadow, walls.ToArray());

            // O chicote: um fio de mercurio emendavel.
            var whipObject = new GameObject("MercuryWhip");
            whipObject.transform.SetParent(root, false);
            SpriteRenderer whip = Sprite(whipObject, Whip, Phase01Assets.SortingActors, 5);
            whip.drawMode = SpriteDrawMode.Tiled;
            whip.enabled = false;

            // O Alquimista.
            var bossObject = new GameObject("MercuryAlchemist");
            bossObject.transform.SetParent(root, false);
            bossObject.transform.position = new Vector3(bx + 0.5f, by, 0f);

            Sprite[] frames = Phase02Assets.LoadSheet(AlchemistSheet, AlchemistArt.FrameCount);
            SpriteRenderer bossBody = bossObject.AddComponent<SpriteRenderer>();
            bossBody.sprite = frames.Length > 0 ? frames[AlchemistArt.Idle] : null;
            bossBody.sortingLayerName = Phase01Assets.SortingActors;
            bossBody.sortingOrder = -2;

            var hitbox = bossObject.AddComponent<BoxCollider2D>();
            hitbox.isTrigger = true;
            hitbox.size = new Vector2(1.6f, 3.4f);
            hitbox.offset = new Vector2(0f, 1.7f);

            var rigidbody = bossObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;

            BuiltGates.TryGetValue(Phase04Layout.BossExitGate, out Gate exit);

            var boss = bossObject.AddComponent<MercuryAlchemist>();
            boss.Configure(arena.X0 + 2.5f, arena.X1 - 1.5f, Phase04Layout.ArenaFloorY, Phase04Layout.ArenaCeilingY,
                new Rect(wake.X, wake.Y, wake.Width, wake.Height),
                circles.ToArray(), jar, flood.ToArray(), whip, Phase02Assets.LoadSprite(Bead), exit);
            boss.ConfigureFrames(frames);

            // As lentes acesas, no escuro da arena.
            Phase01Generator.CreatePointLight(bossObject.transform, "LensGlow",
                (bx + 0.8f, by + 3.6f), OxideGreen, 0.6f, 3.5f);

            // A grade da antessala: fecha enquanto ele luta.
            Phase01Generator.CreateArenaSeal(parent, _groundLayer, new RectInt(arena.X0, arena.Y0, 1, 7));
        }

        /// <summary>O grande circulo do palco, que so aparece com o Alquimista caido, e purifica o distrito.</summary>
        private static void BuildCeremony(Transform parent, AreaPurifier purifier)
        {
            (int sx, int sy) = Phase04Layout.StageCircle;

            var circleObject = new GameObject("StageCircle");
            circleObject.transform.SetParent(parent, false);
            circleObject.transform.position = new Vector3(sx + 0.5f, sy, 0f);

            SpriteRenderer rune = Rune(circleObject, 1.5f);

            var area = circleObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(2.4f, 1.6f);
            area.offset = new Vector2(0f, 0.8f);

            var circle = circleObject.AddComponent<TransmutationCircle>();
            circle.Configure("o coração do distrito", new GameObject[0], new GameObject[0], rune);

            Phase01Generator.CreatePointLight(circleObject.transform, "Glow",
                (sx + 0.5f, sy + 0.5f), new Color(0.85f, 0.95f, 1f), 0.6f, 4f);

            var stage = new GameObject("CeremonyStage");
            stage.transform.SetParent(parent, false);
            stage.AddComponent<CeremonyStage>().Configure(circle, purifier);
        }
    }
}
