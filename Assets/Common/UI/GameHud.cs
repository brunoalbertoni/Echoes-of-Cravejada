using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O HUD: vida, Opaque Shards, gemas, os avisos, as barras dos
    /// chefes e a caixa de dialogo. E desenhado com IMGUI, sem asset nenhum, pra
    /// as mecanicas poderem ser testadas ja — troque por uma interface de verdade
    /// depois.
    ///
    /// Tudo o que e texto solto mora numa caixa do mesmo jeito: borda dourada e
    /// fundo escuro. Os avisos (progresso salvo, habilidade nova, item pego...)
    /// se empilham no alto da tela, pra dois ao mesmo tempo nao se atropelarem;
    /// os pedidos de botao ("E: conversar", "Segure E...") ficam embaixo do meio.
    /// </summary>
    public class GameHud : MonoBehaviour
    {
        private ScrapHealth _vida;
        private GUIStyle _rotulo;
        private GUIStyle _linha;
        private GUIStyle _fala;

        // Os avisos deste quadro, na ordem em que vao ser empilhados.
        private readonly List<string> _avisos = new List<string>();

        private static readonly Color CheioCoracao = new Color(0.95f, 0.80f, 0.45f);
        private static readonly Color VaziaCoracao = new Color(0.25f, 0.20f, 0.18f);
        private static readonly Color CorFragmento = new Color(0.60f, 0.72f, 0.78f);
        private static readonly Color CorGema = new Color(0.55f, 0.85f, 0.95f);
        private static readonly Color PreenchimentoCaixa = new Color(0.07f, 0.06f, 0.08f, 0.96f);

        private const float BordaCaixa = 3f;
        private const float IndicacaoY = 0.70f;

        private void OnGUI()
        {
            // A bancada esta fotografando a tela pra gravura: o HUD sai da foto.
            if (SaveEngraving.HudEsconder) return;

            if (_vida == null) _vida = ScrapRef.Vida;
            GarantirEstilos();

            _avisos.Clear();

            // As faixas de cena vao primeiro: a caixa de fala fica por cima delas.
            DesenharFaixasDeCinema();

            DesenharStatus();
            DesenharDialogo();
            DesenharDistrito1();
            DesenharDistrito2();
            DesenharDistrito3();

            if (HudNotice.Mostrando) _avisos.Add(HudNotice.Texto2);

            DesenharCoberturaDaTela();

            // Por ultimo, pra ficarem por cima de tudo.
            DesenharAvisos();
        }

        /// <summary>As faixas pretas de cena, deslizando pra dentro e pra fora.</summary>
        private static void DesenharFaixasDeCinema()
        {
            float barras = Cinematic.FaixasCinema;
            if (barras <= 0f) return;

            float altura = Screen.height * 0.11f * Mathf.SmoothStep(0f, 1f, barras);
            Preencher(new Rect(0f, 0f, Screen.width, altura), Color.black);
            Preencher(new Rect(0f, Screen.height - altura, Screen.width, altura), Color.black);
        }

        /// <summary>
        /// O clarao e o escuro da tela inteira (a cena do despertar comeca no
        /// preto). Ficam por cima do HUD.
        /// </summary>
        private static void DesenharCoberturaDaTela()
        {
            if (Cinematic.Clarao2 > 0f)
            {
                Preencher(new Rect(0f, 0f, Screen.width, Screen.height), new Color(1f, 0.97f, 0.88f, Mathf.Clamp01(Cinematic.Clarao2)));
            }

            if (Cinematic.Esmaecimento > 0f)
            {
                Preencher(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, Mathf.Clamp01(Cinematic.Esmaecimento)));
            }
        }

        private void GarantirEstilos()
        {
            if (_rotulo != null) return;

            _rotulo = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            _rotulo.normal.textColor = Color.white;

            // Uma linha so, sem quebrar: e o que vai dentro das caixas de aviso.
            _linha = new GUIStyle(_rotulo) { wordWrap = false, alignment = TextAnchor.MiddleCenter };

            _fala = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true, alignment = TextAnchor.UpperLeft };
            _fala.normal.textColor = new Color(0.93f, 0.90f, 0.82f);
        }

        // ------------------------------------------------------------------
        // Vida, shards e gemas
        // ------------------------------------------------------------------

        // Os icones (Resources/HudIcons, desenhados pelo HudIconArt). Em pixel art, sempre em dobro.
        private const float EscalaIcone = 2f;
        private static Texture2D _iconeCheioCoracao;
        private static Texture2D _iconeVaziaCoracao;
        private static Texture2D _iconeFragmento;
        private static Texture2D _iconeGema;
        private static bool _carregadoIcones;

        private static void CarregarIcones()
        {
            if (_carregadoIcones) return;
            _carregadoIcones = true;

            _iconeCheioCoracao = Resources.Load<Texture2D>("HudIcons/heart_full");
            _iconeVaziaCoracao = Resources.Load<Texture2D>("HudIcons/heart_empty");
            _iconeFragmento = Resources.Load<Texture2D>("HudIcons/shard");
            _iconeGema = Resources.Load<Texture2D>("HudIcons/gem");
        }

        /// <summary>Um icone em dobro com o canto em (x, y). Devolve a largura dele na tela.</summary>
        private static float Icone(Texture2D icone, float x, float y, Color tom)
        {
            var retangulo = new Rect(x, y, icone.width * EscalaIcone, icone.height * EscalaIcone);
            Color valorAnterior = GUI.color;
            GUI.color = tom;
            TelaCanvas.Textura(retangulo, icone);
            GUI.color = valorAnterior;
            return retangulo.width;
        }

        /// <summary>
        /// A vida (coracoes de sucata: cheios com a brasa acesa, gastos
        /// apagados), os fragmentos (a pepita azul) e as gemas (o rubi).
        /// </summary>
        private void DesenharStatus()
        {
            const float margem = 16f;
            CarregarIcones();

            float y = margem;

            if (_vida != null)
            {
                if (_iconeCheioCoracao != null && _iconeVaziaCoracao != null)
                {
                    float passo = _iconeCheioCoracao.width * EscalaIcone + 2f;
                    for (int i = 0; i < _vida.VidaMax; i++)
                    {
                        bool cheio = i < _vida.Vida;

                        // O ultimo coracao cheio bate quando a vida esta por um fio.
                        float batida = cheio && _vida.Vida == 1 ? Mathf.Abs(Mathf.Sin(Time.time * 4f)) * 2f : 0f;
                        Icone(cheio ? _iconeCheioCoracao : _iconeVaziaCoracao, margem + i * passo, y - batida, Color.white);
                    }

                    y += _iconeCheioCoracao.height * EscalaIcone + 6f;
                }
                else
                {
                    const float tamanho = 18f;
                    for (int i = 0; i < _vida.VidaMax; i++)
                    {
                        var retangulo = new Rect(margem + i * (tamanho + 6f), y, tamanho, tamanho);
                        Preencher(new Rect(retangulo.x - 2f, retangulo.y - 2f, tamanho + 4f, tamanho + 4f), Color.black);
                        Preencher(retangulo, i < _vida.Vida ? CheioCoracao : VaziaCoracao);
                    }

                    y += 32f;
                }
            }

            if (_iconeFragmento != null)
            {
                float largura = Icone(_iconeFragmento, margem + 4f, y, Color.white);
                TelaCanvas.Texto(new Rect(margem + 4f + largura + 6f, y - 1f, 200f, 28f), ScrapWallet.QuantidadeFragmentos.ToString(), _rotulo);
                y += _iconeFragmento.height * EscalaIcone + 4f;
            }
            else
            {
                Preencher(new Rect(margem, y, 12f, 16f), CorFragmento);
                TelaCanvas.Texto(new Rect(margem + 20f, y - 5f, 200f, 28f), ScrapWallet.QuantidadeFragmentos.ToString(), _rotulo);
                y += 26f;
            }

            DesenharGemas(margem + 4f, y);
            if (GemPurse.Gemas > 0) y += (_iconeGema != null ? _iconeGema.height * EscalaIcone : 16f) + 8f;

            DesenharInventario(margem, y + 4f);
        }

        private static readonly Color PreenchimentoEspaco = new Color(0.06f, 0.05f, 0.07f, 0.55f);
        private static readonly Color AroEspaco = new Color(0.78f, 0.6f, 0.3f, 0.55f);

        /// <summary>
        /// O inventario: uma fileira de casinhas com o icone de cada coisa que
        /// o Scrap carrega pra usar ou entregar.
        /// </summary>
        private void DesenharInventario(float x, float y)
        {
            var itens = InventoryIcons.ItensAtuais();
            if (itens.Count == 0) return;

            const float espacoItem = 38f;
            int sobMouse = -1;
            Vector2 mouse = Event.current.mousePosition;
            for (int i = 0; i < itens.Count; i++)
            {
                var retangulo = new Rect(x + i * (espacoItem + 4f), y, espacoItem, espacoItem);
                Preencher(retangulo, AroEspaco);
                Preencher(new Rect(retangulo.x + 1f, retangulo.y + 1f, espacoItem - 2f, espacoItem - 2f), PreenchimentoEspaco);

                Texture2D icone = itens[i].Icone2;
                float w = icone.width * EscalaIcone, h = icone.height * EscalaIcone;
                TelaCanvas.Textura(new Rect(retangulo.x + (espacoItem - w) * 0.5f, retangulo.y + (espacoItem - h) * 0.5f, w, h), icone);

                if (itens[i].Quantidade > 1)
                    TelaCanvas.Texto(new Rect(retangulo.xMax - 16f, retangulo.yMax - 20f, 20f, 20f), itens[i].Quantidade.ToString(), _rotulo);

                if (retangulo.Contains(mouse)) sobMouse = i;
            }

            if (sobMouse >= 0)
                DesenharDicaDoItem(itens[sobMouse], new Rect(x + sobMouse * (espacoItem + 4f), y, espacoItem, espacoItem));
        }

        private GUIStyle _nomePonta;
        private GUIStyle _usoPonta;
        private static readonly Color PreenchimentoPonta = new Color(0.07f, 0.055f, 0.06f, 0.94f);
        private static readonly Color AroPonta = new Color(0.78f, 0.6f, 0.3f, 0.9f);

        /// <summary>A plaquinha embaixo da casinha: o nome do item e o que fazer com ele.</summary>
        private void DesenharDicaDoItem(InventoryIcons.Entry item, Rect retanguloEspaco)
        {
            if (string.IsNullOrEmpty(item.Nome)) return;

            if (_nomePonta == null)
            {
                _nomePonta = new GUIStyle(_rotulo) { fontSize = 16, wordWrap = false };
                _nomePonta.normal.textColor = new Color(1f, 0.88f, 0.6f);
                _usoPonta = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = false };
                _usoPonta.normal.textColor = new Color(0.85f, 0.82f, 0.76f);
            }

            Vector2 tamanhoNome = _nomePonta.CalcSize(new GUIContent(item.Nome));
            Vector2 tamanhoUso = string.IsNullOrEmpty(item.Uso) ? Vector2.zero : _usoPonta.CalcSize(new GUIContent(item.Uso));
            float w = Mathf.Round(Mathf.Max(tamanhoNome.x, tamanhoUso.x) + 16f);
            float h = Mathf.Round(tamanhoNome.y + tamanhoUso.y + 8f);
            var caixa = new Rect(retanguloEspaco.x, retanguloEspaco.yMax + 6f, w, h);

            // Moldura de dois tons, cantos cortados, como as casinhas.
            Preencher(new Rect(caixa.x + 2f, caixa.y, w - 4f, h), AroPonta);
            Preencher(new Rect(caixa.x, caixa.y + 2f, w, h - 4f), AroPonta);
            Preencher(new Rect(caixa.x + 2f, caixa.y + 2f, w - 4f, h - 4f), PreenchimentoPonta);
            // A setinha apontando pra casinha.
            Preencher(new Rect(retanguloEspaco.center.x - 3f, caixa.y - 4f, 6f, 4f), AroPonta);
            Preencher(new Rect(retanguloEspaco.center.x - 1f, caixa.y - 6f, 2f, 2f), AroPonta);

            TelaCanvas.Texto(new Rect(caixa.x + 8f, caixa.y + 3f, w, tamanhoNome.y), item.Nome, _nomePonta);
            if (tamanhoUso.y > 0f)
                TelaCanvas.Texto(new Rect(caixa.x + 8f, caixa.y + 3f + tamanhoNome.y, w, tamanhoUso.y), item.Uso, _usoPonta);
        }

        /// <summary>
        /// As gemas lapidadas: o rubi e a conta. So aparece depois da
        /// primeira, e brilha por um instante quando chega uma nova.
        /// </summary>
        private void DesenharGemas(float x, float y)
        {
            if (GemPurse.Gemas <= 0) return;

            float brilho = Mathf.Clamp01(1f - (Time.time - GemPurse.UltimoTempoGanho) / 1.5f);

            if (_iconeGema != null)
            {
                // Ao chegar uma nova, ele clareia e da um pulinho.
                float salto = brilho * 3f;
                float largura = Icone(_iconeGema, x, y - salto, Color.Lerp(Color.white, new Color(1f, 0.9f, 0.9f), brilho));
                TelaCanvas.Texto(new Rect(x + largura + 6f, y - 1f, 200f, 28f), GemPurse.Gemas.ToString(), _rotulo);
                return;
            }

            Color gema = Color.Lerp(CorGema, Color.white, brilho * 0.7f);
            int[] larguras = { 4, 8, 12, 8, 4 };
            for (int i = 0; i < larguras.Length; i++)
            {
                Preencher(new Rect(x + 6f - larguras[i] * 0.5f, y + i * 3f, larguras[i], 3f), gema);
            }

            TelaCanvas.Texto(new Rect(x + 20f, y - 4f, 200f, 28f), GemPurse.Gemas.ToString(), _rotulo);
        }

        // ------------------------------------------------------------------
        // Os distritos
        // ------------------------------------------------------------------

        /// <summary>O District 1: a alavanca, o Rusty Scrapper e os avisos da bancada e da purificacao.</summary>
        private void DesenharDistrito1()
        {
            RustyLever alavanca = RustyLever.Proximo;
            if (alavanca != null) DesenharIndicacao("Golpeie a alavanca com o cinzel");

            RustyScrapper chefe = RustyScrapper.Lutando;
            if (chefe != null) BarraDoChefe("O Coletor Enferrujado", chefe.FracaoVida, new Color(0.45f, 0.75f, 0.55f));

            // A bancada nao avisa com frase: quem conta que salvou e a gravura (SaveEngraving).
            Workbench bancada2 = Workbench.Proximo;
            if (bancada2 != null && bancada2.PodeSalvar) DesenharIndicacao(Workbench.Atual == bancada2 ? "E: salvar de novo" : "E: salvar na Bancada de Ourives");

            // As habilidades novas (dash, pulo duplo, golpe ressonante) nao
            // viram aviso aqui: quem mostra e o AbilityCard, na tela inteira.

        }

        /// <summary>O District 2: valvulas, o Boiler Master, a loja de Crisol e os avisos.</summary>
        private void DesenharDistrito2()
        {
            Valve valvula = Valve.Proximo;
            if (valvula != null) DesenharIndicacao("Golpeie a válvula com o cinzel");

            BoilerMaster mestre = BoilerMaster.Lutando;
            if (mestre != null)
            {
                BarraDoChefe(mestre.EstaExposto ? "O Mestre das Caldeiras — FORNALHA EXPOSTA" : "O Mestre das Caldeiras",
                    mestre.FracaoVida,
                    mestre.EstaExposto ? new Color(1f, 0.55f, 0.25f) : new Color(0.55f, 0.6f, 0.7f));
            }

            Crisol loja = Crisol.AbertoLoja;
            if (loja != null) DesenharLoja(loja);

        }

        /// <summary>O District 3 (e o 4): raspar e polir, os chefes, as gemas e as estatuas.</summary>
        private void DesenharDistrito3()
        {
            if (HoldAction.Dono != null) DesenharIndicacao(HoldAction.Texto2, Mathf.Clamp01(HoldAction.Progresso2));

            TheResonant elite = TheResonant.Lutando;
            if (elite != null)
            {
                BarraDoChefe(elite.EstaExposto ? "O Capataz — PRESO" : "O Capataz", elite.FracaoVida, new Color(0.60f, 0.80f, 1f));
            }

            SilverKing rei = SilverKing.Lutando;
            if (rei != null && !rei.EstaTransformando)
            {
                BarraDoChefe(rei.EstaDoisFase ? "O Rei de Prata — CORROMPIDO" : "O Rei de Prata",
                    rei.FracaoVida,
                    rei.EstaDoisFase ? new Color(0.45f, 1f, 0.62f) : new Color(1f, 0.95f, 0.78f));
            }

            MercuryAlchemist alquimista = MercuryAlchemist.Lutando;
            if (alquimista != null)
            {
                BarraDoChefe(alquimista.EstaPrata ? "O Alquimista de Mercúrio — EM PRATA" : "O Alquimista de Mercúrio",
                    alquimista.FracaoVida,
                    alquimista.EstaPrata ? new Color(0.92f, 0.95f, 1f) : new Color(0.60f, 0.66f, 0.80f));
            }

            GeodeDrill broca = GeodeDrill.Lutando;
            if (broca != null)
            {
                BarraDoChefe(broca.EstaExposto ? "A Broca dos Geodos — MOTOR EXPOSTO" : "A Broca dos Geodos",
                    broca.FracaoVida,
                    broca.EstaExposto ? new Color(1f, 0.55f, 0.25f) : new Color(0.62f, 0.55f, 0.80f));
            }

            if (Time.time - ScrapGems.TempoDadoSafira < 6f) _avisos.Add("Safira bruta recebida: crave-a numa Bancada de Ourives");
            if (Time.time - ScrapGems.TempoEncaixadoSafira < 6f) _avisos.Add("Safira cravada: passos e quedas fazem menos ruído");

            // Nao ha medidor de ruido: o barulho se sente pelo que ele acende
            // e pelo que ele acorda.
            StatueButton estatua = StatueButton.Proximo;
            if (estatua != null && estatua.Habilitado && HoldAction.Dono == null && Npc.Conversando == null && Npc.Proximo == null)
            {
                DesenharIndicacao("E: apertar o botão da estátua");
            }
        }

        /// <summary>
        /// A loja do Crisol: o que se tem nas duas moedas, a lista (o item
        /// marcado em dourado, o que nao da pra pagar apagado), a descricao do
        /// marcado, o que ele respondeu e as teclas.
        /// </summary>
        private void DesenharLoja(Crisol loja)
        {
            GarantirEstilosDaLoja();

            const float alturaLinha = 46f;
            const float cabecalhoH = 84f;
            const float rodapeH = 46f;
            float largura = Mathf.Min(Screen.width - 40f, 860f);
            float listaH = Mathf.Max(loja.Itens.Count * alturaLinha + 12f, 250f);
            float altura = Mathf.Min(cabecalhoH + listaH + rodapeH + 20f, Screen.height - 40f);
            float x = Mathf.Round((Screen.width - largura) * 0.5f);
            float y = Mathf.Round((Screen.height - altura) * 0.5f);

            // O fundo escurece o jogo atras, pra loja ser o centro.
            Preencher(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.55f));

            var quadro = new Rect(x, y, largura, altura);
            Painel(quadro, MadeiraLoja, LataoLoja, EscuroLataoLoja);
            Rebites(quadro);

            Color valorAnterior = GUI.contentColor;

            // -- Cabecalho: o retrato do Crisol, o nome da loja, a carteira.
            var cabecalho = new Rect(x + 14f, y + 14f, largura - 28f, cabecalhoH - 14f);
            Preencher(new Rect(cabecalho.x, cabecalho.yMax - 2f, cabecalho.width, 2f), EscuroLataoLoja);

            float textoX = cabecalho.x + 6f;
            Texture2D busto = SpeakerPortraits.Obter(SpeakerPortraits.IdDe(loja.NomeExibido), false);
            if (busto != null)
            {
                float tamanho = cabecalho.height - 12f;
                var retrato = new Rect(cabecalho.x + 2f, cabecalho.y + 2f, tamanho, tamanho);
                Painel(retrato, TintaLoja, LataoLoja, EscuroLataoLoja);
                TelaCanvas.Textura(new Rect(retrato.x + 3f, retrato.y + 3f, tamanho - 6f, tamanho - 6f), busto, ScaleMode.ScaleToFit);
                textoX = retrato.xMax + 14f;
            }

            TelaCanvas.Texto(new Rect(textoX, cabecalho.y + 4f, 400f, 30f), "Oficina do Crisol", _tituloLoja);
            TelaCanvas.Texto(new Rect(textoX, cabecalho.y + 34f, 400f, 22f), "Ourives · peças e lembranças", _subtituloLoja);

            float carteiraX = cabecalho.xMax - 6f;
            carteiraX = Moeda(carteiraX, cabecalho.y + 18f, _iconeGema, GemPurse.Gemas, TomGemaLoja);
            Moeda(carteiraX - 18f, cabecalho.y + 18f, _iconeFragmento, ScrapWallet.QuantidadeFragmentos, TomFragmentoLoja);

            // -- A lista a esquerda, o detalhe a direita.
            float corpoY = cabecalho.yMax + 10f;
            float listaW = Mathf.Round(largura * 0.52f);
            var lista = new Rect(x + 14f, corpoY, listaW, altura - (corpoY - y) - rodapeH - 6f);
            var detalhe = new Rect(lista.xMax + 10f, corpoY, largura - listaW - 38f, lista.height);

            Preencher(lista, TintaLoja);
            for (int i = 0; i < loja.Itens.Count; i++)
            {
                ShopItem item = loja.Itens[i];
                bool selecionado = i == loja.Selecionado;
                bool foraVendido = Shop.Esgotado(item);
                int preco = Shop.Preco(item);
                bool acessivel = Shop.Saldo(item.MoedaTipo) >= preco;
                bool apagado = foraVendido || !acessivel;

                var linha = new Rect(lista.x + 6f, lista.y + 6f + i * alturaLinha, lista.width - 12f, alturaLinha - 6f);

                // O mouse: passar por cima marca a fileira, clicar compra.
                if (MouseNaLoja(linha, out bool clicado))
                {
                    if (clicado) loja.ClicarComprar(i);
                    else loja.PassarMouse(i);
                    selecionado = i == loja.Selecionado;
                }

                if (selecionado)
                {
                    Preencher(linha, LataoLoja);
                    Preencher(new Rect(linha.x + 2f, linha.y + 2f, linha.width - 4f, linha.height - 4f), SelecionadoLoja);
                    // A setinha de escolha, pulsando de leve.
                    float empurraozinho = Mathf.Round(Mathf.PingPong(Time.unscaledTime * 6f, 3f));
                    Preencher(new Rect(linha.x - 2f + empurraozinho, linha.center.y - 4f, 4f, 8f), LuzLataoLoja);
                    Preencher(new Rect(linha.x + 2f + empurraozinho, linha.center.y - 2f, 2f, 4f), LuzLataoLoja);
                }
                else if (i % 2 == 1)
                {
                    Preencher(linha, new Color(1f, 1f, 1f, 0.025f));
                }

                // A casinha do icone.
                var espacoItem = new Rect(linha.x + 12f, linha.y + 3f, linha.height - 6f, linha.height - 6f);
                Preencher(espacoItem, selecionado ? LataoLoja : EscuroLataoLoja);
                Preencher(new Rect(espacoItem.x + 2f, espacoItem.y + 2f, espacoItem.width - 4f, espacoItem.height - 4f), MadeiraLoja);
                Texture2D icone = ShopIcons.Obter(item.Id);
                if (icone != null)
                {
                    GUI.color = apagado && !selecionado ? new Color(1f, 1f, 1f, 0.45f) : Color.white;
                    TelaCanvas.Textura(new Rect(espacoItem.center.x - 12f, espacoItem.center.y - 12f, 24f, 24f), icone);
                    GUI.color = Color.white;
                }

                _itemLoja.normal.textColor = selecionado ? LuzLataoLoja : apagado ? MudoLoja : TextoLoja;
                TelaCanvas.Texto(new Rect(espacoItem.xMax + 10f, linha.y, linha.width - espacoItem.width - 120f, linha.height), item.Nome, _itemLoja);

                // O preco, com o icone da moeda; ou o selo de vendido.
                if (foraVendido)
                {
                    string carimbo = item.QuantidadeMax == 1 ? "COMPRADO" : "ESGOTADO";
                    Vector2 tamanho = _carimboLoja.CalcSize(new GUIContent(carimbo));
                    var etiqueta = new Rect(linha.xMax - tamanho.x - 20f, linha.center.y - 11f, tamanho.x + 12f, 22f);
                    Preencher(etiqueta, MudoLoja);
                    Preencher(new Rect(etiqueta.x + 1f, etiqueta.y + 1f, etiqueta.width - 2f, etiqueta.height - 2f), TintaLoja);
                    TelaCanvas.Texto(etiqueta, carimbo, _carimboLoja);
                }
                else
                {
                    Preco(linha.xMax - 10f, linha.center.y, item.MoedaTipo, preco, acessivel);
                }
            }

            // -- O detalhe do marcado: icone grande, nome, descricao, preco.
            Painel(detalhe, TintaLoja, EscuroLataoLoja, EscuroLataoLoja);
            if (loja.Itens.Count > 0)
            {
                ShopItem item = loja.Itens[loja.Selecionado];
                var grande = new Rect(detalhe.center.x - 36f, detalhe.y + 16f, 72f, 72f);
                Painel(grande, MadeiraLoja, LataoLoja, EscuroLataoLoja);
                Texture2D icone = ShopIcons.Obter(item.Id);
                if (icone != null) TelaCanvas.Textura(new Rect(grande.x + 12f, grande.y + 12f, 48f, 48f), icone);

                TelaCanvas.Texto(new Rect(detalhe.x + 12f, grande.yMax + 8f, detalhe.width - 24f, 28f), item.Nome, _nomeDetalheLoja);
                Preencher(new Rect(detalhe.center.x - 30f, grande.yMax + 38f, 60f, 2f), EscuroLataoLoja);
                TelaCanvas.Texto(new Rect(detalhe.x + 16f, grande.yMax + 46f, detalhe.width - 32f, 80f), item.DescricaoTexto, _descricaoLoja);

                if (!string.IsNullOrEmpty(loja.UltimoMensagem))
                    TelaCanvas.Texto(new Rect(detalhe.x + 12f, detalhe.yMax - 34f, detalhe.width - 24f, 26f), loja.UltimoMensagem, _respostaLoja);
            }

            // -- Rodape: as teclas como teclinhas. "comprar" e "sair" tambem
            // se clicam com o mouse.
            float chaveY = y + altura - rodapeH + 6f;
            float chaveX = x + 20f;
            chaveX = DicaDeTecla(chaveX, chaveY, "W/S", "escolher");
            float comprarX = chaveX + 22f;
            chaveX = DicaDeTecla(comprarX, chaveY, "Espaço", "comprar");
            if (MouseNaLoja(new Rect(comprarX, chaveY, chaveX - comprarX, 32f), out bool cliqueComprar) && cliqueComprar) loja.ClicarComprar(loja.Selecionado);
            float sairX = chaveX + 22f;
            float fimSair = DicaDeTecla(sairX, chaveY, "E", "sair");
            if (MouseNaLoja(new Rect(sairX, chaveY, fimSair - sairX, 32f), out bool cliqueSair) && cliqueSair) loja.ClicarFechar();
            TelaCanvas.Texto(new Rect(fimSair + 22f, chaveY + 4f, 260f, 24f), "ou clique no item", _subtituloLoja);

            GUI.contentColor = valorAnterior;
        }

        // ------------------------------------------------------------------
        // Pecas da loja
        // ------------------------------------------------------------------

        private static readonly Color MadeiraLoja = new Color(0.13f, 0.09f, 0.08f, 0.98f);
        private static readonly Color TintaLoja = new Color(0.07f, 0.05f, 0.06f, 1f);
        private static readonly Color SelecionadoLoja = new Color(0.27f, 0.19f, 0.10f, 1f);
        private static readonly Color LataoLoja = new Color(0.82f, 0.62f, 0.30f);
        private static readonly Color LuzLataoLoja = new Color(1f, 0.87f, 0.55f);
        private static readonly Color EscuroLataoLoja = new Color(0.42f, 0.29f, 0.15f);
        private static readonly Color TextoLoja = new Color(0.92f, 0.88f, 0.80f);
        private static readonly Color MudoLoja = new Color(0.52f, 0.47f, 0.43f);
        private static readonly Color TomFragmentoLoja = new Color(0.72f, 0.92f, 1f);
        private static readonly Color TomGemaLoja = new Color(1f, 0.6f, 0.6f);

        private GUIStyle _tituloLoja, _subtituloLoja, _itemLoja, _precoLoja, _carimboLoja, _nomeDetalheLoja, _descricaoLoja, _respostaLoja, _chaveLoja, _moedaLoja;

        private void GarantirEstilosDaLoja()
        {
            if (_tituloLoja != null) return;

            _tituloLoja = new GUIStyle(_rotulo) { fontSize = 24 };
            _tituloLoja.normal.textColor = LuzLataoLoja;
            _subtituloLoja = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Italic };
            _subtituloLoja.normal.textColor = MudoLoja;
            _itemLoja = new GUIStyle(_rotulo) { fontSize = 17, alignment = TextAnchor.MiddleLeft, wordWrap = false, clipping = TextClipping.Clip };
            _precoLoja = new GUIStyle(_rotulo) { fontSize = 17, alignment = TextAnchor.MiddleRight };
            _carimboLoja = new GUIStyle(_rotulo) { fontSize = 12, alignment = TextAnchor.MiddleCenter };
            _carimboLoja.normal.textColor = MudoLoja;
            _nomeDetalheLoja = new GUIStyle(_rotulo) { fontSize = 19, alignment = TextAnchor.MiddleCenter, wordWrap = false };
            _nomeDetalheLoja.normal.textColor = LuzLataoLoja;
            _descricaoLoja = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, alignment = TextAnchor.UpperCenter };
            _descricaoLoja.normal.textColor = TextoLoja;
            _respostaLoja = new GUIStyle(_rotulo) { fontSize = 15, alignment = TextAnchor.MiddleCenter, wordWrap = false };
            _respostaLoja.normal.textColor = LuzLataoLoja;
            _chaveLoja = new GUIStyle(_rotulo) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
            _chaveLoja.normal.textColor = TintaLoja;
            _moedaLoja = new GUIStyle(_rotulo) { fontSize = 22 };
        }

        /// <summary>Uma placa: borda escura por fora, filete de latao, fundo.</summary>
        private static void Painel(Rect r, Color preenchimento, Color aro, Color sombraChao)
        {
            Preencher(new Rect(r.x - 3f, r.y - 3f, r.width + 6f, r.height + 6f), TintaLoja);
            Preencher(new Rect(r.x - 2f, r.y - 2f, r.width + 4f, r.height + 4f), aro);
            Preencher(new Rect(r.x, r.y + r.height, r.width, 2f), sombraChao);
            Preencher(r, preenchimento);
        }

        /// <summary>Os rebites de latao nos cantos da moldura.</summary>
        private static void Rebites(Rect r)
        {
            foreach (Vector2 c in new[] { new Vector2(r.x + 6f, r.y + 6f), new Vector2(r.xMax - 10f, r.y + 6f),
                         new Vector2(r.x + 6f, r.yMax - 10f), new Vector2(r.xMax - 10f, r.yMax - 10f) })
            {
                Preencher(new Rect(c.x, c.y, 4f, 4f), EscuroLataoLoja);
                Preencher(new Rect(c.x, c.y, 2f, 2f), LuzLataoLoja);
            }
        }

        /// <summary>Um valor da carteira, alinhado a direita em right. Devolve onde comecou.</summary>
        private float Moeda(float direita, float y, Texture2D icone, int valor, Color tom)
        {
            string texto = valor.ToString();
            Vector2 tamanho = _moedaLoja.CalcSize(new GUIContent(texto));
            float w = icone != null ? icone.width * EscalaIcone : 0f;
            float h = icone != null ? icone.height * EscalaIcone : 0f;
            float esquerda = direita - tamanho.x - w - 8f;

            if (icone != null) DesenharCentralizado(icone, new Rect(esquerda, y + (tamanho.y - h) * 0.5f, w, h));
            _moedaLoja.normal.textColor = tom;
            TelaCanvas.Texto(new Rect(esquerda + w + 6f, y, tamanho.x + 4f, tamanho.y), texto, _moedaLoja);
            return esquerda;
        }

        /// <summary>
        /// O preco de uma fileira: o icone da moeda numa casinha fixa na
        /// direita e o numero numa coluna fixa colada nela, os dois centrados
        /// na mesma linha. Assim os precos de todas as fileiras ficam um
        /// embaixo do outro, seja em fragmento (o cristal torto, com a linha
        /// de baixo vazia) ou em gema.
        /// </summary>
        private void Preco(float direita, float centroY, Currency moeda, int preco, bool acessivel)
        {
            Texture2D icone = moeda == Currency.Gemas ? _iconeGema : _iconeFragmento;
            const float caixa = 20f;
            const float coluna = 52f;

            _precoLoja.normal.textColor = acessivel ? TextoLoja : new Color(0.85f, 0.42f, 0.38f);
            TelaCanvas.Texto(new Rect(direita - caixa - 6f - coluna, centroY - 15f, coluna, 30f), preco.ToString(), _precoLoja);

            if (icone != null)
            {
                GUI.color = acessivel ? Color.white : new Color(1f, 1f, 1f, 0.5f);
                DesenharCentralizado(icone, new Rect(direita - caixa, centroY - caixa * 0.5f, caixa, caixa));
                GUI.color = Color.white;
            }
        }

        private Vector2 _ultimoMouse = new Vector2(-1f, -1f);

        /// <summary>
        /// O mouse em cima de uma area da loja. So conta como "passou por
        /// cima" quando ele de fato se mexe (senao o mouse parado brigaria com
        /// W/S); o clique do botao esquerdo vale sempre.
        /// </summary>
        private bool MouseNaLoja(Rect area, out bool clicado)
        {
            Event e = Event.current;
            clicado = false;
            if (!area.Contains(e.mousePosition)) return false;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                clicado = true;
                e.Use();
                return true;
            }

            if (e.type == EventType.Repaint && (e.mousePosition - _ultimoMouse).sqrMagnitude > 0.5f)
            {
                _ultimoMouse = e.mousePosition;
                return true;
            }

            return false;
        }

        private readonly Dictionary<Texture2D, Rect> _visivel = new Dictionary<Texture2D, Rect>();

        /// <summary>
        /// Desenha um icone de pixel com a parte desenhada (sem a borda
        /// transparente) no meio da caixa, em escala inteira.
        /// </summary>
        private void DesenharCentralizado(Texture2D icone, Rect caixa)
        {
            if (!_visivel.TryGetValue(icone, out Rect usado))
            {
                usado = new Rect(0f, 0f, icone.width, icone.height);
                if (icone.isReadable)
                {
                    int x0 = icone.width, y0 = icone.height, x1 = -1, y1 = -1;
                    Color32[] pixels = icone.GetPixels32();
                    for (int y = 0; y < icone.height; y++)
                    {
                        for (int x = 0; x < icone.width; x++)
                        {
                            if (pixels[x + y * icone.width].a == 0) continue;
                            x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x);
                            y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y);
                        }
                    }

                    if (x1 >= 0) usado = new Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
                }

                _visivel[icone] = usado;
            }

            float escalaAtual = Mathf.Max(1f, Mathf.Floor(Mathf.Min(caixa.width / usado.width, caixa.height / usado.height)));
            float w = icone.width * escalaAtual;
            float h = icone.height * escalaAtual;

            // A parte desenhada no centro da caixa (y da textura cresce pra cima).
            float vx = caixa.center.x - (usado.x + usado.width * 0.5f) * escalaAtual;
            float vy = caixa.center.y - (icone.height - usado.y - usado.height * 0.5f) * escalaAtual;
            TelaCanvas.Textura(new Rect(Mathf.Round(vx), Mathf.Round(vy), w, h), icone);
        }

        /// <summary>Uma teclinha de latao com o que ela faz ao lado. Devolve onde terminou.</summary>
        private float DicaDeTecla(float x, float y, string chave, string acao)
        {
            Vector2 tamanhoChave = _chaveLoja.CalcSize(new GUIContent(chave));
            var tampa = new Rect(x, y + 4f, Mathf.Max(tamanhoChave.x + 14f, 26f), 24f);
            Preencher(new Rect(tampa.x, tampa.y + 2f, tampa.width, tampa.height), EscuroLataoLoja);
            Preencher(new Rect(tampa.x, tampa.y, tampa.width, tampa.height - 2f), LataoLoja);
            Preencher(new Rect(tampa.x + 2f, tampa.y + 2f, tampa.width - 4f, 2f), LuzLataoLoja);
            TelaCanvas.Texto(new Rect(tampa.x, tampa.y - 1f, tampa.width, tampa.height - 2f), chave, _chaveLoja);

            Vector2 tamanho = _subtituloLoja.CalcSize(new GUIContent(acao));
            TelaCanvas.Texto(new Rect(tampa.xMax + 8f, y + 6f, tamanho.x + 4f, 22f), acao, _subtituloLoja);
            return tampa.xMax + 8f + tamanho.x;
        }

        private void BarraDoChefe(string titulo, float fracao, Color cor)
        {
            float largura = Screen.width * 0.5f;
            float x = (Screen.width - largura) * 0.5f;
            float y = Screen.height - 46f;

            TelaCanvas.Texto(new Rect(x, y - 26f, 520f, 28f), titulo, _rotulo);
            Preencher(new Rect(x - 2f, y - 2f, largura + 4f, 16f), Color.black);
            Preencher(new Rect(x, y, largura, 12f), VaziaCoracao);
            Preencher(new Rect(x, y, largura * fracao, 12f), cor);
        }

        // ------------------------------------------------------------------
        // Caixas
        // ------------------------------------------------------------------

        /// <summary>A caixa de sempre: borda dourada por fora, fundo escuro por dentro.</summary>
        private static void DesenharCaixa(Rect interno)
        {
            Preencher(new Rect(interno.x - BordaCaixa, interno.y - BordaCaixa,
                interno.width + BordaCaixa * 2f, interno.height + BordaCaixa * 2f), CheioCoracao);
            Preencher(interno, PreenchimentoCaixa);
        }

        /// <summary>Os avisos do quadro, um embaixo do outro, cada um na sua caixa, a partir do alto.</summary>
        private void DesenharAvisos()
        {
            float y = Screen.height * 0.12f;

            foreach (string texto in _avisos)
            {
                Vector2 tamanho = _linha.CalcSize(new GUIContent(texto));
                float largura = Mathf.Min(tamanho.x + 40f, Screen.width - 40f);
                const float altura = 40f;

                var caixa = new Rect((Screen.width - largura) * 0.5f, y, largura, altura);
                DesenharCaixa(caixa);
                TelaCanvas.Texto(caixa, texto, _linha);

                y += altura + BordaCaixa * 2f + 8f;
            }
        }

        /// <summary>
        /// Um pedido de botao, numa caixa embaixo do meio da tela. Com
        /// progresso (segurar pra raspar), ganha a barra embaixo do texto.
        /// </summary>
        private void DesenharIndicacao(string texto, float progresso = -1f)
        {
            // Numa cena o jogador nao tem o controle: nao ha botao a pedir.
            if (Cinematic.Ativo) return;

            Vector2 tamanho = _linha.CalcSize(new GUIContent(texto));
            bool barra = progresso >= 0f;

            float largura = Mathf.Max(tamanho.x + 40f, barra ? 280f : 0f);
            float altura = barra ? 58f : 40f;
            var caixa = new Rect((Screen.width - largura) * 0.5f, Screen.height * IndicacaoY, largura, altura);

            DesenharCaixa(caixa);
            TelaCanvas.Texto(new Rect(caixa.x, caixa.y, caixa.width, 40f), texto, _linha);

            if (!barra) return;

            var faixa = new Rect(caixa.x + 20f, caixa.y + 38f, caixa.width - 40f, 10f);
            Preencher(faixa, VaziaCoracao);
            Preencher(new Rect(faixa.x, faixa.y, faixa.width * Mathf.Clamp01(progresso), faixa.height), CheioCoracao);
        }

        // ------------------------------------------------------------------
        // Dialogo
        // ------------------------------------------------------------------

        /// <summary>
        /// A caixa de fala de um NPC, com o busto de quem fala a esquerda, ou
        /// o pedido de botao de que da pra conversar.
        /// </summary>
        private void DesenharDialogo()
        {
            // A fala de cena (um chefe falando) tem a vez.
            if (ScriptedDialogue.Ativo)
            {
                DesenharFala(ScriptedDialogue.Falante, ScriptedDialogue.IdRetrato, ScriptedDialogue.LinhaAtual, null);
                return;
            }

            Npc conversando = Npc.Conversando;

            if (conversando == null)
            {
                _mostradoFalante = null;
                Npc proximoArea = Npc.Proximo;
                if (proximoArea != null && !Cinematic.Ativo) DesenharIndicacao("E: " + proximoArea.Indicacao + " " + proximoArea.NomeExibido);
                return;
            }

            string linha2 = conversando.LinhaAtual;
            if (linha2 == null) return;

            DesenharFala(conversando.NomeExibido, conversando.IdRetrato, linha2, conversando.Documento);
        }

        // Quem esta na caixa agora, e desde quando (pra o busto entrar deslizando).
        private string _mostradoFalante;
        private float _falanteDesde;
        private GUIStyle _nomeFalante;
        private static readonly Color PreenchimentoFala = new Color(0.025f, 0.03f, 0.045f, 0.86f);

        /// <summary>
        /// A caixa de fala, como numa novela visual: o busto grande de quem fala
        /// saindo de baixo da tela, a esquerda, por cima da borda da caixa; a
        /// caixa escura e translucida com cantoneiras finas na cor de quem
        /// fala; o nome em cima, a linha embaixo e a setinha de continuar no
        /// canto. Se ha o que ler, o desenho aparece grande em cima.
        /// </summary>
        private void DesenharFala(string falante, string idRetrato, string linha2, Sprite documento)
        {
            if (linha2 == null) return;

            // A grade de pixel da interface (320x180 ampliada).
            float u = Mathf.Max(1f, Mathf.Floor(Screen.height / 180f));
            Color destaque = SpeakerPortraits.CorDeDestaque(idRetrato);

            string chave = idRetrato + "|" + falante;
            if (chave != _mostradoFalante)
            {
                _mostradoFalante = chave;
                _falanteDesde = Time.unscaledTime;
            }

            float idade = Time.unscaledTime - _falanteDesde;

            // O busto: inteiro, em passos inteiros, uns 70% da altura da tela.
            bool piscar = Mathf.Repeat(Time.unscaledTime + 0.7f, 3.8f) < 0.12f;
            Texture2D busto = idRetrato != null ? SpeakerPortraits.Obter(idRetrato, piscar) : null;
            float escalaAtual = busto != null ? Mathf.Max(1f, Mathf.Floor(Screen.height * 0.72f / busto.height)) : 0f;
            float bustoW = busto != null ? busto.width * escalaAtual : 0f;
            float bustoH = busto != null ? busto.height * escalaAtual : 0f;

            float caixaH = Mathf.Round(Screen.height * 0.27f / u) * u;
            float caixaY = Screen.height - caixaH - 3f * u;
            float caixaX = Mathf.Round(bustoW * 0.62f / u) * u + 6f * u;
            float caixaW = Mathf.Min(Screen.width - caixaX - 6f * u, Mathf.Round(Screen.width * 0.66f / u) * u);
            if (busto == null) caixaX = Mathf.Round((Screen.width - caixaW) * 0.5f / u) * u;

            // O que se le aparece grande, no meio da tela, em cima da caixa.
            if (documento != null) DesenharDocumento(documento, new Rect(40f, 24f, Screen.width - 80f, caixaY - 24f - 20f));

            // A caixa: o fundo, a luz fininha em cima e as cantoneiras.
            var caixa = new Rect(caixaX, caixaY, caixaW, caixaH);
            Preencher(caixa, PreenchimentoFala);
            Preencher(new Rect(caixa.x, caixa.y, caixa.width, u), new Color(destaque.r, destaque.g, destaque.b, 0.12f));
            Cantos(caixa, u, 7f * u, new Color(destaque.r, destaque.g, destaque.b, 0.9f));

            // O busto por cima da borda esquerda, entrando de lado.
            float textoX = caixa.x + 8f * u;
            if (busto != null)
            {
                float entrar = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(idade / 0.22f));
                float bx = Mathf.Round((caixa.x - bustoW * 0.62f - (1f - entrar) * 16f * u) / escalaAtual) * escalaAtual;
                float by = Screen.height - bustoH;

                Color valorAnterior = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, entrar);
                TelaCanvas.Textura(new Rect(bx, by, bustoW, bustoH), busto);
                GUI.color = valorAnterior;

                textoX = caixa.x + bustoW * 0.38f + 6f * u;
            }

            float textoW = caixa.xMax - 12f * u - textoX;

            int tamanhoNome = Mathf.RoundToInt(5.4f * u);
            if (_nomeFalante == null || _nomeFalante.fontSize != tamanhoNome)
            {
                _nomeFalante = new GUIStyle(GUI.skin.label) { fontSize = tamanhoNome, fontStyle = FontStyle.Bold, wordWrap = false };
                _fala.fontSize = Mathf.RoundToInt(5f * u);
            }

            if (!string.IsNullOrEmpty(falante))
            {
                Color valorAnterior = GUI.contentColor;
                GUI.contentColor = destaque;
                TelaCanvas.Texto(new Rect(textoX, caixa.y + 5f * u, textoW, 9f * u), falante, _nomeFalante);
                GUI.contentColor = valorAnterior;
            }

            TelaCanvas.Texto(new Rect(textoX, caixa.y + 16f * u, textoW, caixa.height - 22f * u), linha2, _fala);

            // A setinha de continuar, quicando no canto.
            float balanco = Mathf.Repeat(Time.unscaledTime, 0.9f) < 0.45f ? 0f : u;
            float ax = caixa.xMax - 10f * u;
            float ay = caixa.yMax - 8f * u + balanco;
            Preencher(new Rect(ax, ay, 5f * u, u), destaque);
            Preencher(new Rect(ax + u, ay + u, 3f * u, u), destaque);
            Preencher(new Rect(ax + 2f * u, ay + 2f * u, u, u), destaque);
        }

        /// <summary>As quatro cantoneiras em L, finas, nos cantos da caixa.</summary>
        private static void Cantos(Rect caixa, float grosso, float comprimento, Color cor)
        {
            float l = caixa.x - grosso, r = caixa.xMax, t = caixa.y - grosso, b = caixa.yMax;
            Preencher(new Rect(l, t, comprimento, grosso), cor);
            Preencher(new Rect(l, t, grosso, comprimento), cor);
            Preencher(new Rect(r - comprimento + grosso, t, comprimento, grosso), cor);
            Preencher(new Rect(r, t, grosso, comprimento), cor);
            Preencher(new Rect(l, b, comprimento, grosso), cor);
            Preencher(new Rect(l, b - comprimento + grosso, grosso, comprimento), cor);
            Preencher(new Rect(r - comprimento + grosso, b, comprimento, grosso), cor);
            Preencher(new Rect(r, b - comprimento + grosso, grosso, comprimento), cor);
        }

        /// <summary>
        /// O desenho do que se le (a carta, a placa, o muro): escurece o jogo
        /// atras e mostra a arte o maior possivel dentro da area, em passos
        /// inteiros quando cabe pelo menos o dobro, pra o pixel art nao borrar.
        /// </summary>
        private static void DesenharDocumento(Sprite sprite, Rect area)
        {
            if (sprite.texture == null || area.width <= 0f || area.height <= 0f) return;

            Preencher(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.6f));

            Rect pixels = sprite.rect;
            float ajuste = Mathf.Min(area.width / pixels.width, area.height / pixels.height);
            float escalaAtual = ajuste >= 2f ? Mathf.Floor(ajuste) : ajuste;

            float larguraDesenho = pixels.width * escalaAtual;
            float alturaDesenho = pixels.height * escalaAtual;
            var alvo = new Rect(
                area.x + (area.width - larguraDesenho) * 0.5f,
                area.y + (area.height - alturaDesenho) * 0.5f,
                larguraDesenho, alturaDesenho);

            Texture textura = sprite.texture;
            var uv = new Rect(
                pixels.x / textura.width, pixels.y / textura.height,
                pixels.width / textura.width, pixels.height / textura.height);

            TelaCanvas.TexturaUV(alvo, textura, uv);
        }

        private static void Preencher(Rect retangulo, Color cor)
        {
            Color valorAnterior = GUI.color;
            GUI.color = cor;
            TelaCanvas.Textura(retangulo, Texture2D.whiteTexture);
            GUI.color = valorAnterior;
        }
    }
}
