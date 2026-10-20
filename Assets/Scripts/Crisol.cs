using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Crisol, o Lapidario. E um Npc como os outros, com duas coisas a mais:
    /// na primeira conversa ele entrega o rubi bruto, e depois de falar abre a
    /// loja.
    ///
    /// O que a loja vende, o preco e o efeito ficam no Shop; aqui ficam a
    /// escolha e a compra. Teclas: W/S ou setas escolhem, Espaco/Enter (ou o
    /// numero do item) compram, E ou Esc fecham. No controle: o direcional
    /// escolhe, o botao de baixo compra, o da direita fecha.
    ///
    /// Se ele tiver uma memoria pra contar (MemoryLines), ela aparece na loja,
    /// paga em gemas; comprada, ele fecha a loja e conta.
    /// </summary>
    public class Crisol : Npc
    {
        [Tooltip("A historia que ele vende como 'Memoria'. Vazio: nao ha memoria pra vender neste distrito.")]
        [TextArea(2, 4)]
        [SerializeField] private string[] memoryLines;

        [Tooltip("O que ele diz das outras vezes, depois da primeira conversa. Vazio: repete a primeira.")]
        [TextArea(2, 4)]
        [SerializeField] private string[] repeatLines;

        // A primeira conversa de cada distrito e a apresentacao; as outras sao curtas.
        private string MetFlag => "Crisol_Met_" + gameObject.scene.name;

        private ScrapController _customer;
        private readonly List<ShopItem> _items = new List<ShopItem>();

        // O E que termina a conversa abre a loja; no mesmo quadro ele nao pode fechar.
        private int _openedFrame = -1;

        /// <summary>O Crisol com a loja aberta, se houver. O HUD usa pra desenhar o painel.</summary>
        public static Crisol ShopOpen { get; private set; }

        /// <summary>O que esta a venda aqui, na ordem do painel.</summary>
        public IReadOnlyList<ShopItem> Items => _items;

        /// <summary>O item marcado no painel.</summary>
        public int Selected { get; private set; }

        /// <summary>O que ele disse da ultima tentativa de compra, pra mostrar no painel.</summary>
        public string LastMessage { get; private set; } = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayShop()
        {
            ShopOpen = null;
        }

        /// <summary>Usado pelos geradores de fase: a memoria que ele vende neste distrito.</summary>
        public void ConfigureMemory(string[] lines)
        {
            memoryLines = lines;
        }

        /// <summary>Usado pelos geradores de fase: o que ele diz depois da primeira conversa.</summary>
        public void ConfigureRepeat(string[] lines)
        {
            repeatLines = lines;
        }

        protected override string[] PickLines()
        {
            // A primeira vez neste distrito, a apresentacao inteira.
            if (!KeyItems.HasFlag(MetFlag)) return FirstLines;

            // Depois: o que mudou com a purificacao, ou a frase curta de sempre.
            string[] chosen = base.PickLines();
            bool hasRepeat = repeatLines != null && repeatLines.Length > 0;
            return chosen == FirstLines && hasRepeat ? repeatLines : chosen;
        }

        protected override void OnTalkFinished(ScrapController scrap)
        {
            KeyItems.SetFlag(MetFlag);
            ScrapGems.GiveRuby();
            Open(scrap);
        }

        private void Open(ScrapController scrap)
        {
            _items.Clear();
            foreach (ShopItem item in Shop.Catalog)
            {
                bool hasMemory = memoryLines != null && memoryLines.Length > 0;
                if (item.IsMemory && !hasMemory) continue;

                _items.Add(item);
            }

            _customer = scrap;
            _customer.ControlLocked = true;
            Selected = Mathf.Clamp(Selected, 0, Mathf.Max(0, _items.Count - 1));
            LastMessage = "";
            ShopOpen = this;
            _openedFrame = Time.frameCount;
        }

        private void LateUpdate()
        {
            if (ShopOpen != this || Time.frameCount == _openedFrame) return;

            Keyboard keyboard = Keyboard.current;
            Gamepad pad = Gamepad.current;

            bool up = (keyboard != null && (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame))
                || (pad != null && pad.dpad.up.wasPressedThisFrame);
            bool down = (keyboard != null && (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame))
                || (pad != null && pad.dpad.down.wasPressedThisFrame);
            bool buy = (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
                || (pad != null && pad.buttonSouth.wasPressedThisFrame);
            bool close = (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame))
                || (pad != null && (pad.buttonEast.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame));

            // O numero do item escolhe e compra de uma vez.
            if (keyboard != null)
            {
                for (int i = 0; i < _items.Count && i < 9; i++)
                {
                    if (!keyboard[Key.Digit1 + i].wasPressedThisFrame && !keyboard[Key.Numpad1 + i].wasPressedThisFrame) continue;

                    Selected = i;
                    buy = true;
                }
            }

            if (_items.Count > 0)
            {
                if (up) Selected = (Selected + _items.Count - 1) % _items.Count;
                if (down) Selected = (Selected + 1) % _items.Count;
            }

            if (buy) Buy();
            else if (close) Close();
        }

        private void Buy()
        {
            if (_items.Count == 0) return;

            ShopItem item = _items[Selected];
            var health = _customer != null ? _customer.GetComponent<ScrapHealth>() : null;

            LastMessage = Shop.TryBuy(item, health, out bool bought);

            if (bought && item.IsMemory)
            {
                // Fecha a loja e conta. Ao fim da historia, a loja reabre.
                ScrapController customer = _customer;
                Close();
                if (customer != null) customer.ControlLocked = true;
                StartTalk(memoryLines);
            }
        }

        private void Close()
        {
            if (_customer != null) _customer.ControlLocked = false;

            _customer = null;
            ShopOpen = null;
        }

        private void OnDestroy()
        {
            if (ShopOpen == this) ShopOpen = null;
        }
    }
}
