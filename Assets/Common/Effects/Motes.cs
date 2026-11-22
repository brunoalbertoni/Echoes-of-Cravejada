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
        private const int Capacidade = 700;
        private const float Pixel = 1f / 16f;

        private enum Kind
        {
            Livre,
            Atrair2,
        }

        private struct Mote
        {
            public bool Vivo;
            public Kind Tipo;
            public Vector2 Posicao;
            public Vector2 VelocidadeVetorial;
            public Vector2 De;
            public Vector2 Alvo;
            public float Redemoinho;
            public float Idade;
            public float DuracaoVida;
            public float Gravidade;
            public float QuadroArrasto;
            public float Oscilacao;
            public Color Inicio;
            public Color Fim;
        }

        private static Motes _instancia;
        private static Sprite _pixel;
        private static Material _sem;

        private readonly Mote[] _particulas = new Mote[Capacidade];
        private readonly SpriteRenderer[] _renderizadores = new SpriteRenderer[Capacidade];
        private int _proximo;

        private static Motes Instancia
        {
            get
            {
                if (_instancia != null) return _instancia;
                var go = new GameObject("Motes");
                _instancia = go.AddComponent<Motes>();
                return _instancia;
            }
        }

        private void Awake()
        {
            if (_pixel == null)
            {
                var textura = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                textura.SetPixel(0, 0, Color.white);
                textura.Apply();
                _pixel = Sprite.Create(textura, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 16f);
            }

            if (_sem == null)
            {
                Shader sombreador = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (sombreador != null) _sem = new Material(sombreador);
            }

            for (int i = 0; i < Capacidade; i++)
            {
                var go = new GameObject("Mote");
                go.transform.SetParent(transform, false);
                var renderizador = go.AddComponent<SpriteRenderer>();
                renderizador.sprite = _pixel;
                renderizador.sortingLayerName = "Actors";
                renderizador.sortingOrder = 45;
                if (_sem != null) renderizador.sharedMaterial = _sem;
                renderizador.enabled = false;
                _renderizadores[i] = renderizador;
            }
        }

        private void OnDestroy()
        {
            if (_instancia == this) _instancia = null;
        }

        private int Pegar()
        {
            // Procura um livre a partir do ultimo; se estiver tudo cheio, recicla o mais antigo da volta.
            for (int n = 0; n < Capacidade; n++)
            {
                int i = (_proximo + n) % Capacidade;
                if (_particulas[i].Vivo) continue;
                _proximo = (i + 1) % Capacidade;
                return i;
            }

            int maisAntigo = _proximo;
            _proximo = (_proximo + 1) % Capacidade;
            return maisAntigo;
        }

        private void Criar(Mote particula, float pixelsTamanho)
        {
            int i = Pegar();
            particula.Vivo = true;
            particula.Idade = 0f;
            _particulas[i] = particula;

            SpriteRenderer renderizador = _renderizadores[i];
            renderizador.enabled = true;
            renderizador.transform.localScale = new Vector3(pixelsTamanho, pixelsTamanho, 1f);
            renderizador.color = particula.Inicio;
            renderizador.transform.position = AlinharAoPixel(particula.Posicao);
        }

        private static Vector3 AlinharAoPixel(Vector2 p) => new Vector3(Mathf.Round(p.x / Pixel) * Pixel, Mathf.Round(p.y / Pixel) * Pixel, 0f);

        private void Update()
        {
            float dt = Time.deltaTime;

            for (int i = 0; i < Capacidade; i++)
            {
                ref Mote m = ref _particulas[i];
                if (!m.Vivo) continue;

                m.Idade += dt;
                float k = m.Idade / m.DuracaoVida;
                if (k >= 1f)
                {
                    m.Vivo = false;
                    _renderizadores[i].enabled = false;
                    continue;
                }

                if (m.Tipo == Kind.Atrair2)
                {
                    // Puxado pro alvo, cada vez mais rapido, num redemoinho.
                    float puxar = k * k * (3f - 2f * k);
                    Vector2 reto = Vector2.Lerp(m.De, m.Alvo, puxar);
                    Vector2 longe2 = m.De - m.Alvo;
                    Vector2 lado = new Vector2(-longe2.y, longe2.x).normalized * Mathf.Sin(puxar * Mathf.PI) * m.Redemoinho;
                    m.Posicao = reto + lado;
                }
                else
                {
                    m.VelocidadeVetorial.y += m.Gravidade * dt;
                    m.VelocidadeVetorial *= Mathf.Max(0f, 1f - m.QuadroArrasto * dt);
                    m.Posicao += m.VelocidadeVetorial * dt;
                    if (m.Oscilacao > 0f) m.Posicao.x += Mathf.Sin((m.Idade + i) * 5f) * m.Oscilacao * dt;
                }

                Color cor = Color.Lerp(m.Inicio, m.Fim, k);
                cor.a *= k > 0.7f ? (1f - k) / 0.3f : 1f;

                SpriteRenderer renderizador = _renderizadores[i];
                renderizador.color = cor;
                renderizador.transform.position = AlinharAoPixel(m.Posicao);
            }
        }

        // ------------------------------------------------------------------
        // O que se pede
        // ------------------------------------------------------------------

        /// <summary>Um estouro em todas as direcoes, com gravidade (gravity negativa puxa pra baixo).</summary>
        public static void Estourar(Vector2 at, int quantidade, Color a, Color b, float minVelocidade, float maxVelocidade, float gravidade,
            float duracaoVida, float pixelsTamanho = 2f, float arrasto = 0.6f)
        {
            Motes particulas = Instancia;
            for (int n = 0; n < quantidade; n++)
            {
                float angulo = Random.value * Mathf.PI * 2f;
                float velocidade = Random.Range(minVelocidade, maxVelocidade);
                particulas.Criar(new Mote
                {
                    Tipo = Kind.Livre,
                    Posicao = at + Random.insideUnitCircle * 0.2f,
                    VelocidadeVetorial = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * velocidade,
                    DuracaoVida = duracaoVida * Random.Range(0.7f, 1.2f),
                    Gravidade = gravidade,
                    QuadroArrasto = arrasto,
                    Inicio = Color.Lerp(a, b, Random.value * 0.5f),
                    Fim = b,
                }, pixelsTamanho * (Random.value < 0.25f ? 0.5f : 1f));
            }
        }

        /// <summary>Uma fumacinha subindo devagar, balancando: o verde evaporando e virando outra cor.</summary>
        public static void Subir(Vector2 at, int quantidade, Color a, Color b, float espalhamento = 0.6f, float duracaoVida = 1.8f)
        {
            Motes particulas = Instancia;
            for (int n = 0; n < quantidade; n++)
            {
                particulas.Criar(new Mote
                {
                    Tipo = Kind.Livre,
                    Posicao = at + new Vector2(Random.Range(-espalhamento, espalhamento), Random.Range(-0.1f, 0.3f)),
                    VelocidadeVetorial = new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(0.8f, 2.2f)),
                    DuracaoVida = duracaoVida * Random.Range(0.7f, 1.3f),
                    Gravidade = 0.4f,
                    QuadroArrasto = 0.3f,
                    Oscilacao = 0.6f,
                    Inicio = a,
                    Fim = b,
                }, Random.value < 0.3f ? 2f : 1f);
            }
        }

        /// <summary>Uma particula que sai de from e e sugada ate to em time segundos, girando.</summary>
        public static void Atrair(Vector2 de, Vector2 to, Color a, Color b, float tempo, float pixelsTamanho = 2f)
        {
            Instancia.Criar(new Mote
            {
                Tipo = Kind.Atrair2,
                Posicao = de,
                De = de,
                Alvo = to,
                Redemoinho = Random.Range(-2.5f, 2.5f),
                DuracaoVida = tempo,
                Inicio = a,
                Fim = b,
            }, pixelsTamanho);
        }

        /// <summary>Poeira de luz caindo devagar numa faixa larga (de cima de area).</summary>
        public static void Flutuar(Rect area, int quantidade, Color a, Color b, float duracaoVida = 4f)
        {
            Motes particulas = Instancia;
            for (int n = 0; n < quantidade; n++)
            {
                particulas.Criar(new Mote
                {
                    Tipo = Kind.Livre,
                    Posicao = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax)),
                    VelocidadeVetorial = new Vector2(Random.Range(-0.15f, 0.15f), Random.Range(-0.9f, -0.35f)),
                    DuracaoVida = duracaoVida * Random.Range(0.6f, 1.2f),
                    Gravidade = 0f,
                    QuadroArrasto = 0f,
                    Oscilacao = 0.8f,
                    Inicio = a,
                    Fim = b,
                }, Random.value < 0.2f ? 2f : 1f);
            }
        }
    }
}
