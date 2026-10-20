using System;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>Um punhado de falas. Existe pra a Unity conseguir guardar uma lista de listas.</summary>
    [Serializable]
    public class LineSet
    {
        [TextArea(2, 4)] public string[] Lines;

        public LineSet(string[] lines)
        {
            Lines = lines;
        }
    }

    /// <summary>
    /// Um personagem que pede um conjunto de coisas espalhadas pelo distrito
    /// (KeyItems com o mesmo comeco de id) e paga em gemas quando recebe todas.
    /// O Rebite pede as ferramentas do mestre; o velho encanador, as paginas
    /// do livro de turnos.
    ///
    ///   Primeira conversa   a de sempre, que termina no pedido.
    ///   Depois do pedido    o que ele diz depende de quantas o Scrap tem na mao.
    ///   Com todas           entrega: ao fim da conversa, as gemas.
    ///   Depois              so agradece.
    ///
    /// Depois da purificacao, toda conversa de depois do pedido abre com
    /// PurifiedGreeting.
    /// </summary>
    public class ItemQuestNpc : Npc
    {
        [Tooltip("O comeco do id das coisas pedidas, no KeyItems.")]
        [SerializeField] private string itemPrefix = "Tool_";

        [SerializeField] private int itemCount = 3;

        [Tooltip("Quantas gemas ele paga quando recebe tudo.")]
        [SerializeField] private int gemReward = 1;

        [Tooltip("O aviso na tela ao pagar.")]
        [SerializeField] private string rewardNotice = "Gema lapidada recebida";

        [TextArea(2, 4)] [SerializeField] private string purifiedGreeting;

        [Tooltip("Depois do pedido, por quantas ele ja tem na mao: 0, 1, 2... A ultima vale pro resto.")]
        [SerializeField] private LineSet[] progressLines;

        [TextArea(2, 4)] [SerializeField] private string[] completeLines;
        [TextArea(2, 4)] [SerializeField] private string[] doneLines;

        private string AskedFlag => "Quest_" + itemPrefix + "Asked";
        private string DeliveredFlag => "Quest_" + itemPrefix + "Delivered";

        /// <summary>Usado pelos geradores de fase.</summary>
        public void ConfigureQuest(string newItemPrefix, int newItemCount, int newGemReward, string newRewardNotice,
            string greeting, LineSet[] progress, string[] complete, string[] done)
        {
            itemPrefix = newItemPrefix;
            itemCount = newItemCount;
            gemReward = newGemReward;
            rewardNotice = newRewardNotice;
            purifiedGreeting = greeting;
            progressLines = progress;
            completeLines = complete;
            doneLines = done;
        }

        protected override string[] PickLines()
        {
            bool delivered = KeyItems.HasFlag(DeliveredFlag);

            // Antes de ouvir o pedido, a conversa de sempre (que termina nele).
            if (!KeyItems.HasFlag(AskedFlag) && !delivered) return base.PickLines();

            int held = KeyItems.CountHeld(itemPrefix);
            string[] chosen;

            if (delivered) chosen = doneLines;
            else if (held >= itemCount) chosen = completeLines;
            else chosen = Progress(held);

            return WithGreeting(chosen);
        }

        protected override void OnTalkFinished(ScrapController scrap)
        {
            // A primeira conversa so faz o pedido: quem ja chega com tudo ouve
            // a entrega na proxima, em vez de ganhar a gema calado.
            bool askedBefore = KeyItems.HasFlag(AskedFlag);
            KeyItems.SetFlag(AskedFlag);
            if (!askedBefore) return;

            if (KeyItems.HasFlag(DeliveredFlag) || KeyItems.CountHeld(itemPrefix) < itemCount) return;

            KeyItems.DeliverAll(itemPrefix);
            KeyItems.SetFlag(DeliveredFlag);

            GemPurse.Add(gemReward);
            HudNotice.Show(rewardNotice);
        }

        private string[] Progress(int held)
        {
            if (progressLines == null || progressLines.Length == 0) return completeLines;
            return progressLines[Mathf.Clamp(held, 0, progressLines.Length - 1)].Lines;
        }

        private string[] WithGreeting(string[] chosen)
        {
            if (!AreaPurifier.IsPurified || string.IsNullOrEmpty(purifiedGreeting) || chosen == null) return chosen;

            var lines = new string[chosen.Length + 1];
            lines[0] = purifiedGreeting;
            chosen.CopyTo(lines, 1);
            return lines;
        }
    }
}
