using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O botao de uma estatua do Salao das Estatuas. De pe, perto dela,
    /// aperta-se E e o botao afunda com um clangor — um ruido forte, que vem
    /// da estatua e acorda a sala. Quem decide se foi a vez certa e o
    /// StatueHall; aqui so se aperta, se afunda e se acende.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class StatueButton : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("order")] [Tooltip("A vez desta estatua na sequencia (1 a 4).")]
        [SerializeField] private int ordem;

        [UnityEngine.Serialization.FormerlySerializedAs("button")] [SerializeField] private SpriteRenderer botao;
        [UnityEngine.Serialization.FormerlySerializedAs("buttonUp")] [SerializeField] private Sprite cimaBotao;
        [UnityEngine.Serialization.FormerlySerializedAs("buttonDown")] [SerializeField] private Sprite baixoBotao;

        [UnityEngine.Serialization.FormerlySerializedAs("glow")] [Tooltip("Os olhos (ou a fenda) acesos: so aparecem enquanto a sequencia vale.")]
        [SerializeField] private SpriteRenderer brilho;
        [UnityEngine.Serialization.FormerlySerializedAs("glowLight")] [SerializeField] private Light2D luzBrilho;

        [UnityEngine.Serialization.FormerlySerializedAs("noiseRadius")] [Tooltip("Ate onde o clangor se ouve, em tiles, e a forca dele.")]
        [SerializeField] private float raioRuido = 12f;
        [UnityEngine.Serialization.FormerlySerializedAs("noiseLoudness")] [SerializeField, Range(0f, 1f)] private float volumeRuido = 0.9f;

        private ScrapController _controlador;
        private float _baixoAte;
        private bool _aceso;

        /// <summary>A estatua perto da qual o Scrap esta, se houver. O HUD usa pro aviso.</summary>
        public static StatueButton Proximo { get; private set; }

        /// <summary>Se o salao ainda escuta os botoes. Desligado depois que a porta abre.</summary>
        public bool Habilitado { get; set; } = true;

        public int Ordem => ordem;

        /// <summary>Avisa que o botao foi apertado. O StatueHall escuta.</summary>
        public event System.Action<StatueButton> Apertado;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Proximo = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(int novoOrdem, SpriteRenderer novoBotao, Sprite novoCimaBotao, Sprite novoBaixoBotao,
            SpriteRenderer novoBrilho, Light2D novoLuzBrilho)
        {
            ordem = novoOrdem;
            botao = novoBotao;
            cimaBotao = novoCimaBotao;
            baixoBotao = novoBaixoBotao;
            brilho = novoBrilho;
            luzBrilho = novoLuzBrilho;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Start()
        {
            DefinirAceso(false);
        }

        /// <summary>Acende ou apaga os olhos da estatua.</summary>
        public void DefinirAceso(bool aceso)
        {
            _aceso = aceso;
            if (brilho != null) brilho.enabled = aceso;
            if (luzBrilho != null) luzBrilho.enabled = aceso;
        }

        public bool EstaAceso => _aceso;

        private void Update()
        {
            if (botao != null && cimaBotao != null && baixoBotao != null)
            {
                botao.sprite = Time.time < _baixoAte ? baixoBotao : cimaBotao;
            }

            if (!Habilitado || _controlador == null) return;
            if (AbilityCard.Mostrando || Crisol.AbertoLoja != null || Cinematic.Ativo || Npc.Conversando != null) return;
            if (!_controlador.EstaNoChao || _controlador.ControleTravado) return;
            if (!InteracaoPressionada()) return;

            _baixoAte = Time.time + 0.5f;
            NoiseSystem.Emitir(transform.position + Vector3.up, raioRuido, volumeRuido);
            Sfx.ApertarEstatua.Tocar(transform.position);
            Apertado?.Invoke(this);
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

            _controlador = null;
            if (Proximo == this) Proximo = null;
        }

        private void OnDisable()
        {
            if (Proximo == this) Proximo = null;
        }
    }
}
