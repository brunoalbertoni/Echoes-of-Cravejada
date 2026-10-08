using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Faz uma luz (e, se houver, o brilho de um sprite) pulsar devagar. E o
    /// que faz as caldeiras parecerem um coracao batendo.
    /// </summary>
    public class GlowPulse : MonoBehaviour
    {
        [SerializeField] private Light2D glowLight;
        [SerializeField] private SpriteRenderer glowSprite;

        [Tooltip("Quanto a intensidade varia, como fracao do valor original.")]
        [SerializeField, Range(0f, 1f)] private float depth = 0.35f;

        [Tooltip("Segundos de uma batida completa.")]
        [SerializeField] private float period = 2.2f;

        [Tooltip("Desloca a batida, pra duas caldeiras vizinhas nao pulsarem juntas.")]
        [SerializeField] private float phase;

        private float _baseIntensity;
        private Color _baseColor;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Light2D light, SpriteRenderer sprite, float newPeriod, float newPhase)
        {
            glowLight = light;
            glowSprite = sprite;
            period = newPeriod;
            phase = newPhase;
        }

        private void Awake()
        {
            if (glowLight != null) _baseIntensity = glowLight.intensity;
            if (glowSprite != null) _baseColor = glowSprite.color;
        }

        private void Update()
        {
            float wave = Mathf.Sin((Time.time / Mathf.Max(0.1f, period) + phase) * Mathf.PI * 2f);

            // So a metade de cima da onda, ao quadrado: sobe rapido e desce
            // devagar, como brasa soprada.
            float beat = Mathf.Max(0f, wave);
            float amount = 1f - depth + depth * beat * beat * 2f;

            if (glowLight != null) glowLight.intensity = _baseIntensity * amount;

            if (glowSprite != null)
            {
                Color color = _baseColor;
                color.a = _baseColor.a * Mathf.Clamp01(0.55f + 0.45f * amount);
                glowSprite.color = color;
            }
        }
    }
}
