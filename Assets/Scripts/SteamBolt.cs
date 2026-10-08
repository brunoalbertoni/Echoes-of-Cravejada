using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um jato de vapor escaldante: voa reto ate o alcance acabar ou bater no
    /// Scrap. Atravessa parede de proposito — some pelo alcance, que quem
    /// atira mede ate a parede.
    /// </summary>
    public class SteamBolt : MonoBehaviour
    {
        private float _speed;
        private float _left;

        public static void Fire(Vector3 from, float speed, float range, Sprite sprite)
        {
            var bolt = new GameObject("SteamBolt");
            bolt.transform.position = from;

            var renderer = bolt.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.flipX = speed < 0f;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 3;

            var trigger = bolt.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.8f, 0.35f);

            var body = bolt.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var script = bolt.AddComponent<SteamBolt>();
            script._speed = speed;
            script._left = range;
        }

        private void Update()
        {
            float step = _speed * Time.deltaTime;
            transform.position += Vector3.right * step;

            _left -= Mathf.Abs(step);
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
