using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Brass Welder: um Tarnished corrompido, ainda de mascara de soldador.
    /// Patrulha um trecho de chao e, quando ve o Scrap, vira pra ele e avanca
    /// com o macarico. Perto, recolhe o macarico (a chama cresce: e o aviso) e
    /// solta um jato de fogo pra frente.
    ///
    /// Ele apara todo golpe que vem de frente — ergue a manopla e sai faisca.
    /// So da pra feri-lo por tras ou por cima, pulando e batendo pra baixo.
    /// Ao morrer, larga o macarico, ajoelha e a mascara cai.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class BrassWelder : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do WelderArt, no editor.
        public const int QuadroOcioso = 0;
        public const int QuadrosOcioso = 4;
        public const int QuadroAndar = 4;
        public const int QuadrosAndar = 8;
        public const int QuadroPreparo = 12;
        public const int QuadrosPreparo = 2;
        public const int QuadroChama = 14;
        public const int QuadrosChama = 3;
        public const int QuadroBloqueio = 17;
        public const int QuadrosBloqueio = 2;
        public const int Ferido = 19;
        public const int QuadroMorte = 20;
        public const int QuadrosMorte = 6;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [SerializeField] private float velocidade = 1.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("sightRange")] [Tooltip("A que distancia ele percebe o Scrap e parte pra cima.")]
        [SerializeField] private float alcanceVisao = 6f;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Tooltip("Limites da patrulha, em x de mundo.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        [UnityEngine.Serialization.FormerlySerializedAs("flameRange")] [Header("O macarico")]
        [SerializeField] private float alcanceChama = 2.2f;
        [UnityEngine.Serialization.FormerlySerializedAs("windupTime")] [SerializeField] private float tempoPreparo = 0.45f;
        [UnityEngine.Serialization.FormerlySerializedAs("flameTime")] [SerializeField] private float tempoChama = 0.5f;
        [UnityEngine.Serialization.FormerlySerializedAs("recoverTime")] [SerializeField] private float tempoRecuperar = 0.3f;
        [UnityEngine.Serialization.FormerlySerializedAs("cooldown")] [SerializeField] private float recarga = 1.4f;

        private enum State { Patrulha, QuadroPreparo, QuadroChama, Recuperar2 }

        private Transform _jogador;
        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private int _direcaoOlhar = 1;
        private State _estado = State.Patrulha;
        private float _tempoEstado;
        private float _proximoAtaque;
        private float _bloqueioAte = -1f;
        private bool _perseguindo;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, Sprite[] novoQuadros = null)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            quadros = novoQuadros;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();
            _danificavel.Bloqueios = BloqueiaGolpeDe;
            _danificavel.AnimacaoMorte = TombarNoChao;
            _danificavel.Revivido += ReiniciarEstado;
        }

        private void OnDestroy()
        {
            if (_danificavel != null) _danificavel.Revivido -= ReiniciarEstado;
        }

        private void Start()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null) _jogador = controlador.transform;
        }

        private void ReiniciarEstado()
        {
            _estado = State.Patrulha;
            _tempoEstado = 0f;
            _proximoAtaque = 0f;
        }

        /// <summary>
        /// De frente ele apara. Vale golpe vindo do lado pra onde ele olha, a
        /// menos que venha bem de cima.
        /// </summary>
        private bool BloqueiaGolpeDe(Vector2 de)
        {
            Vector2 deslocamento = de - (Vector2)transform.position;

            bool deAcima = deslocamento.y > 0.9f && Mathf.Abs(deslocamento.x) < 1.2f;
            if (deAcima) return false;

            bool bloqueado = Mathf.Sign(deslocamento.x) == _direcaoOlhar;
            if (bloqueado) _bloqueioAte = Time.time + 0.25f;
            return bloqueado;
        }

        private void Update()
        {
            if (_danificavel.EstaMorto) return;

            Vector3 posicao = transform.position;
            _tempoEstado += Time.deltaTime;

            // Logo depois de apanhar ele cambaleia pra longe do golpe e perde o ataque.
            if (Time.time - _danificavel.UltimoTempoGolpe < 0.3f)
            {
                _estado = State.Patrulha;
                posicao.x = Mathf.Clamp(posicao.x - _danificavel.UltimoLadoGolpe * 3f * Time.deltaTime, minX, maxX);
                transform.position = posicao;
                Mostrar(Ferido);
                return;
            }

            switch (_estado)
            {
                case State.Patrulha:
                    Patrulhar(ref posicao);
                    break;

                case State.QuadroPreparo:
                    Mostrar(QuadroPreparo + Mathf.Min(QuadrosPreparo - 1, (int)(_tempoEstado / tempoPreparo * QuadrosPreparo)));
                    if (_tempoEstado >= tempoPreparo) Entrar(State.QuadroChama);
                    break;

                case State.QuadroChama:
                    Mostrar(QuadroChama + Mathf.Min(QuadrosChama - 1, (int)(_tempoEstado / tempoChama * QuadrosChama)));
                    Queimar();
                    if (_tempoEstado >= tempoChama) Entrar(State.Recuperar2);
                    break;

                case State.Recuperar2:
                    Mostrar(QuadroOcioso);
                    if (_tempoEstado >= tempoRecuperar)
                    {
                        _proximoAtaque = Time.time + recarga;
                        Entrar(State.Patrulha);
                    }

                    break;
            }

            transform.position = posicao;
        }

        private void Patrulhar(ref Vector3 posicao)
        {
            _perseguindo = _jogador != null
                && Mathf.Abs(_jogador.position.x - posicao.x) < alcanceVisao
                && Mathf.Abs(_jogador.position.y - posicao.y) < 2.5f;

            float ritmo = velocidade;

            if (_perseguindo)
            {
                _direcaoOlhar = _jogador.position.x >= posicao.x ? 1 : -1;
                ritmo = velocidade * 1.7f;

                bool fechar = Mathf.Abs(_jogador.position.x - posicao.x) < alcanceChama
                    && Mathf.Abs(_jogador.position.y - posicao.y) < 1.2f;
                if (fechar && Time.time >= _proximoAtaque)
                {
                    Entrar(State.QuadroPreparo);
                    return;
                }
            }
            else if (posicao.x >= maxX) _direcaoOlhar = -1;
            else if (posicao.x <= minX) _direcaoOlhar = 1;

            float antes = posicao.x;
            posicao.x = Mathf.Clamp(posicao.x + _direcaoOlhar * ritmo * Time.deltaTime, minX, maxX);

            if (Time.time < _bloqueioAte) Mostrar(QuadroBloqueio + (Time.time + 0.12f > _bloqueioAte ? 1 : 0));
            else if (Mathf.Approximately(antes, posicao.x)) Mostrar(QuadroOcioso + (int)(Time.time * 5f) % QuadrosOcioso);
            else Mostrar(QuadroAndar + (int)(Time.time * (_perseguindo ? 15f : 10f)) % QuadrosAndar);
        }

        /// <summary>O jato de fogo: queima quem estiver na frente, na altura do macarico.</summary>
        private void Queimar()
        {
            Vector2 centro = (Vector2)transform.position + new Vector2(_direcaoOlhar * 1.4f, 0.85f);
            foreach (Collider2D golpe in Physics2D.OverlapBoxAll(centro, new Vector2(1.8f, 0.7f), 0f))
            {
                var vida = golpe.GetComponentInParent<ScrapHealth>();
                if (vida != null) vida.LevarDano(1, transform.position);
            }
        }

        private void Entrar(State estado)
        {
            _estado = estado;
            _tempoEstado = 0f;

            if (estado == State.QuadroPreparo) Sfx.PreparoSoldador.Tocar(transform.position);
            else if (estado == State.QuadroChama) Sfx.ChamaSoldador.Tocar(transform.position);
        }

        private void Mostrar(int quadro)
        {
            if (_sprite == null) return;

            // A arte olha pra direita.
            _sprite.flipX = _direcaoOlhar < 0;
            if (quadros != null && quadro >= 0 && quadro < quadros.Length && quadros[quadro] != null) FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }

        /// <summary>A morte: cambaleia, larga o macarico, ajoelha, a mascara cai, tomba.</summary>
        private IEnumerator TombarNoChao()
        {
            float[] segurar = { 0.14f, 0.14f, 0.2f, 0.2f, 0.25f, 0.45f };
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
            if (_danificavel.EstaMorto) return;

            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(1, transform.position);
        }
    }
}
