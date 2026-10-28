using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O estouro do Acid Bloater: uma copia dele que incha e pisca verde por
    /// um instante, e so entao arrebenta e deixa a poca. E o aviso que da
    /// tempo de sair de perto.
    /// </summary>
    public class BloaterBurst : MonoBehaviour
    {
        private const float SwellTime = 0.55f;

        private SpriteRenderer _sprite;
        private Vector3 _ground;
        private float _width;
        private float _time;
        private Sprite _puddleSprite;
        private Sprite[] _swellFrames;

        /// <param name="swellFrames">Os quadros de inchar (o vidro rachando). Sem eles, so a copia do sprite incha.</param>
        public static void Begin(SpriteRenderer source, Vector3 center, Vector3 ground, float width,
            float time, Sprite puddleSprite, Sprite[] swellFrames = null)
        {
            var burstObject = new GameObject("BloaterBurst");
            burstObject.transform.position = center;

            var sprite = burstObject.AddComponent<SpriteRenderer>();
            if (source != null)
            {
                sprite.sprite = source.sprite;
                sprite.flipX = source.flipX;
                sprite.sortingLayerID = source.sortingLayerID;
                sprite.sortingOrder = source.sortingOrder;
                sprite.sharedMaterial = source.sharedMaterial;
            }

            var burst = burstObject.AddComponent<BloaterBurst>();
            burst._sprite = sprite;
            burst._ground = ground;
            burst._width = width;
            burst._time = time;
            burst._puddleSprite = puddleSprite;
            burst._swellFrames = swellFrames;
        }

        private void Awake()
        {
            // Se o Scrap morrer no meio, o bloater volta inteiro: o estouro some.
            ScrapHealth.Died += Cancel;
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= Cancel;
        }

        private void Cancel()
        {
            Destroy(gameObject);
        }

        private IEnumerator Start()
        {
            var acid = new Color(0.7f, 1f, 0.35f);

            for (float t = 0f; t < SwellTime; t += Time.deltaTime)
            {
                float k = t / SwellTime;

                // Com os quadros desenhados, o vidro vai rachando; a escala so ajuda.
                bool drawn = _swellFrames != null && _swellFrames.Length > 0;
                if (drawn && _sprite != null)
                {
                    Sprite frame = _swellFrames[Mathf.Min(_swellFrames.Length - 1, (int)(k * _swellFrames.Length))];
                    if (frame != null) _sprite.sprite = frame;
                }

                // Incha cada vez mais rapido, tremendo, e pisca entre branco e verde.
                float swell = 1f + (drawn ? 0.12f : 0.35f) * k * k + Mathf.Sin(t * 60f) * 0.03f * k;
                transform.localScale = new Vector3(swell, swell * (1f + 0.08f * k), 1f);

                bool blink = Mathf.Repeat(t * (6f + 14f * k), 1f) < 0.5f;
                if (_sprite != null) _sprite.color = blink ? acid : Color.white;

                yield return null;
            }

            AcidPuddle.Spill(_ground, _width, _time, _puddleSprite);
            Destroy(gameObject);
        }
    }
}
