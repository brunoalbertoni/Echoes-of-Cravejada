using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O tablado onde os Tarnished dao o primeiro polimento no Scrap. Quando
    /// ele sobe, troca de aparencia e solta um punhado de faiscas.
    ///
    /// Os Tarnished ainda nao existem: por enquanto o tablado faz sozinho.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PolishStation : MonoBehaviour
    {
        [Tooltip("Faiscas que saem na hora do polimento. Opcional.")]
        [SerializeField] private ParticleSystem sparkles;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(ParticleSystem newSparkles)
        {
            sparkles = newSparkles;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var appearance = other.GetComponentInParent<ScrapAppearance>();
            if (appearance == null) return;

            if (appearance.Polish() && sparkles != null) sparkles.Play();
        }
    }
}
