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
        [SerializeField] private int maxHealth = 5;

        [Tooltip("Segundos sem poder apanhar de novo depois de um golpe.")]
        [SerializeField] private float invulnerabilityTime = 1.0f;

        [Tooltip("Empurrao ao apanhar: pra tras e um pouco pra cima.")]
        [SerializeField] private Vector2 knockback = new Vector2(6f, 7f);

        [Tooltip("O sprite da old scrap deixada no lugar da morte.")]
        [SerializeField] private Sprite oldScrapSprite;

        private ScrapController _controller;
        private ScrapRespawn _respawn;
        private Rigidbody2D _body;
        private SpriteRenderer _sprite;

        private int _health;
        private float _invulnerableUntil;
        private Vector3 _startPosition;

        // A bancada vale pra cena em que ela esta. Trocar de cena esquece.
        private static bool _hasCheckpoint;
        private static Vector3 _checkpoint;
        private static OldScrap _oldScrap;

        public int Health => _health;
        public int MaxHealth => maxHealth;

        /// <summary>
        /// Avisa que o Scrap morreu. Os inimigos e o boss escutam pra renascer
        /// e encher a vida: morrer devolve o mundo ao que era.
        /// </summary>
        public static event System.Action Died;

        /// <summary>Quando ele apanhou pela ultima vez. A alavanca usa pra saber se foi interrompida.</summary>
        public float LastHitTime { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _hasCheckpoint = false;
            _oldScrap = null;
            _bonusHealth = 0;
            Died = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite newOldScrapSprite)
        {
            oldScrapSprite = newOldScrapSprite;
        }

        private void Awake()
        {
            _controller = GetComponent<ScrapController>();
            _respawn = GetComponent<ScrapRespawn>();
            _body = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();

            maxHealth += _bonusHealth;
            _health = maxHealth;
            _startPosition = transform.position;

            // Cena nova, bancada nova.
            _hasCheckpoint = false;
        }

        // O chassi reforcado vale pra sessao toda, em qualquer cena.
        private static int _bonusHealth;

        /// <summary>Crisol reforca o chassi: um ponto de vida a mais, pra sempre.</summary>
        public void UpgradeChassis()
        {
            _bonusHealth++;
            maxHealth++;
            _health = maxHealth;
        }

        /// <summary>A Workbench chama isto: salva o lugar e enche a vida.</summary>
        public void RestAt(Vector3 position)
        {
            _hasCheckpoint = true;
            _checkpoint = position;
            _health = maxHealth;
        }

        /// <summary>
        /// Tira vida. Com <paramref name="returnToSafeGround"/> ele tambem e
        /// jogado de volta pro ultimo chao seguro — e o caso dos espinhos e do
        /// acido, de onde nao da pra sair andando.
        /// </summary>
        public void TakeDamage(int amount, Vector2 from, bool returnToSafeGround = false)
        {
            if (Time.time < _invulnerableUntil || _health <= 0) return;

            // Durante uma cutscene ninguem apanha: o que ainda estava voando
            // quando ela comecou (sucata, onda de choque) passa reto.
            if (Cinematic.Active) return;

            _health -= amount;
            LastHitTime = Time.time;
            _invulnerableUntil = Time.time + invulnerabilityTime;

            if (_health <= 0)
            {
                Die();
                return;
            }

            if (returnToSafeGround && _respawn != null)
            {
                _respawn.Respawn(true);
            }
            else
            {
                float away = transform.position.x >= from.x ? 1f : -1f;
                _controller.Knockback(new Vector2(away * knockback.x, knockback.y), 0.25f);
            }

            StopAllCoroutines();
            StartCoroutine(Blink());
        }

        private void Die()
        {
            Vector3 deathPlace = _respawn != null ? (Vector3)_respawn.SafePosition : transform.position;

            // O que ja estava no chao de uma morte anterior se perde.
            if (_oldScrap != null) Destroy(_oldScrap.gameObject);
            _oldScrap = null;

            int shards = ScrapWallet.TakeAll();

            // Com a Bolsa forrada, da loja, metade fica com ele.
            if (Shop.KeepsHalfOnDeath)
            {
                int kept = shards / 2;
                ScrapWallet.Add(kept);
                shards -= kept;
            }

            if (shards > 0) _oldScrap = OldScrap.Drop(deathPlace, shards, oldScrapSprite);

            Vector3 target = _hasCheckpoint ? _checkpoint : _startPosition;

            _body.position = target;
            transform.position = target;
            _body.linearVelocity = Vector2.zero;
            _controller.ControlLocked = false;

            if (_respawn != null) _respawn.SetSafePosition(target);

            _health = maxHealth;
            _invulnerableUntil = Time.time + invulnerabilityTime;

            Camera main = Camera.main;
            var follow = main != null ? main.GetComponent<ScrapCameraFollow>() : null;
            if (follow != null) follow.SnapToTarget();

            StopAllCoroutines();
            StartCoroutine(Blink());

            Died?.Invoke();
        }

        private IEnumerator Blink()
        {
            if (_sprite == null) yield break;

            while (Time.time < _invulnerableUntil)
            {
                _sprite.enabled = !_sprite.enabled;
                yield return new WaitForSeconds(0.07f);
            }

            _sprite.enabled = true;
        }
    }
}
