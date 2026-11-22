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
        [UnityEngine.Serialization.FormerlySerializedAs("damage")] [SerializeField] private int dano = 1;

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Queimar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Queimar(outro);
        }

        private void Queimar(Collider2D outro)
        {
            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null)
            {
                vida.LevarDano(dano, transform.position, true);

                // Espinho fura com um tinido; o abismo nao tem som de ponta.
                bool agoraFerido = vida.UltimoTempoGolpe == Time.time;
                if (agoraFerido && name.StartsWith("Spikes")) Sfx.GolpeEspinho.Tocar(vida.transform.position);

                // Piscando de um golpe recente ou no meio do dash, o dano nao
                // entra; mas ele tem que voltar pro chao seguro mesmo assim,
                // senao segue caindo pelo abismo ate morrer.
                if (!agoraFerido && vida.Vida > 0 && !Cinematic.Ativo)
                {
                    var costas = outro.GetComponentInParent<ScrapRespawn>();
                    if (costas != null) costas.Renascer();
                }
                return;
            }

            // Sem vida (um Scrap montado na mao, por exemplo), so devolve.
            var renascimento = outro.GetComponentInParent<ScrapRespawn>();
            if (renascimento != null) renascimento.Renascer();
        }
    }
}
