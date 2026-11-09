using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma gota de mercurio que pinga da nuvem do Alquimista quando ele
    /// destila. Cai reto ate o chao e, se pegar o Scrap no caminho, machuca.
    /// Diferente da goteira do teto (MercuryDrop), que so deixa lento.
    /// </summary>
    public class MercuryBead : MonoBehaviour
    {
        private const float Gravity = 26f;

        private Vector2 _velocity;
        private float _floorY;

        public static void Fall(Vector3 from, float floorY, Sprite sprite)
        {
            Spawn(from, new Vector2(0f, -2f), floorY, sprite);
        }

        /// <summary>Um respingo: sai com velocidade e cai em arco.</summary>
        public static void Splash(Vector3 from, Vector2 velocity, float floorY, Sprite sprite)
        {
            Spawn(from, velocity, floorY, sprite);
        }

        private static void Spawn(Vector3 from, Vector2 velocity, float floorY, Sprite sprite)
        {
            var bead = new GameObject("MercuryBead");
            bead.transform.position = from;

            var renderer = bead.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 4;

            var trigger = bead.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.2f;

            var body = bead.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var script = bead.AddComponent<MercuryBead>();
            script._floorY = floorY;
            script._velocity = velocity;

            // Morreu o Scrap no meio: as gotas no ar somem com a luta.
            Destroy(bead, 6f);
        }

        private void Update()
        {
            _velocity.y -= Gravity * Time.deltaTime;
            transform.position += (Vector3)(_velocity * Time.deltaTime);

            if (_velocity.y < 0f && transform.position.y <= _floorY) Destroy(gameObject);
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
