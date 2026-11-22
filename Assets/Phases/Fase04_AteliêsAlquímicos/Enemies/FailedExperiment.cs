using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Failed Experiment: um automato de prata e cobre costurados, resultado de
    /// um experimento. Metade do corpo e rapida, metade e lenta — entao ele
    /// alterna, sem aviso de ritmo, entre arrastar a perna de cobre e disparar
    /// com a de prata.
    ///
    /// O arrasto e lento e da pra ler. O disparo tem um aviso curto: ele trava
    /// e treme antes de sair, e o olho branco acende.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class FailedExperiment : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do ExperimentArt, no editor.
        public const int QuadroOcioso = 0;
        public const int QuadrosOcioso = 4;
        public const int QuadroArrasto = 4;
        public const int QuadrosArrasto = 8;
        public const int QuadroCarga = 12;
        public const int QuadrosCarga = 2;
        public const int QuadroBote = 14;
        public const int QuadrosBote = 4;
        public const int Ferido = 18;
        public const int QuadroMorte = 19;
        public const int QuadrosMorte = 7;

        [UnityEngine.Serialization.FormerlySerializedAs("dragSpeed")] [SerializeField] private float velocidadeArrasto = 1.1f;
        [UnityEngine.Serialization.FormerlySerializedAs("lungeSpeed")] [SerializeField] private float velocidadeBote = 9f;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Tooltip("Limites de onde ele anda, em x de mundo.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;

        [UnityEngine.Serialization.FormerlySerializedAs("sight")] [Tooltip("Ate onde ele percebe o Scrap, em tiles.")]
        [SerializeField] private float visao = 9f;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private Transform _jogador;
        private int _direcao = 1;
        private bool _dando;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, Sprite[] novoQuadros = null)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            if (novoQuadros != null) quadros = novoQuadros;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();
            _danificavel.AnimacaoMorte = Desmontar;
        }

        private void Start()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null) _jogador = controlador.transform;
        }

        private void OnEnable()
        {
            _dando = false;
            StartCoroutine(Cerebro());
        }

        private bool Ferindo => Time.time - _danificavel.UltimoTempoGolpe < 0.25f;

        private IEnumerator Cerebro()
        {
            while (!_danificavel.EstaMorto)
            {
                // A metade de cobre: arrasta.
                float tempoArrasto = Random.Range(0.8f, 2.2f);

                for (float t = 0f; t < tempoArrasto; t += Time.deltaTime)
                {
                    if (_danificavel.EstaMorto) yield break;

                    if (Ferindo)
                    {
                        Mostrar(Ferido);
                    }
                    else
                    {
                        Avancar(velocidadeArrasto);
                        Mostrar(QuadroArrasto + (int)(Time.time * 8f) % QuadrosArrasto);
                    }

                    yield return null;
                }

                if (!Enxerga()) continue;

                // A metade de prata: trava, treme e dispara.
                _direcao = _jogador.position.x >= transform.position.x ? 1 : -1;
                Virar();

                Vector3 ancora = transform.position;
                Sfx.CargaExperimento.Tocar(ancora);
                for (float t = 0f; t < 0.35f; t += Time.deltaTime)
                {
                    if (_danificavel.EstaMorto) yield break;
                    transform.position = ancora + Vector3.right * (Mathf.Sin(t * 90f) * 0.06f);
                    Mostrar(QuadroCarga + (int)(t * 20f) % QuadrosCarga);
                    yield return null;
                }

                transform.position = ancora;
                _dando = true;
                Sfx.BoteExperimento.Tocar(ancora);

                for (float t = 0f; t < 0.45f; t += Time.deltaTime)
                {
                    if (_danificavel.EstaMorto) yield break;
                    Avancar(velocidadeBote);
                    Mostrar(QuadroBote + (int)(t * 14f) % QuadrosBote);
                    yield return null;
                }

                _dando = false;

                for (float t = 0f; t < 0.5f; t += Time.deltaTime)
                {
                    if (_danificavel.EstaMorto) yield break;
                    Mostrar(Ferindo ? Ferido : QuadroOcioso + (int)(t * 6f) % QuadrosOcioso);
                    yield return null;
                }
            }
        }

        private bool Enxerga()
        {
            if (_jogador == null) return false;

            Vector2 deslocamento = _jogador.position - transform.position;
            return Mathf.Abs(deslocamento.x) < visao && Mathf.Abs(deslocamento.y) < 3f;
        }

        private void Avancar(float velocidade)
        {
            Vector3 posicao = transform.position;
            posicao.x += _direcao * velocidade * Time.deltaTime;

            if (posicao.x <= minX) { posicao.x = minX; _direcao = 1; }
            else if (posicao.x >= maxX) { posicao.x = maxX; _direcao = -1; }

            transform.position = posicao;
            Virar();
        }

        private void Virar()
        {
            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _direcao < 0;
        }

        private void Mostrar(int quadro)
        {
            if (_sprite == null || quadros == null || quadro < 0 || quadro >= quadros.Length || quadros[quadro] == null) return;
            FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }

        /// <summary>A morte: a costura arrebenta e as metades caem cada uma pra um lado.</summary>
        private IEnumerator Desmontar()
        {
            _dando = false;
            if (_sprite != null) _sprite.color = Color.white;

            for (int i = 0; i < QuadrosMorte; i++)
            {
                Mostrar(QuadroMorte + i);
                yield return new WaitForSeconds(i == 0 ? 0.15f : 0.11f);
            }

            yield return new WaitForSeconds(0.2f);
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
            if (vida != null) vida.LevarDano(_dando ? 2 : 1, transform.position);
        }
    }
}
