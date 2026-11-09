using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A campanula de vidro que o Alquimista derruba do teto: ele quer uma
    /// amostra do prototipo.
    ///
    /// Primeiro a sombra aparece no chao, embaixo de onde o Scrap estava, e o
    /// vidro range no teto. Depois ela despenca. Se pegar o Scrap, ele leva um
    /// golpe e fica preso: as paredes de vidro nao deixam sair, e so o cinzel
    /// (tres golpes) quebra a redoma. Enquanto esta preso, o ar acaba e ele
    /// apanha de novo de tempos em tempos. Se nao pegar ninguem, ela fica no
    /// chao um instante e estilhaca sozinha.
    ///
    /// Quem a usa chama Drop; ela volta pro teto, escondida, quando acaba.
    /// </summary>
    public class BellJar : MonoBehaviour, IDamageable
    {
        [SerializeField] private float ceilingY = 79f;
        [SerializeField] private float floorY = 62f;
        [SerializeField] private float warnTime = 0.9f;
        [SerializeField] private float suffocateEvery = 2.6f;
        [SerializeField] private int hitsToBreak = 3;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer glass;
        [SerializeField] private Sprite[] crackStages;
        [SerializeField] private SpriteRenderer shadow;

        [Header("Paredes de vidro")]
        [SerializeField] private Collider2D[] walls;

        private ScrapHealth _trapped;
        private int _hits;
        private bool _busy;

        public bool Busy => _busy;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newCeilingY, float newFloorY, SpriteRenderer newGlass, Sprite[] newCracks,
            SpriteRenderer newShadow, Collider2D[] newWalls)
        {
            ceilingY = newCeilingY;
            floorY = newFloorY;
            glass = newGlass;
            crackStages = newCracks;
            shadow = newShadow;
            walls = newWalls;
        }

        private void Awake()
        {
            Hide();
            ScrapHealth.Died += Hide;
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= Hide;
        }

        /// <summary>Some com tudo: sem vidro, sem sombra, sem paredes, sem ninguem preso.</summary>
        public void Hide()
        {
            StopAllCoroutines();
            _busy = false;
            _trapped = null;
            _hits = 0;

            if (glass != null) glass.enabled = false;
            if (shadow != null) shadow.enabled = false;
            SetWalls(false);

            var area = GetComponent<Collider2D>();
            if (area != null) area.enabled = false;
        }

        public void Drop(float x)
        {
            if (_busy) return;
            StartCoroutine(Fall(x));
        }

        /// <summary>O chao embaixo de x: o piso da arena ou o alto de um estrado.</summary>
        private float GroundUnder(float x)
        {
            int mask = LayerMask.GetMask("Ground");
            RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, ceilingY - 3.2f), Vector2.down, ceilingY - floorY + 2f, mask);
            float best = floorY;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider.isTrigger || hit.collider.transform.IsChildOf(transform)) continue;
                best = Mathf.Max(floorY, hit.point.y);
                break;
            }

            return best;
        }

        private IEnumerator Fall(float x)
        {
            _busy = true;
            _hits = 0;
            float land = GroundUnder(x);

            transform.position = new Vector3(x, ceilingY - 2.75f, 0f);
            if (glass != null)
            {
                glass.enabled = true;
                glass.sprite = Stage(0);
                glass.color = new Color(1f, 1f, 1f, 0.9f);
            }

            if (shadow != null)
            {
                shadow.enabled = true;
                shadow.transform.position = new Vector3(x, land, 0f);
            }

            // O aviso: a sombra cresce no chao e o vidro treme no teto.
            Vector3 top = transform.position;
            for (float t = 0f; t < warnTime; t += Time.deltaTime)
            {
                float k = t / warnTime;
                if (shadow != null) shadow.transform.localScale = new Vector3(0.4f + k * 0.6f, 1f, 1f);
                transform.position = top + Vector3.right * (Mathf.Sin(t * 60f) * 0.05f);
                yield return null;
            }

            transform.position = top;

            // A queda.
            float speed = 0f;
            while (transform.position.y > land)
            {
                speed += 60f * Time.deltaTime;
                transform.position += Vector3.down * (speed * Time.deltaTime);
                yield return null;
            }

            transform.position = new Vector3(x, land, 0f);
            if (shadow != null) shadow.enabled = false;
            NoiseSystem.Emit(transform.position, 10f, 0.7f);

            var area = GetComponent<Collider2D>();
            if (area != null) area.enabled = true;

            // Pegou o Scrap?
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null)
            {
                Vector2 offset = controller.transform.position - transform.position;
                if (Mathf.Abs(offset.x) < 1.15f && offset.y > -0.4f && offset.y < 2.2f)
                {
                    _trapped = controller.GetComponent<ScrapHealth>();
                    var body = controller.GetComponent<Rigidbody2D>();
                    if (body != null)
                    {
                        body.position = new Vector2(x, land + 0.05f);
                        body.linearVelocity = Vector2.zero;
                    }

                    if (_trapped != null) _trapped.TakeDamage(1, transform.position + Vector3.up);

                    // O golpe matou: a redoma ja sumiu junto com a luta.
                    if (!_busy) yield break;
                }
            }

            if (_trapped != null)
            {
                SetWalls(true);

                // Preso: o ar vai acabando.
                float next = Time.time + suffocateEvery;
                while (_trapped != null && _hits < hitsToBreak)
                {
                    if (Time.time >= next)
                    {
                        _trapped.TakeDamage(1, transform.position + Vector3.up);
                        next = Time.time + suffocateEvery;
                    }

                    yield return null;
                }

                if (!_busy) yield break;
            }
            else
            {
                // Ninguem dentro: fica um instante e racha sozinha.
                yield return new WaitForSeconds(1.1f);
            }

            yield return Shatter();
        }

        private IEnumerator Shatter()
        {
            SetWalls(false);
            NoiseSystem.Emit(transform.position, 9f, 0.6f);

            if (glass != null)
            {
                glass.sprite = Stage(crackStages != null ? crackStages.Length - 1 : 0);
                for (float t = 0f; t < 0.3f; t += Time.deltaTime)
                {
                    glass.color = new Color(1f, 1f, 1f, 0.9f * (1f - t / 0.3f));
                    transform.localScale = Vector3.one * (1f + t * 0.4f);
                    yield return null;
                }
            }

            transform.localScale = Vector3.one;
            Hide();
        }

        public void Damage(int amount, Vector2 from)
        {
            if (!_busy || _trapped == null) return;

            _hits += Mathf.Max(1, amount);
            if (glass != null) glass.sprite = Stage(Mathf.Min(_hits, hitsToBreak - 1));
            NoiseSystem.Emit(transform.position, 6f, 0.4f);

            if (_hits >= hitsToBreak) _trapped = null;
        }

        private Sprite Stage(int index)
        {
            if (crackStages == null || crackStages.Length == 0) return glass != null ? glass.sprite : null;
            return crackStages[Mathf.Clamp(index, 0, crackStages.Length - 1)];
        }

        private void SetWalls(bool on)
        {
            if (walls == null) return;
            foreach (Collider2D wall in walls)
            {
                if (wall != null) wall.enabled = on;
            }
        }
    }
}
