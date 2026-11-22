using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>Os movimentos que o jogo ensina: as habilidades conquistadas e os das placas de tutorial.</summary>
    public enum ScrapMove
    {
        DashVapor,
        PuloPressao,
        GolpeRessonancia,
        PuloParede,
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
            public string Titulo;
            public string Frase;

            /// <summary>As teclas: o que esta entre colchetes vira tecla desenhada, o resto vira texto.</summary>
            public string Chaves;

            public string Controle;
            public string[] ComoPara;

            /// <summary>Quanto dura uma volta da mini cena, em segundos.</summary>
            public float Repeticao;
        }

        private static readonly Dictionary<ScrapMove, Info> Infos = new Dictionary<ScrapMove, Info>
        {
            [ScrapMove.DashVapor] = new Info
            {
                Titulo = "Sobrecarga de Velocidade",
                Frase = "O motor do Coletor agora pulsa nas suas pernas.",
                Chaves = "[Shift] ou [L]",
                Controle = "Controle: RB",
                ComoPara = new[]
                {
                    "Aperte para arrancar reto para a frente.",
                    "No ar o arranque não cai: use no meio do pulo para cruzar vãos largos.",
                    "Um arranque por pulo. As costas esquentam enquanto ele recarrega.",
                },
                Repeticao = 3f,
            },
            [ScrapMove.PuloPressao] = new Info
            {
                Titulo = "Salto de Pressão",
                Frase = "A caldeira do Mestre agora sopra nos seus pistões.",
                Chaves = "[Espaço] de novo, no ar",
                Controle = "Controle: A, no ar",
                ComoPara = new[]
                {
                    "Pule e, ainda no ar, pule outra vez: uma rajada de vapor empurra você para cima.",
                    "O segundo pulo volta quando você pisa no chão ou encosta numa parede.",
                },
                Repeticao = 3f,
            },
            [ScrapMove.GolpeRessonancia] = new Info
            {
                Titulo = "Golpe Ressonante",
                Frase = "O cinzel aprendeu a cantar como cristal.",
                Chaves = "Segure [J] e solte",
                Controle = "Controle: segure X e solte",
                ComoPara = new[]
                {
                    "Segure o golpe até os olhos brilharem em azul, depois solte.",
                    "A onda fere tudo em volta e estilhaça barreiras de cristal.",
                    "É o golpe mais barulhento que você tem: tudo por perto vai ouvir.",
                },
                Repeticao = 3.2f,
            },
            [ScrapMove.PuloParede] = new Info
            {
                Titulo = "Pulo de Parede",
                Frase = "Onde não há chão, há parede.",
                Chaves = "[←] [→] + [Espaço]",
                Controle = "Controle: direção + A",
                ComoPara = new[]
                {
                    "Pule contra uma parede e, encostado nela, pule de novo para saltar para o outro lado.",
                    "Segure a direção da parede para escorregar devagar.",
                    "Numa chaminé estreita, vá alternando as paredes para subir.",
                },
                Repeticao = 2f,
            },
            [ScrapMove.Pogo] = new Info
            {
                Titulo = "Golpe para Baixo",
                Frase = "Espinho também serve de degrau.",
                Chaves = "No ar: [↓] + [J]",
                Controle = "Controle: baixo + X, no ar",
                ComoPara = new[]
                {
                    "No ar, segure para baixo e golpeie: o cinzel bate embaixo de você.",
                    "Acertar espinhos ou inimigos por baixo faz você quicar para cima.",
                    "Encadeie os quiques para cruzar espinhos largos demais para um pulo.",
                },
                Repeticao = 3.4f,
            },
        };

        public static Info Obter(ScrapMove mover) => Infos[mover];

        // ------------------------------------------------------------------
        // Desenho: cor com a transparencia do quadro todo
        // ------------------------------------------------------------------

        /// <summary>A transparencia de tudo o que se desenha aqui. Quem abre e fecha o painel ajusta.</summary>
        public static float Alfa = 1f;

        public static readonly Color Ouro = new Color(0.95f, 0.80f, 0.45f);
        public static readonly Color EscuroOuro = new Color(0.45f, 0.33f, 0.16f);
        public static readonly Color Tinta = new Color(0.07f, 0.06f, 0.08f, 0.97f);
        public static readonly Color Mudo = new Color(0.25f, 0.20f, 0.18f);
        public static readonly Color Papel = new Color(0.93f, 0.90f, 0.82f);
        public static readonly Color Esmaecido = new Color(0.66f, 0.62f, 0.56f);

        private static readonly Color CostasDemo = new Color(0.11f, 0.10f, 0.12f, 1f);
        private static readonly Color Rocha = new Color(0.30f, 0.23f, 0.20f);
        private static readonly Color TopoRocha = new Color(0.52f, 0.40f, 0.30f);
        private static readonly Color VeioRocha = new Color(0.20f, 0.15f, 0.13f);
        private static readonly Color Espinho = new Color(0.72f, 0.72f, 0.78f);
        private static readonly Color EscuroEspinho = new Color(0.40f, 0.40f, 0.46f);
        private static readonly Color Vapor = new Color(0.92f, 0.96f, 1f);
        private static readonly Color Calor = new Color(1f, 0.55f, 0.40f);
        private static readonly Color Ressonancia = new Color(0.70f, 0.88f, 1f);
        private static readonly Color Cristal = new Color(0.55f, 0.80f, 1f);

        public static void Preencher(Rect retangulo, Color cor)
        {
            Color valorAnterior = GUI.color;
            GUI.color = new Color(cor.r, cor.g, cor.b, cor.a * Alfa);
            TelaCanvas.Textura(retangulo, Texture2D.whiteTexture);
            GUI.color = valorAnterior;
        }

        public static void Texto(Rect retangulo, string texto, GUIStyle estilo, Color cor)
        {
            Color valorAnterior = GUI.color;
            GUI.color = new Color(cor.r, cor.g, cor.b, cor.a * Alfa);
            TelaCanvas.Texto(retangulo, texto, estilo);
            GUI.color = valorAnterior;
        }

        /// <summary>A caixa de sempre do HUD: borda dourada por fora, fundo escuro por dentro.</summary>
        public static void Caixa(Rect interno, float borda = 3f)
        {
            Preencher(new Rect(interno.x - borda, interno.y - borda, interno.width + borda * 2f, interno.height + borda * 2f), Ouro);
            Preencher(interno, Tinta);
        }

        // ------------------------------------------------------------------
        // Estilos de texto
        // ------------------------------------------------------------------

        private static readonly Dictionary<int, GUIStyle> Estilos = new Dictionary<int, GUIStyle>();

        /// <summary>
        /// Um estilo de texto branco (a cor vem do Text). So pode ser chamado
        /// de dentro de um OnGUI, porque parte do GUI.skin.
        /// </summary>
        public static GUIStyle Estilo(int tamanho, bool negrito = false, TextAnchor ancora = TextAnchor.UpperLeft, bool quebrar = false,
            bool italico = false)
        {
            int chave = tamanho * 1000 + (negrito ? 100 : 0) + (italico ? 200 : 0) + (int)ancora * 2 + (quebrar ? 1 : 0);
            if (Estilos.TryGetValue(chave, out GUIStyle estilo) && estilo != null) return estilo;

            FontStyle estiloFonte = negrito
                ? (italico ? FontStyle.BoldAndItalic : FontStyle.Bold)
                : (italico ? FontStyle.Italic : FontStyle.Normal);

            estilo = new GUIStyle(GUI.skin.label)
            {
                fontSize = tamanho,
                fontStyle = estiloFonte,
                alignment = ancora,
                wordWrap = quebrar,
                richText = false,
            };
            estilo.normal.textColor = Color.white;
            estilo.padding = new RectOffset(0, 0, 0, 0);
            estilo.margin = new RectOffset(0, 0, 0, 0);

            Estilos[chave] = estilo;
            return estilo;
        }

        // ------------------------------------------------------------------
        // Teclas desenhadas
        // ------------------------------------------------------------------

        private struct Token
        {
            public string Texto2;
            public bool EstaChave;
        }

        private static List<Token> Partes(string chaves)
        {
            var partes = new List<Token>();
            int i = 0;

            while (i < chaves.Length)
            {
                if (chaves[i] == '[')
                {
                    int fim = chaves.IndexOf(']', i);
                    if (fim < 0) fim = chaves.Length;
                    partes.Add(new Token { Texto2 = chaves.Substring(i + 1, fim - i - 1), EstaChave = true });
                    i = fim + 1;
                    continue;
                }

                int proximo = chaves.IndexOf('[', i);
                if (proximo < 0) proximo = chaves.Length;
                string texto = chaves.Substring(i, proximo - i).Trim();
                if (texto.Length > 0) partes.Add(new Token { Texto2 = texto, EstaChave = false });
                i = proximo;
            }

            return partes;
        }

        /// <summary>
        /// Desenha a linha de teclas a partir de (x, y) e devolve a largura. As
        /// teclas que a mini cena esta apertando agora aparecem afundadas e
        /// acesas. Com <paramref name="desenho"/> falso, so mede.
        /// </summary>
        public static float Teclas(string chaves, float x, float y, float altura, ScrapMove? mover = null, float tempo = 0f, bool desenho = true)
        {
            GUIStyle estiloTampa = Estilo(Mathf.RoundToInt(altura * 0.45f), true, TextAnchor.MiddleCenter);
            GUIStyle estiloPalavra = Estilo(Mathf.RoundToInt(altura * 0.45f), false, TextAnchor.MiddleLeft);
            const float espaco = 8f;
            float cursor = x;

            foreach (Token parteTexto in Partes(chaves))
            {
                if (parteTexto.EstaChave)
                {
                    float largura = Mathf.Max(altura, estiloTampa.CalcSize(new GUIContent(parteTexto.Texto2)).x + altura * 0.6f);

                    if (desenho)
                    {
                        bool apertado = mover.HasValue && EstaPressionado(mover.Value, parteTexto.Texto2, tempo);
                        Tecla(new Rect(cursor, y, largura, altura), parteTexto.Texto2, apertado, estiloTampa);
                    }

                    cursor += largura + espaco;
                }
                else
                {
                    float largura = estiloPalavra.CalcSize(new GUIContent(parteTexto.Texto2)).x;
                    if (desenho) Texto(new Rect(cursor, y - 2f, largura + 4f, altura), parteTexto.Texto2, estiloPalavra, Esmaecido);
                    cursor += largura + espaco;
                }
            }

            return Mathf.Max(0f, cursor - espaco - x);
        }

        private static void Tecla(Rect retangulo, string rotulo, bool apertado, GUIStyle estilo)
        {
            float profundidade = Mathf.Max(3f, retangulo.height * 0.12f);

            if (apertado)
            {
                // Afundada: sem a lateral, e acesa por dentro.
                var rosto = new Rect(retangulo.x, retangulo.y + profundidade, retangulo.width, retangulo.height - profundidade);
                Preencher(new Rect(rosto.x - 4f, rosto.y - 4f, rosto.width + 8f, rosto.height + 8f), new Color(Ouro.r, Ouro.g, Ouro.b, 0.25f));
                Preencher(rosto, Ouro);
                Preencher(new Rect(rosto.x + 2f, rosto.y + 2f, rosto.width - 4f, rosto.height - 4f), new Color(1f, 0.90f, 0.62f));
                Texto(rosto, rotulo, estilo, new Color(0.16f, 0.11f, 0.06f));
            }
            else
            {
                Preencher(retangulo, new Color(0.62f, 0.56f, 0.48f));
                Preencher(new Rect(retangulo.x, retangulo.yMax - profundidade, retangulo.width, profundidade), new Color(0.30f, 0.26f, 0.22f));
                var rosto = new Rect(retangulo.x + 2f, retangulo.y + 2f, retangulo.width - 4f, retangulo.height - profundidade - 2f);
                Preencher(rosto, new Color(0.22f, 0.20f, 0.19f));
                Texto(rosto, rotulo, estilo, Papel);
            }
        }

        /// <summary>Quais teclas a mini cena esta apertando neste instante da volta.</summary>
        private static bool EstaPressionado(ScrapMove mover, string chave, float tempo)
        {
            float t = Mathf.Repeat(tempo, Obter(mover).Repeticao);

            switch (mover)
            {
                case ScrapMove.DashVapor:
                    if (chave == "Espaço") return EstaEntre(t, 0.45f, 0.60f);
                    if (chave == "Shift") return EstaEntre(t, 0.75f, 0.92f);
                    return false;

                case ScrapMove.PuloPressao:
                    return chave == "Espaço" && (EstaEntre(t, 0.40f, 0.54f) || EstaEntre(t, 0.80f, 0.94f));

                case ScrapMove.GolpeRessonancia:
                    return chave == "J" && EstaEntre(t, 0.50f, 1.25f);

                case ScrapMove.PuloParede:
                    if (chave == "←") return EstaEntre(t, 0f, 0.45f);
                    if (chave == "→") return EstaEntre(t, 1.0f, 1.45f);
                    if (chave == "Espaço") return EstaEntre(t, 0.45f, 0.58f) || EstaEntre(t, 1.45f, 1.58f);
                    return false;

                case ScrapMove.Pogo:
                    if (chave == "↓") return EstaEntre(t, 0.72f, 1.02f) || EstaEntre(t, 1.32f, 1.62f);
                    if (chave == "J") return EstaEntre(t, 0.86f, 0.98f) || EstaEntre(t, 1.46f, 1.58f);
                    return false;
            }

            return false;
        }

        private static bool EstaEntre(float t, float de, float to) => t >= de && t < to;

        // ------------------------------------------------------------------
        // A mini cena
        // ------------------------------------------------------------------

        private static ScrapAnimator _animador;

        private static Sprite[] QuadrosDoHeroi()
        {
            if (_animador == null) _animador = ScrapRef.Animador;
            return _animador != null ? _animador.QuadrosAtual : null;
        }

        /// <summary>
        /// A mini cena do movimento, num quadro escuro com moldura. O tempo e
        /// desde que o painel abriu, pra a cena sempre comecar do inicio.
        /// </summary>
        public static void Demonstrar(ScrapMove mover, Rect quadro, float tempo)
        {
            Preencher(new Rect(quadro.x - 2f, quadro.y - 2f, quadro.width + 4f, quadro.height + 4f), Mudo);
            Preencher(quadro, CostasDemo);

            GUI.BeginGroup(quadro);

            TelaCanvas.AbrirRecorte(new Rect(Vector2.zero, (quadro).size));

            float w = quadro.width;
            float h = quadro.height;

            // O tamanho do pixel: inteiro, pra a arte nao borrar.
            int p = Mathf.Max(1, Mathf.FloorToInt(h / 64f));

            // A grade apagada de tiles no fundo.
            float tile = 16f * p;
            for (float gx = tile; gx < w; gx += tile) Preencher(new Rect(gx, 0f, 1f, h), new Color(1f, 1f, 1f, 0.025f));
            for (float gy = tile; gy < h; gy += tile) Preencher(new Rect(0f, gy, w, 1f), new Color(1f, 1f, 1f, 0.025f));

            Info info = Obter(mover);
            float t = Mathf.Repeat(tempo, info.Repeticao);

            switch (mover)
            {
                case ScrapMove.DashVapor: CenaDoDash(w, h, p, t); break;
                case ScrapMove.PuloPressao: CenaDoPulo(w, h, p, t); break;
                case ScrapMove.GolpeRessonancia: CenaDaRessonancia(w, h, p, t); break;
                case ScrapMove.PuloParede: CenaDaParede(w, h, p, t, tempo); break;
                case ScrapMove.Pogo: CenaDoGolpeParaBaixo(w, h, p, t); break;
            }

            // A cena some no preto e volta do comeco (o pulo de parede nao: ele e um ciclo).
            if (mover != ScrapMove.PuloParede)
            {
                float esmaecimento = 0f;
                if (t < 0.25f) esmaecimento = 1f - t / 0.25f;
                else if (t > info.Repeticao - 0.4f) esmaecimento = (t - (info.Repeticao - 0.4f)) / 0.4f;
                if (esmaecimento > 0f) Preencher(new Rect(0f, 0f, w, h), new Color(CostasDemo.r, CostasDemo.g, CostasDemo.b, Mathf.Clamp01(esmaecimento)));
            }

            TelaCanvas.FecharRecorte();

            GUI.EndGroup();
        }

        private static int QuadroParado(float t) => ScrapAnimator.QuadroOcioso + (int)(t * 3f) % ScrapAnimator.QuadrosOcioso;

        private static void CenaDoDash(float w, float h, int p, float t)
        {
            float solo = h * 0.78f;
            float esquerdaFosso = w * 0.34f;
            float direitaFosso = w * 0.66f;

            Chao(0f, esquerdaFosso, solo, h, p);
            Chao(direitaFosso, w, solo, h, p);
            Chao(esquerdaFosso, direitaFosso, h - 3f * p, h, p);
            Espinhos(esquerdaFosso, direitaFosso, h - 3f * p, p);

            float x0 = w * 0.14f, puloX = w * 0.27f, dashX = w * 0.74f, pousarX = w * 0.82f;
            float erguer = h * 0.20f;

            if (t < 0.45f)
            {
                Heroi(QuadroParado(t), x0, solo, p, false, Color.white);
            }
            else if (t < 0.75f)
            {
                float u = (t - 0.45f) / 0.30f;
                Heroi(ScrapAnimator.Pulo, Mathf.Lerp(x0, puloX, u), solo - erguer * Mathf.Sin(u * Mathf.PI * 0.5f), p, false, Color.white);
            }
            else if (t < 1.0f)
            {
                float u = (t - 0.75f) / 0.25f;
                float y = solo - erguer;

                // O rastro: o Scrap de um instante atras, cada vez mais apagado.
                for (int k = 3; k >= 1; k--)
                {
                    float costas = u - k * 0.13f;
                    if (costas < 0f) continue;
                    Heroi(ScrapAnimator.QuadroDash, Mathf.Lerp(puloX, dashX, costas), y, p, false,
                        new Color(Calor.r, Calor.g, Calor.b, 0.45f - k * 0.12f));
                }

                Baforada(new Vector2(puloX - 4f * p, y - 8f * p), t - 0.75f, 0.45f, p, Vapor);
                Heroi(ScrapAnimator.QuadroDash, Mathf.Lerp(puloX, dashX, u), y, p, false, Color.white);
            }
            else if (t < 1.3f)
            {
                float u = (t - 1.0f) / 0.30f;
                Baforada(new Vector2(puloX - 4f * p, solo - erguer - 8f * p), t - 0.75f, 0.45f, p, Vapor);
                Heroi(ScrapAnimator.QuadroQueda, Mathf.Lerp(dashX, pousarX, u), solo - erguer * (1f - u * u), p, false, Color.white);
            }
            else
            {
                // As costas esfriam aos poucos, como no jogo.
                float calor = Mathf.Clamp01(1f - (t - 1.0f) / 0.9f);
                Heroi(QuadroParado(t), pousarX, solo, p, false, Color.Lerp(Color.white, Calor, calor));
            }
        }

        private static void CenaDoPulo(float w, float h, int p, float t)
        {
            float solo = h * 0.86f;
            Chao(0f, w, solo, h, p);

            float x0 = w * 0.2f, x1 = w * 0.8f;
            float primeiro = h * 0.22f, segundo = h * 0.44f;
            float x = Mathf.Lerp(x0, x1, Mathf.InverseLerp(0.4f, 1.75f, t));

            // A rajada do segundo pulo fica no ar, onde ele pulou.
            float rajadaX = Mathf.Lerp(x0, x1, Mathf.InverseLerp(0.4f, 1.75f, 0.8f));
            if (t >= 0.8f) Baforada(new Vector2(rajadaX, solo - primeiro + 2f * p), t - 0.8f, 0.6f, p, Vapor, 1.6f);

            if (t < 0.4f)
            {
                Heroi(QuadroParado(t), x0, solo, p, false, Color.white);
            }
            else if (t < 0.8f)
            {
                float u = (t - 0.4f) / 0.4f;
                Heroi(ScrapAnimator.Pulo, x, solo - primeiro * Mathf.Sin(u * Mathf.PI * 0.5f), p, false, Color.white);
            }
            else if (t < 1.15f)
            {
                float u = (t - 0.8f) / 0.35f;
                Heroi(ScrapAnimator.Pulo, x, solo - primeiro - (segundo - primeiro) * Mathf.Sin(u * Mathf.PI * 0.5f), p, false, Color.white);
            }
            else if (t < 1.75f)
            {
                float u = (t - 1.15f) / 0.6f;
                Heroi(ScrapAnimator.QuadroQueda, x, solo - segundo * (1f - u * u), p, false, Color.white);
            }
            else
            {
                Heroi(QuadroParado(t), x1, solo, p, false, Color.white);
            }
        }

        private static void CenaDaRessonancia(float w, float h, int p, float t)
        {
            float solo = h * 0.84f;
            Chao(0f, w, solo, h, p);

            float cx = w * 0.5f;
            float cristalEsquerda = w * 0.17f, cristalDireita = w * 0.83f;
            const float estilhaco = 1.62f;

            if (t < estilhaco)
            {
                FormaDoCristal(cristalEsquerda, solo, p);
                FormaDoCristal(cristalDireita, solo, p);
            }
            else if (t < 2.3f)
            {
                Fragmentos(cristalEsquerda, solo, t - estilhaco, p, -1f);
                Fragmentos(cristalDireita, solo, t - estilhaco, p, 1f);
            }

            bool carregado2 = t >= 1.0f && t < 1.25f;
            Color tom = carregado2 ? Color.Lerp(Color.white, Ressonancia, 0.5f + 0.5f * Mathf.Sin(t * 40f)) : Color.white;

            if (t < 0.5f || t >= 1.45f)
            {
                Heroi(QuadroParado(t), cx, solo, p, false, Color.white);
            }
            else if (t < 1.25f)
            {
                if (carregado2)
                {
                    float brilho = 0.35f + 0.25f * Mathf.Sin(t * 40f);
                    Preencher(new Rect(cx - 10f * p, solo - 24f * p, 20f * p, 22f * p), new Color(Ressonancia.r, Ressonancia.g, Ressonancia.b, brilho * 0.35f));
                }

                Heroi(ScrapAnimator.QuadroPreparo, cx, solo, p, false, tom);

                // A barra de carga embaixo dos pes.
                float carga = Mathf.Clamp01((t - 0.5f) / 0.5f);
                var barra = new Rect(cx - 10f * p, solo + 3f * p, 20f * p, 2f * p);
                Preencher(barra, Mudo);
                Preencher(new Rect(barra.x, barra.y, barra.width * carga, barra.height), carga >= 1f ? Ressonancia : Ouro);
            }
            else
            {
                Heroi(ScrapAnimator.QuadroGolpe, cx, solo, p, false, Color.white);
            }

            if (t >= 1.25f && t < 1.75f)
            {
                float u = (t - 1.25f) / 0.5f;
                var centro = new Vector2(cx, solo - 10f * p);
                Anel(centro, Mathf.Lerp(4f * p, w * 0.42f, u), p, new Color(Ressonancia.r, Ressonancia.g, Ressonancia.b, 1f - u));
                Anel(centro, Mathf.Lerp(2f * p, w * 0.30f, u), p, new Color(1f, 1f, 1f, (1f - u) * 0.5f));
            }
        }

        private static void CenaDaParede(float w, float h, int p, float t, float tempo)
        {
            float esquerda = w * 0.36f;
            float direita = w * 0.64f;

            // As paredes descem sem parar: e ele que esta subindo.
            float veio = 12f * p;
            float rolagem = Mathf.Repeat(tempo * h * 0.32f, veio * 2f);
            Parede(0f, esquerda, h, p, rolagem, veio, true);
            Parede(direita, w, h, p, rolagem, veio, false);

            float alcanceGolpe = 7f * p;
            float baseY = h * 0.66f;
            float arco = h * 0.20f;
            float deslize = 5f * p;

            if (t < 0.45f)
            {
                float u = t / 0.45f;
                Heroi(ScrapAnimator.Parede2, esquerda + alcanceGolpe, baseY + deslize * u, p, false, Color.white);
            }
            else if (t < 1.0f)
            {
                float u = (t - 0.45f) / 0.55f;
                Baforada(new Vector2(esquerda + 2f * p, baseY + deslize - 6f * p), t - 0.45f, 0.35f, p, Esmaecido);
                float y = Mathf.Lerp(baseY + deslize, baseY, u) - 4f * arco * u * (1f - u);
                Heroi(u < 0.5f ? ScrapAnimator.Pulo : ScrapAnimator.QuadroQueda, Mathf.Lerp(esquerda + alcanceGolpe, direita - alcanceGolpe, u), y, p, false, Color.white);
            }
            else if (t < 1.45f)
            {
                float u = (t - 1.0f) / 0.45f;
                Heroi(ScrapAnimator.Parede2, direita - alcanceGolpe, baseY + deslize * u, p, true, Color.white);
            }
            else
            {
                float u = (t - 1.45f) / 0.55f;
                Baforada(new Vector2(direita - 2f * p, baseY + deslize - 6f * p), t - 1.45f, 0.35f, p, Esmaecido);
                float y = Mathf.Lerp(baseY + deslize, baseY, u) - 4f * arco * u * (1f - u);
                Heroi(u < 0.5f ? ScrapAnimator.Pulo : ScrapAnimator.QuadroQueda, Mathf.Lerp(direita - alcanceGolpe, esquerda + alcanceGolpe, u), y, p, true, Color.white);
            }

            // Setinhas subindo no meio da chamine: o caminho e pra cima.
            for (int i = 0; i < 3; i++)
            {
                float y = Mathf.Repeat(h - tempo * h * 0.32f - i * h / 3f, h);
                Seta(w * 0.5f, y, p, new Color(Ouro.r, Ouro.g, Ouro.b, 0.18f));
            }
        }

        private static void CenaDoGolpeParaBaixo(float w, float h, int p, float t)
        {
            float solo = h * 0.8f;
            float esquerdaEspinho = w * 0.2f, direitaEspinho = w * 0.8f;
            Chao(0f, w, solo, h, p);
            Espinhos(esquerdaEspinho, direitaEspinho, solo, p);

            float x0 = w * 0.09f, golpe1 = w * 0.37f, golpe2 = w * 0.61f, pousar = w * 0.9f;
            float golpeY = solo - 13f * p;

            if (t < 0.35f)
            {
                Heroi(QuadroParado(t), x0, solo, p, false, Color.white);
            }
            else if (t < 0.95f)
            {
                Saltar(x0, solo, golpe1, golpeY, h * 0.32f, (t - 0.35f) / 0.6f, p, t > 0.82f);
            }
            else if (t < 1.55f)
            {
                Saltar(golpe1, golpeY, golpe2, golpeY, h * 0.30f, (t - 0.95f) / 0.6f, p, t > 1.42f);
            }
            else if (t < 2.15f)
            {
                Saltar(golpe2, golpeY, pousar, solo, h * 0.26f, (t - 1.55f) / 0.6f, p, false);
            }
            else
            {
                Heroi(QuadroParado(t), pousar, solo, p, false, Color.white);
            }

            // O risco do cinzel embaixo dos pes, e a faisca no espinho.
            CorteParaBaixo(golpe1, golpeY, t - 0.95f, p);
            CorteParaBaixo(golpe2, golpeY, t - 1.55f, p);
        }

        /// <summary>Um salto em arco de um ponto a outro. Com <paramref name="mirando"/>, ja mira o golpe pra baixo.</summary>
        private static void Saltar(float x0, float y0, float x1, float y1, float altura, float u, int p, bool mirando)
        {
            float x = Mathf.Lerp(x0, x1, u);
            float y = Mathf.Lerp(y0, y1, u) - 4f * altura * u * (1f - u);
            int quadro = mirando ? ScrapAnimator.QuadroPreparo : (u < 0.5f ? ScrapAnimator.Pulo : ScrapAnimator.QuadroQueda);
            Heroi(quadro, x, y, p, false, Color.white);
        }

        private static void CorteParaBaixo(float x, float pesY, float idade, int p)
        {
            const float duracaoVida = 0.16f;
            if (idade < 0f || idade > duracaoVida) return;

            float esmaecimento = 1f - idade / duracaoVida;
            for (int i = 0; i < 4; i++)
            {
                float largura = (14 - i * 3) * p;
                Preencher(new Rect(x - largura * 0.5f, pesY + i * 1.5f * p, largura, 1.5f * p), new Color(1f, 1f, 1f, esmaecimento));
            }

            Baforada(new Vector2(x, pesY + 6f * p), idade, 0.3f, p, Ouro);
        }

        // ------------------------------------------------------------------
        // Pecas da mini cena
        // ------------------------------------------------------------------

        /// <summary>
        /// Um quadro do Scrap, com os pes em (feetX, feetY) e o pixel do
        /// tamanho p. Sem os quadros na cena, um boneco chapado no lugar.
        /// </summary>
        private static void Heroi(int quadro, float pesX, float pesY, int p, bool inverter, Color tom)
        {
            Sprite[] quadros = QuadrosDoHeroi();
            Sprite sprite = quadros != null && quadro >= 0 && quadro < quadros.Length ? quadros[quadro] : null;

            if (sprite == null || sprite.texture == null)
            {
                Preencher(new Rect(pesX - 5f * p, pesY - 18f * p, 10f * p, 18f * p), tom * new Color(0.8f, 0.55f, 0.35f));
                return;
            }

            Rect pixels = sprite.rect;
            Vector2 pivo = sprite.pivot;

            // A cena foi medida pro quadro antigo, de 32 pixels: um quadro
            // maior (mais detalhado) encolhe pra caber no mesmo espaco.
            float escalaAtual = p * Mathf.Min(1f, 32f / pixels.height);

            float esquerda = inverter ? pesX - (pixels.width - pivo.x) * escalaAtual : pesX - pivo.x * escalaAtual;
            float topo = pesY - (pixels.height - pivo.y) * escalaAtual;
            var alvo = new Rect(esquerda, topo, pixels.width * escalaAtual, pixels.height * escalaAtual);

            Texture textura = sprite.texture;
            var uv = new Rect(pixels.x / textura.width, pixels.y / textura.height,
                pixels.width / textura.width, pixels.height / textura.height);
            if (inverter) uv = new Rect(uv.xMax, uv.y, -uv.width, uv.height);

            Color valorAnterior = GUI.color;
            GUI.color = new Color(tom.r, tom.g, tom.b, tom.a * Alfa);
            TelaCanvas.TexturaUV(alvo, textura, uv);
            GUI.color = valorAnterior;
        }

        private static void Chao(float x0, float x1, float topo, float parteInferior, int p)
        {
            if (x1 <= x0) return;

            Preencher(new Rect(x0, topo, x1 - x0, parteInferior - topo), Rocha);
            Preencher(new Rect(x0, topo, x1 - x0, 2f * p), TopoRocha);

            // Umas emendas de pedra, sempre no mesmo lugar.
            for (float x = x0 + 9f * p; x < x1 - 2f * p; x += 15f * p)
            {
                Preencher(new Rect(x, topo + 5f * p, 4f * p, p), VeioRocha);
            }
        }

        private static void Espinhos(float x0, float x1, float baseY, int p)
        {
            float largura = 6f * p;
            float altura = 7f * p;

            for (float x = x0; x + largura <= x1 + 0.5f; x += largura)
            {
                float centro = x + largura * 0.5f;
                for (float r = 0f; r < altura; r += p)
                {
                    float extensao = Mathf.Max(p, largura * (1f - r / altura));
                    Preencher(new Rect(centro - extensao * 0.5f, baseY - r - p, extensao, p), r < 2f * p ? EscuroEspinho : Espinho);
                }
            }
        }

        private static void Parede(float x0, float x1, float h, int p, float rolagem, float veio, bool ladoEsquerda)
        {
            Preencher(new Rect(x0, 0f, x1 - x0, h), Rocha);

            float beirada = ladoEsquerda ? x1 - 2f * p : x0;
            Preencher(new Rect(beirada, 0f, 2f * p, h), TopoRocha);

            for (float y = -veio * 2f + rolagem; y < h; y += veio)
            {
                float turno = Mathf.Repeat(y - rolagem, veio * 2f) < veio ? 0f : 6f * p;
                for (float x = x0 + 3f * p + turno; x < x1 - 4f * p; x += 14f * p)
                {
                    Preencher(new Rect(x, y, 7f * p, p), VeioRocha);
                }
            }
        }

        private static void FormaDoCristal(float cx, float solo, int p)
        {
            int[] linhas = { 2, 4, 6, 6, 6, 6, 6, 4, 4, 2 };
            for (int i = 0; i < linhas.Length; i++)
            {
                float largura = linhas[i] * p;
                Color cor = i < 3 ? Color.Lerp(Cristal, Color.white, 0.5f) : Cristal;
                Preencher(new Rect(cx - largura * 0.5f, solo - (linhas.Length - i) * p * 1.5f, largura, p * 1.5f), cor);
            }
        }

        private static void Fragmentos(float cx, float solo, float idade, int p, float lado)
        {
            float esmaecimento = Mathf.Clamp01(1f - idade / 0.6f);
            for (int i = 0; i < 6; i++)
            {
                float angulo = (35f + i * 22f) * Mathf.Deg2Rad;
                float velocidade = (40f + i * 9f) * p * 0.5f;
                float x = cx + lado * Mathf.Cos(angulo) * velocidade * idade;
                float y = solo - 8f * p - Mathf.Sin(angulo) * velocidade * idade + 120f * p * idade * idade;
                Preencher(new Rect(x, y, 2f * p, 2f * p), new Color(Cristal.r, Cristal.g, Cristal.b, esmaecimento));
            }
        }

        private static void Anel(Vector2 centro, float raioArea, int p, Color cor)
        {
            const int pontos = 40;
            for (int i = 0; i < pontos; i++)
            {
                float angulo = i * Mathf.PI * 2f / pontos;
                float x = centro.x + Mathf.Cos(angulo) * raioArea;
                float y = centro.y + Mathf.Sin(angulo) * raioArea * 0.8f;
                Preencher(new Rect(x - p, y - p, 2f * p, 2f * p), cor);
            }
        }

        /// <summary>Um sopro de vapor ou poeira: quadradinhos se abrindo e sumindo.</summary>
        private static void Baforada(Vector2 centro, float idade, float duracaoVida, int p, Color cor, float tamanho = 1f)
        {
            if (idade < 0f || idade > duracaoVida) return;

            float u = idade / duracaoVida;
            float espalhamento = (3f + 9f * u) * p * tamanho;
            float quadrado = Mathf.Max(p, (3f - 2f * u) * p * tamanho);
            var esmaecido = new Color(cor.r, cor.g, cor.b, (1f - u) * 0.8f);

            for (int i = 0; i < 6; i++)
            {
                float angulo = i * Mathf.PI / 3f + 0.4f;
                float x = centro.x + Mathf.Cos(angulo) * espalhamento;
                float y = centro.y + Mathf.Sin(angulo) * espalhamento * 0.6f;
                Preencher(new Rect(x - quadrado * 0.5f, y - quadrado * 0.5f, quadrado, quadrado), esmaecido);
            }
        }

        private static void Seta(float cx, float y, int p, Color cor)
        {
            for (int i = 0; i < 4; i++)
            {
                Preencher(new Rect(cx - (i + 1) * p * 1.5f, y + i * p, p * 1.5f, p), cor);
                Preencher(new Rect(cx + i * p * 1.5f, y + i * p, p * 1.5f, p), cor);
            }
        }
    }
}
