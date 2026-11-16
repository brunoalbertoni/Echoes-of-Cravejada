using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um espelho de prata num eixo, inclinado a 45 graus. Um raio de luz que
    /// bate nele dobra a esquina. Um golpe de cinzel o faz girar pro outro
    /// lado, de "/" pra "\" e de volta, numa meia volta rapida no eixo.
    ///
    /// O espelho tem dois colisores: a area grande onde o Scrap o alcanca (a
    /// deste objeto) e a face pequena onde o raio bate (um filho).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SilverMirror : MonoBehaviour, IDamageable
    {
        [Tooltip("Verdadeiro se ele esta em \"/\" (sobe pra direita); falso se em \"\\\".")]
        [SerializeField] private bool slash = true;

        [Tooltip("A face onde o raio bate: um colisor pequeno, num filho.")]
        [SerializeField] private Collider2D face;

        [SerializeField] private SpriteRenderer sprite;

        private float _turn = -1f;
        private Vector3 _spriteScale = Vector3.one;

        public Collider2D Face => face;

        /// <summary>O ponto de onde o raio sai depois de refletir.</summary>
        public Vector2 Center => face != null ? (Vector2)face.bounds.center : (Vector2)transform.position;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(bool startsSlash, Collider2D newFace, SpriteRenderer newSprite)
        {
            slash = startsSlash;
            face = newFace;
            sprite = newSprite;
        }

        private void Start()
        {
            if (sprite != null) _spriteScale = sprite.transform.localScale;
            Show();
        }

        /// <summary>Pra onde vai um raio que chega nesta direcao.</summary>
        public Vector2 Reflect(Vector2 direction)
        {
            // "/" troca x com y; "\" troca e inverte.
            return slash
                ? new Vector2(direction.y, direction.x)
                : new Vector2(-direction.y, -direction.x);
        }

        private void Show()
        {
            // A arte e o espelho em "/".
            if (sprite != null) sprite.flipX = !slash;
        }

        public void Damage(int amount, Vector2 from)
        {
            // Ainda girando: o golpe so sacode.
            if (_turn >= 0f) return;

            _turn = 0f;
            NoiseSystem.Emit(transform.position, 5f, 0.2f);
            Sfx.MirrorTurn.Play(transform.position);
        }

        private void Update()
        {
            if (_turn < 0f || sprite == null)
            {
                if (_turn >= 0f) { slash = !slash; Show(); _turn = -1f; }
                return;
            }

            // Meia volta no eixo: a face fecha, troca de lado e abre de novo,
            // com um balanco no fim.
            const float time = 0.3f;
            float before = _turn;
            _turn += Time.deltaTime;
            if (before < time * 0.5f && _turn >= time * 0.5f)
            {
                slash = !slash;
                Show();
            }

            float k = Mathf.Clamp01(_turn / time);
            float width = Mathf.Abs(Mathf.Cos(k * Mathf.PI));
            float wobble = k >= 1f ? 0f : Mathf.Sin(k * Mathf.PI * 3f) * 0.06f * (1f - k);
            sprite.transform.localScale = new Vector3(_spriteScale.x * Mathf.Max(0.08f, width), _spriteScale.y * (1f + wobble), _spriteScale.z);

            if (_turn < time + 0.05f) return;

            sprite.transform.localScale = _spriteScale;
            _turn = -1f;
        }
    }
}
