using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Acid Bloater: um tanque ambulante de acido. Lento e resistente. Quando
    /// o Scrap fica na frente dele, abaixa, gira a valvula (o aviso) e borrifa
    /// acido pelo bico. Ao morrer ele incha, o vidro racha e ele estoura,
    /// deixando uma poca de acido no chao por alguns segundos — entao e melhor
    /// nao mata-lo onde voce vai pisar.
    ///
    /// O pivo do desenho e a base: a posicao e onde os pes pisam.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class AcidBloater : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do BloaterArt, no editor.
        public const int QuadroAndar = 0;
        public const int QuadrosAndar = 6;
        public const int QuadroPreparo = 6;
        public const int QuadrosPreparo = 2;
        public const int QuadroJato = 8;
        public const int QuadrosJato = 3;
        public const int Ferido = 11;
        public const int QuadroInchar = 12;
        public const int QuadrosInchar = 4;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [SerializeField] private float velocidade = 0.8f;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Tooltip("Limites da patrulha, em x de mundo.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;

        [UnityEngine.Serialization.FormerlySerializedAs("puddleTime")] [Tooltip("Segundos que a poca fica no chao.")]
        [SerializeField] private float tempoPoca = 4f;

        [UnityEngine.Serialization.FormerlySerializedAs("puddleWidth")] [Tooltip("Largura da poca, em tiles.")]
        [SerializeField] private float larguraPoca = 2.4f;

        [UnityEngine.Serialization.FormerlySerializedAs("puddleSprite")] [SerializeField] private Sprite spritePoca;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        [UnityEngine.Serialization.FormerlySerializedAs("sprayRange")] [Header("O borrifo")]
        [SerializeField] private float alcanceJato = 2.4f;
        [UnityEngine.Serialization.FormerlySerializedAs("windupTime")] [SerializeField] private float tempoPreparo = 0.5f;
        [UnityEngine.Serialization.FormerlySerializedAs("sprayTime")] [SerializeField] private float tempoJato = 0.6f;
        [UnityEngine.Serialization.FormerlySerializedAs("recoverTime")] [SerializeField] private float tempoRecuperar = 0.3f;
        [UnityEngine.Serialization.FormerlySerializedAs("cooldown")] [SerializeField] private float recarga = 2.5f;

        private enum State { QuadroAndar, QuadroPreparo, QuadroJato, Recuperar2 }

        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private Transform _jogador;
        private int _direcao = 1;
        private State _estado = State.QuadroAndar;
        private float _tempoEstado;
        private float _proximoJato;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, Sprite novoSpritePoca, Sprite[] novoQuadros = null)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            spritePoca = novoSpritePoca;
            quadros = novoQuadros;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();
            _danificavel.Morreu += Respingar;
            _danificavel.Revivido += ReiniciarEstado;

            // Ao morrer ele incha antes de estourar: o estouro toca no BloaterBurst.
            _danificavel.SomMorte = Sfx.IncharInflado;
        }

        private void OnDestroy()
        {
            if (_danificavel != null)
            {
                _danificavel.Morreu -= Respingar;
                _danificavel.Revivido -= ReiniciarEstado;
            }
        }

        private void Start()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null) _jogador = controlador.transform;
        }

        private void ReiniciarEstado()
        {
            _estado = State.QuadroAndar;
            _tempoEstado = 0f;
            _proximoJato = 0f;
        }

        private void Respingar()
        {
            // Ele mesmo some na hora (o Damageable o desliga), entao quem incha
            // e estoura e uma copia: da tempo de o jogador ver e se afastar.
            Sprite[] inchar = null;
            if (quadros != null && quadros.Length >= QuadroInchar + QuadrosInchar)
            {
                inchar = new Sprite[QuadrosInchar];
                for (int i = 0; i < QuadrosInchar; i++) inchar[i] = quadros[QuadroInchar + i];
            }

            BloaterBurst.Iniciar(_sprite, transform.position, transform.position, larguraPoca, tempoPoca, spritePoca, inchar);
        }

        private void Update()
        {
            Vector3 posicao = transform.position;
            _tempoEstado += Time.deltaTime;

            if (Time.time - _danificavel.UltimoTempoGolpe < 0.2f)
            {
                _estado = State.QuadroAndar;
                posicao.x = Mathf.Clamp(posicao.x - _danificavel.UltimoLadoGolpe * 2f * Time.deltaTime, minX, maxX);
                transform.position = posicao;
                Mostrar(Ferido);
                return;
            }

            switch (_estado)
            {
                case State.QuadroAndar:
                    if (posicao.x >= maxX) _direcao = -1;
                    else if (posicao.x <= minX) _direcao = 1;

                    posicao.x = Mathf.Clamp(posicao.x + _direcao * velocidade * Time.deltaTime, minX, maxX);
                    Mostrar(QuadroAndar + (int)(Time.time * 6f) % QuadrosAndar);

                    if (_jogador != null && Time.time >= _proximoJato && EstaNaFrente(posicao))
                    {
                        _direcao = _jogador.position.x >= posicao.x ? 1 : -1;
                        Entrar(State.QuadroPreparo);
                    }

                    break;

                case State.QuadroPreparo:
                    Mostrar(QuadroPreparo + Mathf.Min(QuadrosPreparo - 1, (int)(_tempoEstado / tempoPreparo * QuadrosPreparo)));
                    if (_tempoEstado >= tempoPreparo) Entrar(State.QuadroJato);
                    break;

                case State.QuadroJato:
                    Mostrar(QuadroJato + Mathf.Min(QuadrosJato - 1, (int)(_tempoEstado / tempoJato * QuadrosJato)));
                    Espirrar();
                    if (_tempoEstado >= tempoJato) Entrar(State.Recuperar2);
                    break;

                case State.Recuperar2:
                    Mostrar(QuadroAndar);
                    if (_tempoEstado >= tempoRecuperar)
                    {
                        _proximoJato = Time.time + recarga;
                        Entrar(State.QuadroAndar);
                    }

                    break;
            }

            transform.position = posicao;
        }

        private bool EstaNaFrente(Vector3 posicao)
        {
            float dx = _jogador.position.x - posicao.x;
            return Mathf.Abs(dx) < alcanceJato && Mathf.Abs(_jogador.position.y - posicao.y) < 1.2f;
        }

        /// <summary>O borrifo: queima quem estiver na frente do bico.</summary>
        private void Espirrar()
        {
            Vector2 centro = (Vector2)transform.position + new Vector2(_direcao * 1.75f, 0.6f);
            foreach (Collider2D golpe in Physics2D.OverlapBoxAll(centro, new Vector2(1.6f, 0.7f), 0f))
            {
                var vida = golpe.GetComponentInParent<ScrapHealth>();
                if (vida != null) vida.LevarDano(1, transform.position);
            }
        }

        private void Entrar(State estado)
        {
            _estado = estado;
            _tempoEstado = 0f;

            if (estado == State.QuadroPreparo) Sfx.PreparoInflado.Tocar(transform.position);
            else if (estado == State.QuadroJato) Sfx.JatoInflado.Tocar(transform.position);
        }

        private void Mostrar(int quadro)
        {
            if (_sprite == null) return;

            // A arte olha pra direita.
            _sprite.flipX = _direcao < 0;
            if (quadros != null && quadro >= 0 && quadro < quadros.Length && quadros[quadro] != null) FrameAnimator.Exibir(_sprite, quadros[quadro]);
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
            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(1, transform.position);
        }
    }
}
