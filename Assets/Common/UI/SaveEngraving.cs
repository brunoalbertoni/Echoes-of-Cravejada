using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O aviso de salvo, sem frase nenhuma: a Bancada de Ourives grava o
    /// momento numa placa de latao.
    ///
    /// Na hora do salvamento a tela e fotografada (sem o HUD). A foto congela
    /// grande no meio da tela, num clarao; vira gravura (tons de latao,
    /// pontilhado e riscos de buril) dentro de uma moldura rebitada com a
    /// legenda "GRAVURA N. X" e o nome da area; e voa pro canto, onde carimba
    /// e fica um instante antes de sumir.
    ///
    /// Nasce sozinha na primeira vez em que e pedida.
    /// </summary>
    public class SaveEngraving : MonoBehaviour
    {
        private const int PlateWidth = 240;

        private const float FlashTime = 0.45f;
        private const float EtchFrom = 0.15f;
        private const float EtchTo = 0.8f;
        private const float FlyFrom = 1.55f;
        private const float FlyTo = 2.1f;
        private const float StayUntil = 4.4f;
        private const float GoneAt = 5f;

        private static readonly Color32[] BrassTones =
        {
            new Color32(30, 19, 12, 255),
            new Color32(78, 52, 28, 255),
            new Color32(138, 98, 46, 255),
            new Color32(196, 150, 72, 255),
            new Color32(248, 220, 150, 255),
        };

        private static readonly Dictionary<string, string> Areas = new Dictionary<string, string>
        {
            { "ScrapYard", "Ferro-Velho" },
            { "BrassPipelines", "Tubulações de Latão" },
            { "SilentGeodes", "Geodos Silenciosos" },
            { "AlchemicalAteliers", "Ateliês Alquímicos" },
            { "SilverCity", "Cidade de Prata" },
        };

        private static readonly int[] Bayer = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

        private static SaveEngraving _instance;

        /// <summary>Enquanto a tela e fotografada, o HUD nao se desenha.</summary>
        public static bool HideHud { get; private set; }

        /// <summary>Quantas gravuras ja foram feitas na sessao (o numero da placa).</summary>
        public static int Count { get; private set; }

        private Texture2D _photo;
        private Texture2D _engraved;
        private float _start = -100f;
        private string _area;
        private int _number;
        private bool _stamped;
        private GUIStyle _caption;
        private Texture2D _savedTag;
        private bool _savedTagLoaded;

        // O "jogo salvo" pequeno e meio transparente no canto de cima a direita.
        private const float SavedTagFor = 3.5f;
        private const float SavedTagFade = 0.8f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _instance = null;
            HideHud = false;
            Count = 0;
        }

        /// <summary>Grava o momento: fotografa a tela no fim deste quadro e comeca a cena da placa.</summary>
        public static void Engrave()
        {
            if (_instance == null)
            {
                var host = new GameObject("SaveEngraving");
                DontDestroyOnLoad(host);
                _instance = host.AddComponent<SaveEngraving>();
            }

            _instance.StopAllCoroutines();
            _instance.StartCoroutine(_instance.Capture());
        }

        private IEnumerator Capture()
        {
            // Um quadro sem HUD (nem a placa anterior), e a foto no fim dele.
            HideHud = true;
            yield return null;
            yield return new WaitForEndOfFrame();

            Texture2D shot = ScreenCapture.CaptureScreenshotAsTexture();
            HideHud = false;

            if (shot == null) yield break;

            Release();
            Build(shot);
            Destroy(shot);

            Count++;
            _number = Count;
            string scene = SceneManager.GetActiveScene().name;
            _area = scene;
            foreach (KeyValuePair<string, string> area in Areas)
            {
                if (scene.EndsWith(area.Key)) _area = area.Value;
            }
            _start = Time.unscaledTime;
            _stamped = false;

            Sfx.EngraveEtch.Play(delay: EtchFrom);
        }

        private void OnDestroy()
        {
            Release();
            if (_instance == this) _instance = null;
        }

        private void Release()
        {
            if (_photo != null) Destroy(_photo);
            if (_engraved != null) Destroy(_engraved);
            _photo = null;
            _engraved = null;
        }

        // ------------------------------------------------------------------
        // A gravura
        // ------------------------------------------------------------------

        /// <summary>A foto reduzida (pixelada como o jogo) e a mesma foto gravada em latao.</summary>
        private void Build(Texture2D shot)
        {
            int width = PlateWidth;
            int height = Mathf.Max(1, Mathf.RoundToInt(PlateWidth * shot.height / (float)shot.width));

            Color32[] source = shot.GetPixels32();
            var photo = new Color32[width * height];
            var engraved = new Color32[width * height];

            float stepX = shot.width / (float)width;
            float stepY = shot.height / (float)height;
            int box = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(stepX, stepY)));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // A media de um quadradinho da tela.
                    int sx0 = Mathf.FloorToInt(x * stepX);
                    int sy0 = Mathf.FloorToInt(y * stepY);
                    int r = 0, g = 0, b = 0, n = 0;
                    for (int oy = 0; oy < box; oy += 2)
                    {
                        int sy = Mathf.Min(shot.height - 1, sy0 + oy);
                        for (int ox = 0; ox < box; ox += 2)
                        {
                            Color32 c = source[Mathf.Min(shot.width - 1, sx0 + ox) + sy * shot.width];
                            r += c.r;
                            g += c.g;
                            b += c.b;
                            n++;
                        }
                    }

                    var average = new Color32((byte)(r / n), (byte)(g / n), (byte)(b / n), 255);
                    photo[x + y * width] = average;

                    // Luz, com contraste e as bordas mais escuras.
                    float light = (0.3f * average.r + 0.59f * average.g + 0.11f * average.b) / 255f;
                    light = Mathf.Clamp01((light - 0.06f) / 0.62f);
                    float u = x / (float)width - 0.5f;
                    float v = y / (float)height - 0.5f;
                    light *= 1f - 1.1f * (u * u + v * v);

                    // Pontilhado ordenado: cinco tons de latao.
                    float threshold = (Bayer[(x & 3) + (y & 3) * 4] + 0.5f) / 16f - 0.5f;
                    int level = Mathf.Clamp(Mathf.FloorToInt(light * 4f + 0.5f + threshold * 0.9f), 0, 4);

                    // Os riscos do buril nas sombras, de duas em duas linhas.
                    if (light < 0.5f && y % 3 == 0 && level > 0) level--;

                    engraved[x + y * width] = BrassTones[level];
                }
            }

            _photo = MakeTexture(width, height, photo);
            _engraved = MakeTexture(width, height, engraved);
        }

        private static Texture2D MakeTexture(int width, int height, Color32[] pixels)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        // ------------------------------------------------------------------
        // A cena da placa
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (HideHud || _engraved == null) return;

            DrawSavedTag();

            float age = Time.unscaledTime - _start;
            if (age < 0f || age > GoneAt) return;

            // Por cima do HUD.
            GUI.depth = -100;

            float screenW = Screen.width;
            float screenH = Screen.height;
            float aspect = _engraved.height / (float)_engraved.width;

            // Grande no meio; pequena no canto de baixo a direita.
            float bigWidth = Mathf.Min(screenW * 0.6f, screenH * 0.62f / aspect);
            float smallWidth = Mathf.Max(170f, screenW * 0.17f);

            var bigCenter = new Vector2(screenW * 0.5f, screenH * 0.46f);
            float smallHeight = PlateHeight(smallWidth, aspect);
            var smallCenter = new Vector2(screenW - 24f - smallWidth * 0.5f, screenH - 24f - smallHeight * 0.5f);

            float fly = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(FlyFrom, FlyTo, age));
            float width = Mathf.Lerp(bigWidth, smallWidth, fly);
            Vector2 center = Vector2.Lerp(bigCenter, smallCenter, fly);

            // Uma arqueada no caminho, como se fosse jogada.
            center.y -= Mathf.Sin(fly * Mathf.PI) * screenH * 0.08f;

            // Ao chegar no canto: o carimbo.
            if (age >= FlyTo && !_stamped)
            {
                _stamped = true;
                Sfx.EngraveStamp.Play();
            }

            float punch = age >= FlyTo ? 1f + 0.14f * Mathf.Exp(-(age - FlyTo) * 14f) * Mathf.Cos((age - FlyTo) * 30f) : 1f;
            width *= punch;

            // No meio ela pende pra um lado; no canto, pro outro.
            float tilt = Mathf.Lerp(-2.5f * Mathf.SmoothStep(0f, 1f, age / 0.8f), 3f, fly);

            float alpha = 1f - Mathf.InverseLerp(StayUntil, GoneAt, age);
            float etch = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(EtchFrom, EtchTo, age));

            Matrix4x4 previous = GUI.matrix;
            GUIUtility.RotateAroundPivot(tilt, center);
            DrawPlate(center, width, aspect, etch, alpha);
            GUI.matrix = previous;

            // O clarao da gema, por cima de tudo, no comeco.
            float flash = 1f - Mathf.Clamp01(age / FlashTime);
            if (flash > 0f) Fill(new Rect(0f, 0f, screenW, screenH), new Color(1f, 0.98f, 0.9f, flash * flash));
        }

        private void DrawSavedTag()
        {
            float age = Time.unscaledTime - _start;
            if (age < 0f || age > SavedTagFor) return;

            if (!_savedTagLoaded)
            {
                _savedTagLoaded = true;
                _savedTag = Resources.Load<Texture2D>("PauseScreen/saved");
            }

            float alpha = 0.55f * Mathf.Clamp01(age / 0.3f) * (1f - Mathf.InverseLerp(SavedTagFor - SavedTagFade, SavedTagFor, age));
            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);

            if (_savedTag != null)
            {
                int scale = Mathf.Max(1, Mathf.FloorToInt(Screen.height / 360f));
                float tw = _savedTag.width * scale, th = _savedTag.height * scale;
                GUI.DrawTexture(new Rect(Screen.width - 16f - tw, 14f, tw, th), _savedTag);
            }
            else
            {
                GUI.Label(new Rect(Screen.width - 160f, 12f, 148f, 24f), "jogo salvo");
            }

            GUI.color = previous;
        }

        private static float PlateHeight(float width, float aspect)
        {
            float border = Mathf.Round(width * 0.04f);
            return width * aspect + border * 2f + CaptionHeight(width);
        }

        private static float CaptionHeight(float width) => Mathf.Max(16f, Mathf.Round(width * 0.085f));

        /// <summary>A placa: moldura de latao rebitada, a imagem e a faixa com a legenda.</summary>
        private void DrawPlate(Vector2 center, float width, float aspect, float etch, float alpha)
        {
            float border = Mathf.Max(4f, Mathf.Round(width * 0.04f));
            float imageW = width - border * 2f;
            float imageH = imageW * aspect;
            float captionH = CaptionHeight(width);
            float height = imageH + border * 2f + captionH;

            var outer = new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height);

            // A sombra no chao da tela, a moldura e o friso.
            Fill(new Rect(outer.x + 6f, outer.y + 8f, outer.width, outer.height), new Color(0f, 0f, 0f, 0.45f * alpha));
            Fill(outer, Brass(0, alpha));
            Fill(new Rect(outer.x + 2f, outer.y + 2f, outer.width - 4f, outer.height - 4f), Brass(2, alpha));
            Fill(new Rect(outer.x + 2f, outer.y + 2f, outer.width - 4f, 2f), Brass(4, alpha));
            Fill(new Rect(outer.x + 2f, outer.yMax - 4f, outer.width - 4f, 2f), Brass(1, alpha));

            // A imagem: a foto virando gravura.
            var image = new Rect(outer.x + border, outer.y + border, imageW, imageH);
            Fill(new Rect(image.x - 2f, image.y - 2f, image.width + 4f, image.height + 4f), Brass(0, alpha));

            Color tint = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha * (1f - etch));
            if (_photo != null && etch < 1f) GUI.DrawTexture(image, _photo);
            GUI.color = new Color(1f, 1f, 1f, alpha * etch);
            GUI.DrawTexture(image, _engraved);
            GUI.color = tint;

            // Os rebites nos quatro cantos.
            float rivet = Mathf.Max(3f, Mathf.Round(border * 0.5f));
            float inset = (border - rivet) * 0.5f;
            Rivet(new Vector2(outer.x + inset, outer.y + inset), rivet, alpha);
            Rivet(new Vector2(outer.xMax - inset - rivet, outer.y + inset), rivet, alpha);
            Rivet(new Vector2(outer.x + inset, outer.yMax - inset - rivet), rivet, alpha);
            Rivet(new Vector2(outer.xMax - inset - rivet, outer.yMax - inset - rivet), rivet, alpha);

            // A legenda, gravada fundo na faixa de baixo.
            if (_caption == null)
            {
                _caption = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, clipping = TextClipping.Clip };
            }

            _caption.fontSize = Mathf.Max(9, Mathf.RoundToInt(captionH * 0.55f));
            var strip = new Rect(image.x, image.yMax + 2f, image.width, captionH);
            string text = $"GRAVURA Nº {_number}  ·  {_area.ToUpperInvariant()}";

            _caption.normal.textColor = Brass(4, alpha * 0.8f);
            GUI.Label(new Rect(strip.x + 1f, strip.y + 1f, strip.width, strip.height), text, _caption);
            _caption.normal.textColor = Brass(0, alpha);
            GUI.Label(strip, text, _caption);
        }

        private static void Rivet(Vector2 at, float size, float alpha)
        {
            Fill(new Rect(at.x, at.y, size, size), Brass(0, alpha));
            Fill(new Rect(at.x, at.y, size - 1f, size - 1f), Brass(4, alpha));
            Fill(new Rect(at.x + 1f, at.y + 1f, size - 2f, size - 2f), Brass(3, alpha));
        }

        private static Color Brass(int tone, float alpha)
        {
            Color32 c = BrassTones[tone];
            return new Color(c.r / 255f, c.g / 255f, c.b / 255f, alpha);
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
