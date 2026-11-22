using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Echo Hunter: um mineiro do turno do Capataz, corrompido, de olhos
    /// vendados e uma trompa de cristal no lugar da lanterna. Ele nao ve o
    /// Scrap — so ouve.
    ///
    /// Fica parado ate um ruido chegar nele. Entao corre ate a ORIGEM do som,
    /// fareja um instante e, sem ouvir mais nada, volta devagar pro lugar. Da
    /// pra passar por ele so andando e pulando baixo — sem dash, sem golpe, sem
    /// queda alta — ou manda-lo pra longe golpeando um cristal ressonante. Se
    /// ele alcancar, machuca muito.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class EchoHunter : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do HunterArt, no editor.
        public const int QuadroOcioso = 0;
        public const int QuadrosOcioso = 4;
        public const int QuadroAlerta = 4;
        public const int QuadrosAlerta = 2;
        public const int QuadroCorrida = 6;
        public const int QuadrosCorrida = 6;
        public const int QuadroFarejar = 12;
        public const int QuadrosFarejar = 4;
        public const int QuadroAndar = 16;
        public const int QuadrosAndar = 6;
        public const int Ferido = 22;
        public const int QuadroMorte = 23;
        public const int QuadrosMorte = 6;

        [UnityEngine.Serialization.FormerlySerializedAs("chargeSpeed")] [SerializeField] private float velocidadeCarga = 7f;
        [UnityEngine.Serialization.FormerlySerializedAs("returnSpeed")] [SerializeField] private float velocidadeRetorno = 1.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("hearingThreshold")] [Tooltip("Ruido mais fraco que isso ele nao percebe. Passo faz 0.15 e pulo 0.2; golpe 0.8, dash 1.")]
        [SerializeField, Range(0f, 1f)] private float limiarAudicao = 0.3f;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Tooltip("Limites de onde ele consegue ir, em x de mundo.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;

        [UnityEngine.Serialization.FormerlySerializedAs("contactDamage")] [SerializeField] private int danoContato = 2;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private Vector3 _origem;
        private float _alvoX;
        private float _farejarAte;
        private int _estado;   // 0 parado, 1 correndo pro som, 2 farejando, 3 voltando
        private float _estadoDesde;
        private float _relogio;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, Sprite[] novoQuadros = null)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            if (novoQuadros != null) quadros = novoQuadros;
        }

        /// <summary>O mesmo Configure, devolvendo ele: o gerador encadeia.</summary>
        public EchoHunter ConfigurarEDevolver(float novoMinX, float novoMaxX, Sprite[] novoQuadros = null)
        {
            Configurar(novoMinX, novoMaxX, novoQuadros);
            return this;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();
            _origem = transform.position;
            _relogio = Mathf.Abs(transform.position.x * 0.29f);

            _danificavel.Revivido += VoltarParaCasa;
            _danificavel.AnimacaoMorte = Desabar;
            _danificavel.SomMorte = Sfx.CacadorMorte;
        }

        private void OnDestroy()
        {
            if (_danificavel != null) _danificavel.Revivido -= VoltarParaCasa;
        }

        private void OnEnable()
        {
            NoiseSystem.Ouvido += AoOuvirRuido;
        }

        private void OnDisable()
        {
            NoiseSystem.Ouvido -= AoOuvirRuido;
        }

        private void VoltarParaCasa()
        {
            DefinirEstado(0);
            transform.position = _origem;
        }

        private void DefinirEstado(int estado)
        {
            if (_estado == estado) return;
            _estado = estado;
            _estadoDesde = Time.time;

            // Ouviu: o estalo da trompa antes de sair correndo.
            if (estado == 1) Sfx.AlertaCacador.Tocar(transform.position);
        }

        private void AoOuvirRuido(Vector2 pontoOrigem, float raioArea, float volumeRuido2)
        {
            if (_danificavel.EstaMorto || volumeRuido2 < limiarAudicao) return;

            // Ele escuta na mesma altura, mais ou menos: um andar acima ja abafa.
            Vector2 deslocamento = pontoOrigem - (Vector2)transform.position;
            if (Mathf.Abs(deslocamento.y) > 5f || deslocamento.magnitude > raioArea + 2f) return;

            _alvoX = Mathf.Clamp(pontoOrigem.x, minX, maxX);
            DefinirEstado(1);
        }

        private void Update()
        {
            if (_danificavel.EstaMorto) return;

            Vector3 posicao = transform.position;

            // Apanhou: cambaleia, e passa a saber onde o Scrap esta.
            if (Time.time - _danificavel.UltimoTempoGolpe < 0.25f)
            {
                posicao.x = Mathf.Clamp(posicao.x - _danificavel.UltimoLadoGolpe * 3f * Time.deltaTime, minX, maxX);
                transform.position = posicao;
                Mostrar(Ferido);
                return;
            }

            Animar();

            if (_estado == 0) return;

            if (_estado == 2)
            {
                if (Time.time >= _farejarAte) DefinirEstado(3);
                return;
            }

            float objetivo = _estado == 1 ? _alvoX : _origem.x;
            float velocidade = _estado == 1 ? velocidadeCarga : velocidadeRetorno;

            posicao.x = Mathf.MoveTowards(posicao.x, objetivo, velocidade * Time.deltaTime);
            transform.position = posicao;

            // A arte olha pra direita.
            if (_sprite != null && !Mathf.Approximately(objetivo, posicao.x)) _sprite.flipX = objetivo < posicao.x;

            if (!Mathf.Approximately(posicao.x, objetivo)) return;

            if (_estado == 1)
            {
                DefinirEstado(2);
                _farejarAte = Time.time + 1.6f;
            }
            else
            {
                DefinirEstado(0);
            }
        }

        /// <summary>Escutando, de estalo, correndo, farejando ou voltando.</summary>
        private void Animar()
        {
            float desde = Time.time - _estadoDesde;
            _relogio += Time.deltaTime;

            switch (_estado)
            {
                case 1:
                    // O estalo de quem ouviu, e a corrida.
                    if (desde < 0.16f) Mostrar(QuadroAlerta + Mathf.Min(QuadrosAlerta - 1, (int)(desde / 0.08f)));
                    else Mostrar(QuadroCorrida + (int)(Time.time * 13f) % QuadrosCorrida);
                    break;

                case 2:
                    Mostrar(QuadroFarejar + (int)(desde * 6f) % QuadrosFarejar);
                    break;

                case 3:
                    Mostrar(QuadroAndar + (int)(_relogio * 6f) % QuadrosAndar);
                    break;

                default:
                    Mostrar(QuadroOcioso + (int)(_relogio * 4f) % QuadrosOcioso);
                    break;
            }
        }

        private void Mostrar(int quadro)
        {
            if (_sprite == null || quadros == null || quadro < 0 || quadro >= quadros.Length || quadros[quadro] == null) return;
            FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }

        /// <summary>A morte: leva as maos a trompa, cai de joelhos, tomba e a trompa se desfaz.</summary>
        private IEnumerator Desabar()
        {
            if (_sprite != null) _sprite.color = Color.white;

            float[] segurar = { 0.12f, 0.15f, 0.18f, 0.2f, 0.2f, 0.25f };
            for (int i = 0; i < QuadrosMorte; i++)
            {
                Mostrar(QuadroMorte + i);
                yield return new WaitForSeconds(segurar[i]);
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
