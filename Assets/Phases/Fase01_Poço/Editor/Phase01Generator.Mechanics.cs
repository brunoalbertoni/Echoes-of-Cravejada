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
        // Mecanicas: espinhos, inimigos, alavanca, portao e bancada
        // ------------------------------------------------------------------

        private static void BuildMechanics(int groundLayer)
        {
            var root = new GameObject("Mechanics");

            BuildSpikes(root.transform);
            BuildCrawlers(root.transform);
            BuildLeverAndGate(root.transform, groundLayer);
            BuildWorkbenches(root.transform);
            BuildNpcs(root.transform);
        }

        private static void BuildNpcs(Transform parent)
        {
            var root = new GameObject("Npcs");
            root.transform.SetParent(parent, false);

            // Quem so aparece depois do boss (a Pequena) e montado junto com
            // ele, pra entrar na lista do que a purificacao acende.
            foreach (Phase01Layout.NpcSpot spot in Phase01Layout.Npcs)
            {
                if (!spot.AfterBoss) CreateNpc(root.transform, spot);
            }

            // O que se le pelo mapa. A placa do Coletor fica de fora pelo
            // mesmo motivo.
            var loreRoot = new GameObject("Lore");
            loreRoot.transform.SetParent(parent, false);

            foreach (Phase01Layout.LoreSpot spot in Phase01Layout.Lore)
            {
                if (!spot.AfterBoss) CreateLore(loreRoot.transform, spot);
            }

            // As placas de tutorial. A da obra abandonada ja vem na cena: quem
            // a esconde ate a purificacao e a nevoa na frente do tunel.
            var tutorialRoot = new GameObject("Tutorials");
            tutorialRoot.transform.SetParent(parent, false);

            foreach (Phase01Layout.TutorialSpot spot in Phase01Layout.Tutorials)
            {
                CreateTutorialSign(tutorialRoot.transform, spot);
            }

            // O que se pega. O motor do Coletor tambem fica de fora.
            var itemRoot = new GameObject("Items");
            itemRoot.transform.SetParent(parent, false);

            foreach (Phase01Layout.ItemSpot spot in Phase01Layout.Items)
            {
                if (!spot.AfterBoss) CreateItem(itemRoot.transform, spot);
            }
        }

        /// <summary>
        /// Um descartado com quem se conversa. O papel diz qual componente: o
        /// Ferrolho monta o motor, o Rebite pede as ferramentas, o resto so fala.
        /// </summary>
        private static GameObject CreateNpc(Transform parent, Phase01Layout.NpcSpot spot)
        {
            var npcObject = new GameObject("Npc_" + spot.Name);
            npcObject.transform.SetParent(parent, false);
            npcObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);
            npcObject.transform.localScale = new Vector3(spot.Scale, spot.Scale, 1f);

            // O corpo de lata de cada um, parado mas vivo: respira, pisca e
            // mexe no que e seu (a lanterna, a lupa, o manometro, o giz).
            Sprite[] idle = Phase02Assets.LoadSheet(Phase02Assets.DiscardedSheet(spot.Variant), DiscardedArt.IdleFrames);

            var renderer = npcObject.AddComponent<SpriteRenderer>();
            renderer.sprite = idle[0];
            renderer.sortingLayerName = Phase01Assets.SortingActors;
            renderer.sortingOrder = -4;

            npcObject.AddComponent<SpriteLoop>().Configurar(idle, 5f, DiscardedArt.BlinkFrame);

            // A area em volta onde da pra conversar. Dividida pela escala, pra
            // a Pequena nao ter uma area menor que os outros.
            var area = npcObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(3f, 2.5f) / spot.Scale;
            area.offset = new Vector2(0f, 1.25f) / spot.Scale;

            Npc npc;
            switch (spot.Role)
            {
                case Phase01Layout.NpcRole.MotorFitter:
                    var fitter = npcObject.AddComponent<FerrolhoNpc>();
                    fitter.ConfigurarMotor(Phase01Layout.MotorId, spot.WaitingLines, spot.InstallLines, spot.DoneLines);
                    npc = fitter;
                    break;

                case Phase01Layout.NpcRole.ToolsQuest:
                    var quest = npcObject.AddComponent<ItemQuestNpc>();
                    quest.ConfigurarMissao(Phase01Layout.MasterToolPrefix, Phase01Layout.MasterToolCount, 1,
                        "Gema lapidada recebida", spot.PurifiedGreeting,
                        new[] { new LineSet(spot.NoToolLines), new LineSet(spot.OneToolLines), new LineSet(spot.TwoToolLines) },
                        spot.CompleteLines, spot.DoneLines);
                    npc = quest;
                    break;

                default:
                    npc = npcObject.AddComponent<Npc>();
                    break;
            }

            npc.Configurar(spot.Name, spot.Lines);
            if (spot.PurifiedLines != null) npc.ConfigurarPurificado(spot.PurifiedLines);

            // Uma luz em cima dele, pra dar pra achar no escuro, e o balao que
            // flutua: da pra conversar.
            Light2D npcGlow = CreatePointLight(npcObject.transform, "Glow", (spot.X + 0.5f, spot.Y + 1f),
                new Color(0.80f, 0.95f, 0.90f), 0.8f, 3.8f);
            AddCue(npcObject, CueArt.Talk, renderer.bounds.max.y, npcGlow);

            return npcObject;
        }

        /// <summary>
        /// Algo que flutua e se pega ao encostar: uma ferramenta do ourives, o
        /// motor do Coletor ou um reforco de chassi.
        /// </summary>
        private static GameObject CreateItem(Transform parent, Phase01Layout.ItemSpot spot)
        {
            var item = new GameObject("Item_" + spot.Id);
            item.transform.SetParent(parent, false);
            item.transform.position = new Vector3(spot.X + 0.5f, spot.Y + 0.8f, 0f);

            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = Phase01Assets.SortingActors;
            renderer.sortingOrder = -1;

            var trigger = item.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            Color glow = new Color(0.95f, 0.90f, 0.75f);

            switch (spot.Kind)
            {
                case Phase01Layout.ItemKind.MasterTool:
                    renderer.sprite = Phase02Assets.LoadSprite(Phase02Assets.MasterToolName(spot.Art));
                    item.AddComponent<KeyItemPickup>().Configurar(spot.Id, spot.Message,
                        Phase01Layout.MasterToolPrefix, Phase01Layout.MasterToolCount);
                    break;

                case Phase01Layout.ItemKind.Motor:
                    renderer.sprite = Phase02Assets.LoadSprite(Phase02Assets.ColetorMotor);
                    item.AddComponent<KeyItemPickup>().Configurar(spot.Id, spot.Message);
                    glow = new Color(1f, 0.75f, 0.5f);
                    break;

                case Phase01Layout.ItemKind.Chassis:
                    // O mesmo reforco escondido dos outros distritos: a placa de peito.
                    renderer.sprite = Phase02Assets.LoadSprite(Phase02Assets.ChassisPlate);
                    item.AddComponent<RewardPickup>().Configurar(RewardPickup.Kind.Chassi, spot.Id);
                    break;
            }

            Light2D itemGlow = CreatePointLight(item.transform, "Glow", (spot.X + 0.5f, spot.Y + 0.8f), glow, 0.9f, 4f);
            AddCue(item, CueArt.Item, renderer.sprite != null ? renderer.bounds.max.y : spot.Y + 1.4f, itemGlow);
            return item;
        }

        /// <summary>Grava os desenhos do LoreArt: o do mapa (pivo no meio da base) e o da leitura.</summary>
        private static void WriteLoreArt()
        {
            LoreArtAssets.Write(LoreArt.Phase01.Values);
        }

        /// <summary>
        /// Uma pagina, um muro riscado ou uma placa: chegar perto e apertar E.
        /// Cada um tem o desenho proprio no mapa e um grande, que aparece na
        /// tela durante a leitura (LoreArt). O que nao tiver usa os desenhos
        /// genericos do District 2, sem o grande.
        /// </summary>
        private static GameObject CreateLore(Transform parent, Phase01Layout.LoreSpot spot)
        {
            var loreObject = new GameObject(spot.Name);
            loreObject.transform.SetParent(parent, false);
            loreObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);

            var area = loreObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(2.6f, 2.5f);
            area.offset = new Vector2(0f, 1.25f);

            // O desenho fica num filho, pra poder subir pra parede sem tirar
            // a area de leitura do chao.
            var artObject = new GameObject("Art");
            artObject.transform.SetParent(loreObject.transform, false);

            var art = artObject.AddComponent<SpriteRenderer>();
            art.sortingLayerName = Phase01Assets.SortingTerrain;

            var readable = loreObject.AddComponent<Npc>();
            readable.ConfigurarLeitura(spot.Title, spot.Lines);
            if (spot.PurifiedLines != null) readable.ConfigurarPurificado(spot.PurifiedLines);

            if (LoreArt.Phase01.TryGetValue(spot.Name, out LoreArt.Piece own))
            {
                LoreArtAssets.Apply(own, artObject, readable, Phase01Assets.SortingTerrain);
            }
            else switch (spot.Kind)
            {
                case Phase01Layout.LoreKind.Page:
                    art.sprite = Phase02Assets.LoadSprite(Phase02Assets.LoosePage);
                    art.sortingOrder = 22;
                    break;

                case Phase01Layout.LoreKind.Graffiti:
                    artObject.transform.localPosition = new Vector3(0f, 2f, 0f);
                    art.sprite = Phase02Assets.LoadSprite(Phase02Assets.ScribbleText);
                    art.sortingOrder = 4;
                    break;

                case Phase01Layout.LoreKind.Plaque:
                    artObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                    art.sprite = Phase02Assets.LoadSprite(Phase02Assets.Plaque);
                    art.sortingOrder = 6;
                    break;
            }

            // Uma luz, pra dar pra achar no escuro, e a folhinha que flutua: da
            // pra ler.
            Light2D loreGlow = CreatePointLight(loreObject.transform, "Glow", (spot.X + 0.5f, spot.Y + 1.2f),
                new Color(0.95f, 0.90f, 0.75f), 0.75f, 3.8f);
            float loreTop = art.sprite != null ? art.bounds.max.y : spot.Y + 2f;
            AddCue(loreObject, CueArt.Read, loreTop, loreGlow);

            return loreObject;
        }

        /// <summary>
        /// Uma placa de tutorial fincada no chao. A area cobre o trecho onde o
        /// movimento comeca a ser preciso; enquanto o Scrap esta nela, o painel
        /// com a mini cena fica aberto no canto da tela.
        /// </summary>
        private static GameObject CreateTutorialSign(Transform parent, Phase01Layout.TutorialSpot spot)
        {
            var signObject = new GameObject(spot.Name);
            signObject.transform.SetParent(parent, false);
            signObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);

            var art = signObject.AddComponent<SpriteRenderer>();
            art.sprite = Phase02Assets.LoadSprite(Phase02Assets.TutorialSignName((int)spot.Kind));
            art.sortingLayerName = Phase01Assets.SortingTerrain;
            art.sortingOrder = 6;

            // A area, em coordenadas do objeto (que esta no meio da celula da placa).
            float left = spot.AreaX0;
            float right = spot.AreaX1 + 1f;
            var area = signObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(right - left, 2.5f);
            area.offset = new Vector2((left + right) * 0.5f - (spot.X + 0.5f), 1.25f);

            ScrapMove move = spot.Kind == Phase01Layout.TutorialKind.Pogo ? ScrapMove.Pogo : ScrapMove.PuloParede;
            signObject.AddComponent<TutorialSign>().Configurar(move, art);

            // Uma luz fraca, pra dar pra achar no escuro.
            CreatePointLight(signObject.transform, "Glow", (spot.X + 0.5f, spot.Y + 1.4f),
                new Color(0.95f, 0.90f, 0.75f), 0.5f, 3f);

            return signObject;
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

            Sprite[] frames = Phase02Assets.LoadSheet(Phase02Assets.CrawlerSheet, CrawlerArt.FrameCount);
            Sprite shard = Phase02Assets.LoadSprite(Phase02Assets.Shard);

            foreach (Phase01Layout.Patrol patrol in Phase01Layout.Crawlers)
            {
                var crawler = new GameObject(patrol.Name);
                crawler.transform.SetParent(root.transform, false);
                crawler.transform.position = new Vector3((patrol.X0 + patrol.X1 + 1) * 0.5f, patrol.Y + 0.5f, 0f);

                var renderer = crawler.AddComponent<SpriteRenderer>();
                renderer.sprite = frames[CrawlerArt.Walk];
                renderer.sortingLayerName = Phase01Assets.SortingActors;
                renderer.sortingOrder = -3;

                // Gatilho, e nao parede: o Scrap atravessa e apanha, em vez de
                // ser empurrado pra dentro do chao.
                var trigger = crawler.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(1f, 0.7f);
                trigger.offset = new Vector2(0f, -0.1f);

                var body = crawler.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;

                crawler.AddComponent<Damageable>().Configurar(5, 3, shard);
                crawler.AddComponent<CrawlerEnemy>().Configurar(patrol.X0 + 0.5f, patrol.X1 + 0.5f, frames);

                // O verde do Oxide vazando pela rachadura e pelo olho: sem isso,
                // no escuro do poco, ele vira so um vulto.
                Vector3 at = crawler.transform.position;
                CreatePointLight(crawler.transform, "Glow", (at.x - 0.2f, at.y + 0.1f),
                    OxideGreen, 0.4f, 1.6f);
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
            gate.Configurar(gateCells.Height);

            // A alavanca, e a area em volta onde o cinzel acerta.
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

            leverObject.AddComponent<RustyLever>().Configurar(gate, leverSprite,
                Phase02Assets.LoadSprite(Phase02Assets.LeverClean));

            // A setinha que flutua: da pra acionar (no golpe do cinzel).
            Light2D leverGlow = CreatePointLight(leverObject.transform, "Glow", (leverTile.X + 0.5f, leverTile.Y + 1f),
                new Color(1f, 0.85f, 0.6f), 0.6f, 3f);
            AddCue(leverObject, CueArt.Use, leverSprite.sprite != null ? leverSprite.bounds.max.y : leverTile.Y + 1.5f, leverGlow);
        }

        /// <summary>Grava os marcadores do CueArt, com o pivo no meio da base.</summary>
        internal static void EnsureCueArt()
        {
            foreach ((string name, PixelCanvas canvas) in CueArt.All())
            {
                Phase02Assets.WriteSprite(name, canvas, new Vector2(0.5f, 0f), false, CueArt.PixelsPerUnit);
            }
        }

        /// <summary>
        /// O destaque de algo que da pra usar: o marcador flutuando um pouco
        /// acima do topo (em mundo) e o InteractCue, que faz ele subir, descer
        /// e pulsar junto com a luz.
        /// </summary>
        internal static void AddCue(GameObject target, string cue, float worldTop, Light2D glow)
        {
            Sprite sprite = Phase02Assets.LoadSprite(cue);
            if (sprite == null) return;

            var markerObject = new GameObject("Cue");
            markerObject.transform.SetParent(target.transform, false);
            markerObject.transform.position = new Vector3(target.transform.position.x, worldTop + 0.35f, 0f);

            // Do mesmo tamanho pra todo mundo, mesmo em quem e menor (a Pequena).
            Vector3 scale = target.transform.lossyScale;
            markerObject.transform.localScale = new Vector3(1f / Mathf.Max(0.01f, scale.x), 1f / Mathf.Max(0.01f, scale.y), 1f);

            var renderer = markerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = Phase01Assets.SortingForeground;
            renderer.sortingOrder = 20;

            target.AddComponent<InteractCue>().Configurar(renderer, glow);
        }

        private static void BuildWorkbenches(Transform parent)
        {
            (int X, int Y)[] tiles = Phase01Layout.Workbenches;

            for (int i = 0; i < tiles.Length; i++)
            {
                BuildWorkbench(parent, i == 0 ? "Workbench" : $"Workbench_{i + 1}", tiles[i]);
            }
        }

        private static void BuildWorkbench(Transform parent, string name, (int X, int Y) tile)
        {
            var benchObject = new GameObject(name);
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

            benchObject.AddComponent<Workbench>().Configurar(rest.transform);
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
