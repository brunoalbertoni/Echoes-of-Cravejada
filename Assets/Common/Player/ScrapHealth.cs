using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A vida do Scrap, e o que acontece quando ela acaba.
    ///
    /// Morrer e Souls-like: ele deixa a "old scrap" com todos os Opaque Shards
    /// no ultimo chao seguro em que pisou e renasce na ultima Workbench em que
    /// encostou (ou onde a cena comeca, se ainda nao achou nenhuma). Morrer de
    /// novo antes de buscar perde o que ficou la.
    /// </summary>
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapHealth : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("maxHealth")] [SerializeField] private int vidaMax = 5;

        [UnityEngine.Serialization.FormerlySerializedAs("invulnerabilityTime")] [Tooltip("Segundos sem poder apanhar de novo depois de um golpe.")]
        [SerializeField] private float tempoInvulnerabilidade = 1.0f;

        [UnityEngine.Serialization.FormerlySerializedAs("knockback")] [Tooltip("Empurrao ao apanhar: pra tras e um pouco pra cima.")]
        [SerializeField] private Vector2 recuoGolpe = new Vector2(6f, 7f);

        [UnityEngine.Serialization.FormerlySerializedAs("oldScrapSprite")] [Tooltip("O sprite da old scrap deixada no lugar da morte.")]
        [SerializeField] private Sprite spriteSucataAntigo;

        private ScrapController _controlador;
        private ScrapRespawn _renascimento;
        private Rigidbody2D _corpo;
        private SpriteRenderer _sprite;

        private int _vida;
        private float _invulneravelAte;
        private Vector3 _posicaoInicio;

        // A bancada vale pra cena em que ela esta. Trocar de cena esquece.
        private static bool _temPontoSalvo;
        private static Vector3 _pontoSalvo;
        private static OldScrap _sucataAntigo;

        public int Vida => _vida;
        public int VidaMax => vidaMax;

        /// <summary>
        /// Avisa que o Scrap morreu. Os inimigos e o boss escutam pra renascer
        /// e encher a vida: morrer devolve o mundo ao que era.
        /// </summary>
        public static event System.Action Morreu;

        /// <summary>Quando ele apanhou pela ultima vez. A alavanca usa pra saber se foi interrompida.</summary>
        public float UltimoTempoGolpe { get; private set; } = -100f;

        /// <summary>Quando ele morreu pela ultima vez. A bancada usa pra nao tocar o "salvo" quando ele renasce nela.</summary>
        public static float UltimoTempoMorte { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            UltimoTempoMorte = -100f;
            _temPontoSalvo = false;
            _sucataAntigo = null;
            _vidaBonus = 0;
            Morreu = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Sprite novoSpriteSucataAntigo)
        {
            spriteSucataAntigo = novoSpriteSucataAntigo;
        }

        private void Awake()
        {
            _controlador = GetComponent<ScrapController>();
            _renascimento = GetComponent<ScrapRespawn>();
            _corpo = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();

            vidaMax += _vidaBonus;
            _vida = vidaMax;
            _posicaoInicio = transform.position;

            // Cena nova, bancada nova.
            _temPontoSalvo = false;
        }

        // O chassi reforcado vale pra sessao toda, em qualquer cena.
        [SaveProgress("_bonusHealth")]
        private static int _vidaBonus;

        /// <summary>Crisol reforca o chassi: um ponto de vida a mais, pra sempre.</summary>
        public void MelhorarChassi()
        {
            _vidaBonus++;
            vidaMax++;
            _vida = vidaMax;
        }

        /// <summary>A Workbench chama isto: salva o lugar e enche a vida.</summary>
        public void DescansarEm(Vector3 posicao)
        {
            _temPontoSalvo = true;
            _pontoSalvo = posicao;
            _vida = vidaMax;
        }

        /// <summary>
        /// Tira vida. Com <paramref name="retornoParaSeguroSolo"/> ele tambem e
        /// jogado de volta pro ultimo chao seguro — e o caso dos espinhos e do
        /// acido, de onde nao da pra sair andando.
        /// </summary>
        public void LevarDano(int quantia, Vector2 de, bool retornoParaSeguroSolo = false)
        {
            if (Time.time < _invulneravelAte || _vida <= 0) return;


            // LINHA NOVA: se estiver no Dash, ignora o dano totalmente
            var dash = GetComponent<ScrapDash>();
            if (dash != null && dash.EstaDashando) return;

            // Durante uma cutscene ninguem apanha: o que ainda estava voando
            // quando ela comecou (sucata, onda de choque) passa reto.
            if (Cinematic.Ativo) return;

            _vida -= quantia;
            UltimoTempoGolpe = Time.time;
            _invulneravelAte = Time.time + tempoInvulnerabilidade;

            if (_vida <= 0)
            {
                Morrer();
                return;
            }

            Sfx.Ferido.Tocar();

            if (retornoParaSeguroSolo && _renascimento != null)
            {
                _renascimento.Renascer(true);
            }
            else
            {
                float longe2 = transform.position.x >= de.x ? 1f : -1f;
                _controlador.Empurrar(new Vector2(longe2 * recuoGolpe.x, recuoGolpe.y), 0.25f);
            }

            StopAllCoroutines();
            StartCoroutine(Piscar());
        }

        /// <summary>
        /// A morte: os fragmentos ficam no lugar da queda, o Scrap apaga e o
        /// mundo congela atras da tela de morte. O renascimento so vem quando o
        /// jogador escolhe voltar pra bancada (ver ReturnToBench).
        /// </summary>
        private void Morrer()
        {
            UltimoTempoMorte = Time.time;
            Sfx.QuadroMorte.Tocar();

            Vector3 lugarMorte = _renascimento != null ? (Vector3)_renascimento.PosicaoSeguro : transform.position;

            // O que ja estava no chao de uma morte anterior se perde.
            if (_sucataAntigo != null) Destroy(_sucataAntigo.gameObject);
            _sucataAntigo = null;

            int fragmentos = ScrapWallet.PegarTudo();
            int mantido = 0;

            // Com a Bolsa forrada, da loja, metade fica com ele.
            if (Shop.MantemMetadeLigadoMorte)
            {
                mantido = fragmentos / 2;
                ScrapWallet.Adicionar(mantido);
                fragmentos -= mantido;
            }

            if (fragmentos > 0) _sucataAntigo = OldScrap.Soltar(lugarMorte, fragmentos, spriteSucataAntigo);

            // Caido: parado, sem controle, apagado.
            StopAllCoroutines();
            _controlador.ControleTravado = true;
            _corpo.linearVelocity = Vector2.zero;
            _corpo.simulated = false;
            if (_sprite != null)
            {
                _sprite.enabled = true;
                _sprite.color = new Color(0.32f, 0.28f, 0.28f);
            }

            DeathScreen.Mostrar(VoltarParaBancada);
        }

        /// <summary>O jogador escolheu voltar: o Scrap renasce na ultima bancada, com a vida cheia.</summary>
        private void VoltarParaBancada()
        {
            Sfx.Renascimento.Tocar();

            if (_sprite != null) _sprite.color = Color.white;
            _corpo.simulated = true;

            Vector3 alvo = _temPontoSalvo ? _pontoSalvo : _posicaoInicio;

            _corpo.position = alvo;
            transform.position = alvo;
            _corpo.linearVelocity = Vector2.zero;
            _controlador.ControleTravado = false;

            if (_renascimento != null) _renascimento.DefinirPosicaoSegura(alvo);

            _vida = vidaMax;
            _invulneravelAte = Time.time + tempoInvulnerabilidade;

            Camera principal = Camera.main;
            var seguir = principal != null ? principal.GetComponent<ScrapCameraFollow>() : null;
            if (seguir != null) seguir.IrDiretoAoAlvo();

            StopAllCoroutines();
            StartCoroutine(Piscar());

            Morreu?.Invoke();
        }

        private IEnumerator Piscar()
        {
            if (_sprite == null) yield break;

            while (Time.time < _invulneravelAte)
            {
                _sprite.enabled = !_sprite.enabled;
                yield return new WaitForSeconds(0.07f);
            }

            _sprite.enabled = true;
        }
    }
}
