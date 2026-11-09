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
            Lift,

            /// <summary>Jato deitado: carrega numa direcao e segura a altura.</summary>
            Jet,
        }

        [SerializeField] private Mode mode = Mode.Lift;

        [Tooltip("Velocidade que o vapor impoe, em tiles por segundo.")]
        [SerializeField] private float speed = 9f;

        [Tooltip("Quao rapido ele chega nessa velocidade. Precisa ser maior que a gravidade do Scrap (uns 53 na queda), senao nao segura.")]
        [SerializeField] private float acceleration = 70f;

        [Tooltip("So no jato: +1 empurra pra direita, -1 pra esquerda.")]
        [SerializeField] private float direction = 1f;

        [Tooltip("So no jato: velocidade vertical que ele mantem. Um tiquinho pra cima evita que o Scrap escorregue pra fora por baixo.")]
        [SerializeField] private float hoverSpeed = 0.6f;

        [Header("Pulso")]
        [Tooltip("Segundos ligado. So vale se Off Time for maior que zero.")]
        [SerializeField] private float onTime = 2.5f;

        [Tooltip("Segundos desligado. Zero deixa o vapor sempre ligado.")]
        [SerializeField] private float offTime;

        [Tooltip("As particulas do vapor. Param de sair enquanto ele esta desligado.")]
        [SerializeField] private ParticleSystem[] steam;

        // Guardar quem esta dentro (em vez de usar OnTriggerStay2D) garante o
        // empurrao mesmo com o corpo dormindo: um Scrap parado em cima do bocal
        // nao gera mais eventos de Stay.
        private readonly List<Rigidbody2D> _inside = new List<Rigidbody2D>();
        [Tooltip("Se comeca desligado, esperando uma valvula.")]
        [SerializeField] private bool startsOff;

        private bool _on = true;
        private bool _switchedOn = true;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void SetStartsOff(bool off)
        {
            startsOff = off;
        }

        private void Awake()
        {
            _switchedOn = !startsOff;
        }

        /// <summary>Se o vapor esta saindo agora.</summary>
        public bool IsOn => _on && _switchedOn;

        /// <summary>
        /// Liga ou desliga o vapor de vez. E o que as valvulas fazem: um vapor
        /// desligado nao empurra e nao aparece, ate alguem girar a valvula dele.
        /// </summary>
        public void SetSwitchedOn(bool on)
        {
            _switchedOn = on;
            ApplyEmission();
        }

        private void ApplyEmission()
        {
            if (steam == null) return;

            foreach (ParticleSystem particles in steam)
            {
                if (particles == null) continue;

                ParticleSystem.EmissionModule emission = particles.emission;
                emission.enabled = IsOn;
            }
        }

        private void Start()
        {
            ApplyEmission();
        }

        /// <summary>Usado pelos geradores de fase pra configurar tudo de uma vez.</summary>
        public void Configure(Mode newMode, float newSpeed, float newDirection, float newOnTime,
            float newOffTime, ParticleSystem[] newSteam)
        {
            mode = newMode;
            speed = newSpeed;
            direction = newDirection;
            onTime = newOnTime;
            offTime = newOffTime;
            steam = newSteam;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            bool on = true;

            if (offTime > 0f)
            {
                float cycle = Mathf.Max(0.1f, onTime) + offTime;
                on = Mathf.Repeat(Time.time, cycle) < Mathf.Max(0.1f, onTime);
            }

            if (on == _on) return;
            _on = on;
            ApplyEmission();
        }

        private void FixedUpdate()
        {
            if (!IsOn) return;

            float step = acceleration * Time.fixedDeltaTime;

            for (int i = _inside.Count - 1; i >= 0; i--)
            {
                Rigidbody2D body = _inside[i];
                if (body == null)
                {
                    _inside.RemoveAt(i);
                    continue;
                }

                body.WakeUp();

                // Dentro do vapor as costas do Scrap esfriam na hora: e o que
                // deixa encadear um dash atras do outro.
                var dash = body.GetComponent<ScrapDash>();
                if (dash != null) dash.Refresh();

                Vector2 velocity = body.linearVelocity;

                if (mode == Mode.Lift)
                {
                    // So acelera pra cima: quem ja entra subindo mais rapido
                    // (no auge de um pulo) nao e freado.
                    if (velocity.y < speed) velocity.y = Mathf.MoveTowards(velocity.y, speed, step);
                }
                else
                {
                    velocity.x = Mathf.MoveTowards(velocity.x, Mathf.Sign(direction) * speed, step);
                    velocity.y = Mathf.MoveTowards(velocity.y, hoverSpeed, step);
                }

                body.linearVelocity = velocity;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || body.bodyType != RigidbodyType2D.Dynamic) return;
            if (!_inside.Contains(body)) _inside.Add(body);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (body != null) _inside.Remove(body);
        }

        private void OnDisable()
        {
            _inside.Clear();
        }
    }
}
