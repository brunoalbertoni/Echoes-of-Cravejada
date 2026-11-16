using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O raio do cetro do Rei de Prata: uma linha de luz que sai de um ponto e
    /// vai reto ate bater no terreno (as sacadas fazem sombra). Primeiro so o
    /// fio fino, que mira; depois o raio grosso, que queima quem cruzar.
    ///
    /// O dash atravessa (o Scrap nao apanha durante o dash).
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class KingBeam : MonoBehaviour
    {
        [SerializeField] private float maxLength = 70f;
        [SerializeField] private Color aimColor = new Color(1f, 0.97f, 0.80f, 0.35f);
        [SerializeField] private Color fireColor = new Color(1f, 0.96f, 0.70f, 1f);

        private LineRenderer _line;
        private ScrapHealth _health;
        private Transform _player;
        private bool _firing;
        private int _mask;

        // O fio de mira assobia e o raio aceso zumbe; a cena de cada golpe so diz Aim, Fire e Hide.
        private SfxLoop _aimSound;
        private SfxLoop _fireSound;
        private bool _wasFiring;
        private float _shownAt = -100f;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.positionCount = 0;
            _mask = LayerMask.GetMask("Ground");

            _aimSound = SfxLoop.Attach(gameObject, Sfx.KingBeamAim);
            _fireSound = SfxLoop.Attach(gameObject, Sfx.KingBeamLoop);
            if (_aimSound != null) _aimSound.Fade = 8f;
            if (_fireSound != null) _fireSound.Fade = 8f;
        }

        private void Update()
        {
            // Quem mira chama Aim/Fire todo quadro: parou de chamar, o raio apagou.
            bool shown = Time.time - _shownAt < 0.1f;
            if (_aimSound != null) _aimSound.Level = shown && !_firing ? 1f : 0f;
            if (_fireSound != null) _fireSound.Level = shown && _firing ? 1f : 0f;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null)
            {
                _player = controller.transform;
                _health = controller.GetComponent<ScrapHealth>();
            }
        }

        /// <summary>Mostra o fio fino que mira.</summary>
        public void Aim(Vector2 origin, float angle) => Show(origin, angle, false);

        /// <summary>O raio grosso, queimando.</summary>
        public void Fire(Vector2 origin, float angle) => Show(origin, angle, true);

        public void Hide()
        {
            _firing = false;
            _wasFiring = false;
            _shownAt = -100f;
            if (_line != null) _line.positionCount = 0;
        }

        private void Show(Vector2 origin, float angle, bool firing)
        {
            _firing = firing;
            _shownAt = Time.time;

            // O som vem da ponta do cetro (a linha e em coordenadas de mundo: mover o objeto nao a mexe).
            transform.position = origin;
            if (firing && !_wasFiring) Sfx.KingBeamFire.Play(origin);
            _wasFiring = firing;

            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 end = origin + direction * maxLength;

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxLength, _mask);
            float best = maxLength;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                if (SilverKing.Passable(hit.point)) continue;
                if (hit.distance < best) best = hit.distance;
            }

            end = origin + direction * best;

            float width = firing ? 0.55f + Mathf.Sin(Time.time * 30f) * 0.06f : 0.06f;
            _line.positionCount = 2;
            _line.SetPosition(0, origin);
            _line.SetPosition(1, end);
            _line.startWidth = width;
            _line.endWidth = width * 0.8f;
            _line.startColor = firing ? fireColor : aimColor;
            _line.endColor = firing ? fireColor : aimColor;

            if (!firing || _player == null || _health == null) return;

            // Queima quem estiver perto do segmento (o meio do corpo do Scrap).
            Vector2 body = (Vector2)_player.position + Vector2.up * 0.6f;
            Vector2 segment = end - origin;
            float t = Mathf.Clamp01(Vector2.Dot(body - origin, segment) / Mathf.Max(0.01f, segment.sqrMagnitude));
            float distance = Vector2.Distance(body, origin + segment * t);
            if (distance < 0.6f) _health.TakeDamage(1, origin);
        }
    }
}
