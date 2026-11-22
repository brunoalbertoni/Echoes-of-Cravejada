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
            ground.color = RockTint;

            Tilemap decoration = Phase01Generator.CreateTilemap(root, "TM_Decoration", terrain, 20);
            decoration.color = SpikeTint;

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

            int width = Phase03Layout.Width;
            int height = Phase03Layout.Height;

            var backBuffer = new TileBase[width * height];
            var groundBuffer = new TileBase[width * height];
            var decorationBuffer = new TileBase[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = x + y * width;

                    backBuffer[index] = Phase01Generator.Resolve(tiles, Phase03Layout.BackgroundTileName(x, y));
                    groundBuffer[index] = Phase01Generator.Resolve(tiles, Phase03Layout.GroundTileName(x, y));

                    if (Phase03Layout.IsSpike(x, y))
                    {
                        decorationBuffer[index] = Phase01Generator.Resolve(tiles, Phase03Layout.SpikeTileName(x, y));
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
        // Luz
        // ------------------------------------------------------------------

        private static void BuildLighting(Material fogMaterial)
        {
            var root = new GameObject("Lighting");

            var globalObject = new GameObject("GlobalLight2D");
            globalObject.transform.SetParent(root.transform, false);

            // Quase nada. O distrito e escuro de proposito: o que se ve e o que
            // os cristais acendem. Subir a intensidade daqui e o jeito mais
            // rapido de clarear tudo se ficar dificil demais de enxergar.
            var global = globalObject.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.color = new Color(0.50f, 0.45f, 0.80f);
            global.intensity = 0.08f;

            // Os lampioes das salas seguras: as unicas luzes paradas.
            for (int i = 0; i < Phase03Layout.Lamps.Count; i++)
            {
                (float x, float y, float radius) = Phase03Layout.Lamps[i];
                Phase01Generator.CreatePointLight(root.transform, $"Lamp_{i + 1}", (x, y), LampGlow, 0.85f, radius);
            }

            // O brilho no fundo da Fenda: verde, lento, enorme, e nao depende de som.
            (float X, float Y) heart = Phase03Layout.HeartGlow;

            Light2D deep = Phase01Generator.CreatePointLight(root.transform, "HeartGlow_Deep",
                (heart.X, heart.Y), OxideGreen, 1.6f, 40f);
            deep.gameObject.AddComponent<GlowPulse>().Configurar(deep, null, 4.5f, 0f);

            // Ele sobe pela Fenda ate a beirada do mirante, cada vez mais fraco.
            Light2D rising = Phase01Generator.CreatePointLight(root.transform, "HeartGlow_Rising",
                (heart.X, 24f), OxideGreen, 0.55f, 34f);
            rising.gameObject.AddComponent<GlowPulse>().Configurar(rising, null, 4.5f, 0.15f);

            // E o mesmo verde, bem mais perto, no fundo da arena da broca.
            for (int i = 0; i < Phase03Layout.OxideGlows.Count; i++)
            {
                (float x, float y, float radius) = Phase03Layout.OxideGlows[i];

                Light2D oxide = Phase01Generator.CreatePointLight(root.transform, $"OxideGlow_{i + 1}",
                    (x, y), OxideGreen, 0.55f, radius);
                oxide.gameObject.AddComponent<GlowPulse>().Configurar(oxide, null, 4.5f, 0.1f * i);

                OxideLights.Add(oxide);
            }

            var atmosphere = new GameObject("Atmosphere");
            SceneKit.CreateHaze(atmosphere.transform, "Oxide_Rift", heart.X, 14f, 76f, 20f,
                OxideGreen, 0.10f, 5f, false, fogMaterial);
        }

        // ------------------------------------------------------------------
        // Marcadores
        // ------------------------------------------------------------------

        /// <summary>Objetos vazios marcando os lugares do layout, agrupados por sala.</summary>
        private static void BuildMarkers()
        {
            var root = new GameObject("Markers");
            var groups = new Dictionary<string, Transform>();

            foreach (Phase03Layout.Marker marker in Phase03Layout.Markers)
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
        // Bolsas e enfeites
        // ------------------------------------------------------------------

        /// <summary>O alto do desenho de algo (em mundo), pra pendurar o marcador em cima.</summary>
        private static float TopOf(GameObject target, float fallback)
        {
            float top = float.MinValue;
            foreach (SpriteRenderer renderer in target.GetComponentsInChildren<SpriteRenderer>())
            {
                if (renderer.sprite != null && renderer.gameObject.name != "Cue") top = Mathf.Max(top, renderer.bounds.max.y);
            }

            return top > float.MinValue ? top : fallback;
        }

        /// <summary>Os sacos de fragmentos escondidos.</summary>
        private static void BuildRewards(Transform parent)
        {
            foreach (Phase03Layout.RewardSpot spot in Phase03Layout.Rewards)
            {
                var pickup = new GameObject(spot.Id);
                pickup.transform.SetParent(parent, false);
                pickup.transform.position = new Vector3(spot.X + 0.5f, spot.Y + 0.8f, 0f);

                var trigger = pickup.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = 0.7f;

                Sprite(pickup, Phase02Assets.ShardCache, Phase01Assets.SortingActors, -1);
                pickup.AddComponent<RewardPickup>().Configurar(RewardPickup.Kind.CacheFragmento, spot.Id, spot.Amount);

                Light2D glow = Phase01Generator.CreatePointLight(pickup.transform, "Glow",
                    (spot.X + 0.5f, spot.Y + 0.8f), new Color(0.95f, 0.85f, 0.6f), 0.9f, 4f);
                Phase01Generator.AddCue(pickup, CueArt.Item, TopOf(pickup, spot.Y + 1.4f), glow);
            }
        }

        /// <summary>Os enfeites: a cabine do elevador da chegada, tambores, placas, correntes.</summary>
        private static void BuildDecor(Transform parent)
        {
            int index = 0;

            foreach (Phase03Layout.DecorSpot spot in Phase03Layout.Decor)
            {
                var decorObject = new GameObject($"{spot.Sprite}_{index++}");
                decorObject.transform.SetParent(parent, false);
                decorObject.transform.position = new Vector3(spot.X, spot.Y, 0f);

                SpriteRenderer renderer = Sprite(decorObject, spot.Sprite, Phase01Assets.SortingTerrain, spot.Order);

                // O que o Scrap nao toca fica em segundo plano: atras do chao (ordem
                // menor que a do TM_Ground) e apagado. O piso e a verga do templo
                // (ordem 11) sao terreno e ficam como estao.
                if (spot.Order < 10) renderer.color = Phase01Generator.DecorTint;

                if (spot.Tiled)
                {
                    renderer.drawMode = SpriteDrawMode.Tiled;
                    renderer.size = new Vector2(spot.Width, spot.Height);
                }
            }
        }
    }
}
