using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Gira o objeto devagar, sem parar: as engrenagens enormes presas na
    /// rocha do District 1, que alguma maquina la dentro ainda empurra.
    /// </summary>
    public class SlowSpin : MonoBehaviour
    {
        [Tooltip("Graus por segundo. Negativo gira no sentido horario.")]
        [SerializeField] private float degreesPerSecond = 6f;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newDegreesPerSecond)
        {
            degreesPerSecond = newDegreesPerSecond;
        }

        private float _boost;

        /// <summary>
        /// Um tranco (a onda da purificacao passando): a engrenagem dispara,
        /// times vezes mais rapido, e volta devagar ao ritmo dela.
        /// </summary>
        public void Kick(float times)
        {
            _boost = Mathf.Max(_boost, times);
        }

        private void Update()
        {
            float speed = degreesPerSecond * (1f + _boost);
            if (_boost > 0.01f) _boost *= Mathf.Exp(-0.9f * Time.deltaTime);
            transform.Rotate(0f, 0f, speed * Time.deltaTime);
        }
    }
}
