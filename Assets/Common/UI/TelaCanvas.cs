using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A interface do jogo em Canvas. As telas calculam posicao, cor e animacao
    /// a cada quadro e pedem aqui o que mostrar; imagens viram RawImage e textos
    /// viram TextMeshPro. Cada GUI.depth vira um Canvas, na mesma ordem.
    /// </summary>
    public class TelaCanvas : MonoBehaviour
    {
        private sealed class Camada
        {
            public Canvas Canvas;
            public readonly List<RawImage> Imagens = new List<RawImage>();
            public readonly List<TextMeshProUGUI> Textos = new List<TextMeshProUGUI>();
            public int ImagensUsadas;
            public int TextosUsados;
            public int Ordem;
        }

        private static TelaCanvas _instancia;
        private static readonly Stack<Rect> Recortes = new Stack<Rect>();

        private readonly Dictionary<int, Camada> _camadas = new Dictionary<int, Camada>();
        private bool _repintou;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _instancia = null;
            Recortes.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Inicializar()
        {
            if (_instancia != null) return;

            var objeto = new GameObject("TelaCanvas");
            DontDestroyOnLoad(objeto);
            _instancia = objeto.AddComponent<TelaCanvas>();
        }

        private void OnEnable()
        {
            StartCoroutine(LimparFimDoQuadro());
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint) _repintou = true;
        }

        /// <summary>Esconde o que nao foi pedido neste quadro: uma tela fechada some sozinha.</summary>
        private IEnumerator LimparFimDoQuadro()
        {
            var fimDoQuadro = new WaitForEndOfFrame();

            while (true)
            {
                yield return fimDoQuadro;
                if (!_repintou) continue;
                _repintou = false;

                foreach (Camada camada in _camadas.Values)
                {
                    for (int i = camada.ImagensUsadas; i < camada.Imagens.Count; i++)
                    {
                        if (camada.Imagens[i].enabled) camada.Imagens[i].enabled = false;
                    }

                    for (int i = camada.TextosUsados; i < camada.Textos.Count; i++)
                    {
                        if (camada.Textos[i].enabled) camada.Textos[i].enabled = false;
                    }

                    camada.ImagensUsadas = 0;
                    camada.TextosUsados = 0;
                    camada.Ordem = 0;
                }
            }
        }

        // ------------------------------------------------------------------
        // O que as telas pedem
        // ------------------------------------------------------------------

        public static void Textura(Rect area, Texture textura) => TexturaUV(area, textura, new Rect(0f, 0f, 1f, 1f));

        public static void Textura(Rect area, Texture textura, ScaleMode modo)
        {
            if (textura != null && modo == ScaleMode.ScaleToFit)
            {
                float escala = Mathf.Min(area.width / textura.width, area.height / textura.height);
                var tamanho = new Vector2(textura.width * escala, textura.height * escala);
                area = new Rect(area.center - tamanho * 0.5f, tamanho);
            }

            Textura(area, textura);
        }

        public static void TexturaUV(Rect area, Texture textura, Rect uv, bool comTransparencia) => TexturaUV(area, textura, uv);

        public static void TexturaUV(Rect area, Texture textura, Rect uv)
        {
            if (!PodeDesenhar() || textura == null) return;
            if (!AplicarRecorte(ref area, ref uv)) return;

            Camada camada = _instancia.CamadaAtual();
            RawImage imagem = _instancia.ProximaImagem(camada);
            imagem.texture = textura;
            imagem.uvRect = uv;
            imagem.color = GUI.color;
            Posicionar(imagem.rectTransform, area, camada);
        }

        public static void Texto(Rect area, string texto) => Texto(area, texto, GUI.skin.label);

        public static void Texto(Rect area, GUIContent conteudo, GUIStyle estilo) => Texto(area, conteudo != null ? conteudo.text : "", estilo);

        public static void Texto(Rect area, string texto, GUIStyle estilo)
        {
            if (!PodeDesenhar() || string.IsNullOrEmpty(texto)) return;
            if (Recortes.Count > 0 && !Recortes.Peek().Overlaps(area)) return;

            Camada camada = _instancia.CamadaAtual();
            TextMeshProUGUI rotulo = _instancia.ProximoTexto(camada);
            rotulo.text = texto;
            rotulo.fontSize = estilo.fontSize > 0 ? estilo.fontSize : 14;
            rotulo.fontStyle = Estilo(estilo.fontStyle);
            rotulo.alignment = Alinhamento(estilo.alignment);
            rotulo.textWrappingMode = estilo.wordWrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            rotulo.overflowMode = TextOverflowModes.Overflow;
            rotulo.richText = estilo.richText;
            rotulo.color = estilo.normal.textColor * GUI.color * GUI.contentColor;
            Posicionar(rotulo.rectTransform, area, camada);
        }

        /// <summary>Limita o que for desenhado a uma area, como o GUI.BeginGroup.</summary>
        public static void AbrirRecorte(Rect area)
        {
            Vector2 inicio = GUIUtility.GUIToScreenPoint(area.position);
            Recortes.Push(new Rect(inicio, area.size));
        }

        public static void FecharRecorte()
        {
            if (Recortes.Count > 0) Recortes.Pop();
        }

        // ------------------------------------------------------------------
        // Por dentro
        // ------------------------------------------------------------------

        private static bool PodeDesenhar() => _instancia != null && Event.current != null && Event.current.type == EventType.Repaint;

        private static bool AplicarRecorte(ref Rect area, ref Rect uv)
        {
            if (Recortes.Count == 0) return true;

            Vector2 inicio = GUIUtility.GUIToScreenPoint(area.position);
            var naTela = new Rect(inicio, area.size);
            Rect limite = Recortes.Peek();
            if (!naTela.Overlaps(limite)) return false;

            float xMin = Mathf.Max(naTela.xMin, limite.xMin);
            float xMax = Mathf.Min(naTela.xMax, limite.xMax);
            float yMin = Mathf.Max(naTela.yMin, limite.yMin);
            float yMax = Mathf.Min(naTela.yMax, limite.yMax);

            // O V da textura cresce pra cima; a tela, pra baixo.
            var novoUv = new Rect(
                uv.x + uv.width * (xMin - naTela.xMin) / naTela.width,
                uv.y + uv.height * (naTela.yMax - yMax) / naTela.height,
                uv.width * (xMax - xMin) / naTela.width,
                uv.height * (yMax - yMin) / naTela.height);

            area = new Rect(area.x + xMin - naTela.xMin, area.y + yMin - naTela.yMin, xMax - xMin, yMax - yMin);
            uv = novoUv;
            return true;
        }

        private Camada CamadaAtual()
        {
            int profundidade = GUI.depth;
            if (_camadas.TryGetValue(profundidade, out Camada camada)) return camada;

            var objeto = new GameObject("Camada " + profundidade, typeof(RectTransform));
            objeto.transform.SetParent(transform, false);
            var canvas = objeto.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000 - profundidade;
            var escalador = objeto.AddComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            escalador.scaleFactor = 1f;

            camada = new Camada { Canvas = canvas };
            _camadas[profundidade] = camada;
            return camada;
        }

        private RawImage ProximaImagem(Camada camada)
        {
            if (camada.ImagensUsadas >= camada.Imagens.Count)
            {
                var objeto = new GameObject("Imagem", typeof(RectTransform));
                objeto.transform.SetParent(camada.Canvas.transform, false);
                var nova = objeto.AddComponent<RawImage>();
                nova.raycastTarget = false;
                camada.Imagens.Add(nova);
            }

            RawImage imagem = camada.Imagens[camada.ImagensUsadas++];
            imagem.enabled = true;
            return imagem;
        }

        private TextMeshProUGUI ProximoTexto(Camada camada)
        {
            if (camada.TextosUsados >= camada.Textos.Count)
            {
                var objeto = new GameObject("Texto", typeof(RectTransform));
                objeto.transform.SetParent(camada.Canvas.transform, false);
                var novo = objeto.AddComponent<TextMeshProUGUI>();
                novo.raycastTarget = false;
                camada.Textos.Add(novo);
            }

            TextMeshProUGUI texto = camada.Textos[camada.TextosUsados++];
            texto.enabled = true;
            return texto;
        }

        /// <summary>Leva a area da tela (com grupo, escala e rotacao do GUI) para o Canvas.</summary>
        private static void Posicionar(RectTransform alvo, Rect area, Camada camada)
        {
            Vector2 origem = GUIUtility.GUIToScreenPoint(new Vector2(area.xMin, area.yMin));
            Vector2 direita = GUIUtility.GUIToScreenPoint(new Vector2(area.xMax, area.yMin));
            Vector2 baixo = GUIUtility.GUIToScreenPoint(new Vector2(area.xMin, area.yMax));

            alvo.anchorMin = new Vector2(0f, 1f);
            alvo.anchorMax = new Vector2(0f, 1f);
            alvo.pivot = new Vector2(0f, 1f);
            alvo.anchoredPosition = new Vector2(origem.x, -origem.y);
            alvo.sizeDelta = new Vector2(Vector2.Distance(origem, direita), Vector2.Distance(origem, baixo));

            Vector2 eixo = direita - origem;
            float angulo = eixo.sqrMagnitude > 0f ? Mathf.Atan2(eixo.y, eixo.x) * Mathf.Rad2Deg : 0f;
            alvo.localRotation = Quaternion.Euler(0f, 0f, -angulo);

            // A ordem dos pedidos e a ordem na tela, misturando imagens e textos.
            alvo.SetSiblingIndex(Mathf.Min(camada.Ordem++, alvo.parent.childCount - 1));
        }

        private static FontStyles Estilo(FontStyle estilo)
        {
            switch (estilo)
            {
                case FontStyle.Bold: return FontStyles.Bold;
                case FontStyle.Italic: return FontStyles.Italic;
                case FontStyle.BoldAndItalic: return FontStyles.Bold | FontStyles.Italic;
                default: return FontStyles.Normal;
            }
        }

        private static TextAlignmentOptions Alinhamento(TextAnchor ancora)
        {
            switch (ancora)
            {
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.TopLeft;
            }
        }
    }
}
