using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Rei de Prata, o chefe final. Espera sentado no trono, de cabeca
    /// baixa. Quando o Scrap entra, a cena: os olhos acendem, ele encara o
    /// Scrap e fala (caixa de dialogo); se levanta, desce os degraus e aponta
    /// o cetro. Ai a luta. Ele apanha a qualquer hora, mas cada golpe tem
    /// aviso (ele pisca em branco). Durante a luta, nada de aviso na tela: as
    /// falas so na entrada, na troca de fase e no fim.
    ///
    /// FASE 1 — O REI DE PRATA
    ///   Cetrada      perto: recolhe o cetro por cima do ombro e varre rente
    ///                ao chao. Depois fica apoiado, arfando — exposto, o cinzel
    ///                fere em dobro.
    ///   Raio         meio longe: aponta o cetro; o fio fino mira; o raio varre
    ///                do alto ate o chao na frente dele.
    ///   Facetas      longe: a coroa solta lascas de luz que quicam.
    ///   Coluna       quem fica em cima dele (na sacada, ou pulando pra bater
    ///                na cabeca) leva a Coluna de Luz: ele agacha (o aviso) e
    ///                enfia o cetro pro alto — uma coluna de luz ate o teto e
    ///                dois raios em leque. Ficar la em cima nao e seguro.
    ///
    /// A TRANSFORMACAO: a vida zera, ele cai de joelhos e a coroa racha; ele
    /// fala; a cutscene desenhada; ele se ergue inteiro em Oxide, flutuando.
    /// A vida volta cheia, a sala fica verde, os espelhos quebram e viram
    /// escudo.
    ///
    /// FASE 2 — O REI CORROMPIDO (flutua, mais rapido, mais vida)
    ///   Cetrada, Coluna (mais rapidas), Tesoura (o raio pros dois lados),
    ///   Chuva de facetas, Sopro de Oxide (so os cacos de espelho ou o alto
    ///   protegem; depois arfa, exposto) e Investida (desliza a sala toda).
    ///
    /// Ao cair: larga o cetro, ajoelha, a coroa rola, as ultimas palavras e a
    /// passagem pro Altar do Coracao abre. Se o Scrap morre no meio da luta,
    /// tudo volta ao comeco: o Rei sentado no trono, na fase 1.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public partial class SilverKing : MonoBehaviour, IDamageable, IBossFight
    {
        // A ordem dos quadros. E a mesma do KingArt, no editor (as duas folhas).
        public const int QuadroSentado = 0;
        public const int QuadrosSentado = 4;
        public const int QuadroSubida = 4;
        public const int QuadrosSubida = 10;
        public const int QuadroOcioso = 14;
        public const int QuadrosOcioso = 6;
        public const int QuadroAndar = 20;
        public const int QuadrosAndar = 8;
        public const int QuadroPreparoVarrida = 28;
        public const int QuadrosPreparoVarrida = 2;
        public const int QuadroVarrida = 30;
        public const int QuadrosVarrida = 3;
        public const int QuadroExposto = 33;
        public const int QuadrosExposto = 2;
        public const int QuadroErguerRaio = 35;
        public const int QuadrosErguerRaio = 2;
        public const int QuadroDisparoRaio = 37;
        public const int QuadrosDisparoRaio = 3;
        public const int QuadroLancamento = 40;
        public const int QuadrosLancamento = 3;
        public const int QuadroParaCima = 43;
        public const int QuadrosParaCima = 3;
        public const int QuadroInspirar = 46;
        public const int QuadrosInspirar = 3;
        public const int QuadroSopro = 49;
        public const int QuadrosSopro = 3;
        public const int Ferido = 52;
        public const int QuadroQueda = 53;
        public const int QuadrosQueda = 4;
        public const int QuadroMorte = 57;
        public const int QuadrosMorte = 8;

        private const string Nome = "O Rei de Prata";

        private static readonly string[] LinhasIntroducao =
        {
            "...Uma mancha. No meu palácio.",
            "Eu conheço você. O protótipo de sucata. Fui eu que assinei: \"que não se fale mais nisso\".",
            "E agora a sucata sobe as minhas escadas achando que vai consertar o meu reino?",
            "A prata não se curva à ferrugem. Vem.",
        };

        private static readonly string[] LinhasTransicao =
        {
            "Não... O meu brilho não pode... apagar.",
            "Se a minha luz não basta... que seja a do Coração inteiro.",
        };

        private static readonly string[] LinhasErguido =
        {
            "Agora a Coluna inteira brilha só pra mim, sucata. E nada que brilha pode me tocar.",
        };

        private static readonly string[] UltimoLinhas =
        {
            "...Uma sucata. Derrubado por uma sucata. Que desperdício de rei.",
            "Essa cidade nunca me mereceu. Eu fui a coisa mais bonita que ela já teve.",
            "Pode apagar a luz, sucata... Quem viu o meu brilho nunca vai esquecer.",
        };

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Header("Sala")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;
        [UnityEngine.Serialization.FormerlySerializedAs("floorY")] [SerializeField] private float chaoY;
        [UnityEngine.Serialization.FormerlySerializedAs("ceilingY")] [SerializeField] private float tetoY;
        [UnityEngine.Serialization.FormerlySerializedAs("wakeArea")] [SerializeField] private Rect areaDespertar;

        [UnityEngine.Serialization.FormerlySerializedAs("phaseOneHealth")] [Header("Vida")]
        [SerializeField] private int vidaUmFase = 80;
        [UnityEngine.Serialization.FormerlySerializedAs("phaseTwoHealth")] [SerializeField] private int vidaDoisFase = 110;

        [UnityEngine.Serialization.FormerlySerializedAs("scepterTip")] [Header("Pontos (em tiles a partir do pivo, olhando pra direita)")]
        [SerializeField] private Vector2 pontaCetro = new Vector2(3.4f, 3.1f);
        [UnityEngine.Serialization.FormerlySerializedAs("skywardTip")] [SerializeField] private Vector2 pontaParaCima = new Vector2(1f, 7f);
        [UnityEngine.Serialization.FormerlySerializedAs("crownPoint")] [SerializeField] private Vector2 pontoCoroa = new Vector2(-0.3f, 5.5f);
        [UnityEngine.Serialization.FormerlySerializedAs("mouthPoint")] [SerializeField] private Vector2 pontoBoca = new Vector2(1.5f, 4f);

        [UnityEngine.Serialization.FormerlySerializedAs("sweepReach")] [Tooltip("Onde o cetro varre, em tiles a frente dele.")]
        [SerializeField] private float alcanceVarrida = 3.1f;

        [UnityEngine.Serialization.FormerlySerializedAs("beamA")] [Header("Pecas")]
        [SerializeField] private KingBeam aRaio;
        [UnityEngine.Serialization.FormerlySerializedAs("beamB")] [SerializeField] private KingBeam bRaio;
        [UnityEngine.Serialization.FormerlySerializedAs("breath")] [SerializeField] private OxideBreath sopro;
        [UnityEngine.Serialization.FormerlySerializedAs("facetSprite")] [SerializeField] private Sprite spriteFaceta;
        [UnityEngine.Serialization.FormerlySerializedAs("facetCorruptSprite")] [SerializeField] private Sprite spriteCorrompidoFaceta;
        [UnityEngine.Serialization.FormerlySerializedAs("glintSprite")] [SerializeField] private Sprite spriteBrilhoCurto;

        [UnityEngine.Serialization.FormerlySerializedAs("wreck")] [Tooltip("A carcaca que fica no chao depois da queda (ver BossWreck). Vazio: nao fica nada.")]
        [SerializeField] private GameObject destrocos;

        [UnityEngine.Serialization.FormerlySerializedAs("phaseOneObjects")] [Header("A sala na fase 2")]
        [SerializeField] private GameObject[] objetosUmFase;
        [UnityEngine.Serialization.FormerlySerializedAs("phaseTwoObjects")] [SerializeField] private GameObject[] objetosDoisFase;
        [UnityEngine.Serialization.FormerlySerializedAs("globalLight")] [SerializeField] private Light2D luzGlobal;
        [UnityEngine.Serialization.FormerlySerializedAs("phaseTwoLight")] [SerializeField] private Color luzDoisFase = new Color(0.55f, 0.85f, 0.65f);

        [UnityEngine.Serialization.FormerlySerializedAs("cartoon")] [Header("A cutscene e o fim")]
        [SerializeField] private FilmCutscene desenho2;
        [UnityEngine.Serialization.FormerlySerializedAs("altarGate")] [SerializeField] private Gate portaoAltar;
        [UnityEngine.Serialization.FormerlySerializedAs("fallenCrown")] [SerializeField] private GameObject coroaCaido;

        [UnityEngine.Serialization.FormerlySerializedAs("passThrough")] [Header("Quadros")]
        [Tooltip("As sacadas da sala: a luz e as lascas passam por elas.")]
        [SerializeField] private Rect[] atravesPassar;

        [UnityEngine.Serialization.FormerlySerializedAs("regalFrames")] [SerializeField] private Sprite[] quadrosReal;
        [UnityEngine.Serialization.FormerlySerializedAs("corruptFrames")] [SerializeField] private Sprite[] quadrosCorrompido;

        private SpriteRenderer _sprite;
        private Collider2D _colisor;
        private Transform _jogador;
        private Vector3 _origem;
        private Color _luzBase = Color.white;

        private int _hp;
        private bool _acordado;
        private bool _morto;
        private bool _doisFase;
        private bool _cena;
        private bool _carregando;
        private bool _recuavel;
        private int _direcaoOlhar = -1;
        private int _ultimoAtaque = -1;
        private float _expostoAte;
        private float _feridoAte = -1f;
        private float _prontoParaCima;
        private float _proximoPasso;

        private readonly FramePlayback _reproducao = new FramePlayback(QuadrosSentado, 2f);

        [SaveProgress("_defeated")]
        private static bool _derrotado;
        [SaveProgress("_introHeard")]
        private static bool _ouvidoIntroducao;
        [SaveProgress("_transformationSeen")]
        private static bool _vistoTransformacao;

        /// <summary>O Rei em luta, se houver. O HUD usa pra barra.</summary>
        public static SilverKing Lutando { get; private set; }

        /// <summary>Se ele ja caiu de vez (a trilha sonora usa pra calar a musica da luta).</summary>
        public bool EstaDerrubado => _morto;

        public MusicTrack MusicaBatalha => EstaTransformando ? null : EstaDoisFase ? SoundtrackCatalog.CorrompidaReiPrata : SoundtrackCatalog.RadianteReiPrata;

        public bool CinematicasIgnoraMusica => true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegistrarLuta() => BossRoster.Adicionar(0, () => Lutando);

        public static bool Derrotado => _derrotado;

        public float FracaoVida => Mathf.Clamp01(_hp / (float)Mathf.Max(1, _doisFase ? vidaDoisFase : vidaUmFase));

        public bool EstaDoisFase => _doisFase;

        /// <summary>Verdadeiro durante as cenas (a entrada, a transformacao): o HUD esconde a barra.</summary>
        public bool EstaTransformando => _cena;

        /// <summary>Se ele esta puxando forca pro proximo golpe (o aviso): o Coracao, la embaixo, responde.</summary>
        public bool EstaForcaDesenhando => Time.time < _desenhoAte;

        private float _desenhoAte = -100f;

        /// <summary>Verdadeiro enquanto ele esta exposto (apoiado no cetro ou arfando): o cinzel fere em dobro.</summary>
        public bool EstaExposto => Time.time < _expostoAte;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Lutando = null;
            _derrotado = false;
            _ouvidoIntroducao = false;
            _vistoTransformacao = false;
        }

        /// <summary>Usado pelos geradores de fase: a sala e os pontos do desenho.</summary>
        public void Configurar(float novoMinX, float novoMaxX, float novoChaoY, float novoTetoY, Rect novoAreaDespertar,
            Vector2 ponta, Vector2 paraCima, Vector2 coroa, Vector2 boca, float alcanceGolpe)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            chaoY = novoChaoY;
            tetoY = novoTetoY;
            areaDespertar = novoAreaDespertar;
            pontaCetro = ponta;
            pontaParaCima = paraCima;
            pontoCoroa = coroa;
            pontoBoca = boca;
            alcanceVarrida = alcanceGolpe;
        }

        /// <summary>Usado pelos geradores de fase: os golpes.</summary>
        public void ConfigurarAtaques(KingBeam a, KingBeam b, OxideBreath novoSopro, Sprite faceta, Sprite corrompidoFaceta, Sprite brilhoCurto)
        {
            aRaio = a;
            bRaio = b;
            sopro = novoSopro;
            spriteFaceta = faceta;
            spriteCorrompidoFaceta = corrompidoFaceta;
            spriteBrilhoCurto = brilhoCurto;
        }

        /// <summary>Usado pelos geradores de fase: o que muda na sala, a cutscene e o fim.</summary>
        public void ConfigurarPalco(GameObject[] umObjetos, GameObject[] doisObjetos, Light2D global,
            FilmCutscene novoDesenho, Gate portao, GameObject coroa)
        {
            objetosUmFase = umObjetos;
            objetosDoisFase = doisObjetos;
            luzGlobal = global;
            desenho2 = novoDesenho;
            portaoAltar = portao;
            coroaCaido = coroa;
        }

        /// <summary>Usado pelos geradores de fase: as duas folhas do KingArt.</summary>
        /// <summary>A carcaca que fica no lugar dele depois da queda. Usado pelo gerador.</summary>
        public void ConfigurarDestrocos(GameObject novoDestrocos)
        {
            destrocos = novoDestrocos;
        }

        public void ConfigurarQuadros(Sprite[] real, Sprite[] corrompido)
        {
            quadrosReal = real;
            quadrosCorrompido = corrompido;
        }

        /// <summary>Usado pelos geradores de fase: as sacadas, que a luz e as lascas atravessam.</summary>
        public void ConfigurarPassagem(Rect[] retangulos)
        {
            atravesPassar = retangulos;
        }

        /// <summary>
        /// Se um ponto de batida e de sacada (plataforma): o raio e as lascas
        /// seguem direto, so param em parede, chao e teto. Sacada nao e abrigo.
        /// </summary>
        public static bool EPassavel(Vector2 ponto)
        {
            if (_atravesPassar == null) return false;

            foreach (Rect retangulo in _atravesPassar)
            {
                if (ponto.x >= retangulo.xMin - 0.05f && ponto.x <= retangulo.xMax + 0.05f &&
                    ponto.y >= retangulo.yMin - 0.05f && ponto.y <= retangulo.yMax + 0.05f) return true;
            }

            return false;
        }

        private static Rect[] _atravesPassar;

        private void Awake()
        {
            _atravesPassar = atravesPassar;
            _sprite = GetComponent<SpriteRenderer>();
            _colisor = GetComponent<Collider2D>();
            _corpo = _colisor as BoxCollider2D;
            _origem = transform.position;
            if (luzGlobal != null) _luzBase = luzGlobal.color;

            ScrapHealth.Morreu += ReiniciarLuta;
        }

        private void Start()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null) _jogador = controlador.transform;

            if (_derrotado)
            {
                // Ja caiu: fica deitado, a coroa no chao, o Altar aberto.
                _morto = true;
                _doisFase = true;
                DefinirAtivos(objetosUmFase, false);
                DefinirAtivos(objetosDoisFase, true);
                if (luzGlobal != null) luzGlobal.color = luzDoisFase;
                Tocar(QuadroMorte + QuadrosMorte - 1, 1, 1f, false);
                DeixarDestrocos();
                if (_colisor != null) _colisor.enabled = false;
                if (coroaCaido != null) coroaCaido.SetActive(true);
                if (portaoAltar != null) portaoAltar.Abrir();
                return;
            }

            if (coroaCaido != null) coroaCaido.SetActive(false);
            EntrarFaseUm();
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= ReiniciarLuta;
            if (Lutando == this) Lutando = null;
        }

        /// <summary>O comeco de tudo: sentado no trono, a sala de prata, a vida cheia.</summary>
        private void EntrarFaseUm()
        {
            _doisFase = false;
            _hp = vidaUmFase;
            transform.position = _origem;
            Virar(-1);
            Tocar(QuadroSentado, QuadrosSentado, 2f);
            DefinirAtivos(objetosUmFase, true);
            DefinirAtivos(objetosDoisFase, false);
            if (luzGlobal != null) luzGlobal.color = _luzBase;
        }

        private void AplicarVisualFaseDois()
        {
            _doisFase = true;
            DefinirAtivos(objetosUmFase, false);
            DefinirAtivos(objetosDoisFase, true);
            if (luzGlobal != null) luzGlobal.color = luzDoisFase;
        }

        private void Update()
        {
            if (_sprite != null && !_morto && Time.time >= _feridoAte)
            {
                // Exposto: um pouco em brasa (a deixa pra bater).
                Color alvo = EstaExposto ? new Color(1f, 0.82f, 0.72f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, alvo, 10f * Time.deltaTime);
            }

            if (_morto || _jogador == null || _acordado) return;
            if (!areaDespertar.Contains(_jogador.position)) return;

            _acordado = true;
            Lutando = this;
            StartCoroutine(Introducao());
        }

        // ------------------------------------------------------------------
        // A entrada
        // ------------------------------------------------------------------

        /// <summary>A cena da entrada: acorda, encara, fala, se levanta, desce do trono e aponta o cetro.</summary>
        private IEnumerator Introducao()
        {
            _cena = true;
            var controlador = ScrapRef.Controlador;
            bool comecou = !Cinematic.Ativo;
            if (comecou) Cinematic.Iniciar();
            if (controlador != null) controlador.ControleTravado = true;

            ScrapCameraFollow.Direcionar((Vector2)transform.position + Vector2.up * 3.5f, 0.9f, 0.7f);
            Virar(_jogador != null && _jogador.position.x >= transform.position.x ? 1 : -1);

            // Os olhos acendem, a cabeca sobe, ele encara.
            float[] despertar = { 0.55f, 0.55f, 0.5f };
            Sfx.DespertarRei.Tocar(transform.position + Vector3.up * 3f);
            for (int i = 0; i < despertar.Length; i++)
            {
                Tocar(QuadroSubida + i, 1, 1f, false);
                yield return new WaitForSeconds(despertar[i]);
            }

            if (!_ouvidoIntroducao)
            {
                _ouvidoIntroducao = true;
                yield return ScriptedDialogue.Tocar(Nome, _sprite, LinhasIntroducao);
            }

            // Firma o cetro, inclina, se levanta, desce os degraus, aponta.
            float[] subida = { 0.3f, 0.3f, 0.3f, 0.35f, 0.25f, 0.25f, 0.55f };
            for (int i = 0; i < subida.Length; i++)
            {
                Tocar(QuadroSubida + 3 + i, 1, 1f, false);
                if (i == 5) ScrapCameraFollow.Tremer(0.15f, 0.3f);
                SomDeSubida(i);
                yield return new WaitForSeconds(subida[i]);
            }

            ScrapCameraFollow.Liberar();
            if (controlador != null) controlador.ControleTravado = false;
            if (comecou) Cinematic.Encerrar();
            _cena = false;

            _prontoParaCima = Time.time + 1f;
            yield return Lutar();
        }

        /// <summary>
        /// Os sons da subida do trono, um por quadro: o cetro firmando, a
        /// armadura rangendo, os passos nos degraus (o ultimo, que treme a
        /// sala, mais pesado) e o cetro apontado.
        /// </summary>
        private void SomDeSubida(int passo)
        {
            Vector2 at = transform.position;
            switch (passo)
            {
                case 0: Sfx.FincarCetroRei.Tocar(at); break;
                case 1:
                case 2: Sfx.ArmaduraRei.Tocar(at); break;
                case 3:
                case 4: Sfx.PassoRei.Tocar(at); break;
                case 5: Sfx.PassoRei.Tocar(at, tom2: 0.85f); Sfx.ArmaduraRei.Tocar(at, volume: 0.7f); break;
                default: Sfx.PontoRei.Tocar(at); break;
            }
        }

        private BoxCollider2D _corpo;
        // ------------------------------------------------------------------
        // Animacao
        // ------------------------------------------------------------------

        private static void DefinirAtivos(GameObject[] objetos, bool ativo)
        {
            if (objetos == null) return;
            foreach (GameObject alvo in objetos) if (alvo != null) alvo.SetActive(ativo);
        }

        /// <summary>Toca uma sequencia da folha. Em loop, pedir a mesma de novo nao reinicia.</summary>
        private void Tocar(int inicio, int quantidade, float quadrosPorSegundo, bool repeticao = true) =>
            _reproducao.Tocar(inicio, quantidade, quadrosPorSegundo, repeticao);

        private void LateUpdate()
        {
            Sprite[] conjunto = _doisFase ? quadrosCorrompido : quadrosReal;
            if (_sprite == null || conjunto == null || conjunto.Length == 0) return;

            int quadro;
            if (_recuavel && !_morto && Time.time < _feridoAte)
            {
                quadro = Ferido;
            }
            else
            {
                quadro = _reproducao.Quadro;
            }

            if (quadro >= 0 && quadro < conjunto.Length && conjunto[quadro] != null) FrameAnimator.Exibir(_sprite, conjunto[quadro]);
        }
    }
}
