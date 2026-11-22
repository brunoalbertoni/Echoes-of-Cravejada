using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Pipe Leech: uma sanguessuga de mangueira de borracha e cobre, grudada
    /// num cano do teto pela ventosa. Fica pulsando ate o Scrap passar
    /// embaixo, e entao despenca. No chao ela se arrasta como lagarta atras
    /// dele e, perto, empina e da o bote. Ao morrer, fura e murcha.
    ///
    /// O pivo do desenho e a base: no teto, o objeto fica 1,5 tile abaixo do
    /// cano, e o corpo pende do alto do desenho.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PipeLeech : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do LeechArt, no editor.
        public const int QuadroRastejar = 0;
        public const int QuadrosRastejar = 6;
        public const int QuadroAgarrar = 6;
        public const int QuadrosAgarrar = 4;
        public const int QuadroSoltar = 10;
        public const int QuadrosSoltar = 2;
        public const int Pousar = 12;
        public const int QuadroPreparoMordida = 13;
        public const int QuadrosPreparoMordida = 2;
        public const int QuadroMordida = 15;
        public const int QuadrosMordida = 3;
        public const int Ferido = 18;
        public const int QuadroMorte = 19;
        public const int QuadrosMorte = 6;

        [UnityEngine.Serialization.FormerlySerializedAs("triggerRange")] [Tooltip("A que distancia, de lado, ela percebe o Scrap embaixo.")]
        [SerializeField] private float alcanceGatilho = 1.3f;

        [UnityEngine.Serialization.FormerlySerializedAs("floorY")] [Tooltip("Altura do chao embaixo dela, em y de mundo (onde ficam os pes).")]
        [SerializeField] private float chaoY;

        [UnityEngine.Serialization.FormerlySerializedAs("crawlSpeed")] [SerializeField] private float velocidadeRastejar = 1.1f;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Tooltip("Ate onde ela se arrasta, em x de mundo.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        [UnityEngine.Serialization.FormerlySerializedAs("biteRange")] [Header("O bote")]
        [SerializeField] private float alcanceMordida = 1.6f;
        [UnityEngine.Serialization.FormerlySerializedAs("biteWindup")] [SerializeField] private float preparoMordida = 0.35f;
        [UnityEngine.Serialization.FormerlySerializedAs("biteTime")] [SerializeField] private float tempoMordida = 0.2f;
        [UnityEngine.Serialization.FormerlySerializedAs("biteDistance")] [SerializeField] private float distanciaMordida = 1f;
        [UnityEngine.Serialization.FormerlySerializedAs("biteRecover")] [SerializeField] private float recuperarMordida = 0.4f;
        [UnityEngine.Serialization.FormerlySerializedAs("biteCooldown")] [SerializeField] private float recargaMordida = 1.2f;

        private enum State { QuadroAgarrar, Caindo, Pousado, QuadroRastejar, QuadroPreparo, QuadroBote, Recuperar2 }

        private Transform _jogador;
        private SpriteRenderer _sprite;
        private BoxCollider2D _caixa;
        private Damageable _danificavel;
        private Vector3 _origem;
        private float _velocidadeQueda;
        private State _estado = State.QuadroAgarrar;
        private float _tempoEstado;
        private float _proximoMordida;
        private int _direcaoOlhar = 1;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoChaoY, float novoMinX, float novoMaxX, Sprite[] novoQuadros)
        {
            chaoY = novoChaoY;
            minX = novoMinX;
            maxX = novoMaxX;
            quadros = novoQuadros;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _caixa = GetComponent<BoxCollider2D>();
            _origem = transform.position;

            _danificavel = GetComponent<Damageable>();
            if (_danificavel != null)
            {
                _danificavel.Revivido += VoltarAoTeto;
                _danificavel.AnimacaoMorte = Estourar;
                _danificavel.SomMorte = Sfx.MurcharMorte;
            }

            Entrar(State.QuadroAgarrar);
        }

        private void OnDestroy()
        {
            if (_danificavel != null) _danificavel.Revivido -= VoltarAoTeto;
        }

        private void Start()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null) _jogador = controlador.transform;
        }

        private void VoltarAoTeto()
        {
            _velocidadeQueda = 0f;
            transform.position = _origem;
            if (_sprite != null) _sprite.color = Color.white;
            Entrar(State.QuadroAgarrar);
        }

        private void Entrar(State estado)
        {
            _estado = estado;
            _tempoEstado = 0f;

            switch (estado)
            {
                case State.Caindo: Sfx.QuedaSanguessuga.Tocar(transform.position); break;
                case State.Pousado: Sfx.PousarSanguessuga.Tocar(transform.position); break;
                case State.QuadroPreparo: Sfx.PreparoSanguessuga.Tocar(transform.position); break;
                case State.QuadroBote: Sfx.MordidaSanguessuga.Tocar(transform.position); break;
            }

            if (_caixa == null) return;

            // No teto, o corpo pende do alto do desenho; no chao, fica embaixo.
            bool pendurado = estado == State.QuadroAgarrar || estado == State.Caindo;
            _caixa.size = pendurado ? new Vector2(0.6f, 1f) : new Vector2(0.9f, 0.6f);
            _caixa.offset = pendurado ? new Vector2(0f, 0.95f) : new Vector2(0f, 0.32f);
        }

        private void Update()
        {
            if (_danificavel != null && _danificavel.EstaMorto) return;

            _tempoEstado += Time.deltaTime;
            Vector3 posicao = transform.position;

            switch (_estado)
            {
                case State.QuadroAgarrar:
                    Mostrar(QuadroAgarrar + (int)(Time.time * 4f + _origem.x) % QuadrosAgarrar);
                    if (_jogador != null && Mathf.Abs(_jogador.position.x - posicao.x) < alcanceGatilho && _jogador.position.y < posicao.y + 0.5f)
                    {
                        Entrar(State.Caindo);
                    }

                    return;

                case State.Caindo:
                    _velocidadeQueda += 40f * Time.deltaTime;
                    posicao.y -= _velocidadeQueda * Time.deltaTime;
                    Mostrar(QuadroSoltar + (_tempoEstado < 0.12f ? 0 : 1));

                    if (posicao.y <= chaoY)
                    {
                        posicao.y = chaoY;
                        Entrar(State.Pousado);
                    }

                    transform.position = posicao;
                    return;

                case State.Pousado:
                    Mostrar(Pousar);
                    if (_tempoEstado > 0.25f) Entrar(State.QuadroRastejar);
                    return;
            }

            // Apanhando: e jogada pra tras e perde o bote.
            bool atordoada = _danificavel != null && Time.time - _danificavel.UltimoTempoGolpe < 0.2f;
            if (atordoada)
            {
                if (_estado != State.QuadroRastejar) Entrar(State.QuadroRastejar);
                posicao.x = Mathf.Clamp(posicao.x - _danificavel.UltimoLadoGolpe * 3f * Time.deltaTime, minX, maxX);
                transform.position = posicao;
                Mostrar(Ferido);
                return;
            }

            switch (_estado)
            {
                case State.QuadroRastejar:
                    if (_jogador != null) _direcaoOlhar = _jogador.position.x >= posicao.x ? 1 : -1;
                    posicao.x = Mathf.Clamp(posicao.x + _direcaoOlhar * velocidadeRastejar * Time.deltaTime, minX, maxX);
                    Mostrar(QuadroRastejar + (int)(Time.time * 8f) % QuadrosRastejar);

                    if (_jogador != null && Time.time >= _proximoMordida
                        && Mathf.Abs(_jogador.position.x - posicao.x) < alcanceMordida
                        && Mathf.Abs(_jogador.position.y - posicao.y) < 1f)
                    {
                        Entrar(State.QuadroPreparo);
                    }

                    break;

                case State.QuadroPreparo:
                    Mostrar(QuadroPreparoMordida + Mathf.Min(QuadrosPreparoMordida - 1, (int)(_tempoEstado / preparoMordida * QuadrosPreparoMordida)));
                    if (_tempoEstado >= preparoMordida) Entrar(State.QuadroBote);
                    break;

                case State.QuadroBote:
                    posicao.x = Mathf.Clamp(posicao.x + _direcaoOlhar * distanciaMordida / tempoMordida * Time.deltaTime, minX, maxX);
                    Mostrar(_tempoEstado < tempoMordida * 0.5f ? QuadroMordida : QuadroMordida + 1);
                    if (_tempoEstado >= tempoMordida) Entrar(State.Recuperar2);
                    break;

                case State.Recuperar2:
                    Mostrar(QuadroMordida + 2);
                    if (_tempoEstado >= recuperarMordida)
                    {
                        _proximoMordida = Time.time + recargaMordida;
                        Entrar(State.QuadroRastejar);
                    }

                    break;
            }

            transform.position = posicao;
        }

        private void Mostrar(int quadro)
        {
            if (_sprite == null) return;

            // A arte olha pra direita; no teto ela nao vira.
            _sprite.flipX = _estado != State.QuadroAgarrar && _direcaoOlhar < 0;

            if (quadros != null && quadro >= 0 && quadro < quadros.Length && quadros[quadro] != null) FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }

        /// <summary>A morte: se estava no teto, despenca antes; no chao, fura, vaza e murcha.</summary>
        private IEnumerator Estourar()
        {
            Vector3 posicao = transform.position;
            float velocidade = 0f;

            while (posicao.y > chaoY)
            {
                velocidade += 40f * Time.deltaTime;
                posicao.y = Mathf.Max(chaoY, posicao.y - velocidade * Time.deltaTime);
                transform.position = posicao;
                _estado = State.Caindo;
                Mostrar(QuadroSoltar + 1);
                yield return null;
            }

            _estado = State.QuadroRastejar;
            float[] segurar = { 0.12f, 0.12f, 0.12f, 0.14f, 0.2f, 0.4f };
            for (int i = 0; i < QuadrosMorte; i++)
            {
                Mostrar(QuadroMorte + i);
                yield return new WaitForSeconds(segurar[i]);
            }

            if (_sprite == null) yield break;
            Color inicio = _sprite.color;
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                _sprite.color = new Color(inicio.r, inicio.g, inicio.b, inicio.a * (1f - t / 0.35f));
                yield return null;
            }
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Encostar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Encostar(outro);
        }

        private void Encostar(Collider2D outro)
        {
            if (_danificavel != null && _danificavel.EstaMorto) return;

            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(1, transform.position);
        }
    }
}
