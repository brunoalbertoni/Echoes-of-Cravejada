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
    public static partial class Phase03Generator
    {
        // ------------------------------------------------------------------
        // Inimigos
        // ------------------------------------------------------------------


        private static void BuildFoes(Transform parent)
        {
            Sprite shard = Phase02Assets.LoadSprite(Phase02Assets.Shard);
            Sprite[] mite = Phase02Assets.LoadSheet(MiteSheet, MiteArt.FrameCount);
            Sprite[] hunter = Phase02Assets.LoadSheet(HunterSheet, HunterArt.FrameCount);
            Sprite[] sentinel = Phase02Assets.LoadSheet(SentinelSheet, SentinelArt.FrameCount);

            foreach (Phase03Layout.FoeSpot spot in Phase03Layout.Foes)
            {
                switch (spot.Kind)
                {
                    case Phase03Layout.FoeKind.EchoHunter:
                    {
                        GameObject foe = Foe(parent, spot.Name, new Vector3(spot.HomeX + 0.5f, spot.Y, 0f), hunter, EchoHunter.QuadroOcioso,
                            new Vector2(0.9f, 1.3f), new Vector2(0f, 0.7f));
                        foe.AddComponent<Damageable>().Configurar(HunterHealth, 5, shard);
                        foe.AddComponent<EchoHunter>().Configurar(spot.X0 + 0.5f, spot.X1 + 0.5f, hunter);
                        break;
                    }

                    case Phase03Layout.FoeKind.CrystalMite:
                        BuildMite(parent, spot.Name, new Vector3(spot.HomeX + 0.5f, spot.Y, 0f), spot.X0 + 0.5f, spot.X1 + 0.5f, mite, shard);
                        break;

                    case Phase03Layout.FoeKind.ShardSentinel:
                    {
                        GameObject foe = Foe(parent, spot.Name, new Vector3(spot.HomeX + 0.5f, spot.Y, 0f), sentinel, ShardSentinel.QuadroOcioso,
                            new Vector2(1.1f, 1.3f), new Vector2(0f, 0.7f));
                        foe.AddComponent<Damageable>().Configurar(3, 4, shard);
                        foe.AddComponent<ShardSentinel>().Configurar(Phase02Assets.LoadSprite(ShardShot), sentinel, SentinelArt.CoreY);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// O corpo de um bicho: o primeiro quadro da folha, o gatilho (o Scrap
        /// atravessa e apanha) e o Rigidbody parado que o gatilho precisa.
        /// </summary>
        private static GameObject Foe(Transform parent, string name, Vector3 position, Sprite[] frames, int firstFrame,
            Vector2 size, Vector2 offset)
        {
            var foe = new GameObject(name);
            foe.transform.SetParent(parent, false);
            foe.transform.position = position;

            var body = foe.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var trigger = foe.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = size;
            trigger.offset = offset;

            SpriteRenderer renderer = foe.AddComponent<SpriteRenderer>();
            renderer.sprite = frames.Length > firstFrame ? frames[firstFrame] : null;
            renderer.sortingLayerName = Phase01Assets.SortingActors;
            renderer.sortingOrder = -3;
            return foe;
        }

        /// <summary>Um Crystal Mite: um golpe so, e nao solta fragmento nenhum.</summary>
        private static GameObject BuildMite(Transform parent, string name, Vector3 position, float minX, float maxX,
            Sprite[] frames, Sprite shard)
        {
            GameObject mite = Foe(parent, name, position, frames, CrystalMite.QuadroEnrolar, new Vector2(0.9f, 0.55f), new Vector2(0f, 0.3f));
            mite.AddComponent<Damageable>().Configurar(MiteHealth, 0, shard);
            mite.AddComponent<CrystalMite>().Configurar(minX, maxX, frames);
            return mite;
        }

        // ------------------------------------------------------------------
        // Bancadas, Crisol e a historia
        // ------------------------------------------------------------------

        private static void BuildWorkbenches(Transform parent)
        {
            foreach ((string name, int x, int y) in Phase03Layout.Workbenches)
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

                benchObject.AddComponent<Workbench>().Configurar(rest.transform);
            }
        }

        private static GameObject BuildCrisol(Transform parent)
        {
            return BuildCrisolAt(parent, "Npc_Crisol", Phase03Layout.CrisolTile, Phase03Layout.CrisolLines);
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
            crisol.Configurar("Crisol", lines);

            // O rubi foi no District 2. Aqui ele so conversa e vende.
            crisol.ConfigurarEntregaRubi(false);

            Light2D glow = Phase01Generator.CreatePointLight(crisolObject.transform, "Glow", (tile.X + 0.5f, tile.Y + 1f),
                new Color(0.80f, 0.95f, 0.90f), 0.8f, 3.8f);
            Phase01Generator.AddCue(crisolObject, CueArt.Talk, TopOf(crisolObject, tile.Y + 3f), glow);
            return crisolObject;
        }

        // ------------------------------------------------------------------
        // O Capataz, o Geode Drill e a purificacao
        // ------------------------------------------------------------------

        /// <summary>
        /// O Capataz: ajoelhado no fundo leste da toca, com a folha de quadros,
        /// o corpo (e nele que se bate), as rachaduras do chao de onde o turno
        /// brota quando ele apita e os bichos do turno, ja na cena, desligados.
        /// </summary>
        private static void BuildElite(Transform parent)
        {
            (int X0, int X1) range = Phase03Layout.EliteRange;
            (int X, int Y, int Width, int Height) wake = Phase03Layout.EliteWake;
            (float hx, float hy) = Phase03Layout.ForemanHome;
            (float minX, float maxX) = Phase03Layout.ForemanRange;

            var eliteObject = new GameObject("Foreman");
            eliteObject.transform.SetParent(parent, false);
            eliteObject.transform.position = new Vector3(hx, hy, 0f);

            Sprite[] frames = Phase02Assets.LoadSheet(ForemanSheet, CaveMinerArt.FrameCount);
            SpriteRenderer body = eliteObject.AddComponent<SpriteRenderer>();
            body.sprite = frames.Length > 0 ? frames[CaveMinerArt.Dormant] : null;
            body.sortingLayerName = Phase01Assets.SortingActors;
            body.sortingOrder = -2;
            body.flipX = true;

            var rigidbody = eliteObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;

            // Do tamanho do homem desenhado: do chao ate o capacete.
            var area = eliteObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(1.4f, 2.8f);
            area.offset = new Vector2(0f, 1.4f);

            // A safira no capacete acende a cara dele no escuro, e vai junto.
            Phase01Generator.CreatePointLight(eliteObject.transform, "SapphireGlow",
                (hx, hy + 2.6f), new Color(0.35f, 0.6f, 1f), 0.8f, 4.5f);

            // O turno: rachaduras no chao da toca e quatro mites desligados.
            Sprite shard = Phase02Assets.LoadSprite(Phase02Assets.Shard);
            Sprite[] mite = Phase02Assets.LoadSheet(MiteSheet, MiteArt.FrameCount);
            var spots = new List<Transform>();
            var cracks = new List<SpriteRenderer>();

            foreach (int x in Phase03Layout.MonsterBroods)
            {
                var spot = new GameObject($"Brood_{x}");
                spot.transform.SetParent(parent, false);
                spot.transform.position = new Vector3(x + 0.5f, hy, 0f);

                var art = new GameObject("Art");
                art.transform.SetParent(spot.transform, false);
                cracks.Add(Sprite(art, CrackName(0), Phase01Assets.SortingTerrain, 6));
                spots.Add(spot.transform);
            }

            var brood = new List<GameObject>();
            for (int i = 0; i < 4; i++)
            {
                GameObject one = BuildMite(parent, $"Mite_Brood_{i + 1}", spots[i % spots.Count].position,
                    range.X0 + 0.5f, range.X1 + 0.5f, mite, shard);
                one.SetActive(false);
                brood.Add(one);
            }

            var crackFrames = new Sprite[3];
            for (int frame = 0; frame < 3; frame++) crackFrames[frame] = Phase02Assets.LoadSprite(CrackName(frame));

            var elite = eliteObject.AddComponent<TheResonant>();
            elite.Configurar(
                new Rect(wake.X, wake.Y, wake.Width, wake.Height),
                minX, maxX, hy, Phase03Layout.MonsterCeiling,
                Phase02Assets.LoadSprite(ShardShot),
                shard,
                Phase02Assets.LoadSprite(Phase02Assets.CutGem));
            elite.ConfigurarQuadros(frames, CaveMinerArt.SlamReach);
            elite.ConfigurarChave(Phase03Layout.AncientKeyId, "CHAVE ANTIGA obtida", Phase02Assets.LoadSprite(AncientKey));
            elite.ConfigurarCrias(brood.ToArray(), spots.ToArray(), cracks.ToArray(), crackFrames);

            // A boca da chamine fecha enquanto ele luta: nao se desce no meio.
            (int X, int Y, int Width, int Height) seal = Phase03Layout.EliteSeal;
            Phase01Generator.CreateArenaSeal(parent, _groundLayer, new RectInt(seal.X, seal.Y, seal.Width, seal.Height)).Observar(elite);
        }

        /// <summary>
        /// A purificacao do District 3: quando a broca para, a luz deixa de
        /// depender de som, o verde apaga, os que cacavam somem e os mineiros
        /// montam o salao de concertos na Camara do Ressonante. Os cristais e
        /// os veios de Oxide se resolvem sozinhos: escutam o mesmo aviso.
        /// </summary>
        private static AreaPurifier BuildPurification(Transform parent, GameObject enemies, GameObject crisol)
        {
            GameObject lighting = GameObject.Find("Lighting");
            GameObject globalObject = GameObject.Find("GlobalLight2D");
            GameObject cameraObject = GameObject.Find("Main Camera");
            GameObject groundObject = GameObject.Find("TM_Ground");

            // Quem volta: os mineiros e Crisol, com lampioes.
            Transform hall = Child(parent, "ConcertHall");
            var lamps = new List<Light2D>();

            foreach (Phase03Layout.LoreSpot spot in Phase03Layout.Purified)
            {
                if (spot.Kind == "crisol")
                {
                    BuildCrisolAt(hall, spot.Name, (spot.X, spot.Y), spot.Lines);
                }
                else
                {
                    var minerObject = new GameObject(spot.Name);
                    minerObject.transform.SetParent(hall, false);
                    minerObject.transform.position = new Vector3(spot.X + 0.5f, spot.Y, 0f);

                    // Corpo proprio, animado parado (respira, pisca), olhando pra quem chega do elevador.
                    int variant = spot.Name.EndsWith("2") ? District3NpcArt.MinerWoman : District3NpcArt.Miner;
                    Sprite[] idle = Phase02Assets.LoadSheet(District3NpcArt.SheetName(variant), District3NpcArt.IdleFrames);
                    var bodyObject = new GameObject("Body");
                    bodyObject.transform.SetParent(minerObject.transform, false);
                    SpriteRenderer body = bodyObject.AddComponent<SpriteRenderer>();
                    body.sprite = idle.Length > 0 ? idle[0] : null;
                    body.sortingLayerName = Phase01Assets.SortingActors;
                    body.sortingOrder = -4;
                    bodyObject.AddComponent<SpriteLoop>().Configurar(idle, 5f, District3NpcArt.BlinkFrame);

                    var area = minerObject.AddComponent<BoxCollider2D>();
                    area.isTrigger = true;
                    area.size = new Vector2(2.6f, 2.5f);
                    area.offset = new Vector2(0f, 1.25f);

                    minerObject.AddComponent<Npc>().Configurar(spot.Title, spot.Lines);
                    Phase01Generator.AddCue(minerObject, CueArt.Talk, TopOf(minerObject, spot.Y + 3f), null);
                }

                if (lighting != null)
                {
                    lamps.Add(Phase01Generator.CreatePointLight(lighting.transform, "PurifiedLamp_" + lamps.Count,
                        (spot.X + 0.5f, spot.Y + 3f), LampGlow, 0.8f, 8f));
                }
            }

            var purifierObject = new GameObject("AreaPurifier");
            purifierObject.transform.SetParent(parent, false);

            var purifier = purifierObject.AddComponent<AreaPurifier>();
            purifier.Configurar(
                globalObject != null ? globalObject.GetComponent<Light2D>() : null,
                OxideLights.ToArray(), lamps.ToArray(), new ParticleSystem[0],
                groundObject != null ? new[] { groundObject.GetComponent<Tilemap>() } : new Tilemap[0],
                cameraObject != null ? cameraObject.GetComponent<Camera>() : null,
                new[] { hall.gameObject }, new[] { enemies, crisol }, new Gate[0]);

            // Nada de dourado aqui: a luz fica fria, branca e estavel.
            purifier.ConfigurarVisual(new Color(0.84f, 0.82f, 1f), 0.6f,
                new Color(0.94f, 0.90f, 1f), new Color(0.07f, 0.06f, 0.12f));

            return purifier;
        }

        private static void BuildBoss(Transform parent, AreaPurifier purifier)
        {
            (int X, int Y) tile = Phase03Layout.BossTile;
            (int X0, int X1) range = Phase03Layout.BossRange;
            (int X, int Y, int Width, int Height) wake = Phase03Layout.BossWake;

            var bossObject = new GameObject("GeodeDrill");
            bossObject.transform.SetParent(parent, false);
            bossObject.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);

            Sprite(bossObject, FrameName("drill", 0), Phase01Assets.SortingActors, -2);

            // Parada, o motor ainda trepida; correndo, as esteiras e a espiral giram.
            Sprite[] drillFrames = LoadFrames("drill", GeodeArt.DrillFrames);
            bossObject.AddComponent<MotionFrames>().Configurar(drillFrames, drillFrames, 3f, 2.5f);

            var body = bossObject.AddComponent<BoxCollider2D>();
            body.isTrigger = true;
            body.size = new Vector2(4.2f, 2.4f);
            body.offset = new Vector2(0f, 1.1f);

            var rigidbody = bossObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;

            // A placa do chassi: so aparece quando ela para.
            var plaqueObject = new GameObject("ChassisPlaque");
            plaqueObject.transform.SetParent(bossObject.transform, false);

            var area = plaqueObject.AddComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(3f, 2.5f);
            area.offset = new Vector2(0f, 1.25f);

            var plaque = plaqueObject.AddComponent<Npc>();
            plaque.ConfigurarLeitura("a placa do chassi", Phase03Layout.DrillPlaqueLines);
            if (LoreArt.Phase03.TryGetValue(LoreArt.DrillPlaque, out LoreArt.Piece plaqueArt))
            {
                plaque.ConfigurarDocumento(Phase02Assets.LoadSprite(LoreArt.ViewSprite(plaqueArt.Lore)));
            }

            Light2D plaqueGlow = Phase01Generator.CreatePointLight(plaqueObject.transform, "Glow",
                (tile.X + 0.5f, tile.Y + 1.2f), new Color(0.90f, 0.92f, 1f), 0.6f, 4f);
            Phase01Generator.AddCue(plaqueObject, CueArt.Read, TopOf(plaqueObject, tile.Y + 2f), plaqueGlow);

            var drill = bossObject.AddComponent<GeodeDrill>();
            drill.Configurar(
                range.X0 + 0.5f, range.X1 + 0.5f,
                new Rect(wake.X, wake.Y, wake.Width, wake.Height),
                Phase03Layout.BossFloors.Select(floor => (float)floor).ToArray(),
                SlabRoots.Select(slab => slab.gameObject).ToArray(),
                Pillars.ToArray(),
                Phase02Assets.LoadSprite(ShardShot),
                Phase02Assets.LoadSprite(Phase02Assets.Shard),
                Phase02Assets.LoadSprite(Phase02Assets.ResonanceTip),
                purifier,
                plaqueObject);

            drill.ConfigurarDestrocos(WreckAssets.Build(parent, "GeodeDrillWreck",
                Phase02Assets.LoadSheet(DrillWreckSheet, DrillWreckArt.FrameCount), bossObject.transform.position));

            // As traças que a pancada no pilar solta: seis, desligadas, prontas pra brotar.
            Sprite debrisShard = Phase02Assets.LoadSprite(Phase02Assets.Shard);
            Sprite[] miteFrames = Phase02Assets.LoadSheet(MiteSheet, MiteArt.FrameCount);
            var brood = new List<GameObject>();
            for (int i = 0; i < 6; i++)
            {
                GameObject mite = BuildMite(parent, $"Mite_Drill_{i + 1}", new Vector3(tile.X + 0.5f, tile.Y, 0f),
                    range.X0 + 0.5f, range.X1 + 0.5f, miteFrames, debrisShard);
                mite.SetActive(false);
                brood.Add(mite);
            }

            var drillCracks = new Sprite[3];
            for (int frame = 0; frame < 3; frame++) drillCracks[frame] = Phase02Assets.LoadSprite(CrackName(frame));
            drill.ConfigurarCrias(brood.ToArray(), drillCracks);

            // A entrada da camara fecha enquanto ela luta.
            (int X, int Y, int Width, int Height) seal = Phase03Layout.BossSeal;
            Phase01Generator.CreateArenaSeal(parent, _groundLayer, new RectInt(seal.X, seal.Y, seal.Width, seal.Height)).Observar(drill);
        }

        /// <summary>
        /// O que da pra ler com E: o aviso, o bilhete, a beirada do poco e as
        /// quatro gravuras. As gravuras nao tem luz propria — pra ve-las e
        /// preciso fazer barulho.
        /// </summary>
        private static void BuildLore(Transform parent)
        {
            foreach (Phase03Layout.LoreSpot spot in Phase03Layout.Lore)
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

                bool lit = true;

                if (spot.Kind == "sign")
                {
                    artObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                    Sprite(artObject, Phase02Assets.Plaque, Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "silverplaque")
                {
                    artObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                    Sprite(artObject, SilverPlaque, Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "board")
                {
                    // A tabua do turno, fincada no chao.
                    Sprite(artObject, "roster_board", Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "crownsign")
                {
                    // A placa da Coroa, num poste na beira da Fenda.
                    Sprite(artObject, "crown_sign", Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "note")
                {
                    // Um bilhete preso na rocha por uma lasca de cristal, na altura do peito.
                    artObject.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                    Sprite(artObject, "miner_note", Phase01Assets.SortingTerrain, 6);
                }
                else if (spot.Kind == "helmet")
                {
                    Sprite(artObject, "miner_helmet", Phase01Assets.SortingTerrain, 22);
                }
                else if (spot.Kind == "page")
                {
                    Sprite(artObject, Phase02Assets.LoosePage, Phase01Assets.SortingTerrain, 22);
                }
                else if (spot.Kind.StartsWith("engraving"))
                {
                    // Na parede do fundo, do tamanho de quatro tiles.
                    artObject.transform.localPosition = new Vector3(0f, 3f, 0f);
                    Sprite(artObject, EngravingName(int.Parse(spot.Kind.Substring("engraving".Length))),
                        Phase01Assets.SortingTerrain, 4);
                    lit = false;
                }
                else
                {
                    Object.DestroyImmediate(artObject);
                    lit = false;
                }

                var readable = loreObject.AddComponent<Npc>();
                readable.ConfigurarLeitura(spot.Title, spot.Lines);

                // A arte propria (LoreArt): o desenho do mapa e o grande, mostrado na leitura.
                if (LoreArt.Phase03.TryGetValue(spot.Name, out LoreArt.Piece own))
                {
                    if (artObject == null)
                    {
                        artObject = new GameObject("Art");
                        artObject.transform.SetParent(loreObject.transform, false);
                    }

                    SpriteRenderer old = artObject.GetComponent<SpriteRenderer>();
                    if (old != null) Object.DestroyImmediate(old);
                    LoreArtAssets.Apply(own, artObject, readable, Phase01Assets.SortingTerrain);
                }

                // Uma luz (quando o que se le nao e escuro de proposito) e a
                // folhinha que flutua: da pra ler.
                Light2D loreGlow = null;
                if (lit)
                {
                    loreGlow = Phase01Generator.CreatePointLight(loreObject.transform, "Glow",
                        (spot.X + 0.5f, spot.Y + 1.2f), new Color(0.95f, 0.90f, 0.75f), 0.6f, 3.5f);
                }

                Phase01Generator.AddCue(loreObject, CueArt.Read, TopOf(loreObject, spot.Y + 2f), loreGlow);
            }
        }
    }
}
