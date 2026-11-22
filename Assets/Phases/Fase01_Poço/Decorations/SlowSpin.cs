using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Gira o objeto devagar, sem parar: as engrenagens enormes presas na
    /// rocha do District 1, que alguma maquina la dentro ainda empurra.
    /// </summary>
    public class SlowSpin : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("degreesPerSecond")] [Tooltip("Graus por segundo. Negativo gira no sentido horario.")]
        [SerializeField] private float grausPorSegundo = 6f;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoGrausPorSegundo)
        {
            grausPorSegundo = novoGrausPorSegundo;
        }

        private float _impulso;

        /// <summary>
        /// Um tranco (a onda da purificacao passando): a engrenagem dispara,
        /// times vezes mais rapido, e volta devagar ao ritmo dela.
        /// </summary>
        public void Impulsionar(float tempos)
        {
            _impulso = Mathf.Max(_impulso, tempos);
        }

        private void Update()
        {
            float velocidade = grausPorSegundo * (1f + _impulso);
            if (_impulso > 0.01f) _impulso *= Mathf.Exp(-0.9f * Time.deltaTime);
            transform.Rotate(0f, 0f, velocidade * Time.deltaTime);
        }
    }
}
