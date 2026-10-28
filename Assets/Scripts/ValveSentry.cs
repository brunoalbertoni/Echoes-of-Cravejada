using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Valve Sentry: uma torre corrompida, parada, que cospe jatos de vapor
    /// escaldante em linha reta, num ritmo fixo. Da pra contar o tempo e passar
    /// entre um jato e outro, ou pular por cima.
    ///
    /// O aviso e o desenho: antes de cada tiro o volante gira, o ponteiro do
    /// manometro sobe e o bocal esquenta; o tiro da coice. Ao morrer, o volante
    /// salta e ela tomba soltando vapor.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ValveSentry : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do SentryArt, no editor.
        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Charge = 4;
        public const int ChargeFrames = 3;
        public const int Fire = 7;
        public const int FireFrames = 3;
        public const int Hurt = 10;
        public const int Death = 11;
        public const int DeathFrames = 6;

        [Tooltip("+1 atira pra direita, -1 pra esquerda.")]
        [SerializeField] private int direction = 1;

        [Tooltip("Segundos entre um jato e o proximo.")]
        [SerializeField] private float interval = 2.2f;

        [Tooltip("Quanto antes do tiro ela comeca a carregar.")]
        [SerializeField] private float chargeTime = 0.55f;

        [Tooltip("Ate onde o jato vai, em tiles.")]
        [SerializeField] private float range = 16f;

        [SerializeField] private float boltSpeed = 8f;
        [SerializeField] private Sprite boltSprite;

        [Tooltip("A boca do bocal, em tiles, a partir do pe (olhando pra direita).")]
        [SerializeField] private Vector2 muzzle = new Vector2(0.66f, 1.06f);

        [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private float _timer;
        private float _firedAt = -100f;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(int newDirection, float newRange, Sprite newBoltSprite, Sprite[] newFrames = null, Vector2? newMuzzle = null)
        {
            direction = newDirection;
            range = newRange;
            boltSprite = newBoltSprite;
            frames = newFrames;
            if (newMuzzle.HasValue) muzzle = newMuzzle.Value;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            if (_damageable != null) _damageable.DeathAnimation = Collapse;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = direction < 0;

            // Cada torre comeca num ponto diferente do ciclo.
            _timer = Mathf.Repeat(transform.position.x * 0.37f, interval);
        }

        private void Update()
        {
            if (_damageable != null && _damageable.IsDead) return;

            _timer += Time.deltaTime;

            if (_timer >= interval)
            {
                _timer = 0f;
                _firedAt = Time.time;

                Vector3 mouth = transform.position + new Vector3(direction * muzzle.x, muzzle.y, 0f);
                SteamBolt.Fire(mouth, direction * boltSpeed, range, boltSprite);
            }

            PickFrame();
        }

        private void PickFrame()
        {
            if (_sprite == null || frames == null || frames.Length == 0) return;

            int frame;
            float sinceFire = Time.time - _firedAt;
            float toFire = interval - _timer;

            if (_damageable != null && Time.time - _damageable.LastHitTime < 0.15f) frame = Hurt;
            else if (sinceFire < 0.3f) frame = Fire + Mathf.Min(FireFrames - 1, (int)(sinceFire / 0.1f));
            else if (toFire < chargeTime) frame = Charge + Mathf.Min(ChargeFrames - 1, (int)((1f - toFire / chargeTime) * ChargeFrames));
            else frame = Idle + (int)(Time.time * 5f + transform.position.x) % IdleFrames;

            Show(frame);
        }

        private void Show(int frame)
        {
            if (frames != null && frame >= 0 && frame < frames.Length && frames[frame] != null) _sprite.sprite = frames[frame];
        }

        /// <summary>A morte: o volante salta, o vapor escapa, a cabeca tomba no chao.</summary>
        private IEnumerator Collapse()
        {
            float[] hold = { 0.12f, 0.12f, 0.14f, 0.14f, 0.25f, 0.45f };
            for (int i = 0; i < DeathFrames; i++)
            {
                if (_sprite != null) Show(Death + i);
                yield return new WaitForSeconds(hold[i]);
            }

            if (_sprite == null) yield break;
            Color start = _sprite.color;
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                _sprite.color = new Color(start.r, start.g, start.b, start.a * (1f - t / 0.35f));
                yield return null;
            }
        }
    }
}
