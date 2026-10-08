using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma valvula enferrujada: a purificacao local do District 2. Segurar o
    /// botao perto dela faz o Scrap raspar e polir o volante; quando termina,
    /// ele gira e muda a sala — liga vapor, seca acido, abre passagem.
    ///
    /// Enquanto raspa o Scrap nao anda nem pula: fica vulneravel. Apanhar no
    /// meio joga fora o que ja tinha raspado.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Valve : MonoBehaviour
    {
        [Tooltip("Segundos segurando ate o volante girar.")]
        [SerializeField] private float scrapeTime = 2.2f;

        [Header("O que ela muda")]
        [SerializeField] private SteamVent[] ventsToTurnOn;
        [SerializeField] private AcidPool[] poolsToDrain;
        [SerializeField] private Gate[] gatesToOpen;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer wheel;
        [SerializeField] private Sprite cleanSprite;

        private Sprite _rustySprite;
        private ScrapController _controller;
        private ScrapHealth _health;
        private float _progress;
        private bool _scraping;
        private bool _turned;

        /// <summary>A valvula perto da qual o Scrap esta, se houver. O HUD usa pro aviso e pra barra.</summary>
        public static Valve Nearby { get; private set; }

        public float Progress => Mathf.Clamp01(_progress / Mathf.Max(0.1f, scrapeTime));

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
            _progress = 0f;
            if (wheel != null && _rustySprite != null) wheel.sprite = _rustySprite;
            if (_controller != null) Nearby = this;
        }

        private void Update()
        {
            if (_turned || _controller == null) return;

            bool holding = InteractHeld() && _controller.IsGrounded;

            // Apanhou no meio: perde o que tinha raspado.
            if (_scraping && _health != null && Time.time - _health.LastHitTime < 0.1f)
            {
                _progress = 0f;
                holding = false;
            }

            if (holding != _scraping)
            {
                _scraping = holding;
                _controller.ControlLocked = holding;
            }

            if (!_scraping) return;

            _progress += Time.deltaTime;
            if (_progress < scrapeTime) return;

            _turned = true;
            _scraping = false;
            _controller.ControlLocked = false;
            if (Nearby == this) Nearby = null;

            if (wheel != null && cleanSprite != null) wheel.sprite = cleanSprite;
            if (wheel != null) StartCoroutine(Spin());

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

            if (gatesToOpen != null)
            {
                foreach (Gate gate in gatesToOpen)
                {
                    if (gate != null) gate.Open();
                }
            }

            Turned?.Invoke(this);
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

        private static bool InteractHeld()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.isPressed || keyboard.kKey.isPressed)) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonNorth.isPressed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            _controller = controller;
            _health = controller.GetComponent<ScrapHealth>();
            if (!_turned) Nearby = this;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_controller == null || other.GetComponentInParent<ScrapController>() != _controller) return;

            if (_scraping) _controller.ControlLocked = false;

            _scraping = false;
            _controller = null;
            _health = null;
            if (Nearby == this) Nearby = null;
        }
    }
}
