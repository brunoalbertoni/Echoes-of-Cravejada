using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A superficie de uma poca de mercurio. O Scrap e de ferro, e ferro
    /// flutua em mercurio: ele anda por cima, escorregando como em gelo.
    ///
    /// Este componente fica numa faixa de gatilho logo acima da poca (o corpo
    /// solido e do pai) e avisa o ScrapController enquanto ele pisa ali.
    ///
    /// Mercurio corrompido pelo Oxide tem correnteza: arrasta pra um lado.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MercurySurface : MonoBehaviour
    {
        [Tooltip("A correnteza, em tiles por segundo. Positivo arrasta pra direita; 0 e poca parada.")]
        [SerializeField] private float current;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newCurrent)
        {
            current = newCurrent;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller != null) controller.OnMercury(current);
        }
    }
}
