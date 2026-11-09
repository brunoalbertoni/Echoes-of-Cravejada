using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Shard Sentinel: um geodo vivo, plantado num pedestal de cristais. Nao
    /// sai do lugar. Quando ouve barulho por perto, a tampa de pedra abre, o
    /// miolo de ametista acende e ele lanca um leque de estilhacos na direcao
    /// do som; depois fecha. Quebra com alguns golpes.
    ///
    /// Como atira no som, e nao no Scrap, da pra faze-lo gastar a rajada num
    /// cristal ressonante e passar enquanto ele recarrega.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class ShardSentinel : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do SentinelArt, no editor.
        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Open = 4;
        public const int OpenFrames = 3;
        public const int Fire = 7;
        public const int FireFrames = 2;
        public const int Close = 9;
        public const int CloseFrames = 2;
        public const int Hurt = 11;
        public const int Death = 12;
        public const int DeathFrames = 6;

        [Tooltip("Ate onde ela escuta, em tiles.")]
        [SerializeField] private float hearingRange = 9f;

        [Tooltip("Segundos entre ouvir e atirar: o tempo de sair da frente.")]
        [SerializeField] private float windup = 0.45f;

        [Tooltip("Segundos ate poder atirar de novo.")]
        [SerializeField] private float cooldown = 2.2f;

        [SerializeField] private float shardSpeed = 11f;
        [SerializeField] private Sprite shardSprite;

        [Tooltip("A altura do nucleo, de onde saem os estilhacos, em tiles.")]
        [SerializeField] private float coreHeight = 1.06f;

        [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private float _readyAt;
        private bool _firing;
        private float _clock;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite newShardSprite, Sprite[] newFrames = null, float newCoreHeight = 1.06f)
        {
            shardSprite = newShardSprite;
            if (newFrames != null) frames = newFrames;
            coreHeight = newCoreHeight;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            _clock = Mathf.Abs(transform.position.x * 0.41f);

            _damageable.Revived += ResetState;
            _damageable.DeathAnimation = Crack;
            _damageable.DeathSound = Sfx.DeathSentinel;
        }

        private void OnDestroy()
        {
            if (_damageable != null) _damageable.Revived -= ResetState;
        }

        private void OnEnable()
        {
            NoiseSystem.Heard += OnNoise;
            _firing = false;
        }

        private void OnDisable()
        {
            NoiseSystem.Heard -= OnNoise;
        }

        private void ResetState()
        {
            StopAllCoroutines();
            _firing = false;
            _readyAt = 0f;
        }

        private Vector2 Muzzle => (Vector2)transform.position + Vector2.up * coreHeight;

        private void OnNoise(Vector2 origin, float radius, float loudness)
        {
            if (_damageable.IsDead || _firing || Time.time < _readyAt || loudness < 0.3f) return;

            if (Vector2.Distance(origin, Muzzle) > Mathf.Min(hearingRange, radius + 1f)) return;

            StartCoroutine(Shoot(origin + Vector2.up * 0.6f));
        }

        private void Update()
        {
            if (_damageable.IsDead || _firing) return;

            // Apanhando: a tampa estremece.
            if (Time.time - _damageable.LastHitTime < 0.15f)
            {
                Show(Hurt);
                return;
            }

            _clock += Time.deltaTime * 4f;
            Show(Idle + (int)_clock % IdleFrames);
        }

        private IEnumerator Shoot(Vector2 target)
        {
            _firing = true;
            Sfx.SentinelOpen.Play(transform.position);

            // O aviso: a tampa abre e o miolo acende.
            for (int i = 0; i < OpenFrames; i++)
            {
                Show(Open + i);
                yield return new WaitForSeconds(windup / OpenFrames);
                if (_damageable.IsDead) yield break;
            }

            Vector2 muzzle = Muzzle;
            Vector2 aim = (target - muzzle).normalized;
            if (aim.sqrMagnitude < 0.01f) aim = Vector2.right;

            // Tres estilhacos em leque.
            Show(Fire);
            Sfx.SentinelFire.Play(muzzle);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 direction = Quaternion.Euler(0f, 0f, i * 14f) * aim;
                ShardShot.Fire(muzzle, direction * shardSpeed, 14f, shardSprite);
            }

            // O proprio tiro faz barulho: acende a sala em volta dele.
            NoiseSystem.Emit(muzzle, 6f, 0.2f);
            yield return new WaitForSeconds(0.08f);
            if (_damageable.IsDead) yield break;
            Show(Fire + 1);
            yield return new WaitForSeconds(0.12f);

            for (int i = 0; i < CloseFrames; i++)
            {
                if (_damageable.IsDead) yield break;
                Show(Close + i);
                yield return new WaitForSeconds(0.1f);
            }

            _readyAt = Time.time + cooldown;
            _firing = false;
        }

        private void Show(int frame)
        {
            if (_sprite == null || frames == null || frame < 0 || frame >= frames.Length || frames[frame] == null) return;
            _sprite.sprite = frames[frame];
        }

        /// <summary>A morte: as metades racham e caem pros lados, o miolo estoura em po.</summary>
        private IEnumerator Crack()
        {
            _firing = true;
            if (_sprite != null) _sprite.color = Color.white;

            for (int i = 0; i < DeathFrames; i++)
            {
                Show(Death + i);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
