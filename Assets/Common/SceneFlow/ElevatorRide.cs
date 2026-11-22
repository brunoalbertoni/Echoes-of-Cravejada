using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um elevador de estacao: a saida pro distrito de cima. O Scrap entra na
    /// cabine e segura o botao de interagir; a cabine sobe o poco inteiro com
    /// ele dentro, a tela escurece no alto e a proxima cena carrega.
    ///
    /// Se o poco estiver entupido (o objeto em Blocker ainda ativo, como os
    /// escombros do District 3), a cabine nao sai: so aparece o aviso.
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ElevatorRide : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("topY")] [Tooltip("Altura (y de mundo) em que a cabine chega no alto e a cena troca.")]
        [SerializeField] private float topoY;

        [UnityEngine.Serialization.FormerlySerializedAs("sceneName")] [SerializeField] private string nomeCena;
        [UnityEngine.Serialization.FormerlySerializedAs("spawnId")] [SerializeField] private string idNascimento;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [Tooltip("Tiles por segundo, na velocidade de cruzeiro.")]
        [SerializeField] private float velocidade = 6f;

        [UnityEngine.Serialization.FormerlySerializedAs("holdTime")] [Tooltip("Segundos segurando o botao ate a cabine partir.")]
        [SerializeField] private float tempoSegurar = 0.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("blocker")] [Tooltip("O que entope o poco. Enquanto estiver ativo, a cabine nao sobe.")]
        [SerializeField] private GameObject bloqueador;

        [UnityEngine.Serialization.FormerlySerializedAs("blockedNotice")] [Tooltip("O aviso de quando o poco esta entupido.")]
        [SerializeField] private string avisoBloqueado = "O poço está bloqueado.";

        private ScrapController _controlador;
        private float _segurado;
        private bool _viajando;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoTopoY, string novoNomeCena, string novoIdNascimento)
        {
            topoY = novoTopoY;
            nomeCena = novoNomeCena;
            idNascimento = novoIdNascimento;
        }

        /// <summary>Usado pelos geradores de fase: o que entope o poco, e o aviso.</summary>
        /// <summary>Usado pelos geradores de fase: a velocidade de cruzeiro, pra pocos longos.</summary>
        public void ConfigurarVelocidade(float novoVelocidade)
        {
            velocidade = novoVelocidade;
        }

        public void ConfigurarBloqueio(GameObject novoBloqueador, string novoAviso)
        {
            bloqueador = novoBloqueador;
            avisoBloqueado = novoAviso;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (_viajando || _controlador == null) return;

            // Bloqueado: os escombros ja dizem tudo.
            if (bloqueador != null && bloqueador.activeInHierarchy)
            {
                HoldAction.Esconder(this);
                _segurado = 0f;
                return;
            }

            if (InteracaoSegurada() && _controlador.EstaNoChao) _segurado += Time.deltaTime;
            else _segurado = 0f;

            HoldAction.Mostrar(this, "Segure E para subir no elevador", _segurado / tempoSegurar);

            if (_segurado >= tempoSegurar)
            {
                HoldAction.Esconder(this);
                StartCoroutine(Viajar());
            }
        }

        private IEnumerator Viajar()
        {
            if (!Application.CanStreamedLevelBeLoaded(nomeCena))
            {
                Debug.LogError("[ElevatorRide] A cena " + nomeCena + " nao esta no Build Settings. " +
                               "Gere a fase pelo menu Echoes, que ela e adicionada sozinha.", this);
                yield break;
            }

            _viajando = true;
            Cinematic.Iniciar();

            Transform jogador = _controlador.transform;
            var corpo = jogador.GetComponent<Rigidbody2D>();

            _controlador.ControleTravado = true;
            if (corpo != null)
            {
                corpo.linearVelocity = Vector2.zero;
                corpo.simulated = false;
            }

            // O Scrap vai no meio da cabine.
            Vector3 deslocamento = new Vector3(transform.position.x + 1.5f - jogador.position.x, 0f, 0f);
            jogador.position += deslocamento;
            Vector3 deslocamentoPassageiro = jogador.position - transform.position;

            // O tranco da partida.
            ScrapCameraFollow.Tremer(0.08f, 0.35f);
            Sfx.InicioElevador.Tocar();
            yield return new WaitForSeconds(0.6f);

            // O motor, em loop, enquanto sobe: engrossa com a velocidade e some com a tela.
            var motor = gameObject.AddComponent<AudioSource>();
            motor.clip = SfxPlayer.Carregar(Sfx.RepeticaoElevador.Clipe);
            motor.loop = true;
            motor.spatialBlend = 0f;
            motor.volume = 0f;
            if (motor.clip != null) motor.Play();

            float inicioY = transform.position.y;
            float distancia = Mathf.Max(1f, topoY - inicioY);
            float atual = 0f;

            while (transform.position.y < topoY)
            {
                // Arranca devagar e pega velocidade.
                atual = Mathf.MoveTowards(atual, velocidade, velocidade * 0.8f * Time.deltaTime);

                Vector3 posicao = transform.position;
                posicao.y = Mathf.Min(topoY, posicao.y + atual * Time.deltaTime);
                transform.position = posicao;
                jogador.position = posicao + deslocamentoPassageiro;

                // Os ultimos tiles escurecem a tela.
                float esquerda = topoY - posicao.y;
                Cinematic.Esmaecimento = Mathf.Clamp01(1f - esquerda / Mathf.Min(6f, distancia));

                float ritmo = atual / Mathf.Max(0.1f, velocidade);
                motor.pitch = 0.8f + 0.25f * ritmo;
                motor.volume = Sfx.Volume * Sfx.RepeticaoElevador.Volume * Mathf.Lerp(0.5f, 1f, ritmo) * (1f - Cinematic.Esmaecimento);

                yield return null;
            }

            Cinematic.Esmaecimento = 1f;
            yield return new WaitForSeconds(0.3f);

            // O estado de cena e estatico: tem que acabar aqui, senao a
            // proxima fase comeca com as faixas pretas e o Scrap imune.
            Cinematic.Encerrar();

            // A tela ja esta preta: o loading entra direto e a fase nova abre com a transicao.
            GameManager.Instancia.TrocarCena(nomeCena, idNascimento, telaJaPreta: true);
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
            if (controlador != null) _controlador = controlador;
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (_viajando || _controlador == null) return;
            if (outro.GetComponentInParent<ScrapController>() != _controlador) return;

            _controlador = null;
            _segurado = 0f;
            HoldAction.Esconder(this);
        }

        private void OnDisable()
        {
            HoldAction.Esconder(this);
        }
    }
}
