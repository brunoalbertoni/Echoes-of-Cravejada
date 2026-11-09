using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O botao de uma estatua do Salao das Estatuas. De pe, perto dela,
    /// aperta-se E e o botao afunda com um clangor — um ruido forte, que vem
    /// da estatua e acorda a sala. Quem decide se foi a vez certa e o
    /// StatueHall; aqui so se aperta, se afunda e se acende.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class StatueButton : MonoBehaviour
    {
        [Tooltip("A vez desta estatua na sequencia (1 a 4).")]
        [SerializeField] private int order;

        [SerializeField] private SpriteRenderer button;
        [SerializeField] private Sprite buttonUp;
        [SerializeField] private Sprite buttonDown;

        [Tooltip("Os olhos (ou a fenda) acesos: so aparecem enquanto a sequencia vale.")]
        [SerializeField] private SpriteRenderer glow;
        [SerializeField] private Light2D glowLight;

        [Tooltip("Ate onde o clangor se ouve, em tiles, e a forca dele.")]
        [SerializeField] private float noiseRadius = 12f;
        [SerializeField, Range(0f, 1f)] private float noiseLoudness = 0.9f;

        private ScrapController _controller;
        private float _downUntil;
        private bool _lit;

        /// <summary>A estatua perto da qual o Scrap esta, se houver. O HUD usa pro aviso.</summary>
        public static StatueButton Nearby { get; private set; }

        /// <summary>Se o salao ainda escuta os botoes. Desligado depois que a porta abre.</summary>
        public bool Enabled { get; set; } = true;

        public int Order => order;

        /// <summary>Avisa que o botao foi apertado. O StatueHall escuta.</summary>
        public event System.Action<StatueButton> Pressed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Nearby = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(int newOrder, SpriteRenderer newButton, Sprite newButtonUp, Sprite newButtonDown,
            SpriteRenderer newGlow, Light2D newGlowLight)
        {
            order = newOrder;
            button = newButton;
            buttonUp = newButtonUp;
            buttonDown = newButtonDown;
            glow = newGlow;
            glowLight = newGlowLight;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Start()
        {
            SetLit(false);
        }

        /// <summary>Acende ou apaga os olhos da estatua.</summary>
        public void SetLit(bool lit)
        {
            _lit = lit;
            if (glow != null) glow.enabled = lit;
            if (glowLight != null) glowLight.enabled = lit;
        }

        public bool IsLit => _lit;

        private void Update()
        {
            if (button != null && buttonUp != null && buttonDown != null)
            {
                button.sprite = Time.time < _downUntil ? buttonDown : buttonUp;
            }

            if (!Enabled || _controller == null) return;
            if (AbilityCard.Showing || Crisol.ShopOpen != null || Cinematic.Active || Npc.Talking != null) return;
            if (!_controller.IsGrounded || _controller.ControlLocked) return;
            if (!InteractPressed()) return;

            _downUntil = Time.time + 0.5f;
            NoiseSystem.Emit(transform.position + Vector3.up, noiseRadius, noiseLoudness);
            Sfx.StatuePress.Play(transform.position);
            Pressed?.Invoke(this);
        }

        private static bool InteractPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame)) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonNorth.wasPressedThisFrame;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            _controller = controller;
            Nearby = this;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_controller == null || other.GetComponentInParent<ScrapController>() != _controller) return;

            _controller = null;
            if (Nearby == this) Nearby = null;
        }

        private void OnDisable()
        {
            if (Nearby == this) Nearby = null;
        }
    }
}
