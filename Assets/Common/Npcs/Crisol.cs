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
        [UnityEngine.Serialization.FormerlySerializedAs("memoryLines")] [Tooltip("A historia que ele vende como 'Memoria'. Vazio: nao ha memoria pra vender neste distrito.")]
        [TextArea(2, 4)]
        [SerializeField] private string[] linhasMemoria;

        [UnityEngine.Serialization.FormerlySerializedAs("givesRuby")] [Tooltip("Se a primeira conversa entrega o rubi bruto. So no distrito em que ele aparece pela primeira vez.")]
        [SerializeField] private bool rubiEntrega = true;

        [UnityEngine.Serialization.FormerlySerializedAs("repeatLines")] [Tooltip("O que ele diz das outras vezes, depois da primeira conversa. Vazio: repete a primeira.")]
        [TextArea(2, 4)]
        [SerializeField] private string[] linhasRepetir;

        // A primeira conversa de cada distrito e a apresentacao; as outras sao curtas.
        private string MarcaConhecido => "Crisol_Met_" + gameObject.scene.name;

        private ScrapController _cliente;
        private readonly List<ShopItem> _itens = new List<ShopItem>();

        // O E que termina a conversa abre a loja; no mesmo quadro ele nao pode fechar.
        private int _quadroAbertos = -1;

        /// <summary>O Crisol com a loja aberta, se houver. O HUD usa pra desenhar o painel.</summary>
        public static Crisol AbertoLoja { get; private set; }

        /// <summary>O que esta a venda aqui, na ordem do painel.</summary>
        public IReadOnlyList<ShopItem> Itens => _itens;

        /// <summary>O item marcado no painel.</summary>
        public int Selecionado { get; private set; }

        /// <summary>O que ele disse da ultima tentativa de compra, pra mostrar no painel.</summary>
        public string UltimoMensagem { get; private set; } = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarLojaAoJogar()
        {
            AbertoLoja = null;
        }

        /// <summary>Usado pelos geradores de fase: a memoria que ele vende neste distrito.</summary>
        public void ConfigurarMemoria(string[] linhas2)
        {
            linhasMemoria = linhas2;
        }

        /// <summary>Usado pelos geradores de fase: se ele entrega o rubi neste distrito.</summary>
        public void ConfigurarEntregaRubi(bool entrega)
        {
            rubiEntrega = entrega;
        }

        /// <summary>Usado pelos geradores de fase: o que ele diz depois da primeira conversa.</summary>
        public void ConfigurarRepeticao(string[] linhas2)
        {
            linhasRepetir = linhas2;
        }

        protected override string[] EscolherFalas()
        {
            // A primeira vez neste distrito, a apresentacao inteira.
            if (!KeyItems.TemMarca(MarcaConhecido)) return PrimeiroLinhas;

            // Depois: o que mudou com a purificacao, ou a frase curta de sempre.
            string[] escolhido = base.EscolherFalas();
            bool temRepetir = linhasRepetir != null && linhasRepetir.Length > 0;
            return escolhido == PrimeiroLinhas && temRepetir ? linhasRepetir : escolhido;
        }

        protected override void AoTerminarConversa(ScrapController sucata)
        {
            KeyItems.DefinirMarca(MarcaConhecido);
            if (rubiEntrega) ScrapGems.DarRubi();
            Abrir(sucata);
        }

        private void Abrir(ScrapController sucata)
        {
            _itens.Clear();
            foreach (ShopItem item in Shop.Catalogo)
            {
                bool temMemoria = linhasMemoria != null && linhasMemoria.Length > 0;
                if (item.EstaMemoria && !temMemoria) continue;

                _itens.Add(item);
            }

            _cliente = sucata;
            _cliente.ControleTravado = true;
            Selecionado = Mathf.Clamp(Selecionado, 0, Mathf.Max(0, _itens.Count - 1));
            UltimoMensagem = "";
            AbertoLoja = this;
            _quadroAbertos = Time.frameCount;
        }

        private void LateUpdate()
        {
            if (AbertoLoja != this || Time.frameCount == _quadroAbertos) return;

            Keyboard teclado = Keyboard.current;
            Gamepad controle = Gamepad.current;

            bool up = (teclado != null && (teclado.upArrowKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame))
                || (controle != null && controle.dpad.up.wasPressedThisFrame);
            bool baixo2 = (teclado != null && (teclado.downArrowKey.wasPressedThisFrame || teclado.sKey.wasPressedThisFrame))
                || (controle != null && controle.dpad.down.wasPressedThisFrame);
            bool comprar = (teclado != null && (teclado.spaceKey.wasPressedThisFrame || teclado.enterKey.wasPressedThisFrame))
                || (controle != null && controle.buttonSouth.wasPressedThisFrame);
            bool fechar = (teclado != null && (teclado.escapeKey.wasPressedThisFrame || teclado.eKey.wasPressedThisFrame || teclado.kKey.wasPressedThisFrame))
                || (controle != null && (controle.buttonEast.wasPressedThisFrame || controle.buttonNorth.wasPressedThisFrame));

            // O numero do item escolhe e compra de uma vez.
            if (teclado != null)
            {
                for (int i = 0; i < _itens.Count && i < 9; i++)
                {
                    if (!teclado[Key.Digit1 + i].wasPressedThisFrame && !teclado[Key.Numpad1 + i].wasPressedThisFrame) continue;

                    Selecionado = i;
                    comprar = true;
                }
            }

            if (_itens.Count > 0)
            {
                if (up) Selecionado = (Selecionado + _itens.Count - 1) % _itens.Count;
                if (baixo2) Selecionado = (Selecionado + 1) % _itens.Count;
                if ((up || baixo2) && !comprar) Sfx.MoverLoja.Tocar();
            }

            if (comprar) Comprar();
            else if (fechar)
            {
                Sfx.FecharDialogo.Tocar();
                Fechar();
            }
        }

        /// <summary>O mouse passou por cima de uma fileira: ela fica marcada.</summary>
        public void PassarMouse(int indice)
        {
            if (AbertoLoja != this || indice < 0 || indice >= _itens.Count || indice == Selecionado) return;
            Selecionado = indice;
            Sfx.MoverLoja.Tocar();
        }

        /// <summary>Clique numa fileira: marca e compra, como o numero do item.</summary>
        public void ClicarComprar(int indice)
        {
            if (AbertoLoja != this || indice < 0 || indice >= _itens.Count) return;
            Selecionado = indice;
            Comprar();
        }

        /// <summary>Clique em "sair" ou fora da loja.</summary>
        public void ClicarFechar()
        {
            if (AbertoLoja != this) return;
            Sfx.FecharDialogo.Tocar();
            Fechar();
        }

        private void Comprar()
        {
            if (_itens.Count == 0) return;

            ShopItem item = _itens[Selecionado];
            var vida = _cliente != null ? _cliente.GetComponent<ScrapHealth>() : null;

            UltimoMensagem = Shop.TentarComprar(item, vida, out bool comprados);
            (comprados ? Sfx.ComprarLoja : Sfx.NegarLoja).Tocar();

            if (comprados && item.EstaMemoria)
            {
                // Fecha a loja e conta. Ao fim da historia, a loja reabre.
                ScrapController cliente = _cliente;
                Fechar();
                if (cliente != null) cliente.ControleTravado = true;
                IniciarConversa(linhasMemoria);
            }
        }

        private void Fechar()
        {
            if (_cliente != null) _cliente.ControleTravado = false;

            _cliente = null;
            AbertoLoja = null;
        }

        private void OnDestroy()
        {
            if (AbertoLoja == this) AbertoLoja = null;
        }
    }
}
