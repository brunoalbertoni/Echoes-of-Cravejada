using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um personagem com quem da pra conversar. Chegar perto mostra o aviso;
    /// apertar o botao abre a fala, e apertar de novo passa pra proxima ate
    /// acabar. Enquanto conversa, o Scrap fica parado.
    ///
    /// As falas sao so texto, uma por tela, mostradas pelo GameHud. Depois da
    /// purificacao ele pode dizer outras; e quem herda daqui pode escolher as
    /// falas pelo que o Scrap ja fez (PickLines).
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Npc : MonoBehaviour
    {
        [SerializeField] private string displayName = "???";

        [TextArea(2, 4)]
        [SerializeField] private string[] lines;

        [Tooltip("O que ele diz depois que a area e purificada. Vazio repete as de cima.")]
        [TextArea(2, 4)]
        [SerializeField] private string[] purifiedLines;

        [Tooltip("O que aparece no aviso antes do nome: 'conversar com', 'ler'...")]
        [SerializeField] private string prompt = "conversar com";

        [Tooltip("Se vira pro Scrap. Desligado pra pagina, pichacao e placa, que nao viram.")]
        [SerializeField] private bool facesPlayer = true;

        [Tooltip("Pro que se le: o desenho grande (a carta, a placa, o muro) mostrado na tela durante a leitura.")]
        [SerializeField] private Sprite document;

        [Tooltip("O desenho de depois da purificacao, se ele muda. Vazio repete o de cima.")]
        [SerializeField] private Sprite purifiedDocument;

        // O desenho desta leitura, escolhido junto com as falas.
        private Sprite _document;

        private ScrapController _controller;
        private SpriteRenderer _sprite;
        private int _line = -1;

        // As falas desta conversa, escolhidas quando ela comeca: o que o Scrap
        // faz no meio (ou no fim) dela nao troca o texto na tela.
        private string[] _talk;

        /// <summary>O personagem perto do qual o Scrap esta, se houver.</summary>
        public static Npc Nearby { get; private set; }

        /// <summary>O personagem que esta falando agora, se houver.</summary>
        public static Npc Talking { get; private set; }

        public string DisplayName => displayName;

        /// <summary>
        /// O desenho de quem fala, pro retrato na caixa de dialogo: o sprite
        /// dele, ou o do filho quando o corpo fica num filho (a Assistente, no
        /// armario). Nulo pro que se le — pagina, placa, gravura nao tem rosto.
        /// </summary>
        public SpriteRenderer Portrait { get; private set; }

        /// <summary>O verbo do aviso: "conversar com" pra gente, "ler" pra coisa escrita.</summary>
        public string Prompt => prompt;

        /// <summary>
        /// O desenho grande do que se esta lendo agora (a carta, a placa), ou
        /// null pra quem so fala.
        /// </summary>
        public Sprite Document => Talking == this ? _document : null;

        /// <summary>A fala na tela agora, ou null se ele nao esta falando.</summary>
        public string CurrentLine => _line >= 0 && _talk != null && _line < _talk.Length ? _talk[_line] : null;

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

        /// <summary>Usado pelos geradores de fase: o que ele diz depois da purificacao.</summary>
        public void ConfigurePurified(string[] newLines)
        {
            purifiedLines = newLines;
        }

        /// <summary>
        /// Usado pelos geradores de fase: o desenho grande mostrado durante a
        /// leitura, e o de depois da purificacao, se muda.
        /// </summary>
        public void ConfigureDocument(Sprite newDocument, Sprite newPurifiedDocument = null)
        {
            document = newDocument;
            purifiedDocument = newPurifiedDocument;
        }

        /// <summary>As falas de sempre, as configuradas primeiro (antes da purificacao).</summary>
        protected string[] FirstLines => lines;

        /// <summary>
        /// As falas da proxima conversa. Por padrao, as de depois da
        /// purificacao, se a area ja foi purificada e ele tiver alguma.
        /// </summary>
        protected virtual string[] PickLines()
        {
            bool purified = AreaPurifier.IsPurified && purifiedLines != null && purifiedLines.Length > 0;
            return purified ? purifiedLines : lines;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();

            // So quem conversa vira o rosto; e so quem conversa tem retrato.
            if (facesPlayer) Portrait = _sprite != null ? _sprite : GetComponentInChildren<SpriteRenderer>();
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (_controller == null) return;

            // Com o card de conquista na tela, o E e o de fechar o card.
            if (AbilityCard.Showing) return;

            // Com a loja aberta, o botao de conversar e o de fechar a loja.
            if (Crisol.ShopOpen != null) return;

            // Numa cena o E e o de pular a cena, nao o de conversar.
            if (Cinematic.Active) return;

            // Olha pra quem esta falando com ele. A arte olha pra direita.
            if (_sprite != null && facesPlayer) _sprite.flipX = _controller.transform.position.x < transform.position.x;

            if (!InteractPressed()) return;

            // So comeca a conversa com os pes no chao; continuar pode a qualquer hora.
            if (_line < 0 && !_controller.IsGrounded) return;

            if (_line < 0)
            {
                _talk = PickLines();

                // O desenho acompanha o texto: o de depois, se o texto e o de depois.
                bool after = _talk != null && _talk == purifiedLines && purifiedDocument != null;
                _document = after ? purifiedDocument : document;
            }

            _line++;

            if (_talk == null || _line >= _talk.Length)
            {
                EndTalk(true);
                return;
            }

            Talking = this;
            _controller.ControlLocked = true;
        }

        /// <summary>
        /// Comeca uma conversa com estas falas, agora, sem esperar o botao. O
        /// Crisol usa pra contar a memoria que acabou de vender.
        /// </summary>
        protected void StartTalk(string[] newLines)
        {
            if (_controller == null || newLines == null || newLines.Length == 0) return;

            _talk = newLines;
            _document = document;
            _line = 0;
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
