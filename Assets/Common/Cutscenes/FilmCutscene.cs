using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Toca um filme de camadas (Film.cs) em tela cheia, por cima do jogo:
    /// planos com camera, animacao, particulas, claroes e som, com faixas
    /// pretas de cinema. Os quadros vem do editor (FilmAssets), legiveis.
    ///
    /// Termina de um de dois jeitos:
    ///   endWhite   sobe ao branco; no pico chama onPeak e desbota sobre o
    ///              jogo (a transformacao do Rei: ele ja e o outro).
    ///   no preto   chama onPeak no preto e segura o preto um pouco,
    ///              desbotando devagar (o cartao de FIM entra por baixo).
    ///
    /// Pular (espaco, J, X, Enter ou A/X no controle) depois do primeiro
    /// segundo vai pro fim do ultimo plano.
    /// </summary>
    public class FilmCutscene : MonoBehaviour
    {
        private const int Largura = FilmCompositor.Largura;
        private const int Altura = FilmCompositor.Altura;
        private const float FracaoBarra = 0.068f;
        private const float PularDepois = 1f;
        private const float SubidaBranco = 0.5f;
        private const float EsmaecerBranco = 0.5f;
        private const float SegurarPreto = 2.2f;

        [UnityEngine.Serialization.FormerlySerializedAs("shots")] [SerializeField] private FilmShot[] planos;
        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [SerializeField] private Texture2D[] quadros;
        [UnityEngine.Serialization.FormerlySerializedAs("endWhite")] [SerializeField] private bool brancoFim = true;

        private List<FilmFrame> _quadros;
        private uint[] _reserva;
        private Color32[] _cores;
        private Texture2D _tela;
        private bool _painelMostrar;
        private float _branco;
        private float _preto;

        /// <summary>Se algum filme esta na tela.</summary>
        public static bool EstaTocando { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarEstaticos()
        {
            EstaTocando = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(FilmShot[] novoPlanos, Texture2D[] novoQuadros, bool novoBrancoFim)
        {
            planos = novoPlanos;
            quadros = novoQuadros;
            brancoFim = novoBrancoFim;
        }

        public bool EstaProntoUso
        {
            get
            {
                if (planos == null || planos.Length == 0 || quadros == null || quadros.Length == 0) return false;
                foreach (Texture2D quadro in quadros)
                {
                    if (quadro == null || !quadro.isReadable) return false;
                }

                return true;
            }
        }

        public IEnumerator Tocar(Action ligadoPico)
        {
            bool atingiuPico = false;

            void PeakOnce()
            {
                if (atingiuPico) return;
                atingiuPico = true;
                ligadoPico?.Invoke();
            }

            if (!EstaProntoUso)
            {
                PeakOnce();
                yield break;
            }

            Preparar();
            EstaTocando = true;
            _painelMostrar = true;
            _branco = 0f;
            _preto = 0f;

            int ultimo = planos.Length - 1;
            int indice = 0;
            float t = 0f;
            float decorrido = 0f;
            float ultimoT = -1f;
            bool pulado = false;

            while (indice < planos.Length)
            {
                FilmShot plano = planos[indice];

                if (!pulado && decorrido > PularDepois && PularPressionado())
                {
                    pulado = true;
                    indice = ultimo;
                    plano = planos[ultimo];
                    t = Mathf.Max(0f, plano.duracao - (brancoFim ? SubidaBranco + 0.2f : 1f));
                    ultimoT = t;
                }

                if (!pulado) TocarDeixas(plano, ultimoT, t);
                ultimoT = t;

                if (brancoFim && indice == ultimo)
                {
                    float subida = (t - (plano.duracao - SubidaBranco)) / SubidaBranco;
                    _branco = Mathf.Clamp01(subida);
                    if (subida > 0f && subida - Time.deltaTime / SubidaBranco <= 0f) Sfx.ClaraoDesenho.Tocar();
                }

                FilmCompositor.Renderizar(plano, _quadros, t, _reserva);
                EnviarTextura();

                yield return null;

                t += Time.deltaTime;
                decorrido += Time.deltaTime;

                if (t >= plano.duracao)
                {
                    indice++;
                    t = 0f;
                    ultimoT = -1f;
                }
            }

            if (brancoFim)
            {
                _branco = 1f;
                PeakOnce();
                _painelMostrar = false;
                for (float f = 0f; f < EsmaecerBranco; f += Time.deltaTime)
                {
                    _branco = 1f - f / EsmaecerBranco;
                    yield return null;
                }
            }
            else
            {
                _preto = 1f;
                _painelMostrar = false;
                PeakOnce();
                StartCoroutine(LiberarTelaPreta());
                yield break;
            }

            Parar();
        }

        private IEnumerator LiberarTelaPreta()
        {
            for (float f = 0f; f < SegurarPreto; f += Time.deltaTime)
            {
                _preto = 1f - f / SegurarPreto;
                yield return null;
            }

            Parar();
        }

        private static readonly Dictionary<string, SfxCue> Sons = new Dictionary<string, SfxCue>();

        private static void TocarDeixas(FilmShot plano, float de, float to)
        {
            if (plano.deixas == null) return;

            foreach (FilmCue deixa in plano.deixas)
            {
                if (deixa == null || string.IsNullOrEmpty(deixa.som)) continue;
                if (deixa.t <= de || deixa.t > to) continue;
                if (de < 0f && deixa.t < 0f) continue;

                SfxCue som = Encontrar(deixa.som);
                som?.Tocar(null, deixa.volume, deixa.tom2);
            }
        }

        private static SfxCue Encontrar(string nome)
        {
            if (Sons.TryGetValue(nome, out SfxCue guardado)) return guardado;
            FieldInfo campo = typeof(Sfx).GetField(nome, BindingFlags.Public | BindingFlags.Static);
            SfxCue deixa = campo != null ? campo.GetValue(null) as SfxCue : null;
            if (deixa == null) Debug.LogWarning($"[Film] Som '{nome}' nao existe em Sfx.");
            Sons[nome] = deixa;
            return deixa;
        }

        private void Preparar()
        {
            if (_quadros != null) return;

            _quadros = new List<FilmFrame>(quadros.Length);
            foreach (Texture2D textura in quadros)
            {
                Color32[] cores = textura.GetPixels32();
                int w = textura.width, h = textura.height;
                var pixels = new uint[w * h];
                for (int y = 0; y < h; y++)
                {
                    int origem = (h - 1 - y) * w;
                    int destino = y * w;
                    for (int x = 0; x < w; x++)
                    {
                        Color32 c = cores[origem + x];
                        pixels[destino + x] = (uint)c.a << 24 | (uint)c.r << 16 | (uint)c.g << 8 | c.b;
                    }
                }

                _quadros.Add(new FilmFrame(w, h, pixels));
            }

            _reserva = new uint[Largura * Altura];
            _cores = new Color32[Largura * Altura];
            _tela = new Texture2D(Largura, Altura, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
        }

        private void EnviarTextura()
        {
            for (int y = 0; y < Altura; y++)
            {
                int origem = y * Largura;
                int destino = (Altura - 1 - y) * Largura;
                for (int x = 0; x < Largura; x++)
                {
                    uint p = _reserva[origem + x];
                    _cores[destino + x] = new Color32((byte)(p >> 16), (byte)(p >> 8), (byte)p, 255);
                }
            }

            _tela.SetPixels32(_cores);
            _tela.Apply(false);
        }

        private void Parar()
        {
            _branco = 0f;
            _preto = 0f;
            _painelMostrar = false;
            EstaTocando = false;
        }

        private void OnDisable()
        {
            Parar();
        }

        private void OnDestroy()
        {
            EstaTocando = false;
            if (_tela != null) Destroy(_tela);
        }

        private static bool PularPressionado()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.spaceKey.wasPressedThisFrame || teclado.jKey.wasPressedThisFrame
                                     || teclado.xKey.wasPressedThisFrame || teclado.enterKey.wasPressedThisFrame))
            {
                return true;
            }

            Gamepad controle = Gamepad.current;
            return controle != null && (controle.buttonSouth.wasPressedThisFrame || controle.buttonWest.wasPressedThisFrame);
        }

        private void OnGUI()
        {
            if (!EstaTocando) return;
            if (Event.current.type != EventType.Repaint) return;

            GUI.depth = -1000;
            Color valorAnterior = GUI.color;
            var cheio = new Rect(0f, 0f, Screen.width, Screen.height);

            if (_painelMostrar)
            {
                Preencher(cheio, Color.black);

                float escalaAtual = Mathf.Min(Screen.width / (float)Largura, Screen.height / (float)Altura);
                float w = Largura * escalaAtual, h = Altura * escalaAtual;
                var painel = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

                if (_tela != null)
                {
                    GUI.color = Color.white;
                    TelaCanvas.Textura(painel, _tela);
                }

                float barra = h * FracaoBarra;
                Preencher(new Rect(painel.x, painel.y, w, barra), Color.black);
                Preencher(new Rect(painel.x, painel.yMax - barra, w, barra), Color.black);
            }

            if (_preto > 0f) Preencher(cheio, new Color(0f, 0f, 0f, _preto));
            if (_branco > 0f) Preencher(cheio, new Color(1f, 0.98f, 0.92f, _branco));

            GUI.color = valorAnterior;
        }

        private static void Preencher(Rect retangulo, Color cor)
        {
            GUI.color = cor;
            TelaCanvas.Textura(retangulo, Texture2D.whiteTexture);
        }
    }
}
