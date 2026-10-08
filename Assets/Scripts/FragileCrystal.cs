using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma plataforma de cristal fino. Aguenta o Scrap andando e pulando baixo;
    /// com barulho forte em cima dela — dash, golpe, queda alta — ela trinca e
    /// se desfaz. Volta depois de um tempo.
    ///
    /// E o teste de peso do District 3: o corpo de ferro tem que aprender a
    /// pisar leve.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class FragileCrystal : MonoBehaviour
    {
        [Tooltip("A partir de que forca o ruido a quebra. Passo faz 0.15 e pulo 0.2; golpe 0.8, dash 1.")]
        [SerializeField, Range(0f, 1f)] private float breakLoudness = 0.35f;

        [Tooltip("Segundos entre trincar e se desfazer: o tempo de sair de cima.")]
        [SerializeField] private float crackTime = 0.35f;

        [Tooltip("Segundos ate ela se refazer.")]
        [SerializeField] private float regrowTime = 4f;

        [SerializeField] private SpriteRenderer sprite;

        private BoxCollider2D _collider;
        private bool _breaking;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(SpriteRenderer newSprite)
        {
            sprite = newSprite;
        }

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        private void OnEnable()
        {
            NoiseSystem.Heard += OnNoise;
            ScrapHealth.Died += Restore;
        }

        private void OnDisable()
        {
            NoiseSystem.Heard -= OnNoise;
            ScrapHealth.Died -= Restore;
        }

        private void OnNoise(Vector2 origin, float radius, float loudness)
        {
            if (_breaking || loudness < breakLoudness) return;

            // So conta barulho feito em cima dela (ou encostado).
            Bounds bounds = _collider.bounds;
            bounds.Expand(new Vector3(0.6f, 1.6f, 0f));
            if (!bounds.Contains(new Vector3(origin.x, origin.y, bounds.center.z))) return;

            StartCoroutine(Break());
        }

        private IEnumerator Break()
        {
            _breaking = true;

            // Trinca: pisca por um instante antes de ceder.
            for (float t = 0f; t < crackTime; t += Time.deltaTime)
            {
                if (sprite != null) sprite.enabled = Mathf.FloorToInt(t * 30f) % 2 == 0;
                yield return null;
            }

            _collider.enabled = false;
            if (sprite != null) sprite.enabled = false;

            yield return new WaitForSeconds(regrowTime);
            Restore();
        }

        private void Restore()
        {
            StopAllCoroutines();

            _breaking = false;
            if (_collider != null) _collider.enabled = true;
            if (sprite != null) sprite.enabled = true;
        }
    }
}
