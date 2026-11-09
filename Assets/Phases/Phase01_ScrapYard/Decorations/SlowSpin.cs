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

        private void Update()
        {
            transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
        }
    }
}
