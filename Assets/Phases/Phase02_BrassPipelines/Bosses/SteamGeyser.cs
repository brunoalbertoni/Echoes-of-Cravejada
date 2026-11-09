using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um geiser de vapor escaldante que brota do chao: primeiro um aviso fraco
    /// no lugar, depois a coluna de verdade, que machuca, e entao some.
    /// E o que o Boiler Master solta na segunda fase.
    /// </summary>
    public class SteamGeyser : MonoBehaviour
    {
        private const float WarnTime = 0.8f;
        private const float BurnTime = 1.0f;
        private const float Height = 7f;

        private SpriteRenderer _sprite;
        private BoxCollider2D _trigger;
        private float _age;

        public static void Erupt(Vector3 ground, Sprite sprite)
        {
            var geyser = new GameObject("SteamGeyser");
            geyser.transform.position = ground;

            var renderer = geyser.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(1.5f, Height);
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 4;

            var trigger = geyser.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.1f, Height);
            trigger.offset = new Vector2(0f, Height * 0.5f);
            trigger.enabled = false;

            var body = geyser.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var script = geyser.AddComponent<SteamGeyser>();
            script._sprite = renderer;
            script._trigger = trigger;
        }

        private void Update()
        {
            _age += Time.deltaTime;

            bool burning = _age >= WarnTime;
            _trigger.enabled = burning;

            // No aviso ele pisca fraco; depois fica cheio.
            float alpha = burning ? 0.9f : (Mathf.FloorToInt(_age * 14f) % 2 == 0 ? 0.35f : 0.15f);
            _sprite.color = new Color(1f, 1f, 1f, alpha);

            if (_age >= WarnTime + BurnTime) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Scald(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Scald(other);
        }

        private void Scald(Collider2D other)
        {
            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
