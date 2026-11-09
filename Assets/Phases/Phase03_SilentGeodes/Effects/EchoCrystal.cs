using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um cristal que reage ao som. No escuro ele quase nao brilha; quando um
    /// ruido chega nele, acende, e vai apagando devagar.
    ///
    /// E a "ecolocalizacao" do District 3: o jogador so ve o mundo pelos
    /// cristais que o proprio barulho acendeu. Quanto mais longe do som, mais
    /// fraco e mais atrasado ele acende — a luz se espalha em onda.
    /// </summary>
    public class EchoCrystal : MonoBehaviour
    {
        [SerializeField] private Light2D glow;
        [SerializeField] private SpriteRenderer sprite;

        [Tooltip("Brilho no escuro, sem som nenhum.")]
        [SerializeField] private float restIntensity = 0.04f;

        [Tooltip("Brilho no auge, com um som bem em cima.")]
        [SerializeField] private float peakIntensity = 1.2f;

        [Tooltip("Segundos ate voltar ao escuro.")]
        [SerializeField] private float fadeTime = 2.8f;

        private float _level;
        private float _target;
        private float _riseAt;
        private bool _alwaysLit;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Light2D newGlow, SpriteRenderer newSprite, float newPeak)
        {
            glow = newGlow;
            sprite = newSprite;
            peakIntensity = newPeak;
        }

        /// <summary>Deixa o cristal aceso pra sempre. E o que a purificacao faz.</summary>
        public void LightForever()
        {
            _alwaysLit = true;
        }

        /// <summary>Acende agora, a toda. Quem chama e o proprio cristal ressonante, ao ser golpeado.</summary>
        public void Flash()
        {
            _level = 1f;
            _target = 0f;
        }

        private void OnEnable()
        {
            NoiseSystem.Heard += OnNoise;
            AreaPurifier.Purifying += LightForever;
            Apply();
        }

        private void OnDisable()
        {
            NoiseSystem.Heard -= OnNoise;
            AreaPurifier.Purifying -= LightForever;
        }

        private void Start()
        {
            // Distrito ja purificado numa visita anterior: ja chega aceso.
            if (AreaPurifier.IsPurified)
            {
                _alwaysLit = true;
                _level = 0.45f;
                Apply();
            }
        }

        private void OnNoise(Vector2 origin, float radius, float loudness)
        {
            float distance = Vector2.Distance(origin, transform.position);
            if (distance > radius) return;

            // Mais perto do som, mais forte. Ninguem dentro do raio fica no zero.
            float strength = Mathf.Lerp(1f, 0.35f, distance / Mathf.Max(0.1f, radius));
            if (strength <= _target) return;

            _target = strength;

            // A onda leva um instante pra chegar.
            _riseAt = Time.time + distance * 0.025f;
        }

        private void Update()
        {
            if (_alwaysLit)
            {
                _level = Mathf.MoveTowards(_level, 0.45f, Time.deltaTime);
                Apply();
                return;
            }

            if (_target > 0f && Time.time >= _riseAt)
            {
                _level = Mathf.Max(_level, _target);
                _target = 0f;
            }

            if (_level <= 0f) return;

            _level = Mathf.Max(0f, _level - Time.deltaTime / Mathf.Max(0.1f, fadeTime));
            Apply();
        }

        private void Apply()
        {
            // Apaga mais devagar no comeco: fica um tempo claro e some no fim.
            float eased = _level * _level * (3f - 2f * _level);

            if (glow != null) glow.intensity = Mathf.Lerp(restIntensity, peakIntensity, eased);

            if (sprite != null)
            {
                float shade = Mathf.Lerp(0.35f, 1f, eased);
                Color color = sprite.color;
                sprite.color = new Color(shade, shade, shade, color.a);
            }
        }
    }
}
