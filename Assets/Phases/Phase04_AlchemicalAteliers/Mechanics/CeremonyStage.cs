using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O grande circulo do palco do Anfiteatro de Demonstracoes: o coracao do
    /// District 4. So aparece depois que o Alquimista cai. Polido (e um
    /// TransmutationCircle comum, de segurar o botao), ele purifica o distrito
    /// a partir do palco.
    /// </summary>
    public class CeremonyStage : MonoBehaviour
    {
        [SerializeField] private TransmutationCircle circle;
        [SerializeField] private AreaPurifier purifier;

        private bool _litHere;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(TransmutationCircle newCircle, AreaPurifier newPurifier)
        {
            circle = newCircle;
            purifier = newPurifier;
        }

        private void Awake()
        {
            if (circle != null) circle.Lit += OnLit;
        }

        private void OnDestroy()
        {
            if (circle != null) circle.Lit -= OnLit;
        }

        private void Update()
        {
            if (circle == null) return;

            // O circulo so existe pra quem ja derrubou o Alquimista, e some depois da purificacao.
            bool show = MercuryAlchemist.Defeated && (_litHere || !AreaPurifier.IsPurified);
            if (circle.gameObject.activeSelf != show) circle.gameObject.SetActive(show);
        }

        private void OnLit()
        {
            _litHere = true;
            if (purifier != null) purifier.Purify(circle.transform.position);
        }
    }
}
