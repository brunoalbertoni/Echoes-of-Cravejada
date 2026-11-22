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
            exitGate.Configurar(gateCells.Height + 1f);

            // --- A passagem pro District 2: vapor e saida, desligados ate a purificacao ---
            var passage = new GameObject("District2Passage");
            passage.transform.SetParent(root.transform, false);

            LevelMap.Lift vent = Phase01Layout.District2Vent;
            var ventCells = new RectInt(vent.X0, vent.Bottom, vent.X1 - vent.X0 + 1, vent.Top - vent.Bottom + 1);

            SceneKit.CreateSteamVent(passage.transform, "Lift_ToDistrict2", ventCells, SteamVent.Mode.Erguer2, 1,
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

                    // Os postes de lampiao da cenografia: acendem com a onda.
                    else if (light.name.StartsWith(Phase01DecorBuilder.LampLightPrefix)) purified.Add(light);
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

            // Os quadros dele: inteiro e rachado (da metade da vida pra baixo).
            Sprite[] bossFrames = Phase02Assets.LoadSheet(Phase02Assets.ColetorSheet, ColetorArt.FrameCount);
            Sprite[] bossCracked = Phase02Assets.LoadSheet(Phase02Assets.ColetorCrackedSheet, ColetorArt.FrameCount);

            var bossSprite = bossObject.AddComponent<SpriteRenderer>();
            bossSprite.sprite = bossFrames[ColetorArt.Dormant];
            bossSprite.flipX = true;
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

            // A nuvem de Oxide: a baforada da metade da luta (sem dano) e, na
            // queda, os jatos e o estouro dele.
            ParticleSystem cloud = CreateOxideCloud(bossObject.transform, steamMaterial);

            // --- O que muda quando ele cai ---
            // So aparecem com a purificacao: a passagem pro District 2, o vapor
            // que leva da arena de volta a Antessala, o que se le no lugar dele
            // (a placa do chassi), o motor dele e quem se escondia na nevoa.
            var appear = new List<GameObject> { passage, BuildArenaReturn(root.transform, steamMaterial) };

            foreach (Phase01Layout.LoreSpot spot in Phase01Layout.Lore)
            {
                if (spot.AfterBoss) appear.Add(CreateLore(root.transform, spot));
            }

            foreach (Phase01Layout.ItemSpot spot in Phase01Layout.Items)
            {
                if (spot.AfterBoss) appear.Add(CreateItem(root.transform, spot));
            }

            foreach (Phase01Layout.NpcSpot spot in Phase01Layout.Npcs)
            {
                if (spot.AfterBoss) appear.Add(CreateNpc(root.transform, spot));
            }

            // E somem: os corrompidos e a nevoa que tampa a obra abandonada.
            var vanish = new List<GameObject> { BuildTrialVeil(root.transform, groundLayer) };
            if (enemies != null) vanish.Add(enemies);

            // As pocas e os respiros de Oxide da cenografia.
            GameObject oxideDecor = GameObject.Find(Phase01DecorBuilder.OxideRoot);
            if (oxideDecor != null) vanish.Add(oxideDecor);

            BuildArenaSeal(root.transform, groundLayer);

            var purifierObject = new GameObject("AreaPurifier");
            purifierObject.transform.SetParent(root.transform, false);

            var purifier = purifierObject.AddComponent<AreaPurifier>();
            purifier.Configurar(global, corrupted.ToArray(), purified.ToArray(), fog, tilemaps.ToArray(),
                cameraObject != null ? cameraObject.GetComponent<Camera>() : null,
                appear.ToArray(),
                vanish.ToArray(),
                new[] { exitGate });

            // A cutscene desenhada da troca de visual. Fica num objeto proprio:
            // o boss se destroi no fim da queda. Desligada por enquanto (ver
            // UsePurificationCartoon): sem ela, o Scrap so brilha e troca.
            PurificationCartoon cartoon = null;
            if (UsePurificationCartoon)
            {
                var cartoonObject = new GameObject("PurificationCartoon");
                cartoonObject.transform.SetParent(bossObject.transform.parent, false);
                cartoon = cartoonObject.AddComponent<PurificationCartoon>();
                cartoon.Configurar(CartoonAssets.LoadShots());
            }

            // Ele anda pela arena inteira, e acorda quando o Scrap pisa em
            // qualquer lugar dela.
            var collector = bossObject.AddComponent<RustyScrapper>();
            collector.Configurar(
                Phase01Layout.BossRange.Min, Phase01Layout.BossRange.Max, spawn.Y,
                new Rect(arena.X, arena.Y, arena.Width, 10f),
                Phase02Assets.LoadSprite(Phase02Assets.ScrapChunk),
                Phase02Assets.LoadSprite(Phase02Assets.ShockWave),
                Phase02Assets.LoadSprite(Phase02Assets.Shard),
                cloud, exitGate, purifier, cartoon, bossFrames, bossCracked);

            // A carcaca dele: aparece onde ele cair (ou, voltando, onde ele dormia).
            collector.ConfigurarDestrocos(WreckAssets.Build(root.transform, "ColetorWreck",
                Phase02Assets.LoadSheet(Phase02Assets.ColetorWreckSheet, ColetorWreckArt.FrameCount),
                bossObject.transform.position));
        }

        /// <summary>
        /// A coluna de vapor que sobe do fosso da arena ate a Antessala. Volta
        /// desligada: e a purificacao que a acende.
        /// </summary>
        private static GameObject BuildArenaReturn(Transform parent, Material steamMaterial)
        {
            var root = new GameObject("ArenaReturn");
            root.transform.SetParent(parent, false);

            LevelMap.Lift vent = Phase01Layout.ArenaReturnVent;
            var cells = new RectInt(vent.X0, vent.Bottom, vent.X1 - vent.X0 + 1, vent.Top - vent.Bottom + 1);

            SceneKit.CreateSteamVent(root.transform, "Lift_ToAntechamber", cells, SteamVent.Mode.Erguer2, 1,
                0f, 0f, steamMaterial);

            var nozzle = new GameObject("Nozzle");
            nozzle.transform.SetParent(root.transform, false);
            nozzle.transform.position = new Vector3(vent.X0, vent.Bottom - 1, 0f);

            var nozzleSprite = nozzle.AddComponent<SpriteRenderer>();
            nozzleSprite.sprite = Phase02Assets.LoadSprite(Phase02Assets.NozzleName(cells.width));
            nozzleSprite.sortingLayerName = Phase01Assets.SortingTerrain;
            nozzleSprite.sortingOrder = 21;

            return root;
        }

        /// <summary>
        /// A nevoa densa que tampa o tunel da obra abandonada: uma parede
        /// invisivel, com o verde por cima (o banco de nevoa "Fog_Veil" do
        /// layout) e uma luz. Some com a purificacao.
        /// </summary>
        private static GameObject BuildTrialVeil(Transform parent, int groundLayer)
        {
            (int X, int Y, int Width, int Height) cells = Phase01Layout.TrialVeil;

            var veil = new GameObject("OxideVeil_Trial") { layer = groundLayer };
            veil.transform.SetParent(parent, false);
            veil.transform.position = new Vector3(cells.X, cells.Y, 0f);

            var wall = veil.AddComponent<BoxCollider2D>();
            wall.size = new Vector2(cells.Width, cells.Height);
            wall.offset = new Vector2(cells.Width * 0.5f, cells.Height * 0.5f);

            veil.AddComponent<OxideVeil>();

            CreatePointLight(veil.transform, "Glow",
                (cells.X + cells.Width * 0.5f, cells.Y + cells.Height * 0.5f), OxideGreen, 0.9f, 4.5f);

            return veil;
        }

        /// <summary>
        /// A grade na boca do poco que desce na arena: aberta, ela some; fecha
        /// enquanto o Coletor luta, pra nao dar pra fugir pra Antessala.
        /// </summary>
        private static void BuildArenaSeal(Transform parent, int groundLayer)
        {
            (int X, int Y, int Width) cells = Phase01Layout.ArenaSeal;
            CreateArenaSeal(parent, groundLayer, new RectInt(cells.X, cells.Y, cells.Width, 1));
        }

        /// <summary>
        /// Plataformas finas de verdade: o desenho delas sai do chao (que tem
        /// colisao de celula inteira) e vai pro tilemap de decoracao (que nao
        /// tem), e a colisao vira uma tabua fina no topo. Uma plataforma a um
        /// tile do chao deixa o Scrap (1,05 de altura) passar por baixo e
        /// continua sendo degrau por cima.
        ///
        /// Chamar depois de pintar o chao e antes de gerar o colisor composto.
        /// </summary>
        internal static void CreateThinSteps(Tilemap ground, Tilemap art, IEnumerable<(int X0, int X1, int Y)> steps)
        {
            const float board = 0.3f;

            foreach ((int x0, int x1, int y) in steps)
            {
                for (int x = x0; x <= x1; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    TileBase tile = ground.GetTile(cell);
                    if (tile == null) continue;

                    art.SetTile(cell, tile);
                    ground.SetTile(cell, null);
                }

                var step = new GameObject($"ThinStep_{x0}_{y}") { layer = ground.gameObject.layer };
                step.transform.SetParent(ground.transform, false);
                step.transform.position = new Vector3(x0, y, 0f);

                int width = x1 - x0 + 1;
                var collider = step.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(width, board);
                collider.offset = new Vector2(width * 0.5f, 1f - board * 0.5f);
            }
        }

        /// <summary>
        /// Uma grade de arena de chefe, deitada ou em pe: fecha as celulas
        /// enquanto ele luta (ver ArenaSeal). As fases a usam pra trancar a arena.
        /// </summary>
        internal static ArenaSeal CreateArenaSeal(Transform parent, int groundLayer, RectInt cells)
        {
            var seal = new GameObject("Gate_ArenaEntry") { layer = groundLayer };
            seal.transform.SetParent(parent, false);
            seal.transform.position = new Vector3(cells.x, cells.y, 0f);

            var bars = seal.AddComponent<SpriteRenderer>();
            bars.sprite = Phase02Assets.LoadSprite(Phase02Assets.GateBars);
            bars.drawMode = SpriteDrawMode.Tiled;
            bars.size = new Vector2(cells.width, cells.height);
            bars.sortingLayerName = Phase01Assets.SortingTerrain;
            bars.sortingOrder = 15;

            var wall = seal.AddComponent<BoxCollider2D>();
            wall.size = new Vector2(cells.width, cells.height);
            wall.offset = new Vector2(cells.width * 0.5f, cells.height * 0.5f);

            return seal.AddComponent<ArenaSeal>();
        }

        /// <summary>
        /// A nuvem de Oxide de um chefe: um filho dele, parado, que so solta
        /// verde quando alguem manda (a baforada no meio da luta e a explosao
        /// da queda, ver BossFall).
        /// </summary>
        internal static ParticleSystem CreateOxideCloud(Transform boss, Material material)
        {
            var cloudObject = new GameObject("OxideCloud");
            cloudObject.transform.SetParent(boss, false);
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
            main.maxParticles = 320;

            ParticleSystem.EmissionModule emission = cloud.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

            ParticleSystem.ShapeModule shape = cloud.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;

            ParticleSystem.ColorOverLifetimeModule fade = cloud.colorOverLifetime;
            fade.enabled = true;
            fade.color = new ParticleSystem.MinMaxGradient(SceneKit.FadeInOut(0.1f, 0.5f));

            var renderer = cloudObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = Phase01Assets.SortingForeground;
            renderer.sortingOrder = 8;
            if (material != null) renderer.sharedMaterial = material;

            cloud.Stop();
            return cloud;
        }
    }
}
