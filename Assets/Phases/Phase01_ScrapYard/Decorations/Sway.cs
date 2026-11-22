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

        private float _kick;

        /// <summary>
        /// Um empurrao (a onda da purificacao passando): balanca forte, em
        /// graus, e vai acalmando sozinho.
        /// </summary>
        public void Kick(float degrees)
        {
            _kick = Mathf.Max(_kick, degrees);
        }

        private void Update()
        {
            float angle = Mathf.Sin((Time.time / Mathf.Max(0.1f, period) + phase) * Mathf.PI * 2f) * amplitude;

            if (_kick > 0.05f)
            {
                angle += Mathf.Sin(Time.time * 7f + phase * 6f) * _kick;
                _kick *= Mathf.Exp(-1.6f * Time.deltaTime);
            }

            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
