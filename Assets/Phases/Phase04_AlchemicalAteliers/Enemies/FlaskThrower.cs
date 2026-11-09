using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Flask Thrower: um assistente de laboratorio de cobre, corrompido. Nao
    /// chega perto: fica onde esta e arremessa frascos em arco, que estouram
    /// no chao e deixam uma poca de Oxide por alguns segundos. Fragil — o
    /// problema e chegar nele.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class FlaskThrower : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do ThrowerArt, no editor.
        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Windup = 4;
        public const int WindupFrames = 3;
        public const int Throw = 7;
        public const int ThrowFrames = 2;
        public const int Hurt = 9;
        public const int Death = 10;
        public const int DeathFrames = 6;

        [Tooltip("Ate onde ele enxerga o Scrap, em tiles.")]
        [SerializeField] private float range = 11f;

        [SerializeField] private float cooldown = 2.6f;

        [Tooltip("Segundos de aviso: ele puxa o frasco e joga o braco pra tras.")]
        [SerializeField] private float windup = 0.45f;

        [Tooltip("De onde o frasco sai, em tiles acima dos pes.")]
        [SerializeField] private float handHeight = 1.3f;

        [SerializeField] private Sprite flaskSprite;
        [SerializeField] private Sprite puddleSprite;

        [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Transform _player;
        private float _readyAt;
        private bool _throwing;
        private float _clock;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite newFlaskSprite, Sprite newPuddleSprite, Sprite[] newFrames = null, float newHandHeight = 1.3f)
        {
            flaskSprite = newFlaskSprite;
            puddleSprite = newPuddleSprite;
            if (newFrames != null) frames = newFrames;
            handHeight = newHandHeight;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            _clock = Mathf.Abs(transform.position.x * 0.23f);

            _damageable.DeathAnimation = Collapse;
            _damageable.Revived += ResetState;
        }

        private void OnDestroy()
        {
            if (_damageable != null) _damageable.Revived -= ResetState;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void OnEnable()
        {
            _throwing = false;
            _readyAt = Time.time + 1f;
        }

        private void ResetState()
        {
            StopAllCoroutines();
            _throwing = false;
            _readyAt = Time.time + 1f;
        }

        private void Update()
        {
            if (_damageable.IsDead || _throwing) return;

            if (Time.time - _damageable.LastHitTime < 0.2f)
            {
                Show(Hurt);
                return;
            }

            // Parado, mexendo na bolsa.
            _clock += Time.deltaTime * 4f;
            Show(Idle + (int)_clock % IdleFrames);

            if (_player == null || Time.time < _readyAt) return;

            Vector2 offset = _player.position - transform.position;
            if (Mathf.Abs(offset.x) > range || Mathf.Abs(offset.y) > 6f) return;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = offset.x < 0f;

            StartCoroutine(ThrowFlask());
        }

        private IEnumerator ThrowFlask()
        {
            _throwing = true;
            Sfx.ThrowerWindup.Play(transform.position);

            // O aviso: puxa o frasco da bandoleira e joga o braco pra tras.
            for (int i = 0; i < WindupFrames; i++)
            {
                Show(Windup + i);
                yield return new WaitForSeconds(windup / WindupFrames);
                if (_damageable.IsDead) yield break;
            }

            Show(Throw);
            if (_player != null)
            {
                Vector2 hand = (Vector2)transform.position + Vector2.up * handHeight;
                Flask.Throw(hand, _player.position, flaskSprite, puddleSprite);
                Sfx.ThrowerThrow.Play(hand);
            }

            yield return new WaitForSeconds(0.1f);
            if (_damageable.IsDead) yield break;
            Show(Throw + 1);
            yield return new WaitForSeconds(0.25f);

            _readyAt = Time.time + cooldown;
            _throwing = false;
        }

        private void Show(int frame)
        {
            if (_sprite == null || frames == null || frame < 0 || frame >= frames.Length || frames[frame] == null) return;
            _sprite.sprite = frames[frame];
        }

        /// <summary>A morte: cambaleia, cai de joelhos, os frascos estouram e ele tomba.</summary>
        private IEnumerator Collapse()
        {
            _throwing = true;
            if (_sprite != null) _sprite.color = Color.white;

            // Os frascos da bandoleira estouram quando ele cai de joelhos.
            Sfx.FlaskBreak.Play(transform.position, volume: 0.7f, delay: 0.3f);

            float[] hold = { 0.12f, 0.15f, 0.18f, 0.18f, 0.2f, 0.25f };
            for (int i = 0; i < DeathFrames; i++)
            {
                Show(Death + i);
                yield return new WaitForSeconds(hold[i]);
            }
        }
    }
}
