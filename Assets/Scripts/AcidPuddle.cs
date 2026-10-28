using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma poca rasa de acido que some sozinha: o que o Acid Bloater deixa ao
    /// estourar. Machuca quem pisar, mas nao joga de volta — e rasa demais pra
    /// afundar.
    /// </summary>
    public class AcidPuddle : MonoBehaviour
    {
        private float _left;
        private float _total;
        private SpriteRenderer _sprite;

        /// <summary>
        /// Derrama uma poca centrada em ground. O sprite da superficie tem o
        /// pivo no canto de baixo, a esquerda: por isso o objeto vai meia
        /// largura pra esquerda, e o gatilho acompanha o desenho — machuca so
        /// onde se ve acido.
        /// </summary>
        public static void Spill(Vector3 ground, float width, float time, Sprite sprite)
        {
            var puddle = new GameObject("AcidPuddle");
            puddle.transform.position = ground - new Vector3(width * 0.5f, 0f, 0f);

            var renderer = puddle.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(width, 0.4f);
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = -5;

            var trigger = puddle.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(width - 0.4f, 0.35f);
            trigger.offset = new Vector2(width * 0.5f, 0.18f);

            var body = puddle.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var script = puddle.AddComponent<AcidPuddle>();
            script._left = time;
            script._total = time;
            script._sprite = renderer;
        }

        private void Update()
        {
            _left -= Time.deltaTime;

            // Vai sumindo no ultimo segundo.
            if (_sprite != null)
            {
                Color color = _sprite.color;
                color.a = Mathf.Clamp01(_left);
                _sprite.color = color;
            }

            if (_left <= 0f) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Burn(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Burn(other);
        }

        private void Burn(Collider2D other)
        {
            // No finalzinho ja esta seca demais pra queimar.
            if (_left < _total * 0.1f) return;

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
