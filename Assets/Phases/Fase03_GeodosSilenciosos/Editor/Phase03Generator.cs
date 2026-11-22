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
    /// Monta a cena da Phase 3 do zero, a partir do desenho em Phase03Layout.
    ///
    /// Rodar de novo refaz a cena inteira e descarta o que voce tiver ajustado
    /// nela na mao — entao ajuste o layout no codigo enquanto estiver iterando.
    ///
    /// Menu:      Echoes > Gerar Phase 3 (Silent Geodes)
    ///            Echoes > Validar layout da Phase 3
    /// Batchmode: -executeMethod EchoesOfCravejada.EditorTools.Phase03Generator.GenerateFromCommandLine
    /// </summary>
    public static partial class Phase03Generator
    {
        public const string ScenePath = "Assets/Phases/Fase03_GeodosSilenciosos/Scene/" + Phase03Layout.SceneName + ".unity";
        private const string GroundLayerName = "Ground";

        // Os nomes dos sprites desta fase, na mesma pasta dos de latao.
        private const string Resonant = "crystal_resonant";
        private const string Vein = "crystal_vein";
        private const string FragileTile = "crystal_fragile";
        private const string Glyph = "crystal_glyph";
        private const string ShardShot = "shard_shot";
        private const string GeodeDrill = "geode_drill";
        private const string Sapphire = "sapphire_raw";
        private const string Barrier = "crystal_barrier";
        private const string Rubble = "elevator_rubble";
        private const string ButtonUp = "statue_button_up";
        private const string ButtonDown = "statue_button_down";
        private const string SilverPlaque = "silver_plaque";
        private const string AncientKey = "ancient_key";
        private const string DrillWreckSheet = "d3_drill_wreck_sheet";
        private const string ResonantPrism = "resonant_prism";
        private const string ResonantBase = "resonant_base";
        private const string CaveGate = "cave_gate";
        private const string ForemanSheet = "foreman_sheet";
        private const string MiteSheet = "d3_mite_sheet";
        private const string HunterSheet = "d3_hunter_sheet";
        private const string SentinelSheet = "d3_sentinel_sheet";
        private const string KeyLock = "key_lock";

        /// <summary>
        /// A vida dos mites: um golpe so, com ou sem rubi. E eles nao soltam
        /// fragmento nenhum.
        /// </summary>
        private const int MiteHealth = 1;

        /// <summary>
        /// Quanto tempo um pilar da arena fica aceso depois do golpe. Curto de
        /// proposito: o primeiro toque chama a broca, e e preciso tocar de novo
        /// com ela chegando pra ela cravar.
        /// </summary>
        private const float DrillPillarLitTime = 1.2f;

        private static string FrameName(string prefix, int frame) => $"{prefix}_{frame}";

        private static Sprite[] LoadFrames(string prefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++) frames[i] = Phase02Assets.LoadSprite(FrameName(prefix, i));
            return frames;
        }

        private static string ClusterName(int size, int hue) => $"crystal_cluster_{size}_{hue}";
        private static string StatueName(int kind) => "statue_" + kind;
        private static string StatueGlowName(int kind) => "statue_glow_" + kind;
        private static string CrackName(int frame) => "statue_crack_" + frame;
        private static string PebblesName(int seed) => "rubble_pebbles_" + seed;

        /// <summary>A luz dos olhos das estatuas: branca e fria, a mesma pras quatro.</summary>
        private static readonly Color StatueGlowColor = new Color(0.85f, 0.92f, 1f);
        private static string EngravingName(int scene) => "engraving_" + scene;

        /// <summary>O fundo da camera: quase preto, puxado pro violeta.</summary>
        private static readonly Color Void = new Color(0.020f, 0.015f, 0.035f);

        private static readonly Color RockTint = new Color(0.78f, 0.72f, 1f);
        private static readonly Color BackTint = new Color(0.30f, 0.28f, 0.45f);
        private static readonly Color SpikeTint = new Color(0.80f, 0.62f, 1f);
        private static readonly Color OxideGreen = new Color(0.36f, 0.95f, 0.60f);
        private static readonly Color LampGlow = new Color(1f, 0.82f, 0.55f);

        /// <summary>A luz de cada familia de cristal: ametista, quartzo leitoso e cristal fume.</summary>
        private static readonly Color[] HueGlow =
        {
            new Color(0.74f, 0.52f, 1f),
            new Color(0.80f, 0.88f, 1f),
            new Color(0.70f, 0.66f, 0.85f),
        };

        /// <summary>A cor de cada nota. A 0 e a dos cristais que sao so isca.</summary>
        private static readonly Color[] NoteColors =
        {
            new Color(0.90f, 0.88f, 1f),
            new Color(1f, 0.40f, 0.50f),
            new Color(1f, 0.80f, 0.30f),
            new Color(0.40f, 0.90f, 1f),
            new Color(0.62f, 0.50f, 1f),
        };

        private static Material _unlit;
        private static int _groundLayer;

        // O que uma parte da montagem deixa pra outra.
        private static readonly List<Transform> SlabRoots = new List<Transform>();
        private static readonly Dictionary<string, List<ResonantCrystal>> DoorCrystals =
            new Dictionary<string, List<ResonantCrystal>>();
        private static readonly List<ResonantCrystal> Pillars = new List<ResonantCrystal>();
        private static readonly List<Light2D> OxideLights = new List<Light2D>();
        private static readonly Dictionary<string, Gate> BuiltGates = new Dictionary<string, Gate>();

        public static void Generate()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[Phase 3] Geracao cancelada.");
                return;
            }

            Build();
        }

        /// <summary>Confere que o mapa e navegavel sem precisar entrar no jogo.</summary>
        public static void ValidateLayout()
        {
            List<string> problems = Phase03Layout.Validate();

            if (problems.Count == 0)
            {
                Debug.Log("[Phase 3] Layout OK: da pra tocar a Porta Harmonica e ir da Fronteira ate a Camara de Perfuracao.");
            }
            else
            {
                foreach (string problem in problems) Debug.LogWarning($"[Phase 3] {problem}");
                Debug.LogError($"[Phase 3] {problems.Count} problema(s) no layout. Veja os avisos acima.");
            }

            // A saida gradeada aparece aqui, e esta certo: ela so abre com o boss.
            foreach (string floor in Phase03Layout.UnreachableFloors())
            {
                Debug.Log($"[Phase 3] Chao sem acesso (pode ser so enfeite): {floor}");
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

            foreach (string problem in Phase03Layout.Validate())
            {
                Debug.LogWarning($"[Phase 3] {problem}");
            }

            if (!Phase01Assets.SliceTileset() || !Phase01Assets.SliceBackgroundTileset() || !Phase01Assets.SliceObjects())
            {
                Debug.LogError("[Phase 3] Abortado: nao consegui fatiar as folhas do pack de caverna.");
                return;
            }

            // A fronteira ainda e de latao, e a bancada, Crisol e as paginas vem de la.
            if (!Phase02Assets.EnsureArt(new[] { 4 }) || !Phase02Assets.EnsureMechanicsArt())
            {
                Debug.LogError("[Phase 3] Abortado: nao consegui preparar a arte de latao.");
                return;
            }

            EnsureArt();

            Dictionary<string, TileBase> tiles = Phase02Assets.BuildTiles();
            if (tiles.Count == 0)
            {
                Debug.LogError("[Phase 3] Abortado: nenhum tile foi criado.");
                return;
            }

            Sprite scrapSprite = Phase01Assets.CreateScrapPlaceholder();
            Sprite polishedSprite = Phase02Assets.LoadSprite(Phase02Assets.ScrapPolished);
            Material fogMaterial = Phase01Assets.GetOrCreateFogMaterial();
            _unlit = Phase01Assets.GetOrCreateUnlitMaterial();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SlabRoots.Clear();
            DoorCrystals.Clear();
            Pillars.Clear();
            OxideLights.Clear();
            BuiltGates.Clear();

            BuildTilemaps(tiles);
            BuildLighting(fogMaterial);
            BuildMarkers();

            var travel = new GameObject("Travel");
            SceneKit.CreateSpawnPoint(travel.transform, Phase03Layout.FromDistrict2SpawnId, Phase03Layout.SpawnTile);

            // A saida pro District 4 e o proprio elevador da chegada (BuildElevator),
            // depois que os escombros do poco caem.

            Transform scrap = Phase01Generator.BuildScrap(scrapSprite, polishedSprite, _groundLayer, Phase03Layout.SpawnTile);
            Phase01Generator.BuildCamera(scrap, Phase03Layout.Width, Phase03Layout.Height, Void);

            // O mapa desta fase conta com o Steam Dash e o Pressure Jump, entao
            // aqui o Scrap ja chega com os dois — mesmo dando Play direto na cena.
            scrap.GetComponent<ScrapDash>().DefinirLiberadoDesdeInicio(true);

            // A ferrugem saiu na purificacao do District 1: aqui ele ja chega polido.
            scrap.GetComponent<ScrapAppearance>().DefinirPolidoDesdeInicio(true);
            scrap.gameObject.AddComponent<AbilityGrant>().Configurar(true, true);
            scrap.gameObject.AddComponent<ScrapNoise>();

            // Os olhos dele iluminam bem menos aqui: quem mostra o caminho e o som.
            Transform eyes = scrap.Find("EyeGlow");
            if (eyes != null)
            {
                var eyeLight = eyes.GetComponent<Light2D>();
                eyeLight.intensity = 0.5f;

                var serialized = new SerializedObject(eyeLight);
                serialized.FindProperty("m_PointLightOuterRadius").floatValue = 3f;
                serialized.FindProperty("m_PointLightInnerRadius").floatValue = 0.6f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            var root = new GameObject("Mechanics").transform;

            BuildSpikes(Child(root, "Spikes"));

            // Os chaos da arena vem antes: cristais e pilares em cima deles
            // sao filhos deles, pra sumirem junto quando desabam.
            BuildSlabs(Child(root, "CollapsingFloors"));
            BuildEchoCrystals(Child(root, "EchoCrystals"));
            BuildFragiles(Child(root, "FragilePlatforms"));
            BuildResonants(Child(root, "ResonantCrystals"));
            BuildDoors(Child(root, "Gates"));
            BuildBarriers(Child(root, "CrystalBarriers"));
            BuildElevator(Child(root, "Elevator"));
            BuildRewards(Child(root, "Rewards"));
            BuildDecor(Child(root, "Decor"));

            Transform enemies = Child(root, "Enemies");
            BuildFoes(enemies);
            BuildElite(enemies);
            BuildStatues(Child(root, "StatueHall"), enemies);

            BuildWorkbenches(Child(root, "Workbenches"));
            GameObject crisol = BuildCrisol(root);
            BuildLore(Child(root, "Lore"));

            // Por ultimo: a purificacao precisa de tudo o que ela mexe, e o
            // boss precisa dela.
            AreaPurifier purifier = BuildPurification(root, enemies.gameObject, crisol);
            BuildBoss(root, purifier);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneKit.AddSceneToBuildSettings(ScenePath);

            Debug.Log($"[Phase 3] Cena gerada: {ScenePath}. " +
                      $"Mapa {Phase03Layout.Width}x{Phase03Layout.Height}, {Phase03Layout.Rooms.Count} salas, " +
                      $"{Phase03Layout.Echoes.Count} cristais de eco, {Phase03Layout.Resonants.Count} ressonantes, " +
                      $"{Phase03Layout.Foes.Count} inimigos, o Capataz e The Geode Drill.");
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

        private static void EnsureArt()
        {
            var feet = new Vector2(0.5f, 0f);
            var middle = new Vector2(0.5f, 0.5f);

            for (int size = 0; size < CrystalArt.SizeCount; size++)
            {
                for (int hue = 0; hue < CrystalArt.HueCount; hue++)
                {
                    Phase02Assets.WriteSprite(ClusterName(size, hue), CrystalArt.Cluster(size, hue), feet);
                }
            }

            Phase02Assets.WriteSprite(Resonant, CrystalArt.Resonant(), feet);
            Phase02Assets.WriteSprite(Vein, CrystalArt.Vein(), feet);
            Phase02Assets.WriteSprite(FragileTile, CrystalArt.FragileTile(), Vector2.zero, true);
            Phase02Assets.WriteSprite(Glyph, CrystalArt.Glyph(), middle);

            for (int scene = 0; scene < CrystalArt.EngravingCount; scene++)
            {
                Phase02Assets.WriteSprite(EngravingName(scene), CrystalArt.Engraving(scene), middle);
            }

            Phase02Assets.WriteSprite(ShardShot, CrystalArt.ShardShot(), middle);
            Phase02Assets.WriteSprite(GeodeDrill, CrystalArt.GeodeDrill(), feet);
            Phase02Assets.WriteSprite(Sapphire, LootArt.SapphireRaw(), middle, false, LootArt.PixelsPerUnit);
            Phase02Assets.WriteSprite(Barrier, CrystalArt.Barrier(), Vector2.zero, true);

            (int _, int _, int rubbleWidth, int rubbleHeight) = Phase03Layout.ElevatorRubble;
            Phase02Assets.WriteSprite(Rubble, RubbleArt.ElevatorRubble(rubbleWidth, rubbleHeight), Vector2.zero);
            for (int seed = 0; seed < 2; seed++) Phase02Assets.WriteSprite(PebblesName(seed), RubbleArt.Pebbles(seed), feet);

            for (int kind = 0; kind < StatueArt.KindCount; kind++)
            {
                Phase02Assets.WriteSprite(StatueName(kind), StatueArt.Statue(kind), feet);
                Phase02Assets.WriteSprite(StatueGlowName(kind), StatueArt.StatueGlow(kind), feet);
            }

            Phase02Assets.WriteSprite(ButtonUp, StatueArt.Button(false), Vector2.zero);
            Phase02Assets.WriteSprite(ButtonDown, StatueArt.Button(true), Vector2.zero);
            for (int frame = 0; frame < 3; frame++) Phase02Assets.WriteSprite(CrackName(frame), StatueArt.Crack(frame), feet);
            Phase02Assets.WriteSprite(SilverPlaque, StatueArt.SilverPlaque(), middle);

            // Os bichos e o Capataz, no detalhe das outras fases: uma folha
            // cada, 32 px por tile, com todas as poses e a morte.
            Phase02Assets.WriteSheet(MiteSheet, MiteArt.Frames(), new Vector2(0.5f, MiteArt.PivotY), MiteArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(HunterSheet, HunterArt.Frames(), new Vector2(HunterArt.PivotX, HunterArt.PivotY), HunterArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(SentinelSheet, SentinelArt.Frames(), new Vector2(0.5f, SentinelArt.PivotY), SentinelArt.PixelsPerUnit);
            Phase02Assets.WriteSheet(ForemanSheet, CaveMinerArt.Frames(),
                new Vector2(CaveMinerArt.PivotX, CaveMinerArt.PivotY), CaveMinerArt.PixelsPerUnit);
            // Os mineiros que voltam depois da broca, no padrao dos NPCs do District 2.
            for (int variant = 0; variant < District3NpcArt.Count; variant++)
            {
                Phase02Assets.WriteSheet(District3NpcArt.SheetName(variant), District3NpcArt.Frames(variant),
                    new Vector2(0.5f, 0f), District3NpcArt.PixelsPerUnit);
            }

            Phase02Assets.WriteSprite(AncientKey, LootArt.AncientKey(), middle, false, LootArt.PixelsPerUnit);
            Phase02Assets.WriteSprite(KeyLock, CaveArt.Lock(), middle);

            // A broca animada, o ressonante com base, o portao de caverna, as
            // placas, o bilhete, o capacete e a mina.
            for (int frame = 0; frame < GeodeArt.DrillFrames; frame++)
            {
                Phase02Assets.WriteSprite(FrameName("drill", frame), GeodeArt.Drill(frame), feet);
            }

            // A carcaca dela, no mesmo 16 px por tile da broca.
            Phase02Assets.WriteSheet(DrillWreckSheet, DrillWreckArt.Frames(), feet, Phase01Assets.PixelsPerUnit);

            Phase02Assets.WriteSprite(ResonantPrism, GeodeArt.Resonant(), feet);
            Phase02Assets.WriteSprite(ResonantBase, GeodeArt.ResonantBase(), feet);
            Phase02Assets.WriteSprite(CaveGate, GeodeArt.CaveGate(), Vector2.zero, true);
            Phase02Assets.WriteSprite("roster_board", GeodeArt.RosterBoard(), feet);
            Phase02Assets.WriteSprite("crown_sign", GeodeArt.CrownSign(), feet);
            Phase02Assets.WriteSprite("miner_note", GeodeArt.MinerNote(), middle);
            Phase02Assets.WriteSprite("miner_helmet", GeodeArt.Helmet(), feet);
            Phase02Assets.WriteSprite("mine_post", GeodeArt.MinePost(), Vector2.zero, true);
            Phase02Assets.WriteSprite("mine_beam", GeodeArt.MineBeam(), Vector2.zero, true);
            Phase02Assets.WriteSprite("mine_rail", GeodeArt.MineRail(), Vector2.zero, true);
            Phase02Assets.WriteSprite("mine_cart", GeodeArt.MineCart(), Vector2.zero);

            // O que os mineiros largaram (no chao, pivo no canto de baixo) e os
            // marcadores que flutuam em cima do que da pra usar.
            foreach ((string name, PixelCanvas canvas) in MineDressArt.All())
            {
                Phase02Assets.WriteSprite(name, canvas, Vector2.zero);
            }

            Phase01Generator.EnsureCueArt();

            // O que se le: o desenho do mapa e o grande, da leitura (LoreArt).
            LoreArtAssets.Write(LoreArt.Phase03.Values);

            // O templo do Salao das Estatuas: todos emendaveis (modo Tiled).
            Phase02Assets.WriteSprite("temple_bricks", StatueArt.BrickWall(), Vector2.zero, true);
            Phase02Assets.WriteSprite("temple_floor", StatueArt.TempleFloor(), Vector2.zero, true);
            Phase02Assets.WriteSprite("temple_lintel", StatueArt.TempleLintel(), Vector2.zero, true);
            Phase02Assets.WriteSprite("temple_pillar", StatueArt.Pillar(), Vector2.zero, true);
        }

        /// <summary>
        /// A vida do minerador que corre atras do barulho. Aqui o golpe do
        /// Scrap ja tira 2 (cinzel + rubi) ou 3 (com o engaste de ouro da
        /// loja): com 8, ele cai em 4 ou 3 golpes, e nao em 2.
        /// </summary>
        private const int HunterHealth = 8;
    }
}
