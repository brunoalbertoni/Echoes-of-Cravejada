using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Particulas de pixel, desenhadas sem luz (brilham no escuro), presas na
    /// grade de 16 px por tile: o Oxide sendo sugado, a ferrugem estourando, o
    /// verde evaporando em ouro, a poeira de luz caindo. Um gerente so por
    /// cena, criado na primeira vez que alguem pede, com um estoque fixo de
    /// sprites reaproveitados.
    /// </summary>
    public class Motes : MonoBehaviour
    {
        private const int Capacity = 700;
        private const float Pixel = 1f / 16f;

        private enum Kind
        {
            Free,
            Attract,
        }

        private struct Mote
        {
            public bool Alive;
            public Kind Kind;
            public Vector2 Position;
            public Vector2 Velocity;
            public Vector2 From;
            public Vector2 Target;
            public float Swirl;
            public float Age;
            public float Life;
            public float Gravity;
            public float Drag;
            public float Wobble;
            public Color Start;
            public Color End;
        }

        private static Motes _instance;
        private static Sprite _pixel;
        private static Material _unlit;

        private readonly Mote[] _motes = new Mote[Capacity];
        private readonly SpriteRenderer[] _renderers = new SpriteRenderer[Capacity];
        private int _next;

        private static Motes Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var go = new GameObject("Motes");
                _instance = go.AddComponent<Motes>();
                return _instance;
            }
        }

        private void Awake()
        {
            if (_pixel == null)
            {
                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                _pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 16f);
            }

            if (_unlit == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader != null) _unlit = new Material(shader);
            }

            for (int i = 0; i < Capacity; i++)
            {
                var go = new GameObject("Mote");
                go.transform.SetParent(transform, false);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = _pixel;
                renderer.sortingLayerName = "Actors";
                renderer.sortingOrder = 45;
                if (_unlit != null) renderer.sharedMaterial = _unlit;
                renderer.enabled = false;
                _renderers[i] = renderer;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private int Take()
        {
            // Procura um livre a partir do ultimo; se estiver tudo cheio, recicla o mais antigo da volta.
            for (int n = 0; n < Capacity; n++)
            {
                int i = (_next + n) % Capacity;
                if (_motes[i].Alive) continue;
                _next = (i + 1) % Capacity;
                return i;
            }

            int oldest = _next;
            _next = (_next + 1) % Capacity;
            return oldest;
        }

        private void Spawn(Mote mote, float sizePixels)
        {
            int i = Take();
            mote.Alive = true;
            mote.Age = 0f;
            _motes[i] = mote;

            SpriteRenderer renderer = _renderers[i];
            renderer.enabled = true;
            renderer.transform.localScale = new Vector3(sizePixels, sizePixels, 1f);
            renderer.color = mote.Start;
            renderer.transform.position = Snap(mote.Position);
        }

        private static Vector3 Snap(Vector2 p) => new Vector3(Mathf.Round(p.x / Pixel) * Pixel, Mathf.Round(p.y / Pixel) * Pixel, 0f);

        private void Update()
        {
            float dt = Time.deltaTime;

            for (int i = 0; i < Capacity; i++)
            {
                ref Mote m = ref _motes[i];
                if (!m.Alive) continue;

                m.Age += dt;
                float k = m.Age / m.Life;
                if (k >= 1f)
                {
                    m.Alive = false;
                    _renderers[i].enabled = false;
                    continue;
                }

                if (m.Kind == Kind.Attract)
                {
                    // Puxado pro alvo, cada vez mais rapido, num redemoinho.
                    float pull = k * k * (3f - 2f * k);
                    Vector2 straight = Vector2.Lerp(m.From, m.Target, pull);
                    Vector2 away = m.From - m.Target;
                    Vector2 side = new Vector2(-away.y, away.x).normalized * Mathf.Sin(pull * Mathf.PI) * m.Swirl;
                    m.Position = straight + side;
                }
                else
                {
                    m.Velocity.y += m.Gravity * dt;
                    m.Velocity *= Mathf.Max(0f, 1f - m.Drag * dt);
                    m.Position += m.Velocity * dt;
                    if (m.Wobble > 0f) m.Position.x += Mathf.Sin((m.Age + i) * 5f) * m.Wobble * dt;
                }

                Color color = Color.Lerp(m.Start, m.End, k);
                color.a *= k > 0.7f ? (1f - k) / 0.3f : 1f;

                SpriteRenderer renderer = _renderers[i];
                renderer.color = color;
                renderer.transform.position = Snap(m.Position);
            }
        }

        // ------------------------------------------------------------------
        // O que se pede
        // ------------------------------------------------------------------

        /// <summary>Um estouro em todas as direcoes, com gravidade (gravity negativa puxa pra baixo).</summary>
        public static void Burst(Vector2 at, int count, Color a, Color b, float speedMin, float speedMax, float gravity,
            float life, float sizePixels = 2f, float drag = 0.6f)
        {
            Motes motes = Instance;
            for (int n = 0; n < count; n++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float speed = Random.Range(speedMin, speedMax);
                motes.Spawn(new Mote
                {
                    Kind = Kind.Free,
                    Position = at + Random.insideUnitCircle * 0.2f,
                    Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed,
                    Life = life * Random.Range(0.7f, 1.2f),
                    Gravity = gravity,
                    Drag = drag,
                    Start = Color.Lerp(a, b, Random.value * 0.5f),
                    End = b,
                }, sizePixels * (Random.value < 0.25f ? 0.5f : 1f));
            }
        }

        /// <summary>Uma fumacinha subindo devagar, balancando: o verde evaporando e virando outra cor.</summary>
        public static void Rise(Vector2 at, int count, Color a, Color b, float spread = 0.6f, float life = 1.8f)
        {
            Motes motes = Instance;
            for (int n = 0; n < count; n++)
            {
                motes.Spawn(new Mote
                {
                    Kind = Kind.Free,
                    Position = at + new Vector2(Random.Range(-spread, spread), Random.Range(-0.1f, 0.3f)),
                    Velocity = new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(0.8f, 2.2f)),
                    Life = life * Random.Range(0.7f, 1.3f),
                    Gravity = 0.4f,
                    Drag = 0.3f,
                    Wobble = 0.6f,
                    Start = a,
                    End = b,
                }, Random.value < 0.3f ? 2f : 1f);
            }
        }

        /// <summary>Uma particula que sai de from e e sugada ate to em time segundos, girando.</summary>
        public static void Attract(Vector2 from, Vector2 to, Color a, Color b, float time, float sizePixels = 2f)
        {
            Instance.Spawn(new Mote
            {
                Kind = Kind.Attract,
                Position = from,
                From = from,
                Target = to,
                Swirl = Random.Range(-2.5f, 2.5f),
                Life = time,
                Start = a,
                End = b,
            }, sizePixels);
        }

        /// <summary>Poeira de luz caindo devagar numa faixa larga (de cima de area).</summary>
        public static void Drift(Rect area, int count, Color a, Color b, float life = 4f)
        {
            Motes motes = Instance;
            for (int n = 0; n < count; n++)
            {
                motes.Spawn(new Mote
                {
                    Kind = Kind.Free,
                    Position = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax)),
                    Velocity = new Vector2(Random.Range(-0.15f, 0.15f), Random.Range(-0.9f, -0.35f)),
                    Life = life * Random.Range(0.6f, 1.2f),
                    Gravity = 0f,
                    Drag = 0f,
                    Wobble = 0.8f,
                    Start = a,
                    End = b,
                }, Random.value < 0.2f ? 2f : 1f);
            }
        }
    }
}
