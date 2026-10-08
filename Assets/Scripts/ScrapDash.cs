using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Steam Dash: um arranque curto e reto, na horizontal, com os pistoes
    /// das pernas. So existe depois que o Rusty Scrapper cai.
    ///
    /// Durante o arranque a gravidade nao age, entao da pra cruzar vaos que o
    /// pulo sozinho nao cruza. No ar, so um por pulo. Enquanto recarrega, o
    /// Scrap fica avermelhado de calor.
    ///
    /// Teclas: Shift esquerdo ou L; no controle, o gatilho direito de cima.
    /// </summary>
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapDash : MonoBehaviour
    {
        [SerializeField] private float speed = 17f;
        [SerializeField] private float duration = 0.20f;
        [SerializeField] private float cooldown = 0.9f;

        [Tooltip("A cor que ele vai pegando enquanto o dash recarrega.")]
        [SerializeField] private Color heatColor = new Color(1f, 0.45f, 0.35f);

        private ScrapController _controller;
        private Rigidbody2D _body;
        private SpriteRenderer _sprite;

        private float _dashUntil;
        private float _readyAt;
        private int _direction;
        private bool _usedInAir;

        private static bool _unlocked;

        /// <summary>Verdadeiro durante o arranque.</summary>
        public bool IsDashing => Time.time < _dashUntil;

        /// <summary>Se o Scrap ja tem o Steam Dash.</summary>
        public static bool Unlocked => _unlocked;

        /// <summary>Quando ele foi conquistado. O HUD usa pra mostrar o aviso.</summary>
        public static float UnlockTime { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _unlocked = false;
            UnlockTime = -100f;
        }

        /// <summary>
        /// Zera a recarga e devolve o dash do ar. Quem chama e o vapor: dar dash
        /// dentro de uma nuvem deixa dar outro logo em seguida.
        /// </summary>
        public void Refresh()
        {
            if (IsDashing) return;

            _readyAt = 0f;
            _usedInAir = false;
        }

        /// <summary>Da o dash sem aviso na tela: pra cenas em que ele ja chega com ele.</summary>
        public static void UnlockSilently()
        {
            _unlocked = true;
        }

        public static void Unlock()
        {
            if (_unlocked) return;

            _unlocked = true;
            UnlockTime = Time.time;
        }

        [Tooltip("Se nesta cena ele ja chega com o dash. Vale pro District 2, que conta com ele.")]
        [SerializeField] private bool unlockedFromStart;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void SetUnlockedFromStart(bool unlocked)
        {
            unlockedFromStart = unlocked;
        }

        private void Awake()
        {
            // Sem aviso na tela: aqui ele nao esta ganhando nada, so ja tem.
            if (unlockedFromStart) _unlocked = true;

            _controller = GetComponent<ScrapController>();
            _body = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_controller.IsGrounded) _usedInAir = false;

            // As costas esfriam aos poucos.
            if (_sprite != null && _unlocked)
            {
                float heat = Mathf.Clamp01((_readyAt - Time.time) / Mathf.Max(0.1f, cooldown));
                _sprite.color = Color.Lerp(Color.white, heatColor, heat);
            }

            if (!_unlocked || _controller.ControlLocked) return;
            if (Time.time < _readyAt || _usedInAir) return;
            if (!DashPressed()) return;

            _direction = _controller.Facing;
            _dashUntil = Time.time + duration;
            _readyAt = Time.time + cooldown;
            if (!_controller.IsGrounded) _usedInAir = true;

            // O Knockback trava a direcao pelo tempo do arranque, pra o
            // controller nao frear no meio.
            _controller.Knockback(new Vector2(_direction * speed, 0f), duration);
        }

        private void FixedUpdate()
        {
            if (Time.time >= _dashUntil) return;

            // Reto: sem cair e sem perder velocidade ate acabar.
            _body.linearVelocity = new Vector2(_direction * speed, 0f);
        }

        private static bool DashPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.lKey.wasPressedThisFrame)) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.rightShoulder.wasPressedThisFrame;
        }
    }
}
