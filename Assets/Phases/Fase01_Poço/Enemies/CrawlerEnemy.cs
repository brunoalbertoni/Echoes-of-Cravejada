using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um autômato corrompido que rasteja de um lado pro outro num trecho de
    /// chao e machuca quem encosta. E o primeiro inimigo do jogo, e existe pra
    /// ensinar a bater: quando o Scrap chega perto, ele recua, abre as pincas
    /// (o aviso) e da um bote curto pra frente. Apanhar no meio do aviso
    /// cancela o bote.
    ///
    /// O trecho e dado pelo gerador da fase, entao ele nao precisa sondar o
    /// chao: so vai e volta entre dois pontos. Ao morrer ele se desmonta (ver
    /// Damageable.DeathAnimation) antes de sumir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CrawlerEnemy : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do CrawlerArt, no editor.
        public const int QuadroAndar = 0;
        public const int QuadrosAndar = 8;
        public const int QuadroPreparo = 8;
        public const int QuadrosPreparo = 2;
        public const int QuadroMordida = 10;
        public const int QuadrosMordida = 3;
        public const int Ferido = 13;
        public const int QuadroMorte = 14;
        public const int QuadrosMorte = 6;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [SerializeField] private float velocidade = 1.4f;
        [UnityEngine.Serialization.FormerlySerializedAs("contactDamage")] [SerializeField] private int danoContato = 1;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Tooltip("Limites da patrulha, em x de mundo.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;
        [UnityEngine.Serialization.FormerlySerializedAs("walkFramesPerSecond")] [SerializeField] private float andarQuadrosPorSegundo = 11f;

        [UnityEngine.Serialization.FormerlySerializedAs("attackRange")] [Header("O bote")]
        [Tooltip("A que distancia do Scrap ele resolve dar o bote, em tiles.")]
        [SerializeField] private float alcanceAtaque = 2.2f;
        [UnityEngine.Serialization.FormerlySerializedAs("windupTime")] [SerializeField] private float tempoPreparo = 0.5f;
        [UnityEngine.Serialization.FormerlySerializedAs("lungeTime")] [SerializeField] private float tempoBote = 0.18f;
        [UnityEngine.Serialization.FormerlySerializedAs("lungeDistance")] [SerializeField] private float distanciaBote = 1.2f;
        [UnityEngine.Serialization.FormerlySerializedAs("recoverTime")] [SerializeField] private float tempoRecuperar = 0.45f;
        [UnityEngine.Serialization.FormerlySerializedAs("attackCooldown")] [SerializeField] private float recargaAtaque = 1.4f;

        private enum State { QuadroAndar, QuadroPreparo, QuadroBote, Recuperar2 }

        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private Transform _jogador;
        private int _direcao = 1;
        private int _direcaoInicio = 1;
        private State _estado = State.QuadroAndar;
        private float _tempoEstado;
        private float _proximoAtaque;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, Sprite[] novoQuadros)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            quadros = novoQuadros;
        }

        private void Awake()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();

            // Cada um comeca indo pra um lado.
            _direcaoInicio = Mathf.RoundToInt(transform.position.x) % 2 == 0 ? 1 : -1;
            _direcao = _direcaoInicio;

            if (_danificavel != null)
            {
                _danificavel.AnimacaoMorte = Desmontar;
                _danificavel.Revivido += ReiniciarEstado;
            }
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
            _estado = State.QuadroAndar;
            _direcao = _direcaoInicio;
            _proximoAtaque = 0f;
        }

        private void Update()
        {
            if (_danificavel != null && _danificavel.EstaMorto) return;

            Vector3 posicao = transform.position;
            _tempoEstado += Time.deltaTime;

            // Logo depois de apanhar ele e empurrado pra longe do golpe, parado,
            // e perde o bote que estava armando.
            bool atordoada = _danificavel != null && Time.time - _danificavel.UltimoTempoGolpe < 0.25f;
            if (atordoada)
            {
                if (_estado != State.QuadroAndar) Entrar(State.QuadroAndar);
                _proximoAtaque = Mathf.Max(_proximoAtaque, Time.time + 0.6f);

                posicao.x = Mathf.Clamp(posicao.x - _danificavel.UltimoLadoGolpe * 3.5f * Time.deltaTime, minX, maxX);
                transform.position = posicao;
                MostrarQuadro(Ferido);
                return;
            }

            switch (_estado)
            {
                case State.QuadroAndar:
                    posicao.x = Mathf.Clamp(posicao.x + _direcao * velocidade * Time.deltaTime, minX, maxX);
                    if (posicao.x >= maxX) _direcao = -1;
                    else if (posicao.x <= minX) _direcao = 1;

                    MostrarQuadro(QuadroAndar + (int)(Time.time * andarQuadrosPorSegundo) % QuadrosAndar);

                    if (Time.time >= _proximoAtaque && JogadorAoAlcance(posicao))
                    {
                        _direcao = _jogador.position.x >= posicao.x ? 1 : -1;
                        Entrar(State.QuadroPreparo);
                    }

                    break;

                case State.QuadroPreparo:
                    MostrarQuadro(QuadroPreparo + Mathf.Min(QuadrosPreparo - 1, (int)(_tempoEstado / tempoPreparo * QuadrosPreparo)));
                    if (_tempoEstado >= tempoPreparo) Entrar(State.QuadroBote);
                    break;

                case State.QuadroBote:
                    posicao.x = Mathf.Clamp(posicao.x + _direcao * distanciaBote / tempoBote * Time.deltaTime, minX, maxX);
                    MostrarQuadro(_tempoEstado < tempoBote * 0.5f ? QuadroMordida : QuadroMordida + 1);
                    if (_tempoEstado >= tempoBote) Entrar(State.Recuperar2);
                    break;

                case State.Recuperar2:
                    MostrarQuadro(QuadroMordida + 2);
                    if (_tempoEstado >= tempoRecuperar)
                    {
                        _proximoAtaque = Time.time + recargaAtaque;
                        Entrar(State.QuadroAndar);
                    }

                    break;
            }

            transform.position = posicao;
        }

        private void Entrar(State estado)
        {
            _estado = estado;
            _tempoEstado = 0f;

            if (estado == State.QuadroPreparo) Sfx.PreparoRastejante.Tocar(transform.position);
            else if (estado == State.QuadroBote) Sfx.MordidaRastejante.Tocar(transform.position);
        }

        /// <summary>O Scrap esta perto, na mesma altura, e dentro do trecho que ele alcanca.</summary>
        private bool JogadorAoAlcance(Vector3 posicao)
        {
            if (_jogador == null) return false;

            Vector3 alvo = _jogador.position;
            if (Mathf.Abs(alvo.y - (posicao.y - 0.5f)) > 1.2f) return false;
            if (Mathf.Abs(alvo.x - posicao.x) > alcanceAtaque) return false;

            return alvo.x > minX - 1f && alvo.x < maxX + 1f;
        }

        private void MostrarQuadro(int quadro)
        {
            if (_sprite == null) return;

            // A arte olha pra direita.
            _sprite.flipX = _direcao < 0;

            if (quadros != null && quadro >= 0 && quadro < quadros.Length && quadros[quadro] != null)
            {
                FrameAnimator.Exibir(_sprite, quadros[quadro]);
            }
        }

        /// <summary>
        /// A morte: a chave salta, as patas cedem, o casco tomba e a cabeca
        /// rola. Os pedacos ficam um instante no chao e somem.
        /// </summary>
        private IEnumerator Desmontar()
        {
            float[] segurar = { 0.12f, 0.1f, 0.12f, 0.12f, 0.2f, 0.45f };

            for (int i = 0; i < QuadrosMorte; i++)
            {
                MostrarQuadro(QuadroMorte + i);
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
            if (vida != null) vida.LevarDano(danoContato, transform.position);
        }
    }
}
