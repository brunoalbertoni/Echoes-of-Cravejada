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

                // Espinho fura com um tinido; o abismo nao tem som de ponta.
                bool hurtNow = health.LastHitTime == Time.time;
                if (hurtNow && name.StartsWith("Spikes")) Sfx.SpikeHit.Play(health.transform.position);

                // Piscando de um golpe recente ou no meio do dash, o dano nao
                // entra; mas ele tem que voltar pro chao seguro mesmo assim,
                // senao segue caindo pelo abismo ate morrer.
                if (!hurtNow && health.Health > 0 && !Cinematic.Active)
                {
                    var back = other.GetComponentInParent<ScrapRespawn>();
                    if (back != null) back.Respawn();
                }
                return;
            }

            // Sem vida (um Scrap montado na mao, por exemplo), so devolve.
            var respawn = other.GetComponentInParent<ScrapRespawn>();
            if (respawn != null) respawn.Respawn();
        }
    }
}
