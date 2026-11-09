using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma area que machuca e de onde nao da pra sair andando: espinhos de
    /// sucata e pocas de acido.
    ///
    /// Encostar tira vida e joga o Scrap de volta pro ultimo chao seguro. Da
    /// pra quicar em cima com o cinzel apontado pra baixo (ver ScrapChisel).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hazard : MonoBehaviour
    {
        [SerializeField] private int damage = 1;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Burn(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Burn(other);
        }

        private void Burn(Collider2D other)
        {
            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null)
            {
                health.TakeDamage(damage, transform.position, true);
                return;
            }

            // Sem vida (um Scrap montado na mao, por exemplo), so devolve.
            var respawn = other.GetComponentInParent<ScrapRespawn>();
            if (respawn != null) respawn.Respawn();
        }
    }
}
