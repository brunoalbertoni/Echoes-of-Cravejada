using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A abertura do jogo: o Scrap, rejeitado pela Coroa, despencando pela
    /// Calha de Descarte N.º 1 ate o fundo do Poco de Sucata.
    ///
    /// A tela comeca preta e abre na boca da calha, que treme e chacoalha com
    /// algo descendo por dentro. Ele sai rolando, apagado, cai no monte de
    /// sucata num estrondo (poeira, parafusos e chapas voando, a tela
    /// tremendo), quica e fica largado de costas. A etiqueta de remessa vem
    /// atras, planando, e pousa do lado. Os olhos piscam ate pegar, ele se
    /// apoia no cinzel, fica de joelhos, de pe — e o controle e do jogador.
    /// Sem nenhuma frase na tela. E/Espaco/Enter pulam a cena.
    ///
    /// So roda no comeco de um jogo novo: nunca no Continuar (o save guarda
    /// _played) nem chegando de outra cena por uma SceneExit.
    ///
    /// Os quadros (IntroFallArt) e os pontos da calha vem do gerador da fase.
    /// Sem eles (cena antiga), roda o despertar simples de antes.
    /// </summary>
    // Antes do SceneArrival, que consome o recado de quem veio de outra cena.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(ScrapController))]
    public class WakeUpCutscene : MonoBehaviour
    {
        // A ordem dos quadros na folha. E a mesma do IntroFallArt, no editor.
        private const int QuadroTombar = 0;
        private const int QuadrosTombar = 8;
        private const int QuadroPancada = 8;
        private const int Quique = 9;
        private const int DeitarDesligado = 10;
        private const int ApagadoDeitar = 11;
        private const int DeitarLigado = 12;
        private const int CabecaErguer = 13;
        private const int Objeto = 14;
        private const int Ajoelhar = 15;
        private const int Agachar = 16;
        private const int QuantidadeQuadro = 17;

        private const int QuadroPoeira = 0;
        private const int QuadrosPoeira = 6;
        private const int QuadroEntulho = 6;
        private const int QuadrosEntulho = 6;
        private const int Faisca2 = 12;
        private const int QuantidadeEfeitoVisual = 14;

        [UnityEngine.Serialization.FormerlySerializedAs("darkTime")] [SerializeField] private float tempoEscuro = 0.6f;
        [UnityEngine.Serialization.FormerlySerializedAs("fadeTime")] [SerializeField] private float tempoEsmaecer = 1.6f;
        [UnityEngine.Serialization.FormerlySerializedAs("flickerTime")] [SerializeField] private float tempoTremeluzir = 1.4f;

        [UnityEngine.Serialization.FormerlySerializedAs("offColor")] [Tooltip("A cor dele apagado, no despertar simples (sem os quadros da queda).")]
        [SerializeField] private Color desligadoCor = new Color(0.35f, 0.38f, 0.40f);

        [UnityEngine.Serialization.FormerlySerializedAs("fallFrames")] [Header("A queda pela calha")]
        [Tooltip("Os quadros do Scrap caindo e se levantando (IntroFallArt).")]
        [SerializeField] private Sprite[] quadrosQueda;

        [UnityEngine.Serialization.FormerlySerializedAs("fxFrames")] [Tooltip("Poeira, pecas de sucata e faisca (IntroFallArt.Fx).")]
        [SerializeField] private Sprite[] quadrosEfeitoVisual;

        [UnityEngine.Serialization.FormerlySerializedAs("chuteMouth")] [Tooltip("A boca da Calha N.º 1, em unidades de mundo.")]
        [SerializeField] private Vector2 bocaCalha = new Vector2(21.5f, 25.6f);

        [UnityEngine.Serialization.FormerlySerializedAs("impactX")] [Tooltip("Onde ele bate no monte (x). A coluna da calha ate ali nao passa por plataforma.")]
        [SerializeField] private float impactoX = 18.2f;

        [UnityEngine.Serialization.FormerlySerializedAs("restX")] [Tooltip("Onde ele para depois do quique (x), e onde se levanta.")]
        [SerializeField] private float descansoX = 20.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("tagName")] [Tooltip("O nome do legivel da etiqueta de remessa, que cai junto.")]
        [SerializeField] private string nomeEtiqueta = "Lore_TornTag";

        private const float Gravidade = 38f;

        [SaveProgress("_played")]
        private static bool _tocado;

        private ScrapController _controlador;
        private Rigidbody2D _corpo;
        private SpriteRenderer _sprite;
        private ScrapAnimator _animador;
        private bool _pular;

        private readonly List<GameObject> _fx = new List<GameObject>();
        private Transform _arteEtiqueta;
        private Vector3 _origemEtiqueta;
        private Quaternion _rotacaoOrigemEtiqueta;
        private Renderer[] _renderizadoresEtiqueta;
        private Collider2D _areaEtiqueta;
        private bool _pousadoEtiqueta = true;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _tocado = false;
        }

        /// <summary>Usado pelo gerador da fase 1.</summary>
        public void ConfigurarQueda(Sprite[] quadros, Sprite[] fx, Vector2 boca, float impacto, float descanso)
        {
            quadrosQueda = quadros;
            quadrosEfeitoVisual = fx;
            bocaCalha = boca;
            impactoX = impacto;
            descansoX = descanso;
        }

        private void Awake()
        {
            _controlador = GetComponent<ScrapController>();
            _corpo = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _animador = GetComponent<ScrapAnimator>();
        }

        private void Start()
        {
            bool chegando = !string.IsNullOrEmpty(SceneTravel.IdNascimentoPendente);

            // Continuar: o save ja traz _played, mas a bancada carregada tambem diz.
            if (_tocado || chegando || SaveSystem.BancadaCarregado.HasValue) return;

            _tocado = true;

            // Ja comeca no preto, antes do primeiro quadro aparecer.
            Cinematic.Iniciar();
            Cinematic.Esmaecimento = 1f;

            bool temArte = quadrosQueda != null && quadrosQueda.Length >= QuantidadeQuadro
                && quadrosEfeitoVisual != null && quadrosEfeitoVisual.Length >= QuantidadeEfeitoVisual;
            StartCoroutine(temArte ? TocarQueda() : TocarDespertar());
        }

        // ------------------------------------------------------------------
        // A queda
        // ------------------------------------------------------------------

        private IEnumerator TocarQueda()
        {
            yield return EsperarChegada();

            Vector3 nascimentoPonto = transform.position;
            float chao = ChaoEm(descansoX, nascimentoPonto.y);
            float chaoImpacto = ChaoEm(impactoX, nascimentoPonto.y);

            // Some com ele ate sair da calha; a etiqueta tambem espera a vez.
            _controlador.ControleTravado = true;
            _corpo.linearVelocity = Vector2.zero;
            _corpo.simulated = false;
            if (_animador != null) _animador.enabled = false;
            _sprite.enabled = false;
            _sprite.flipX = false;
            EsconderEtiqueta();

            Vector2 visual = bocaCalha + new Vector2(-1f, -3f);
            ScrapCameraFollow.Direcionar(visual, 0.85f, 0.02f);

            yield return Esperar(tempoEscuro);

            // Abre na calha. Algo desce chacoalhando la dentro, cada vez mais perto.
            ScrapCameraFollow.Direcionar(visual, 0.85f, 1.2f);
            Sfx.CalhaIntroducao.Tocar(bocaCalha);
            const float chocalhar = 2.1f;
            float proximoBatida = 0.5f;
            for (float t = 0f; t < chocalhar && !_pular; t += Time.deltaTime)
            {
                Cinematic.Esmaecimento = 1f - Mathf.Clamp01(t / tempoEsmaecer);

                if (t >= proximoBatida)
                {
                    float k = t / chocalhar;
                    ScrapCameraFollow.Tremer(0.02f + 0.05f * k, 0.18f);
                    CriarBaforada(bocaCalha + new Vector2(Random.Range(-1.2f, 1.2f), -0.2f), new Vector2(0f, -1.2f), 0.7f, 0.5f);
                    proximoBatida += Mathf.Lerp(0.45f, 0.14f, k);
                }

                Segurar();
                yield return null;
            }

            Cinematic.Esmaecimento = 0f;

            // Sai rolando pela boca, puxado pro canto do funil amassado.
            if (!_pular)
            {
                _sprite.enabled = true;
                CriarBaforada(bocaCalha + new Vector2(0f, -0.3f), new Vector2(0f, -2f), 1.1f, 0.7f);
                CriarBaforada(bocaCalha + new Vector2(-0.8f, -0.1f), new Vector2(-1.5f, -1f), 0.9f, 0.6f);
                ScrapCameraFollow.Tremer(0.1f, 0.25f);
                StartCoroutine(QuedaDaEtiqueta(bocaCalha, chao));
            }

            // Sai pelo lado esquerdo do funil e escorrega pra coluna livre de plataformas.
            float saidaX = bocaCalha.x - 0.9f;
            float deriva = saidaX - impactoX;
            float vy0 = -2.5f;
            float pousarY = chaoImpacto + 0.5f;
            Vector2 centro = bocaCalha;
            for (float t = 0f; !_pular; t += Time.deltaTime)
            {
                centro.x = saidaX - deriva * (1f - Mathf.Exp(-7f * t));
                centro.y = bocaCalha.y - 0.9f + vy0 * t - 0.5f * Gravidade * t * t;

                if (centro.y <= pousarY) break;

                MostrarQuadro(QuadroTombar + (int)(t * 18f) % QuadrosTombar);
                transform.position = new Vector3(centro.x, centro.y - 1f, nascimentoPonto.z);
                ScrapCameraFollow.Direcionar(new Vector2(centro.x + 0.6f, centro.y - 0.6f), 0.85f, 0.1f);

                Segurar();
                yield return null;
            }

            // O tombo no monte.
            if (!_pular)
            {
                var impacto = new Vector3(impactoX, chaoImpacto, nascimentoPonto.z);
                transform.position = impacto;
                MostrarQuadro(QuadroPancada);

                ScrapCameraFollow.Tremer(0.4f, 0.6f);
                Cinematic.Clarao2 = 0.18f;
                Sfx.ImpactoIntroducao.Tocar(impacto);
                NoiseSystem.Emitir(impacto, 10f, 0.8f);
                Estourar(impacto, chaoImpacto);
                ScrapCameraFollow.Direcionar(new Vector2(impactoX + 0.8f, chaoImpacto + 1.6f), 0.8f, 0.25f);

                yield return Esperar(0.14f);
                Cinematic.Clarao2 = 0f;
            }

            // Quica e escorrega pro lado, ate parar de costas.
            const float salto = 0.42f;
            for (float t = 0f; t < salto && !_pular; t += Time.deltaTime)
            {
                float k = t / salto;
                float x = Mathf.Lerp(impactoX, descansoX, k);
                float y = Mathf.Lerp(chaoImpacto, chao, k) + 0.9f * 4f * k * (1f - k);
                MostrarQuadro(Quique);
                transform.position = new Vector3(x, y - 0.3f * (1f - 4f * k * (1f - k)), nascimentoPonto.z);
                Segurar();
                yield return null;
            }

            var descanso = new Vector3(descansoX, chao, nascimentoPonto.z);
            transform.position = descanso;
            MostrarQuadro(DeitarDesligado);

            if (!_pular)
            {
                Sfx.PousarPedaco.Tocar(descanso, tom2: 0.85f);
                ScrapCameraFollow.Tremer(0.1f, 0.2f);
                CriarBaforada(descanso + new Vector3(-0.6f, 0.3f), new Vector2(-0.8f, 0.4f), 0.8f, 0.6f);
                CriarBaforada(descanso + new Vector3(0.7f, 0.3f), new Vector2(0.8f, 0.4f), 0.8f, 0.6f);
            }

            // Largado. A camera chega perto devagar; a etiqueta acaba de pousar.
            ScrapCameraFollow.Direcionar(new Vector2(descansoX + 0.5f, chao + 1.3f), 0.62f, 1.1f);
            yield return Esperar(1.7f);

            // Os olhos pegando: brasa, apaga, brasa, acende, apaga, acende de vez.
            int[] olhos = { ApagadoDeitar, DeitarDesligado, ApagadoDeitar, DeitarLigado, DeitarDesligado, DeitarLigado };
            float[] pausas = { 0.18f, 0.32f, 0.14f, 0.12f, 0.2f, 0.5f };
            for (int i = 0; i < olhos.Length && !_pular; i++)
            {
                MostrarQuadro(olhos[i]);
                if (olhos[i] != DeitarDesligado) Sfx.TremeluzirOlho.Tocar(descanso, tom2: 0.85f + 0.06f * i);
                if (i == olhos.Length - 1)
                {
                    NoiseSystem.Emitir(descanso, 6f, 0.3f);
                    Sfx.PartidaSucata.Tocar(descanso);
                }

                yield return Esperar(Mathf.Max(pausas[i], tempoTremeluzir / olhos.Length * 0.5f));
            }

            // Levanta: ergue a cabeca, finca o cinzel, joelhos, agacha, de pe.
            (int frame, float hold)[] subida = { (CabecaErguer, 0.4f), (Objeto, 0.45f), (Ajoelhar, 0.4f), (Agachar, 0.22f) };
            foreach ((int quadro, float segurar) in subida)
            {
                if (_pular) break;
                MostrarQuadro(quadro);
                if (quadro == Objeto) Sfx.PousarPedaco.Tocar(descanso, volume: 0.5f, tom2: 1.4f);
                if (quadro == Ajoelhar) Sfx.Passo2.Tocar(descanso);
                if (quadro == Agachar)
                {
                    Sfx.Pousar.Tocar(descanso);
                    CriarBaforada(descanso + new Vector3(0f, 0.2f), new Vector2(0f, 0.3f), 0.6f, 0.45f);
                }

                yield return Esperar(segurar);
            }

            Concluir(descanso);
        }

        /// <summary>Devolve tudo ao jogo: o Scrap de pe, a fisica, a camera, a etiqueta.</summary>
        private void Concluir(Vector3 dePe)
        {
            if (_pular) Cinematic.Esmaecimento = 0f;

            transform.rotation = Quaternion.identity;
            transform.position = dePe;
            _sprite.enabled = true;
            _sprite.color = Color.white;
            if (_animador != null) _animador.enabled = true;

            _corpo.position = dePe;
            _corpo.simulated = true;
            _corpo.linearVelocity = Vector2.zero;

            var renascimento = GetComponent<ScrapRespawn>();
            if (renascimento != null) renascimento.DefinirPosicaoSegura(dePe);

            if (_pular)
            {
                foreach (GameObject peca in _fx) if (peca != null) Destroy(peca);
                _fx.Clear();
            }

            PousarEtiqueta();

            if (!_pular) ScrapCameraFollow.Tremer(0.05f, 0.15f);
            ScrapCameraFollow.Liberar();

            _controlador.ControleTravado = false;
            Cinematic.Encerrar();
        }

        private void MostrarQuadro(int quadro)
        {
            Sprite sprite = quadrosQueda[quadro];
            if (sprite != null) FrameAnimator.Exibir(_sprite, sprite);
        }

        /// <summary>A altura do chao em x, olhando de logo acima do ponto de chegada pra baixo (por baixo das plataformas finas).</summary>
        private static float ChaoEm(float x, float nascimentoY)
        {
            int solo = LayerMask.GetMask("Ground");
            RaycastHit2D golpe = Physics2D.Raycast(new Vector2(x, nascimentoY + 1.2f), Vector2.down, 6f, solo == 0 ? ~0 : solo);
            return golpe.collider != null ? golpe.point.y : nascimentoY;
        }

        // ------------------------------------------------------------------
        // Poeira e sucata
        // ------------------------------------------------------------------

        /// <summary>O estrondo: nuvens pros lados, pecas de sucata voando, faiscas.</summary>
        private void Estourar(Vector3 at, float chao)
        {
            for (int i = 0; i < 6; i++)
            {
                float lado = i % 2 == 0 ? -1f : 1f;
                float espalhamento = 0.4f + 0.35f * (i / 2);
                CriarBaforada(at + new Vector3(lado * espalhamento * 0.6f, 0.25f + 0.1f * i), new Vector2(lado * (1.2f + i * 0.4f), 0.5f + 0.15f * i),
                    0.7f + 0.08f * i, 0.55f + 0.05f * i);
            }

            for (int i = 0; i < 7; i++)
            {
                float angulo = Mathf.Lerp(35f, 145f, (i + Random.Range(-0.3f, 0.3f)) / 6f) * Mathf.Deg2Rad;
                float velocidade = Random.Range(5.5f, 9f);
                var velocidadeVetorial = new Vector2(Mathf.Cos(angulo) * velocidade * 0.7f, Mathf.Sin(angulo) * velocidade);
                StartCoroutine(Voar(QuadroEntulho + i % QuadrosEntulho, at + new Vector3(0f, 0.4f), velocidadeVetorial, chao));
            }

            for (int i = 0; i < 3; i++)
            {
                StartCoroutine(Clarao(at + new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(0.1f, 0.7f)), 0.05f * i));
            }
        }

        private SpriteRenderer CriarEfeito(string nome, int quadro, Vector3 at, int deslocamentoOrdem)
        {
            var peca = new GameObject(nome);
            peca.transform.position = at;
            var renderizador = peca.AddComponent<SpriteRenderer>();
            renderizador.sprite = quadrosEfeitoVisual[quadro];
            renderizador.sortingLayerID = _sprite.sortingLayerID;
            renderizador.sortingOrder = _sprite.sortingOrder + deslocamentoOrdem;
            _fx.Add(peca);
            return renderizador;
        }

        private void CriarBaforada(Vector2 at, Vector2 velocidadeVetorial, float duracaoVida, float escalaAtual)
        {
            StartCoroutine(Baforada(at, velocidadeVetorial, duracaoVida, escalaAtual));
        }

        /// <summary>Uma nuvem que incha, desliza, freia e some.</summary>
        private IEnumerator Baforada(Vector2 at, Vector2 velocidadeVetorial, float duracaoVida, float escalaAtual)
        {
            SpriteRenderer renderizador = CriarEfeito("IntroDust", QuadroPoeira, at, 2);
            Transform peca = renderizador.transform;
            peca.localScale = Vector3.one * escalaAtual;
            Vector3 posicao = at;

            for (float t = 0f; t < duracaoVida && renderizador != null; t += Time.deltaTime)
            {
                float k = t / duracaoVida;
                renderizador.sprite = quadrosEfeitoVisual[QuadroPoeira + Mathf.Min(QuadrosPoeira - 1, (int)(k * QuadrosPoeira))];
                renderizador.color = new Color(1f, 1f, 1f, 1f - k * k);
                velocidadeVetorial *= Mathf.Exp(-3f * Time.deltaTime);
                posicao += (Vector3)(velocidadeVetorial * Time.deltaTime);
                peca.position = posicao;
                yield return null;
            }

            if (renderizador != null)
            {
                _fx.Remove(renderizador.gameObject);
                Destroy(renderizador.gameObject);
            }
        }

        /// <summary>Uma peca de sucata: sobe girando, cai, quica uma vez com um tinido, assenta e some.</summary>
        private IEnumerator Voar(int quadro, Vector3 at, Vector2 velocidadeVetorial, float chao)
        {
            SpriteRenderer renderizador = CriarEfeito("IntroDebris", quadro, at, 1);
            Transform peca = renderizador.transform;
            float giro = Random.Range(-720f, 720f);
            Vector3 posicao = at;
            int quiques = 0;

            for (float t = 0f; t < 4f && renderizador != null; t += Time.deltaTime)
            {
                velocidadeVetorial.y -= Gravidade * 0.8f * Time.deltaTime;
                posicao += (Vector3)(velocidadeVetorial * Time.deltaTime);

                float solo = chao + 0.15f;
                if (posicao.y < solo && velocidadeVetorial.y < 0f)
                {
                    posicao.y = solo;
                    if (quiques++ < 1 && velocidadeVetorial.y < -3f)
                    {
                        velocidadeVetorial = new Vector2(velocidadeVetorial.x * 0.5f, -velocidadeVetorial.y * 0.35f);
                        giro *= 0.5f;
                        Sfx.PousarPedaco.Tocar(posicao, volume: 0.35f, tom2: Random.Range(1.4f, 1.9f));
                    }
                    else
                    {
                        velocidadeVetorial = Vector2.zero;
                        giro = 0f;
                    }
                }

                peca.position = posicao;
                peca.Rotate(0f, 0f, giro * Time.deltaTime);
                if (t > 3f) renderizador.color = new Color(1f, 1f, 1f, 4f - t);
                yield return null;
            }

            if (renderizador != null)
            {
                _fx.Remove(renderizador.gameObject);
                Destroy(renderizador.gameObject);
            }
        }

        private IEnumerator Clarao(Vector3 at, float atraso)
        {
            yield return new WaitForSeconds(atraso);
            SpriteRenderer renderizador = CriarEfeito("IntroSpark", Faisca2, at, 3);
            yield return new WaitForSeconds(0.05f);
            if (renderizador != null) renderizador.sprite = quadrosEfeitoVisual[Faisca2 + 1];
            yield return new WaitForSeconds(0.06f);
            if (renderizador != null)
            {
                _fx.Remove(renderizador.gameObject);
                Destroy(renderizador.gameObject);
            }
        }

        // ------------------------------------------------------------------
        // A etiqueta de remessa
        // ------------------------------------------------------------------

        private void EsconderEtiqueta()
        {
            GameObject etiqueta = GameObject.Find(nomeEtiqueta);
            if (etiqueta == null) return;

            Transform arte = etiqueta.transform.Find("Art");
            _arteEtiqueta = arte != null ? arte : etiqueta.transform;
            _origemEtiqueta = _arteEtiqueta.position;
            _rotacaoOrigemEtiqueta = _arteEtiqueta.rotation;
            _renderizadoresEtiqueta = _arteEtiqueta.GetComponentsInChildren<Renderer>();
            _areaEtiqueta = etiqueta.GetComponent<Collider2D>();

            foreach (Renderer renderizador in _renderizadoresEtiqueta) renderizador.enabled = false;
            if (_areaEtiqueta != null) _areaEtiqueta.enabled = false;
            _pousadoEtiqueta = false;
        }

        /// <summary>Plana da boca da calha ate o lugar dela, balancando como papel.</summary>
        private IEnumerator QuedaDaEtiqueta(Vector2 de, float chao)
        {
            if (_arteEtiqueta == null) yield break;

            yield return new WaitForSeconds(0.3f);
            if (_pousadoEtiqueta) yield break;

            foreach (Renderer renderizador in _renderizadoresEtiqueta) renderizador.enabled = true;

            const float queda = 3.4f;
            Vector3 inicio = new Vector3(de.x, de.y - 0.4f, _origemEtiqueta.z);
            for (float t = 0f; t < queda && !_pousadoEtiqueta; t += Time.deltaTime)
            {
                float k = t / queda;
                float suavizado = 1f - (1f - k) * (1f - k * 0.4f);
                float balancar = Mathf.Sin(t * 4.2f) * 0.9f * (1f - k);
                Vector3 posicao = Vector3.Lerp(inicio, _origemEtiqueta, suavizado);
                posicao.x += balancar;
                _arteEtiqueta.position = posicao;
                _arteEtiqueta.rotation = _rotacaoOrigemEtiqueta * Quaternion.Euler(0f, 0f, Mathf.Cos(t * 4.2f) * 32f * (1f - k));
                yield return null;
            }

            PousarEtiqueta();
        }

        private void PousarEtiqueta()
        {
            if (_pousadoEtiqueta || _arteEtiqueta == null) return;
            _pousadoEtiqueta = true;

            _arteEtiqueta.position = _origemEtiqueta;
            _arteEtiqueta.rotation = _rotacaoOrigemEtiqueta;
            foreach (Renderer renderizador in _renderizadoresEtiqueta) if (renderizador != null) renderizador.enabled = true;
            if (_areaEtiqueta != null) _areaEtiqueta.enabled = true;
        }

        // ------------------------------------------------------------------
        // O despertar simples (cena gerada antes dos quadros da queda)
        // ------------------------------------------------------------------

        private IEnumerator TocarDespertar()
        {
            yield return EsperarChegada();

            Vector3 dePe = transform.position;
            Quaternion ereto = transform.rotation;

            // Caido de lado: sem fisica, deitado e erguido o bastante pra nao
            // afundar no monte (o pivo e nos pes).
            _controlador.ControleTravado = true;
            _corpo.linearVelocity = Vector2.zero;
            _corpo.simulated = false;
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            transform.position = dePe + Vector3.up * Mathf.Max(0f, dePe.y - _sprite.bounds.min.y);
            _sprite.color = desligadoCor;

            yield return Esperar(tempoEscuro);

            for (float t = 0f; t < tempoEsmaecer && !_pular; t += Time.deltaTime)
            {
                Cinematic.Esmaecimento = 1f - t / tempoEsmaecer;
                Segurar();
                yield return null;
            }

            Cinematic.Esmaecimento = 0f;

            // Os olhos pegando: acende, apaga, acende de vez.
            float[] piscadas = { 0.15f, 0.25f, 0.12f, 0.15f, 0.08f, 0.4f };
            float passo = tempoTremeluzir / piscadas.Length;
            for (int i = 0; i < piscadas.Length && !_pular; i++)
            {
                _sprite.color = i % 2 == 0 ? Color.white : desligadoCor;
                if (i % 2 == 0) Sfx.TremeluzirOlho.Tocar(transform.position, tom2: 0.9f + 0.05f * i);
                if (i == piscadas.Length - 1)
                {
                    NoiseSystem.Emitir(transform.position, 6f, 0.3f);
                    Sfx.PartidaSucata.Tocar(transform.position);
                }
                yield return Esperar(Mathf.Max(piscadas[i], passo * 0.5f));
            }

            // Levanta: de volta em pe, com um tranco pra cima.
            const float tempoSubida = 0.25f;
            Vector3 deitado = transform.position;
            Quaternion lado = transform.rotation;
            for (float t = 0f; t < tempoSubida && !_pular; t += Time.deltaTime)
            {
                float k = t / tempoSubida;
                transform.rotation = Quaternion.Slerp(lado, ereto, k);
                transform.position = Vector3.Lerp(deitado, dePe, k);
                Segurar();
                yield return null;
            }

            transform.rotation = ereto;
            transform.position = dePe;
            _sprite.color = Color.white;
            _corpo.position = dePe;
            _corpo.simulated = true;
            _corpo.linearVelocity = _pular ? Vector2.zero : new Vector2(0f, 6f);
            if (!_pular) Sfx.Pulo.Tocar();

            ScrapCameraFollow.Tremer(0.08f, 0.25f);

            // Deixa ele cair de volta no monte antes de soltar o controle.
            yield return null;
            for (float t = 0f; t < 1f && !_controlador.EstaNoChao; t += Time.deltaTime)
            {
                _controlador.ControleTravado = true;
                yield return null;
            }

            _controlador.ControleTravado = false;
            Cinematic.Encerrar();
        }

        // ------------------------------------------------------------------
        // Comum
        // ------------------------------------------------------------------

        /// <summary>Espera o chao existir (o SceneArrival segura o Scrap ate la) e ele assentar.</summary>
        private IEnumerator EsperarChegada()
        {
            for (float t = 0f; t < 3f && !(_corpo.simulated && _controlador.EstaNoChao); t += Time.deltaTime)
            {
                _controlador.ControleTravado = true;
                yield return null;
            }
        }

        private IEnumerator Esperar(float segundos)
        {
            for (float t = 0f; t < segundos && !_pular; t += Time.deltaTime)
            {
                Segurar();
                yield return null;
            }
        }

        /// <summary>Mantem o controle travado e escuta o botao de pular a cena.</summary>
        private void Segurar()
        {
            _controlador.ControleTravado = true;

            // Meio segundo de tolerancia: o botao que comecou o jogo nao pula a cena.
            if (Time.time - Cinematic.TempoInicio > 0.5f && PularPressionado()) _pular = true;
        }

        private static bool PularPressionado()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.eKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame
                || teclado.enterKey.wasPressedThisFrame))
            {
                return true;
            }

            Gamepad controle = Gamepad.current;
            return controle != null && (controle.buttonSouth.wasPressedThisFrame || controle.startButton.wasPressedThisFrame);
        }
    }
}
