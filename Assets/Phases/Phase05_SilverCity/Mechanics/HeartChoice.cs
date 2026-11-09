using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O encaixe do nucleo, no Altar do Coracao. Com o Rei caido, segura-se o
    /// botao de pe ao lado dele e o Scrap entrega o nucleo limpo a Coluna: a
    /// luz desce ate o Coracao de Quartzo e o Oxide vai embora (O Grande
    /// Polimento, o final do jogo).
    ///
    /// Nao ha outra escolha nem conta de purificacao: ninguem chega ao palacio
    /// sem ter limpado os quatro distritos.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    public class HeartChoice : MonoBehaviour
    {
        [SerializeField] private FinaleDirector director;
        [SerializeField] private float holdTime = 2f;

        private ScrapController _controller;
        private float _progress;
        private bool _holding;
        private bool _chosen;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(FinaleDirector newDirector)
        {
            director = newDirector;
        }

        private void Update()
        {
            if (_chosen || _controller == null) return;
            if (!SilverKing.Defeated) return;

            bool held = InteractHeld() && _controller.IsGrounded;
            if (held != _holding)
            {
                _holding = held;
                _controller.ControlLocked = held;
                if (!held) _progress = 0f;
            }

            if (_holding) _progress += Time.deltaTime;

            HoldAction.Show(this, "Segure E: entregar o núcleo limpo à Coluna", _progress / holdTime);

            if (_progress < holdTime) return;

            _chosen = true;
            HoldAction.Hide(this);
            if (director != null) director.Begin();
        }

        private static bool InteractHeld()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.isPressed || keyboard.kKey.isPressed)) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonNorth.isPressed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller != null) _controller = controller;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_controller == null || other.GetComponentInParent<ScrapController>() != _controller) return;

            if (_holding && !_chosen) _controller.ControlLocked = false;
            _holding = false;
            _progress = 0f;
            _controller = null;
            HoldAction.Hide(this);
        }
    }
}
