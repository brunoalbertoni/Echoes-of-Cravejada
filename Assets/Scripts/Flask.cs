using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um frasco arremessado em arco. Onde cai, estoura e deixa uma poca de
    /// Oxide. Acertar o Scrap em cheio tambem machuca.
    /// </summary>
    public class Flask : MonoBehaviour
    {
        private const float Gravity = 22f;
        private const float FlightTime = 0.9f;

        private Vector2 _velocity;
        private float _groundY;
        private float _age;
        private Sprite _puddleSprite;

        public static void Throw(Vector2 from, Vector2 target, Sprite sprite, Sprite puddleSprite)
        {
            var flask = new GameObject("Flask");
            flask.transform.position = from;

            var renderer = flask.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 3;

            var trigger = flask.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.25f;

            var body = flask.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            // Cai onde o Scrap esta agora, depois de um tempo fixo de voo.
            Vector2 delta = target - from;

            var script = flask.AddComponent<Flask>();
            script._velocity = new Vector2(delta.x / FlightTime, delta.y / FlightTime + 0.5f * Gravity * FlightTime);
            script._groundY = target.y;
            script._puddleSprite = puddleSprite;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            _velocity.y -= Gravity * Time.deltaTime;

            transform.position += (Vector3)(_velocity * Time.deltaTime);
            transform.Rotate(0f, 0f, 540f * Time.deltaTime);

            // So estoura na descida, ao chegar na altura do alvo.
            if (_velocity.y < 0f && transform.position.y <= _groundY) Burst();
            else if (_age > 3f) Destroy(gameObject);
        }

        private void Burst()
        {
            AcidPuddle.Spill(new Vector3(transform.position.x, _groundY, 0f), 2.6f, 3f, _puddleSprite);
            NoiseSystem.Emit(transform.position, 7f, 0.5f);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponentInParent<ScrapHealth>();
            if (health == null) return;

            health.TakeDamage(1, transform.position);
            Burst();
        }
    }
}
