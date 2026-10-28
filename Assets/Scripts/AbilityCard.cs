using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A tela de conquista: quando o Scrap ganha uma habilidade (Steam Dash,
    /// Pressure Jump, Resonance Strike), o jogo para e aparece um card no
    /// meio da tela com o medalhao, o nome, a mini cena do movimento com as
    /// teclas acendendo e o como usar. Fecha com Espaco, Enter, E ou J.
    ///
    /// Nao precisa estar na cena: o primeiro Show cria o objeto. Enquanto o
    /// card esta aberto, o tempo para (timeScale 0) e o controle do Scrap fica
    /// travado, pra a tecla de fechar nao virar pulo nem conversa.
    /// </summary>
    public class AbilityCard : MonoBehaviour
    {
        // Um respiro entre ganhar e o card abrir, pra o momento ser visto.
        private const float OpenDelay = 0.35f;

        // Ninguem fecha sem querer: o aviso de fechar so aparece depois disto.
        private const float MinOpenTime = 1.1f;

        private const float AppearTime = 0.4f;
        private const float CloseTime = 0.25f;

        private static AbilityCard _instance;
        private static readonly Queue<ScrapMove> Pending = new Queue<ScrapMove>();

        private ScrapMove? _current;
        private float _readyAt;
        private float _openedAt;
        private float _closingAt = -1f;
        private float _previousTimeScale = 1f;
        private ScrapController _scrap;
        private bool _previousLock;

        /// <summary>Verdadeiro enquanto o card esta na tela. Npc e placas olham aqui.</summary>
        public static bool Showing => _instance != null && _instance._current.HasValue;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _instance = null;
            Pending.Clear();
            if (Time.timeScale == 0f) Time.timeScale = 1f;
        }

        /// <summary>Mostra o card desta habilidade. Se ja houver um aberto, entra na fila.</summary>
        public static void Show(ScrapMove move)
        {
            if (_instance == null)
            {
                var host = new GameObject("AbilityCard");
                _instance = host.AddComponent<AbilityCard>();
            }

            Pending.Enqueue(move);
            if (!_instance._current.HasValue) _instance._readyAt = Time.unscaledTime + OpenDelay;
        }

        private void Update()
        {
            if (!_current.HasValue)
            {
                if (Pending.Count > 0 && Time.unscaledTime >= _readyAt) Open(Pending.Dequeue());
                return;
            }

            if (_closingAt >= 0f)
            {
                if (Time.unscaledTime - _closingAt >= CloseTime) Close();
                return;
            }

            if (Time.unscaledTime - _openedAt >= MinOpenTime && ContinuePressed())
            {
                _closingAt = Time.unscaledTime;
            }
        }

        private void Open(ScrapMove move)
        {
            _current = move;
            _openedAt = Time.unscaledTime;
            _closingAt = -1f;

            _previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;

            _scrap = FindAnyObjectByType<ScrapController>();
            if (_scrap != null)
            {
                _previousLock = _scrap.ControlLocked;
                _scrap.ControlLocked = true;
            }
        }

        private void Close()
        {
            _current = null;
            _closingAt = -1f;
            Time.timeScale = _previousTimeScale;

            if (_scrap != null) _scrap.ControlLocked = _previousLock;
            _scrap = null;

            // O proximo da fila, se houver, com o mesmo respiro.
            _readyAt = Time.unscaledTime + OpenDelay;
        }

        private void OnDestroy()
        {
            // Trocar de cena com o card aberto nao pode deixar o jogo parado.
            if (_current.HasValue)
            {
                Time.timeScale = _previousTimeScale;
                if (_scrap != null) _scrap.ControlLocked = _previousLock;
            }

            if (_instance == this) _instance = null;
        }

        private static bool ContinuePressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame
                    || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame
                    || keyboard.jKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
            {
                return true;
            }

            Gamepad pad = Gamepad.current;
            return pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame
                || pad.buttonNorth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame);
        }

        // ------------------------------------------------------------------
        // Desenho
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (!_current.HasValue) return;

            // Por cima de todo o HUD.
            GUI.depth = -100;

            ScrapMove move = _current.Value;
            MoveGuide.Info info = MoveGuide.Get(move);
            float now = Time.unscaledTime;
            float age = now - _openedAt;

            float appear = Mathf.Clamp01(age / AppearTime);
            float leaving = _closingAt >= 0f ? Mathf.Clamp01((now - _closingAt) / CloseTime) : 0f;
            float alpha = Mathf.Clamp01(appear * 2f) * (1f - leaving);

            // O fundo escurece primeiro, inteiro.
            MoveGuide.Alpha = 1f;
            MoveGuide.Fill(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.72f * alpha));

            MoveGuide.Alpha = alpha;

            // --- As medidas ---
            float cardWidth = Mathf.Min(Screen.width - 40f, 800f);
            bool stacked = cardWidth < 640f;
            const float pad = 24f;

            float demoWidth = stacked ? cardWidth - pad * 2f : Mathf.Round(cardWidth * 0.46f);
            float demoHeight = Mathf.Round(demoWidth * 0.58f);
            float textWidth = stacked ? cardWidth - pad * 2f : cardWidth - pad * 3f - demoWidth;

            GUIStyle howStyle = MoveGuide.Style(17, false, TextAnchor.UpperLeft, true);
            float howHeight = 0f;
            foreach (string line in info.HowTo) howHeight += howStyle.CalcHeight(new GUIContent(line), textWidth - 18f) + 8f;

            // Como usar: titulo, teclas, controle e as linhas.
            const float keyHeight = 36f;
            float textBlock = 26f + keyHeight + 12f + 24f + 10f + howHeight;
            float body = stacked ? demoHeight + 18f + textBlock : Mathf.Max(demoHeight, textBlock);

            const float head = 178f;
            const float foot = 64f;
            float cardHeight = head + body + foot;

            var card = new Rect((Screen.width - cardWidth) * 0.5f, (Screen.height - cardHeight) * 0.5f + 26f, cardWidth, cardHeight);

            // --- A entrada: o card cresce passando um pouco do ponto e assenta ---
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = Mathf.LerpUnclamped(0.82f, 1f, EaseOutBack(appear)) * Mathf.Lerp(1f, 0.94f, leaving);
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), card.center);

            var medal = new Vector2(card.center.x, card.y);
            Rays(medal, now, Mathf.Max(cardWidth, 420f) * 0.42f);

            MoveGuide.Box(card);
            MoveGuide.Fill(new Rect(card.x + 6f, card.y + 6f, card.width - 12f, 1f), MoveGuide.GoldDark);
            MoveGuide.Fill(new Rect(card.x + 6f, card.yMax - 7f, card.width - 12f, 1f), MoveGuide.GoldDark);

            Medal(medal, now, age);
            Sparkles(medal, now, cardWidth);

            // --- O cabecalho ---
            float y = card.y + 56f;
            Ribbon(new Rect(card.x, y, card.width, 22f), "HABILIDADE CONQUISTADA");

            y += 30f;
            GUIStyle titleStyle = MoveGuide.Style(stacked ? 30 : 36, true, TextAnchor.MiddleCenter);
            MoveGuide.Text(new Rect(card.x + 2f, y + 2f, card.width, 48f), info.Title, titleStyle, new Color(0f, 0f, 0f, 0.6f));
            MoveGuide.Text(new Rect(card.x, y, card.width, 48f), info.Title, titleStyle, MoveGuide.Gold);

            y += 52f;
            GUIStyle tagStyle = MoveGuide.Style(16, false, TextAnchor.MiddleCenter, false, true);
            MoveGuide.Text(new Rect(card.x + pad, y, card.width - pad * 2f, 24f), info.Tagline, tagStyle, MoveGuide.Faded);

            // --- O corpo: a mini cena e o como usar ---
            float bodyY = card.y + head;
            var demo = new Rect(card.x + pad, bodyY, demoWidth, demoHeight);
            float demoTime = Mathf.Max(0f, age - AppearTime * 0.5f);
            MoveGuide.Demo(move, demo, demoTime);

            float textX = stacked ? card.x + pad : demo.xMax + pad;
            float textY = stacked ? demo.yMax + 18f : bodyY;

            MoveGuide.Text(new Rect(textX, textY, textWidth, 22f), "COMO USAR", MoveGuide.Style(14, true), MoveGuide.Gold);
            textY += 26f;

            MoveGuide.Keys(info.Keys, textX, textY, keyHeight, move, demoTime);
            textY += keyHeight + 12f;

            MoveGuide.Text(new Rect(textX, textY, textWidth, 22f), info.Pad, MoveGuide.Style(14), MoveGuide.Faded);
            textY += 34f;

            foreach (string line in info.HowTo)
            {
                float lineHeight = howStyle.CalcHeight(new GUIContent(line), textWidth - 18f);
                MoveGuide.Fill(new Rect(textX + 2f, textY + 7f, 6f, 6f), MoveGuide.Gold);
                MoveGuide.Text(new Rect(textX + 18f, textY, textWidth - 18f, lineHeight), line, howStyle, MoveGuide.Paper);
                textY += lineHeight + 8f;
            }

            // --- O rodape: como fechar, piscando, so depois do tempo minimo ---
            if (age >= MinOpenTime)
            {
                const string hint = "[Espaço] ou [E] para continuar";
                const float hintHeight = 30f;
                float hintWidth = MoveGuide.Keys(hint, 0f, 0f, hintHeight, null, 0f, false);

                float blink = 0.55f + 0.45f * Mathf.Sin((age - MinOpenTime) * 4f);
                float fadeIn = Mathf.Clamp01((age - MinOpenTime) / 0.3f);
                MoveGuide.Alpha = alpha * fadeIn * blink;
                MoveGuide.Keys(hint, card.center.x - hintWidth * 0.5f, card.yMax - foot + 14f, hintHeight);
                MoveGuide.Alpha = alpha;
            }

            // O clarao da chegada.
            float flash = 1f - Mathf.Clamp01(age / 0.35f);
            if (flash > 0f) MoveGuide.Fill(card, new Color(1f, 0.95f, 0.8f, flash * 0.55f));

            GUI.matrix = previousMatrix;
            MoveGuide.Alpha = 1f;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }

        /// <summary>Os raios dourados girando devagar atras do medalhao.</summary>
        private static void Rays(Vector2 center, float now, float length)
        {
            Matrix4x4 saved = GUI.matrix;
            const int count = 14;

            for (int i = 0; i < count; i++)
            {
                GUI.matrix = saved;
                float angle = now * 12f + i * 360f / count;
                GUIUtility.RotateAroundPivot(angle, center);

                float width = i % 2 == 0 ? 14f : 8f;
                float reach = i % 2 == 0 ? length : length * 0.7f;
                MoveGuide.Fill(new Rect(center.x - width * 0.5f, center.y - reach, width, reach),
                    new Color(MoveGuide.Gold.r, MoveGuide.Gold.g, MoveGuide.Gold.b, 0.10f));
            }

            GUI.matrix = saved;
        }

        /// <summary>
        /// O medalhao em cima do card: um disco dourado com o Scrap parado
        /// dentro, a borda pulsando de leve.
        /// </summary>
        private static void Medal(Vector2 center, float now, float age)
        {
            const float radius = 46f;
            float pulse = 0.5f + 0.5f * Mathf.Sin(now * 3f);

            Disc(center, radius + 8f, new Color(MoveGuide.Gold.r, MoveGuide.Gold.g, MoveGuide.Gold.b, 0.15f + 0.15f * pulse));
            Disc(center, radius, MoveGuide.Gold);
            Disc(center, radius - 4f, MoveGuide.GoldDark);
            Disc(center, radius - 7f, new Color(0.13f, 0.11f, 0.13f, 1f));

            // O Scrap no meio, de frente pro card: o primeiro quadro parado.
            HeroPortrait(center, radius);

            // Um brilho passando em diagonal, logo na chegada.
            float sweep = Mathf.Clamp01((age - 0.3f) / 0.6f);
            if (sweep > 0f && sweep < 1f)
            {
                float x = Mathf.Lerp(center.x - radius, center.x + radius, sweep);
                MoveGuide.Fill(new Rect(x - 6f, center.y - radius + 6f, 12f, (radius - 6f) * 2f), new Color(1f, 1f, 1f, 0.35f * (1f - sweep)));
            }
        }

        /// <summary>
        /// O Scrap parado dentro do medalhao. O quadro tem bastante sobra
        /// transparente em volta do corpo, entao ele pode ser maior que o
        /// disco; os pes ficam um pouco abaixo do centro.
        /// </summary>
        private static void HeroPortrait(Vector2 center, float radius)
        {
            var animator = FindAnyObjectByType<ScrapAnimator>();
            Sprite[] frames = animator != null ? animator.CurrentFrames : null;
            Sprite sprite = frames != null && frames.Length > 0 ? frames[ScrapAnimator.Idle] : null;
            if (sprite == null || sprite.texture == null) return;

            Rect pixels = sprite.rect;
            float scale = Mathf.Max(1f, Mathf.Floor((radius * 2f - 16f) / pixels.height * 1.3f));
            float width = pixels.width * scale;
            float height = pixels.height * scale;
            float feet = center.y + radius * 0.62f;
            var target = new Rect(center.x - width * 0.5f, feet - (pixels.height - sprite.pivot.y) * scale, width, height);

            Texture texture = sprite.texture;
            var uv = new Rect(pixels.x / texture.width, pixels.y / texture.height,
                pixels.width / texture.width, pixels.height / texture.height);

            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, MoveGuide.Alpha);
            GUI.DrawTextureWithTexCoords(target, texture, uv);
            GUI.color = previous;
        }

        /// <summary>Um disco de faixas horizontais: o IMGUI so desenha retangulo.</summary>
        private static void Disc(Vector2 center, float radius, Color color)
        {
            const float step = 2f;
            for (float dy = -radius; dy < radius; dy += step)
            {
                float mid = dy + step * 0.5f;
                float half = Mathf.Sqrt(Mathf.Max(0f, radius * radius - mid * mid));
                MoveGuide.Fill(new Rect(center.x - half, center.y + dy, half * 2f, step), color);
            }
        }

        /// <summary>Faiscas douradas piscando em volta do medalhao.</summary>
        private static void Sparkles(Vector2 center, float now, float width)
        {
            for (int i = 0; i < 10; i++)
            {
                float seed = i * 12.9898f;
                float x = center.x + (Mathf.Repeat(Mathf.Sin(seed) * 43758.55f, 1f) - 0.5f) * width * 0.7f;
                float y = center.y + (Mathf.Repeat(Mathf.Sin(seed * 1.7f) * 24634.63f, 1f) - 0.5f) * 90f;
                float twinkle = Mathf.Max(0f, Mathf.Sin(now * 2.5f + i * 1.3f));
                float size = 2f + 3f * twinkle;

                var color = new Color(1f, 0.92f, 0.65f, twinkle * 0.9f);
                MoveGuide.Fill(new Rect(x - size * 0.5f, y - size * 1.5f, size, size * 3f), color);
                MoveGuide.Fill(new Rect(x - size * 1.5f, y - size * 0.5f, size * 3f, size), color);
            }
        }

        /// <summary>A faixa do cabecalho: o texto entre dois tracos com losangos.</summary>
        private static void Ribbon(Rect row, string text)
        {
            GUIStyle style = MoveGuide.Style(15, true, TextAnchor.MiddleCenter);
            float textWidth = style.CalcSize(new GUIContent(text)).x;
            float lineWidth = Mathf.Min(140f, (row.width - textWidth) * 0.5f - 40f);
            float midY = row.y + row.height * 0.5f;

            MoveGuide.Text(row, text, style, MoveGuide.Gold);

            float leftEnd = row.center.x - textWidth * 0.5f - 14f;
            float rightStart = row.center.x + textWidth * 0.5f + 14f;

            MoveGuide.Fill(new Rect(leftEnd - lineWidth, midY - 1f, lineWidth, 2f), MoveGuide.GoldDark);
            MoveGuide.Fill(new Rect(rightStart, midY - 1f, lineWidth, 2f), MoveGuide.GoldDark);
            Diamond(new Vector2(leftEnd + 4f, midY));
            Diamond(new Vector2(rightStart - 4f, midY));
        }

        private static void Diamond(Vector2 center)
        {
            MoveGuide.Fill(new Rect(center.x - 1f, center.y - 4f, 2f, 8f), MoveGuide.Gold);
            MoveGuide.Fill(new Rect(center.x - 3f, center.y - 2f, 6f, 4f), MoveGuide.Gold);
        }
    }
}
