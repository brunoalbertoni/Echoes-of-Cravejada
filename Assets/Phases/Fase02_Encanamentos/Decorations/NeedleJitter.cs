using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O ponteiro de um manometro tremendo em volta de um angulo. Zero graus e
    /// pra cima; positivo gira pra direita, onde fica a zona vermelha.
    /// </summary>
    public class NeedleJitter : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("restAngle")] [Tooltip("Angulo em volta do qual o ponteiro treme. Uns 80 poe ele no vermelho.")]
        [SerializeField] private float anguloDescanso = 80f;

        [UnityEngine.Serialization.FormerlySerializedAs("amplitude")] [Tooltip("Quantos graus ele se afasta pra cada lado.")]
        [SerializeField] private float amplitude = 9f;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [SerializeField] private float velocidade = 7f;

        private float _semente;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoAnguloDescanso, float novoAmplitude)
        {
            anguloDescanso = novoAnguloDescanso;
            amplitude = novoAmplitude;
        }

        private void Awake()
        {
            // Cada manometro treme do seu jeito.
            _semente = transform.position.x * 3.1f + transform.position.y * 7.3f;
        }

        private void Update()
        {
            float ruido = Mathf.PerlinNoise(_semente, Time.time * velocidade) * 2f - 1f;
            transform.localRotation = Quaternion.Euler(0f, 0f, -(anguloDescanso + ruido * amplitude));
        }
    }
}
