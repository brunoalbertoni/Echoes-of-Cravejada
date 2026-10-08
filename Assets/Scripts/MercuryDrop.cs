using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>Uma gota de mercurio caindo. Quem solta e a MercuryDrip.</summary>
    public class MercuryDrop : MonoBehaviour
    {
        private float _speed;
        private float _floorY;
        private float _slowTime;

        public static void Fall(Vector3 from, float distance, float slowTime, Sprite sprite)
        {
            var drop = new GameObject("MercuryDrop");
            drop.transform.position = from;

            var renderer = drop.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.80f, 1f, 0.85f);
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 3;

            var trigger = drop.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.22f;

            var body = drop.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var script = drop.AddComponent<MercuryDrop>();
            script._floorY = from.y - distance;
            script._slowTime = slowTime;
        }

        private void Update()
        {
            // Engorda um instante no teto e depois despenca.
            _speed += 22f * Time.deltaTime;
            transform.position += Vector3.down * (_speed * Time.deltaTime);

            if (transform.position.y <= _floorY) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            controller.Slow(_slowTime);
            Destroy(gameObject);
        }
    }
}
