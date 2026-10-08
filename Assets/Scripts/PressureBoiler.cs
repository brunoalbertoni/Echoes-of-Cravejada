using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma caldeira corrompida. Cada golpe do cinzel sobe a pressao; no
    /// terceiro ela dispara, pisca por um instante — o tempo de se afastar — e
    /// explode. A explosao derruba as paredes rachadas ligadas a ela e machuca
    /// quem estiver perto, inimigo ou Scrap.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PressureBoiler : MonoBehaviour, IDamageable
    {
        [Tooltip("Quantos golpes ate ela disparar.")]
        [SerializeField] private int hitsToBurst = 3;

        [Tooltip("Segundos entre disparar e explodir.")]
        [SerializeField] private float fuse = 1.4f;

        [SerializeField] private float blastRadius = 4f;

        [Tooltip("As paredes rachadas que a explosao derruba.")]
        [SerializeField] private GameObject[] walls;

        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private ParticleSystem blast;

        private int _hits;
        private bool _burst;

        /// <summary>Quao cheia de pressao ela esta, de 0 a 1. O ponteiro do manometro dela pode usar.</summary>
        public float Pressure => Mathf.Clamp01(_hits / (float)Mathf.Max(1, hitsToBurst));

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(GameObject[] newWalls, SpriteRenderer newSprite, ParticleSystem newBlast)
        {
            walls = newWalls;
            sprite = newSprite;
            blast = newBlast;
        }

        public void Damage(int amount, Vector2 from)
        {
            if (_burst) return;

            _hits++;

            if (_hits >= hitsToBurst)
            {
                _burst = true;
                StartCoroutine(Burst());
                return;
            }

            if (sprite != null) StartCoroutine(Flash(0.12f, new Color(1f, 0.7f, 0.4f)));
        }

        private IEnumerator Flash(float time, Color color)
        {
            sprite.color = color;
            yield return new WaitForSeconds(time);
            sprite.color = Color.white;
        }

        private IEnumerator Burst()
        {
            // Pisca cada vez mais rapido ate estourar.
            for (float t = 0f; t < fuse; t += Time.deltaTime)
            {
                if (sprite != null)
                {
                    float rate = Mathf.Lerp(6f, 22f, t / fuse);
                    sprite.color = Mathf.FloorToInt(t * rate) % 2 == 0 ? new Color(1f, 0.45f, 0.3f) : Color.white;
                }

                yield return null;
            }

            if (blast != null) blast.Play();

            if (walls != null)
            {
                foreach (GameObject wall in walls)
                {
                    if (wall != null) wall.SetActive(false);
                }
            }

            Vector2 center = (Vector2)transform.position + Vector2.up;

            foreach (Collider2D hit in Physics2D.OverlapCircleAll(center, blastRadius))
            {
                if (hit.transform == transform) continue;

                var health = hit.GetComponentInParent<ScrapHealth>();
                if (health != null)
                {
                    health.TakeDamage(1, center);
                    continue;
                }

                var damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable != null && !ReferenceEquals(damageable, this)) damageable.Damage(3, center);
            }

            // Fica ali, apagada e escura: ja deu o que tinha.
            if (sprite != null) sprite.color = new Color(0.35f, 0.32f, 0.32f);
        }
    }
}
