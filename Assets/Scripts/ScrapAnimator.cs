using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Escolhe o quadro do Scrap a cada instante, olhando o que ele esta
    /// fazendo: parado (respirando e piscando de vez em quando), andando, no
    /// ar (subindo, no alto, caindo), aterrissando, na parede, preparando e
    /// dando o golpe (de lado, pra cima ou pra baixo), no dash, apanhando.
    ///
    /// Nao usa Animator nem AnimationClip: sao so duas listas de sprites na
    /// mesma ordem, uma da sucata e uma do polido. Pra trocar a arte, basta
    /// trocar os sprites das listas mantendo a ordem (ver as constantes).
    /// </summary>
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ScrapAnimator : MonoBehaviour
    {
        // A ordem dos quadros nas duas listas. E a mesma do HeroArt, no editor.
        public const int Idle = 0;
        public const int IdleFrames = 8;
        public const int Walk = 8;
        public const int WalkFrames = 8;
        public const int Jump = 16;
        public const int Apex = 17;
        public const int Fall = 18;
        public const int FallFrames = 2;
        public const int Land = 20;
        public const int Wall = 21;
        public const int Windup = 22;
        public const int WindupFrames = 2;
        public const int Strike = 24;
        public const int StrikeFrames = 3;
        public const int StrikeUp = 27;
        public const int StrikeUpFrames = 2;
        public const int StrikeDown = 29;
        public const int StrikeDownFrames = 2;
        public const int Dash = 31;
        public const int DashFrames = 2;
        public const int Hurt = 33;

        /// <summary>O quadro de olhos fechados, dentro do ciclo de parado.</summary>
        private const int BlinkFrame = Idle + 5;

        [Tooltip("Os quadros da sucata, na ordem das constantes.")]
        [SerializeField] private Sprite[] scrapFrames;

        [Tooltip("Os quadros do polido, na mesma ordem.")]
        [SerializeField] private Sprite[] polishedFrames;

        [SerializeField] private float idleFramesPerSecond = 6f;
        [SerializeField] private float walkFramesPerSecond = 13f;

        [Tooltip("Quanto dura o quadro de aterrissagem, em segundos.")]
        [SerializeField] private float landTime = 0.1f;

        [Tooltip("A queda mais lenta que ainda mostra a aterrissagem, em tiles por segundo.")]
        [SerializeField] private float landSpeed = 6f;

        private ScrapController _controller;
        private ScrapChisel _chisel;
        private ScrapDash _dash;
        private ScrapHealth _health;
        private Rigidbody2D _body;
        private SpriteRenderer _renderer;

        private bool _wasGrounded = true;
        private float _fallSpeed;
        private float _landUntil = -100f;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite[] scrap, Sprite[] polished)
        {
            scrapFrames = scrap;
            polishedFrames = polished;
        }

        /// <summary>
        /// Os quadros da aparencia atual (sucata ou polido). Os cards de
        /// habilidade e as placas de tutorial usam pra animar o proprio Scrap.
        /// </summary>
        public Sprite[] CurrentFrames => ScrapAppearance.IsPolished ? polishedFrames : scrapFrames;

        private void Awake()
        {
            _controller = GetComponent<ScrapController>();
            _chisel = GetComponent<ScrapChisel>();
            _dash = GetComponent<ScrapDash>();
            _health = GetComponent<ScrapHealth>();
            _body = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        // No LateUpdate, depois de todo mundo decidir o que aconteceu no quadro.
        private void LateUpdate()
        {
            TrackLanding();

            Sprite[] frames = ScrapAppearance.IsPolished ? polishedFrames : scrapFrames;
            if (frames == null || frames.Length == 0) return;

            int frame = Mathf.Clamp(PickFrame(), 0, frames.Length - 1);
            if (frames[frame] != null) _renderer.sprite = frames[frame];
        }

        /// <summary>Guarda a velocidade da queda e, ao tocar o chao depois de uma queda de verdade, marca a aterrissagem.</summary>
        private void TrackLanding()
        {
            bool grounded = _controller.IsGrounded;

            if (!grounded) _fallSpeed = Mathf.Min(_fallSpeed, _body.linearVelocity.y);

            if (grounded && !_wasGrounded && -_fallSpeed >= landSpeed) _landUntil = Time.time + landTime;
            if (grounded) _fallSpeed = 0f;

            _wasGrounded = grounded;
        }

        private int PickFrame()
        {
            // O que interrompe tudo vem primeiro.
            if (_health != null && Time.time - _health.LastHitTime < 0.3f) return Hurt;
            if (_dash != null && _dash.IsDashing) return Dash + (int)(Time.time * 16f) % DashFrames;

            if (_chisel != null)
            {
                int attack = AttackFrame();
                if (attack >= 0) return attack;
            }

            if (_controller.IsWallSliding) return Wall;

            if (!_controller.IsGrounded)
            {
                float vy = _body.linearVelocity.y;
                if (vy > 2.5f) return Jump;
                if (vy > -2.5f) return Apex;
                return Fall + (int)(Time.time * 8f) % FallFrames;
            }

            if (Time.time < _landUntil) return Land;

            if (_controller.NormalizedSpeed > 0.12f)
            {
                float rate = walkFramesPerSecond * Mathf.Max(0.6f, _controller.NormalizedSpeed);
                return Walk + (int)(Time.time * rate) % WalkFrames;
            }

            // Parado: respira, e so pisca de vez em quando (um ciclo em tres).
            int idle = Idle + (int)(Time.time * idleFramesPerSecond) % IdleFrames;
            float cycle = IdleFrames / idleFramesPerSecond;
            if (idle == BlinkFrame && (int)(Time.time / cycle) % 3 != 0) idle = BlinkFrame - 1;
            return idle;
        }

        /// <summary>O quadro do golpe, pelo relogio do cinzel, ou -1 se ele nao esta golpeando.</summary>
        private int AttackFrame()
        {
            Vector2 direction = _chisel.StrikeDirection;

            if (_chisel.IsWindingUp)
            {
                // Pra cima e pra baixo o preparo e o primeiro quadro do proprio golpe.
                if (direction.y > 0f) return StrikeUp;
                if (direction.y < 0f) return StrikeDown;

                float t = (Time.time - _chisel.WindupStartTime) / Mathf.Max(0.01f, _chisel.WindupDuration);
                return Windup + Mathf.Clamp((int)(t * WindupFrames), 0, WindupFrames - 1);
            }

            if (_chisel.IsStriking)
            {
                float t = (Time.time - _chisel.StrikeStartTime) / Mathf.Max(0.01f, _chisel.StrikeDuration);

                if (direction.y > 0f) return StrikeUp + Mathf.Clamp(1 + (int)(t * 2f), 1, StrikeUpFrames - 1);
                if (direction.y < 0f) return StrikeDown + Mathf.Clamp(1 + (int)(t * 2f), 1, StrikeDownFrames - 1);

                return Strike + Mathf.Clamp((int)(t * StrikeFrames), 0, StrikeFrames - 1);
            }

            // O acompanhamento: o golpe de lado termina com a lamina embaixo, um instante.
            if (direction.y == 0f && Time.time - _chisel.StrikeEndTime < 0.1f) return Strike + StrikeFrames - 1;

            return -1;
        }
    }
}
