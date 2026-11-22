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
        // Protagonista
        // ------------------------------------------------------------------

        /// <summary>
        /// Monta o Scrap. E o mesmo em todas as fases: o que muda e so onde
        /// ele nasce.
        /// </summary>
        internal static Transform BuildScrap(Sprite sprite, Sprite polishedSprite, int groundLayer,
            (int X, int Y) spawn)
        {
            var scrap = new GameObject("Scrap");
            scrap.transform.position = new Vector3(spawn.X + 0.5f, spawn.Y, 0f);

            // Os quadros de verdade do personagem. Os dois sprites recebidos sao
            // os placeholders antigos, e so entram se a arte nova nao existir.
            Sprite[] scrapFrames = Phase02Assets.HeroFrames(false);
            Sprite[] polishedFrames = Phase02Assets.HeroFrames(true);

            if (scrapFrames[0] != null) sprite = scrapFrames[0];
            if (polishedFrames[0] != null) polishedSprite = polishedFrames[0];

            var renderer = scrap.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = Phase01Assets.SortingActors;
            renderer.sortingOrder = 0;

            var body = scrap.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = scrap.AddComponent<CapsuleCollider2D>();
            collider.direction = CapsuleDirection2D.Vertical;
            // O Scrap e pequeno: a caixa acompanha o desenho (uns 1,2 tile de altura).
            collider.size = new Vector2(0.7f, 1.05f);
            collider.offset = new Vector2(0f, 0.525f);
            collider.sharedMaterial = GetOrCreateFrictionlessMaterial();

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(scrap.transform, false);
            groundCheck.transform.localPosition = new Vector3(0f, 0.06f, 0f);

            var controller = scrap.AddComponent<ScrapController>();

            // Os campos do controller sao privados com [SerializeField], entao
            // a ligacao e feita pelo SerializedObject.
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("checagemSolo").objectReferenceValue = groundCheck.transform;
            serialized.FindProperty("camadasSolo").intValue = 1 << groundLayer;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // O que ele carrega de uma cena pra outra: a aparencia (sucata ou
            // polido), a chegada no ponto certo e a volta pro chao seguro
            // quando cai no acido.
            // Os olhos dele iluminam em volta. Sem isso ele some nos trechos
            // escuros: a ferrugem do corpo tem quase a cor da sombra.
            CreatePointLight(scrap.transform, "EyeGlow", (spawn.X + 0.5f, spawn.Y + 0.75f),
                new Color(1f, 0.90f, 0.70f), 0.75f, 4.5f);

            scrap.AddComponent<ScrapAppearance>().Configurar(sprite, polishedSprite);
            scrap.AddComponent<ScrapRespawn>();
            scrap.AddComponent<SceneArrival>();
            scrap.AddComponent<ScrapHealth>().Configurar(Phase02Assets.LoadSprite(Phase02Assets.OldScrap));

            // O risco do cinzel: um sprite filho, sem luz, que so aparece na
            // hora do golpe.
            var slashObject = new GameObject("ChiselSlash");
            slashObject.transform.SetParent(scrap.transform, false);

            var slash = slashObject.AddComponent<SpriteRenderer>();
            slash.sprite = Phase02Assets.LoadSprite(Phase02Assets.ChiselSlash);
            slash.sortingLayerName = Phase01Assets.SortingActors;
            slash.sortingOrder = 5;

            Material unlit = Phase01Assets.GetOrCreateUnlitMaterial();
            if (unlit != null) slash.sharedMaterial = unlit;
            slash.enabled = false;

            scrap.AddComponent<ScrapChisel>().Configurar(slash);

            // O Steam Dash ja vai junto, desligado: quem liga e o Ferrolho, ao
            // montar o motor do Rusty Scrapper.
            scrap.AddComponent<ScrapDash>();

            // Por ultimo, porque le o estado de todos os outros.
            scrap.AddComponent<ScrapAnimator>().Configurar(scrapFrames, polishedFrames);

            return scrap.transform;
        }

        /// <summary>
        /// Sem atrito: senao o Scrap gruda na lateral das paredes ao encostar
        /// no ar. Toda a velocidade horizontal vem do controller.
        /// </summary>
        private static PhysicsMaterial2D GetOrCreateFrictionlessMaterial()
        {
            const string path = Phase01Assets.GeneratedRoot + "/ScrapFrictionless.physicsMaterial2D";

            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (material != null) return material;

            material = new PhysicsMaterial2D("ScrapFrictionless")
            {
                friction = 0f,
                bounciness = 0f,
            };

            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssets();
            return material;
        }

        // ------------------------------------------------------------------
        // Camera, luz e poeira
        // ------------------------------------------------------------------

        internal static void BuildCamera(Transform scrap, int mapWidth, int mapHeight, Color background)
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.nearClipPlane = -100f;
            camera.farClipPlane = 100f;

            // 384x216 a 16 PPU mostra ~13,5 tiles de altura: um leve zoom sobre
            // os 480x270 de antes, com o Scrap ainda pequeno o bastante pra a
            // sala respirar. O zoom vem daqui, nao do Size da Camera: o Pixel
            // Perfect sobrescreve o Size. Use resolucoes que dividem 1920x1080
            // sem sobrar (480x270, 384x216, 320x180), senao nada muda.
            var pixelPerfect = cameraObject.AddComponent<PixelPerfectCamera>();
            pixelPerfect.assetsPPU = Phase01Assets.PixelsPerUnit;
            pixelPerfect.refResolutionX = 384;
            pixelPerfect.refResolutionY = 216;

            camera.orthographicSize = 216f / (2f * Phase01Assets.PixelsPerUnit);

            cameraObject.AddComponent<AudioListener>();

            cameraObject.transform.position = new Vector3(
                scrap.position.x, scrap.position.y + 2f, -10f);

            var follow = cameraObject.AddComponent<ScrapCameraFollow>();
            follow.Configurar(scrap, new Rect(0f, 0f, mapWidth, mapHeight));

            cameraObject.AddComponent<GameHud>();
        }

        private static void BuildLighting()
        {
            var root = new GameObject("Lighting");

            var globalObject = new GameObject("GlobalLight2D");
            globalObject.transform.SetParent(root.transform, false);

            // Fraca, fria e esverdeada: a sala e escura e doente, e o que da
            // pra ver vem dos pontos de luz. Subir a intensidade daqui e o
            // jeito mais rapido de clarear tudo se ficar dificil de enxergar.
            var global = globalObject.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.color = new Color(0.62f, 0.80f, 0.78f);
            global.intensity = 0.5f;

            // A unica luz quente do poco, em volta de onde o Scrap acorda: ele
            // tem que ser o ponto mais vivo da tela.
            CreatePointLight(root.transform, "WakeGlow", Phase01Layout.WakeGlowPosition,
                new Color(1f, 0.80f, 0.55f), 0.8f, 6f);

            // O lampiao da bancada: a unica luz quente e parada dos andares de cima.
            for (int i = 0; i < Phase01Layout.LampGlows.Length; i++)
            {
                (float X, float Y, float Radius) lamp = Phase01Layout.LampGlows[i];
                CreatePointLight(root.transform, $"LampGlow_{i + 1}", (lamp.X, lamp.Y),
                    new Color(1f, 0.82f, 0.55f), 0.9f, lamp.Radius);
            }

            // A luz fria descendo pela chamine: cai em cima do patamar do topo
            // e mostra pra onde e a subida antes de voce conseguir chegar la.
            CreatePointLight(root.transform, "ChimneyLight", Phase01Layout.ChimneyLightPosition,
                new Color(0.62f, 0.85f, 0.95f), 0.9f, 11f);

            // Uma brasa cor de ferrugem na boca do tunel da arena. Fraca de
            // proposito: e um aviso, nao uma lanterna.
            CreatePointLight(root.transform, "LairGlow", Phase01Layout.LairGlowPosition,
                new Color(1f, 0.42f, 0.22f), 0.55f, 5f);

            // E o Oxide, verde-cobre, no chao e atras das janelas.
            for (int i = 0; i < Phase01Layout.OxideGlows.Length; i++)
            {
                (float X, float Y, float Radius) glow = Phase01Layout.OxideGlows[i];
                CreatePointLight(root.transform, $"OxideGlow_{i + 1}", (glow.X, glow.Y),
                    OxideGreen, 0.7f, glow.Radius);
            }
        }

        internal static Light2D CreatePointLight(Transform parent, string name, (float X, float Y) position,
            Color color, float intensity, float outerRadius)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.position = new Vector3(position.X, position.Y, 0f);

            var light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.falloffIntensity = 0.7f;

            // O raio nao tem setter publico nesta versao da URP, entao vai
            // pelos campos serializados.
            var serialized = new SerializedObject(light);
            SetFloatIfPresent(serialized, "m_PointLightOuterRadius", outerRadius);
            SetFloatIfPresent(serialized, "m_PointLightInnerRadius", outerRadius * 0.25f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return light;
        }

        private static void SetFloatIfPresent(SerializedObject serialized, string path, float value)
        {
            SerializedProperty property = serialized.FindProperty(path);
            if (property != null) property.floatValue = value;
        }

        /// <summary>
        /// A nevoa do Oxide, em faixas deitadas — quais e onde, o layout diz.
        ///
        /// Ela so aparece com o jogo rodando (ou com o objeto selecionado na
        /// cena): sao particulas, e o editor nao simula o que nao esta em foco.
        /// </summary>
        private static void BuildFog(Material material)
        {
            var root = new GameObject("OxideFog");

            foreach (Phase01Layout.FogBank bank in Phase01Layout.FogBanks)
            {
                CreateFogBank(root.transform, bank, material);
            }
        }

        internal static void CreateFogBank(Transform parent, Phase01Layout.FogBank bank, Material material)
        {
            var fogObject = new GameObject(bank.Name);
            fogObject.transform.SetParent(parent, false);
            fogObject.transform.position = new Vector3(bank.X, bank.Y, 0f);

            var particles = fogObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            // Senao a sala comeca limpa e a nevoa leva meio minuto pra chegar.
            main.prewarm = true;
            main.startLifetime = 22f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(5f, 9f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(OxideGreen.r, OxideGreen.g, OxideGreen.b, bank.Alpha);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 200;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = bank.Rate;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(bank.Width, bank.Thickness, 1f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.04f, 0.04f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            // Cada borrao aparece e some devagar, senao da pra ver eles
            // nascendo e morrendo.
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.25f),
                    new GradientAlphaKey(1f, 0.75f),
                    new GradientAlphaKey(0f, 1f),
                });

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fade);

            // Atras do terreno a nevoa some dentro da rocha, que e o certo; na
            // frente ela passa por cima do Scrap e do chao.
            var renderer = fogObject.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = bank.InFront
                ? Phase01Assets.SortingForeground
                : Phase01Assets.SortingTerrain;
            renderer.sortingOrder = bank.InFront ? 5 : 4;
            if (material != null) renderer.sharedMaterial = material;

            particles.Stop();
        }


        /// <summary>Metal dust: poeira de minerio descendo devagar, sala por sala.</summary>
        private static void BuildDust(Material material)
        {
            var root = new GameObject("MetalDust");

            foreach ((string name, float x, float width, float top, float fall, float rate)
                     in Phase01Layout.DustFalls)
            {
                var dustObject = new GameObject(name);
                dustObject.transform.SetParent(root.transform, false);
                dustObject.transform.position = new Vector3(x, top, 0f);

                var particles = dustObject.AddComponent<ParticleSystem>();

                ParticleSystem.MainModule main = particles.main;
                main.loop = true;
                main.prewarm = true;
                // O grao morre mais ou menos na altura do chao. Vivendo mais que
                // isso ele atravessa o piso e aparece caindo por dentro da rocha.
                main.startLifetime = DustLifetime(fall);
                main.startSpeed = 0f;
                main.startSize = 0.12f;
                // Clara e esverdeada, pra aparecer contra o fundo escuro.
                main.startColor = new Color(0.70f, 0.85f, 0.75f, 0.55f);
                main.gravityModifier = DustGravityModifier;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 400;

                ParticleSystem.EmissionModule emission = particles.emission;
                emission.rateOverTime = rate;

                ParticleSystem.ShapeModule shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(width, 1f, 1f);

                ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.World;
                velocity.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
                velocity.y = new ParticleSystem.MinMaxCurve(-DustFastestFall, -DustSlowestFall);
                velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

                var renderer = dustObject.GetComponent<ParticleSystemRenderer>();
                renderer.sortingLayerName = Phase01Assets.SortingForeground;
                renderer.sortingOrder = 10;
                if (material != null) renderer.sharedMaterial = material;

                particles.Stop();
            }
        }

        /// <summary>
        /// Quanto tempo um grao de poeira leva pra cair tantos tiles, na
        /// velocidade media: resolve d = v*t + a*t²/2 pra t.
        /// </summary>
        private static float DustLifetime(float fall)
        {
            float speed = (DustSlowestFall + DustFastestFall) * 0.5f;
            float acceleration = Mathf.Abs(Physics.gravity.y) * DustGravityModifier;

            return (-speed + Mathf.Sqrt(speed * speed + 2f * acceleration * fall)) / acceleration;
        }

        /// <summary>
        /// Objetos vazios marcando os lugares que as mecanicas vao precisar.
        /// Nao fazem nada sozinhos: sao so pontos com nome pra ligar depois.
        /// </summary>
        /// <summary>
        /// Os pontos de chegada desta cena. A ida pro District 2 fica depois do
        /// abismo e e montada junto com o boss, que e quem a destranca.
        /// </summary>
        private static void BuildTravel()
        {
            var root = new GameObject("Travel");

            SceneKit.CreateSpawnPoint(root.transform, Phase01Layout.StartSpawnId, Phase01Layout.SpawnTile);
            SceneKit.CreateSpawnPoint(root.transform, Phase01Layout.FromDistrict2SpawnId, Phase01Layout.ReturnTile);
        }
    }
}
