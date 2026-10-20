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
        [Tooltip("O id do motor no KeyItems.")]
        [SerializeField] private string motorId = "Motor_Collector";

        [TextArea(2, 4)] [SerializeField] private string[] waitingLines;
        [TextArea(2, 4)] [SerializeField] private string[] installLines;
        [TextArea(2, 4)] [SerializeField] private string[] doneLines;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void ConfigureMotor(string newMotorId, string[] waiting, string[] install, string[] done)
        {
            motorId = newMotorId;
            waitingLines = waiting;
            installLines = install;
            doneLines = done;
        }

        protected override string[] PickLines()
        {
            if (ScrapDash.Unlocked) return doneLines;
            if (KeyItems.Has(motorId)) return installLines;
            if (AreaPurifier.IsPurified) return waitingLines;
            return base.PickLines();
        }

        protected override void OnTalkFinished(ScrapController scrap)
        {
            // So a conversa de instalar entrega o motor; as outras nao mexem nele.
            if (!ScrapDash.Unlocked && KeyItems.Deliver(motorId)) ScrapDash.Unlock();
        }
    }
}
