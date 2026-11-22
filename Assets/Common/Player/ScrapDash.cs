using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapDash : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [SerializeField] private float velocidade = 17f;
        [UnityEngine.Serialization.FormerlySerializedAs("duration")] [SerializeField] private float duracao = 0.20f;
        [UnityEngine.Serialization.FormerlySerializedAs("cooldown")] [SerializeField] private float recarga = 0.9f;

        [UnityEngine.Serialization.FormerlySerializedAs("heatColor")] [Tooltip("A cor que ele vai pegando enquanto o dash recarrega.")]
        [SerializeField] private Color corCalor = new Color(1f, 0.45f, 0.35f);

        [UnityEngine.Serialization.FormerlySerializedAs("normalLayerName")] [Header("Intangibilidade")]
        [SerializeField] private string nomeCamadaNormal = "Player";
        [UnityEngine.Serialization.FormerlySerializedAs("intangibleLayerName")] [SerializeField] private string nomeCamadaIntangivel = "PlayerIntangible";
        [UnityEngine.Serialization.FormerlySerializedAs("dashAlpha")] [SerializeField, Range(0.1f, 1f)] private float alfaDash = 0.45f;

        private ScrapController _controlador;
        private Rigidbody2D _corpo;
        private SpriteRenderer _sprite;

        private float _dashAte;
        private float _prontoEm;
        private int _direcao;
        private bool _usadoDentroAr;

        private int _camadaNormal;
        private int _camadaIntangivel;
        private bool _estavaDashando;

        [SaveProgress("_unlocked")]
        private static bool _desbloqueado;

        private float Recarga => recarga * Shop.EscalaRecargaDash;
        public bool EstaDashando => Time.time < _dashAte;
        public static bool Desbloqueado => _desbloqueado;
        public static float TempoDesbloquear { get; private set; } = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _desbloqueado = false;
            TempoDesbloquear = -100f;
        }

        public void Atualizar()
        {
            if (EstaDashando) return;
            _prontoEm = 0f;
            _usadoDentroAr = false;
        }

        public static void DesbloquearEmSilencio() => _desbloqueado = true;

        public static void Desbloquear()
        {
            if (_desbloqueado) return;
            _desbloqueado = true;
            TempoDesbloquear = Time.time;
            AbilityCard.Mostrar(ScrapMove.DashVapor);
        }

        [UnityEngine.Serialization.FormerlySerializedAs("unlockedFromStart")] [SerializeField] private bool desbloqueadoDeInicio;

        public void DefinirLiberadoDesdeInicio(bool desbloqueado) => desbloqueadoDeInicio = desbloqueado;

        private void Awake()
        {
            if (desbloqueadoDeInicio) _desbloqueado = true;

            _controlador = GetComponent<ScrapController>();
            _corpo = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();

            _camadaNormal = LayerMask.NameToLayer(nomeCamadaNormal);
            _camadaIntangivel = LayerMask.NameToLayer(nomeCamadaIntangivel);

            if (_camadaNormal == -1) _camadaNormal = gameObject.layer;
            if (_camadaIntangivel == -1) _camadaIntangivel = _camadaNormal;
        }

        private void Update()
        {
            if (_controlador.EstaNoChao) _usadoDentroAr = false;

            AtualizarEstadoDoDash();

            if (_sprite != null && _desbloqueado)
            {
                if (EstaDashando)
                {
                    Color c = Color.white;
                    c.a = alfaDash;
                    _sprite.color = c;
                }
                else
                {
                    float calor = Mathf.Clamp01((_prontoEm - Time.time) / Mathf.Max(0.1f, Recarga));
                    Color c = Color.Lerp(Color.white, corCalor, calor);
                    c.a = 1f;
                    _sprite.color = c;
                }
            }

            if (!_desbloqueado || _controlador.ControleTravado) return;
            if (Time.time < _prontoEm || _usadoDentroAr) return;
            if (!DashPressionado()) return;

            _direcao = _controlador.Virado;
            _dashAte = Time.time + duracao;
            _prontoEm = Time.time + Recarga;
            if (!_controlador.EstaNoChao) _usadoDentroAr = true;

            _controlador.Empurrar(new Vector2(_direcao * velocidade, 0f), duracao);
            Sfx.QuadroDash.Tocar();
        }

        private void AtualizarEstadoDoDash()
        {
            bool agoraDashando = EstaDashando;

            if (agoraDashando && !_estavaDashando)
            {
                gameObject.layer = _camadaIntangivel;
                _estavaDashando = true;
            }
            else if (!agoraDashando && _estavaDashando)
            {
                gameObject.layer = _camadaNormal;
                _estavaDashando = false;
            }
        }

        private void FixedUpdate()
        {
            if (Time.time >= _dashAte) return;
            _corpo.linearVelocity = new Vector2(_direcao * velocidade, 0f);
        }

        private static bool DashPressionado()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.leftShiftKey.wasPressedThisFrame || teclado.lKey.wasPressedThisFrame)) return true;

            Gamepad controle = Gamepad.current;
            return controle != null && controle.rightShoulder.wasPressedThisFrame;
        }
    }
}