using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O barulho que o Scrap faz. Cada coisa que ele faz vira um ruido no
    /// NoiseSystem, com um alcance e uma forca:
    ///
    ///   passo e pulo        fracos: acendem um pouco em volta e nao acordam ninguem
    ///   queda               depende da altura: um degrau quase nao soa, um tombo acende a sala
    ///   golpe do cinzel     forte
    ///   Steam Dash          o mais forte de todos: vapor e pistoes
    ///
    /// Com a safira cravada, o que vem do corpo (passo, pulo e queda) soa pela
    /// metade. Golpe e dash continuam altos: sao ferramenta, nao peso.
    ///
    /// Ele anda numa velocidade so. A escolha do District 3 nao e correr ou
    /// andar: e usar ou nao o dash, o golpe e a queda — que sao o que mostra o
    /// caminho e tambem o que acorda o escuro.
    /// </summary>
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapNoise : MonoBehaviour
    {
        [Header("Passos")]
        [SerializeField] private float stepInterval = 0.34f;
        [SerializeField] private float stepRadius = 3.5f;
        [SerializeField, Range(0f, 1f)] private float stepLoudness = 0.15f;

        [Header("Pulo e queda")]
        [SerializeField] private float jumpRadius = 4f;
        [SerializeField, Range(0f, 1f)] private float jumpLoudness = 0.2f;

        [Tooltip("Alcance de uma queda bem alta. Quedas menores ficam entre o passo e isto.")]
        [SerializeField] private float landRadius = 10f;

        [Header("Golpe e dash")]
        [SerializeField] private float strikeRadius = 10f;
        [SerializeField, Range(0f, 1f)] private float strikeLoudness = 0.8f;
        [SerializeField] private float dashRadius = 14f;
        [SerializeField, Range(0f, 1f)] private float dashLoudness = 1f;

        private ScrapController _controller;
        private ScrapChisel _chisel;
        private ScrapDash _dash;
        private Rigidbody2D _body;

        private float _stepTimer;
        private bool _wasGrounded = true;
        private bool _wasStriking;
        private bool _wasDashing;
        private float _fallSpeed;

        private void Awake()
        {
            _controller = GetComponent<ScrapController>();
            _chisel = GetComponent<ScrapChisel>();
            _dash = GetComponent<ScrapDash>();
            _body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            Vector2 feet = transform.position;
            bool grounded = _controller.IsGrounded;

            // Passos: so no chao, e so andando de verdade.
            if (grounded && _controller.NormalizedSpeed > 0.15f)
            {
                _stepTimer -= Time.deltaTime;

                if (_stepTimer <= 0f)
                {
                    _stepTimer = stepInterval;
                    NoiseSystem.Emit(feet, stepRadius, stepLoudness * ScrapGems.BodyNoise);
                }
            }
            else
            {
                _stepTimer = 0f;
            }

            // Sair do chao subindo e pulo; chegar no chao e queda, e pesa pela velocidade.
            if (_wasGrounded && !grounded && _body.linearVelocity.y > 1f)
            {
                NoiseSystem.Emit(feet, jumpRadius, jumpLoudness * ScrapGems.BodyNoise);
            }

            if (!grounded) _fallSpeed = Mathf.Max(_fallSpeed, -_body.linearVelocity.y);

            if (!_wasGrounded && grounded)
            {
                // Voltar de um pulo comum chega a uns 17,5 de velocidade; so
                // acima disso a queda comeca a pesar de verdade.
                float weight = Mathf.InverseLerp(19f, 32f, _fallSpeed);

                NoiseSystem.Emit(feet,
                    Mathf.Lerp(stepRadius, landRadius, weight),
                    Mathf.Lerp(stepLoudness, 1f, weight) * ScrapGems.BodyNoise);

                _fallSpeed = 0f;
            }

            _wasGrounded = grounded;

            bool striking = _chisel != null && _chisel.IsStriking;
            if (striking && !_wasStriking) NoiseSystem.Emit(feet + Vector2.up * 0.6f, strikeRadius, strikeLoudness);
            _wasStriking = striking;

            bool dashing = _dash != null && _dash.IsDashing;
            if (dashing && !_wasDashing) NoiseSystem.Emit(feet + Vector2.up * 0.6f, dashRadius, dashLoudness);
            _wasDashing = dashing;
        }
    }
}
