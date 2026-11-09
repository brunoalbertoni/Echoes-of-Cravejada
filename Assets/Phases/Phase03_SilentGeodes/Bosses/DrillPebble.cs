using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma lasca de pedra que pula do chao onde a Geode Drill vai sair por
    /// baixo. E so o aviso: nao machuca. Sobe, cai e some.
    /// </summary>
    public class DrillPebble : MonoBehaviour
    {
        private const float Gravity = 30f;

        private Vector2 _velocity;
        private float _until;
        private SpriteRenderer _sprite;

        public static void Pop(Vector2 from, Sprite sprite)
        {
            var pebble = new GameObject("DrillPebble");
            pebble.transform.position = from;
            pebble.transform.localScale = Vector3.one * 0.55f;
            pebble.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            var renderer = pebble.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 6;
            renderer.color = new Color(0.75f, 0.72f, 0.80f);

            var script = pebble.AddComponent<DrillPebble>();
            script._sprite = renderer;
            script._velocity = new Vector2(Random.Range(-1.6f, 1.6f), Random.Range(5f, 7.5f));
            script._until = Time.time + 0.55f;
        }

        private void Update()
        {
            _velocity.y -= Gravity * Time.deltaTime;
            transform.position += (Vector3)(_velocity * Time.deltaTime);
            transform.Rotate(0f, 0f, 720f * Time.deltaTime);

            float left = _until - Time.time;
            if (_sprite != null && left < 0.2f) _sprite.color = new Color(0.75f, 0.72f, 0.80f, Mathf.Clamp01(left / 0.2f));
            if (left <= 0f) Destroy(gameObject);
        }
    }
}
