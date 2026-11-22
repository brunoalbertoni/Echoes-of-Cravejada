using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma nuvem de vapor parada. Nao empurra: so esfria as costas do Scrap,
    /// devolvendo o dash na hora. E com elas que se cruza um vao grande demais
    /// pra um dash so — dash, nuvem, dash de novo.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SteamCloud : MonoBehaviour
    {
        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            // Um sopro de vapor nas costas: o dash voltou.
            if (outro.GetComponentInParent<ScrapDash>() != null && ScrapDash.Desbloqueado) Sfx.RecarregarVapor.Tocar();
            Resfriar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Resfriar(outro);
        }

        private static void Resfriar(Collider2D outro)
        {
            var dash = outro.GetComponentInParent<ScrapDash>();
            if (dash != null) dash.Atualizar();
        }
    }
}
