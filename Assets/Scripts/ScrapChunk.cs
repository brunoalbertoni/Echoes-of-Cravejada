using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um pedaco de sucata arremessado em arco. Machuca o Scrap se encostar e
    /// some ao bater no chao.
    /// </summary>
    public class ScrapChunk : MonoBehaviour
    {
        private Vector2 _velocity;
        private float _gravity;
        private float _floorY;
        private float _age;

        public static void Throw(Vector3 from, Vector2 velocity, float gravity, float floorY, Sprite sprite)
        {
            var chunk = new GameObject("ScrapChunk");
            chunk.transform.position = from;

            var renderer = chunk.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 2;

            var trigger = chunk.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.3f;

            var body = chunk.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var script = chunk.AddComponent<ScrapChunk>();
            script._velocity = velocity;
            script._gravity = gravity;
            script._floorY = floorY;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            _velocity.y -= _gravity * Time.deltaTime;

            transform.position += (Vector3)(_velocity * Time.deltaTime);
            transform.Rotate(0f, 0f, 360f * Time.deltaTime);

            if (transform.position.y <= _floorY || _age > 5f) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponentInParent<ScrapHealth>();
            if (health == null) return;

            health.TakeDamage(1, transform.position);
            Destroy(gameObject);
        }
    }
}
