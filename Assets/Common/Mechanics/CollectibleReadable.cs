using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Algo que se le e se leva: ao terminar de ler, o Scrap guarda (no
    /// KeyItems) e a coisa some do lugar. As paginas soltas do livro de turnos
    /// sao assim — o velho encanador quer o livro de volta.
    /// </summary>
    public class CollectibleReadable : Npc
    {
        [SerializeField] private string id;

        [UnityEngine.Serialization.FormerlySerializedAs("message")] [Tooltip("O aviso na tela ao guardar.")]
        [SerializeField] private string mensagem;

        [UnityEngine.Serialization.FormerlySerializedAs("group")] [Tooltip("O comeco de id do conjunto, pra contar quantas ja foram achadas.")]
        [SerializeField] private string grupo;

        [UnityEngine.Serialization.FormerlySerializedAs("groupSize")] [SerializeField] private int tamanhoGrupo;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void ConfigurarColecionavel(string novoId, string novoMensagem, string novoGrupo, int novoTamanhoGrupo)
        {
            id = novoId;
            mensagem = novoMensagem;
            grupo = novoGrupo;
            tamanhoGrupo = novoTamanhoGrupo;
        }

        private void Start()
        {
            // Ja levada numa visita anterior.
            if (KeyItems.FoiEncontrado(id)) gameObject.SetActive(false);
        }

        protected override void AoTerminarConversa(ScrapController sucata)
        {
            if (KeyItems.FoiEncontrado(id)) return;

            KeyItems.Adicionar(id);
            Sfx.ItemChave.Tocar();

            // Contada: sem aviso, so o som.

            gameObject.SetActive(false);
        }
    }
}
