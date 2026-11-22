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
                    glow.gameObject.AddComponent<GlowPulse>().Configurar(glow, null, 2.6f + OxideLights.Count % 3 * 0.4f, OxideLights.Count * 0.17f);
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

                propObject.transform.position = new Vector3(prop.X, prop.Y, 0f);
                SpriteRenderer propSprite = Sprite(propObject, prop.Sprite, Phase01Assets.SortingTerrain, prop.Order);

                // O que o Scrap nao toca fica em segundo plano: atras do chao (as
                // ordens daqui sao todas menores que a do TM_Ground) e apagado. O
                // lustre, que tem luz, so escurece um pouco.
                if (propSprite != null)
                {
                    propSprite.sortingOrder = Mathf.Min(propSprite.sortingOrder, 9);
                    propSprite.color = prop.Sprite == "lab_chandelier"
                        ? Color.Lerp(Color.white, Phase01Generator.DecorTint, 0.4f)
                        : Phase01Generator.DecorTint;
                }

                // O lustre balanca devagar, preso no teto.
                if (prop.Sprite == "lab_chandelier") propObject.AddComponent<Sway>().Configurar(1.5f, 4.5f, (prop.X * 0.37f) % 1f);
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
        // O elevador da chegada
        // ------------------------------------------------------------------

        /// <summary>
        /// A cabine do District 3, parada no andar do saguao, com o proprio
        /// chao (ela tampa a boca do poco) e os trilhos descendo ate o fundo.
        /// Na chegada ela parte do fundo e sobe com o Scrap (ElevatorArrival).
        /// </summary>
        private static void BuildElevator(Transform parent)
        {
            (int x, int bottom, int stop, int top) = Phase04Layout.ElevatorShaft;
            float plate = 4f / Phase01Assets.PixelsPerUnit;

            var rails = new GameObject("ElevatorRails");
            rails.transform.SetParent(parent, false);
            rails.transform.position = new Vector3(x, bottom, 0f);
            SpriteRenderer rail = Sprite(rails, Phase02Assets.ElevatorRail, Phase01Assets.SortingTerrain, 4);
            rail.drawMode = SpriteDrawMode.Tiled;
            rail.size = new Vector2(3f, top + 1 - bottom);

            var car = new GameObject("ElevatorCar") { layer = _groundLayer };
            car.transform.SetParent(parent, false);

            // O piso de chapa da cabine fica logo abaixo da linha onde se pisa.
            car.transform.position = new Vector3(x, stop - plate, 0f);
            Sprite(car, Phase02Assets.ElevatorCage, Phase01Assets.SortingActors, -4);

            var body = car.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            // O chao da cabine: a celula logo abaixo do andar, que o mapa deixou sem tile.
            var floor = car.AddComponent<BoxCollider2D>();
            floor.size = new Vector2(3f, 1f);
            floor.offset = new Vector2(1.5f, plate - 0.5f);

            car.AddComponent<ElevatorArrival>().Configurar(Phase04Layout.FromDistrict3SpawnId, bottom - plate, 6f);
        }
    }
}
