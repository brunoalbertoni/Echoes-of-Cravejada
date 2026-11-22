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
        [UnityEngine.Serialization.FormerlySerializedAs("current")] [Tooltip("A correnteza, em tiles por segundo. Positivo arrasta pra direita; 0 e poca parada.")]
        [SerializeField] private float atual;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoAtual)
        {
            atual = novoAtual;
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            var controlador = outro.GetComponentInParent<ScrapController>();
            if (controlador != null) controlador.NoMercurio(atual);
        }
    }
}
