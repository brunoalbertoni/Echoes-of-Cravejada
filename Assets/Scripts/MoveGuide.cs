using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>Os movimentos que o jogo ensina: as habilidades conquistadas e os das placas de tutorial.</summary>
    public enum ScrapMove
    {
        SteamDash,
        PressureJump,
        ResonanceStrike,
        WallJump,
        Pogo,
    }

    /// <summary>
    /// O que se diz de cada movimento (nome, teclas, como usar) e a mini cena
    /// animada que mostra o proprio Scrap fazendo o movimento, com as teclas
    /// acendendo na hora certa. Quem usa e o AbilityCard (a conquista) e a
    /// TutorialSign (as placas).
    ///
    /// Tudo em IMGUI, como o GameHud: sem asset nenhum alem dos quadros do
    /// Scrap, que vem do ScrapAnimator da cena.
    /// </summary>
    public static class MoveGuide
    {
        public sealed class Info
        {
            public string Title;
            public string Tagline;

            /// <summary>As teclas: o que esta entre colchetes vira tecla desenhada, o resto vira texto.</summary>
            public string Keys;

            public string Pad;
            public string[] HowTo;

            /// <summary>Quanto dura uma volta da mini cena, em segundos.</summary>
            public float Loop;
        }

        private static readonly Dictionary<ScrapMove, Info> Infos = new Dictionary<ScrapMove, Info>
        {
            [ScrapMove.SteamDash] = new Info
            {
                Title = "Sobrecarga de Velocidade",
                Tagline = "O motor do Coletor agora pulsa nas suas pernas.",
                Keys = "[Shift] ou [L]",
                Pad = "Controle: RB",
                HowTo = new[]
                {
                    "Aperte para arrancar reto para a frente.",
                    "No ar o arranque não cai: use no meio do pulo para cruzar vãos largos.",
                    "Um arranque por pulo. As costas esquentam enquanto ele recarrega.",
                },
                Loop = 3f,
            },
            [ScrapMove.PressureJump] = new Info
            {
                Title = "Salto de Pressão",
                Tagline = "A caldeira do Mestre agora sopra nos seus pistões.",
                Keys = "[Espaço] de novo, no ar",
                Pad = "Controle: A, no ar",
                HowTo = new[]
                {
                    "Pule e, ainda no ar, pule outra vez: uma rajada de vapor empurra você para cima.",
                    "O segundo pulo volta quando você pisa no chão ou encosta numa parede.",
                },
                Loop = 3f,
            },
            [ScrapMove.ResonanceStrike] = new Info
            {
                Title = "Golpe Ressonante",
                Tagline = "O cinzel aprendeu a cantar como cristal.",
                Keys = "Segure [J] e solte",
                Pad = "Controle: segure X e solte",
                HowTo = new[]
                {
                    "Segure o golpe até os olhos brilharem em azul, depois solte.",
                    "A onda fere tudo em volta e estilhaça barreiras de cristal.",
                    "É o golpe mais barulhento que você tem: tudo por perto vai ouvir.",
                },
                Loop = 3.2f,
            },
            [ScrapMove.WallJump] = new Info
            {
                Title = "Pulo de Parede",
                Tagline = "Onde não há chão, há parede.",
                Keys = "[←] [→] + [Espaço]",
                Pad = "Controle: direção + A",
                HowTo = new[]
                {
                    "Pule contra uma parede e, encostado nela, pule de novo para saltar para o outro lado.",
                    "Segure a direção da parede para escorregar devagar.",
                    "Numa chaminé estreita, vá alternando as paredes para subir.",
                },
                Loop = 2f,
            },
            [ScrapMove.Pogo] = new Info
            {
                Title = "Golpe para Baixo",
                Tagline = "Espinho também serve de degrau.",
                Keys = "No ar: [↓] + [J]",
                Pad = "Controle: baixo + X, no ar",
                HowTo = new[]
                {
                    "No ar, segure para baixo e golpeie: o cinzel bate embaixo de você.",
                    "Acertar espinhos ou inimigos por baixo faz você quicar para cima.",
                    "Encadeie os quiques para cruzar espinhos largos demais para um pulo.",
                },
                Loop = 3.4f,
            },
        };

        public static Info Get(ScrapMove move) => Infos[move];

        // ------------------------------------------------------------------
        // Desenho: cor com a transparencia do quadro todo
        // ------------------------------------------------------------------

        /// <summary>A transparencia de tudo o que se desenha aqui. Quem abre e fecha o painel ajusta.</summary>
        public static float Alpha = 1f;

        public static readonly Color Gold = new Color(0.95f, 0.80f, 0.45f);
        public static readonly Color GoldDark = new Color(0.45f, 0.33f, 0.16f);
        public static readonly Color Ink = new Color(0.07f, 0.06f, 0.08f, 0.97f);
        public static readonly Color Muted = new Color(0.25f, 0.20f, 0.18f);
        public static readonly Color Paper = new Color(0.93f, 0.90f, 0.82f);
        public static readonly Color Faded = new Color(0.66f, 0.62f, 0.56f);

        private static readonly Color DemoBack = new Color(0.11f, 0.10f, 0.12f, 1f);
        private static readonly Color Rock = new Color(0.30f, 0.23f, 0.20f);
        private static readonly Color RockTop = new Color(0.52f, 0.40f, 0.30f);
        private static readonly Color RockSeam = new Color(0.20f, 0.15f, 0.13f);
        private static readonly Color Spike = new Color(0.72f, 0.72f, 0.78f);
        private static readonly Color SpikeDark = new Color(0.40f, 0.40f, 0.46f);
        private static readonly Color Steam = new Color(0.92f, 0.96f, 1f);
        private static readonly Color Heat = new Color(1f, 0.55f, 0.40f);
        private static readonly Color Resonance = new Color(0.70f, 0.88f, 1f);
        private static readonly Color Crystal = new Color(0.55f, 0.80f, 1f);

        public static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = new Color(color.r, color.g, color.b, color.a * Alpha);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        public static void Text(Rect rect, string text, GUIStyle style, Color color)
        {
            Color previous = GUI.color;
            GUI.color = new Color(color.r, color.g, color.b, color.a * Alpha);
            GUI.Label(rect, text, style);
            GUI.color = previous;
        }

        /// <summary>A caixa de sempre do HUD: borda dourada por fora, fundo escuro por dentro.</summary>
        public static void Box(Rect inner, float border = 3f)
        {
            Fill(new Rect(inner.x - border, inner.y - border, inner.width + border * 2f, inner.height + border * 2f), Gold);
            Fill(inner, Ink);
        }

        // ------------------------------------------------------------------
        // Estilos de texto
        // ------------------------------------------------------------------

        private static readonly Dictionary<int, GUIStyle> Styles = new Dictionary<int, GUIStyle>();

        /// <summary>
        /// Um estilo de texto branco (a cor vem do Text). So pode ser chamado
        /// de dentro de um OnGUI, porque parte do GUI.skin.
        /// </summary>
        public static GUIStyle Style(int size, bool bold = false, TextAnchor anchor = TextAnchor.UpperLeft, bool wrap = false,
            bool italic = false)
        {
            int key = size * 1000 + (bold ? 100 : 0) + (italic ? 200 : 0) + (int)anchor * 2 + (wrap ? 1 : 0);
            if (Styles.TryGetValue(key, out GUIStyle style) && style != null) return style;

            FontStyle fontStyle = bold
                ? (italic ? FontStyle.BoldAndItalic : FontStyle.Bold)
                : (italic ? FontStyle.Italic : FontStyle.Normal);

            style = new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                fontStyle = fontStyle,
                alignment = anchor,
                wordWrap = wrap,
                richText = false,
            };
            style.normal.textColor = Color.white;
            style.padding = new RectOffset(0, 0, 0, 0);
            style.margin = new RectOffset(0, 0, 0, 0);

            Styles[key] = style;
            return style;
        }

        // ------------------------------------------------------------------
        // Teclas desenhadas
        // ------------------------------------------------------------------

        private struct Token
        {
            public string Text;
            public bool IsKey;
        }

        private static List<Token> Tokens(string keys)
        {
            var tokens = new List<Token>();
            int i = 0;

            while (i < keys.Length)
            {
                if (keys[i] == '[')
                {
                    int end = keys.IndexOf(']', i);
                    if (end < 0) end = keys.Length;
                    tokens.Add(new Token { Text = keys.Substring(i + 1, end - i - 1), IsKey = true });
                    i = end + 1;
                    continue;
                }

                int next = keys.IndexOf('[', i);
                if (next < 0) next = keys.Length;
                string text = keys.Substring(i, next - i).Trim();
                if (text.Length > 0) tokens.Add(new Token { Text = text, IsKey = false });
                i = next;
            }

            return tokens;
        }

        /// <summary>
        /// Desenha a linha de teclas a partir de (x, y) e devolve a largura. As
        /// teclas que a mini cena esta apertando agora aparecem afundadas e
        /// acesas. Com <paramref name="draw"/> falso, so mede.
        /// </summary>
        public static float Keys(string keys, float x, float y, float height, ScrapMove? move = null, float time = 0f, bool draw = true)
        {
            GUIStyle capStyle = Style(Mathf.RoundToInt(height * 0.45f), true, TextAnchor.MiddleCenter);
            GUIStyle wordStyle = Style(Mathf.RoundToInt(height * 0.45f), false, TextAnchor.MiddleLeft);
            const float gap = 8f;
            float cursor = x;

            foreach (Token token in Tokens(keys))
            {
                if (token.IsKey)
                {
                    float width = Mathf.Max(height, capStyle.CalcSize(new GUIContent(token.Text)).x + height * 0.6f);

                    if (draw)
                    {
                        bool pressed = move.HasValue && IsPressed(move.Value, token.Text, time);
                        Cap(new Rect(cursor, y, width, height), token.Text, pressed, capStyle);
                    }

                    cursor += width + gap;
                }
                else
                {
                    float width = wordStyle.CalcSize(new GUIContent(token.Text)).x;
                    if (draw) Text(new Rect(cursor, y - 2f, width + 4f, height), token.Text, wordStyle, Faded);
                    cursor += width + gap;
                }
            }

            return Mathf.Max(0f, cursor - gap - x);
        }

        private static void Cap(Rect rect, string label, bool pressed, GUIStyle style)
        {
            float depth = Mathf.Max(3f, rect.height * 0.12f);

            if (pressed)
            {
                // Afundada: sem a lateral, e acesa por dentro.
                var face = new Rect(rect.x, rect.y + depth, rect.width, rect.height - depth);
                Fill(new Rect(face.x - 4f, face.y - 4f, face.width + 8f, face.height + 8f), new Color(Gold.r, Gold.g, Gold.b, 0.25f));
                Fill(face, Gold);
                Fill(new Rect(face.x + 2f, face.y + 2f, face.width - 4f, face.height - 4f), new Color(1f, 0.90f, 0.62f));
                Text(face, label, style, new Color(0.16f, 0.11f, 0.06f));
            }
            else
            {
                Fill(rect, new Color(0.62f, 0.56f, 0.48f));
                Fill(new Rect(rect.x, rect.yMax - depth, rect.width, depth), new Color(0.30f, 0.26f, 0.22f));
                var face = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - depth - 2f);
                Fill(face, new Color(0.22f, 0.20f, 0.19f));
                Text(face, label, style, Paper);
            }
        }

        /// <summary>Quais teclas a mini cena esta apertando neste instante da volta.</summary>
        private static bool IsPressed(ScrapMove move, string key, float time)
        {
            float t = Mathf.Repeat(time, Get(move).Loop);

            switch (move)
            {
                case ScrapMove.SteamDash:
                    if (key == "Espaço") return Between(t, 0.45f, 0.60f);
                    if (key == "Shift") return Between(t, 0.75f, 0.92f);
                    return false;

                case ScrapMove.PressureJump:
                    return key == "Espaço" && (Between(t, 0.40f, 0.54f) || Between(t, 0.80f, 0.94f));

                case ScrapMove.ResonanceStrike:
                    return key == "J" && Between(t, 0.50f, 1.25f);

                case ScrapMove.WallJump:
                    if (key == "←") return Between(t, 0f, 0.45f);
                    if (key == "→") return Between(t, 1.0f, 1.45f);
                    if (key == "Espaço") return Between(t, 0.45f, 0.58f) || Between(t, 1.45f, 1.58f);
                    return false;

                case ScrapMove.Pogo:
                    if (key == "↓") return Between(t, 0.72f, 1.02f) || Between(t, 1.32f, 1.62f);
                    if (key == "J") return Between(t, 0.86f, 0.98f) || Between(t, 1.46f, 1.58f);
                    return false;
            }

            return false;
        }

        private static bool Between(float t, float from, float to) => t >= from && t < to;

        // ------------------------------------------------------------------
        // A mini cena
        // ------------------------------------------------------------------

        private static ScrapAnimator _animator;

        private static Sprite[] HeroFrames()
        {
            if (_animator == null) _animator = Object.FindAnyObjectByType<ScrapAnimator>();
            return _animator != null ? _animator.CurrentFrames : null;
        }

        /// <summary>
        /// A mini cena do movimento, num quadro escuro com moldura. O tempo e
        /// desde que o painel abriu, pra a cena sempre comecar do inicio.
        /// </summary>
        public static void Demo(ScrapMove move, Rect frame, float time)
        {
            Fill(new Rect(frame.x - 2f, frame.y - 2f, frame.width + 4f, frame.height + 4f), Muted);
            Fill(frame, DemoBack);

            GUI.BeginGroup(frame);

            float w = frame.width;
            float h = frame.height;

            // O tamanho do pixel: inteiro, pra a arte nao borrar.
            int p = Mathf.Max(1, Mathf.FloorToInt(h / 64f));

            // A grade apagada de tiles no fundo.
            float tile = 16f * p;
            for (float gx = tile; gx < w; gx += tile) Fill(new Rect(gx, 0f, 1f, h), new Color(1f, 1f, 1f, 0.025f));
            for (float gy = tile; gy < h; gy += tile) Fill(new Rect(0f, gy, w, 1f), new Color(1f, 1f, 1f, 0.025f));

            Info info = Get(move);
            float t = Mathf.Repeat(time, info.Loop);

            switch (move)
            {
                case ScrapMove.SteamDash: DashScene(w, h, p, t); break;
                case ScrapMove.PressureJump: JumpScene(w, h, p, t); break;
                case ScrapMove.ResonanceStrike: ResonanceScene(w, h, p, t); break;
                case ScrapMove.WallJump: WallScene(w, h, p, t, time); break;
                case ScrapMove.Pogo: PogoScene(w, h, p, t); break;
            }

            // A cena some no preto e volta do comeco (o pulo de parede nao: ele e um ciclo).
            if (move != ScrapMove.WallJump)
            {
                float fade = 0f;
                if (t < 0.25f) fade = 1f - t / 0.25f;
                else if (t > info.Loop - 0.4f) fade = (t - (info.Loop - 0.4f)) / 0.4f;
                if (fade > 0f) Fill(new Rect(0f, 0f, w, h), new Color(DemoBack.r, DemoBack.g, DemoBack.b, Mathf.Clamp01(fade)));
            }

            GUI.EndGroup();
        }

        private static int IdleFrame(float t) => ScrapAnimator.Idle + (int)(t * 3f) % ScrapAnimator.IdleFrames;

        private static void DashScene(float w, float h, int p, float t)
        {
            float ground = h * 0.78f;
            float pitLeft = w * 0.34f;
            float pitRight = w * 0.66f;

            Ground(0f, pitLeft, ground, h, p);
            Ground(pitRight, w, ground, h, p);
            Ground(pitLeft, pitRight, h - 3f * p, h, p);
            Spikes(pitLeft, pitRight, h - 3f * p, p);

            float x0 = w * 0.14f, xJump = w * 0.27f, xDash = w * 0.74f, xLand = w * 0.82f;
            float lift = h * 0.20f;

            if (t < 0.45f)
            {
                Hero(IdleFrame(t), x0, ground, p, false, Color.white);
            }
            else if (t < 0.75f)
            {
                float u = (t - 0.45f) / 0.30f;
                Hero(ScrapAnimator.Jump, Mathf.Lerp(x0, xJump, u), ground - lift * Mathf.Sin(u * Mathf.PI * 0.5f), p, false, Color.white);
            }
            else if (t < 1.0f)
            {
                float u = (t - 0.75f) / 0.25f;
                float y = ground - lift;

                // O rastro: o Scrap de um instante atras, cada vez mais apagado.
                for (int k = 3; k >= 1; k--)
                {
                    float back = u - k * 0.13f;
                    if (back < 0f) continue;
                    Hero(ScrapAnimator.Dash, Mathf.Lerp(xJump, xDash, back), y, p, false,
                        new Color(Heat.r, Heat.g, Heat.b, 0.45f - k * 0.12f));
                }

                Puff(new Vector2(xJump - 4f * p, y - 8f * p), t - 0.75f, 0.45f, p, Steam);
                Hero(ScrapAnimator.Dash, Mathf.Lerp(xJump, xDash, u), y, p, false, Color.white);
            }
            else if (t < 1.3f)
            {
                float u = (t - 1.0f) / 0.30f;
                Puff(new Vector2(xJump - 4f * p, ground - lift - 8f * p), t - 0.75f, 0.45f, p, Steam);
                Hero(ScrapAnimator.Fall, Mathf.Lerp(xDash, xLand, u), ground - lift * (1f - u * u), p, false, Color.white);
            }
            else
            {
                // As costas esfriam aos poucos, como no jogo.
                float heat = Mathf.Clamp01(1f - (t - 1.0f) / 0.9f);
                Hero(IdleFrame(t), xLand, ground, p, false, Color.Lerp(Color.white, Heat, heat));
            }
        }

        private static void JumpScene(float w, float h, int p, float t)
        {
            float ground = h * 0.86f;
            Ground(0f, w, ground, h, p);

            float x0 = w * 0.2f, x1 = w * 0.8f;
            float first = h * 0.22f, second = h * 0.44f;
            float x = Mathf.Lerp(x0, x1, Mathf.InverseLerp(0.4f, 1.75f, t));

            // A rajada do segundo pulo fica no ar, onde ele pulou.
            float burstX = Mathf.Lerp(x0, x1, Mathf.InverseLerp(0.4f, 1.75f, 0.8f));
            if (t >= 0.8f) Puff(new Vector2(burstX, ground - first + 2f * p), t - 0.8f, 0.6f, p, Steam, 1.6f);

            if (t < 0.4f)
            {
                Hero(IdleFrame(t), x0, ground, p, false, Color.white);
            }
            else if (t < 0.8f)
            {
                float u = (t - 0.4f) / 0.4f;
                Hero(ScrapAnimator.Jump, x, ground - first * Mathf.Sin(u * Mathf.PI * 0.5f), p, false, Color.white);
            }
            else if (t < 1.15f)
            {
                float u = (t - 0.8f) / 0.35f;
                Hero(ScrapAnimator.Jump, x, ground - first - (second - first) * Mathf.Sin(u * Mathf.PI * 0.5f), p, false, Color.white);
            }
            else if (t < 1.75f)
            {
                float u = (t - 1.15f) / 0.6f;
                Hero(ScrapAnimator.Fall, x, ground - second * (1f - u * u), p, false, Color.white);
            }
            else
            {
                Hero(IdleFrame(t), x1, ground, p, false, Color.white);
            }
        }

        private static void ResonanceScene(float w, float h, int p, float t)
        {
            float ground = h * 0.84f;
            Ground(0f, w, ground, h, p);

            float cx = w * 0.5f;
            float leftCrystal = w * 0.17f, rightCrystal = w * 0.83f;
            const float shatter = 1.62f;

            if (t < shatter)
            {
                CrystalShape(leftCrystal, ground, p);
                CrystalShape(rightCrystal, ground, p);
            }
            else if (t < 2.3f)
            {
                Shards(leftCrystal, ground, t - shatter, p, -1f);
                Shards(rightCrystal, ground, t - shatter, p, 1f);
            }

            bool charged = t >= 1.0f && t < 1.25f;
            Color tint = charged ? Color.Lerp(Color.white, Resonance, 0.5f + 0.5f * Mathf.Sin(t * 40f)) : Color.white;

            if (t < 0.5f || t >= 1.45f)
            {
                Hero(IdleFrame(t), cx, ground, p, false, Color.white);
            }
            else if (t < 1.25f)
            {
                if (charged)
                {
                    float glow = 0.35f + 0.25f * Mathf.Sin(t * 40f);
                    Fill(new Rect(cx - 10f * p, ground - 24f * p, 20f * p, 22f * p), new Color(Resonance.r, Resonance.g, Resonance.b, glow * 0.35f));
                }

                Hero(ScrapAnimator.Windup, cx, ground, p, false, tint);

                // A barra de carga embaixo dos pes.
                float charge = Mathf.Clamp01((t - 0.5f) / 0.5f);
                var bar = new Rect(cx - 10f * p, ground + 3f * p, 20f * p, 2f * p);
                Fill(bar, Muted);
                Fill(new Rect(bar.x, bar.y, bar.width * charge, bar.height), charge >= 1f ? Resonance : Gold);
            }
            else
            {
                Hero(ScrapAnimator.Strike, cx, ground, p, false, Color.white);
            }

            if (t >= 1.25f && t < 1.75f)
            {
                float u = (t - 1.25f) / 0.5f;
                var center = new Vector2(cx, ground - 10f * p);
                Ring(center, Mathf.Lerp(4f * p, w * 0.42f, u), p, new Color(Resonance.r, Resonance.g, Resonance.b, 1f - u));
                Ring(center, Mathf.Lerp(2f * p, w * 0.30f, u), p, new Color(1f, 1f, 1f, (1f - u) * 0.5f));
            }
        }

        private static void WallScene(float w, float h, int p, float t, float time)
        {
            float left = w * 0.36f;
            float right = w * 0.64f;

            // As paredes descem sem parar: e ele que esta subindo.
            float seam = 12f * p;
            float scroll = Mathf.Repeat(time * h * 0.32f, seam * 2f);
            Wall(0f, left, h, p, scroll, seam, true);
            Wall(right, w, h, p, scroll, seam, false);

            float reach = 7f * p;
            float baseY = h * 0.66f;
            float arc = h * 0.20f;
            float slide = 5f * p;

            if (t < 0.45f)
            {
                float u = t / 0.45f;
                Hero(ScrapAnimator.Wall, left + reach, baseY + slide * u, p, false, Color.white);
            }
            else if (t < 1.0f)
            {
                float u = (t - 0.45f) / 0.55f;
                Puff(new Vector2(left + 2f * p, baseY + slide - 6f * p), t - 0.45f, 0.35f, p, Faded);
                float y = Mathf.Lerp(baseY + slide, baseY, u) - 4f * arc * u * (1f - u);
                Hero(u < 0.5f ? ScrapAnimator.Jump : ScrapAnimator.Fall, Mathf.Lerp(left + reach, right - reach, u), y, p, false, Color.white);
            }
            else if (t < 1.45f)
            {
                float u = (t - 1.0f) / 0.45f;
                Hero(ScrapAnimator.Wall, right - reach, baseY + slide * u, p, true, Color.white);
            }
            else
            {
                float u = (t - 1.45f) / 0.55f;
                Puff(new Vector2(right - 2f * p, baseY + slide - 6f * p), t - 1.45f, 0.35f, p, Faded);
                float y = Mathf.Lerp(baseY + slide, baseY, u) - 4f * arc * u * (1f - u);
                Hero(u < 0.5f ? ScrapAnimator.Jump : ScrapAnimator.Fall, Mathf.Lerp(right - reach, left + reach, u), y, p, true, Color.white);
            }

            // Setinhas subindo no meio da chamine: o caminho e pra cima.
            for (int i = 0; i < 3; i++)
            {
                float y = Mathf.Repeat(h - time * h * 0.32f - i * h / 3f, h);
                Chevron(w * 0.5f, y, p, new Color(Gold.r, Gold.g, Gold.b, 0.18f));
            }
        }

        private static void PogoScene(float w, float h, int p, float t)
        {
            float ground = h * 0.8f;
            float spikeLeft = w * 0.2f, spikeRight = w * 0.8f;
            Ground(0f, w, ground, h, p);
            Spikes(spikeLeft, spikeRight, ground, p);

            float x0 = w * 0.09f, hit1 = w * 0.37f, hit2 = w * 0.61f, land = w * 0.9f;
            float hitY = ground - 13f * p;

            if (t < 0.35f)
            {
                Hero(IdleFrame(t), x0, ground, p, false, Color.white);
            }
            else if (t < 0.95f)
            {
                Leap(x0, ground, hit1, hitY, h * 0.32f, (t - 0.35f) / 0.6f, p, t > 0.82f);
            }
            else if (t < 1.55f)
            {
                Leap(hit1, hitY, hit2, hitY, h * 0.30f, (t - 0.95f) / 0.6f, p, t > 1.42f);
            }
            else if (t < 2.15f)
            {
                Leap(hit2, hitY, land, ground, h * 0.26f, (t - 1.55f) / 0.6f, p, false);
            }
            else
            {
                Hero(IdleFrame(t), land, ground, p, false, Color.white);
            }

            // O risco do cinzel embaixo dos pes, e a faisca no espinho.
            SlashDown(hit1, hitY, t - 0.95f, p);
            SlashDown(hit2, hitY, t - 1.55f, p);
        }

        /// <summary>Um salto em arco de um ponto a outro. Com <paramref name="aiming"/>, ja mira o golpe pra baixo.</summary>
        private static void Leap(float x0, float y0, float x1, float y1, float height, float u, int p, bool aiming)
        {
            float x = Mathf.Lerp(x0, x1, u);
            float y = Mathf.Lerp(y0, y1, u) - 4f * height * u * (1f - u);
            int frame = aiming ? ScrapAnimator.Windup : (u < 0.5f ? ScrapAnimator.Jump : ScrapAnimator.Fall);
            Hero(frame, x, y, p, false, Color.white);
        }

        private static void SlashDown(float x, float feetY, float age, int p)
        {
            const float life = 0.16f;
            if (age < 0f || age > life) return;

            float fade = 1f - age / life;
            for (int i = 0; i < 4; i++)
            {
                float width = (14 - i * 3) * p;
                Fill(new Rect(x - width * 0.5f, feetY + i * 1.5f * p, width, 1.5f * p), new Color(1f, 1f, 1f, fade));
            }

            Puff(new Vector2(x, feetY + 6f * p), age, 0.3f, p, Gold);
        }

        // ------------------------------------------------------------------
        // Pecas da mini cena
        // ------------------------------------------------------------------

        /// <summary>
        /// Um quadro do Scrap, com os pes em (feetX, feetY) e o pixel do
        /// tamanho p. Sem os quadros na cena, um boneco chapado no lugar.
        /// </summary>
        private static void Hero(int frame, float feetX, float feetY, int p, bool flip, Color tint)
        {
            Sprite[] frames = HeroFrames();
            Sprite sprite = frames != null && frame >= 0 && frame < frames.Length ? frames[frame] : null;

            if (sprite == null || sprite.texture == null)
            {
                Fill(new Rect(feetX - 5f * p, feetY - 18f * p, 10f * p, 18f * p), tint * new Color(0.8f, 0.55f, 0.35f));
                return;
            }

            Rect pixels = sprite.rect;
            Vector2 pivot = sprite.pivot;

            // A cena foi medida pro quadro antigo, de 32 pixels: um quadro
            // maior (mais detalhado) encolhe pra caber no mesmo espaco.
            float scale = p * Mathf.Min(1f, 32f / pixels.height);

            float left = flip ? feetX - (pixels.width - pivot.x) * scale : feetX - pivot.x * scale;
            float top = feetY - (pixels.height - pivot.y) * scale;
            var target = new Rect(left, top, pixels.width * scale, pixels.height * scale);

            Texture texture = sprite.texture;
            var uv = new Rect(pixels.x / texture.width, pixels.y / texture.height,
                pixels.width / texture.width, pixels.height / texture.height);
            if (flip) uv = new Rect(uv.xMax, uv.y, -uv.width, uv.height);

            Color previous = GUI.color;
            GUI.color = new Color(tint.r, tint.g, tint.b, tint.a * Alpha);
            GUI.DrawTextureWithTexCoords(target, texture, uv);
            GUI.color = previous;
        }

        private static void Ground(float x0, float x1, float top, float bottom, int p)
        {
            if (x1 <= x0) return;

            Fill(new Rect(x0, top, x1 - x0, bottom - top), Rock);
            Fill(new Rect(x0, top, x1 - x0, 2f * p), RockTop);

            // Umas emendas de pedra, sempre no mesmo lugar.
            for (float x = x0 + 9f * p; x < x1 - 2f * p; x += 15f * p)
            {
                Fill(new Rect(x, top + 5f * p, 4f * p, p), RockSeam);
            }
        }

        private static void Spikes(float x0, float x1, float baseY, int p)
        {
            float width = 6f * p;
            float height = 7f * p;

            for (float x = x0; x + width <= x1 + 0.5f; x += width)
            {
                float center = x + width * 0.5f;
                for (float r = 0f; r < height; r += p)
                {
                    float span = Mathf.Max(p, width * (1f - r / height));
                    Fill(new Rect(center - span * 0.5f, baseY - r - p, span, p), r < 2f * p ? SpikeDark : Spike);
                }
            }
        }

        private static void Wall(float x0, float x1, float h, int p, float scroll, float seam, bool leftSide)
        {
            Fill(new Rect(x0, 0f, x1 - x0, h), Rock);

            float edge = leftSide ? x1 - 2f * p : x0;
            Fill(new Rect(edge, 0f, 2f * p, h), RockTop);

            for (float y = -seam * 2f + scroll; y < h; y += seam)
            {
                float shift = Mathf.Repeat(y - scroll, seam * 2f) < seam ? 0f : 6f * p;
                for (float x = x0 + 3f * p + shift; x < x1 - 4f * p; x += 14f * p)
                {
                    Fill(new Rect(x, y, 7f * p, p), RockSeam);
                }
            }
        }

        private static void CrystalShape(float cx, float ground, int p)
        {
            int[] rows = { 2, 4, 6, 6, 6, 6, 6, 4, 4, 2 };
            for (int i = 0; i < rows.Length; i++)
            {
                float width = rows[i] * p;
                Color color = i < 3 ? Color.Lerp(Crystal, Color.white, 0.5f) : Crystal;
                Fill(new Rect(cx - width * 0.5f, ground - (rows.Length - i) * p * 1.5f, width, p * 1.5f), color);
            }
        }

        private static void Shards(float cx, float ground, float age, int p, float side)
        {
            float fade = Mathf.Clamp01(1f - age / 0.6f);
            for (int i = 0; i < 6; i++)
            {
                float angle = (35f + i * 22f) * Mathf.Deg2Rad;
                float speed = (40f + i * 9f) * p * 0.5f;
                float x = cx + side * Mathf.Cos(angle) * speed * age;
                float y = ground - 8f * p - Mathf.Sin(angle) * speed * age + 120f * p * age * age;
                Fill(new Rect(x, y, 2f * p, 2f * p), new Color(Crystal.r, Crystal.g, Crystal.b, fade));
            }
        }

        private static void Ring(Vector2 center, float radius, int p, Color color)
        {
            const int dots = 40;
            for (int i = 0; i < dots; i++)
            {
                float angle = i * Mathf.PI * 2f / dots;
                float x = center.x + Mathf.Cos(angle) * radius;
                float y = center.y + Mathf.Sin(angle) * radius * 0.8f;
                Fill(new Rect(x - p, y - p, 2f * p, 2f * p), color);
            }
        }

        /// <summary>Um sopro de vapor ou poeira: quadradinhos se abrindo e sumindo.</summary>
        private static void Puff(Vector2 center, float age, float life, int p, Color color, float size = 1f)
        {
            if (age < 0f || age > life) return;

            float u = age / life;
            float spread = (3f + 9f * u) * p * size;
            float square = Mathf.Max(p, (3f - 2f * u) * p * size);
            var faded = new Color(color.r, color.g, color.b, (1f - u) * 0.8f);

            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3f + 0.4f;
                float x = center.x + Mathf.Cos(angle) * spread;
                float y = center.y + Mathf.Sin(angle) * spread * 0.6f;
                Fill(new Rect(x - square * 0.5f, y - square * 0.5f, square, square), faded);
            }
        }

        private static void Chevron(float cx, float y, int p, Color color)
        {
            for (int i = 0; i < 4; i++)
            {
                Fill(new Rect(cx - (i + 1) * p * 1.5f, y + i * p, p * 1.5f, p), color);
                Fill(new Rect(cx + i * p * 1.5f, y + i * p, p * 1.5f, p), color);
            }
        }
    }
}
