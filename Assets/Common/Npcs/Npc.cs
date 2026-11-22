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
        [UnityEngine.Serialization.FormerlySerializedAs("displayName")] [SerializeField] private string nomeExibido = "???";

        [UnityEngine.Serialization.FormerlySerializedAs("lines")] [TextArea(2, 4)]
        [SerializeField] private string[] linhas2;

        [UnityEngine.Serialization.FormerlySerializedAs("purifiedLines")] [Tooltip("O que ele diz depois que a area e purificada. Vazio repete as de cima.")]
        [TextArea(2, 4)]
        [SerializeField] private string[] linhasPurificado;

        [UnityEngine.Serialization.FormerlySerializedAs("prompt")] [Tooltip("O que aparece no aviso antes do nome: 'conversar com', 'ler'...")]
        [SerializeField] private string indicacao = "conversar com";

        [UnityEngine.Serialization.FormerlySerializedAs("facesPlayer")] [Tooltip("Se vira pro Scrap. Desligado pra pagina, pichacao e placa, que nao viram.")]
        [SerializeField] private bool jogadorEncara = true;

        [UnityEngine.Serialization.FormerlySerializedAs("document")] [Tooltip("Pro que se le: o desenho grande (a carta, a placa, o muro) mostrado na tela durante a leitura.")]
        [SerializeField] private Sprite documento;

        [UnityEngine.Serialization.FormerlySerializedAs("purifiedDocument")] [Tooltip("O desenho de depois da purificacao, se ele muda. Vazio repete o de cima.")]
        [SerializeField] private Sprite documentoPurificado;

        // O desenho desta leitura, escolhido junto com as falas.
        private Sprite _documento;

        private ScrapController _controlador;
        private SpriteRenderer _sprite;
        private int _linha = -1;

        // As falas desta conversa, escolhidas quando ela comeca: o que o Scrap
        // faz no meio (ou no fim) dela nao troca o texto na tela.
        private string[] _conversa;

        /// <summary>O personagem perto do qual o Scrap esta, se houver.</summary>
        public static Npc Proximo { get; private set; }

        /// <summary>O personagem que esta falando agora, se houver.</summary>
        public static Npc Conversando { get; private set; }

        public string NomeExibido => nomeExibido;

        /// <summary>
        /// O desenho de quem fala, pro retrato na caixa de dialogo: o sprite
        /// dele, ou o do filho quando o corpo fica num filho (a Assistente, no
        /// armario). Nulo pro que se le — pagina, placa, gravura nao tem rosto.
        /// </summary>
        public SpriteRenderer Retrato { get; private set; }

        /// <summary>
        /// O busto de quem fala na caixa de dialogo (<see cref="SpeakerPortraits"/>),
        /// pelo nome. Nulo pro que se le: pagina, placa e gravura nao tem rosto.
        /// </summary>
        public string IdRetrato => jogadorEncara ? SpeakerPortraits.IdDe(nomeExibido) : null;

        /// <summary>O verbo do aviso: "conversar com" pra gente, "ler" pra coisa escrita.</summary>
        public string Indicacao => indicacao;

        /// <summary>
        /// O desenho grande do que se esta lendo agora (a carta, a placa), ou
        /// null pra quem so fala.
        /// </summary>
        public Sprite Documento => Conversando == this ? _documento : null;

        /// <summary>A fala na tela agora, ou null se ele nao esta falando.</summary>
        public string LinhaAtual => _linha >= 0 && _conversa != null && _linha < _conversa.Length ? _conversa[_linha] : null;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Proximo = null;
            Conversando = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(string novoNome, string[] novoLinhas)
        {
            nomeExibido = novoNome;
            linhas2 = novoLinhas;
        }

        /// <summary>Usado pelos geradores de fase, pra coisas que se leem em vez de conversar.</summary>
        public void ConfigurarLeitura(string novoNome, string[] novoLinhas)
        {
            nomeExibido = novoNome;
            linhas2 = novoLinhas;
            indicacao = "ler";
            jogadorEncara = false;
        }

        /// <summary>Usado pelos geradores de fase: o que ele diz depois da purificacao.</summary>
        public void ConfigurarPurificado(string[] novoLinhas)
        {
            linhasPurificado = novoLinhas;
        }

        /// <summary>
        /// Usado pelos geradores de fase: o desenho grande mostrado durante a
        /// leitura, e o de depois da purificacao, se muda.
        /// </summary>
        public void ConfigurarDocumento(Sprite novoDocumento, Sprite novoDocumentoPurificado = null)
        {
            documento = novoDocumento;
            documentoPurificado = novoDocumentoPurificado;
        }

        /// <summary>As falas de sempre, as configuradas primeiro (antes da purificacao).</summary>
        protected string[] PrimeiroLinhas => linhas2;

        /// <summary>
        /// As falas da proxima conversa. Por padrao, as de depois da
        /// purificacao, se a area ja foi purificada e ele tiver alguma.
        /// </summary>
        protected virtual string[] EscolherFalas()
        {
            bool purificado = AreaPurifier.EstaPurificado && linhasPurificado != null && linhasPurificado.Length > 0;
            return purificado ? linhasPurificado : linhas2;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();

            // So quem conversa vira o rosto; e so quem conversa tem retrato.
            if (jogadorEncara) Retrato = _sprite != null ? _sprite : GetComponentInChildren<SpriteRenderer>();
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (_controlador == null) return;

            // Com o card de conquista na tela, o E e o de fechar o card.
            if (AbilityCard.Mostrando) return;

            // Com a loja aberta, o botao de conversar e o de fechar a loja.
            if (Crisol.AbertoLoja != null) return;

            // Numa cena o E e o de pular a cena, nao o de conversar.
            if (Cinematic.Ativo) return;

            // Olha pra quem esta falando com ele. A arte olha pra direita.
            if (_sprite != null && jogadorEncara) _sprite.flipX = _controlador.transform.position.x < transform.position.x;

            if (!InteracaoPressionada()) return;

            // So comeca a conversa com os pes no chao; continuar pode a qualquer hora.
            if (_linha < 0 && !_controlador.EstaNoChao) return;

            if (_linha < 0)
            {
                _conversa = EscolherFalas();

                // O desenho acompanha o texto: o de depois, se o texto e o de depois.
                bool depois = _conversa != null && _conversa == linhasPurificado && documentoPurificado != null;
                _documento = depois ? documentoPurificado : documento;
            }

            _linha++;

            if (_conversa == null || _linha >= _conversa.Length)
            {
                if (_linha > 0) Sfx.FecharDialogo.Tocar();
                EncerrarConversa(true);
                return;
            }

            if (_linha == 0) Sfx.AbertoDialogo.Tocar();
            else Sfx.ProximoDialogo.Tocar();

            Conversando = this;
            _controlador.ControleTravado = true;
        }

        /// <summary>
        /// Comeca uma conversa com estas falas, agora, sem esperar o botao. O
        /// Crisol usa pra contar a memoria que acabou de vender.
        /// </summary>
        protected void IniciarConversa(string[] novoLinhas)
        {
            if (_controlador == null || novoLinhas == null || novoLinhas.Length == 0) return;

            _conversa = novoLinhas;
            _documento = documento;
            _linha = 0;
            Conversando = this;
            Sfx.AbertoDialogo.Tocar();
            _controlador.ControleTravado = true;
        }

        private void EncerrarConversa(bool terminado = false)
        {
            // So conta como conversa terminada se ele ouviu ate o fim.
            bool estavaConversando = terminado && _linha >= 0;

            _linha = -1;
            if (Conversando == this) Conversando = null;
            if (_controlador != null) _controlador.ControleTravado = false;

            if (estavaConversando && _controlador != null) AoTerminarConversa(_controlador);
        }

        /// <summary>Chamado quando a conversa termina. Crisol usa pra entregar o rubi e abrir a loja.</summary>
        protected virtual void AoTerminarConversa(ScrapController sucata)
        {
        }

        private static bool InteracaoPressionada()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.eKey.wasPressedThisFrame || teclado.kKey.wasPressedThisFrame)) return true;

            Gamepad controle = Gamepad.current;
            return controle != null && controle.buttonNorth.wasPressedThisFrame;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var controlador = outro.GetComponentInParent<ScrapController>();
            if (controlador == null) return;

            _controlador = controlador;
            Proximo = this;
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (_controlador == null || outro.GetComponentInParent<ScrapController>() != _controlador) return;

            // Foi tirado dali no meio da conversa (apanhou, morreu): encerra.
            EncerrarConversa();

            _controlador = null;
            if (Proximo == this) Proximo = null;
        }

        private void OnDisable()
        {
            EncerrarConversa();
            if (Proximo == this) Proximo = null;
        }
    }
}
