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
        [Tooltip("Quantos graus pra cada lado.")]
        [SerializeField] private float amplitude = 3f;

        [Tooltip("Segundos de um vai e vem completo.")]
        [SerializeField] private float period = 4f;

        [Tooltip("Desloca o balanco, pra correntes vizinhas nao irem juntas.")]
        [SerializeField] private float phase;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newAmplitude, float newPeriod, float newPhase)
        {
            amplitude = newAmplitude;
            period = newPeriod;
            phase = newPhase;
        }

        private void Update()
        {
            float angle = Mathf.Sin((Time.time / Mathf.Max(0.1f, period) + phase) * Mathf.PI * 2f) * amplitude;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
