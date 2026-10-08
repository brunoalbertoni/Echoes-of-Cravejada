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

        private void OnTriggerEnter2D(Collider2D other)
        {
            Cool(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Cool(other);
        }

        private static void Cool(Collider2D other)
        {
            var dash = other.GetComponentInParent<ScrapDash>();
            if (dash != null) dash.Refresh();
        }
    }
}
