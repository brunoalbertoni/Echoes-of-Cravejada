using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Crisol, o Lapidario. E um Npc como os outros, com duas coisas a mais:
    /// na primeira conversa ele entrega o rubi bruto, e depois de falar abre a
    /// loja.
    ///
    /// A loja e minima, por enquanto: um item so, o reforco de chassi, pago em
    /// Opaque Shards. Teclas: 1 compra, E ou Esc fecha.
    /// </summary>
    public class Crisol : Npc
    {
        [Tooltip("Preco do primeiro reforco de chassi. Cada um seguinte custa esse tanto a mais.")]
        [SerializeField] private int chassisPrice = 25;

        private ScrapController _customer;
        private int _sold;

        /// <summary>O Crisol com a loja aberta, se houver. O HUD usa pra desenhar o painel.</summary>
        public static Crisol ShopOpen { get; private set; }

        public int ChassisPrice => chassisPrice * (_sold + 1);

        /// <summary>O que aconteceu na ultima tentativa de compra, pra mostrar no painel.</summary>
        public string LastMessage { get; private set; } = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayShop()
        {
            ShopOpen = null;
        }

        protected override void OnTalkFinished(ScrapController scrap)
        {
            ScrapGems.GiveRuby();

            _customer = scrap;
            _customer.ControlLocked = true;
            LastMessage = "";
            ShopOpen = this;
        }

        private void LateUpdate()
        {
            if (ShopOpen != this) return;

            Keyboard keyboard = Keyboard.current;
            Gamepad pad = Gamepad.current;

            bool buy = (keyboard != null && (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame))
                || (pad != null && pad.buttonSouth.wasPressedThisFrame);

            bool close = (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame))
                || (pad != null && (pad.buttonEast.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame));

            if (buy) Buy();
            else if (close) Close();
        }

        private void Buy()
        {
            if (ScrapWallet.Shards < ChassisPrice)
            {
                LastMessage = "Shards de menos. Volte quando tiver com o que pagar.";
                return;
            }

            var health = _customer != null ? _customer.GetComponent<ScrapHealth>() : null;
            if (health == null) return;

            ScrapWallet.Add(-ChassisPrice);
            health.UpgradeChassis();
            _sold++;

            LastMessage = "Pronto. Nao me faca me arrepender.";
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
