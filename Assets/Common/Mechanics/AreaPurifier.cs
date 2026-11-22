using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A purificacao da area: o que acontece com o distrito quando o mini boss
    /// cai. A nevoa do Oxide para, as luzes verdes se apagam, a luz geral muda,
    /// os lampioes acendem, o acido seca e os corrompidos somem.
    ///
    /// Ao vivo, e uma cena com camera dirigida:
    ///
    ///   1. O coracao. A camera fecha no ponto de onde a purificacao parte, o
    ///      mundo escurece e o Oxide da area inteira e sugado pra la — fios de
    ///      particulas verdes vindo de todo lado, girando — enquanto um nucleo
    ///      de luz cresce, pulsando, com a tela tremendo cada vez mais.
    ///   2. O estouro. Um clarao branco, um tremor forte, a ferrugem estourando
    ///      em lascas que caem e faiscas douradas, e uma onda de luz que se abre
    ///      em circulo a partir dali.
    ///   3. A onda. A camera abre devagar enquanto a onda corre o mapa, e cada
    ///      coisa muda quando a onda passa por ela — nao tudo de uma vez: os
    ///      verdes evaporam virando ouro, os lampioes acendem soltando faisca,
    ///      as correntes balancam, as engrenagens disparam, a nevoa para, o
    ///      acido seca, os corrompidos se desfazem, do centro pra fora.
    ///      Depois dela, uma poeira de luz cai devagar sobre tudo.
    ///   4. O passeio. A camera vai ate quem aparece com a purificacao (a
    ///      Solda, os mineiros) e mostra a chegada.
    ///   5. A volta. A camera volta pro Scrap e devolve o controle.
    ///
    /// O estado persiste: voltar a cena depois de purificada ja a encontra
    /// assim, sem cena. Ainda nao ha save, entao vale ate o jogo fechar.
    /// </summary>
    public class AreaPurifier : MonoBehaviour
    {
        [Header("Luz geral")]
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Color purifiedColor = new Color(1f, 0.90f, 0.70f);
        [SerializeField] private float purifiedIntensity = 0.95f;

        [Header("O que apaga e o que acende")]
        [SerializeField] private Light2D[] corruptedLights;
        [SerializeField] private Light2D[] purifiedLights;
        [SerializeField] private ParticleSystem[] oxideFog;

        [Header("Cenario")]
        [SerializeField] private Tilemap[] tintedTilemaps;
        [SerializeField] private Color purifiedTint = new Color(0.85f, 0.80f, 0.68f);
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Color purifiedBackground = new Color(0.30f, 0.24f, 0.14f);

        [Header("O que aparece e o que some")]
        [SerializeField] private GameObject[] appearWhenPurified;
        [SerializeField] private GameObject[] vanishWhenPurified;
        [SerializeField] private Gate[] gatesToOpen;

        [Tooltip("Pocas de acido que secam com a purificacao.")]
        [SerializeField] private AcidPool[] poolsToDrain;

        // As marcas da cerimonia, em segundos.
        private const float HeartTime = 2.2f;
        private const float WaveTime = 4.4f;
        private const float TourStop = 1.6f;
        private const float ReturnTime = 1.0f;
        private const int MaxTourStops = 2;

        /// <summary>Quantos tiles a beirada da onda leva pra apagar ou acender uma luz depois de passar por ela.</summary>
        private const float WaveSoftness = 7f;

        private float[] _purifiedTargets;

        // Cada cena tem a sua purificacao; guarda-se o nome das que ja foram.
        private static readonly HashSet<string> Purified = new HashSet<string>();

        private bool _purified;

        /// <summary>Se uma cena (pelo nome) ja foi purificada nesta sessao. O palacio (e o final) vem depois dos quatro distritos.</summary>
        public static bool WasPurified(string sceneName) => Purified.Contains(sceneName);

        /// <summary>Marca uma cena como purificada sem cerimonia (pra testar o palacio dando Play direto nele).</summary>
        public static void MarkPurified(string sceneName) => Purified.Add(sceneName);

        /// <summary>Se a cena aberta agora ja foi purificada nesta sessao.</summary>
        public static bool IsPurified =>
            Purified.Contains(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        /// <summary>
        /// Quantos segundos a cerimonia leva, do coracao a volta da camera. Quem
        /// espera ela acabar (a cena do boss) usa.
        /// </summary>
        public float Duration => HeartTime + WaveTime + TourStops().Count * TourStop + ReturnTime;

        /// <summary>Quando a onda terminou de correr o mapa. O HUD usa pra mostrar o aviso.</summary>
        public static float PurifiedTime { get; private set; } = -100f;

        /// <summary>
        /// Avisa que a cena aberta acabou de ser purificada. Serve pra quem tem
        /// um jeito proprio de reagir, como os cristais do District 3.
        /// </summary>
        public static event System.Action Purifying;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Purified.Clear();
            PurifiedTime = -100f;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Light2D global, Light2D[] corrupted, Light2D[] purified, ParticleSystem[] fog,
            Tilemap[] tilemaps, Camera camera, GameObject[] appear, GameObject[] vanish, Gate[] gates)
        {
            globalLight = global;
            corruptedLights = corrupted;
            purifiedLights = purified;
            oxideFog = fog;
            tintedTilemaps = tilemaps;
            viewCamera = camera;
            appearWhenPurified = appear;
            vanishWhenPurified = vanish;
            gatesToOpen = gates;
        }

        /// <summary>Usado pelos geradores de fase, pra distritos que nao ficam dourados.</summary>
        public void ConfigureLook(Color lightColor, float lightIntensity, Color tilemapTint, Color background)
        {
            purifiedColor = lightColor;
            purifiedIntensity = lightIntensity;
            purifiedTint = tilemapTint;
            purifiedBackground = background;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void ConfigurePools(AcidPool[] pools)
        {
            poolsToDrain = pools;
        }

        private void Awake()
        {
            // Os lampioes ficam apagados ate a purificacao; a intensidade que o
            // gerador deu a cada um e ate onde eles acendem.
            _purifiedTargets = new float[purifiedLights != null ? purifiedLights.Length : 0];

            for (int i = 0; i < _purifiedTargets.Length; i++)
            {
                if (purifiedLights[i] == null) continue;

                _purifiedTargets[i] = purifiedLights[i].intensity;
                purifiedLights[i].intensity = 0f;
            }

            SetActive(appearWhenPurified, false);
        }

        private void Start()
        {
            // Ja purificada numa visita anterior: aplica tudo de uma vez, sem cena.
            if (IsPurified)
            {
                _purified = true;
                ApplyAll();
            }
        }

        /// <summary>Dispara a purificacao partindo de onde o Scrap esta. Chamar de novo nao faz nada.</summary>
        public void Purify()
        {
            var scrap = FindAnyObjectByType<ScrapController>();
            Purify(scrap != null ? scrap.transform.position : transform.position);
        }

        /// <summary>Dispara a purificacao partindo de source (onde o boss caiu). Chamar de novo nao faz nada.</summary>
        public void Purify(Vector3 source)
        {
            if (_purified) return;

            _purified = true;
            Purified.Add(gameObject.scene.name);
            Purifying?.Invoke();
            Sfx.PurifyWave.Play();
            StartCoroutine(Ceremony(source));
        }

        // ------------------------------------------------------------------
        // A cerimonia
        // ------------------------------------------------------------------

        /// <summary>O que a onda vai mudando, cada coisa com a distancia dela ate o centro.</summary>
        private sealed class Wave
        {
            public Vector2 Center;
            public float Radius;
            public float MaxRadius;

            public readonly List<(Light2D Light, float From, float Distance)> Fading = new List<(Light2D, float, float)>();
            public readonly List<(Light2D Light, float Target, float Distance)> Rising = new List<(Light2D, float, float)>();
            public readonly List<(ParticleSystem Fog, float Distance)> Fog = new List<(ParticleSystem, float)>();
            public readonly List<(GameObject Target, float Distance)> Vanish = new List<(GameObject, float)>();
            public readonly List<(GameObject Target, float Distance)> Appear = new List<(GameObject, float)>();
            public readonly List<(AcidPool Pool, float Distance)> Pools = new List<(AcidPool, float)>();
            public readonly List<(Sway Swing, float Distance)> Swings = new List<(Sway, float)>();
            public readonly List<(SlowSpin Gear, float Distance)> Gears = new List<(SlowSpin, float)>();
        }

        private static readonly Color OxideMote = new Color(0.45f, 1f, 0.62f);
        private static readonly Color RustLight = new Color(0.78f, 0.46f, 0.24f);
        private static readonly Color RustDark = new Color(0.40f, 0.21f, 0.13f);
        private Color Glint => Color.Lerp(purifiedColor, Color.white, 0.55f);

        private IEnumerator Ceremony(Vector3 source)
        {
            var controller = FindAnyObjectByType<ScrapController>();
            bool began = !Cinematic.Active;
            if (began) Cinematic.Begin();

            void Hold()
            {
                if (controller != null) controller.ControlLocked = true;
            }

            Vector2 center = (Vector2)source + Vector2.up * 1.2f;
            Wave wave = BuildWave(center);

            Color fromGlobal = globalLight != null ? globalLight.color : Color.white;
            float fromIntensity = globalLight != null ? globalLight.intensity : 1f;
            Color fromBackground = viewCamera != null ? viewCamera.backgroundColor : Color.black;
            Color[] fromTints = CurrentTints();

            // 1. O coracao: a camera fecha, o mundo escurece e o nucleo cresce pulsando.
            ScrapCameraFollow.Direct(center, 0.6f, 0.45f);
            Light2D core = CreateLight("PurificationCore", center, Color.Lerp(purifiedColor, Color.white, 0.5f));

            for (float t = 0f; t < HeartTime; t += Time.deltaTime)
            {
                Hold();
                float k = t / HeartTime;

                if (globalLight != null) globalLight.intensity = Mathf.Lerp(fromIntensity, fromIntensity * 0.45f, k);

                float beat = 1f + 0.35f * Mathf.Sin(t * Mathf.Lerp(8f, 22f, k));
                core.intensity = Mathf.Lerp(0.2f, 3.2f, k * k) * beat;
                core.pointLightOuterRadius = Mathf.Lerp(1f, 6f, k);

                // O tremor cresce junto, em batidas.
                if (Mathf.Repeat(t, 0.35f) < Time.deltaTime) ScrapCameraFollow.Shake(0.03f + 0.08f * k, 0.25f);

                // O Oxide da area sendo sugado pro coracao: cada vez mais, e
                // chegando antes do estouro.
                float travel = Mathf.Lerp(1.1f, 0.45f, k);
                if (t + travel < HeartTime) Inhale(wave, center, Mathf.Lerp(40f, 170f, k) * Time.deltaTime, travel);

                yield return null;
            }

            // 2. O estouro: a ferrugem arrancada em lascas que caem, faiscas
            //    douradas e o resto do verde se desfazendo.
            Cinematic.Flash = 1f;
            ScrapCameraFollow.Shake(0.45f, 1.1f);
            Motes.Burst(center, 150, RustLight, RustDark, 3f, 10f, -12f, 1.9f, 2f, 0.8f);
            Motes.Burst(center, 90, Color.white, purifiedColor, 6f, 14f, -2f, 1.1f, 1f, 1.4f);
            Motes.Burst(center, 40, OxideMote, purifiedColor, 2f, 6f, 1f, 1.4f, 1f, 1f);
            SpriteRenderer ring = CreateRing(center);
            Light2D front = CreateLight("PurificationFront", center, purifiedColor);

            // 3. A onda: a camera abre devagar enquanto ela corre o mapa.
            ScrapCameraFollow.Direct(center, 1.75f, 1.1f);

            for (float t = 0f; t < WaveTime; t += Time.deltaTime)
            {
                Hold();
                float k = t / WaveTime;

                // A onda corre rapido no comeco e desacelera.
                wave.Radius = wave.MaxRadius * (1f - (1f - k) * (1f - k));
                Spread(wave);

                float look = Mathf.SmoothStep(0f, 1f, k);
                ApplyGlobal(look, fromGlobal, fromIntensity * 0.45f, fromBackground, fromTints);

                Cinematic.Flash = Mathf.Max(0f, 1f - t * 2.2f);
                core.intensity = Mathf.Lerp(3.2f, 0f, Mathf.Clamp01(k * 2.5f));

                if (ring != null)
                {
                    ring.transform.localScale = Vector3.one * Mathf.Max(0.1f, wave.Radius);
                    Color color = ring.color;
                    color.a = Mathf.Lerp(0.85f, 0f, k);
                    ring.color = color;
                }

                // Uma luz acompanha a beirada da onda, pra o mundo acender por onde ela passa.
                front.pointLightOuterRadius = Mathf.Max(2f, wave.Radius + 3f);
                front.pointLightInnerRadius = Mathf.Max(0f, wave.Radius - 4f);
                front.intensity = Mathf.Lerp(1.4f, 0f, k);

                yield return null;
            }

            Cinematic.Flash = 0f;
            wave.Radius = float.MaxValue;
            Spread(wave);

            // Os corrompidos sumiram um por um; agora some o grupo inteiro, pra
            // ninguem voltar quando o Scrap quebrar (eles revivem sozinhos).
            SetActive(vanishWhenPurified, false);
            ApplyGlobal(1f, fromGlobal, fromIntensity * 0.45f, fromBackground, fromTints);
            OpenGates();

            // O aviso do HUD ("Area purificada") entra quando a onda termina.
            PurifiedTime = Time.time;

            // A poeira de luz que fica caindo, mesmo depois de a camera voltar.
            StartCoroutine(Afterglow(7f));

            Destroy(core.gameObject);
            Destroy(front.gameObject);
            if (ring != null) Destroy(ring.gameObject);

            // 4. O passeio: quem apareceu com a purificacao.
            foreach (Vector2 stop in TourStops())
            {
                ScrapCameraFollow.Direct(stop + Vector2.up * 1.5f, 0.8f, 0.55f);
                StartCoroutine(Sparkle(stop));

                for (float t = 0f; t < TourStop; t += Time.deltaTime)
                {
                    Hold();
                    yield return null;
                }
            }

            // 5. A volta.
            if (controller != null)
            {
                ScrapCameraFollow.Direct((Vector2)controller.transform.position + Vector2.up * 1.5f, 1f, 0.45f);
            }

            for (float t = 0f; t < ReturnTime; t += Time.deltaTime)
            {
                Hold();
                yield return null;
            }

            ScrapCameraFollow.Release();

            if (began)
            {
                if (controller != null) controller.ControlLocked = false;
                Cinematic.End();
            }
        }

        /// <summary>Mede a distancia de cada coisa ate o centro, e o raio que a onda precisa cobrir.</summary>
        private Wave BuildWave(Vector2 center)
        {
            var wave = new Wave { Center = center };
            float farthest = 20f;

            float Measure(Vector3 position)
            {
                float distance = Vector2.Distance(center, position);
                farthest = Mathf.Max(farthest, distance);
                return distance;
            }

            if (corruptedLights != null)
            {
                foreach (Light2D light in corruptedLights)
                {
                    if (light != null) wave.Fading.Add((light, light.intensity, Measure(light.transform.position)));
                }
            }

            for (int i = 0; i < _purifiedTargets.Length; i++)
            {
                if (purifiedLights[i] != null) wave.Rising.Add((purifiedLights[i], _purifiedTargets[i], Measure(purifiedLights[i].transform.position)));
            }

            if (oxideFog != null)
            {
                foreach (ParticleSystem fog in oxideFog)
                {
                    if (fog != null) wave.Fog.Add((fog, Measure(fog.transform.position)));
                }
            }

            // O que some: os filhos um por um (os corrompidos vem todos num pai so).
            if (vanishWhenPurified != null)
            {
                foreach (GameObject target in vanishWhenPurified)
                {
                    if (target == null) continue;

                    if (target.transform.childCount == 0)
                    {
                        wave.Vanish.Add((target, Measure(target.transform.position)));
                        continue;
                    }

                    foreach (Transform child in target.transform)
                    {
                        if (child.gameObject.activeSelf) wave.Vanish.Add((child.gameObject, Measure(child.position)));
                    }
                }
            }

            if (appearWhenPurified != null)
            {
                foreach (GameObject target in appearWhenPurified)
                {
                    if (target != null) wave.Appear.Add((target, Measure(FocusOf(target))));
                }
            }

            if (poolsToDrain != null)
            {
                foreach (AcidPool pool in poolsToDrain)
                {
                    if (pool != null) wave.Pools.Add((pool, Measure(pool.transform.position)));
                }
            }

            // O que balanca e o que gira no cenario reage quando a onda passa.
            foreach (Sway swing in FindObjectsByType<Sway>(FindObjectsSortMode.None))
            {
                wave.Swings.Add((swing, Vector2.Distance(center, swing.transform.position)));
            }

            foreach (SlowSpin gear in FindObjectsByType<SlowSpin>(FindObjectsSortMode.None))
            {
                wave.Gears.Add((gear, Vector2.Distance(center, gear.transform.position)));
            }

            wave.MaxRadius = Mathf.Clamp(farthest + 6f, 24f, 220f);
            return wave;
        }

        /// <summary>Aplica a onda ate o raio atual: cada coisa muda conforme a beirada passa por ela.</summary>
        private void Spread(Wave wave)
        {
            float r = wave.Radius;

            foreach ((Light2D light, float from, float distance) in wave.Fading)
            {
                if (light == null) continue;
                bool wasLit = light.intensity > from * 0.98f;
                light.intensity = from * (1f - Mathf.Clamp01((r - distance) / WaveSoftness));

                // O verde evapora virando ouro, na hora que a beirada passa.
                if (wasLit && r >= distance) Motes.Rise(light.transform.position, 18, OxideMote, purifiedColor, 1f, 2f);
            }

            foreach ((Light2D light, float target, float distance) in wave.Rising)
            {
                if (light == null) continue;
                bool wasOff = light.intensity <= 0.001f;
                light.intensity = target * Mathf.Clamp01((r - distance) / WaveSoftness);

                // O lampiao acende soltando faisca.
                if (wasOff && r >= distance) Motes.Burst(light.transform.position, 12, Color.white, Glint, 1f, 3.5f, -4f, 0.9f, 1f, 1f);
            }

            for (int i = wave.Swings.Count - 1; i >= 0; i--)
            {
                (Sway swing, float distance) = wave.Swings[i];
                if (r < distance) continue;
                if (swing != null) swing.Kick(14f);
                wave.Swings.RemoveAt(i);
            }

            for (int i = wave.Gears.Count - 1; i >= 0; i--)
            {
                (SlowSpin gear, float distance) = wave.Gears[i];
                if (r < distance) continue;
                if (gear != null)
                {
                    gear.Kick(9f);
                    Motes.Burst(gear.transform.position, 8, Glint, purifiedColor, 1f, 2.5f, -6f, 0.8f, 1f, 1f);
                }

                wave.Gears.RemoveAt(i);
            }

            for (int i = wave.Fog.Count - 1; i >= 0; i--)
            {
                (ParticleSystem fog, float distance) = wave.Fog[i];
                if (r < distance) continue;

                // Para de nascer nevoa; a que ja existe se desfaz sozinha.
                if (fog != null) fog.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                wave.Fog.RemoveAt(i);
            }

            for (int i = wave.Vanish.Count - 1; i >= 0; i--)
            {
                (GameObject target, float distance) = wave.Vanish[i];
                if (r < distance) continue;

                if (target != null && target.activeInHierarchy)
                {
                    StartCoroutine(Puff(target.transform.position + Vector3.up * 0.6f, new Color(0.55f, 1f, 0.7f)));
                    Motes.Burst((Vector2)target.transform.position + Vector2.up * 0.6f, 24, RustLight, RustDark, 1.5f, 4.5f, -10f, 1.2f, 2f, 0.8f);
                    Motes.Rise(target.transform.position, 10, OxideMote, purifiedColor, 0.5f, 1.4f);
                    target.SetActive(false);
                }

                wave.Vanish.RemoveAt(i);
            }

            for (int i = wave.Appear.Count - 1; i >= 0; i--)
            {
                (GameObject target, float distance) = wave.Appear[i];
                if (r < distance) continue;

                if (target != null)
                {
                    target.SetActive(true);
                    StartCoroutine(Puff((Vector3)FocusOf(target) + Vector3.up, purifiedColor));
                }

                wave.Appear.RemoveAt(i);
            }

            for (int i = wave.Pools.Count - 1; i >= 0; i--)
            {
                (AcidPool pool, float distance) = wave.Pools[i];
                if (r < distance) continue;

                if (pool != null)
                {
                    pool.Drain();
                    Motes.Rise(pool.transform.position, 20, OxideMote, purifiedColor, 1.5f, 2f);
                }

                wave.Pools.RemoveAt(i);
            }
        }

        /// <summary>Os pontos que a camera visita no passeio: quem aparece com a purificacao.</summary>
        private List<Vector2> TourStops()
        {
            var stops = new List<Vector2>();
            if (appearWhenPurified == null) return stops;

            foreach (GameObject target in appearWhenPurified)
            {
                if (target == null || stops.Count >= MaxTourStops) continue;
                stops.Add(FocusOf(target));
            }

            return stops;
        }

        /// <summary>Onde uma coisa "esta": ela mesma, ou o primeiro filho, se ela e so um agrupador.</summary>
        private static Vector2 FocusOf(GameObject target)
        {
            if (target.transform.childCount > 0 && target.transform.position == Vector3.zero)
            {
                Vector3 sum = Vector3.zero;
                foreach (Transform child in target.transform) sum += child.position;
                return sum / target.transform.childCount;
            }

            return target.transform.position;
        }

        // ------------------------------------------------------------------
        // Efeitos da cerimonia
        // ------------------------------------------------------------------

        /// <summary>
        /// O Oxide sendo sugado pro coracao: particulas verdes saindo das
        /// luzes verdes por perto (de onde o Oxide esta) e de qualquer canto em
        /// volta, todas chegando ao centro em travel segundos. amount e
        /// quantas soltar neste quadro (a fracao vira chance).
        /// </summary>
        private void Inhale(Wave wave, Vector2 center, float amount, float travel)
        {
            int count = (int)amount + (Random.value < amount - (int)amount ? 1 : 0);
            for (int n = 0; n < count; n++)
            {
                Vector2 from;
                if (wave.Fading.Count > 0 && Random.value < 0.45f)
                {
                    (Light2D light, float _, float distance) = wave.Fading[Random.Range(0, wave.Fading.Count)];
                    from = light != null && distance < 22f
                        ? (Vector2)light.transform.position + Random.insideUnitCircle * 1.2f
                        : center + Random.insideUnitCircle.normalized * Random.Range(7f, 14f);
                }
                else
                {
                    from = center + Random.insideUnitCircle.normalized * Random.Range(6f, 14f);
                }

                Motes.Attract(from, center, OxideMote, Color.Lerp(OxideMote, Color.white, 0.6f), travel, Random.value < 0.3f ? 2f : 1f);
            }
        }

        /// <summary>A poeira de luz caindo devagar em volta da camera, rareando ate parar.</summary>
        private IEnumerator Afterglow(float seconds)
        {
            Camera camera = viewCamera != null ? viewCamera : Camera.main;
            float carry = 0f;

            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                if (camera == null) yield break;

                float height = camera.orthographicSize * 2f;
                float width = height * camera.aspect;
                Vector3 c = camera.transform.position;
                var area = new Rect(c.x - width * 0.5f, c.y + height * 0.1f, width, height * 0.45f);

                carry += Mathf.Lerp(30f, 0f, t / seconds) * Time.deltaTime;
                int count = (int)carry;
                carry -= count;
                if (count > 0) Motes.Drift(area, count, Glint, purifiedColor, 4f);

                yield return null;
            }
        }

        private Light2D CreateLight(string name, Vector2 position, Color color)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.position = new Vector3(position.x, position.y, 0f);

            var light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.intensity = 0f;
            light.pointLightInnerRadius = 0f;
            light.pointLightOuterRadius = 1f;
            return light;
        }

        private static Sprite _ringSprite;
        private static Material _unlit;

        /// <summary>A onda: um anel claro, desenhado aqui mesmo, que cresce a partir do centro.</summary>
        private SpriteRenderer CreateRing(Vector2 center)
        {
            if (_ringSprite == null)
            {
                const int size = 256;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                var pixels = new Color32[size * size];

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = (x + 0.5f) / size * 2f - 1f;
                        float dy = (y + 0.5f) / size * 2f - 1f;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);

                        // Uma beirada forte por fora e um veu fraco por dentro.
                        float edge = Mathf.Clamp01(1f - Mathf.Abs(d - 0.95f) / 0.05f);
                        float veil = d < 0.95f ? Mathf.Pow(d, 6f) * 0.35f : 0f;
                        byte alpha = (byte)(Mathf.Clamp01(edge + veil) * 255f);
                        pixels[x + y * size] = new Color32(255, 255, 255, alpha);
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply();

                // Diametro de 2 unidades: a escala vira o raio.
                _ringSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
            }

            if (_unlit == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader != null) _unlit = new Material(shader);
            }

            var ringObject = new GameObject("PurificationWave");
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.position = new Vector3(center.x, center.y, 0f);

            var ring = ringObject.AddComponent<SpriteRenderer>();
            ring.sprite = _ringSprite;
            ring.sortingLayerName = "Actors";
            ring.sortingOrder = 40;
            ring.color = new Color(purifiedColor.r, purifiedColor.g, purifiedColor.b, 0.85f);
            if (_unlit != null) ring.sharedMaterial = _unlit;
            return ring;
        }

        /// <summary>Um brilho rapido num ponto: um corrompido se desfazendo, ou alguem chegando.</summary>
        private IEnumerator Puff(Vector3 position, Color color)
        {
            Light2D light = CreateLight("PurificationPuff", position, color);

            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                float k = t / 0.6f;
                light.intensity = Mathf.Sin(k * Mathf.PI) * 1.8f;
                light.pointLightOuterRadius = Mathf.Lerp(1f, 3.5f, k);
                yield return null;
            }

            Destroy(light.gameObject);
        }

        /// <summary>O brilho em quem a camera visita no passeio.</summary>
        private IEnumerator Sparkle(Vector2 position)
        {
            Light2D light = CreateLight("PurificationSparkle", position + Vector2.up, Color.Lerp(purifiedColor, Color.white, 0.4f));

            for (float t = 0f; t < TourStop; t += Time.deltaTime)
            {
                float k = t / TourStop;
                light.intensity = Mathf.Sin(k * Mathf.PI) * 1.4f;
                light.pointLightOuterRadius = 4f + Mathf.Sin(t * 6f) * 0.5f;
                yield return null;
            }

            Destroy(light.gameObject);
        }

        // ------------------------------------------------------------------
        // O estado final
        // ------------------------------------------------------------------

        /// <summary>Tudo de uma vez: pra cena que ja chega purificada.</summary>
        private void ApplyAll()
        {
            if (oxideFog != null)
            {
                foreach (ParticleSystem fog in oxideFog)
                {
                    if (fog != null) fog.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            SetActive(vanishWhenPurified, false);

            if (poolsToDrain != null)
            {
                foreach (AcidPool pool in poolsToDrain)
                {
                    if (pool != null) pool.Drain();
                }
            }

            if (corruptedLights != null)
            {
                foreach (Light2D light in corruptedLights)
                {
                    if (light != null) light.intensity = 0f;
                }
            }

            for (int i = 0; i < _purifiedTargets.Length; i++)
            {
                if (purifiedLights[i] != null) purifiedLights[i].intensity = _purifiedTargets[i];
            }

            ApplyGlobal(1f, globalLight != null ? globalLight.color : Color.white,
                globalLight != null ? globalLight.intensity : 1f,
                viewCamera != null ? viewCamera.backgroundColor : Color.black, CurrentTints());

            SetActive(appearWhenPurified, true);
            OpenGates();
        }

        private Color[] CurrentTints()
        {
            var tints = new Color[tintedTilemaps != null ? tintedTilemaps.Length : 0];
            for (int i = 0; i < tints.Length; i++)
            {
                tints[i] = tintedTilemaps[i] != null ? tintedTilemaps[i].color : Color.white;
            }

            return tints;
        }

        private void ApplyGlobal(float k, Color fromGlobal, float fromIntensity, Color fromBackground, Color[] fromTints)
        {
            if (globalLight != null)
            {
                globalLight.color = Color.Lerp(fromGlobal, purifiedColor, k);
                globalLight.intensity = Mathf.Lerp(fromIntensity, purifiedIntensity, k);
            }

            if (viewCamera != null) viewCamera.backgroundColor = Color.Lerp(fromBackground, purifiedBackground, k);

            for (int i = 0; i < fromTints.Length; i++)
            {
                if (tintedTilemaps[i] != null) tintedTilemaps[i].color = Color.Lerp(fromTints[i], purifiedTint, k);
            }
        }

        private void OpenGates()
        {
            if (gatesToOpen == null) return;

            foreach (Gate gate in gatesToOpen)
            {
                if (gate != null) gate.Open();
            }
        }

        private static void SetActive(GameObject[] objects, bool active)
        {
            if (objects == null) return;

            foreach (GameObject target in objects)
            {
                if (target != null) target.SetActive(active);
            }
        }
    }
}
