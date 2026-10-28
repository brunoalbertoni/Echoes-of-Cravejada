using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapDash : MonoBehaviour
    {
        [SerializeField] private float speed = 17f;
        [SerializeField] private float duration = 0.20f;
        [SerializeField] private float cooldown = 0.9f;

        [Tooltip("A cor que ele vai pegando enquanto o dash recarrega.")]
        [SerializeField] private Color heatColor = new Color(1f, 0.45f, 0.35f);

        [Header("Intangibilidade")]
        [SerializeField] private string normalLayerName = "Player";
        [SerializeField] private string intangibleLayerName = "PlayerIntangible";
        [SerializeField, Range(0.1f, 1f)] private float dashAlpha = 0.45f;

        private ScrapController _controller;
        private Rigidbody2D _body;
        private SpriteRenderer _sprite;

        private float _dashUntil;
        private float _readyAt;
        private int _direction;
        private bool _usedInAir;

        private int _normalLayer;
        private int _intangibleLayer;
        private bool _wasDashing;

        private static bool _unlocked;

        private float Cooldown => cooldown * Shop.DashCooldownScale;
        public bool IsDashing => Time.time < _dashUntil;
        public static bool Unlocked => _unlocked;
        public static float UnlockTime { get; private set; } = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _unlocked = false;
            UnlockTime = -100f;
        }

        public void Refresh()
        {
            if (IsDashing) return;
            _readyAt = 0f;
            _usedInAir = false;
        }

        public static void UnlockSilently() => _unlocked = true;

        public static void Unlock()
        {
            if (_unlocked) return;
            _unlocked = true;
            UnlockTime = Time.time;
        }

        [SerializeField] private bool unlockedFromStart;

        public void SetUnlockedFromStart(bool unlocked) => unlockedFromStart = unlocked;

        private void Awake()
        {
            if (unlockedFromStart) _unlocked = true;

            _controller = GetComponent<ScrapController>();
            _body = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();

            _normalLayer = LayerMask.NameToLayer(normalLayerName);
            _intangibleLayer = LayerMask.NameToLayer(intangibleLayerName);

            if (_normalLayer == -1) _normalLayer = gameObject.layer;
            if (_intangibleLayer == -1) _intangibleLayer = _normalLayer;
        }

        private void Update()
        {
            if (_controller.IsGrounded) _usedInAir = false;

            UpdateDashState();

            if (_sprite != null && _unlocked)
            {
                if (IsDashing)
                {
                    Color c = Color.white;
                    c.a = dashAlpha;
                    _sprite.color = c;
                }
                else
                {
                    float heat = Mathf.Clamp01((_readyAt - Time.time) / Mathf.Max(0.1f, Cooldown));
                    Color c = Color.Lerp(Color.white, heatColor, heat);
                    c.a = 1f;
                    _sprite.color = c;
                }
            }

            if (!_unlocked || _controller.ControlLocked) return;
            if (Time.time < _readyAt || _usedInAir) return;
            if (!DashPressed()) return;

            _direction = _controller.Facing;
            _dashUntil = Time.time + duration;
            _readyAt = Time.time + Cooldown;
            if (!_controller.IsGrounded) _usedInAir = true;

            _controller.Knockback(new Vector2(_direction * speed, 0f), duration);
        }

        private void UpdateDashState()
        {
            bool dashingNow = IsDashing;

            if (dashingNow && !_wasDashing)
            {
                gameObject.layer = _intangibleLayer;
                _wasDashing = true;
            }
            else if (!dashingNow && _wasDashing)
            {
                gameObject.layer = _normalLayer;
                _wasDashing = false;
            }
        }

        private void FixedUpdate()
        {
            if (Time.time >= _dashUntil) return;
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