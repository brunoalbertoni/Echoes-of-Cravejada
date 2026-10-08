using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A alavanca enferrujada: a purificacao local. Segurar o botao perto dela
    /// faz o Scrap raspar e polir; quando termina, o portao abre.
    ///
    /// Enquanto raspa ele nao anda nem pula — fica vulneravel. Apanhar no meio
    /// joga fora o que ja tinha raspado.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RustyLever : MonoBehaviour
    {
        [Tooltip("Segundos segurando ate terminar.")]
        [SerializeField] private float scrapeTime = 2.5f;

        [SerializeField] private Gate gate;
        [SerializeField] private SpriteRenderer leverSprite;
        [SerializeField] private Sprite cleanSprite;

        private ScrapController _controller;
        private ScrapHealth _health;
        private float _progress;
        private bool _scraping;
        private bool _done;

        /// <summary>A alavanca perto da qual o Scrap esta, se houver. O HUD usa pra mostrar o aviso e a barra.</summary>
        public static RustyLever Nearby { get; private set; }

        /// <summary>Quanto ja foi raspado, de 0 a 1.</summary>
        public float Progress => Mathf.Clamp01(_progress / Mathf.Max(0.1f, scrapeTime));

        public bool IsDone => _done;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Nearby = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Gate newGate, SpriteRenderer newLeverSprite, Sprite newCleanSprite)
        {
            gate = newGate;
            leverSprite = newLeverSprite;
            cleanSprite = newCleanSprite;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (_done || _controller == null) return;

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

            _done = true;
            _scraping = false;
            _controller.ControlLocked = false;

            if (leverSprite != null && cleanSprite != null) leverSprite.sprite = cleanSprite;
            if (gate != null) gate.Open();
            if (Nearby == this) Nearby = null;
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
            if (!_done) Nearby = this;
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
