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
    public static partial class Phase04Generator
    {
        public const string ScenePath = "Assets/Phases/Fase04_AteliêsAlquímicos/Scene/" + Phase04Layout.SceneName + ".unity";
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
        private const string MirrorSprite = "silver_mirror";
        private const string LockSprite = "crystal_lock";
        private const string Drop = "mercury_drop";
        private const string Flask = "oxide_flask";
        private const string Cabinet = "reagent_cabinet";
        private const string AlchemistSheet = "alchemist_sheet";
        private const string AlchemistWreckSheet = "d4_alchemist_wreck_sheet";
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
            scrap.GetComponent<ScrapDash>().DefinirLiberadoDesdeInicio(true);
            scrap.GetComponent<ScrapAppearance>().DefinirPolidoDesdeInicio(true);
            scrap.gameObject.AddComponent<AbilityGrant>().Configurar(true, true, true);

            var root = new GameObject("Mechanics").transform;

            BuildSpikes(Child(root, "Spikes"));
            BuildPools(Child(root, "Mercury"));
            BuildBlocks(Child(root, "Transmutables"));
            BuildVapor(Child(root, "Vapor"));
            BuildCircles(Child(root, "TransmutationCircles"));
            BuildElevator(Child(root, "Elevator"));
            BuildBeams(Child(root, "LightBeams"));
            BuildDrips(Child(root, "MercuryDrips"));
            BuildGates(Child(root, "Gates"));
            BuildMirrorPuzzles(Child(root, "MirrorPuzzles"));
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
            Phase02Assets.WriteSprite(MirrorSprite, MarbleArt.Mirror(), middle);
            Phase02Assets.WriteSprite(LockSprite, MarbleArt.Lock(), middle);
            Phase02Assets.WriteSprite(Drop, MarbleArt.Drop(), middle);
            Phase02Assets.WriteSprite(Flask, MarbleArt.Flask(), middle);
            Phase02Assets.WriteSprite(Cabinet, MarbleArt.Cabinet(), feet);
            for (int variant = 0; variant < 2; variant++) Phase02Assets.WriteSprite(WindowName(variant), MarbleArt.Window(variant), feet);

            // O laboratorio.
            foreach ((string name, System.Func<PixelCanvas> draw, bool hanging) in LabArt.Sprites())
            {
                Phase02Assets.WriteSprite(name, draw(), hanging ? top : feet);
            }

            // O que sobrou largado (LabClutterArt) e os marcadores do que se usa.
            foreach ((string name, PixelCanvas canvas) in LabClutterArt.All()) Phase02Assets.WriteSprite(name, canvas, feet);
            Phase01Generator.EnsureCueArt();

            for (int lit = 0; lit <= 3; lit++) Phase02Assets.WriteSprite(LabArt.GreatAlembicName(lit), LabArt.GreatAlembic(lit), feet);
            for (int i = 0; i < 3; i++) Phase02Assets.WriteSprite(LabArt.ReagentName(i), LootArt.Reagent(i), middle, false, LootArt.PixelsPerUnit);
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
            Phase02Assets.WriteSheet(District4NpcArt.SheetName, District4NpcArt.Frames(),
                new Vector2(0.5f, 0f), District4NpcArt.PixelsPerUnit);

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
            Phase02Assets.WriteSheet(AlchemistWreckSheet, AlchemistWreckArt.Frames(),
                new Vector2(AlchemistWreckArt.PivotX, AlchemistWreckArt.PivotY), AlchemistWreckArt.PixelsPerUnit);

            return Phase01Assets.SliceGrid(MarblePath, MarblePrefix);
        }

        private static readonly Color HazardLight = new Color(1f, 0.96f, 0.62f, 0.95f);
        private static readonly Color GuideLight = new Color(0.62f, 0.90f, 1f, 0.85f);
    }
}
