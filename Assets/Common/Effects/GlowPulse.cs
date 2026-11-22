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
        [UnityEngine.Serialization.FormerlySerializedAs("glowLight")] [SerializeField] private Light2D luzBrilho;
        [UnityEngine.Serialization.FormerlySerializedAs("glowSprite")] [SerializeField] private SpriteRenderer spriteBrilho;

        [UnityEngine.Serialization.FormerlySerializedAs("depth")] [Tooltip("Quanto a intensidade varia, como fracao do valor original.")]
        [SerializeField, Range(0f, 1f)] private float profundidade = 0.35f;

        [UnityEngine.Serialization.FormerlySerializedAs("period")] [Tooltip("Segundos de uma batida completa.")]
        [SerializeField] private float periodo = 2.2f;

        [UnityEngine.Serialization.FormerlySerializedAs("phase")] [Tooltip("Desloca a batida, pra duas caldeiras vizinhas nao pulsarem juntas.")]
        [SerializeField] private float fase;

        private float _intensidadeBase;
        private Color _corBase;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Light2D luz, SpriteRenderer sprite, float novoPeriodo, float novoFase)
        {
            luzBrilho = luz;
            spriteBrilho = sprite;
            periodo = novoPeriodo;
            fase = novoFase;
        }

        private void Awake()
        {
            if (luzBrilho != null) _intensidadeBase = luzBrilho.intensity;
            if (spriteBrilho != null) _corBase = spriteBrilho.color;
        }

        private void Update()
        {
            float onda = Mathf.Sin((Time.time / Mathf.Max(0.1f, periodo) + fase) * Mathf.PI * 2f);

            // So a metade de cima da onda, ao quadrado: sobe rapido e desce
            // devagar, como brasa soprada.
            float batida = Mathf.Max(0f, onda);
            float quantia = 1f - profundidade + profundidade * batida * batida * 2f;

            if (luzBrilho != null) luzBrilho.intensity = _intensidadeBase * quantia;

            if (spriteBrilho != null)
            {
                Color cor = _corBase;
                cor.a = _corBase.a * Mathf.Clamp01(0.55f + 0.45f * quantia);
                spriteBrilho.color = cor;
            }
        }
    }
}
