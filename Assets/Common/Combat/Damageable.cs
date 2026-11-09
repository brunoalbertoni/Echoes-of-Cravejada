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
        /// <summary>Leva um golpe vindo de <paramref name="from"/>.</summary>
        void Damage(int amount, Vector2 from);
    }

    /// <summary>
    /// Vida de inimigo: pisca ao apanhar e, ao morrer, solta Opaque Shards.
    ///
    /// Morrer nao destroi o inimigo, so o desliga: quando o Scrap morre, todos
    /// voltam pro lugar de origem com a vida cheia.
    /// </summary>
    public class Damageable : MonoBehaviour, IDamageable
    {
        [SerializeField] private int health = 2;

        [Tooltip("Quantos Opaque Shards solta ao morrer.")]
        [SerializeField] private int shards = 3;

        [SerializeField] private Sprite shardSprite;

        private SpriteRenderer _sprite;
        private Collider2D[] _colliders;
        private Color _baseColor = Color.white;
        private bool _dead;
        private int _fullHealth;
        private Vector3 _home;

        /// <summary>Quando apanhou pela ultima vez. Quem move o inimigo usa pra recuar.</summary>
        public float LastHitTime { get; private set; } = -100f;

        /// <summary>De que lado veio o ultimo golpe: -1 da esquerda, +1 da direita.</summary>
        public int LastHitSide { get; private set; }

        /// <summary>
        /// Se preenchido, decide se um golpe vindo de um ponto e aparado. E como
        /// o Brass Welder se defende de frente.
        /// </summary>
        public System.Func<Vector2, bool> Blocks;

        /// <summary>Avisa que ele morreu, antes de sumir. O Acid Bloater usa pra estourar.</summary>
        public event System.Action Died;

        /// <summary>Avisa que ele voltou, quando o Scrap morre. Serve pra quem tem estado pra zerar.</summary>
        public event System.Action Revived;

        /// <summary>
        /// Se preenchido, a animacao de morte: roda (sem colisao, sem dano)
        /// antes de ele sumir. Sem ela, ele some no ato. E como o rastejante
        /// do District 1 se desmonta.
        /// </summary>
        public System.Func<IEnumerator> DeathAnimation;

        /// <summary>O som da morte dele. Cada inimigo escolhe o seu; sem escolha, o de metal desmontando.</summary>
        public SfxCue DeathSound;

        /// <summary>Quando ele aparou um golpe pela ultima vez. O cinzel usa pra tinir em vez de acertar.</summary>
        public float LastBlockTime { get; private set; } = -100f;

        public bool IsDead => _dead;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(int newHealth, int newShards, Sprite newShardSprite)
        {
            health = newHealth;
            shards = newShards;
            shardSprite = newShardSprite;
        }

        private void Awake()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();
            if (_sprite != null) _baseColor = _sprite.color;
            _colliders = GetComponents<Collider2D>();

            _fullHealth = health;
            _home = transform.position;

            // Inscrito aqui, e nao no OnEnable, porque desligado ele tambem
            // precisa ouvir: e assim que volta.
            ScrapHealth.Died += Revive;
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= Revive;
        }

        private void Revive()
        {
            StopAllCoroutines();
            SetColliders(true);

            _dead = false;
            health = _fullHealth;
            transform.position = _home;
            if (_sprite != null) _sprite.color = _baseColor;

            gameObject.SetActive(true);
            Revived?.Invoke();
        }

        /// <summary>
        /// Traz de volta um que foi reaproveitado (os bichos que brotam do chao
        /// na luta do Capataz e da broca): vida cheia, vivo, colisao ligada. Sem isso ele
        /// voltava marcado como morto e nenhum golpe pegava.
        /// </summary>
        public void Restore()
        {
            StopAllCoroutines();
            SetColliders(true);

            _dead = false;
            health = _fullHealth;
            if (_sprite != null) _sprite.color = _baseColor;

            Revived?.Invoke();
        }

        public void Damage(int amount, Vector2 from)
        {
            if (_dead) return;

            // Aparado: faz barulho de metal, mas nao tira nada.
            if (Blocks != null && Blocks(from))
            {
                LastBlockTime = Time.time;

                if (_sprite != null && gameObject.activeInHierarchy)
                {
                    StopAllCoroutines();
                    StartCoroutine(Clang());
                }

                return;
            }

            health -= amount;
            LastHitTime = Time.time;
            LastHitSide = from.x <= transform.position.x ? -1 : 1;

            if (health <= 0)
            {
                _dead = true;

                // O Cinzel de colheita, da loja, arranca mais.
                int dropped = Mathf.CeilToInt(shards * Shop.ShardDropScale);
                for (int i = 0; i < dropped; i++)
                {
                    ShardPickup.Spawn(transform.position, shardSprite, 1, i);
                }

                (DeathSound ?? Sfx.DeathMetal).Play(transform.position);
                Died?.Invoke();

                if (DeathAnimation != null && gameObject.activeInHierarchy)
                {
                    StopAllCoroutines();
                    if (_sprite != null) _sprite.color = _baseColor;
                    SetColliders(false);
                    StartCoroutine(Vanish());
                    return;
                }

                gameObject.SetActive(false);
                return;
            }

            if (_sprite != null)
            {
                StopAllCoroutines();
                StartCoroutine(Flash());
            }
        }

        private IEnumerator Vanish()
        {
            yield return DeathAnimation();
            gameObject.SetActive(false);
        }

        private void SetColliders(bool enabled)
        {
            if (_colliders == null) return;
            foreach (Collider2D part in _colliders)
            {
                if (part != null) part.enabled = enabled;
            }
        }

        private IEnumerator Clang()
        {
            _sprite.color = new Color(0.75f, 0.85f, 1f, _baseColor.a);
            yield return new WaitForSeconds(0.08f);
            _sprite.color = _baseColor;
        }

        private IEnumerator Flash()
        {
            _sprite.color = new Color(1f, 0.35f, 0.3f, _baseColor.a);
            yield return new WaitForSeconds(0.12f);
            _sprite.color = _baseColor;
        }
    }
}
