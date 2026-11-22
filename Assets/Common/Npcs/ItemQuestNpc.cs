using System;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>Um punhado de falas. Existe pra a Unity conseguir guardar uma lista de listas.</summary>
    [Serializable]
    public class LineSet
    {
        [UnityEngine.Serialization.FormerlySerializedAs("Lines")] [TextArea(2, 4)] public string[] Linhas;

        public LineSet(string[] linhas2)
        {
            Linhas = linhas2;
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
        [UnityEngine.Serialization.FormerlySerializedAs("itemPrefix")] [Tooltip("O comeco do id das coisas pedidas, no KeyItems.")]
        [SerializeField] private string prefixoItem = "Tool_";

        [UnityEngine.Serialization.FormerlySerializedAs("itemCount")] [SerializeField] private int quantidadeItem = 3;

        [UnityEngine.Serialization.FormerlySerializedAs("gemReward")] [Tooltip("Quantas gemas ele paga quando recebe tudo.")]
        [SerializeField] private int recompensaGema = 1;

        [UnityEngine.Serialization.FormerlySerializedAs("rewardNotice")] [Tooltip("O aviso na tela ao pagar.")]
        [SerializeField] private string avisoRecompensa = "Gema lapidada recebida";

        [UnityEngine.Serialization.FormerlySerializedAs("purifiedGreeting")] [TextArea(2, 4)] [SerializeField] private string saudacaoPurificado;

        [UnityEngine.Serialization.FormerlySerializedAs("progressLines")] [Tooltip("Depois do pedido, por quantas ele ja tem na mao: 0, 1, 2... A ultima vale pro resto.")]
        [SerializeField] private LineSet[] linhasProgresso;

        [UnityEngine.Serialization.FormerlySerializedAs("completeLines")] [TextArea(2, 4)] [SerializeField] private string[] linhasCompleto;
        [UnityEngine.Serialization.FormerlySerializedAs("doneLines")] [TextArea(2, 4)] [SerializeField] private string[] linhasPronto;

        private string MarcaPerguntado => "Quest_" + prefixoItem + "Asked";
        private string MarcaEntregues => "Quest_" + prefixoItem + "Delivered";

        /// <summary>Usado pelos geradores de fase.</summary>
        public void ConfigurarMissao(string novoPrefixoItem, int novoQuantidadeItem, int novoRecompensaGema, string novoAvisoRecompensa,
            string saudacao, LineSet[] progresso, string[] completo, string[] pronto)
        {
            prefixoItem = novoPrefixoItem;
            quantidadeItem = novoQuantidadeItem;
            recompensaGema = novoRecompensaGema;
            avisoRecompensa = novoAvisoRecompensa;
            saudacaoPurificado = saudacao;
            linhasProgresso = progresso;
            linhasCompleto = completo;
            linhasPronto = pronto;
        }

        protected override string[] EscolherFalas()
        {
            bool entregues = KeyItems.TemMarca(MarcaEntregues);

            // Antes de ouvir o pedido, a conversa de sempre (que termina nele).
            if (!KeyItems.TemMarca(MarcaPerguntado) && !entregues) return base.EscolherFalas();

            int segurado = KeyItems.ContarGuardados(prefixoItem);
            string[] escolhido;

            if (entregues) escolhido = linhasPronto;
            else if (segurado >= quantidadeItem) escolhido = linhasCompleto;
            else escolhido = Progresso(segurado);

            return ComSaudacao(escolhido);
        }

        protected override void AoTerminarConversa(ScrapController sucata)
        {
            // A primeira conversa so faz o pedido: quem ja chega com tudo ouve
            // a entrega na proxima, em vez de ganhar a gema calado.
            // Sem nada a pedir (itemCount 0): a primeira conversa ja e a entrega.
            bool perguntadoAntes = KeyItems.TemMarca(MarcaPerguntado) || quantidadeItem <= 0;
            KeyItems.DefinirMarca(MarcaPerguntado);
            if (!perguntadoAntes) return;

            if (KeyItems.TemMarca(MarcaEntregues) || KeyItems.ContarGuardados(prefixoItem) < quantidadeItem) return;

            KeyItems.EntregarTudo(prefixoItem);
            KeyItems.DefinirMarca(MarcaEntregues);

            GemPurse.Adicionar(recompensaGema);
            Sfx.Gema.Tocar();
        }

        private string[] Progresso(int segurado)
        {
            if (linhasProgresso == null || linhasProgresso.Length == 0) return linhasCompleto;
            return linhasProgresso[Mathf.Clamp(segurado, 0, linhasProgresso.Length - 1)].Linhas;
        }

        private string[] ComSaudacao(string[] escolhido)
        {
            if (!AreaPurifier.EstaPurificado || string.IsNullOrEmpty(saudacaoPurificado) || escolhido == null) return escolhido;

            var linhas2 = new string[escolhido.Length + 1];
            linhas2[0] = saudacaoPurificado;
            escolhido.CopyTo(linhas2, 1);
            return linhas2;
        }
    }
}
