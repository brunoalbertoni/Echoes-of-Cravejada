using System;
using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um circulo de transmutacao no estrado do Grande Laboratorio. A runa do
    /// centro acende com golpes de cinzel: cada golpe a enche um pouco de luz
    /// (dois bastam), sem aviso nenhum na tela. Se o Alquimista estiver perto
    /// quando ela acender, ele vira prata.
    ///
    /// Depois de usado, o Alquimista o afoga em mercurio (Smother): a runa
    /// apaga e fica inutil por um tempo, e o Scrap tem que ir a outro.
    /// </summary>
    public class ArenaCircle : MonoBehaviour, IDamageable
    {
        [Tooltip("Quantos golpes ate a runa acender.")]
        [SerializeField] private int hitsToLight = 2;

        [Tooltip("Quanto tempo um golpe fica valendo; parado demais, a luz da runa se apaga.")]
        [SerializeField] private float hitMemory = 2.5f;

        [SerializeField] private SpriteRenderer rune;
        [SerializeField] private Color idleColor = new Color(0.62f, 0.68f, 0.82f);
        [SerializeField] private Color litColor = new Color(0.80f, 1f, 1f);
        [SerializeField] private Color smotheredColor = new Color(0.30f, 0.32f, 0.40f);

        private int _hits;
        private float _lastHit;
        private float _smotheredUntil;
        private float _flashUntil;
        private float _bump;

        /// <summary>Avisa quando a runa acende.</summary>
        public event Action<ArenaCircle> Polished;

        /// <summary>So funciona com a luta acontecendo.</summary>
        public bool Armed { get; set; }

        public bool Usable => Armed && Time.time >= _smotheredUntil;

        /// <summary>Usado pelos geradores de fase. O segundo valor sobrou de quando se segurava o botao.</summary>
        public void Configure(SpriteRenderer newRune, float unused)
        {
            rune = newRune;
        }

        /// <summary>Afogado em mercurio: inutil por tantos segundos.</summary>
        public void Smother(float seconds)
        {
            _smotheredUntil = Time.time + seconds;
            _hits = 0;
        }

        /// <summary>Volta como no comeco da luta.</summary>
        public void ResetCircle()
        {
            _smotheredUntil = 0f;
            _flashUntil = 0f;
            _hits = 0;
        }

        public void Damage(int amount, Vector2 from)
        {
            if (!Usable) return;

            if (Time.time - _lastHit > hitMemory) _hits = 0;
            _lastHit = Time.time;
            _hits++;
            _bump = 1f;

            if (_hits < hitsToLight) return;

            _hits = 0;
            _flashUntil = Time.time + 0.5f;
            NoiseSystem.Emit(transform.position, 10f, 0.5f);
            Sfx.RunePolished.Play(transform.position);
            StartCoroutine(Burst());
            Polished?.Invoke(this);
        }

        // ------------------------------------------------------------------
        // O estouro
        // ------------------------------------------------------------------

        private static Sprite _pixel;

        private static Sprite Pixel()
        {
            if (_pixel != null) return _pixel;
            var texture = new Texture2D(1, 1) { filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            _pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
            return _pixel;
        }

        private SpriteRenderer Piece(string name, Sprite sprite, int order)
        {
            var piece = new GameObject(name);
            piece.transform.position = transform.position;
            var renderer = piece.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            if (rune != null)
            {
                renderer.sortingLayerID = rune.sortingLayerID;
                renderer.sharedMaterial = rune.sharedMaterial;
            }
            renderer.sortingOrder = order;
            return renderer;
        }

        /// <summary>A runa estoura: um anel de luz abre e some.</summary>
        private IEnumerator Burst()
        {
            if (rune == null || rune.sprite == null) yield break;

            SpriteRenderer ring = Piece("RuneBurst", rune.sprite, rune.sortingOrder + 1);
            Vector3 baseScale = transform.lossyScale;
            for (float t = 0f; t < 0.45f; t += Time.deltaTime)
            {
                float k = t / 0.45f;
                ring.transform.localScale = baseScale * Mathf.Lerp(1f, 2.6f, 1f - (1f - k) * (1f - k));
                ring.color = new Color(litColor.r, litColor.g, litColor.b, 1f - k);
                yield return null;
            }

            Destroy(ring.gameObject);
        }

        /// <summary>
        /// O raio da runa ate o Alquimista: corre ate ele, pega, engrossa,
        /// pisca e some, com um clarao no ponto onde acerta.
        /// </summary>
        public void Strike(Vector3 target)
        {
            StartCoroutine(StrikeRoutine(target));
        }

        private IEnumerator StrikeRoutine(Vector3 target)
        {
            Vector3 from = transform.position + Vector3.up * 0.3f;
            Vector3 delta = target - from;
            float length = delta.magnitude;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            SpriteRenderer bolt = Piece("RuneBolt", Pixel(), 30);
            bolt.transform.position = from;
            bolt.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            bolt.color = litColor;

            for (float t = 0f; t < 0.12f; t += Time.deltaTime)
            {
                bolt.transform.localScale = new Vector3(length * (t / 0.12f), 0.18f, 1f);
                yield return null;
            }

            SpriteRenderer flash = rune != null && rune.sprite != null ? Piece("RuneHit", rune.sprite, 31) : null;
            if (flash != null) flash.transform.position = target;

            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                float k = t / 0.35f;
                float flicker = Mathf.Repeat(t * 30f, 1f) < 0.5f ? 1f : 0.6f;
                bolt.transform.localScale = new Vector3(length, Mathf.Lerp(0.5f, 0.05f, k) * flicker, 1f);
                Color c = Color.Lerp(Color.white, litColor, k);
                c.a = 1f - k * 0.6f;
                bolt.color = c;
                if (flash != null)
                {
                    flash.transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.8f, k);
                    flash.color = new Color(1f, 1f, 1f, 1f - k);
                }
                yield return null;
            }

            Destroy(bolt.gameObject);
            if (flash != null) Destroy(flash.gameObject);
        }

        private void Update()
        {
            if (_hits > 0 && Time.time - _lastHit > hitMemory) _hits = 0;
            _bump = Mathf.MoveTowards(_bump, 0f, Time.deltaTime * 4f);

            if (rune == null) return;

            // A runa enche de luz a cada golpe, e pulsa no golpe.
            Color target = Time.time < _flashUntil ? litColor : Usable ? idleColor : smotheredColor;
            if (Usable && _hits > 0) target = Color.Lerp(idleColor, litColor, _hits / (float)hitsToLight);
            target = Color.Lerp(target, Color.white, _bump * 0.5f);
            rune.color = Color.Lerp(rune.color, target, 12f * Time.deltaTime);
        }
    }
}
