using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Movimento do Scrap: andar, pular e o Wall Grip (escorregar devagar
    /// numa parede e pular dela).
    ///
    /// O "heavy feel" do GDD sai daqui, dos numeros, e nao da arte: aceleracao
    /// baixa, gravidade alta e um freio lento que faz ele derrapar um tiquinho
    /// ao parar. Tudo esta no Inspector pra tunar com o jogo rodando.
    ///
    /// Le o teclado e o gamepad direto pelo Input System, sem depender de um
    /// asset de InputActions. Se um dia quiser rebind em runtime, migre pro
    /// Assets/Settings/InputSystem_Actions.inputactions que ja existe no projeto.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapController : MonoBehaviour
    {
        [Header("Andar")]
        [Tooltip("Velocidade maxima, em tiles por segundo.")]
        [SerializeField] private float maxSpeed = 5.5f;

        [Tooltip("Segundos ate atingir a velocidade maxima. Maior = mais pesado pra engrenar.")]
        [SerializeField] private float accelTime = 0.22f;

        [Tooltip("Segundos ate parar por completo. Maior = derrapa mais ao soltar a direcao.")]
        [SerializeField] private float brakeTime = 0.30f;

        [Header("Pular")]
        [Tooltip("Altura do pulo em tiles. A velocidade inicial e calculada a partir daqui, entao esse valor e literal.")]
        [SerializeField] private float jumpHeight = 3.0f;

        [Tooltip("Gravidade base. Alta de proposito: o Scrap e de ferro.")]
        [SerializeField] private float gravityScale = 3.6f;

        [Tooltip("Multiplica a gravidade durante a queda. Cair mais rapido do que sobe tira a sensacao de flutuar.")]
        [SerializeField] private float fallGravityMultiplier = 1.5f;

        [Tooltip("Soltar o botao no meio da subida corta o pulo por este fator. 1 = sem corte.")]
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.45f;

        [Tooltip("Janela em que ainda da pra pular depois de ja ter saido da plataforma.")]
        [SerializeField] private float coyoteTime = 0.10f;

        [Tooltip("Janela em que um pulo apertado cedo demais ainda vale quando encostar no chao.")]
        [SerializeField] private float jumpBufferTime = 0.12f;

        [Header("Chao")]
        [Tooltip("Transform vazio nos pes do Scrap. O gerador cria como filho 'GroundCheck'.")]
        [SerializeField] private Transform groundCheck;

        [SerializeField] private float groundCheckRadius = 0.18f;

        [Tooltip("Quais layers contam como chao. O gerador aponta pra layer 'Ground'.")]
        [SerializeField] private LayerMask groundLayers = ~0;

        [Header("Wall Grip")]
        [Tooltip("Velocidade maxima de queda enquanto ele segura numa parede. Menor = gruda mais.")]
        [SerializeField] private float wallSlideSpeed = 1.6f;

        [Tooltip("Empurrao pra longe da parede ao pular dela, em tiles por segundo.")]
        [SerializeField] private float wallJumpPush = 7f;

        [Tooltip("Altura do pulo de parede, como fracao do pulo normal.")]
        [SerializeField, Range(0.5f, 1.2f)] private float wallJumpHeight = 1.0f;

        [Tooltip("Por quanto tempo depois do pulo de parede ele ignora a direcao. E o que impede de voltar na hora pra mesma parede sem ganhar altura.")]
        [SerializeField] private float wallJumpLockTime = 0.16f;

        [Tooltip("Janela em que ainda da pra pular depois de ja ter soltado a parede.")]
        [SerializeField] private float wallCoyoteTime = 0.10f;

        private Rigidbody2D _body;
        private SpriteRenderer _sprite;

        private float _moveInput;
        private float _coyoteCounter;
        private float _jumpBufferCounter;
        private bool _jumpHeld;
        private bool _jumpReleasedThisFrame;
        private bool _isGrounded;

        // -1 parede a esquerda, +1 a direita, 0 nenhuma.
        private int _wallSide;
        private int _lastWallSide;
        private float _wallCoyoteCounter;
        private bool _wallSliding;
        private float _steerLock;
        private int _facing = 1;
        private bool _airJumpUsed;

        // Mercurio: enquanto ele pisa numa poca, escorrega; se ela tem
        // correnteza, e arrastado. Gota grudada no corpo deixa ele lento.
        private float _mercuryUntil;
        private float _current;

        private static bool _pressureJump;

        /// <summary>Ate quando o Scrap esta lento por causa de mercurio grudado. O HUD usa pro aviso.</summary>
        public static float SlowedUntil { get; private set; } = -100f;

        /// <summary>
        /// Se o Scrap ja tem o Pressure Jump: um segundo pulo no ar, com uma
        /// rajada de vapor dos pistoes. Quem da e o Boiler Master, ao cair.
        /// </summary>
        public static bool HasPressureJump => _pressureJump;

        /// <summary>Quando ele foi conquistado. O HUD usa pra mostrar o aviso.</summary>
        public static float PressureJumpTime { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _pressureJump = false;
            PressureJumpTime = -100f;
            SlowedUntil = -100f;
        }

        /// <summary>
        /// Chamado a cada passo de fisica pela poca de mercurio em que ele
        /// pisa. <paramref name="current"/> e a correnteza, em tiles por
        /// segundo (0 numa poca parada).
        /// </summary>
        public void OnMercury(float current)
        {
            _mercuryUntil = Time.time + 0.1f;
            _current = current;
        }

        /// <summary>Verdadeiro enquanto ele pisa em mercurio.</summary>
        public bool IsOnMercury => Time.time < _mercuryUntil;

        /// <summary>Mercurio grudou no corpo: ele anda mais devagar por um tempo.</summary>
        public void Slow(float seconds)
        {
            SlowedUntil = Mathf.Max(SlowedUntil, Time.time + seconds);
        }

        /// <summary>Da o Pressure Jump sem aviso na tela: pra cenas em que ele ja chega com ele.</summary>
        public static void UnlockPressureJumpSilently()
        {
            _pressureJump = true;
        }

        public static void UnlockPressureJump()
        {
            if (_pressureJump) return;

            _pressureJump = true;
            PressureJumpTime = Time.time;
        }

        /// <summary>Verdadeiro enquanto o GroundCheck encosta em algo da layer de chao.</summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>Verdadeiro enquanto ele escorrega segurando numa parede.</summary>
        public bool IsWallSliding => _wallSliding;

        /// <summary>Pra que lado ele esta virado: -1 esquerda, +1 direita.</summary>
        public int Facing => _facing;

        /// <summary>
        /// Trava o controle: ele para de andar e nao pula. Usado enquanto raspa
        /// uma alavanca — e o que deixa ele vulneravel.
        /// </summary>
        public bool ControlLocked { get; set; }

        /// <summary>
        /// Joga o Scrap com uma velocidade e deixa ele sem direcao por um
        /// instante, pra o empurrao nao ser desfeito no quadro seguinte.
        /// </summary>
        public void Knockback(Vector2 velocity, float lockTime)
        {
            _body.linearVelocity = velocity;
            _steerLock = Mathf.Max(_steerLock, lockTime);
        }

        /// <summary>Velocidade horizontal atual, normalizada pela maxima. Util pro Animator depois.</summary>
        public float NormalizedSpeed => maxSpeed <= 0f ? 0f : Mathf.Abs(_body.linearVelocity.x) / maxSpeed;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _sprite = GetComponentInChildren<SpriteRenderer>();

            _body.gravityScale = gravityScale;
            _body.freezeRotation = true;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            ReadInput();

            _isGrounded = groundCheck != null
                && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);

            // Coyote time: recarrega no chao, escorre no ar.
            _coyoteCounter = _isGrounded ? coyoteTime : _coyoteCounter - Time.deltaTime;
            _jumpBufferCounter -= Time.deltaTime;
            _steerLock -= Time.deltaTime;

            UpdateWall();

            // O pulo do ar volta ao pisar no chao ou encostar numa parede.
            if (_isGrounded || _wallSide != 0) _airJumpUsed = false;

            if (!Mathf.Approximately(_moveInput, 0f)) _facing = _moveInput < 0f ? -1 : 1;

            // Na parede ele olha pra fora dela.
            if (_wallSliding) _facing = -_wallSide;

            if (_sprite != null) _sprite.flipX = _facing < 0;
        }

        /// <summary>
        /// Ve se ha parede encostada de algum lado, na altura do corpo. So vale
        /// no ar: no chao, parede e so parede.
        /// </summary>
        private void UpdateWall()
        {
            _wallSide = 0;

            if (!_isGrounded)
            {
                Vector2 center = (Vector2)transform.position + new Vector2(0f, 0.65f);
                var size = new Vector2(0.14f, 0.7f);

                if (Physics2D.OverlapBox(center + new Vector2(0.46f, 0f), size, 0f, groundLayers)) _wallSide = 1;
                else if (Physics2D.OverlapBox(center + new Vector2(-0.46f, 0f), size, 0f, groundLayers)) _wallSide = -1;
            }

            if (_wallSide != 0)
            {
                _lastWallSide = _wallSide;
                _wallCoyoteCounter = wallCoyoteTime;
            }
            else
            {
                _wallCoyoteCounter -= Time.deltaTime;
            }

            // Segura quem empurra contra a parede enquanto cai.
            _wallSliding = _wallSide != 0
                && _body.linearVelocity.y <= 0f
                && Mathf.Abs(_moveInput) > 0.1f
                && Mathf.Sign(_moveInput) == _wallSide;
        }

        private void FixedUpdate()
        {
            ApplyHorizontal();
            ApplyJump();
            ApplyGravity();

            // Consumido aqui pra o corte de pulo acontecer uma vez so, e nao a cada passo de fisica.
            _jumpReleasedThisFrame = false;
        }

        private void ReadInput()
        {
            float axis = 0f;
            bool jumpPressed = false;
            bool jumpHeldNow = false;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) axis -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) axis += 1f;

                jumpPressed |= keyboard.spaceKey.wasPressedThisFrame
                    || keyboard.wKey.wasPressedThisFrame
                    || keyboard.upArrowKey.wasPressedThisFrame;

                jumpHeldNow |= keyboard.spaceKey.isPressed
                    || keyboard.wKey.isPressed
                    || keyboard.upArrowKey.isPressed;
            }

            Gamepad pad = Gamepad.current;
            if (pad != null)
            {
                float stick = pad.leftStick.x.ReadValue();
                if (Mathf.Abs(stick) > 0.2f) axis += stick;
                if (pad.dpad.left.isPressed) axis -= 1f;
                if (pad.dpad.right.isPressed) axis += 1f;

                jumpPressed |= pad.buttonSouth.wasPressedThisFrame;
                jumpHeldNow |= pad.buttonSouth.isPressed;
            }

            if (ControlLocked)
            {
                axis = 0f;
                jumpPressed = false;
                jumpHeldNow = false;
            }

            _moveInput = Mathf.Clamp(axis, -1f, 1f);

            if (_jumpHeld && !jumpHeldNow) _jumpReleasedThisFrame = true;
            _jumpHeld = jumpHeldNow;

            if (jumpPressed) _jumpBufferCounter = jumpBufferTime;
        }

        private void ApplyHorizontal()
        {
            // Logo depois de um pulo de parede ou de apanhar, o embalo manda.
            if (_steerLock > 0f) return;

            float top = maxSpeed * (Time.time < SlowedUntil ? 0.55f : 1f);
            float target = _moveInput * top;

            // Acelerar e frear tem taxas separadas. E essa diferenca que da o peso:
            // o Scrap custa a engrenar e custa a parar.
            float rate = Mathf.Approximately(target, 0f)
                ? maxSpeed / Mathf.Max(brakeTime, 0.01f)
                : maxSpeed / Mathf.Max(accelTime, 0.01f);

            // Ferro flutua em mercurio, mas nao tem onde firmar o pe: ele anda
            // um pouco mais devagar, custa muito a engrenar e mais ainda a
            // parar. A correnteza entra por cima de tudo.
            if (IsOnMercury && _isGrounded)
            {
                rate = Mathf.Approximately(target, 0f) ? maxSpeed / 1.5f : maxSpeed / 0.8f;
                target = target * 0.85f + _current;
            }

            Vector2 velocity = _body.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, target, rate * Time.fixedDeltaTime);
            _body.linearVelocity = velocity;
        }

        private void ApplyJump()
        {
            if (_jumpBufferCounter > 0f && _coyoteCounter > 0f)
            {
                // v = raiz(2 * g * h), entao jumpHeight vale literalmente em tiles,
                // mesmo se voce mexer na gravidade depois.
                float gravity = Mathf.Abs(Physics2D.gravity.y) * gravityScale;
                float launchSpeed = Mathf.Sqrt(2f * gravity * Mathf.Max(jumpHeight, 0.01f));

                Vector2 velocity = _body.linearVelocity;
                velocity.y = launchSpeed;
                _body.linearVelocity = velocity;

                _jumpBufferCounter = 0f;
                _coyoteCounter = 0f;
                return;
            }

            // Pulo de parede: sai na diagonal, pra longe dela.
            if (_jumpBufferCounter > 0f && !_isGrounded && _wallCoyoteCounter > 0f && _lastWallSide != 0)
            {
                float gravity = Mathf.Abs(Physics2D.gravity.y) * gravityScale;
                float launchSpeed = Mathf.Sqrt(2f * gravity * Mathf.Max(jumpHeight * wallJumpHeight, 0.01f));

                _body.linearVelocity = new Vector2(-_lastWallSide * wallJumpPush, launchSpeed);

                _steerLock = wallJumpLockTime;
                _jumpBufferCounter = 0f;
                _wallCoyoteCounter = 0f;
                _facing = -_lastWallSide;
                return;
            }

            // Pressure Jump: um pulo a mais, no ar, um pouco mais baixo.
            if (_jumpBufferCounter > 0f && !_isGrounded && _pressureJump && !_airJumpUsed)
            {
                float gravity = Mathf.Abs(Physics2D.gravity.y) * gravityScale;
                float launchSpeed = Mathf.Sqrt(2f * gravity * Mathf.Max(jumpHeight * 0.85f, 0.01f));

                Vector2 velocity = _body.linearVelocity;
                velocity.y = launchSpeed;
                _body.linearVelocity = velocity;

                _airJumpUsed = true;
                _jumpBufferCounter = 0f;
                return;
            }

            if (_jumpReleasedThisFrame && _body.linearVelocity.y > 0f)
            {
                Vector2 velocity = _body.linearVelocity;
                velocity.y *= jumpCutMultiplier;
                _body.linearVelocity = velocity;
            }
        }

        private void ApplyGravity()
        {
            _body.gravityScale = _body.linearVelocity.y < 0f
                ? gravityScale * fallGravityMultiplier
                : gravityScale;

            // Segurando na parede ele nao cai mais rapido que isso.
            if (_wallSliding && _body.linearVelocity.y < -wallSlideSpeed)
            {
                Vector2 velocity = _body.linearVelocity;
                velocity.y = -wallSlideSpeed;
                _body.linearVelocity = velocity;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;

            Gizmos.color = Application.isPlaying && _isGrounded ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
