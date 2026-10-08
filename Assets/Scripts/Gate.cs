using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um portao de grade. Fechado, e parede; aberto, sobe pra dentro do teto e
    /// deixa de barrar. Quem manda abrir e a alavanca.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Gate : MonoBehaviour
    {
        [Tooltip("Quantos tiles ele anda ao abrir.")]
        [SerializeField] private float rise = 6f;

        [Tooltip("Pra que lado ele abre. Pra cima e o normal; uma escotilha no chao abre de lado.")]
        [SerializeField] private Vector2 direction = Vector2.up;

        [SerializeField] private float openTime = 1.2f;

        private bool _open;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newRise)
        {
            rise = newRise;
        }

        /// <summary>Usado pelos geradores de fase, pra portoes que nao abrem pra cima.</summary>
        public void Configure(float newRise, Vector2 newDirection)
        {
            rise = newRise;
            direction = newDirection;
        }

        public void Open()
        {
            if (_open) return;
            _open = true;

            StartCoroutine(Lift());
        }

        private IEnumerator Lift()
        {
            Vector3 from = transform.position;
            Vector3 to = from + (Vector3)(direction.normalized * rise);

            for (float t = 0f; t < openTime; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(from, to, t / openTime);
                yield return null;
            }

            transform.position = to;

            // Ja esta todo dentro do teto: nao precisa mais barrar nada.
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
