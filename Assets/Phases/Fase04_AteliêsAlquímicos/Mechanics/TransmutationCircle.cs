using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um circulo de transmutacao gravado em prata no chao: a purificacao local
    /// do District 4. Segurar o botao em cima dele faz o Scrap raspar e polir a
    /// runa do centro; quando termina, o circulo acende e transmuta um
    /// material da sala em outro:
    ///
    ///   vidro em prata       a parede some, a ponte aparece
    ///   mercurio em vapor    a poca some, a coluna de vapor aparece
    ///   marmore em areia     o bloco se desfaz
    ///
    /// Pro circulo isso e sempre a mesma coisa: uma lista de objetos que somem
    /// e outra dos que aparecem.
    ///
    /// Enquanto raspa o Scrap nao anda nem pula: fica vulneravel. Apanhar no
    /// meio joga fora o que ja tinha raspado.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TransmutationCircle : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("scrapeTime")] [Tooltip("Segundos segurando ate a runa acender.")]
        [SerializeField] private float tempoRaspagem = 2.4f;

        [UnityEngine.Serialization.FormerlySerializedAs("effect")] [Tooltip("O que o aviso diz que ele transmuta. Ex.: \"vidro em prata\".")]
        [SerializeField] private string efeito = "a matéria";

        [UnityEngine.Serialization.FormerlySerializedAs("vanish")] [Header("O que ele muda")]
        [SerializeField] private GameObject[] sumir;
        [UnityEngine.Serialization.FormerlySerializedAs("appear")] [SerializeField] private GameObject[] aparecer;

        [UnityEngine.Serialization.FormerlySerializedAs("rune")] [Header("Visual")]
        [SerializeField] private SpriteRenderer runa;
        [UnityEngine.Serialization.FormerlySerializedAs("litColor")] [SerializeField] private Color corAceso = new Color(0.75f, 0.95f, 1f);

        private ScrapController _controlador;
        private ScrapHealth _vida;
        private float _progresso;
        private bool _raspando;
        private bool _aceso;

        public bool EstaAceso => _aceso;

        /// <summary>Avisa quando a runa acende (o palco do District 4 purifica com isso).</summary>
        public event System.Action QuadroAceso;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(string novoEfeito, GameObject[] novoSumir, GameObject[] novoAparecer, SpriteRenderer novoRuna)
        {
            efeito = novoEfeito;
            sumir = novoSumir;
            aparecer = novoAparecer;
            runa = novoRuna;
        }

        private void Start()
        {
            // O que ele cria ainda nao existe.
            Definir(aparecer, false);
        }

        private static void Definir(GameObject[] objetos, bool ativo)
        {
            if (objetos == null) return;

            foreach (GameObject alvo in objetos)
            {
                if (alvo != null) alvo.SetActive(ativo);
            }
        }

        private void Update()
        {
            if (_aceso || _controlador == null) return;

            bool segurando = InteracaoSegurada() && _controlador.EstaNoChao;

            // Apanhou no meio: perde o que tinha raspado.
            if (_raspando && _vida != null && Time.time - _vida.UltimoTempoGolpe < 0.1f)
            {
                _progresso = 0f;
                segurando = false;
            }

            if (segurando != _raspando)
            {
                _raspando = segurando;
                _controlador.ControleTravado = segurando;
            }

            if (_raspando) _progresso += Time.deltaTime;

            HoldAction.Mostrar(this, "Segure E para polir a runa: " + efeito, _progresso / tempoRaspagem);

            if (_progresso < tempoRaspagem) return;

            _aceso = true;
            _raspando = false;
            _controlador.ControleTravado = false;
            HoldAction.Esconder(this);

            if (runa != null) runa.color = corAceso;

            Definir(sumir, false);
            Definir(aparecer, true);

            // A transmutacao estala: quem escuta, escuta.
            NoiseSystem.Emitir(transform.position, 10f, 0.5f);
            Sfx.AtivarCirculo.Tocar(transform.position);

            QuadroAceso?.Invoke();
        }

        private static bool InteracaoSegurada()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.eKey.isPressed || teclado.kKey.isPressed)) return true;

            Gamepad controle = Gamepad.current;
            return controle != null && controle.buttonNorth.isPressed;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var controlador = outro.GetComponentInParent<ScrapController>();
            if (controlador == null) return;

            _controlador = controlador;
            _vida = controlador.GetComponent<ScrapHealth>();
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (_controlador == null || outro.GetComponentInParent<ScrapController>() != _controlador) return;

            if (_raspando) _controlador.ControleTravado = false;

            _raspando = false;
            _controlador = null;
            _vida = null;
            HoldAction.Esconder(this);
        }
    }
}
