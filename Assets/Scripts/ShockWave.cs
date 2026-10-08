using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A onda de choque da segunda fase: corre rente ao chao pra um lado ate
    /// sair da arena. E baixa de proposito — pula-se por cima.
    /// </summary>
    public class ShockWave : MonoBehaviour
    {
        private float _speed;
        private float _minX;
        private float _maxX;

        public static void Launch(Vector3 from, float speed, float minX, float maxX, Sprite sprite)
        {
            var wave = new GameObject("ShockWave");
            wave.transform.position = from;

            var renderer = wave.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.flipX = speed < 0f;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 2;

            var trigger = wave.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.9f, 0.6f);
            trigger.offset = new Vector2(0f, 0.3f);

            var body = wave.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var script = wave.AddComponent<ShockWave>();
            script._speed = speed;
            script._minX = minX;
            script._maxX = maxX;
        }

        private void Update()
        {
            transform.position += Vector3.right * (_speed * Time.deltaTime);

            float x = transform.position.x;
            if (x < _minX || x > _maxX) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
