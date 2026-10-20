using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A Bancada de Ourives: o save point. Encostar nela enche a vida e passa
    /// a ser onde o Scrap renasce quando morre.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Workbench : MonoBehaviour
    {
        [Tooltip("Onde ele renasce. Vazio usa a posicao da propria bancada.")]
        [SerializeField] private Transform restPoint;

        /// <summary>A ultima vez em que alguem descansou numa bancada. O HUD usa pra avisar.</summary>
        public static float LastRestTime { get; private set; } = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            LastRestTime = -100f;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Transform newRestPoint)
        {
            restPoint = newRestPoint;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponentInParent<ScrapHealth>();
            if (health == null) return;

            health.RestAt(restPoint != null ? restPoint.position : transform.position);
            LastRestTime = Time.time;

            // E na bancada que se crava uma gema.
            ScrapGems.SocketAtWorkbench();
        }
    }
}
