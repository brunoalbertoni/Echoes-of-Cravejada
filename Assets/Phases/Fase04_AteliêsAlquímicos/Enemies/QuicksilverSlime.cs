using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Quicksilver Slime: uma massa de mercurio corrompido, com uma engrenagem
    /// dentro e dois olhos de vidro boiando. Rasteja de um lado pro outro, como
    /// lagarta. Golpeada, nao morre: se esparrama e se parte em duas menores,
    /// mais rapidas. So as pequenas se desfazem de vez.
    ///
    /// As pequenas nascem em jogo; quando o Scrap morre elas somem e a grande
    /// volta inteira.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class QuicksilverSlime : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do SlimeArt, no editor.
        public const int QuadroOcioso = 0;
        public const int QuadrosOcioso = 4;
        public const int QuadroRastejar = 4;
        public const int QuadrosRastejar = 6;
        public const int Ferido = 10;
        public const int QuadroMorte = 11;
        public const int QuadrosMorte = 6;

        private const float EscalaPequeno = 0.65f;

        // A divisao: tremer, inchar pros lados e so entao estourar em duas.
        private const float TempoTremor = 0.38f;
        private const float TempoEsticar = 0.24f;

        // A pequena recem-nascida fica um instante parada antes de rastejar.
        private const float TempoAssentarMetade = 0.35f;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [SerializeField] private float velocidade = 1.4f;

        [UnityEngine.Serialization.FormerlySerializedAs("halfHealth")] [Tooltip("Vida de cada metade que nasce da divisao.")]
        [SerializeField] private int vidaMetade = 2;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Tooltip("Limites de onde ela rasteja, em x de mundo.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;

        [UnityEngine.Serialization.FormerlySerializedAs("small")] [Tooltip("As pequenas nao se partem de novo.")]
        [SerializeField] private bool pequeno;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes. As pequenas herdam.")]
        [SerializeField] private Sprite[] quadros;

        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private int _direcao = 1;
        private float _baseY;
        private float _relogio;
        private float _nascidoEm = -100f;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, Sprite[] novoQuadros = null, int novoVidaMetade = 2)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            if (novoQuadros != null) quadros = novoQuadros;
            vidaMetade = novoVidaMetade;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();
            _baseY = transform.position.y;
            _relogio = Mathf.Abs(transform.position.x * 0.31f);

            // A grande "morre" se partindo, devagar o bastante pra ver; as
            // pequenas (acertadas no Split) se desfazem numa poca.
            _danificavel.AnimacaoMorte = pequeno ? (System.Func<IEnumerator>)Espalhar : DividirSe;
            _danificavel.SomMorte = pequeno ? Sfx.GosmaMorte : Sfx.DivisaoGosma;
            _danificavel.Revivido += ReiniciarForma;

            if (pequeno) ScrapHealth.Morreu += Sumir;
        }

        private void OnDestroy()
        {
            if (pequeno) ScrapHealth.Morreu -= Sumir;
            if (_danificavel != null) _danificavel.Revivido -= ReiniciarForma;
        }

        private void ReiniciarForma()
        {
            transform.localScale = Vector3.one * (pequeno ? EscalaPequeno : 1f);
        }

        private void Sumir()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// A grande "morreu", mas nao se parte na hora: treme, incha pros
        /// lados, afina no meio e so entao estoura em duas. Sem colisao
        /// durante isso (o Damageable ja a desligou): e a folga do Scrap.
        /// </summary>
        private IEnumerator DividirSe()
        {
            if (_sprite != null) _sprite.color = Color.white;
            Mostrar(Ferido);

            Vector3 descanso = Vector3.one;

            // 1. Treme, cada vez mais rapido.
            for (float t = 0f; t < TempoTremor; t += Time.deltaTime)
            {
                float k = t / TempoTremor;
                float oscilacao = Mathf.Sin(t * Mathf.Lerp(30f, 60f, k)) * Mathf.Lerp(0.04f, 0.10f, k);
                transform.localScale = new Vector3(descanso.x + oscilacao, descanso.y - oscilacao, 1f);
                Mostrar(QuadroOcioso + (int)(t * 16f) % QuadrosOcioso);
                yield return null;
            }

            // 2. Incha pros lados e abaixa: vai se partir no meio.
            for (float t = 0f; t < TempoEsticar; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / TempoEsticar);
                transform.localScale = new Vector3(Mathf.Lerp(1f, 1.55f, k), Mathf.Lerp(1f, 0.62f, k), 1f);
                yield return null;
            }

            // 3. Estoura: nascem as duas e a grande some.
            Dividir();
            transform.localScale = descanso;
            if (_sprite != null) _sprite.enabled = false;
            yield return null;
            if (_sprite != null) _sprite.enabled = true;
        }

        /// <summary>Nascem duas pequenas, uma pra cada lado, ainda moles.</summary>
        private void Dividir()
        {
            for (int lado = -1; lado <= 1; lado += 2)
            {
                var metade = new GameObject(name + "_Half");
                metade.transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x + lado * 0.6f, minX, maxX), _baseY, 0f);
                metade.transform.localScale = Vector3.one * EscalaPequeno;

                var renderizador = metade.AddComponent<SpriteRenderer>();
                renderizador.sprite = quadros != null && quadros.Length > QuadroRastejar ? quadros[QuadroRastejar] : (_sprite != null ? _sprite.sprite : null);
                renderizador.sortingLayerName = "Actors";
                renderizador.sortingOrder = -3;
                renderizador.flipX = lado < 0;

                var corpo = metade.AddComponent<Rigidbody2D>();
                corpo.bodyType = RigidbodyType2D.Kinematic;

                var gatilho = metade.AddComponent<BoxCollider2D>();
                gatilho.isTrigger = true;
                gatilho.size = new Vector2(1.3f, 0.9f);
                gatilho.offset = new Vector2(0f, 0.45f);

                metade.AddComponent<Damageable>().Configurar(vidaMetade, 0, null);

                var gosma = metade.AddComponent<QuicksilverSlime>();
                gosma.minX = minX;
                gosma.maxX = maxX;
                gosma.velocidade = velocidade * 1.9f;
                gosma.pequeno = true;
                gosma.quadros = quadros;
                gosma._direcao = lado;
                gosma._nascidoEm = Time.time;

                // O Awake rodou antes de "small" valer: acerta as inscricoes.
                gosma._danificavel.AnimacaoMorte = gosma.Espalhar;
                gosma._danificavel.SomMorte = Sfx.GosmaMorte;
                ScrapHealth.Morreu += gosma.Sumir;
            }
        }

        private void Update()
        {
            if (_danificavel.EstaMorto) return;

            // Recem-nascida: assenta no chao e incha ate o tamanho dela antes de sair rastejando.
            if (pequeno && Time.time - _nascidoEm < TempoAssentarMetade)
            {
                float k = (Time.time - _nascidoEm) / TempoAssentarMetade;
                float achatamento = Mathf.Sin(k * Mathf.PI) * 0.25f;
                transform.localScale = new Vector3(EscalaPequeno * (1f + achatamento), EscalaPequeno * (1f - achatamento), 1f);
                Mostrar(QuadroOcioso + (int)(k * 6f) % QuadrosOcioso);
                return;
            }

            if (pequeno) transform.localScale = Vector3.one * EscalaPequeno;

            // Apanhou: espirra, parada.
            if (Time.time - _danificavel.UltimoTempoGolpe < 0.2f)
            {
                Mostrar(Ferido);
                return;
            }

            Vector3 posicao = transform.position;
            posicao.x += _direcao * velocidade * Time.deltaTime;

            if (posicao.x <= minX) { posicao.x = minX; _direcao = 1; }
            else if (posicao.x >= maxX) { posicao.x = maxX; _direcao = -1; }

            transform.position = posicao;

            // Rasteja como lagarta: mais rapido, mais quadros.
            _relogio += Time.deltaTime * (4f + velocidade * 2.5f);
            Mostrar(QuadroRastejar + (int)_relogio % QuadrosRastejar);

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _direcao < 0;
        }

        private void Mostrar(int quadro)
        {
            if (_sprite == null || quadros == null || quadro < 0 || quadro >= quadros.Length || quadros[quadro] == null) return;
            FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }

        /// <summary>A morte: se esparrama numa poca e os olhos rolam pro chao.</summary>
        private IEnumerator Espalhar()
        {
            if (_sprite != null) _sprite.color = Color.white;

            for (int i = 0; i < QuadrosMorte; i++)
            {
                Mostrar(QuadroMorte + i);
                yield return new WaitForSeconds(1f / 12f);
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

            // Ainda assentando depois da divisao: mole demais pra machucar.
            if (pequeno && Time.time - _nascidoEm < TempoAssentarMetade) return;

            var controlador = outro.GetComponentInParent<ScrapController>();
            if (controlador == null) return;

            // Encostar machuca e ainda gruda mercurio no corpo.
            var vida = controlador.GetComponent<ScrapHealth>();
            if (vida != null) vida.LevarDano(1, transform.position);

            controlador.Desacelerar(1.5f);
        }
    }
}
