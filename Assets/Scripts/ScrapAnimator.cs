using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Escolhe o quadro do Scrap a cada instante, olhando o que ele esta
    /// fazendo: parado, andando, no ar, na parede, batendo, no dash, apanhando.
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
        public const int IdleFrames = 4;
        public const int Walk = 4;
        public const int WalkFrames = 4;
        public const int Jump = 8;
        public const int Fall = 9;
        public const int Wall = 10;
        public const int Windup = 11;
        public const int Strike = 12;
        public const int Dash = 13;
        public const int Hurt = 14;

        [Tooltip("Os quadros da sucata (Fase 1), na ordem das constantes.")]
        [SerializeField] private Sprite[] scrapFrames;

        [Tooltip("Os quadros do polido (Fase 2), na mesma ordem.")]
        [SerializeField] private Sprite[] polishedFrames;

        [SerializeField] private float idleFramesPerSecond = 3f;
        [SerializeField] private float walkFramesPerSecond = 9f;

        private ScrapController _controller;
        private ScrapChisel _chisel;
        private ScrapDash _dash;
        private ScrapHealth _health;
        private Rigidbody2D _body;
        private SpriteRenderer _renderer;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite[] scrap, Sprite[] polished)
        {
            scrapFrames = scrap;
            polishedFrames = polished;
        }

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
            Sprite[] frames = ScrapAppearance.IsPolished ? polishedFrames : scrapFrames;
            if (frames == null || frames.Length == 0) return;

            int frame = Mathf.Clamp(PickFrame(), 0, frames.Length - 1);
            if (frames[frame] != null) _renderer.sprite = frames[frame];
        }

        private int PickFrame()
        {
            // O que interrompe tudo vem primeiro.
            if (_health != null && Time.time - _health.LastHitTime < 0.3f) return Hurt;
            if (_dash != null && _dash.IsDashing) return Dash;

            if (_chisel != null)
            {
                if (_chisel.IsStriking) return Strike;
                if (_chisel.IsWindingUp) return Windup;
            }

            if (_controller.IsWallSliding) return Wall;

            if (!_controller.IsGrounded) return _body.linearVelocity.y > 0.5f ? Jump : Fall;

            if (_controller.NormalizedSpeed > 0.12f)
            {
                return Walk + (int)(Time.time * walkFramesPerSecond) % WalkFrames;
            }

            return Idle + (int)(Time.time * idleFramesPerSecond) % IdleFrames;
        }
    }
}
