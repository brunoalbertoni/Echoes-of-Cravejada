using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Balanca o objeto em volta do proprio ponto (o de cima, numa corrente
    /// pendurada): um vai e vem lento, como se uma corrente de ar passasse
    /// pelo poco.
    /// </summary>
    public class Sway : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("amplitude")] [Tooltip("Quantos graus pra cada lado.")]
        [SerializeField] private float amplitude = 3f;

        [UnityEngine.Serialization.FormerlySerializedAs("period")] [Tooltip("Segundos de um vai e vem completo.")]
        [SerializeField] private float periodo = 4f;

        [UnityEngine.Serialization.FormerlySerializedAs("phase")] [Tooltip("Desloca o balanco, pra correntes vizinhas nao irem juntas.")]
        [SerializeField] private float fase;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoAmplitude, float novoPeriodo, float novoFase)
        {
            amplitude = novoAmplitude;
            periodo = novoPeriodo;
            fase = novoFase;
        }

        private float _empurrao;

        /// <summary>
        /// Um empurrao (a onda da purificacao passando): balanca forte, em
        /// graus, e vai acalmando sozinho.
        /// </summary>
        public void Impulsionar(float graus)
        {
            _empurrao = Mathf.Max(_empurrao, graus);
        }

        private void Update()
        {
            float angulo = Mathf.Sin((Time.time / Mathf.Max(0.1f, periodo) + fase) * Mathf.PI * 2f) * amplitude;

            if (_empurrao > 0.05f)
            {
                angulo += Mathf.Sin(Time.time * 7f + fase * 6f) * _empurrao;
                _empurrao *= Mathf.Exp(-1.6f * Time.deltaTime);
            }

            transform.localRotation = Quaternion.Euler(0f, 0f, angulo);
        }
    }
}
