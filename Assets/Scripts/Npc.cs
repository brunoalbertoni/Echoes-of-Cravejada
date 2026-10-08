using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um personagem com quem da pra conversar. Chegar perto mostra o aviso;
    /// apertar o botao abre a fala, e apertar de novo passa pra proxima ate
    /// acabar. Enquanto conversa, o Scrap fica parado.
    ///
    /// As falas sao so texto, uma por tela, mostradas pelo GameHud.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Npc : MonoBehaviour
    {
        [SerializeField] private string displayName = "???";

        [TextArea(2, 4)]
        [SerializeField] private string[] lines;

        [Tooltip("O que aparece no aviso antes do nome: 'conversar com', 'ler'...")]
        [SerializeField] private string prompt = "conversar com";

        [Tooltip("Se vira pro Scrap. Desligado pra pagina, pichacao e placa, que nao viram.")]
        [SerializeField] private bool facesPlayer = true;

        private ScrapController _controller;
        private SpriteRenderer _sprite;
        private int _line = -1;

        /// <summary>O personagem perto do qual o Scrap esta, se houver.</summary>
        public static Npc Nearby { get; private set; }

        /// <summary>O personagem que esta falando agora, se houver.</summary>
        public static Npc Talking { get; private set; }

        public string DisplayName => displayName;

        /// <summary>O verbo do aviso: "conversar com" pra gente, "ler" pra coisa escrita.</summary>
        public string Prompt => prompt;

        /// <summary>A fala na tela agora, ou null se ele nao esta falando.</summary>
        public string CurrentLine => _line >= 0 && lines != null && _line < lines.Length ? lines[_line] : null;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Nearby = null;
            Talking = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(string newName, string[] newLines)
        {
            displayName = newName;
            lines = newLines;
        }

        /// <summary>Usado pelos geradores de fase, pra coisas que se leem em vez de conversar.</summary>
        public void ConfigureReadable(string newName, string[] newLines)
        {
            displayName = newName;
            lines = newLines;
            prompt = "ler";
            facesPlayer = false;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (_controller == null) return;

            // Com a loja aberta, o botao de conversar e o de fechar a loja.
            if (Crisol.ShopOpen != null) return;

            // Olha pra quem esta falando com ele. A arte olha pra direita.
            if (_sprite != null && facesPlayer) _sprite.flipX = _controller.transform.position.x < transform.position.x;

            if (!InteractPressed()) return;

            // So comeca a conversa com os pes no chao; continuar pode a qualquer hora.
            if (_line < 0 && !_controller.IsGrounded) return;

            _line++;

            if (lines == null || _line >= lines.Length)
            {
                EndTalk(true);
                return;
            }

            Talking = this;
            _controller.ControlLocked = true;
        }

        private void EndTalk(bool finished = false)
        {
            // So conta como conversa terminada se ele ouviu ate o fim.
            bool wasTalking = finished && _line >= 0;

            _line = -1;
            if (Talking == this) Talking = null;
            if (_controller != null) _controller.ControlLocked = false;

            if (wasTalking && _controller != null) OnTalkFinished(_controller);
        }

        /// <summary>Chamado quando a conversa termina. Crisol usa pra entregar o rubi e abrir a loja.</summary>
        protected virtual void OnTalkFinished(ScrapController scrap)
        {
        }

        private static bool InteractPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame)) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonNorth.wasPressedThisFrame;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            _controller = controller;
            Nearby = this;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_controller == null || other.GetComponentInParent<ScrapController>() != _controller) return;

            // Foi tirado dali no meio da conversa (apanhou, morreu): encerra.
            EndTalk();

            _controller = null;
            if (Nearby == this) Nearby = null;
        }

        private void OnDisable()
        {
            EndTalk();
            if (Nearby == this) Nearby = null;
        }
    }
}
