using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Scrap Chisel: o golpe do Scrap. Lento e pesado de proposito — a
    /// ferramenta e grande demais pra ele, entao ha um instante de preparo
    /// antes de acertar e uma pausa depois.
    ///
    /// Bate pro lado em que ele olha, pra cima (segurando cima) ou pra baixo
    /// (segurando baixo, so no ar). Acertar qualquer coisa por baixo faz ele
    /// quicar: e o pogo, que serve pra cruzar espinhos e ficar em cima de inimigo.
    ///
    /// Teclas: J, X ou botao esquerdo do mouse; no controle, o botao da esquerda.
    ///
    /// Depois do Geode Drill ele ganha o Resonance Strike: segurar o botao do
    /// golpe ate o cinzel vibrar e soltar. Sai uma onda sonora em volta dele,
    /// que fere mais, acende uma area grande e quebra barreiras de cristal.
    /// </summary>
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapChisel : MonoBehaviour
    {
        [SerializeField] private int damage = 1;

        [Tooltip("Preparo antes de o golpe acertar. E daqui que vem o peso.")]
        [SerializeField] private float windup = 0.12f;

        [Tooltip("Por quanto tempo o risco do golpe fica na tela.")]
        [SerializeField] private float swingTime = 0.14f;

        [Tooltip("Pausa ate poder bater de novo.")]
        [SerializeField] private float recovery = 0.30f;

        [Tooltip("Velocidade pra cima ao acertar algo por baixo.")]
        [SerializeField] private float pogoSpeed = 12.5f;

        [Tooltip("Recuo ao acertar algo de lado.")]
        [SerializeField] private float recoil = 3f;

        [Tooltip("O risco do golpe: um SpriteRenderer filho, desligado.")]
        [SerializeField] private SpriteRenderer slash;

        [Header("Resonance Strike")]
        [Tooltip("Segundos segurando o botao ate o cinzel carregar.")]
        [SerializeField] private float chargeTime = 0.75f;

        [Tooltip("Ate onde a onda alcanca, em tiles.")]
        [SerializeField] private float resonanceRadius = 3.5f;

        [SerializeField] private int resonanceDamage = 2;

        private ScrapController _controller;
        private Rigidbody2D _body;
        private bool _busy;
        private float _holdStart = -1f;
        private UnityEngine.Rendering.Universal.Light2D _eyes;
        private float _eyesIntensity;
        private Color _eyesColor;

        private static bool _resonance;

        /// <summary>Se o Scrap ja tem o Resonance Strike. Quem da e o Geode Drill, ao parar.</summary>
        public static bool HasResonanceStrike => _resonance;

        /// <summary>Quando ele foi conquistado. O HUD usa pra mostrar o aviso.</summary>
        public static float ResonanceTime { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _resonance = false;
            ResonanceTime = -100f;
        }

        /// <summary>Da o Resonance Strike sem aviso na tela: pra cenas em que ele ja chega com ele.</summary>
        public static void UnlockResonanceStrikeSilently()
        {
            _resonance = true;
        }

        public static void UnlockResonanceStrike()
        {
            if (_resonance) return;

            _resonance = true;
            ResonanceTime = Time.time;
            AbilityCard.Show(ScrapMove.ResonanceStrike);
        }

        /// <summary>Verdadeiro enquanto o cinzel esta carregado, esperando soltar.</summary>
        public bool IsCharged { get; private set; }

        /// <summary>Verdadeiro durante o preparo do golpe.</summary>
        public bool IsWindingUp { get; private set; }

        /// <summary>Verdadeiro enquanto o golpe esta acertando.</summary>
        public bool IsStriking { get; private set; }

        /// <summary>Pra onde foi o ultimo golpe: o lado, cima ou baixo. O ScrapAnimator escolhe os quadros por ela.</summary>
        public Vector2 StrikeDirection { get; private set; } = Vector2.right;

        /// <summary>Quando comecaram o preparo e o golpe, e quando o golpe acabou. Sao o relogio da animacao.</summary>
        public float WindupStartTime { get; private set; } = -100f;
        public float StrikeStartTime { get; private set; } = -100f;
        public float StrikeEndTime { get; private set; } = -100f;

        public float WindupDuration => windup;
        public float StrikeDuration => swingTime;

        private readonly Collider2D[] _hits = new Collider2D[16];

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(SpriteRenderer newSlash)
        {
            slash = newSlash;
        }

        private void Awake()
        {
            _controller = GetComponent<ScrapController>();
            _body = GetComponent<Rigidbody2D>();
            if (slash != null) slash.enabled = false;

            // Os olhos dele avisam que o cinzel carregou.
            Transform eyes = transform.Find("EyeGlow");
            if (eyes != null) _eyes = eyes.GetComponent<UnityEngine.Rendering.Universal.Light2D>();

            if (_eyes != null)
            {
                _eyesIntensity = _eyes.intensity;
                _eyesColor = _eyes.color;
            }
        }

        private void Update()
        {
            bool pressed = AttackPressed();

            if (_controller.ControlLocked)
            {
                _holdStart = -1f;
                SetCharged(false);
                return;
            }

            if (_resonance)
            {
                if (pressed) _holdStart = Time.time;

                bool ready = _holdStart >= 0f && Time.time - _holdStart >= chargeTime && !_busy;

                if (!AttackHeld())
                {
                    _holdStart = -1f;
                    SetCharged(false);

                    if (ready)
                    {
                        StartCoroutine(Resonate());
                        return;
                    }
                }
                else
                {
                    SetCharged(ready);
                }
            }

            if (_busy || !pressed) return;

            StartCoroutine(Swing(AimDirection()));
        }

        private void SetCharged(bool charged)
        {
            if (IsCharged == charged) return;

            IsCharged = charged;
            if (_eyes == null) return;

            _eyes.intensity = charged ? _eyesIntensity * 1.8f + 0.4f : _eyesIntensity;
            _eyes.color = charged ? new Color(0.70f, 0.88f, 1f) : _eyesColor;
        }

        private static bool AttackHeld()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.jKey.isPressed || keyboard.xKey.isPressed)) return true;

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonWest.isPressed;
        }

        /// <summary>
        /// O Resonance Strike: uma onda em volta dele. Fere o que estiver no
        /// raio, estoura barreira de cristal e faz o maior barulho que ele tem.
        /// </summary>
        private IEnumerator Resonate()
        {
            _busy = true;
            IsStriking = true;
            StrikeDirection = new Vector2(_controller.Facing, 0f);
            StrikeStartTime = Time.time;

            Vector2 center = (Vector2)transform.position + Vector2.up * 0.6f;

            NoiseSystem.Emit(center, 22f, 1f);

            if (slash != null)
            {
                ResonanceRing.Spawn(center, slash.sprite, resonanceRadius, slash.sharedMaterial);
            }

            foreach (Collider2D hit in Physics2D.OverlapCircleAll(center, resonanceRadius))
            {
                if (hit == null || hit.transform.IsChildOf(transform)) continue;

                var barrier = hit.GetComponentInParent<CrystalBarrier>();
                if (barrier != null)
                {
                    barrier.Shatter();
                    continue;
                }

                var damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable != null) damageable.Damage(resonanceDamage + ScrapGems.BonusDamage, center);
            }

            yield return new WaitForSeconds(swingTime);
            IsStriking = false;
            StrikeEndTime = Time.time;

            yield return new WaitForSeconds(recovery);
            _busy = false;
        }

        private static bool AttackPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.jKey.wasPressedThisFrame || keyboard.xKey.wasPressedThisFrame)) return true;

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonWest.wasPressedThisFrame;
        }

        /// <summary>Pra onde o golpe vai: cima, baixo (so no ar) ou o lado em que ele olha.</summary>
        private Vector2 AimDirection()
        {
            float vertical = 0f;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
            }

            Gamepad pad = Gamepad.current;
            if (pad != null)
            {
                float stick = pad.leftStick.y.ReadValue();
                if (Mathf.Abs(stick) > 0.5f) vertical += Mathf.Sign(stick);
                if (pad.dpad.down.isPressed) vertical -= 1f;
                if (pad.dpad.up.isPressed) vertical += 1f;
            }

            if (vertical < 0f && !_controller.IsGrounded) return Vector2.down;
            if (vertical > 0f) return Vector2.up;
            return new Vector2(_controller.Facing, 0f);
        }

        private IEnumerator Swing(Vector2 direction)
        {
            _busy = true;
            StrikeDirection = direction;
            IsWindingUp = true;
            WindupStartTime = Time.time;
            yield return new WaitForSeconds(windup);
            IsWindingUp = false;
            IsStriking = true;
            StrikeStartTime = Time.time;

            // A caixa do golpe, medida a partir dos pes.
            Vector2 center;
            Vector2 size;

            if (direction.y > 0f) { center = new Vector2(0f, 1.9f); size = new Vector2(1.1f, 1.4f); }
            else if (direction.y < 0f) { center = new Vector2(0f, -0.55f); size = new Vector2(1.0f, 1.3f); }
            else { center = new Vector2(direction.x * 1.0f, 0.65f); size = new Vector2(1.5f, 1.1f); }

            Vector2 origin = transform.position;
            ShowSlash(center, direction);

            bool struck = false;
            int count = Physics2D.OverlapBoxNonAlloc(origin + center, size, 0f, _hits);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _hits[i];
                if (hit == null || hit.transform.IsChildOf(transform)) continue;

                var damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.Damage(damage + ScrapGems.BonusDamage, origin);
                    struck = true;
                    continue;
                }

                // Espinho e acido nao apanham, mas da pra quicar neles.
                if (direction.y < 0f && hit.GetComponentInParent<Hazard>() != null) struck = true;
            }

            if (struck)
            {
                Vector2 velocity = _body.linearVelocity;

                if (direction.y < 0f) velocity.y = pogoSpeed;
                else if (direction.y == 0f) velocity.x = -direction.x * recoil;

                _body.linearVelocity = velocity;
            }

            yield return new WaitForSeconds(swingTime);
            if (slash != null) slash.enabled = false;
            IsStriking = false;
            StrikeEndTime = Time.time;

            yield return new WaitForSeconds(recovery);
            _busy = false;
        }

        private void ShowSlash(Vector2 center, Vector2 direction)
        {
            if (slash == null) return;

            slash.transform.localPosition = center;

            // O sprite do risco aponta pra direita.
            float angle = direction.y > 0f ? 90f : (direction.y < 0f ? -90f : 0f);
            slash.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            slash.flipX = direction.y == 0f && direction.x < 0f;

            // Com o rubi cravado o golpe sai em brasa.
            slash.color = ScrapGems.RubySocketed ? new Color(1f, 0.45f, 0.30f) : Color.white;
            slash.enabled = true;
        }
    }
}
