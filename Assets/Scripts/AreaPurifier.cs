using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A purificacao da area: o que acontece com o distrito quando o mini boss
    /// cai. A nevoa do Oxide para, as luzes verdes se apagam, a luz geral fica
    /// quente e dourada, os lampioes acendem e os corrompidos somem.
    ///
    /// O estado persiste: voltar a cena depois de purificada ja a encontra
    /// assim. Ainda nao ha save, entao vale ate o jogo fechar.
    /// </summary>
    public class AreaPurifier : MonoBehaviour
    {
        [Tooltip("Segundos que a transformacao leva.")]
        [SerializeField] private float duration = 4f;

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

        private float[] _purifiedTargets;

        // Cada cena tem a sua purificacao; guarda-se o nome das que ja foram.
        private static readonly System.Collections.Generic.HashSet<string> Purified =
            new System.Collections.Generic.HashSet<string>();

        private bool _purified;

        /// <summary>Se a cena aberta agora ja foi purificada nesta sessao.</summary>
        public static bool IsPurified =>
            Purified.Contains(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        /// <summary>Quando aconteceu. O HUD usa pra mostrar o aviso.</summary>
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
            // Ja purificada numa visita anterior: aplica tudo de uma vez.
            if (IsPurified)
            {
                _purified = true;
                StartCoroutine(Run(0f));
            }
        }

        /// <summary>Dispara a transformacao. Chamar de novo nao faz nada.</summary>
        public void Purify()
        {
            if (_purified) return;

            _purified = true;
            Purified.Add(gameObject.scene.name);
            PurifiedTime = Time.time;
            Purifying?.Invoke();
            StartCoroutine(Run(duration));
        }

        private IEnumerator Run(float time)
        {
            if (oxideFog != null)
            {
                foreach (ParticleSystem fog in oxideFog)
                {
                    if (fog == null) continue;

                    // Para de nascer nevoa; a que ja existe se desfaz sozinha.
                    // Numa cena que ja chega purificada, some de uma vez.
                    fog.Stop(true, time > 0f
                        ? ParticleSystemStopBehavior.StopEmitting
                        : ParticleSystemStopBehavior.StopEmittingAndClear);
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

            Color fromGlobal = globalLight != null ? globalLight.color : Color.white;
            float fromIntensity = globalLight != null ? globalLight.intensity : 1f;
            Color fromBackground = viewCamera != null ? viewCamera.backgroundColor : Color.black;

            var fromLights = new float[corruptedLights != null ? corruptedLights.Length : 0];
            for (int i = 0; i < fromLights.Length; i++)
            {
                fromLights[i] = corruptedLights[i] != null ? corruptedLights[i].intensity : 0f;
            }

            var fromTints = new Color[tintedTilemaps != null ? tintedTilemaps.Length : 0];
            for (int i = 0; i < fromTints.Length; i++)
            {
                fromTints[i] = tintedTilemaps[i] != null ? tintedTilemaps[i].color : Color.white;
            }

            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                Apply(t / time, fromGlobal, fromIntensity, fromBackground, fromLights, fromTints);
                yield return null;
            }

            Apply(1f, fromGlobal, fromIntensity, fromBackground, fromLights, fromTints);

            SetActive(appearWhenPurified, true);

            if (gatesToOpen != null)
            {
                foreach (Gate gate in gatesToOpen)
                {
                    if (gate != null) gate.Open();
                }
            }
        }

        private void Apply(float k, Color fromGlobal, float fromIntensity, Color fromBackground,
            float[] fromLights, Color[] fromTints)
        {
            // Comeca devagar e termina devagar.
            k = Mathf.SmoothStep(0f, 1f, k);

            if (globalLight != null)
            {
                globalLight.color = Color.Lerp(fromGlobal, purifiedColor, k);
                globalLight.intensity = Mathf.Lerp(fromIntensity, purifiedIntensity, k);
            }

            if (viewCamera != null) viewCamera.backgroundColor = Color.Lerp(fromBackground, purifiedBackground, k);

            for (int i = 0; i < fromLights.Length; i++)
            {
                if (corruptedLights[i] != null) corruptedLights[i].intensity = Mathf.Lerp(fromLights[i], 0f, k);
            }

            for (int i = 0; i < _purifiedTargets.Length; i++)
            {
                if (purifiedLights[i] != null) purifiedLights[i].intensity = Mathf.Lerp(0f, _purifiedTargets[i], k);
            }

            for (int i = 0; i < fromTints.Length; i++)
            {
                if (tintedTilemaps[i] != null) tintedTilemaps[i].color = Color.Lerp(fromTints[i], purifiedTint, k);
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
