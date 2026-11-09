using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma faceta de luz da coroa do Rei de Prata. Voa reto e ricocheteia no
    /// terreno (parede, chao, teto) algumas vezes antes de se apagar. Na chuva
    /// da fase 2 ela so cai, sem ricochete. Machuca quem tocar.
    /// </summary>
    public class FacetShard : MonoBehaviour
    {
        private Vector2 _velocity;
        private int _bounces;
        private int _mask;
        private float _until;

        public static void Launch(Vector2 from, Vector2 velocity, int bounces, Sprite sprite, float life = 5f)
        {
            var shard = new GameObject("FacetShard");
            shard.transform.position = from;

            var renderer = shard.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 8;

            var trigger = shard.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.25f;

            var body = shard.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var script = shard.AddComponent<FacetShard>();
            script._velocity = velocity;
            script._bounces = bounces;
            script._mask = LayerMask.GetMask("Ground");
            script._until = Time.time + life;

            ScrapHealth.Died += script.Vanish;
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= Vanish;
        }

        private void Vanish()
        {
            if (this != null) Destroy(gameObject);
        }

        private void Update()
        {
            if (Time.time > _until)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 position = transform.position;
            Vector2 step = _velocity * Time.deltaTime;

            RaycastHit2D hit = Physics2D.Raycast(position, step.normalized, step.magnitude + 0.2f, _mask);
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                if (_bounces <= 0)
                {
                    Destroy(gameObject);
                    return;
                }

                _bounces--;
                _velocity = Vector2.Reflect(_velocity, hit.normal);
                transform.position = hit.point + hit.normal * 0.25f;
                return;
            }

            transform.position = position + step;
            transform.Rotate(0f, 0f, 540f * Time.deltaTime);
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
