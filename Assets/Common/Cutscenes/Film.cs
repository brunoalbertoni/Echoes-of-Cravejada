using System;
using System.Collections.Generic;

namespace EchoesOfCravejada
{
    // ----------------------------------------------------------------------
    // O filme: cutscenes de camadas, com camera, animacao e particulas.
    //
    // Cada plano (FilmShot) e um palco em pixels (y pra baixo) com camadas
    // desenhadas: cada uma tem os seus quadros (animacao), as suas chaves
    // (posicao, escala, transparencia ao longo do tempo) e a sua profundidade
    // (parallax: 0 colado na tela, 1 no palco, acima de 1 na frente). A
    // camera anda, aproxima e treme; claroes, fades e particulas por cima.
    //
    // Este arquivo nao depende de UnityEngine de proposito: o mesmo
    // compositor toca o filme no jogo e gera as previas fora do Unity.
    // ----------------------------------------------------------------------

    /// <summary>Uma chave de animacao de camada: onde o pivo esta, escala e transparencia, num instante.</summary>
    [Serializable]
    public class FilmKey
    {
        public float t;
        public float x;
        public float y;
        [UnityEngine.Serialization.FormerlySerializedAs("scale")] public float escalaAtual = 1f;
        [UnityEngine.Serialization.FormerlySerializedAs("alpha")] public float alfa = 1f;

        /// <summary>Como chega nesta chave: 0 linear, 1 suave, 2 freando, 3 acelerando.</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("ease")] public int suavidade = 1;

        public FilmKey() { }

        public FilmKey(float t, float x, float y, float escalaAtual = 1f, float alfa = 1f, int suavidade = 1)
        {
            this.t = t;
            this.x = x;
            this.y = y;
            this.escalaAtual = escalaAtual;
            this.alfa = alfa;
            this.suavidade = suavidade;
        }
    }

    /// <summary>Uma camada de um plano.</summary>
    [Serializable]
    public class FilmLayer
    {
        [UnityEngine.Serialization.FormerlySerializedAs("name")] public string nome;

        /// <summary>Indice do primeiro quadro na lista do filme, e quantos sao.</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("firstFrame")] public int primeiroQuadro;
        [UnityEngine.Serialization.FormerlySerializedAs("frameCount")] public int quantidadeQuadro = 1;

        /// <summary>A animacao: quadros por segundo, quando comeca e se repete.</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("fps")] public float qps = 8f;
        [UnityEngine.Serialization.FormerlySerializedAs("animStart")] public float inicioAnim;
        [UnityEngine.Serialization.FormerlySerializedAs("loop")] public bool repeticao;

        /// <summary>Profundidade: 0 colado na tela, 1 no palco, maior que 1 passa na frente.</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("parallax")] public float paralaxe = 1f;

        /// <summary>Soma a luz em vez de cobrir (brilhos, raios).</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("additive")] public bool aditivo;

        /// <summary>Onde fica o pivo, em fracao do quadro (0,0 canto de cima a esquerda).</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("pivotX")] public float pivoX = 0.5f;
        [UnityEngine.Serialization.FormerlySerializedAs("pivotY")] public float pivoY = 0.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("keys")] public FilmKey[] chaves;
    }

    /// <summary>A camera: o ponto do palco no meio da tela e o zoom.</summary>
    [Serializable]
    public class FilmCam
    {
        public float t;
        public float x;
        public float y;
        [UnityEngine.Serialization.FormerlySerializedAs("zoom")] public float zoom = 1f;
        [UnityEngine.Serialization.FormerlySerializedAs("ease")] public int suavidade = 1;

        public FilmCam() { }

        public FilmCam(float t, float x, float y, float zoom = 1f, int suavidade = 1)
        {
            this.t = t;
            this.x = x;
            this.y = y;
            this.zoom = zoom;
            this.suavidade = suavidade;
        }
    }

    /// <summary>Um tremor de camera, que comeca forte e some.</summary>
    [Serializable]
    public class FilmShake
    {
        public float t;
        [UnityEngine.Serialization.FormerlySerializedAs("duration")] public float duracao;
        [UnityEngine.Serialization.FormerlySerializedAs("amount")] public float quantia;

        public FilmShake() { }
        public FilmShake(float t, float duracao, float quantia) { this.t = t; this.duracao = duracao; this.quantia = quantia; }
    }

    /// <summary>Um clarao de cor por cima de tudo: sobe rapido e desbota.</summary>
    [Serializable]
    public class FilmFlash
    {
        public float t;
        [UnityEngine.Serialization.FormerlySerializedAs("duration")] public float duracao;
        [UnityEngine.Serialization.FormerlySerializedAs("color")] public uint cor = 0xFFFFFFFF;
        [UnityEngine.Serialization.FormerlySerializedAs("strength")] public float forca2 = 1f;

        public FilmFlash() { }

        public FilmFlash(float t, float duracao, uint cor = 0xFFFFFFFF, float forca2 = 1f)
        {
            this.t = t;
            this.duracao = duracao;
            this.cor = cor;
            this.forca2 = forca2;
        }
    }

    /// <summary>
    /// Uma fonte de particulas. As particulas nao guardam estado: cada uma
    /// nasce num instante fixo, com sorteio fixo, e a posicao sai da conta —
    /// pular e voltar no tempo da sempre o mesmo quadro.
    /// </summary>
    [Serializable]
    public class FilmSpray
    {
        /// <summary>0 quadradinho, 1 bolinha macia, 2 risco no sentido do voo.</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("shape")] public int forma;

        /// <summary>Onde nascem, no palco.</summary>
        public float x, y, w, h;

        /// <summary>Quando nascem e quantas por segundo.</summary>
        public float from, to, rate;

        /// <summary>Velocidade (pixels por segundo) e o quanto ela varia.</summary>
        public float vx, vy, spreadX, spreadY;

        /// <summary>Velocidade pra fora de um centro (estouros): soma com a de cima.</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("burst")] public float rajada;

        [UnityEngine.Serialization.FormerlySerializedAs("gravity")] public float gravidade;
        [UnityEngine.Serialization.FormerlySerializedAs("life")] public float duracaoVida = 1f;
        [UnityEngine.Serialization.FormerlySerializedAs("color")] public uint cor = 0xFFFFFFFF;
        [UnityEngine.Serialization.FormerlySerializedAs("color2")] public uint cor2 = 0x00FFFFFF;
        [UnityEngine.Serialization.FormerlySerializedAs("size")] public int tamanho = 1;
        [UnityEngine.Serialization.FormerlySerializedAs("grow")] public float crescer;
        [UnityEngine.Serialization.FormerlySerializedAs("additive")] public bool aditivo;
        [UnityEngine.Serialization.FormerlySerializedAs("parallax")] public float paralaxe = 1f;
        [UnityEngine.Serialization.FormerlySerializedAs("seed")] public int semente = 1;
    }

    /// <summary>Um som num instante do plano (nome do campo em Sfx).</summary>
    [Serializable]
    public class FilmCue
    {
        public float t;
        [UnityEngine.Serialization.FormerlySerializedAs("sound")] public string som;
        [UnityEngine.Serialization.FormerlySerializedAs("pitch")] public float tom2 = 1f;
        [UnityEngine.Serialization.FormerlySerializedAs("volume")] public float volume = 1f;

        public FilmCue() { }

        public FilmCue(float t, string som, float tom2 = 1f, float volume = 1f)
        {
            this.t = t;
            this.som = som;
            this.tom2 = tom2;
            this.volume = volume;
        }
    }

    /// <summary>Um plano do filme.</summary>
    [Serializable]
    public class FilmShot
    {
        [UnityEngine.Serialization.FormerlySerializedAs("name")] public string nome;
        [UnityEngine.Serialization.FormerlySerializedAs("duration")] public float duracao = 4f;
        [UnityEngine.Serialization.FormerlySerializedAs("background")] public uint fundo = 0xFF000000;
        [UnityEngine.Serialization.FormerlySerializedAs("layers")] public FilmLayer[] camadas;
        [UnityEngine.Serialization.FormerlySerializedAs("camera")] public FilmCam[] camera;
        [UnityEngine.Serialization.FormerlySerializedAs("shakes")] public FilmShake[] tremores;
        [UnityEngine.Serialization.FormerlySerializedAs("flashes")] public FilmFlash[] claroes;
        [UnityEngine.Serialization.FormerlySerializedAs("sprays")] public FilmSpray[] jatos;
        [UnityEngine.Serialization.FormerlySerializedAs("cues")] public FilmCue[] deixas;

        /// <summary>Entrada e saida pelo preto (0: corte seco).</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("fadeIn")] public float esmaecerDentro = 0.3f;
        [UnityEngine.Serialization.FormerlySerializedAs("fadeOut")] public float foraEsmaecer = 0.3f;

        /// <summary>Quanto as bordas escurecem.</summary>
        [UnityEngine.Serialization.FormerlySerializedAs("vignette")] public float vinheta = 0.35f;
    }

    /// <summary>Os pixels de um quadro: 0xAARRGGBB, linha a linha de cima pra baixo.</summary>
    public sealed class FilmFrame
    {
        public readonly int Largura;
        public readonly int Altura;
        public readonly uint[] Pixels;

        public FilmFrame(int largura, int altura, uint[] pixels)
        {
            Largura = largura;
            Altura = altura;
            Pixels = pixels;
        }
    }

    /// <summary>Monta um instante de um plano num quadro de 320x180.</summary>
    public static class FilmCompositor
    {
        public const int Largura = 320;
        public const int Altura = 180;

        private static float[] _vinheta;
        private static float _vinhetaPor = -1f;

        public static float Suavizar(float k, int suavidade)
        {
            k = k < 0f ? 0f : k > 1f ? 1f : k;
            switch (suavidade)
            {
                case 0: return k;
                case 2: return 1f - (1f - k) * (1f - k);
                case 3: return k * k;
                default: return k * k * (3f - 2f * k);
            }
        }

        private static float Interpolar(float a, float b, float k) => a + (b - a) * k;

        public static FilmKey Amostrar(FilmKey[] chaves, float t)
        {
            if (chaves == null || chaves.Length == 0) return new FilmKey(0f, 160f, 90f);
            if (t <= chaves[0].t || chaves.Length == 1) return chaves[0];

            for (int i = 1; i < chaves.Length; i++)
            {
                if (t > chaves[i].t) continue;
                FilmKey a = chaves[i - 1], b = chaves[i];
                float k = Suavizar((t - a.t) / Math.Max(0.0001f, b.t - a.t), b.suavidade);
                return new FilmKey(t, Interpolar(a.x, b.x, k), Interpolar(a.y, b.y, k), Interpolar(a.escalaAtual, b.escalaAtual, k), Interpolar(a.alfa, b.alfa, k));
            }

            return chaves[chaves.Length - 1];
        }

        public static FilmCam AmostrarCamera(FilmCam[] chaves, float t)
        {
            if (chaves == null || chaves.Length == 0) return new FilmCam(0f, 160f, 90f);
            if (t <= chaves[0].t || chaves.Length == 1) return chaves[0];

            for (int i = 1; i < chaves.Length; i++)
            {
                if (t > chaves[i].t) continue;
                FilmCam a = chaves[i - 1], b = chaves[i];
                float k = Suavizar((t - a.t) / Math.Max(0.0001f, b.t - a.t), b.suavidade);
                return new FilmCam(t, Interpolar(a.x, b.x, k), Interpolar(a.y, b.y, k), Interpolar(a.zoom, b.zoom, k));
            }

            return chaves[chaves.Length - 1];
        }

        public static int QuadroEm(FilmLayer camadaVisual, float t)
        {
            if (camadaVisual.quantidadeQuadro <= 1 || t < camadaVisual.inicioAnim) return camadaVisual.primeiroQuadro;
            int indice = (int)((t - camadaVisual.inicioAnim) * camadaVisual.qps);
            indice = camadaVisual.repeticao ? indice % camadaVisual.quantidadeQuadro : Math.Min(indice, camadaVisual.quantidadeQuadro - 1);
            return camadaVisual.primeiroQuadro + indice;
        }

        /// <summary>Monta o plano no instante t. O alvo tem 320x180, y pra baixo.</summary>
        public static void Renderizar(FilmShot plano, IList<FilmFrame> quadros, float t, uint[] alvo)
        {
            for (int i = 0; i < alvo.Length; i++) alvo[i] = plano.fundo | 0xFF000000;

            FilmCam cam = AmostrarCamera(plano.camera, t);
            (float tremorX, float tremorY) = Tremer(plano, t);

            if (plano.camadas != null)
            {
                foreach (FilmLayer camadaVisual in plano.camadas)
                {
                    if (camadaVisual == null) continue;
                    FilmKey chave = Amostrar(camadaVisual.chaves, t);
                    if (chave.alfa <= 0.003f) continue;

                    int indice = QuadroEm(camadaVisual, t);
                    if (indice < 0 || indice >= quadros.Count || quadros[indice] == null) continue;
                    DesenharCamada(alvo, quadros[indice], camadaVisual, chave, cam, tremorX, tremorY);
                }
            }

            if (plano.jatos != null)
            {
                foreach (FilmSpray jato in plano.jatos) DesenharJato(alvo, jato, t, cam, tremorX, tremorY);
            }

            Vinheta(alvo, plano.vinheta);

            if (plano.claroes != null)
            {
                foreach (FilmFlash clarao in plano.claroes)
                {
                    float desde = t - clarao.t;
                    if (desde < 0f || desde > clarao.duracao) continue;
                    float subida = 0.08f;
                    float a = desde < subida ? desde / subida : 1f - (desde - subida) / Math.Max(0.001f, clarao.duracao - subida);
                    Lavar(alvo, clarao.cor, a * clarao.forca2);
                }
            }

            float esmaecimento = 1f;
            if (plano.esmaecerDentro > 0f && t < plano.esmaecerDentro) esmaecimento = t / plano.esmaecerDentro;
            if (plano.foraEsmaecer > 0f && t > plano.duracao - plano.foraEsmaecer) esmaecimento = Math.Min(esmaecimento, (plano.duracao - t) / plano.foraEsmaecer);
            if (esmaecimento < 1f) Lavar(alvo, 0xFF000000, 1f - Math.Max(0f, esmaecimento));
        }

        private static (float, float) Tremer(FilmShot plano, float t)
        {
            float x = 0f, y = 0f;
            if (plano.tremores == null) return (0f, 0f);

            foreach (FilmShake tremor in plano.tremores)
            {
                float desde = t - tremor.t;
                if (desde < 0f || desde > tremor.duracao) continue;
                float forca = tremor.quantia * (1f - desde / tremor.duracao);
                x += (float)Math.Sin(desde * 71f + tremor.t * 13f) * forca;
                y += (float)Math.Cos(desde * 59f + tremor.t * 7f) * forca;
            }

            return (x, y);
        }

        /// <summary>Do palco pra tela, com a profundidade da camada.</summary>
        private static void Projetar(float wx, float wy, float paralaxe, FilmCam cam, float tremorX, float tremorY,
            out float sx, out float sy, out float zoom)
        {
            zoom = 1f + (cam.zoom - 1f) * paralaxe;
            sx = Largura * 0.5f + (wx - cam.x * paralaxe - Largura * 0.5f * (1f - paralaxe)) * zoom + tremorX * paralaxe;
            sy = Altura * 0.5f + (wy - cam.y * paralaxe - Altura * 0.5f * (1f - paralaxe)) * zoom + tremorY * paralaxe;
        }

        private static void DesenharCamada(uint[] alvo, FilmFrame quadro, FilmLayer camadaVisual, FilmKey chave, FilmCam cam, float tremorX, float tremorY)
        {
            Projetar(chave.x, chave.y, camadaVisual.paralaxe, cam, tremorX, tremorY, out float px, out float py, out float zoom);
            float escalaAtual = chave.escalaAtual * zoom;
            if (escalaAtual <= 0.001f) return;

            float esquerda = px - quadro.Largura * camadaVisual.pivoX * escalaAtual;
            float topo = py - quadro.Altura * camadaVisual.pivoY * escalaAtual;
            int x0 = Math.Max(0, (int)Math.Floor(esquerda));
            int y0 = Math.Max(0, (int)Math.Floor(topo));
            int x1 = Math.Min(Largura, (int)Math.Ceiling(esquerda + quadro.Largura * escalaAtual));
            int y1 = Math.Min(Altura, (int)Math.Ceiling(topo + quadro.Altura * escalaAtual));
            float inv = 1f / escalaAtual;
            int alfa = (int)(Math.Min(1f, chave.alfa) * 256f);
            uint[] origem = quadro.Pixels;

            for (int y = y0; y < y1; y++)
            {
                int v = (int)((y + 0.5f - topo) * inv);
                if (v < 0 || v >= quadro.Altura) continue;
                int linha = v * quadro.Largura;
                int linhaDestino = y * Largura;

                for (int x = x0; x < x1; x++)
                {
                    int u = (int)((x + 0.5f - esquerda) * inv);
                    if (u < 0 || u >= quadro.Largura) continue;

                    uint p = origem[linha + u];
                    int a = (int)(p >> 24);
                    if (a == 0) continue;
                    a = a * alfa >> 8;
                    if (a <= 0) continue;

                    int i = linhaDestino + x;
                    alvo[i] = camadaVisual.aditivo ? Adicionar(alvo[i], p, a) : Sobrepor(alvo[i], p, a);
                }
            }
        }

        private static uint Sobrepor(uint abaixo, uint sobre, int a)
        {
            if (a >= 255) return sobre | 0xFF000000;
            int r0 = (int)(abaixo >> 16 & 255), g0 = (int)(abaixo >> 8 & 255), b0 = (int)(abaixo & 255);
            int r1 = (int)(sobre >> 16 & 255), g1 = (int)(sobre >> 8 & 255), b1 = (int)(sobre & 255);
            int r = r0 + ((r1 - r0) * a >> 8);
            int g = g0 + ((g1 - g0) * a >> 8);
            int b = b0 + ((b1 - b0) * a >> 8);
            return 0xFF000000 | (uint)r << 16 | (uint)g << 8 | (uint)b;
        }

        private static uint Adicionar(uint abaixo, uint sobre, int a)
        {
            int r = (int)(abaixo >> 16 & 255) + ((int)(sobre >> 16 & 255) * a >> 8);
            int g = (int)(abaixo >> 8 & 255) + ((int)(sobre >> 8 & 255) * a >> 8);
            int b = (int)(abaixo & 255) + ((int)(sobre & 255) * a >> 8);
            return 0xFF000000 | (uint)Math.Min(255, r) << 16 | (uint)Math.Min(255, g) << 8 | (uint)Math.Min(255, b);
        }

        private static void Lavar(uint[] alvo, uint cor, float quantia)
        {
            if (quantia <= 0f) return;
            int a = (int)(Math.Min(1f, quantia) * 256f);
            if (a > 255) a = 255;
            for (int i = 0; i < alvo.Length; i++) alvo[i] = Sobrepor(alvo[i], cor, a);
        }

        private static void Vinheta(uint[] alvo, float forca2)
        {
            if (forca2 <= 0f) return;

            if (_vinheta == null || Math.Abs(_vinhetaPor - forca2) > 0.0001f)
            {
                _vinheta = new float[Largura * Altura];
                _vinhetaPor = forca2;
                for (int y = 0; y < Altura; y++)
                {
                    for (int x = 0; x < Largura; x++)
                    {
                        float dx = (x - Largura * 0.5f) / (Largura * 0.5f);
                        float dy = (y - Altura * 0.5f) / (Altura * 0.5f);
                        float d = (float)Math.Sqrt(dx * dx * 0.8f + dy * dy * 0.6f);
                        float k = Math.Max(0f, d - 0.55f) / 0.6f;
                        _vinheta[x + y * Largura] = 1f - Math.Min(1f, k * k) * forca2;
                    }
                }
            }

            for (int i = 0; i < alvo.Length; i++)
            {
                float k = _vinheta[i];
                if (k >= 0.999f) continue;
                uint p = alvo[i];
                alvo[i] = 0xFF000000 | (uint)((p >> 16 & 255) * k) << 16 | (uint)((p >> 8 & 255) * k) << 8 | (uint)((p & 255) * k);
            }
        }

        // ------------------------------------------------------------------
        // Particulas
        // ------------------------------------------------------------------

        private static float Aleatorio(int semente, int i, int sal)
        {
            unchecked
            {
                uint h = (uint)(semente * 73856093) ^ (uint)(i * 19349663) ^ (uint)(sal * 83492791);
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                return (h & 0xFFFFFF) / (float)0x1000000;
            }
        }

        private static uint MisturarCores(uint a, uint b, float k)
        {
            int r = (int)Interpolar(a >> 16 & 255, b >> 16 & 255, k);
            int g = (int)Interpolar(a >> 8 & 255, b >> 8 & 255, k);
            int bl = (int)Interpolar(a & 255, b & 255, k);
            int al = (int)Interpolar(a >> 24 & 255, b >> 24 & 255, k);
            return (uint)al << 24 | (uint)r << 16 | (uint)g << 8 | (uint)bl;
        }

        private static void DesenharJato(uint[] alvo, FilmSpray jato, float t, FilmCam cam, float tremorX, float tremorY)
        {
            if (jato == null || jato.rate <= 0f || t < jato.from) return;

            int total = (int)((jato.to - jato.from) * jato.rate);
            int ultimo = Math.Min(total - 1, (int)((t - jato.from) * jato.rate));
            int primeiro = Math.Max(0, (int)((t - jato.from - jato.duracaoVida * 1.3f) * jato.rate));
            float cx = jato.x + jato.w * 0.5f, cy = jato.y + jato.h * 0.5f;

            for (int i = primeiro; i <= ultimo; i++)
            {
                float nascimento = jato.from + i / jato.rate;
                float duracaoVida = jato.duracaoVida * (0.7f + 0.6f * Aleatorio(jato.semente, i, 5));
                float idade = t - nascimento;
                if (idade < 0f || idade > duracaoVida) continue;

                float x0 = jato.x + Aleatorio(jato.semente, i, 1) * jato.w;
                float y0 = jato.y + Aleatorio(jato.semente, i, 2) * jato.h;
                float vx = jato.vx + (Aleatorio(jato.semente, i, 3) * 2f - 1f) * jato.spreadX;
                float vy = jato.vy + (Aleatorio(jato.semente, i, 4) * 2f - 1f) * jato.spreadY;

                if (jato.rajada != 0f)
                {
                    float dx = x0 - cx, dy = y0 - cy;
                    float tam = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (tam < 0.01f)
                    {
                        float ang = Aleatorio(jato.semente, i, 6) * 6.2832f;
                        dx = (float)Math.Cos(ang);
                        dy = (float)Math.Sin(ang);
                        tam = 1f;
                    }

                    float forca = jato.rajada * (0.5f + Aleatorio(jato.semente, i, 7));
                    vx += dx / tam * forca;
                    vy += dy / tam * forca;
                }

                float wx = x0 + vx * idade;
                float wy = y0 + vy * idade + 0.5f * jato.gravidade * idade * idade;
                float k = idade / duracaoVida;
                uint cor = MisturarCores(jato.cor, jato.cor2, k);
                float esmaecimento = (cor >> 24 & 255) / 255f * (1f - k * k);
                float tamanho = jato.tamanho + jato.crescer * idade;

                Projetar(wx, wy, jato.paralaxe, cam, tremorX, tremorY, out float sx, out float sy, out float zoom);
                tamanho *= zoom;

                if (jato.forma == 2)
                {
                    float svx = vx, svy = vy + jato.gravidade * idade;
                    float tam = (float)Math.Sqrt(svx * svx + svy * svy);
                    if (tam < 0.01f) continue;
                    int passos = Math.Max(2, (int)(tam * 0.05f * zoom));
                    for (int s = 0; s < passos; s++)
                    {
                        float f = s / (float)passos;
                        Ponto(alvo, sx - svx / tam * s, sy - svy / tam * s, Math.Max(1f, tamanho), cor, esmaecimento * (1f - f), jato.aditivo, false);
                    }
                }
                else
                {
                    Ponto(alvo, sx, sy, Math.Max(1f, tamanho), cor, esmaecimento, jato.aditivo, jato.forma == 1);
                }
            }
        }

        private static void Ponto(uint[] alvo, float cx, float cy, float tamanho, uint cor, float alfa, bool aditivo, bool macio)
        {
            int s = Math.Max(1, (int)Math.Round(tamanho));
            int x0 = (int)Math.Round(cx - s * 0.5f), y0 = (int)Math.Round(cy - s * 0.5f);
            float r = s * 0.5f;

            for (int y = y0; y < y0 + s; y++)
            {
                if (y < 0 || y >= Altura) continue;
                for (int x = x0; x < x0 + s; x++)
                {
                    if (x < 0 || x >= Largura) continue;
                    float a = alfa;
                    if (macio && s > 2)
                    {
                        float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                        float d = (float)Math.Sqrt(dx * dx + dy * dy) / r;
                        if (d > 1f) continue;
                        a *= d < 0.5f ? 1f : 1f - (d - 0.5f) * 1.6f;
                    }

                    int ai = (int)(Math.Min(1f, a) * 255f);
                    if (ai <= 0) continue;
                    int i = x + y * Largura;
                    alvo[i] = aditivo ? Adicionar(alvo[i], cor, ai) : Sobrepor(alvo[i], cor, ai);
                }
            }
        }
    }
}
