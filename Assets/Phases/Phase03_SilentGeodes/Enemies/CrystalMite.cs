using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Crystal Mite: um tatuzinho de pedra com geodo nas costas. No escuro e
    /// no silencio ele fica enrolado e e so mais uma pedra. Quando um som o
    /// alcanca, ele se desenrola e corre pra cima do Scrap aos pulinhos por
    /// alguns segundos; depois se enrola de novo. Fraco — um golpe — mas
    /// aparece em grupo, e estoura alto.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class CrystalMite : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do MiteArt, no editor.
        public const int Curl = 0;
        public const int CurlFrames = 2;
        public const int Unroll = 2;
        public const int UnrollFrames = 3;
        public const int Run = 5;
        public const int RunFrames = 6;
        public const int Death = 11;
        public const int DeathFrames = 5;

        private const float UnrollFps = 12f;

        [SerializeField] private float speed = 3.2f;

        [Tooltip("Segundos acordado depois do ultimo som.")]
        [SerializeField] private float awakeTime = 3.5f;

        [Tooltip("Ate onde se ouve o estalo dele estilhacando, em tiles.")]
        [SerializeField] private float deathNoiseRadius = 10f;

        [Tooltip("Limites de onde ele anda, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;

        private Transform _player;
        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Vector3 _home;
        private float _awakeUntil;
        private bool _wasAwake;
        private float _changedAt = -10f;
        private float _clock;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Sprite[] newFrames = null)
        {
            minX = newMinX;
            maxX = newMaxX;
            if (newFrames != null) frames = newFrames;
        }

        /// <summary>O mesmo Configure, devolvendo ele: o gerador encadeia.</summary>
        public CrystalMite ConfigureAndReturn(float newMinX, float newMaxX, Sprite[] newFrames = null)
        {
            Configure(newMinX, newMaxX, newFrames);
            return this;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            _home = transform.position;

            // Cada um respira num ponto do ciclo.
            _clock = Mathf.Abs(transform.position.x * 0.37f);

            _damageable.Revived += Sleep;
            _damageable.Died += Shatter;
            _damageable.DeathAnimation = Burst;
            _damageable.DeathSound = Sfx.DeathMite;
        }

        private void OnDestroy()
        {
            if (_damageable == null) return;
            _damageable.Revived -= Sleep;
            _damageable.Died -= Shatter;
        }

        /// <summary>
        /// Morrendo ele estilhaca — e o estalo do cristal e alto: acorda os
        /// outros mites e chama os cacadores da mesma altura.
        /// </summary>
        private void Shatter()
        {
            NoiseSystem.Emit(transform.position + Vector3.up * 0.3f, deathNoiseRadius, 0.7f);
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void OnEnable()
        {
            NoiseSystem.Heard += OnNoise;
        }

        private void OnDisable()
        {
            NoiseSystem.Heard -= OnNoise;
        }

        /// <summary>Muda a casa dele (quem nasce em outro chao precisa pular rente a esse chao).</summary>
        public void Rehome(Vector3 position)
        {
            _home = position;
            transform.position = position;
        }

        private void Sleep()
        {
            _awakeUntil = 0f;
            _wasAwake = false;
            _changedAt = -10f;
            transform.position = _home;
        }

        private void OnNoise(Vector2 origin, float radius, float loudness)
        {
            if (loudness < 0.3f) return;
            if (Vector2.Distance(origin, transform.position) > radius) return;

            _awakeUntil = Time.time + awakeTime;
        }

        private void Update()
        {
            if (_damageable.IsDead) return;

            bool awake = Time.time < _awakeUntil;
            if (awake != _wasAwake)
            {
                _wasAwake = awake;
                _changedAt = Time.time;
                if (awake) Sfx.MiteWake.Play(transform.position);
            }

            // Dormindo ele quase some; acordado, brilha.
            if (_sprite != null)
            {
                Color color = _sprite.color;
                color.a = Mathf.MoveTowards(color.a, awake ? 1f : 0.35f, 4f * Time.deltaTime);
                _sprite.color = color;
            }

            Animate(awake);

            if (!awake || _player == null) return;
            if (Time.time - _damageable.LastHitTime < 0.2f) return;

            // Enquanto se desenrola, ainda nao corre.
            if (Time.time - _changedAt < UnrollFrames / UnrollFps) return;

            Vector3 position = transform.position;
            float direction = Mathf.Sign(_player.position.x - position.x);

            position.x = Mathf.Clamp(position.x + direction * speed * Time.deltaTime, minX, maxX);

            // Um pulinho a cada passo: ele corre saltitando.
            position.y = _home.y + Mathf.Abs(Mathf.Sin(Time.time * 14f)) * 0.25f;
            transform.position = position;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = direction < 0f;
        }

        /// <summary>Enrolado, desenrolando, correndo — ou se enrolando de volta.</summary>
        private void Animate(bool awake)
        {
            float since = Time.time - _changedAt;
            int unroll = Mathf.Min(UnrollFrames - 1, (int)(since * UnrollFps));
            bool turning = since < UnrollFrames / UnrollFps;

            if (awake)
            {
                Show(turning ? Unroll + unroll : Run + (int)(Time.time * 12f) % RunFrames);
            }
            else
            {
                _clock += Time.deltaTime * 2f;
                Show(turning ? Unroll + UnrollFrames - 1 - unroll : Curl + (int)_clock % CurlFrames);
            }
        }

        private void Show(int frame)
        {
            if (_sprite == null || frames == null || frame < 0 || frame >= frames.Length || frames[frame] == null) return;
            _sprite.sprite = frames[frame];
        }

        /// <summary>A morte: racha, acende, estoura em cacos e po.</summary>
        private IEnumerator Burst()
        {
            if (_sprite != null) _sprite.color = Color.white;

            for (int i = 0; i < DeathFrames; i++)
            {
                Show(Death + i);
                yield return new WaitForSeconds(1f / 12f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Touch(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Touch(other);
        }

        private void Touch(Collider2D other)
        {
            // Dormindo ele e so pedra: nao machuca.
            if (_damageable.IsDead || Time.time >= _awakeUntil) return;

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
