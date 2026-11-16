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
            // Como nas outras fases: o Alquimista cai e o distrito purifica na
            // hora, de onde ele caiu. O circulo do palco nao e mais usado.
            if (circle != null && circle.gameObject.activeSelf) circle.gameObject.SetActive(false);
            if (_litHere || !MercuryAlchemist.Defeated || AreaPurifier.IsPurified || purifier == null) return;

            _litHere = true;
            var alchemist = FindAnyObjectByType<MercuryAlchemist>();
            purifier.Purify(alchemist != null ? alchemist.transform.position : transform.position);
        }

        private void OnLit()
        {
            _litHere = true;
            if (purifier != null) purifier.Purify(circle.transform.position);
        }
    }
}
