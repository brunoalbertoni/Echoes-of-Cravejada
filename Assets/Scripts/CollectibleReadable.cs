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

        [Tooltip("O aviso na tela ao guardar.")]
        [SerializeField] private string message;

        [Tooltip("O comeco de id do conjunto, pra contar quantas ja foram achadas.")]
        [SerializeField] private string group;

        [SerializeField] private int groupSize;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void ConfigureCollectible(string newId, string newMessage, string newGroup, int newGroupSize)
        {
            id = newId;
            message = newMessage;
            group = newGroup;
            groupSize = newGroupSize;
        }

        private void Start()
        {
            // Ja levada numa visita anterior.
            if (KeyItems.WasFound(id)) gameObject.SetActive(false);
        }

        protected override void OnTalkFinished(ScrapController scrap)
        {
            if (KeyItems.WasFound(id)) return;

            KeyItems.Add(id);

            string text = message;
            if (!string.IsNullOrEmpty(group) && groupSize > 0) text += $" ({KeyItems.CountFound(group)}/{groupSize})";
            if (!string.IsNullOrEmpty(text)) HudNotice.Show(text);

            gameObject.SetActive(false);
        }
    }
}
