using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O ponteiro de um manometro tremendo em volta de um angulo. Zero graus e
    /// pra cima; positivo gira pra direita, onde fica a zona vermelha.
    /// </summary>
    public class NeedleJitter : MonoBehaviour
    {
        [Tooltip("Angulo em volta do qual o ponteiro treme. Uns 80 poe ele no vermelho.")]
        [SerializeField] private float restAngle = 80f;

        [Tooltip("Quantos graus ele se afasta pra cada lado.")]
        [SerializeField] private float amplitude = 9f;

        [SerializeField] private float speed = 7f;

        private float _seed;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newRestAngle, float newAmplitude)
        {
            restAngle = newRestAngle;
            amplitude = newAmplitude;
        }

        private void Awake()
        {
            // Cada manometro treme do seu jeito.
            _seed = transform.position.x * 3.1f + transform.position.y * 7.3f;
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(_seed, Time.time * speed) * 2f - 1f;
            transform.localRotation = Quaternion.Euler(0f, 0f, -(restAngle + noise * amplitude));
        }
    }
}
