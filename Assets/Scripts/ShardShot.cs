using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>Um estilhaco de cristal: voa reto ate o alcance acabar ou acertar o Scrap.</summary>
    public class ShardShot : MonoBehaviour
    {
        private Vector2 _velocity;
        private float _left;

        public static void Fire(Vector2 from, Vector2 velocity, float range, Sprite sprite)
        {
            var shot = new GameObject("ShardShot");
            shot.transform.position = from;
            shot.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);

            var renderer = shot.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 3;

            var trigger = shot.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.22f;

            var body = shot.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var script = shot.AddComponent<ShardShot>();
            script._velocity = velocity;
            script._left = range;
        }

        private void Update()
        {
            Vector2 step = _velocity * Time.deltaTime;
            transform.position += (Vector3)step;

            _left -= step.magnitude;
            if (_left <= 0f) Destroy(gameObject);
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
