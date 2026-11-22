using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Ferrolho, mecanico de caldeira: e quem monta no Scrap o motor de vapor
    /// do Coletor, e o motor e o Steam Dash.
    ///
    ///   Antes da purificacao   fala do Coletor e pede o motor, se ele cair.
    ///   Purificado, sem motor  manda buscar o motor na arena.
    ///   Com o motor na mao     instala: ao fim da conversa, o Steam Dash.
    ///   Depois                 so conversa.
    /// </summary>
    public class FerrolhoNpc : Npc
    {
        [UnityEngine.Serialization.FormerlySerializedAs("motorId")] [Tooltip("O id do motor no KeyItems.")]
        [SerializeField] private string idMotor = "Motor_Collector";

        [UnityEngine.Serialization.FormerlySerializedAs("waitingLines")] [TextArea(2, 4)] [SerializeField] private string[] linhasEsperando;
        [UnityEngine.Serialization.FormerlySerializedAs("installLines")] [TextArea(2, 4)] [SerializeField] private string[] linhasInstalar;
        [UnityEngine.Serialization.FormerlySerializedAs("doneLines")] [TextArea(2, 4)] [SerializeField] private string[] linhasPronto;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void ConfigurarMotor(string novoIdMotor, string[] esperando, string[] instalar, string[] pronto)
        {
            idMotor = novoIdMotor;
            linhasEsperando = esperando;
            linhasInstalar = instalar;
            linhasPronto = pronto;
        }

        protected override string[] EscolherFalas()
        {
            if (ScrapDash.Desbloqueado) return linhasPronto;
            if (KeyItems.Tem(idMotor)) return linhasInstalar;
            if (AreaPurifier.EstaPurificado) return linhasEsperando;
            return base.EscolherFalas();
        }

        protected override void AoTerminarConversa(ScrapController sucata)
        {
            // So a conversa de instalar entrega o motor; as outras nao mexem nele.
            if (!ScrapDash.Desbloqueado && KeyItems.Entregar(idMotor)) ScrapDash.Desbloquear();
        }
    }
}
