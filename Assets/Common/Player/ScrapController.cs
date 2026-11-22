using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Movimento do Scrap: andar, pular e o Wall Grip (escorregar devagar
    /// numa parede e pular dela).
    ///
    /// O "heavy feel" do GDD sai daqui, dos numeros, e nao da arte: aceleracao
    /// baixa, gravidade alta e um freio lento que faz ele derrapar um tiquinho
    /// ao parar. Tudo esta no Inspector pra tunar com o jogo rodando.
    ///
    /// Le o teclado e o gamepad direto pelo Input System, sem depender de um
    /// asset de InputActions. Se um dia quiser rebind em runtime, migre pro
    /// Assets/Settings/InputSystem_Actions.inputactions que ja existe no projeto.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapController : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("maxSpeed")] [Header("Andar")]
        [Tooltip("Velocidade maxima, em tiles por segundo.")]
        [SerializeField] private float velocidadeMax = 5.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("accelTime")] [Tooltip("Segundos ate atingir a velocidade maxima. Maior = mais pesado pra engrenar.")]
        [SerializeField] private float tempoAceleracao = 0.22f;

        [UnityEngine.Serialization.FormerlySerializedAs("brakeTime")] [Tooltip("Segundos ate parar por completo. Maior = derrapa mais ao soltar a direcao.")]
        [SerializeField] private float tempoFreio = 0.30f;

        [UnityEngine.Serialization.FormerlySerializedAs("jumpHeight")] [Header("Pular")]
        [Tooltip("Altura do pulo em tiles. A velocidade inicial e calculada a partir daqui, entao esse valor e literal.")]
        [SerializeField] private float alturaPulo = 3.0f;

        [UnityEngine.Serialization.FormerlySerializedAs("gravityScale")] [Tooltip("Gravidade base. Alta de proposito: o Scrap e de ferro.")]
        [SerializeField] private float escalaGravidade = 3.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("fallGravityMultiplier")] [Tooltip("Multiplica a gravidade durante a queda. Cair mais rapido do que sobe tira a sensacao de flutuar.")]
        [SerializeField] private float multiplicadorGravidadeQueda = 1.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("jumpCutMultiplier")] [Tooltip("Soltar o botao no meio da subida corta o pulo por este fator. 1 = sem corte.")]
        [SerializeField, Range(0f, 1f)] private float multiplicadorCortePulo = 0.45f;

        [UnityEngine.Serialization.FormerlySerializedAs("coyoteTime")] [Tooltip("Janela em que ainda da pra pular depois de ja ter saido da plataforma.")]
        [SerializeField] private float tempoCoiote = 0.10f;

        [UnityEngine.Serialization.FormerlySerializedAs("jumpBufferTime")] [Tooltip("Janela em que um pulo apertado cedo demais ainda vale quando encostar no chao.")]
        [SerializeField] private float tempoReservaPulo = 0.12f;

        [UnityEngine.Serialization.FormerlySerializedAs("groundCheck")] [Header("Chao")]
        [Tooltip("Transform vazio nos pes do Scrap. O gerador cria como filho 'GroundCheck'.")]
        [SerializeField] private Transform checagemSolo;

        [UnityEngine.Serialization.FormerlySerializedAs("groundCheckRadius")] [SerializeField] private float raioChecagemSolo = 0.18f;

        [UnityEngine.Serialization.FormerlySerializedAs("groundLayers")] [Tooltip("Quais layers contam como chao. O gerador aponta pra layer 'Ground'.")]
        [SerializeField] private LayerMask camadasSolo = ~0;

        [UnityEngine.Serialization.FormerlySerializedAs("wallSlideSpeed")] [Header("Wall Grip")]
        [Tooltip("Velocidade maxima de queda enquanto ele segura numa parede. Menor = gruda mais.")]
        [SerializeField] private float velocidadeDeslizeParede = 1.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("wallJumpPush")] [Tooltip("Empurrao pra longe da parede ao pular dela, em tiles por segundo.")]
        [SerializeField] private float empurraPuloParede = 7f;

        [UnityEngine.Serialization.FormerlySerializedAs("wallJumpHeight")] [Tooltip("Altura do pulo de parede, como fracao do pulo normal.")]
        [SerializeField, Range(0.5f, 1.2f)] private float alturaPuloParede = 1.0f;

        [UnityEngine.Serialization.FormerlySerializedAs("wallJumpLockTime")] [Tooltip("Por quanto tempo depois do pulo de parede ele ignora a direcao. E o que impede de voltar na hora pra mesma parede sem ganhar altura.")]
        [SerializeField] private float tempoTravaPuloParede = 0.16f;

        [UnityEngine.Serialization.FormerlySerializedAs("wallCoyoteTime")] [Tooltip("Janela em que ainda da pra pular depois de ja ter soltado a parede.")]
        [SerializeField] private float tempoCoioteParede = 0.10f;

        [UnityEngine.Serialization.FormerlySerializedAs("wallStickDuration")] [Header("Wall Grip - Novo Comportamento")]
        [Tooltip("Tempo em segundos que ele fica grudado parado na parede antes de começar a escorregar.")]
        [SerializeField] private float duracaoGrudarParede = 1.0f;

        [UnityEngine.Serialization.FormerlySerializedAs("maxStepHeight")] [Header("Degraus")]
        [Tooltip("Degrau ate essa altura (em tiles) ele sobe andando, sem pular. Os de tile inteiro continuam pedindo pulo: so as escadarias tem degrau de meio tile.")]
        [SerializeField] private float alturaPassoMax = 0.55f;

        private float _cronometroGrudarParede;
        private int _ultimoLadoParedePulou;

        private Rigidbody2D _corpo;
        private SpriteRenderer _sprite;

        private float _entradaMover;
        private float _contadorCoiote;
        private float _contadorReservaPulo;
        private bool _seguradoPulo;
        private bool _quadroEsteSoltoPulo;
        private bool _estaNoChao;

        // -1 parede a esquerda, +1 a direita, 0 nenhuma.
        private int _ladoParede;
        private int _ultimoLadoParede;
        private float _contadorCoioteParede;
        private bool _deslizandoParede;
        private float _travaDirecao;
        private int _direcaoOlhar = 1;
        private bool _usadoPuloAr;

        // Mercurio: enquanto ele pisa numa poca, escorrega; se ela tem
        // correnteza, e arrastado. Gota grudada no corpo deixa ele lento.
        private float _mercurioAte;
        private float _atual;

        [SaveProgress("_pressureJump")]
        private static bool _puloPressao;

        /// <summary>Ate quando o Scrap esta lento por causa de mercurio grudado. O HUD usa pro aviso.</summary>
        public static float LentidaoAte { get; private set; } = -100f;

        /// <summary>
        /// Se o Scrap ja tem o Pressure Jump: um segundo pulo no ar, com uma
        /// rajada de vapor dos pistoes. Quem da e o Boiler Master, ao cair.
        /// </summary>
        public static bool TemPuloPressao => _puloPressao;

        /// <summary>Quando ele foi conquistado. O HUD usa pra mostrar o aviso.</summary>
        public static float TempoPuloPressao { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _puloPressao = false;
            TempoPuloPressao = -100f;
            LentidaoAte = -100f;
        }

        /// <summary>
        /// Chamado a cada passo de fisica pela poca de mercurio em que ele
        /// pisa. <paramref name="atual"/> e a correnteza, em tiles por
        /// segundo (0 numa poca parada).
        /// </summary>
        public void NoMercurio(float atual)
        {
            _mercurioAte = Time.time + 0.1f;
            _atual = atual;
        }

        /// <summary>Verdadeiro enquanto ele pisa em mercurio.</summary>
        public bool EstaLigadoMercurio => Time.time < _mercurioAte;

        /// <summary>Mercurio grudou no corpo: ele anda mais devagar por um tempo.</summary>
        public void Desacelerar(float segundos)
        {
            LentidaoAte = Mathf.Max(LentidaoAte, Time.time + segundos);
        }

        /// <summary>Da o Pressure Jump sem aviso na tela: pra cenas em que ele ja chega com ele.</summary>
        public static void DesbloquearSaltoDePressaoEmSilencio()
        {
            _puloPressao = true;
        }

        public static void DesbloquearSaltoDePressao()
        {
            if (_puloPressao) return;

            _puloPressao = true;
            TempoPuloPressao = Time.time;
            AbilityCard.Mostrar(ScrapMove.PuloPressao);
        }

        /// <summary>Verdadeiro enquanto o GroundCheck encosta em algo da layer de chao.</summary>
        public bool EstaNoChao => _estaNoChao;

        /// <summary>Verdadeiro enquanto ele escorrega segurando numa parede.</summary>
        public bool EstaDeslizandoParede => _deslizandoParede;

        /// <summary>Pra que lado ele esta virado: -1 esquerda, +1 direita.</summary>
        public int Virado => _direcaoOlhar;

        /// <summary>
        /// Trava o controle: ele para de andar e nao pula. Usado enquanto raspa
        /// uma alavanca — e o que deixa ele vulneravel.
        /// </summary>
        public bool ControleTravado { get; set; }

        /// <summary>
        /// Joga o Scrap com uma velocidade e deixa ele sem direcao por um
        /// instante, pra o empurrao nao ser desfeito no quadro seguinte.
        /// </summary>
        public void Empurrar(Vector2 velocidadeVetorial, float tempoTrava)
        {
            _corpo.linearVelocity = velocidadeVetorial;
            _travaDirecao = Mathf.Max(_travaDirecao, tempoTrava);
        }

        /// <summary>Velocidade horizontal atual, normalizada pela maxima. Util pro Animator depois.</summary>
        public float VelocidadeNormalizada => velocidadeMax <= 0f ? 0f : Mathf.Abs(_corpo.linearVelocity.x) / velocidadeMax;

        private void Awake()
        {
            _corpo = GetComponent<Rigidbody2D>();
            _sprite = GetComponentInChildren<SpriteRenderer>();

            _corpo.gravityScale = escalaGravidade;
            _corpo.freezeRotation = true;
            _corpo.interpolation = RigidbodyInterpolation2D.Interpolate;
            _corpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            LerEntrada();

            _estaNoChao = checagemSolo != null
                && Physics2D.OverlapCircle(checagemSolo.position, raioChecagemSolo, camadasSolo);

            // Se tocou no chão, limpa a memória da parede anterior
            if (_estaNoChao)
            {
                _ultimoLadoParedePulou = 0;
            }

            // Coyote time: recarrega no chao, escorre no ar.
            _contadorCoiote = _estaNoChao ? tempoCoiote : _contadorCoiote - Time.deltaTime;
            _contadorReservaPulo -= Time.deltaTime;
            _travaDirecao -= Time.deltaTime;

            AtualizarParede();

            // O pulo do ar volta ao pisar no chao ou encostar numa parede valida.
            if (_estaNoChao || _ladoParede != 0) _usadoPuloAr = false;

            if (!Mathf.Approximately(_entradaMover, 0f)) _direcaoOlhar = _entradaMover < 0f ? -1 : 1;

            // Na parede ele olha pra fora dela.
            if (_deslizandoParede) _direcaoOlhar = -_ladoParede;

            if (_sprite != null) _sprite.flipX = _direcaoOlhar < 0;
        }

        /// <summary>
        /// Ve se ha parede encostada de algum lado, na altura do corpo. So vale
        /// no ar: no chao, parede e so parede.
        /// </summary>
        private void AtualizarParede()
        {
            int paredeAnterior = _ladoParede;
            _ladoParede = 0;

            if (!_estaNoChao)
            {
                Vector2 centro = (Vector2)transform.position + new Vector2(0f, 0.65f);
                var tamanho = new Vector2(0.14f, 0.7f);

                if (Physics2D.OverlapBox(centro + new Vector2(0.46f, 0f), tamanho, 0f, camadasSolo)) _ladoParede = 1;
                else if (Physics2D.OverlapBox(centro + new Vector2(-0.46f, 0f), tamanho, 0f, camadasSolo)) _ladoParede = -1;
            }

            // Se encontrou uma parede diferente da que acabou de pular, libera a trava
            if (_ladoParede != 0 && _ladoParede != _ultimoLadoParedePulou)
            {
                _ultimoLadoParedePulou = 0;
            }

            // Se for a MESMA parede de onde acabou de sair, ignora para não escalar a mesma parede
            if (_ladoParede != 0 && _ladoParede == _ultimoLadoParedePulou)
            {
                _ladoParede = 0;
            }

            if (_ladoParede != 0)
            {
                _ultimoLadoParede = _ladoParede;
                _contadorCoioteParede = tempoCoioteParede;

                // Se acabou de encostar na parede, ativa a parada de 1s
                if (paredeAnterior == 0)
                {
                    _cronometroGrudarParede = duracaoGrudarParede;
                }
                else if (_cronometroGrudarParede > 0f)
                {
                    _cronometroGrudarParede -= Time.deltaTime;
                }
            }
            else
            {
                _contadorCoioteParede -= Time.deltaTime;
                _cronometroGrudarParede = 0f;
            }

            // Segura na parede se estiver encostado e caindo
            _deslizandoParede = _ladoParede != 0
                && _corpo.linearVelocity.y <= 0f
                && Mathf.Abs(_entradaMover) > 0.1f
                && Mathf.Sign(_entradaMover) == _ladoParede;
        }

        private void FixedUpdate()
        {
            AplicarHorizontal();
            SubirDegrau();
            AplicarPulo();
            AplicarGravidade();

            // Consumido aqui pra o corte de pulo acontecer uma vez so, e nao a cada passo de fisica.
            _quadroEsteSoltoPulo = false;
        }

        private void LerEntrada()
        {
            float eixo = 0f;
            bool apertadoPulo = false;
            bool agoraSeguradoPulo = false;

            Keyboard teclado = Keyboard.current;
            if (teclado != null)
            {
                if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) eixo -= 1f;
                if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) eixo += 1f;

                apertadoPulo |= teclado.spaceKey.wasPressedThisFrame
                    || teclado.wKey.wasPressedThisFrame
                    || teclado.upArrowKey.wasPressedThisFrame;

                agoraSeguradoPulo |= teclado.spaceKey.isPressed
                    || teclado.wKey.isPressed
                    || teclado.upArrowKey.isPressed;
            }

            Gamepad controle = Gamepad.current;
            if (controle != null)
            {
                float grudar = controle.leftStick.x.ReadValue();
                if (Mathf.Abs(grudar) > 0.2f) eixo += grudar;
                if (controle.dpad.left.isPressed) eixo -= 1f;
                if (controle.dpad.right.isPressed) eixo += 1f;

                apertadoPulo |= controle.buttonSouth.wasPressedThisFrame;
                agoraSeguradoPulo |= controle.buttonSouth.isPressed;
            }

            if (ControleTravado)
            {
                eixo = 0f;
                apertadoPulo = false;
                agoraSeguradoPulo = false;
            }

            _entradaMover = Mathf.Clamp(eixo, -1f, 1f);

            if (_seguradoPulo && !agoraSeguradoPulo) _quadroEsteSoltoPulo = true;
            _seguradoPulo = agoraSeguradoPulo;

            if (apertadoPulo) _contadorReservaPulo = tempoReservaPulo;
        }

        private void AplicarHorizontal()
        {
            // Logo depois de um pulo de parede ou de apanhar, o embalo manda.
            if (_travaDirecao > 0f) return;

            float topo = velocidadeMax * (Time.time < LentidaoAte ? 0.55f : 1f);
            float alvo = _entradaMover * topo;

            // Acelerar e frear tem taxas separadas. E essa diferenca que da o peso:
            // o Scrap custa a engrenar e custa a parar.
            float taxa = Mathf.Approximately(alvo, 0f)
                ? velocidadeMax / Mathf.Max(tempoFreio, 0.01f)
                : velocidadeMax / Mathf.Max(tempoAceleracao, 0.01f);

            // Ferro flutua em mercurio, mas nao tem onde firmar o pe: ele anda
            // um pouco mais devagar, custa muito a engrenar e mais ainda a
            // parar. A correnteza entra por cima de tudo.
            if (EstaLigadoMercurio && _estaNoChao)
            {
                taxa = Mathf.Approximately(alvo, 0f) ? velocidadeMax / 1.5f : velocidadeMax / 0.8f;
                alvo = alvo * 0.85f + _atual;
            }

            Vector2 velocidadeVetorial = _corpo.linearVelocity;
            velocidadeVetorial.x = Mathf.MoveTowards(velocidadeVetorial.x, alvo, taxa * Time.fixedDeltaTime);
            _corpo.linearVelocity = velocidadeVetorial;
        }

        /// <summary>
        /// Sobe um degrau baixo andando, sem precisar pular: se ha um degrau
        /// de ate maxStepHeight logo a frente e espaco livre em cima dele, o
        /// corpo e posto em cima. Degrau de tile inteiro nao entra aqui.
        /// </summary>
        private void SubirDegrau()
        {
            if (!_estaNoChao || ControleTravado || _travaDirecao > 0f || Mathf.Abs(_entradaMover) < 0.1f) return;
            if (alturaPassoMax <= 0.05f) return;

            float direcao = Mathf.Sign(_entradaMover);
            Vector2 pes = _corpo.position;

            // Logo a frente do corpo (o casco tem 0,35 de raio).
            float sondaX = pes.x + direcao * 0.42f;

            // A altura do degrau: um raio de cima pra baixo, na frente. Se ele
            // ja comeca dentro de algo, e parede alta: nada a fazer.
            Vector2 de = new Vector2(sondaX, pes.y + alturaPassoMax + 0.05f);
            RaycastHit2D golpe = Physics2D.Raycast(de, Vector2.down, alturaPassoMax + 0.05f, camadasSolo);
            if (!golpe || golpe.collider.isTrigger || golpe.distance <= 0.001f) return;

            float subida = golpe.point.y - pes.y;
            if (subida < 0.05f || subida > alturaPassoMax) return;

            // Cabe em cima do degrau?
            Vector2 corpo = new Vector2(pes.x + direcao * 0.12f, pes.y + subida + 0.6f);
            if (Physics2D.OverlapBox(corpo, new Vector2(0.6f, 0.95f), 0f, camadasSolo)) return;

            _corpo.position = new Vector2(pes.x + direcao * 0.04f, pes.y + subida + 0.02f);

            Vector2 velocidadeVetorial = _corpo.linearVelocity;
            if (velocidadeVetorial.y < 0f) velocidadeVetorial.y = 0f;
            _corpo.linearVelocity = velocidadeVetorial;
        }

        private void AplicarPulo()
        {
            if (_contadorReservaPulo > 0f && _contadorCoiote > 0f)
            {
                // v = raiz(2 * g * h), entao jumpHeight vale literalmente em tiles,
                // mesmo se voce mexer na gravidade depois.
                float gravidade = Mathf.Abs(Physics2D.gravity.y) * escalaGravidade;
                float velocidadeLancamento = Mathf.Sqrt(2f * gravidade * Mathf.Max(alturaPulo, 0.01f));

                Vector2 velocidadeVetorial = _corpo.linearVelocity;
                velocidadeVetorial.y = velocidadeLancamento;
                _corpo.linearVelocity = velocidadeVetorial;

                _contadorReservaPulo = 0f;
                _contadorCoiote = 0f;
                Sfx.Pulo.Tocar();
                return;
            }

            // Pulo de parede: depende do direcional apontado
            if (_contadorReservaPulo > 0f && !_estaNoChao && _contadorCoioteParede > 0f && _ultimoLadoParede != 0)
            {
                float gravidade = Mathf.Abs(Physics2D.gravity.y) * escalaGravidade;

                // Registra qual parede foi usada para pular (impede que grude nela de novo até ir pra outra ou tocar no chão)
                _ultimoLadoParedePulou = _ultimoLadoParede;

                bool longeEmpurrando = Mathf.Abs(_entradaMover) > 0.2f && Mathf.Sign(_entradaMover) != _ultimoLadoParede;

                if (longeEmpurrando)
                {
                    // Pulo completo: joga na diagonal pra outra parede
                    float velocidadeLancamento = Mathf.Sqrt(2f * gravidade * Mathf.Max(alturaPulo * alturaPuloParede, 0.01f));
                    _corpo.linearVelocity = new Vector2(-_ultimoLadoParede * empurraPuloParede, velocidadeLancamento);

                    _travaDirecao = tempoTravaPuloParede;
                    _direcaoOlhar = -_ultimoLadoParede;
                }
                else
                {
                    // Pulo neutro pra cima: descola e cai reto sem impulso horizontal
                    float velocidadeLancamento = Mathf.Sqrt(2f * gravidade * Mathf.Max(alturaPulo * 0.7f, 0.01f));
                    _corpo.linearVelocity = new Vector2(0f, velocidadeLancamento);
                }

                _contadorReservaPulo = 0f;
                _contadorCoioteParede = 0f;
                _cronometroGrudarParede = 0f;
                Sfx.PuloParede.Tocar();
                return;
            }

            // Pressure Jump: um pulo a mais, no ar, um pouco mais baixo.
            if (_contadorReservaPulo > 0f && !_estaNoChao && _puloPressao && !_usadoPuloAr)
            {
                float gravidade = Mathf.Abs(Physics2D.gravity.y) * escalaGravidade;
                float velocidadeLancamento = Mathf.Sqrt(2f * gravidade * Mathf.Max(alturaPulo * 0.85f, 0.01f));

                Vector2 velocidadeVetorial = _corpo.linearVelocity;
                velocidadeVetorial.y = velocidadeLancamento;
                _corpo.linearVelocity = velocidadeVetorial;

                _usadoPuloAr = true;
                _contadorReservaPulo = 0f;
                Sfx.PuloPressao.Tocar();
                return;
            }

            if (_quadroEsteSoltoPulo && _corpo.linearVelocity.y > 0f)
            {
                Vector2 velocidadeVetorial = _corpo.linearVelocity;
                velocidadeVetorial.y *= multiplicadorCortePulo;
                _corpo.linearVelocity = velocidadeVetorial;
            }
        }

        private void AplicarGravidade()
        {
            _corpo.gravityScale = _corpo.linearVelocity.y < 0f
                ? escalaGravidade * multiplicadorGravidadeQueda
                : escalaGravidade;

            if (_deslizandoParede)
            {
                Vector2 velocidadeVetorial = _corpo.linearVelocity;

                // Se ainda estiver no tempo de "grudar", congela o Scrap verticalmente na parede
                if (_cronometroGrudarParede > 0f)
                {
                    velocidadeVetorial.y = 0f;
                }
                else if (velocidadeVetorial.y < -velocidadeDeslizeParede)
                {
                    // Passado o tempo, escorrega na velocidade configurada
                    velocidadeVetorial.y = -velocidadeDeslizeParede;
                }

                _corpo.linearVelocity = velocidadeVetorial;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (checagemSolo == null) return;

            Gizmos.color = Application.isPlaying && _estaNoChao ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(checagemSolo.position, raioChecagemSolo);
        }
    }
}