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
        private const float AtrasoAberto = 0.35f;

        // Ninguem fecha sem querer: o aviso de fechar so aparece depois disto.
        private const float TempoAbertoMin = 1.1f;

        private const float TempoAparecer = 0.4f;
        private const float TempoFechar = 0.25f;

        private static AbilityCard _instancia;
        private static readonly Queue<ScrapMove> Pendente = new Queue<ScrapMove>();

        private ScrapMove? _atual;
        private float _prontoEm;
        private float _abertosEm;
        private float _fechandoEm = -1f;
        private float _escalaTempoAnterior = 1f;
        private ScrapController _sucata;
        private bool _travaAnterior;

        /// <summary>Verdadeiro enquanto o card esta na tela. Npc e placas olham aqui.</summary>
        public static bool Mostrando => _instancia != null && _instancia._atual.HasValue;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _instancia = null;
            Pendente.Clear();
            if (Time.timeScale == 0f) Time.timeScale = 1f;
        }

        /// <summary>Mostra o card desta habilidade. Se ja houver um aberto, entra na fila.</summary>
        public static void Mostrar(ScrapMove mover)
        {
            if (_instancia == null)
            {
                var hospedeiro = new GameObject("AbilityCard");
                _instancia = hospedeiro.AddComponent<AbilityCard>();
            }

            Pendente.Enqueue(mover);
            if (!_instancia._atual.HasValue) _instancia._prontoEm = Time.unscaledTime + AtrasoAberto;
        }

        private void Update()
        {
            if (!_atual.HasValue)
            {
                if (Pendente.Count > 0 && Time.unscaledTime >= _prontoEm) Abrir(Pendente.Dequeue());
                return;
            }

            if (_fechandoEm >= 0f)
            {
                if (Time.unscaledTime - _fechandoEm >= TempoFechar) Fechar();
                return;
            }

            if (Time.unscaledTime - _abertosEm >= TempoAbertoMin && ContinuarPressionado())
            {
                _fechandoEm = Time.unscaledTime;
                Sfx.FecharPlaca.Tocar();
            }
        }

        private void Abrir(ScrapMove mover)
        {
            _atual = mover;
            _abertosEm = Time.unscaledTime;
            _fechandoEm = -1f;

            _escalaTempoAnterior = GameManager.Instancia.CongelarTempo();
            Sfx.ObtidoHabilidade.Tocar();

            _sucata = ScrapRef.Controlador;
            if (_sucata != null)
            {
                _travaAnterior = _sucata.ControleTravado;
                _sucata.ControleTravado = true;
            }
        }

        private void Fechar()
        {
            _atual = null;
            _fechandoEm = -1f;
            GameManager.Instancia.RestaurarTempo(_escalaTempoAnterior);

            if (_sucata != null) _sucata.ControleTravado = _travaAnterior;
            _sucata = null;

            // O proximo da fila, se houver, com o mesmo respiro.
            _prontoEm = Time.unscaledTime + AtrasoAberto;
        }

        private void OnDestroy()
        {
            // Trocar de cena com o card aberto nao pode deixar o jogo parado.
            if (_atual.HasValue)
            {
                GameManager.Instancia.RestaurarTempo(_escalaTempoAnterior);
                if (_sucata != null) _sucata.ControleTravado = _travaAnterior;
            }

            if (_instancia == this) _instancia = null;
        }

        private static bool ContinuarPressionado()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.spaceKey.wasPressedThisFrame || teclado.enterKey.wasPressedThisFrame
                    || teclado.numpadEnterKey.wasPressedThisFrame || teclado.eKey.wasPressedThisFrame
                    || teclado.jKey.wasPressedThisFrame || teclado.escapeKey.wasPressedThisFrame))
            {
                return true;
            }

            Gamepad controle = Gamepad.current;
            return controle != null && (controle.buttonSouth.wasPressedThisFrame || controle.buttonEast.wasPressedThisFrame
                || controle.buttonNorth.wasPressedThisFrame || controle.startButton.wasPressedThisFrame);
        }

        // ------------------------------------------------------------------
        // Desenho
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (!_atual.HasValue) return;

            // Por cima de todo o HUD.
            GUI.depth = -100;

            ScrapMove mover = _atual.Value;
            MoveGuide.Info info = MoveGuide.Obter(mover);
            float agora = Time.unscaledTime;
            float idade = agora - _abertosEm;

            float aparecer = Mathf.Clamp01(idade / TempoAparecer);
            float saindo = _fechandoEm >= 0f ? Mathf.Clamp01((agora - _fechandoEm) / TempoFechar) : 0f;
            float alfa = Mathf.Clamp01(aparecer * 2f) * (1f - saindo);

            // O fundo escurece primeiro, inteiro.
            MoveGuide.Alfa = 1f;
            MoveGuide.Preencher(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.72f * alfa));

            MoveGuide.Alfa = alfa;

            // --- As medidas ---
            float larguraCartao = Mathf.Min(Screen.width - 40f, 800f);
            bool empilhado = larguraCartao < 640f;
            const float controle = 24f;

            float larguraDemo = empilhado ? larguraCartao - controle * 2f : Mathf.Round(larguraCartao * 0.46f);
            float alturaDemo = Mathf.Round(larguraDemo * 0.58f);
            float larguraTexto = empilhado ? larguraCartao - controle * 2f : larguraCartao - controle * 3f - larguraDemo;

            GUIStyle estiloComo = MoveGuide.Estilo(17, false, TextAnchor.UpperLeft, true);
            float alturaComo = 0f;
            foreach (string linha2 in info.ComoPara) alturaComo += estiloComo.CalcHeight(new GUIContent(linha2), larguraTexto - 18f) + 8f;

            // Como usar: titulo, teclas, controle e as linhas.
            const float alturaChave = 36f;
            float bloqueioTexto = 26f + alturaChave + 12f + 24f + 10f + alturaComo;
            float corpo = empilhado ? alturaDemo + 18f + bloqueioTexto : Mathf.Max(alturaDemo, bloqueioTexto);

            const float cabeca = 178f;
            const float pe = 64f;
            float alturaCartao = cabeca + corpo + pe;

            var cartao = new Rect((Screen.width - larguraCartao) * 0.5f, (Screen.height - alturaCartao) * 0.5f + 26f, larguraCartao, alturaCartao);

            // --- A entrada: o card cresce passando um pouco do ponto e assenta ---
            Matrix4x4 matrizAnterior = GUI.matrix;
            float escalaAtual = Mathf.LerpUnclamped(0.82f, 1f, SuavizarComRecuo(aparecer)) * Mathf.Lerp(1f, 0.94f, saindo);
            GUIUtility.ScaleAroundPivot(new Vector2(escalaAtual, escalaAtual), cartao.center);

            var medalha = new Vector2(cartao.center.x, cartao.y);
            Raios(medalha, agora, Mathf.Max(larguraCartao, 420f) * 0.42f);

            MoveGuide.Caixa(cartao);
            MoveGuide.Preencher(new Rect(cartao.x + 6f, cartao.y + 6f, cartao.width - 12f, 1f), MoveGuide.EscuroOuro);
            MoveGuide.Preencher(new Rect(cartao.x + 6f, cartao.yMax - 7f, cartao.width - 12f, 1f), MoveGuide.EscuroOuro);

            Medalha(medalha, agora, idade);
            Brilhos(medalha, agora, larguraCartao);

            // --- O cabecalho ---
            float y = cartao.y + 56f;
            Faixa(new Rect(cartao.x, y, cartao.width, 22f), "HABILIDADE CONQUISTADA");

            y += 30f;
            GUIStyle estiloTitulo = MoveGuide.Estilo(empilhado ? 30 : 36, true, TextAnchor.MiddleCenter);
            MoveGuide.Texto(new Rect(cartao.x + 2f, y + 2f, cartao.width, 48f), info.Titulo, estiloTitulo, new Color(0f, 0f, 0f, 0.6f));
            MoveGuide.Texto(new Rect(cartao.x, y, cartao.width, 48f), info.Titulo, estiloTitulo, MoveGuide.Ouro);

            y += 52f;
            GUIStyle estiloEtiqueta = MoveGuide.Estilo(16, false, TextAnchor.MiddleCenter, false, true);
            MoveGuide.Texto(new Rect(cartao.x + controle, y, cartao.width - controle * 2f, 24f), info.Frase, estiloEtiqueta, MoveGuide.Esmaecido);

            // --- O corpo: a mini cena e o como usar ---
            float corpoY = cartao.y + cabeca;
            var demo = new Rect(cartao.x + controle, corpoY, larguraDemo, alturaDemo);
            float tempoDemo = Mathf.Max(0f, idade - TempoAparecer * 0.5f);
            MoveGuide.Demonstrar(mover, demo, tempoDemo);

            float textoX = empilhado ? cartao.x + controle : demo.xMax + controle;
            float textoY = empilhado ? demo.yMax + 18f : corpoY;

            MoveGuide.Texto(new Rect(textoX, textoY, larguraTexto, 22f), "COMO USAR", MoveGuide.Estilo(14, true), MoveGuide.Ouro);
            textoY += 26f;

            MoveGuide.Teclas(info.Chaves, textoX, textoY, alturaChave, mover, tempoDemo);
            textoY += alturaChave + 12f;

            MoveGuide.Texto(new Rect(textoX, textoY, larguraTexto, 22f), info.Controle, MoveGuide.Estilo(14), MoveGuide.Esmaecido);
            textoY += 34f;

            foreach (string linha2 in info.ComoPara)
            {
                float alturaLinha2 = estiloComo.CalcHeight(new GUIContent(linha2), larguraTexto - 18f);
                MoveGuide.Preencher(new Rect(textoX + 2f, textoY + 7f, 6f, 6f), MoveGuide.Ouro);
                MoveGuide.Texto(new Rect(textoX + 18f, textoY, larguraTexto - 18f, alturaLinha2), linha2, estiloComo, MoveGuide.Papel);
                textoY += alturaLinha2 + 8f;
            }

            // --- O rodape: como fechar, piscando, so depois do tempo minimo ---
            if (idade >= TempoAbertoMin)
            {
                const string dica = "[Espaço] ou [E] para continuar";
                const float alturaDica = 30f;
                float larguraDica = MoveGuide.Teclas(dica, 0f, 0f, alturaDica, null, 0f, false);

                float piscar = 0.55f + 0.45f * Mathf.Sin((idade - TempoAbertoMin) * 4f);
                float esmaecerDentro = Mathf.Clamp01((idade - TempoAbertoMin) / 0.3f);
                MoveGuide.Alfa = alfa * esmaecerDentro * piscar;
                MoveGuide.Teclas(dica, cartao.center.x - larguraDica * 0.5f, cartao.yMax - pe + 14f, alturaDica);
                MoveGuide.Alfa = alfa;
            }

            // O clarao da chegada.
            float clarao = 1f - Mathf.Clamp01(idade / 0.35f);
            if (clarao > 0f) MoveGuide.Preencher(cartao, new Color(1f, 0.95f, 0.8f, clarao * 0.55f));

            GUI.matrix = matrizAnterior;
            MoveGuide.Alfa = 1f;
        }

        private static float SuavizarComRecuo(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }

        /// <summary>Os raios dourados girando devagar atras do medalhao.</summary>
        private static void Raios(Vector2 centro, float agora, float comprimento)
        {
            Matrix4x4 salvo = GUI.matrix;
            const int quantidade = 14;

            for (int i = 0; i < quantidade; i++)
            {
                GUI.matrix = salvo;
                float angulo = agora * 12f + i * 360f / quantidade;
                GUIUtility.RotateAroundPivot(angulo, centro);

                float largura = i % 2 == 0 ? 14f : 8f;
                float alcanceGolpe = i % 2 == 0 ? comprimento : comprimento * 0.7f;
                MoveGuide.Preencher(new Rect(centro.x - largura * 0.5f, centro.y - alcanceGolpe, largura, alcanceGolpe),
                    new Color(MoveGuide.Ouro.r, MoveGuide.Ouro.g, MoveGuide.Ouro.b, 0.10f));
            }

            GUI.matrix = salvo;
        }

        /// <summary>
        /// O medalhao em cima do card: um disco dourado com o Scrap parado
        /// dentro, a borda pulsando de leve.
        /// </summary>
        private static void Medalha(Vector2 centro, float agora, float idade)
        {
            const float raioArea = 46f;
            float pulso = 0.5f + 0.5f * Mathf.Sin(agora * 3f);

            Disco(centro, raioArea + 8f, new Color(MoveGuide.Ouro.r, MoveGuide.Ouro.g, MoveGuide.Ouro.b, 0.15f + 0.15f * pulso));
            Disco(centro, raioArea, MoveGuide.Ouro);
            Disco(centro, raioArea - 4f, MoveGuide.EscuroOuro);
            Disco(centro, raioArea - 7f, new Color(0.13f, 0.11f, 0.13f, 1f));

            // O Scrap no meio, de frente pro card: o primeiro quadro parado.
            RetratoDoHeroi(centro, raioArea);

            // Um brilho passando em diagonal, logo na chegada.
            float varrida = Mathf.Clamp01((idade - 0.3f) / 0.6f);
            if (varrida > 0f && varrida < 1f)
            {
                float x = Mathf.Lerp(centro.x - raioArea, centro.x + raioArea, varrida);
                MoveGuide.Preencher(new Rect(x - 6f, centro.y - raioArea + 6f, 12f, (raioArea - 6f) * 2f), new Color(1f, 1f, 1f, 0.35f * (1f - varrida)));
            }
        }

        /// <summary>
        /// O Scrap parado dentro do medalhao. O quadro tem bastante sobra
        /// transparente em volta do corpo, entao ele pode ser maior que o
        /// disco; os pes ficam um pouco abaixo do centro.
        /// </summary>
        private static void RetratoDoHeroi(Vector2 centro, float raioArea)
        {
            var animador = ScrapRef.Animador;
            Sprite[] quadros = animador != null ? animador.QuadrosAtual : null;
            Sprite sprite = quadros != null && quadros.Length > 0 ? quadros[ScrapAnimator.QuadroOcioso] : null;
            if (sprite == null || sprite.texture == null) return;

            Rect pixels = sprite.rect;
            float escalaAtual = Mathf.Max(1f, Mathf.Floor((raioArea * 2f - 16f) / pixels.height * 1.3f));
            float largura = pixels.width * escalaAtual;
            float altura = pixels.height * escalaAtual;
            float pes = centro.y + raioArea * 0.62f;
            var alvo = new Rect(centro.x - largura * 0.5f, pes - (pixels.height - sprite.pivot.y) * escalaAtual, largura, altura);

            Texture textura = sprite.texture;
            var uv = new Rect(pixels.x / textura.width, pixels.y / textura.height,
                pixels.width / textura.width, pixels.height / textura.height);

            Color valorAnterior = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, MoveGuide.Alfa);
            TelaCanvas.TexturaUV(alvo, textura, uv);
            GUI.color = valorAnterior;
        }

        /// <summary>Um disco de faixas horizontais: o IMGUI so desenha retangulo.</summary>
        private static void Disco(Vector2 centro, float raioArea, Color cor)
        {
            const float passo = 2f;
            for (float dy = -raioArea; dy < raioArea; dy += passo)
            {
                float meio = dy + passo * 0.5f;
                float metade = Mathf.Sqrt(Mathf.Max(0f, raioArea * raioArea - meio * meio));
                MoveGuide.Preencher(new Rect(centro.x - metade, centro.y + dy, metade * 2f, passo), cor);
            }
        }

        /// <summary>Faiscas douradas piscando em volta do medalhao.</summary>
        private static void Brilhos(Vector2 centro, float agora, float largura)
        {
            for (int i = 0; i < 10; i++)
            {
                float semente = i * 12.9898f;
                float x = centro.x + (Mathf.Repeat(Mathf.Sin(semente) * 43758.55f, 1f) - 0.5f) * largura * 0.7f;
                float y = centro.y + (Mathf.Repeat(Mathf.Sin(semente * 1.7f) * 24634.63f, 1f) - 0.5f) * 90f;
                float reluzir = Mathf.Max(0f, Mathf.Sin(agora * 2.5f + i * 1.3f));
                float tamanho = 2f + 3f * reluzir;

                var cor = new Color(1f, 0.92f, 0.65f, reluzir * 0.9f);
                MoveGuide.Preencher(new Rect(x - tamanho * 0.5f, y - tamanho * 1.5f, tamanho, tamanho * 3f), cor);
                MoveGuide.Preencher(new Rect(x - tamanho * 1.5f, y - tamanho * 0.5f, tamanho * 3f, tamanho), cor);
            }
        }

        /// <summary>A faixa do cabecalho: o texto entre dois tracos com losangos.</summary>
        private static void Faixa(Rect linha, string texto)
        {
            GUIStyle estilo = MoveGuide.Estilo(15, true, TextAnchor.MiddleCenter);
            float larguraTexto = estilo.CalcSize(new GUIContent(texto)).x;
            float larguraLinha = Mathf.Min(140f, (linha.width - larguraTexto) * 0.5f - 40f);
            float meioY = linha.y + linha.height * 0.5f;

            MoveGuide.Texto(linha, texto, estilo, MoveGuide.Ouro);

            float fimEsquerda = linha.center.x - larguraTexto * 0.5f - 14f;
            float inicioDireita = linha.center.x + larguraTexto * 0.5f + 14f;

            MoveGuide.Preencher(new Rect(fimEsquerda - larguraLinha, meioY - 1f, larguraLinha, 2f), MoveGuide.EscuroOuro);
            MoveGuide.Preencher(new Rect(inicioDireita, meioY - 1f, larguraLinha, 2f), MoveGuide.EscuroOuro);
            Diamante(new Vector2(fimEsquerda + 4f, meioY));
            Diamante(new Vector2(inicioDireita - 4f, meioY));
        }

        private static void Diamante(Vector2 centro)
        {
            MoveGuide.Preencher(new Rect(centro.x - 1f, centro.y - 4f, 2f, 8f), MoveGuide.Ouro);
            MoveGuide.Preencher(new Rect(centro.x - 3f, centro.y - 2f, 6f, 4f), MoveGuide.Ouro);
        }
    }
}
