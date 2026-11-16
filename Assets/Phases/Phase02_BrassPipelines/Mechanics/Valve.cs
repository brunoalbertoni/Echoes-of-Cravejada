using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma valvula emperrada: a purificacao local do District 2. O volante
    /// travou de ferrugem; cada golpe de cinzel o sacode e solta um pouco, e no
    /// ultimo ele gira e muda a sala — liga vapor, seca acido, abre passagem.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Valve : MonoBehaviour, IDamageable
    {
        [Tooltip("Quantos golpes ate o volante girar.")]
        [SerializeField] private int hitsToTurn = 3;

        [Header("O que ela muda")]
        [SerializeField] private SteamVent[] ventsToTurnOn;
        [SerializeField] private AcidPool[] poolsToDrain;
        [SerializeField] private Gate[] gatesToOpen;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer wheel;
        [SerializeField] private Sprite cleanSprite;

        private Sprite _rustySprite;
        private ScrapController _controller;
        private int _hits;
        private bool _turned;
        private float _shake;

        /// <summary>A valvula perto da qual o Scrap esta, se houver. O HUD usa pro aviso e pra barra.</summary>
        public static Valve Nearby { get; private set; }

        public float Progress => Mathf.Clamp01(_hits / (float)Mathf.Max(1, hitsToTurn));

        /// <summary>
        /// Quem abre os portoes e outro (a cena das travas): o volante gira, mas
        /// os portoes ficam esperando a camera chegar neles.
        /// </summary>
        public bool DeferGates { get; set; }

        public Gate[] Gates => gatesToOpen;

        public bool IsTurned => _turned;

        /// <summary>Avisa quando o volante gira. O Boiler Master escuta as valvulas da arena.</summary>
        public event System.Action<Valve> Turned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Nearby = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(SteamVent[] vents, AcidPool[] pools, Gate[] gates,
            SpriteRenderer newWheel, Sprite newCleanSprite)
        {
            ventsToTurnOn = vents;
            poolsToDrain = pools;
            gatesToOpen = gates;
            wheel = newWheel;
            cleanSprite = newCleanSprite;
        }

        private void Awake()
        {
            if (wheel != null) _rustySprite = wheel.sprite;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        /// <summary>
        /// Volta a ficar enferrujada, sem desfazer o que ja mudou. O Boiler
        /// Master usa nas valvulas da arena, pra poderem ser raspadas de novo.
        /// </summary>
        public void Rust()
        {
            _turned = false;
            _hits = 0;
            if (wheel != null && _rustySprite != null) wheel.sprite = _rustySprite;
            if (_controller != null) Nearby = this;
        }

        public void Damage(int amount, Vector2 from)
        {
            if (_turned) return;

            _hits++;
            _shake = 0.25f;
            if (_hits < hitsToTurn) return;

            _turned = true;
            _shake = 0f;
            if (Nearby == this) Nearby = null;

            if (wheel != null && cleanSprite != null) wheel.sprite = cleanSprite;
            if (wheel != null) StartCoroutine(Spin());
            Sfx.ValveTurn.Play(transform.position);

            if (ventsToTurnOn != null)
            {
                foreach (SteamVent vent in ventsToTurnOn)
                {
                    if (vent != null) vent.SetSwitchedOn(true);
                }
            }

            if (poolsToDrain != null)
            {
                foreach (AcidPool pool in poolsToDrain)
                {
                    if (pool != null) pool.Drain();
                }
            }

            if (gatesToOpen != null && !DeferGates)
            {
                foreach (Gate gate in gatesToOpen)
                {
                    if (gate != null) gate.Open();
                }
            }

            Turned?.Invoke(this);
        }

        private void Update()
        {
            if (wheel == null || _shake <= 0f) return;

            _shake -= Time.deltaTime;
            float angle = Mathf.Sin(Time.time * 80f) * 8f * Mathf.Clamp01(_shake / 0.25f);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, _shake > 0f ? angle : 0f);
        }

        private IEnumerator Spin()
        {
            const float time = 0.8f;

            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, -360f * (t / time));
                yield return null;
            }

            wheel.transform.localRotation = Quaternion.identity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            _controller = controller;
            if (!_turned) Nearby = this;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_controller == null || other.GetComponentInParent<ScrapController>() != _controller) return;

            _controller = null;
            if (Nearby == this) Nearby = null;
        }
    }
}
