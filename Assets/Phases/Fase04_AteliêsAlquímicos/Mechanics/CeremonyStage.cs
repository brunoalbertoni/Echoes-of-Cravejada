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
        [UnityEngine.Serialization.FormerlySerializedAs("circle")] [SerializeField] private TransmutationCircle circulo;
        [UnityEngine.Serialization.FormerlySerializedAs("purifier")] [SerializeField] private AreaPurifier purificador;

        private bool _aquiAceso;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(TransmutationCircle novoCirculo, AreaPurifier novoPurificador)
        {
            circulo = novoCirculo;
            purificador = novoPurificador;
        }

        private void Awake()
        {
            if (circulo != null) circulo.QuadroAceso += AoAcender;
        }

        private void OnDestroy()
        {
            if (circulo != null) circulo.QuadroAceso -= AoAcender;
        }

        private void Update()
        {
            // Como nas outras fases: o Alquimista cai e o distrito purifica na
            // hora, de onde ele caiu. O circulo do palco nao e mais usado.
            if (circulo != null && circulo.gameObject.activeSelf) circulo.gameObject.SetActive(false);
            if (_aquiAceso || !MercuryAlchemist.Derrotado || AreaPurifier.EstaPurificado || purificador == null) return;

            _aquiAceso = true;
            var alquimista = FindAnyObjectByType<MercuryAlchemist>();
            purificador.Purificar(alquimista != null ? alquimista.transform.position : transform.position);
        }

        private void AoAcender()
        {
            _aquiAceso = true;
            if (purificador != null) purificador.Purificar(circulo.transform.position);
        }
    }
}
