using UnityEditor;
using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Pecas de cena que mais de uma fase usa: vapor que empurra, passagem
    /// entre cenas, ponto de chegada e faixas de nevoa.
    /// </summary>
    internal static class SceneKit
    {
        /// <summary>Velocidade com que uma coluna de vapor levanta o Scrap.</summary>
        public const float LiftSpeed = 9f;

        /// <summary>Velocidade com que um jato deitado o carrega.</summary>
        public const float JetSpeed = 11f;

        private static readonly Color SteamColor = new Color(0.95f, 0.95f, 0.88f);

        /// <summary>
        /// Cria uma coluna (ou jato) de vapor que empurra: a area de gatilho, o
        /// componente SteamVent e as particulas.
        ///
        /// A area e dada em celulas. O gatilho e encolhido um pouco nos lados
        /// pra o Scrap nao ser pego so de roçar na beirada.
        /// </summary>
        public static SteamVent CreateSteamVent(Transform parent, string name, RectInt cells,
            SteamVent.Mode mode, int direction, float onTime, float offTime, Material steamMaterial,
            float visualStart = float.NaN)
        {
            var ventObject = new GameObject(name);
            if (parent != null) ventObject.transform.SetParent(parent, false);
            ventObject.transform.position = new Vector3(cells.x + cells.width * 0.5f, cells.y + cells.height * 0.5f, 0f);

            var trigger = ventObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = mode == SteamVent.Mode.Lift
                ? new Vector2(cells.width - 0.3f, cells.height)
                : new Vector2(cells.width, cells.height - 0.2f);

            bool lift = mode == SteamVent.Mode.Lift;
            float speed = lift ? LiftSpeed : JetSpeed;

            // As particulas nascem na boca e viajam o comprimento todo.
            float length = lift ? cells.height : cells.width;
            float mouth = lift ? cells.width : cells.height;

            var steamObject = new GameObject("Steam");
            steamObject.transform.SetParent(ventObject.transform, false);

            // A coluna pode comecar a aparecer antes de comecar a empurrar: e
            // o caso da chamine da Phase 1, onde o vapor so pega quem pula.
            float start = lift ? cells.y : (direction > 0 ? cells.x : cells.x + cells.width);
            if (!float.IsNaN(visualStart))
            {
                length += Mathf.Abs(start - visualStart);
                start = visualStart;
            }

            steamObject.transform.position = lift
                ? new Vector3(cells.x + cells.width * 0.5f, start, 0f)
                : new Vector3(start, cells.y + cells.height * 0.5f, 0f);

            var particles = steamObject.AddComponent<ParticleSystem>();
            float visualSpeed = speed * 0.8f;

            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = length / visualSpeed;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(1.1f, 2.3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(SteamColor.r, SteamColor.g, SteamColor.b, 0.22f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 300;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = mouth * 5f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = lift ? new Vector3(mouth * 0.8f, 0.2f, 1f) : new Vector3(0.2f, mouth * 0.8f, 1f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;

            if (lift)
            {
                velocity.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
                velocity.y = new ParticleSystem.MinMaxCurve(visualSpeed * 0.85f, visualSpeed * 1.15f);
            }
            else
            {
                velocity.x = new ParticleSystem.MinMaxCurve(
                    direction * visualSpeed * 0.85f, direction * visualSpeed * 1.15f);
                velocity.y = new ParticleSystem.MinMaxCurve(-0.2f, 0.3f);
            }

            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
            fade.enabled = true;
            fade.color = new ParticleSystem.MinMaxGradient(FadeInOut(0.1f, 0.6f));

            var renderer = steamObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = Phase01Assets.SortingTerrain;
            renderer.sortingOrder = 26;
            if (steamMaterial != null) renderer.sharedMaterial = steamMaterial;

            particles.Stop();

            var vent = ventObject.AddComponent<SteamVent>();
            vent.Configure(mode, speed, direction, onTime, offTime, new[] { particles });
            return vent;
        }

        /// <summary>Uma area que, quando o Scrap encosta, carrega outra cena.</summary>
        public static void CreateSceneExit(Transform parent, string name, RectInt cells,
            string sceneName, string spawnId)
        {
            var exitObject = new GameObject(name);
            if (parent != null) exitObject.transform.SetParent(parent, false);
            exitObject.transform.position = new Vector3(cells.x + cells.width * 0.5f, cells.y + cells.height * 0.5f, 0f);

            var trigger = exitObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(cells.width, cells.height);

            exitObject.AddComponent<SceneExit>().Configure(sceneName, spawnId);
        }

        /// <summary>Um ponto de chegada, com os pes do Scrap no chao da celula.</summary>
        public static void CreateSpawnPoint(Transform parent, string id, (int X, int Y) tile)
        {
            var point = new GameObject("SpawnPoint_" + id);
            if (parent != null) point.transform.SetParent(parent, false);
            point.transform.position = new Vector3(tile.X + 0.5f, tile.Y, 0f);
            point.AddComponent<SpawnPoint>().Configure(id);
        }

        /// <summary>
        /// Uma faixa deitada de nevoa feita de borroes grandes e lentos. Serve
        /// pra Oxide, vapor e fumaca de acido: so muda a cor.
        /// </summary>
        public static void CreateHaze(Transform parent, string name, float x, float y, float width,
            float thickness, Color color, float alpha, float rate, bool inFront, Material material)
        {
            var hazeObject = new GameObject(name);
            if (parent != null) hazeObject.transform.SetParent(parent, false);
            hazeObject.transform.position = new Vector3(x, y, 0f);

            var particles = hazeObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = 22f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(5f, 9f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(color.r, color.g, color.b, alpha);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 200;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = rate;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(width, thickness, 1f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.04f, 0.04f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
            fade.enabled = true;
            fade.color = new ParticleSystem.MinMaxGradient(FadeInOut(0.25f, 0.75f));

            var renderer = hazeObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = inFront ? Phase01Assets.SortingForeground : Phase01Assets.SortingTerrain;
            renderer.sortingOrder = inFront ? 5 : 4;
            if (material != null) renderer.sharedMaterial = material;

            particles.Stop();
        }

        /// <summary>Aparece ate o primeiro ponto, fica, e some a partir do segundo.</summary>
        public static Gradient FadeInOut(float fullAt, float fadeFrom)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, fullAt),
                    new GradientAlphaKey(1f, fadeFrom),
                    new GradientAlphaKey(0f, 1f),
                });

            return gradient;
        }

        /// <summary>Poe a cena no Build Settings, no fim da lista, se ainda nao estiver.</summary>
        public static void AddSceneToBuildSettings(string path)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            if (scenes.Exists(entry => entry.path == path)) return;

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
