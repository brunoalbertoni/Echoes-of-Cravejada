using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma fagulha solta: pula, cai com a gravidade, esfria do amarelo pro
    /// laranja e some. A Bancada de Ourives solta um punhado a cada martelada.
    /// </summary>
    public class BenchSpark : MonoBehaviour
    {
        private static Sprite _dot;

        private SpriteRenderer _renderer;
        private Vector2 _velocity;
        private float _life;
        private float _age;
        private Color _hot;
        private Color _cold;

        /// <summary>Solta <paramref name="count"/> fagulhas de <paramref name="from"/>.</summary>
        public static void Burst(Vector2 from, int count, Color hot, Color cold, float speed = 4f, float spread = 70f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (90f + Random.Range(-spread, spread)) * Mathf.Deg2Rad;
                var velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed * Random.Range(0.5f, 1.2f);
                Spawn(from, velocity, Random.Range(0.35f, 0.7f), hot, cold);
            }
        }

        private static void Spawn(Vector2 from, Vector2 velocity, float life, Color hot, Color cold)
        {
            var spark = new GameObject("BenchSpark");
            spark.transform.position = from;

            var renderer = spark.AddComponent<SpriteRenderer>();
            renderer.sprite = Dot();
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 30;
            renderer.color = hot;

            var script = spark.AddComponent<BenchSpark>();
            script._renderer = renderer;
            script._velocity = velocity;
            script._life = life;
            script._hot = hot;
            script._cold = cold;
        }

        /// <summary>Um pixel branco de 2x2 (a 32 px por tile), feito uma vez so.</summary>
        private static Sprite Dot()
        {
            if (_dot != null) return _dot;

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            texture.SetPixels32(new[] { new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255) });
            texture.Apply();
            _dot = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 32f);
            return _dot;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_age >= _life)
            {
                Destroy(gameObject);
                return;
            }

            _velocity.y -= 14f * Time.deltaTime;
            transform.position += (Vector3)(_velocity * Time.deltaTime);

            float k = _age / _life;
            Color color = Color.Lerp(_hot, _cold, k);
            color.a = 1f - k * k;
            _renderer.color = color;
        }
    }
}
