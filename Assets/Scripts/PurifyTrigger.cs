using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma area que purifica o distrito quando o Scrap chega nela. No
    /// District 2 fica no topo do poco de fuga: a purificacao so vem depois de
    /// derrotar o Boiler Master E escapar do acido ascendente.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PurifyTrigger : MonoBehaviour
    {
        [SerializeField] private AreaPurifier purifier;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(AreaPurifier newPurifier)
        {
            purifier = newPurifier;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (purifier == null || !BoilerMaster.Defeated) return;
            if (other.GetComponentInParent<ScrapController>() == null) return;

            purifier.Purify();
        }
    }
}
