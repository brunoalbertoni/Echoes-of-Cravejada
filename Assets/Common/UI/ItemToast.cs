using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O jeito discreto de mostrar um item ganho, sem frase e sem parar o
    /// jogo: o desenho do item salta de dentro do Scrap pra cima da cabeca
    /// dele, solta um anel de brilho, fica um instante cintilando
    /// (acompanhando o Scrap, que continua andando) e some subindo.
    ///
    /// O rubi cravado na bancada tem o seu proprio (<see cref="Socket"/>): o
    /// rubi aparece em cima, desce pra dentro do peito e o corpo acende em
    /// vermelho, com o estouro de faiscas.
    ///
    /// Itens contados (ferramentas, paginas, reagentes) nao chamam isto.
    /// </summary>
    public class ItemToast : MonoBehaviour
    {
        private const float Above = 2.1f;
        private const float Size = 0.85f;

        private static Sprite _ring;
        private static Sprite _spark;
        private static Sprite _ruby;
        private static Sprite _shard;

        private Transform _target;
        private SpriteRenderer _item;
        private SpriteRenderer _halo;
        private SpriteRenderer _label;
        private SpriteRenderer[] _sparks;
        private SpriteRenderer[] _body;
        private Color[] _bodyColors;
        private Color _glow;
        private float _baseScale;
        private float _age;
        private float _delay;
        private bool _socket;
        private bool _burst;

        /// <summary>O item ganho salta em cima do Scrap. glow: a cor do brilho.</summary>
        public static void Show(Sprite sprite, Color glow)
        {
            Spawn(sprite, glow, false, 0f);
        }

        /// <summary>O mesmo, com "+N" ao lado do desenho (o cofre de fragmentos).</summary>
        public static void Show(Sprite sprite, Color glow, int amount)
        {
            var toast = Spawn(sprite, glow, false, 0f);
            if (toast != null && amount > 0) toast.AddCount(amount);
        }

        /// <summary>O icone de fragmento do HUD.</summary>
        public static Sprite ShardSprite()
        {
            if (_shard != null) return _shard;
            var texture = Resources.Load<Texture2D>("HudIcons/shard");
            if (texture == null) return null;
            _shard = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
            return _shard;
        }

        /// <summary>O mesmo, desenhando um icone do HUD (o rubi bruto).</summary>
        public static void ShowRuby()
        {
            Spawn(RubySprite(), new Color(1f, 0.45f, 0.4f), false, 0f);
        }

        /// <summary>
        /// O rubi sendo cravado: espera a gravura da bancada sair do meio da
        /// tela (e nao entra na foto) e entao desce pro peito do Scrap.
        /// </summary>
        public static void Socket(float delay)
        {
            Spawn(RubySprite(), new Color(1f, 0.3f, 0.28f), true, delay);
        }

        private static ItemToast Spawn(Sprite sprite, Color glow, bool socket, float delay)
        {
            if (sprite == null) return null;

            ScrapController scrap = FindAnyObjectByType<ScrapController>();
            if (scrap == null) return null;

            var root = new GameObject(socket ? "RubySocketFx" : "ItemToast");
            var toast = root.AddComponent<ItemToast>();
            toast.Setup(scrap.transform, sprite, glow, socket, delay);
            return toast;
        }

        /// <summary>
        /// Poe o "+N" em pixel a esquerda do desenho. Fica filho do item, entao
        /// salta, cresce e some junto com ele.
        /// </summary>
        private void AddCount(int amount)
        {
            Sprite count = CountSprite("+" + amount);
            var child = new GameObject("Count");
            child.transform.SetParent(_item.transform, false);
            _label = child.AddComponent<SpriteRenderer>();
            _label.sprite = count;
            _label.sortingLayerName = "Actors";
            _label.sortingOrder = 32;

            const float gap = 2f / 32f;
            float itemW = _item.sprite.rect.width / _item.sprite.pixelsPerUnit;
            float labelW = count.rect.width / 32f;
            child.transform.localPosition = new Vector3(-(itemW + labelW) * 0.5f - gap, 0f, 0f);
            // O conjunto todo fica centrado sobre a cabeca.
            _item.transform.localPosition = new Vector3((labelW + gap) * 0.5f * _baseScale, 0f, 0f);
        }

        // Algarismos 3x5, uma linha por string, de cima pra baixo.
        private static readonly string[] Glyphs =
        {
            "111101101101111", "010110010010111", "111001111100111", "111001111001111", "101101111001001",
            "111100111001111", "111100111101111", "111001001001001", "111101111101111", "111101111001111",
        };
        private const string Plus = "000010111010000";

        /// <summary>O texto em pixel, claro com contorno escuro, no padrao do HUD.</summary>
        private static Sprite CountSprite(string text)
        {
            int w = text.Length * 4 - 1 + 2, h = 5 + 2;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var fill = new bool[w, h];
            for (int c = 0; c < text.Length; c++)
            {
                string g = text[c] == '+' ? Plus : Glyphs[text[c] - '0'];
                for (int gy = 0; gy < 5; gy++)
                    for (int gx = 0; gx < 3; gx++)
                        if (g[gy * 3 + gx] == '1') fill[1 + c * 4 + gx, h - 2 - gy] = true;
            }

            var ink = new Color(1f, 0.93f, 0.7f);
            var edge = new Color(0.12f, 0.08f, 0.06f, 1f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool near = false;
                    for (int dy = -1; dy <= 1 && !near; dy++)
                        for (int dx = -1; dx <= 1 && !near; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            near = nx >= 0 && ny >= 0 && nx < w && ny < h && fill[nx, ny];
                        }
                    texture.SetPixel(x, y, fill[x, y] ? ink : near ? edge : Color.clear);
                }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
        }

        private void Setup(Transform target, Sprite sprite, Color glow, bool socket, float delay)
        {
            _target = target;
            _glow = glow;
            _socket = socket;
            _delay = delay;
            transform.position = target.position + Vector3.up * Above;

            _halo = Layer("Halo", Ring(), 30);
            _halo.color = Color.clear;

            _item = Layer("Item", sprite, 32);
            // Cresce em passos inteiros de pixel, pra nao entortar o desenho.
            float pixels = Mathf.Max(1f, Mathf.Max(sprite.rect.width, sprite.rect.height));
            _baseScale = Mathf.Max(1f, Mathf.Floor(Size * 32f / pixels)) * sprite.pixelsPerUnit / 32f;
            _item.transform.localScale = Vector3.zero;

            _sparks = new SpriteRenderer[socket ? 10 : 4];
            for (int i = 0; i < _sparks.Length; i++)
            {
                _sparks[i] = Layer("Spark" + i, Spark(), 33);
                _sparks[i].color = Color.clear;
            }

            if (socket)
            {
                _body = target.GetComponentsInChildren<SpriteRenderer>();
                _bodyColors = new Color[_body.Length];
                for (int i = 0; i < _body.Length; i++) _bodyColors[i] = _body[i].color;
            }
        }

        private SpriteRenderer Layer(string name, Sprite sprite, int order)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = order;
            return renderer;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                Destroy(gameObject);
                return;
            }

            // Com conversa ou loja na tela, espera: o efeito e pra ser visto.
            if (Npc.Talking != null || Crisol.ShopOpen != null || ScriptedDialogue.Active) return;

            if (_delay > 0f)
            {
                _delay -= Time.deltaTime;
                if (_delay > 0f) return;
            }

            _age += Time.deltaTime;
            if (_socket) SocketStep();
            else ToastStep();
        }

        // ------------------------------------------------------------------
        // O item ganho: salta, brilha, cintila e some subindo.
        // ------------------------------------------------------------------

        private void ToastStep()
        {
            const float pop = 0.32f, leave = 1.55f, gone = 1.95f;

            float rise = EaseOut(Mathf.Clamp01(_age / pop));
            float y = Mathf.Lerp(0.7f, Above, rise);
            float scale = _age < pop ? Back(_age / pop) : 1f;

            float fade = 1f;
            if (_age > leave)
            {
                float t = Mathf.Clamp01((_age - leave) / (gone - leave));
                y += t * 0.5f;
                scale *= 1f - t * 0.6f;
                fade = 1f - t;
            }

            // Bobeia meio pixel pra cima e pra baixo, em degrau de pixel.
            if (_age > pop && _age < leave) y += Mathf.Round(Mathf.Sin((_age - pop) * 5f) * 1.5f) / 32f;

            transform.position = _target.position + Vector3.up * y;
            _item.transform.localScale = Vector3.one * _baseScale * scale;
            _item.color = new Color(1f, 1f, 1f, fade);
            if (_label != null) _label.color = new Color(1f, 1f, 1f, fade);

            Halo(_age - pop * 0.7f, 0.45f, 0.6f, 1.7f);
            Twinkle(fade, 0.55f);

            if (_age >= gone) Destroy(gameObject);
        }

        // ------------------------------------------------------------------
        // O rubi cravado: aparece, desce pro peito, o corpo acende.
        // ------------------------------------------------------------------

        private void SocketStep()
        {
            const float pop = 0.3f, hold = 0.75f, sink = 1.05f, flashEnd = 1.5f, gone = 1.7f;

            float y;
            float scale;
            float alpha = 1f;

            if (_age < pop)
            {
                y = Mathf.Lerp(Above + 0.5f, Above, EaseOut(_age / pop));
                scale = Back(_age / pop);
            }
            else if (_age < hold)
            {
                y = Above + Mathf.Round(Mathf.Sin((_age - pop) * 6f) * 1.5f) / 32f;
                scale = 1f;
            }
            else if (_age < sink)
            {
                float t = (_age - hold) / (sink - hold);
                y = Mathf.Lerp(Above, 0.75f, t * t);
                scale = 1f - t * 0.7f;
            }
            else
            {
                y = 0.75f;
                scale = 0f;
                alpha = 0f;
            }

            transform.position = _target.position + Vector3.up * y;
            _item.transform.localScale = Vector3.one * _baseScale * scale;
            _item.color = new Color(1f, 1f, 1f, alpha);

            if (_age >= sink && !_burst)
            {
                _burst = true;
                Sfx.Gem.Play();
                ScrapCameraFollow.Shake(0.05f, 0.12f);
            }

            // O estouro: o anel grande, as faiscas pra fora e o corpo em vermelho.
            Halo(_age - sink, 0.5f, 0.4f, 3.2f);
            if (_age >= sink) Burst(_age - sink, 0.6f);

            float flash = _age >= sink ? 1f - Mathf.Clamp01((_age - sink) / (flashEnd - sink)) : 0f;
            for (int i = 0; i < _body.Length; i++)
            {
                if (_body[i] == null) continue;
                _body[i].color = Color.Lerp(_bodyColors[i], new Color(1f, 0.35f, 0.32f, _bodyColors[i].a), flash * 0.85f);
            }

            if (_age >= gone)
            {
                for (int i = 0; i < _body.Length; i++) if (_body[i] != null) _body[i].color = _bodyColors[i];
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_body == null) return;
            for (int i = 0; i < _body.Length; i++) if (_body[i] != null) _body[i].color = _bodyColors[i];
        }

        // ------------------------------------------------------------------
        // Pecas do brilho
        // ------------------------------------------------------------------

        /// <summary>O anel que abre uma vez em volta do item e some.</summary>
        private void Halo(float t, float length, float from, float to)
        {
            if (t < 0f || t > length)
            {
                _halo.color = Color.clear;
                return;
            }

            float k = t / length;
            _halo.transform.localScale = Vector3.one * Mathf.Lerp(from, to, EaseOut(k));
            _halo.color = new Color(_glow.r, _glow.g, _glow.b, 1f - k);
        }

        /// <summary>As cruzinhas que piscam em volta do item, cada uma na sua vez.</summary>
        private void Twinkle(float fade, float radius)
        {
            for (int i = 0; i < _sparks.Length; i++)
            {
                float phase = _age * 2.2f + i * 0.25f;
                float on = Mathf.Repeat(phase, 1f);
                float a = on < 0.35f ? Mathf.Sin(on / 0.35f * Mathf.PI) : 0f;
                float angle = i * Mathf.PI * 0.5f + 0.6f + Mathf.Floor(phase) * 1.3f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                _sparks[i].transform.localPosition = Snap(offset);
                _sparks[i].color = Color.Lerp(Color.white, _glow, 0.4f) * new Color(1f, 1f, 1f, a * fade * (_age > 0.2f ? 1f : 0f));
            }
        }

        /// <summary>As faiscas voando pra fora do peito.</summary>
        private void Burst(float t, float length)
        {
            float k = Mathf.Clamp01(t / length);
            for (int i = 0; i < _sparks.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / _sparks.Length + 0.3f;
                float reach = Mathf.Lerp(0.2f, 1.3f + (i % 3) * 0.2f, EaseOut(k));
                _sparks[i].transform.localPosition = Snap(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * reach);
                _sparks[i].color = Color.Lerp(new Color(1f, 0.9f, 0.8f), _glow, k) * new Color(1f, 1f, 1f, 1f - k);
            }
        }

        private static Vector3 Snap(Vector3 v) => new Vector3(Mathf.Round(v.x * 32f) / 32f, Mathf.Round(v.y * 32f) / 32f, 0f);
        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);

        private static float Back(float t)
        {
            t = Mathf.Clamp01(t);
            const float s = 1.9f;
            t -= 1f;
            return t * t * ((s + 1f) * t + s) + 1f;
        }

        // ------------------------------------------------------------------
        // As texturas do brilho, desenhadas uma vez em codigo
        // ------------------------------------------------------------------

        private static Sprite Ring()
        {
            if (_ring != null) return _ring;

            const int size = 32;
            var texture = NewTexture(size);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - size * 0.5f, dy = y + 0.5f - size * 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    texture.SetPixel(x, y, d > 13f && d < 15f ? Color.white : Color.clear);
                }

            texture.Apply();
            _ring = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
            return _ring;
        }

        private static Sprite Spark()
        {
            if (_spark != null) return _spark;

            var texture = NewTexture(5);
            for (int y = 0; y < 5; y++)
                for (int x = 0; x < 5; x++)
                    texture.SetPixel(x, y, (x == 2 || y == 2) ? Color.white : Color.clear);
            texture.SetPixel(0, 2, new Color(1f, 1f, 1f, 0.5f));
            texture.SetPixel(4, 2, new Color(1f, 1f, 1f, 0.5f));
            texture.SetPixel(2, 0, new Color(1f, 1f, 1f, 0.5f));
            texture.SetPixel(2, 4, new Color(1f, 1f, 1f, 0.5f));
            texture.Apply();
            _spark = Sprite.Create(texture, new Rect(0, 0, 5, 5), new Vector2(0.5f, 0.5f), 32f);
            return _spark;
        }

        private static Sprite RubySprite()
        {
            if (_ruby != null) return _ruby;

            var texture = Resources.Load<Texture2D>("HudIcons/gem");
            if (texture == null) return null;
            _ruby = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
            return _ruby;
        }

        private static Texture2D NewTexture(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
        }
    }
}
