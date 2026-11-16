using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A troca de fase: a tela fecha em blocos pretos que crescem na diagonal,
    /// o "Carregando" aparece no canto de baixo a direita (com uma engrenagem
    /// de latao girando) enquanto a cena nova carrega por tras, e a tela abre
    /// com a mesma transicao ao contrario.
    ///
    /// Quem troca de cena chama <see cref="Go"/> em vez de SceneManager.LoadScene.
    /// Nasce sozinho na primeira vez e atravessa as cenas.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        private const float CloseTime = 0.55f;
        private const float OpenTime = 0.55f;

        /// <summary>Mesmo carregando rapido, o loading fica pelo menos isto na tela (sem piscar).</summary>
        private const float MinShown = 0.7f;

        private static readonly Color Ink = new Color(0.03f, 0.025f, 0.035f, 1f);
        private static readonly Color TextColor = new Color(0.93f, 0.82f, 0.56f, 1f);

        private static SceneLoader _instance;
        private static Texture2D[] _gear;

        /// <summary>Se ha uma troca de cena acontecendo agora.</summary>
        public static bool Busy => _instance != null && _instance._busy;

        private bool _busy;

        /// <summary>Quanto a tela esta coberta: 0 aberta, 1 toda preta.</summary>
        private float _cover;

        /// <summary>Se a cobertura esta fechando (true) ou abrindo (false): muda o sentido da varredura.</summary>
        private bool _closing;

        /// <summary>O "Carregando": 0 escondido, 1 inteiro.</summary>
        private float _label;
        private GUIStyle _style;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _instance = null;
        }

        /// <summary>
        /// Leva o Scrap pra <paramref name="sceneName"/>, aparecendo no
        /// SpawnPoint <paramref name="spawnId"/>. Com <paramref name="alreadyBlack"/>
        /// (o elevador, que ja escureceu a tela) a tela nao fecha de novo.
        /// </summary>
        public static void Go(string sceneName, string spawnId, bool alreadyBlack = false)
        {
            if (_instance == null)
            {
                var host = new GameObject("SceneLoader");
                DontDestroyOnLoad(host);
                _instance = host.AddComponent<SceneLoader>();
            }

            if (_instance._busy) return;
            _instance.StartCoroutine(_instance.Travel(sceneName, spawnId, alreadyBlack));
        }

        private IEnumerator Travel(string sceneName, string spawnId, bool alreadyBlack)
        {
            _busy = true;
            _closing = true;

            // O Scrap para onde esta enquanto a tela fecha.
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null)
            {
                controller.ControlLocked = true;
                var body = controller.GetComponent<Rigidbody2D>();
                if (body != null) body.linearVelocity = new Vector2(0f, Mathf.Min(0f, body.linearVelocity.y));
            }

            // 1. Fecha.
            if (alreadyBlack)
            {
                _cover = 1f;
            }
            else
            {
                Sfx.CartoonShot.Play(pitch: 0.8f);
                for (float t = 0f; t < CloseTime; t += Time.unscaledDeltaTime)
                {
                    _cover = t / CloseTime;
                    yield return null;
                }

                _cover = 1f;
            }

            // 2. Carrega por tras, com o loading no canto.
            float shownAt = Time.unscaledTime;
            SceneTravel.PendingSpawnId = spawnId;
            AsyncOperation loading = SceneManager.LoadSceneAsync(sceneName);

            if (loading == null)
            {
                Debug.LogError("[SceneLoader] Nao deu pra carregar a cena " + sceneName + ".");
                _busy = false;
                _cover = 0f;
                yield break;
            }

            loading.allowSceneActivation = false;

            while (loading.progress < 0.9f || Time.unscaledTime - shownAt < MinShown)
            {
                _label = Mathf.MoveTowards(_label, 1f, Time.unscaledDeltaTime * 4f);
                yield return null;
            }

            loading.allowSceneActivation = true;
            while (!loading.isDone)
            {
                yield return null;
            }

            // Uns quadros pra cena nova se montar (o chao, a camera no Scrap).
            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                yield return null;
            }

            // 3. O loading some e a tela abre, a mesma varredura ao contrario.
            while (_label > 0f)
            {
                _label = Mathf.MoveTowards(_label, 0f, Time.unscaledDeltaTime * 5f);
                yield return null;
            }

            _closing = false;
            Sfx.CartoonShot.Play(pitch: 1.1f);
            for (float t = 0f; t < OpenTime; t += Time.unscaledDeltaTime)
            {
                _cover = 1f - t / OpenTime;
                yield return null;
            }

            _cover = 0f;
            _busy = false;
        }

        // ------------------------------------------------------------------
        // Desenho
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (_cover <= 0f && _label <= 0f) return;

            // Por cima de tudo, ate da gravura do salvamento.
            GUI.depth = -200;

            DrawCover();
            if (_label > 0f) DrawLoading();
        }

        /// <summary>
        /// A cobertura: uma grade de quadrados pretos que crescem do centro de
        /// cada celula, um atras do outro na diagonal (da esquerda pra direita
        /// ao fechar; ao abrir, somem na mesma ordem).
        /// </summary>
        private void DrawCover()
        {
            float w = Screen.width;
            float h = Screen.height;

            if (_cover >= 1f)
            {
                Fill(new Rect(0f, 0f, w, h), Ink);
                return;
            }

            float cell = Mathf.Ceil(h / 10f);
            int cols = Mathf.CeilToInt(w / cell);
            int rows = Mathf.CeilToInt(h / cell);
            float span = cols + rows * 0.5f;

            // Cada quadrado leva este tanto da transicao pra crescer.
            const float grow = 0.35f;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    float order = (col + row * 0.5f) / span;

                    // Ao fechar, os da esquerda vem primeiro; ao abrir, os da esquerda saem primeiro.
                    float k = _closing
                        ? Mathf.Clamp01((_cover - order * (1f - grow)) / grow)
                        : 1f - Mathf.Clamp01(((1f - _cover) - order * (1f - grow)) / grow);

                    if (k <= 0f) continue;

                    // Em degraus de pixel, como o resto do jogo.
                    float size = Mathf.Ceil(cell * k / 4f) * 4f;
                    float x = col * cell + (cell - size) * 0.5f;
                    float y = row * cell + (cell - size) * 0.5f;
                    Fill(new Rect(x, y, size + 1f, size + 1f), Ink);
                }
            }
        }

        /// <summary>O "Carregando" no canto de baixo a direita, com a engrenagem girando aos trancos.</summary>
        private void DrawLoading()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                    fontStyle = FontStyle.Bold,
                };
            }

            float scale = Mathf.Max(1f, Screen.height / 360f);
            _style.fontSize = Mathf.RoundToInt(9f * scale);

            float gear = 16f * scale;
            float margin = 14f * scale;
            var gearRect = new Rect(Screen.width - margin - gear, Screen.height - margin - gear, gear, gear);

            // Os pontinhos andando.
            int dots = (int)(Time.unscaledTime * 3f) % 4;
            string text = "Carregando" + new string('.', dots) + new string(' ', 3 - dots);
            var textRect = new Rect(gearRect.x - 220f * scale, gearRect.y, 220f * scale - 6f * scale, gear);

            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, _label);

            // A engrenagem: 8 quadros de um dente, trocados aos trancos.
            Texture2D[] frames = GearFrames();
            GUI.DrawTexture(gearRect, frames[(int)(Time.unscaledTime * 12f) % frames.Length]);

            _style.normal.textColor = new Color(TextColor.r, TextColor.g, TextColor.b, _label);
            GUI.Label(textRect, text, _style);

            GUI.color = previous;
        }

        /// <summary>
        /// Uma engrenagem de latao de 16 px, em 8 quadros que giram um dente
        /// inteiro (45 graus): em loop, ela gira sem parar.
        /// </summary>
        private static Texture2D[] GearFrames()
        {
            if (_gear != null) return _gear;

            var dark = new Color32(90, 60, 30, 255);
            var mid = new Color32(198, 152, 70, 255);
            var light = new Color32(240, 206, 124, 255);
            var gem = new Color32(120, 230, 255, 255);
            var clear = new Color32(0, 0, 0, 0);

            const int size = 16;
            const int teeth = 8;
            const int count = 8;
            _gear = new Texture2D[count];

            for (int f = 0; f < count; f++)
            {
                var pixels = new Color32[size * size];
                float turn = f * (360f / teeth) / count;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x + 0.5f - size * 0.5f;
                        float dy = y + 0.5f - size * 0.5f;
                        float r = Mathf.Sqrt(dx * dx + dy * dy);
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg - turn;
                        float phase = Mathf.Repeat(angle, 360f / teeth) / (360f / teeth);

                        Color32 c = clear;
                        if (r < 2.1f) c = gem;
                        else if (r < 3.2f) c = clear;
                        else if (r < 5.6f) c = (dx - dy) < 0f ? light : mid;
                        else if (r < 6.6f) c = dark;
                        else if (r < 7.9f && phase < 0.5f) c = phase < 0.25f ? mid : dark;

                        pixels[x + y * size] = c;
                    }
                }

                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                };
                texture.SetPixels32(pixels);
                texture.Apply();
                _gear[f] = texture;
            }

            return _gear;
        }

        private static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
