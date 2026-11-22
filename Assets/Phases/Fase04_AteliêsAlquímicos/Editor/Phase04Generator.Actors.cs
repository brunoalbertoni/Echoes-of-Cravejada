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
    public static partial class Phase04Generator
    {
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
                        Body(foe, slime, QuicksilverSlime.QuadroRastejar);
                        trigger.size = new Vector2(1.2f, 0.85f);
                        trigger.offset = new Vector2(0f, 0.42f);

                        // A vida dos corrompidos do District 4 e o dobro da das fases de
                        // antes; cada metade da gosma tambem aguenta dois golpes.
                        foe.AddComponent<Damageable>().Configurar(4, 3, shard);
                        foe.AddComponent<QuicksilverSlime>().Configurar(minX, maxX, slime, 2);
                        break;

                    case Phase04Layout.FoeKind.FlaskThrower:
                        Body(foe, thrower, FlaskThrower.QuadroOcioso);
                        trigger.size = new Vector2(0.8f, 1.5f);
                        trigger.offset = new Vector2(0f, 0.75f);

                        foe.AddComponent<Damageable>().Configurar(4, 4, shard);
                        foe.AddComponent<FlaskThrower>().Configurar(Phase02Assets.LoadSprite(Flask),
                            Phase02Assets.LoadSprite(Phase02Assets.AcidSurface), thrower, ThrowerArt.HandHeight);
                        break;

                    case Phase04Layout.FoeKind.FailedExperiment:
                        Body(foe, experiment, FailedExperiment.QuadroOcioso);
                        trigger.size = new Vector2(1.2f, 1.8f);
                        trigger.offset = new Vector2(0f, 0.9f);

                        foe.AddComponent<Damageable>().Configurar(10, 6, shard);
                        foe.AddComponent<FailedExperiment>().Configurar(minX, maxX, experiment);
                        break;

                    case Phase04Layout.FoeKind.Sentinel:
                        Body(foe, glass, TransmutedSentinel.QuadroOcioso);
                        trigger.size = new Vector2(0.9f, 1.8f);
                        trigger.offset = new Vector2(0f, 0.9f);

                        // A armadura que vira prata (ferro, pro Scrap) pesa: encostar tira 2.
                        foe.AddComponent<Damageable>().Configurar(8, 5, shard);
                        foe.AddComponent<TransmutedSentinel>().Configurar(minX, maxX, glass, silver, 2);
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

                benchObject.AddComponent<Workbench>().Configurar(rest.transform);
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
            crisol.Configurar("Crisol", lines);

            // O rubi foi no District 2. Aqui ele so conversa e vende.
            crisol.ConfigurarEntregaRubi(false);

            Light2D crisolGlow = Phase01Generator.CreatePointLight(crisolObject.transform, "Glow",
                (crisolObject.transform.position.x, crisolObject.transform.position.y + 1f), new Color(0.80f, 0.95f, 0.90f), 0.8f, 3.8f);
            Phase01Generator.AddCue(crisolObject, CueArt.Talk, TopOf(crisolObject, crisolObject.transform.position.y + 3f), crisolGlow);
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
                    npc.Configurar(spot.Title, spot.Lines);
                    cabinet = loreObject;
                }
                else
                {
                    npc.ConfigurarLeitura(spot.Title, spot.Lines);

                    if (LoreArt.Phase04.TryGetValue(spot.Name, out LoreArt.Piece own))
                    {
                        LoreArtAssets.Apply(own, artObject, npc, Phase01Assets.SortingTerrain);
                    }
                    else
                    {
                        Debug.LogWarning($"[Phase 4] {spot.Name} nao tem arte propria no LoreArt.Phase04.");
                    }
                }

                // Uma luz, pra dar pra achar, e o marcador que flutua: balao pra
                // quem fala, folhinha pro que se le.
                bool talks = spot.Kind == "assistant";
                Light2D loreGlow = Phase01Generator.CreatePointLight(loreObject.transform, "Glow",
                    (spot.X + 0.5f, spot.Y + 1.6f), new Color(0.95f, 0.92f, 0.80f), 0.7f, 3.8f);
                Phase01Generator.AddCue(loreObject, talks ? CueArt.Talk : CueArt.Read, TopOf(loreObject, spot.Y + 2f), loreGlow);
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

                    // Corpo proprio, no padrao dos outros NPCs: respira e pisca.
                    Sprite[] idle = Phase02Assets.LoadSheet(District4NpcArt.SheetName, District4NpcArt.IdleFrames);
                    var bodyObject = new GameObject("Body");
                    bodyObject.transform.SetParent(npcObject.transform, false);
                    SpriteRenderer body = bodyObject.AddComponent<SpriteRenderer>();
                    body.sprite = idle.Length > 0 ? idle[0] : null;
                    body.sortingLayerName = Phase01Assets.SortingActors;
                    body.sortingOrder = -4;
                    bodyObject.AddComponent<SpriteLoop>().Configurar(idle, 5f, District4NpcArt.BlinkFrame);

                    var area = npcObject.AddComponent<BoxCollider2D>();
                    area.isTrigger = true;
                    area.size = new Vector2(2.6f, 2.5f);
                    area.offset = new Vector2(0f, 1.25f);

                    npcObject.AddComponent<Npc>().Configurar(spot.Title, spot.Lines);
                    Phase01Generator.AddCue(npcObject, CueArt.Talk, TopOf(npcObject, spot.Y + 3f), null);
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
            purifier.Configurar(
                globalObject != null ? globalObject.GetComponent<Light2D>() : null,
                OxideLights.ToArray(), lamps.ToArray(), new ParticleSystem[0],
                groundObject != null ? new[] { groundObject.GetComponent<Tilemap>() } : new Tilemap[0],
                cameraObject != null ? cameraObject.GetComponent<Camera>() : null,
                new[] { audience.gameObject }, vanish.ToArray(), new Gate[0]);

            // Prata limpa: a luz fica branca e clara, sem o verde.
            purifier.ConfigurarVisual(new Color(0.90f, 0.93f, 1f), 0.7f,
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
                circle.Configurar(rune, 1.1f);
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
            jar.Configurar(Phase04Layout.ArenaCeilingY, Phase04Layout.ArenaFloorY, glass, cracks, shadow, walls.ToArray());

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
            // Da grade da entrada (x 129) ate a grade da saida (x 171): nao sobra canto longe dele.
            boss.Configurar(arena.X0 + 1.5f, arena.X1 + 0.2f, Phase04Layout.ArenaFloorY, Phase04Layout.ArenaCeilingY,
                new Rect(wake.X, wake.Y, wake.Width, wake.Height),
                circles.ToArray(), jar, flood.ToArray(), whip, Phase02Assets.LoadSprite(Bead), exit);
            boss.ConfigurarQuadros(frames);

            // O que sobra quando ele escorre: a poca, o jaleco, a mascara, o tanque.
            boss.ConfigurarDestrocos(WreckAssets.Build(root, "MercuryAlchemistWreck",
                Phase02Assets.LoadSheet(AlchemistWreckSheet, AlchemistWreckArt.FrameCount), bossObject.transform.position));

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
            circle.Configurar("o coração do distrito", new GameObject[0], new GameObject[0], rune);

            Phase01Generator.CreatePointLight(circleObject.transform, "Glow",
                (sx + 0.5f, sy + 0.5f), new Color(0.85f, 0.95f, 1f), 0.6f, 4f);

            var stage = new GameObject("CeremonyStage");
            stage.transform.SetParent(parent, false);
            stage.AddComponent<CeremonyStage>().Configurar(circle, purifier);
        }
    }
}
