using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Scrap Chisel: o golpe do Scrap. Lento e pesado de proposito — a
    /// ferramenta e grande demais pra ele, entao ha um instante de preparo
    /// antes de acertar e uma pausa depois.
    ///
    /// Bate pro lado em que ele olha, pra cima (segurando cima) ou pra baixo
    /// (segurando baixo, so no ar). Acertar qualquer coisa por baixo faz ele
    /// quicar: e o pogo, que serve pra cruzar espinhos e ficar em cima de inimigo.
    ///
    /// Teclas: J, X ou botao esquerdo do mouse; no controle, o botao da esquerda.
    ///
    /// Depois do Geode Drill ele ganha o Resonance Strike: segurar o botao do
    /// golpe ate o cinzel vibrar e soltar. Sai uma onda sonora em volta dele,
    /// que fere mais, acende uma area grande e quebra barreiras de cristal.
    /// </summary>
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapChisel : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("damage")] [SerializeField] private int dano = 1;

        [UnityEngine.Serialization.FormerlySerializedAs("windup")] [Tooltip("Preparo antes de o golpe acertar. E daqui que vem o peso.")]
        [SerializeField] private float preparo = 0.12f;

        [UnityEngine.Serialization.FormerlySerializedAs("swingTime")] [Tooltip("Por quanto tempo o risco do golpe fica na tela.")]
        [SerializeField] private float tempoBalanco = 0.14f;

        [UnityEngine.Serialization.FormerlySerializedAs("recovery")] [Tooltip("Pausa ate poder bater de novo.")]
        [SerializeField] private float recuperacao = 0.30f;

        [UnityEngine.Serialization.FormerlySerializedAs("pogoSpeed")] [Tooltip("Velocidade pra cima ao acertar algo por baixo.")]
        [SerializeField] private float velocidadePogo = 12.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("recoil")] [Tooltip("Recuo ao acertar algo de lado.")]
        [SerializeField] private float coice = 3f;

        [UnityEngine.Serialization.FormerlySerializedAs("slash")] [Tooltip("O risco do golpe: um SpriteRenderer filho, desligado.")]
        [SerializeField] private SpriteRenderer corte;

        [UnityEngine.Serialization.FormerlySerializedAs("chargeTime")] [Header("Resonance Strike")]
        [Tooltip("Segundos segurando o botao ate o cinzel carregar.")]
        [SerializeField] private float tempoCarga = 0.75f;

        [UnityEngine.Serialization.FormerlySerializedAs("resonanceRadius")] [Tooltip("Ate onde a onda alcanca, em tiles.")]
        [SerializeField] private float raioRessonancia = 3.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("resonanceDamage")] [SerializeField] private int danoRessonancia = 2;

        private ScrapController _controlador;
        private Rigidbody2D _corpo;
        private bool _ocupado;
        private float _inicioSegurar = -1f;
        private UnityEngine.Rendering.Universal.Light2D _olhos;
        private float _intensidadeOlhos;
        private Color _corOlhos;

        [SaveProgress("_resonance")]
        private static bool _ressonancia;

        /// <summary>Se o Scrap ja tem o Resonance Strike. Quem da e o Geode Drill, ao parar.</summary>
        public static bool TemGolpeRessonancia => _ressonancia;

        /// <summary>Quando ele foi conquistado. O HUD usa pra mostrar o aviso.</summary>
        public static float TempoRessonancia { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _ressonancia = false;
            TempoRessonancia = -100f;
        }

        /// <summary>Da o Resonance Strike sem aviso na tela: pra cenas em que ele ja chega com ele.</summary>
        public static void DesbloquearGolpeRessonanteEmSilencio()
        {
            _ressonancia = true;
        }

        public static void DesbloquearGolpeRessonante()
        {
            if (_ressonancia) return;

            _ressonancia = true;
            TempoRessonancia = Time.time;
            AbilityCard.Mostrar(ScrapMove.GolpeRessonancia);
        }

        /// <summary>Verdadeiro enquanto o cinzel esta carregado, esperando soltar.</summary>
        public bool EstaCarregado { get; private set; }

        /// <summary>Verdadeiro durante o preparo do golpe.</summary>
        public bool EstaCimaPreparando { get; private set; }

        /// <summary>Verdadeiro enquanto o golpe esta acertando.</summary>
        public bool EstaGolpeando { get; private set; }

        /// <summary>Pra onde foi o ultimo golpe: o lado, cima ou baixo. O ScrapAnimator escolhe os quadros por ela.</summary>
        public Vector2 DirecaoGolpe { get; private set; } = Vector2.right;

        /// <summary>Quando comecaram o preparo e o golpe, e quando o golpe acabou. Sao o relogio da animacao.</summary>
        public float TempoInicioPreparo { get; private set; } = -100f;
        public float TempoInicioGolpe { get; private set; } = -100f;
        public float TempoFimGolpe { get; private set; } = -100f;

        public float DuracaoPreparo => preparo;
        public float DuracaoGolpe => tempoBalanco;

        private readonly Collider2D[] _golpes = new Collider2D[16];

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(SpriteRenderer novoCorte)
        {
            corte = novoCorte;
        }

        private void Awake()
        {
            _controlador = GetComponent<ScrapController>();
            _corpo = GetComponent<Rigidbody2D>();
            if (corte != null) corte.enabled = false;

            // Os olhos dele avisam que o cinzel carregou.
            Transform olhos = transform.Find("EyeGlow");
            if (olhos != null) _olhos = olhos.GetComponent<UnityEngine.Rendering.Universal.Light2D>();

            if (_olhos != null)
            {
                _intensidadeOlhos = _olhos.intensity;
                _corOlhos = _olhos.color;
            }
        }

        private void Update()
        {
            bool apertado = AtaquePressionado();

            if (_controlador.ControleTravado)
            {
                _inicioSegurar = -1f;
                DefinirCarregado(false);
                return;
            }

            if (_ressonancia)
            {
                if (apertado) _inicioSegurar = Time.time;

                bool prontoUso = _inicioSegurar >= 0f && Time.time - _inicioSegurar >= tempoCarga && !_ocupado;

                if (!AtaqueSegurado())
                {
                    _inicioSegurar = -1f;
                    DefinirCarregado(false);

                    if (prontoUso)
                    {
                        StartCoroutine(Ressoar());
                        return;
                    }
                }
                else
                {
                    DefinirCarregado(prontoUso);
                }
            }

            if (_ocupado || !apertado) return;

            StartCoroutine(Balancar(DirecaoDaMira()));
        }

        private void DefinirCarregado(bool carregado2)
        {
            if (EstaCarregado == carregado2) return;

            EstaCarregado = carregado2;
            if (carregado2) Sfx.ProntoCarga.Tocar();
            if (_olhos == null) return;

            _olhos.intensity = carregado2 ? _intensidadeOlhos * 1.8f + 0.4f : _intensidadeOlhos;
            _olhos.color = carregado2 ? new Color(0.70f, 0.88f, 1f) : _corOlhos;
        }

        private static bool AtaqueSegurado()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.jKey.isPressed || teclado.xKey.isPressed)) return true;

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed) return true;

            Gamepad controle = Gamepad.current;
            return controle != null && controle.buttonWest.isPressed;
        }

        /// <summary>
        /// O Resonance Strike: uma onda em volta dele. Fere o que estiver no
        /// raio, estoura barreira de cristal e faz o maior barulho que ele tem.
        /// </summary>
        private IEnumerator Ressoar()
        {
            _ocupado = true;
            EstaGolpeando = true;
            DirecaoGolpe = new Vector2(_controlador.Virado, 0f);
            TempoInicioGolpe = Time.time;

            Vector2 centro = (Vector2)transform.position + Vector2.up * 0.6f;

            NoiseSystem.Emitir(centro, 22f, 1f);
            Sfx.Ressonancia.Tocar();

            if (corte != null)
            {
                ResonanceRing.Criar(centro, corte.sprite, raioRessonancia, corte.sharedMaterial);
            }

            foreach (Collider2D golpe in Physics2D.OverlapCircleAll(centro, raioRessonancia))
            {
                if (golpe == null || golpe.transform.IsChildOf(transform)) continue;

                var barreira = golpe.GetComponentInParent<CrystalBarrier>();
                if (barreira != null)
                {
                    barreira.Estilhacar();
                    continue;
                }

                var danificavel = golpe.GetComponentInParent<IDamageable>();
                if (danificavel != null) danificavel.ReceberDano(danoRessonancia + ScrapGems.DanoBonus, centro);
            }

            yield return new WaitForSeconds(tempoBalanco);
            EstaGolpeando = false;
            TempoFimGolpe = Time.time;

            yield return new WaitForSeconds(recuperacao);
            _ocupado = false;
        }

        private static bool AtaquePressionado()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.jKey.wasPressedThisFrame || teclado.xKey.wasPressedThisFrame)) return true;

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;

            Gamepad controle = Gamepad.current;
            return controle != null && controle.buttonWest.wasPressedThisFrame;
        }

        /// <summary>Pra onde o golpe vai: cima, baixo (so no ar) ou o lado em que ele olha.</summary>
        private Vector2 DirecaoDaMira()
        {
            float vertical = 0f;

            Keyboard teclado = Keyboard.current;
            if (teclado != null)
            {
                if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) vertical -= 1f;
                if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) vertical += 1f;
            }

            Gamepad controle = Gamepad.current;
            if (controle != null)
            {
                float grudar = controle.leftStick.y.ReadValue();
                if (Mathf.Abs(grudar) > 0.5f) vertical += Mathf.Sign(grudar);
                if (controle.dpad.down.isPressed) vertical -= 1f;
                if (controle.dpad.up.isPressed) vertical += 1f;
            }

            if (vertical < 0f && !_controlador.EstaNoChao) return Vector2.down;
            if (vertical > 0f) return Vector2.up;
            return new Vector2(_controlador.Virado, 0f);
        }

        private IEnumerator Balancar(Vector2 direcao)
        {
            _ocupado = true;
            DirecaoGolpe = direcao;
            EstaCimaPreparando = true;
            TempoInicioPreparo = Time.time;
            yield return new WaitForSeconds(preparo);
            EstaCimaPreparando = false;
            EstaGolpeando = true;
            TempoInicioGolpe = Time.time;

            // A caixa do golpe, medida a partir dos pes.
            Vector2 centro;
            Vector2 tamanho;

            // Um pouco mais longe que o corpo do cinzel: pro lado, ate 2 tiles do pe.
            if (direcao.y > 0f) { centro = new Vector2(0f, 2.05f); tamanho = new Vector2(1.2f, 1.7f); }
            else if (direcao.y < 0f) { centro = new Vector2(0f, -0.7f); tamanho = new Vector2(1.1f, 1.6f); }
            else { centro = new Vector2(direcao.x * 1.15f, 0.65f); tamanho = new Vector2(1.8f, 1.2f); }

            Vector2 pontoOrigem = transform.position;
            MostrarCorte(centro, direcao);
            Sfx.Balanco.Tocar();

            bool atingido = false;
            bool pousado = false;
            int quantidade = Physics2D.OverlapBoxNonAlloc(pontoOrigem + centro, tamanho, 0f, _golpes);

            for (int i = 0; i < quantidade; i++)
            {
                Collider2D golpe = _golpes[i];
                if (golpe == null || golpe.transform.IsChildOf(transform)) continue;

                var danificavel = golpe.GetComponentInParent<IDamageable>();
                if (danificavel != null)
                {
                    danificavel.ReceberDano(dano + ScrapGems.DanoBonus, pontoOrigem);
                    atingido = true;

                    // Aparado (o Soldador de frente, o Sentinela em prata) tine em vez de acertar.
                    bool bloqueado = danificavel is Damageable alvo && alvo.UltimoTempoBloqueio == Time.time;
                    if (!bloqueado) pousado = true;
                    continue;
                }

                // Espinho e acido nao apanham, mas da pra quicar neles.
                if (direcao.y < 0f && golpe.GetComponentInParent<Hazard>() != null)
                {
                    atingido = true;
                    pousado = true;
                }
            }

            if (atingido)
            {
                (pousado ? Sfx.Golpe : Sfx.QuadroBloqueio).Tocar();

                Vector2 velocidadeVetorial = _corpo.linearVelocity;

                if (direcao.y < 0f) velocidadeVetorial.y = velocidadePogo;
                else if (direcao.y == 0f) velocidadeVetorial.x = -direcao.x * coice;

                _corpo.linearVelocity = velocidadeVetorial;
            }

            yield return new WaitForSeconds(tempoBalanco);
            if (corte != null) corte.enabled = false;
            EstaGolpeando = false;
            TempoFimGolpe = Time.time;

            yield return new WaitForSeconds(recuperacao);
            _ocupado = false;
        }

        private void MostrarCorte(Vector2 centro, Vector2 direcao)
        {
            if (corte == null) return;

            corte.transform.localPosition = centro;

            // O sprite do risco aponta pra direita.
            float angulo = direcao.y > 0f ? 90f : (direcao.y < 0f ? -90f : 0f);
            corte.transform.localRotation = Quaternion.Euler(0f, 0f, angulo);
            corte.flipX = direcao.y == 0f && direcao.x < 0f;

            // Com o rubi cravado o golpe sai em brasa.
            corte.color = ScrapGems.EncaixadoRubi ? new Color(1f, 0.45f, 0.30f) : Color.white;
            corte.enabled = true;
        }
    }
}
