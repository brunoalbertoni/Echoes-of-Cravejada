using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Vapor que move o Scrap: uma coluna em pe que o levanta, ou um jato
    /// deitado que o carrega de lado.
    ///
    /// Nao mexe no ScrapController. So empurra a velocidade do Rigidbody2D na
    /// direcao do vapor, mais forte do que a gravidade e do que o freio do
    /// controller conseguem desfazer. Na coluna o jogador continua mandando no
    /// eixo horizontal; no jato, nao — ele e carregado ate o fim.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SteamVent : MonoBehaviour
    {
        public enum Mode
        {
            /// <summary>Coluna em pe: levanta.</summary>
            Erguer2,

            /// <summary>Jato deitado: carrega numa direcao e segura a altura.</summary>
            Jato,
        }

        [UnityEngine.Serialization.FormerlySerializedAs("mode")] [SerializeField] private Mode modo = Mode.Erguer2;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [Tooltip("Velocidade que o vapor impoe, em tiles por segundo.")]
        [SerializeField] private float velocidade = 9f;

        [UnityEngine.Serialization.FormerlySerializedAs("acceleration")] [Tooltip("Quao rapido ele chega nessa velocidade. Precisa ser maior que a gravidade do Scrap (uns 53 na queda), senao nao segura.")]
        [SerializeField] private float aceleracao = 70f;

        [UnityEngine.Serialization.FormerlySerializedAs("direction")] [Tooltip("So no jato: +1 empurra pra direita, -1 pra esquerda.")]
        [SerializeField] private float direcao = 1f;

        [UnityEngine.Serialization.FormerlySerializedAs("hoverSpeed")] [Tooltip("So no jato: velocidade vertical que ele mantem. Um tiquinho pra cima evita que o Scrap escorregue pra fora por baixo.")]
        [SerializeField] private float velocidadeFlutuar = 0.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("onTime")] [Header("Pulso")]
        [Tooltip("Segundos ligado. So vale se Off Time for maior que zero.")]
        [SerializeField] private float ligadoTempo = 2.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("offTime")] [Tooltip("Segundos desligado. Zero deixa o vapor sempre ligado.")]
        [SerializeField] private float desligadoTempo;

        [UnityEngine.Serialization.FormerlySerializedAs("steam")] [Tooltip("As particulas do vapor. Param de sair enquanto ele esta desligado.")]
        [SerializeField] private ParticleSystem[] vapor;

        // Guardar quem esta dentro (em vez de usar OnTriggerStay2D) garante o
        // empurrao mesmo com o corpo dormindo: um Scrap parado em cima do bocal
        // nao gera mais eventos de Stay.
        private readonly List<Rigidbody2D> _dentro = new List<Rigidbody2D>();
        [UnityEngine.Serialization.FormerlySerializedAs("startsOff")] [Tooltip("Se comeca desligado, esperando uma valvula.")]
        [SerializeField] private bool comecaDesligado;

        private bool _on = true;
        private bool _chaveadoLigado = true;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void DefinirComecaDesligado(bool desligado)
        {
            comecaDesligado = desligado;
        }

        private void Awake()
        {
            _chaveadoLigado = !comecaDesligado;
        }

        /// <summary>Se o vapor esta saindo agora.</summary>
        public bool EstaLigado => _on && _chaveadoLigado;

        /// <summary>
        /// Liga ou desliga o vapor de vez. E o que as valvulas fazem: um vapor
        /// desligado nao empurra e nao aparece, ate alguem girar a valvula dele.
        /// </summary>
        public void DefinirLigado(bool on)
        {
            _chaveadoLigado = on;
            AplicarEmissao();
        }

        private void AplicarEmissao()
        {
            if (vapor == null) return;

            foreach (ParticleSystem particulas2 in vapor)
            {
                if (particulas2 == null) continue;

                ParticleSystem.EmissionModule emissao = particulas2.emission;
                emissao.enabled = EstaLigado;
            }
        }

        private void Start()
        {
            AplicarEmissao();
        }

        /// <summary>Usado pelos geradores de fase pra configurar tudo de uma vez.</summary>
        public void Configurar(Mode novoModo, float novoVelocidade, float novoDirecao, float novoLigadoTempo,
            float novoDesligadoTempo, ParticleSystem[] novoVapor)
        {
            modo = novoModo;
            velocidade = novoVelocidade;
            direcao = novoDirecao;
            ligadoTempo = novoLigadoTempo;
            desligadoTempo = novoDesligadoTempo;
            vapor = novoVapor;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            bool on = true;

            if (desligadoTempo > 0f)
            {
                float ciclo = Mathf.Max(0.1f, ligadoTempo) + desligadoTempo;
                on = Mathf.Repeat(Time.time, ciclo) < Mathf.Max(0.1f, ligadoTempo);
            }

            if (on == _on) return;
            _on = on;
            AplicarEmissao();
        }

        private void FixedUpdate()
        {
            if (!EstaLigado) return;

            float passo = aceleracao * Time.fixedDeltaTime;

            for (int i = _dentro.Count - 1; i >= 0; i--)
            {
                Rigidbody2D corpo = _dentro[i];
                if (corpo == null)
                {
                    _dentro.RemoveAt(i);
                    continue;
                }

                corpo.WakeUp();

                // Dentro do vapor as costas do Scrap esfriam na hora: e o que
                // deixa encadear um dash atras do outro.
                var dash = corpo.GetComponent<ScrapDash>();
                if (dash != null) dash.Atualizar();

                Vector2 velocidadeVetorial = corpo.linearVelocity;

                if (modo == Mode.Erguer2)
                {
                    // So acelera pra cima: quem ja entra subindo mais rapido
                    // (no auge de um pulo) nao e freado.
                    if (velocidadeVetorial.y < velocidade) velocidadeVetorial.y = Mathf.MoveTowards(velocidadeVetorial.y, velocidade, passo);
                }
                else
                {
                    velocidadeVetorial.x = Mathf.MoveTowards(velocidadeVetorial.x, Mathf.Sign(direcao) * velocidade, passo);
                    velocidadeVetorial.y = Mathf.MoveTowards(velocidadeVetorial.y, velocidadeFlutuar, passo);
                }

                corpo.linearVelocity = velocidadeVetorial;
            }
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Rigidbody2D corpo = outro.attachedRigidbody;
            if (corpo == null || corpo.bodyType != RigidbodyType2D.Dynamic) return;
            if (!_dentro.Contains(corpo)) _dentro.Add(corpo);
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            Rigidbody2D corpo = outro.attachedRigidbody;
            if (corpo != null) _dentro.Remove(corpo);
        }

        private void OnDisable()
        {
            _dentro.Clear();
        }
    }
}
