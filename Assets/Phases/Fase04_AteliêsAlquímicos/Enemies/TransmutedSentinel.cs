using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Transmuted Sentinel: uma armadura oca de vidro, movida por uma chama
    /// verde de Oxide que aparece pelo vidro. Muda de material quando e
    /// atingida:
    ///
    ///   vidro   translucida, a chama a vista. Fragil: o cinzel fere.
    ///   prata   opaca e polida. Blindada: o golpe so tine.
    ///
    /// Cada golpe que acerta no vidro a transmuta em prata por alguns segundos.
    /// Nao adianta martelar: e bater, recuar, esperar o vidro voltar. O raio de
    /// luz alquimica nao quer saber de material — queima do mesmo jeito.
    ///
    /// Os dois materiais sao duas folhas com os mesmos quadros (GlassSentinelArt).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class TransmutedSentinel : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do GlassSentinelArt, no editor.
        public const int QuadroOcioso = 0;
        public const int QuadrosOcioso = 4;
        public const int QuadroAndar = 4;
        public const int QuadrosAndar = 8;
        public const int Ferido = 12;
        public const int QuadroMorte = 13;
        public const int QuadrosMorte = 6;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [SerializeField] private float velocidade = 1.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Tooltip("Limites de onde ela anda, em x de mundo.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;

        [UnityEngine.Serialization.FormerlySerializedAs("silverTime")] [Tooltip("Segundos que ela fica em prata depois de um golpe.")]
        [SerializeField] private float tempoPrata = 2.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("contactDamage")] [Tooltip("Quanto tira do Scrap ao encostar. A armadura pesa: o dobro dos outros.")]
        [SerializeField] private int danoContato = 2;

        [UnityEngine.Serialization.FormerlySerializedAs("glassFrames")] [Tooltip("Os quadros de vidro e os de prata, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadrosVidro;
        [UnityEngine.Serialization.FormerlySerializedAs("silverFrames")] [SerializeField] private Sprite[] quadrosPrata;

        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private Transform _jogador;
        private float _prataAte;
        private float _golpeVisto = -100f;
        private float _relogio;

        public bool EstaPrata => Time.time < _prataAte;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, Sprite[] novoQuadrosVidro = null, Sprite[] novoQuadrosPrata = null, int novoDanoContato = 2)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            danoContato = novoDanoContato;
            if (novoQuadrosVidro != null) quadrosVidro = novoQuadrosVidro;
            if (novoQuadrosPrata != null) quadrosPrata = novoQuadrosPrata;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();
            _relogio = Mathf.Abs(transform.position.x * 0.27f);

            // Em prata ela apara tudo, venha de onde vier.
            _danificavel.Bloqueios = de => EstaPrata;
            _danificavel.Revivido += () => _prataAte = 0f;
            _danificavel.AnimacaoMorte = Estilhacar;
            _danificavel.SomMorte = Sfx.VidroMorte;
        }

        private void Start()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null) _jogador = controlador.transform;
        }

        private void Update()
        {
            if (_danificavel.EstaMorto) return;

            // Um golpe novo entrou no vidro: vira prata.
            if (_danificavel.UltimoTempoGolpe > _golpeVisto)
            {
                _golpeVisto = _danificavel.UltimoTempoGolpe;
                _prataAte = Time.time + tempoPrata;
                Sfx.Transmutar.Tocar(transform.position);
            }

            // O material e o aviso. Perto de voltar a vidro, ela pisca.
            float esquerda = _prataAte - Time.time;
            bool prataMostrar = esquerda > 0.6f || (esquerda > 0f && Mathf.FloorToInt(esquerda * 12f) % 2 == 0);

            bool andando = false;
            if (_jogador != null)
            {
                Vector2 deslocamento = _jogador.position - transform.position;
                if (Mathf.Abs(deslocamento.x) <= 10f && Mathf.Abs(deslocamento.y) <= 3f && Mathf.Abs(deslocamento.x) >= 0.3f)
                {
                    // Em prata ela e pesada: anda pela metade.
                    float ritmo = EstaPrata ? velocidade * 0.5f : velocidade;
                    float direcao = Mathf.Sign(deslocamento.x);

                    Vector3 posicao = transform.position;
                    float antes = posicao.x;
                    posicao.x = Mathf.Clamp(posicao.x + direcao * ritmo * Time.deltaTime, minX, maxX);
                    transform.position = posicao;
                    andando = !Mathf.Approximately(antes, posicao.x);

                    // A arte olha pra direita.
                    if (_sprite != null) _sprite.flipX = direcao < 0f;
                }
            }

            int quadro;
            if (Time.time - _danificavel.UltimoTempoGolpe < 0.15f)
            {
                quadro = Ferido;
            }
            else if (andando)
            {
                _relogio += Time.deltaTime * (EstaPrata ? 5f : 9f);
                quadro = QuadroAndar + (int)_relogio % QuadrosAndar;
            }
            else
            {
                _relogio += Time.deltaTime * 4f;
                quadro = QuadroOcioso + (int)_relogio % QuadrosOcioso;
            }

            Mostrar(prataMostrar ? quadrosPrata : quadrosVidro, quadro);
        }

        private void Mostrar(Sprite[] quadros, int quadro)
        {
            if (_sprite == null || quadros == null || quadro < 0 || quadro >= quadros.Length || quadros[quadro] == null) return;
            FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }

        /// <summary>A morte (so morre de vidro): racha inteira, vira cacos e a chama escapa.</summary>
        private IEnumerator Estilhacar()
        {
            if (_sprite != null) _sprite.color = Color.white;

            for (int i = 0; i < QuadrosMorte; i++)
            {
                Mostrar(quadrosVidro, QuadroMorte + i);
                yield return new WaitForSeconds(i == 0 ? 0.15f : 0.1f);
            }
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Encostar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Encostar(outro);
        }

        private void Encostar(Collider2D outro)
        {
            if (_danificavel.EstaMorto) return;

            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(danoContato, transform.position);
        }
    }
}
