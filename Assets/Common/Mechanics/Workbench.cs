using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A Bancada de Ourives: o checkpoint. Um altar de pedra e latao com a
    /// bigorna, o cadinho e uma gema presa numa engrenagem.
    ///
    /// Apagada, a gema e cinza e respira devagar. Perto dela, E salva: tres
    /// marteladas na bigorna (com fagulhas), a gema esquenta ate o branco, a
    /// vida enche, as gemas brutas sao cravadas e o momento vira uma gravura
    /// numa placa de latao (ver <see cref="SaveEngraving"/>). Dali pra frente
    /// ela fica acesa — a engrenagem gira, o cadinho tem brasa — e e onde o
    /// Scrap renasce. So uma fica acesa por vez.
    ///
    /// Os quadros vem de Resources/Checkpoint (ver CheckpointArt, no editor):
    /// a bancada troca de cara sozinha em todas as cenas.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Workbench : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do CheckpointArt, no editor.
        private const int QuadroAdormecido = 0;
        private const int QuadrosAdormecido = 4;
        private const int QuadroAceso = 4;
        private const int QuadrosAceso = 6;
        private const int QuadroForja = 10;
        private const int QuadrosForja = 6;

        /// <summary>Onde fica o meio da gema e a bigorna, em tiles a partir do canto de baixo a esquerda.</summary>
        private static readonly Vector2 DeslocamentoGema = new Vector2(1.5f, 1.94f);
        private static readonly Vector2 DeslocamentoBigorna = new Vector2(0.72f, 1.08f);

        private static readonly Color LuzAdormecido = new Color(0.55f, 0.68f, 0.95f);
        private static readonly Color LuzAceso = new Color(0.55f, 0.92f, 1f);

        [UnityEngine.Serialization.FormerlySerializedAs("restPoint")] [Tooltip("Onde ele renasce. Vazio usa a posicao da propria bancada.")]
        [SerializeField] private Transform pontoDescanso;

        /// <summary>A ultima vez em que alguem salvou numa bancada.</summary>
        public static float UltimoTempoDescanso { get; private set; } = -100f;

        /// <summary>A bancada em que o Scrap esta encostado agora (o HUD pede o E).</summary>
        public static Workbench Proximo { get; private set; }

        /// <summary>A bancada acesa: a ultima em que se salvou nesta cena.</summary>
        public static Workbench Atual { get; private set; }

        /// <summary>Se ela esta no meio de salvar (o HUD nao pede o E de novo).</summary>
        public bool Ocupado => _ocupado;

        private static Sprite[] _quadros;

        private SpriteRenderer _sprite;
        private Light2D _luz;
        private ScrapController _controlador;
        private ScrapHealth _vida;
        private bool _ocupado;
        private float _inicioForja = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            UltimoTempoDescanso = -100f;
            Proximo = null;
            Atual = null;
            _quadros = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Transform novoPontoDescanso)
        {
            pontoDescanso = novoPontoDescanso;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            if (Quadros() != null && _sprite != null) _sprite.sprite = _quadros[QuadroAdormecido];

            // A luz da gema: fraca apagada, forte acesa, um estouro ao salvar.
            var objetoLuz = new GameObject("GemLight");
            objetoLuz.transform.SetParent(transform, false);
            objetoLuz.transform.localPosition = DeslocamentoGema;
            _luz = objetoLuz.AddComponent<Light2D>();
            _luz.lightType = Light2D.LightType.Point;
            _luz.pointLightInnerRadius = 0f;
            _luz.pointLightOuterRadius = 2f;
            _luz.color = LuzAdormecido;
            _luz.intensity = 0.2f;
        }

        private void Start()
        {
            // O jogo foi carregado de um save feito aqui: ela ja abre acesa.
            Vector3 descanso = pontoDescanso != null ? pontoDescanso.position : transform.position;
            if (SaveSystem.BancadaCarregado.HasValue && Vector2.Distance(SaveSystem.BancadaCarregado.Value, descanso) < 0.5f) Atual = this;
        }

        private void OnDestroy()
        {
            if (Proximo == this) Proximo = null;
            if (Atual == this) Atual = null;
        }

        /// <summary>Os quadros, carregados uma vez. Null se nao estiverem la (fica a arte antiga da cena).</summary>
        private static Sprite[] Quadros()
        {
            if (_quadros != null) return _quadros;

            Sprite[] carregado = Resources.LoadAll<Sprite>("Checkpoint");
            if (carregado == null || carregado.Length < QuadroForja + QuadrosForja)
            {
                Debug.LogWarning("[Bancada] Quadros nao encontrados em Resources/Checkpoint.");
                return null;
            }

            System.Array.Sort(carregado, (a, b) => string.CompareOrdinal(a.name, b.name));
            _quadros = carregado;
            return _quadros;
        }

        // ------------------------------------------------------------------
        // Chegar perto e apertar E
        // ------------------------------------------------------------------

        private void OnTriggerEnter2D(Collider2D outro)
        {
            var controlador = outro.GetComponentInParent<ScrapController>();
            if (controlador == null) return;

            _controlador = controlador;
            _vida = controlador.GetComponent<ScrapHealth>();
            Proximo = this;
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (_controlador == null || outro.GetComponentInParent<ScrapController>() != _controlador) return;

            if (Proximo == this) Proximo = null;
            _controlador = null;
        }

        /// <summary>Se da pra salvar agora: perto, no chao, sem conversa, sem cena, sem cartao aberto.</summary>
        public bool PodeSalvar =>
            !_ocupado && _controlador != null && _controlador.EstaNoChao && !_controlador.ControleTravado
            && !Cinematic.Ativo && !AbilityCard.Mostrando && Npc.Conversando == null && Npc.Proximo == null;

        private void Update()
        {
            Animar();

            if (Proximo == this && PodeSalvar && SalvarPressionado()) StartCoroutine(Salvar());
        }

        private static bool SalvarPressionado()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.eKey.wasPressedThisFrame || teclado.kKey.wasPressedThisFrame)) return true;

            Gamepad controle = Gamepad.current;
            return controle != null && controle.buttonNorth.wasPressedThisFrame;
        }

        // ------------------------------------------------------------------
        // Salvar
        // ------------------------------------------------------------------

        /// <summary>
        /// As tres marteladas, a gema esquentando, e no pico do clarao: salva,
        /// enche a vida, crava as gemas e grava a placa.
        /// </summary>
        private IEnumerator Salvar()
        {
            _ocupado = true;
            ScrapController controlador = _controlador;
            ScrapHealth vida = _vida;

            if (controlador != null)
            {
                controlador.ControleTravado = true;
                var corpo = controlador.GetComponent<Rigidbody2D>();
                if (corpo != null) corpo.linearVelocity = new Vector2(0f, corpo.linearVelocity.y);
            }

            // Tres marteladas, cada uma mais aguda: tin, tin, TAN.
            Vector2 bigorna = (Vector2)transform.position + DeslocamentoBigorna;
            float[] tom2 = { 0.94f, 1f, 1.12f };
            for (int i = 0; i < 3; i++)
            {
                Sfx.GolpeBancada.Tocar(bigorna, volume: i == 2 ? 1f : 0.8f, tom2: tom2[i]);
                BenchSpark.Estourar(bigorna, i == 2 ? 10 : 5, new Color(1f, 0.95f, 0.6f), new Color(1f, 0.4f, 0.1f), i == 2 ? 5f : 3.5f);
                if (i == 2) ScrapCameraFollow.Tremer(0.06f, 0.15f);
                yield return new WaitForSeconds(0.2f);
            }

            // A gema acende.
            Atual = this;
            _inicioForja = Time.time;
            Sfx.AcenderBancada.Tocar((Vector2)transform.position + DeslocamentoGema);
            BenchSpark.Estourar((Vector2)transform.position + DeslocamentoGema, 14, Color.white, new Color(0.4f, 0.85f, 1f), 6f, 180f);
            yield return new WaitForSeconds(0.2f);

            // O pico: salvo (no jogo e no arquivo).
            Vector3 descanso = pontoDescanso != null ? pontoDescanso.position : transform.position;
            if (vida != null) vida.DescansarEm(descanso);
            UltimoTempoDescanso = Time.time;
            ScrapGems.EncaixarNaBancada();
            SaveEngraving.Gravar();
            GameManager.Instancia.SalvarJogo(descanso);

            yield return new WaitForSeconds(0.45f);

            if (controlador != null) controlador.ControleTravado = false;
            _ocupado = false;
        }

        // ------------------------------------------------------------------
        // Animacao e luz
        // ------------------------------------------------------------------

        private void Animar()
        {
            float forja = (Time.time - _inicioForja) / 0.6f;
            bool aceso = Atual == this;

            if (_quadros != null && _sprite != null)
            {
                int quadro;
                if (forja >= 0f && forja < 1f) quadro = QuadroForja + Mathf.Min(QuadrosForja - 1, (int)(forja * QuadrosForja));
                else if (aceso) quadro = QuadroAceso + (int)(Time.time * 8f) % QuadrosAceso;
                else quadro = QuadroAdormecido + (int)(Time.time * 2.5f) % QuadrosAdormecido;

                _sprite.sprite = _quadros[quadro];
            }

            if (_luz == null) return;

            float pulso = 0.5f + 0.5f * Mathf.Sin(Time.time * (aceso ? 3f : 1.5f));
            float intensidade = aceso ? 0.9f + 0.25f * pulso : 0.18f + 0.08f * pulso;
            float raioArea = aceso ? 3.6f : 2f;

            // O estouro do salvamento: sobe na hora e volta em meio segundo e pouco.
            if (forja >= 0f && forja < 1.6f)
            {
                float rajada = Mathf.Sin(Mathf.Clamp01(forja / 1.6f) * Mathf.PI);
                intensidade += 2.6f * rajada;
                raioArea += 5f * rajada;
            }

            _luz.color = Color.Lerp(_luz.color, aceso ? LuzAceso : LuzAdormecido, Time.deltaTime * 4f);
            _luz.intensity = intensidade;
            _luz.pointLightOuterRadius = raioArea;
        }
    }
}
