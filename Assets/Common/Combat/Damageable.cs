using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Qualquer coisa em que o cinzel bate e conta: inimigos hoje, parede
    /// quebravel e boss depois.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>Leva um golpe vindo de <paramref name="de"/>.</summary>
        void ReceberDano(int quantia, Vector2 de);
    }

    /// <summary>
    /// Vida de inimigo: pisca ao apanhar e, ao morrer, solta Opaque Shards.
    ///
    /// Morrer nao destroi o inimigo, so o desliga: quando o Scrap morre, todos
    /// voltam pro lugar de origem com a vida cheia.
    /// </summary>
    public class Damageable : MonoBehaviour, IDamageable
    {
        [UnityEngine.Serialization.FormerlySerializedAs("health")] [SerializeField] private int vida = 2;

        [UnityEngine.Serialization.FormerlySerializedAs("shards")] [Tooltip("Quantos Opaque Shards solta ao morrer.")]
        [SerializeField] private int fragmentos = 3;

        [UnityEngine.Serialization.FormerlySerializedAs("shardSprite")] [SerializeField] private Sprite spriteFragmento;

        private SpriteRenderer _sprite;
        private Collider2D[] _colisores;
        private Color _corBase = Color.white;
        private bool _morto;
        private int _vidaCheio;
        private Vector3 _origem;

        /// <summary>Quando apanhou pela ultima vez. Quem move o inimigo usa pra recuar.</summary>
        public float UltimoTempoGolpe { get; private set; } = -100f;

        /// <summary>De que lado veio o ultimo golpe: -1 da esquerda, +1 da direita.</summary>
        public int UltimoLadoGolpe { get; private set; }

        /// <summary>
        /// Se preenchido, decide se um golpe vindo de um ponto e aparado. E como
        /// o Brass Welder se defende de frente.
        /// </summary>
        [UnityEngine.Serialization.FormerlySerializedAs("Blocks")] public System.Func<Vector2, bool> Bloqueios;

        /// <summary>Avisa que ele morreu, antes de sumir. O Acid Bloater usa pra estourar.</summary>
        public event System.Action Morreu;

        /// <summary>Avisa que ele voltou, quando o Scrap morre. Serve pra quem tem estado pra zerar.</summary>
        public event System.Action Revivido;

        /// <summary>
        /// Se preenchido, a animacao de morte: roda (sem colisao, sem dano)
        /// antes de ele sumir. Sem ela, ele some no ato. E como o rastejante
        /// do District 1 se desmonta.
        /// </summary>
        [UnityEngine.Serialization.FormerlySerializedAs("DeathAnimation")] public System.Func<IEnumerator> AnimacaoMorte;

        /// <summary>O som da morte dele. Cada inimigo escolhe o seu; sem escolha, o de metal desmontando.</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("DeathSound")] public SfxCue SomMorte;

        /// <summary>Quando ele aparou um golpe pela ultima vez. O cinzel usa pra tinir em vez de acertar.</summary>
        public float UltimoTempoBloqueio { get; private set; } = -100f;

        public bool EstaMorto => _morto;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(int novoVida, int novoFragmentos, Sprite novoSpriteFragmento)
        {
            vida = novoVida;
            fragmentos = novoFragmentos;
            spriteFragmento = novoSpriteFragmento;
        }

        private void Awake()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();
            if (_sprite != null) _corBase = _sprite.color;
            _colisores = GetComponents<Collider2D>();

            _vidaCheio = vida;
            _origem = transform.position;

            // Inscrito aqui, e nao no OnEnable, porque desligado ele tambem
            // precisa ouvir: e assim que volta.
            ScrapHealth.Morreu += Reviver;
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= Reviver;
        }

        private void Reviver()
        {
            StopAllCoroutines();
            DefinirColisores(true);

            _morto = false;
            vida = _vidaCheio;
            transform.position = _origem;
            if (_sprite != null) _sprite.color = _corBase;

            gameObject.SetActive(true);
            Revivido?.Invoke();
        }

        /// <summary>
        /// Traz de volta um que foi reaproveitado (os bichos que brotam do chao
        /// na luta do Capataz e da broca): vida cheia, vivo, colisao ligada. Sem isso ele
        /// voltava marcado como morto e nenhum golpe pegava.
        /// </summary>
        public void Restaurar()
        {
            StopAllCoroutines();
            DefinirColisores(true);

            _morto = false;
            vida = _vidaCheio;
            if (_sprite != null) _sprite.color = _corBase;

            Revivido?.Invoke();
        }

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (_morto) return;

            // Aparado: faz barulho de metal, mas nao tira nada.
            if (Bloqueios != null && Bloqueios(de))
            {
                UltimoTempoBloqueio = Time.time;

                if (_sprite != null && gameObject.activeInHierarchy)
                {
                    StopAllCoroutines();
                    StartCoroutine(Tinir());
                }

                return;
            }

            vida -= quantia;
            UltimoTempoGolpe = Time.time;
            UltimoLadoGolpe = de.x <= transform.position.x ? -1 : 1;

            if (vida <= 0)
            {
                _morto = true;

                // O Cinzel de colheita, da loja, arranca mais.
                int derrubado = Mathf.CeilToInt(fragmentos * Shop.EscalaQuedaFragmento);
                for (int i = 0; i < derrubado; i++)
                {
                    ShardPickup.Criar(transform.position, spriteFragmento, 1, i);
                }

                (SomMorte ?? Sfx.MetalMorte).Tocar(transform.position);
                Morreu?.Invoke();

                if (AnimacaoMorte != null && gameObject.activeInHierarchy)
                {
                    StopAllCoroutines();
                    if (_sprite != null) _sprite.color = _corBase;
                    DefinirColisores(false);
                    StartCoroutine(Sumir());
                    return;
                }

                gameObject.SetActive(false);
                return;
            }

            if (_sprite != null)
            {
                StopAllCoroutines();
                StartCoroutine(Piscada());
            }
        }

        private IEnumerator Sumir()
        {
            yield return AnimacaoMorte();
            gameObject.SetActive(false);
        }

        private void DefinirColisores(bool habilitado)
        {
            if (_colisores == null) return;
            foreach (Collider2D parte in _colisores)
            {
                if (parte != null) parte.enabled = habilitado;
            }
        }

        private IEnumerator Tinir()
        {
            _sprite.color = new Color(0.75f, 0.85f, 1f, _corBase.a);
            yield return new WaitForSeconds(0.08f);
            _sprite.color = _corBase;
        }

        private IEnumerator Piscada()
        {
            _sprite.color = new Color(1f, 0.35f, 0.3f, _corBase.a);
            yield return new WaitForSeconds(0.12f);
            _sprite.color = _corBase;
        }
    }
}
