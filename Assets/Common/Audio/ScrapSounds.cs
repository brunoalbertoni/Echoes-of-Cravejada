using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Os sons do corpo do Scrap que saem de olhar o que ele esta fazendo, e
    /// nao de um momento so no codigo: os passos (no quadro em que o pe pisa),
    /// a queda (mais pesada quanto mais alto ele caiu), agarrar a parede e a
    /// lixa enquanto ele raspa uma alavanca, valvula, veio ou runa.
    ///
    /// Pulo, golpe, dash e dano tocam direto de onde acontecem.
    /// O SfxPlayer prende este componente no Scrap quando a cena abre.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class ScrapSounds : MonoBehaviour
    {
        // Os quadros da passada em que um pe assenta no chao (ver HeroArt).
        private const int StepFrameA = ScrapAnimator.Walk;
        private const int StepFrameB = ScrapAnimator.Walk + 4;

        /// <summary>Queda mais lenta que isto (tiles/s) nem faz barulho de aterrissar.</summary>
        private const float SoftLanding = 2f;

        /// <summary>A partir daqui e queda de verdade: um pulo comum chega a uns 18.</summary>
        private const float Landing = 5f;

        /// <summary>A partir daqui, queda de muito alto (umas 6 tiles ou mais).</summary>
        private const float HeavyLanding = 24f;

        private ScrapController _controller;
        private ScrapAnimator _animator;
        private Rigidbody2D _body;

        private int _lastFrame = -1;
        private bool _wasGrounded = true;
        private bool _wasOnWall;
        private float _fallSpeed;
        private float _bornAt;

        private AudioSource _scrape;
        private float _scrapeLevel;
        private Object _holdOwner;
        private float _holdProgress;
        private bool _advancing;

        private void Awake()
        {
            _controller = GetComponent<ScrapController>();
            _animator = GetComponent<ScrapAnimator>();
            _body = GetComponent<Rigidbody2D>();
            _bornAt = Time.time;

            _scrape = gameObject.AddComponent<AudioSource>();
            _scrape.clip = SfxPlayer.Load(Sfx.ScrapeLoop.Clip);
            _scrape.loop = true;
            _scrape.playOnAwake = false;
            _scrape.spatialBlend = 0f;
            _scrape.volume = 0f;

            ScrapHealth.Died += OnDied;
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= OnDied;
        }

        // Renascer na bancada nao e cair nela.
        private void OnDied()
        {
            _bornAt = Time.time;
            _fallSpeed = 0f;
        }

        private void Update()
        {
            UpdateScrape();
        }

        // Depois do ScrapAnimator escolher o quadro.
        private void LateUpdate()
        {
            if (_controller == null) return;

            UpdateSteps();
            UpdateLanding();
            UpdateWall();
        }

        private void UpdateSteps()
        {
            int frame = _animator != null ? _animator.Frame : -1;
            if (frame == _lastFrame) return;
            _lastFrame = frame;

            if (!_controller.IsGrounded || (frame != StepFrameA && frame != StepFrameB)) return;

            // Com a safira cravada ele pisa leve. No mercurio, o passo chapinha.
            float volume = Mathf.Lerp(0.6f, 1f, _controller.NormalizedSpeed) * (ScrapGems.SapphireSocketed ? 0.6f : 1f);
            (_controller.IsOnMercury ? Sfx.MercuryStep : Sfx.Step).Play(volume: volume);
        }

        private void UpdateLanding()
        {
            bool grounded = _controller.IsGrounded;

            if (!grounded && _body != null) _fallSpeed = Mathf.Min(_fallSpeed, _body.linearVelocity.y);

            // Logo que a cena abre ele assenta no chao: isso nao e queda.
            if (grounded && !_wasGrounded && Time.time - _bornAt > 0.4f)
            {
                float speed = -_fallSpeed;
                float quiet = ScrapGems.SapphireSocketed ? 0.6f : 1f;

                if (speed >= HeavyLanding) Sfx.LandHeavy.Play(volume: quiet);
                else if (speed >= Landing) Sfx.Land.Play(volume: Mathf.Lerp(0.55f, 1f, Mathf.InverseLerp(Landing, 18f, speed)) * quiet);
                else if (speed >= SoftLanding) Sfx.Step.Play(volume: 0.8f * quiet);
            }

            if (grounded) _fallSpeed = 0f;
            _wasGrounded = grounded;
        }

        private void UpdateWall()
        {
            bool onWall = _controller.IsWallSliding;
            if (onWall && !_wasOnWall) Sfx.WallGrab.Play();
            _wasOnWall = onWall;
        }

        /// <summary>
        /// A lixa: toca enquanto a barra de raspar de alguma coisa esta
        /// enchendo, e da o brilho de polido quando ela enche.
        /// </summary>
        private void UpdateScrape()
        {
            Object owner = null;
            float progress = 0f;

            if (RustyLever.Nearby != null)
            {
                owner = RustyLever.Nearby;
                progress = RustyLever.Nearby.Progress;
            }
            else if (Valve.Nearby != null)
            {
                owner = Valve.Nearby;
                progress = Valve.Nearby.Progress;
            }
            else if (HoldAction.Owner != null && !(HoldAction.Owner is HeartChoice))
            {
                // A escolha do final se segura, mas nao se raspa.
                owner = HoldAction.Owner;
                progress = HoldAction.Progress;
            }

            bool advancing = owner != null && owner == _holdOwner && progress > _holdProgress + 0.0001f;

            // Terminou: estava raspando quase no fim e a barra encheu (ou o alvo saiu, que e o que fazem ao terminar).
            bool finished = _advancing && _holdProgress >= 0.95f && (owner != _holdOwner || progress >= 0.999f);
            if (finished)
            {
                Sfx.ScrapeDone.Play();
                advancing = false;
            }

            _advancing = advancing;
            _holdOwner = owner;
            _holdProgress = progress;

            float target = advancing ? Sfx.Volume * Sfx.ScrapeLoop.Volume : 0f;
            _scrapeLevel = Mathf.MoveTowards(_scrapeLevel, target, Time.unscaledDeltaTime * (advancing ? 3f : 5f));

            if (_scrape.clip == null) return;
            _scrape.volume = _scrapeLevel;

            if (_scrapeLevel > 0.001f && !_scrape.isPlaying)
            {
                _scrape.pitch = Random.Range(0.95f, 1.05f);
                _scrape.time = Random.Range(0f, _scrape.clip.length * 0.9f);
                _scrape.Play();
            }
            else if (_scrapeLevel <= 0.001f && _scrape.isPlaying)
            {
                _scrape.Stop();
            }
        }
    }
}
