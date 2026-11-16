using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A alavanca emperrada: a ferrugem travou ela no lugar. Cada golpe de
    /// cinzel a sacode e solta um pouco; no ultimo, ela desce e o portao abre.
    ///
    /// E o primeiro "use o cinzel no mundo" do jogo: o mesmo golpe que derruba
    /// corrompido tambem destrava coisa.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RustyLever : MonoBehaviour, IDamageable
    {
        [Tooltip("Quantos golpes ate soltar.")]
        [SerializeField] private int hitsToFree = 3;

        [SerializeField] private Gate gate;
        [SerializeField] private SpriteRenderer leverSprite;
        [SerializeField] private Sprite cleanSprite;

        private int _hits;
        private bool _done;
        private float _shake;
        private Vector3 _spriteHome;
        private ScrapController _near;

        /// <summary>A alavanca perto da qual o Scrap esta, se ainda emperrada. O HUD usa pra mostrar o aviso.</summary>
        public static RustyLever Nearby { get; private set; }

        /// <summary>Quanto ja soltou, de 0 a 1.</summary>
        public float Progress => Mathf.Clamp01(_hits / (float)Mathf.Max(1, hitsToFree));

        public bool IsDone => _done;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Nearby = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Gate newGate, SpriteRenderer newLeverSprite, Sprite newCleanSprite)
        {
            gate = newGate;
            leverSprite = newLeverSprite;
            cleanSprite = newCleanSprite;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Awake()
        {
            _spriteHome = transform.localPosition;
        }

        public void Damage(int amount, Vector2 from)
        {
            if (_done) return;

            _hits++;
            _shake = 0.25f;

            if (_hits < hitsToFree) return;

            _done = true;
            if (leverSprite != null && cleanSprite != null) leverSprite.sprite = cleanSprite;
            if (gate != null) gate.Open();
            Sfx.ScrapeDone.Play();
            if (Nearby == this) Nearby = null;
        }

        private void Update()
        {
            if (_shake > 0f)
            {
                _shake -= Time.deltaTime;
                float offset = Mathf.Sin(Time.time * 90f) * 0.06f * Mathf.Clamp01(_shake / 0.25f);
                transform.localPosition = _spriteHome + new Vector3(offset, 0f, 0f);
            }
            else if (transform.localPosition != _spriteHome)
            {
                transform.localPosition = _spriteHome;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            _near = controller;
            if (!_done) Nearby = this;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_near == null || other.GetComponentInParent<ScrapController>() != _near) return;

            _near = null;
            if (Nearby == this) Nearby = null;
        }
    }
}
