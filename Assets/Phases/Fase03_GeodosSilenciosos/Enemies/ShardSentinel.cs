using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Shard Sentinel: um geodo vivo, plantado num pedestal de cristais. Nao
    /// sai do lugar. Quando ouve barulho por perto, a tampa de pedra abre, o
    /// miolo de ametista acende e ele lanca um leque de estilhacos na direcao
    /// do som; depois fecha. Quebra com alguns golpes.
    ///
    /// Como atira no som, e nao no Scrap, da pra faze-lo gastar a rajada num
    /// cristal ressonante e passar enquanto ele recarrega.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class ShardSentinel : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do SentinelArt, no editor.
        public const int QuadroOcioso = 0;
        public const int QuadrosOcioso = 4;
        public const int QuadroAberto = 4;
        public const int QuadrosAberto = 3;
        public const int QuadroDisparo = 7;
        public const int QuadrosDisparo = 2;
        public const int QuadroFechar = 9;
        public const int QuadrosFechar = 2;
        public const int Ferido = 11;
        public const int QuadroMorte = 12;
        public const int QuadrosMorte = 6;

        [UnityEngine.Serialization.FormerlySerializedAs("hearingRange")] [Tooltip("Ate onde ela escuta, em tiles.")]
        [SerializeField] private float alcanceAudicao = 9f;

        [UnityEngine.Serialization.FormerlySerializedAs("windup")] [Tooltip("Segundos entre ouvir e atirar: o tempo de sair da frente.")]
        [SerializeField] private float preparo = 0.45f;

        [UnityEngine.Serialization.FormerlySerializedAs("cooldown")] [Tooltip("Segundos ate poder atirar de novo.")]
        [SerializeField] private float recarga = 2.2f;

        [UnityEngine.Serialization.FormerlySerializedAs("shardSpeed")] [SerializeField] private float velocidadeFragmento = 11f;
        [UnityEngine.Serialization.FormerlySerializedAs("shardSprite")] [SerializeField] private Sprite spriteFragmento;

        [UnityEngine.Serialization.FormerlySerializedAs("coreHeight")] [Tooltip("A altura do nucleo, de onde saem os estilhacos, em tiles.")]
        [SerializeField] private float alturaNucleo = 1.06f;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private float _prontoEm;
        private bool _disparando;
        private float _relogio;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Sprite novoSpriteFragmento, Sprite[] novoQuadros = null, float novoAlturaNucleo = 1.06f)
        {
            spriteFragmento = novoSpriteFragmento;
            if (novoQuadros != null) quadros = novoQuadros;
            alturaNucleo = novoAlturaNucleo;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();
            _relogio = Mathf.Abs(transform.position.x * 0.41f);

            _danificavel.Revivido += ReiniciarEstado;
            _danificavel.AnimacaoMorte = Rachar;
            _danificavel.SomMorte = Sfx.SentinelaMorte;
        }

        private void OnDestroy()
        {
            if (_danificavel != null) _danificavel.Revivido -= ReiniciarEstado;
        }

        private void OnEnable()
        {
            NoiseSystem.Ouvido += AoOuvirRuido;
            _disparando = false;
        }

        private void OnDisable()
        {
            NoiseSystem.Ouvido -= AoOuvirRuido;
        }

        private void ReiniciarEstado()
        {
            StopAllCoroutines();
            _disparando = false;
            _prontoEm = 0f;
        }

        private Vector2 Bocal => (Vector2)transform.position + Vector2.up * alturaNucleo;

        private void AoOuvirRuido(Vector2 pontoOrigem, float raioArea, float volumeRuido2)
        {
            if (_danificavel.EstaMorto || _disparando || Time.time < _prontoEm || volumeRuido2 < 0.3f) return;

            if (Vector2.Distance(pontoOrigem, Bocal) > Mathf.Min(alcanceAudicao, raioArea + 1f)) return;

            StartCoroutine(Atirar(pontoOrigem + Vector2.up * 0.6f));
        }

        private void Update()
        {
            if (_danificavel.EstaMorto || _disparando) return;

            // Apanhando: a tampa estremece.
            if (Time.time - _danificavel.UltimoTempoGolpe < 0.15f)
            {
                Mostrar(Ferido);
                return;
            }

            _relogio += Time.deltaTime * 4f;
            Mostrar(QuadroOcioso + (int)_relogio % QuadrosOcioso);
        }

        private IEnumerator Atirar(Vector2 alvo)
        {
            _disparando = true;
            Sfx.AbertoSentinela.Tocar(transform.position);

            // O aviso: a tampa abre e o miolo acende.
            for (int i = 0; i < QuadrosAberto; i++)
            {
                Mostrar(QuadroAberto + i);
                yield return new WaitForSeconds(preparo / QuadrosAberto);
                if (_danificavel.EstaMorto) yield break;
            }

            Vector2 bocal = Bocal;
            Vector2 mira = (alvo - bocal).normalized;
            if (mira.sqrMagnitude < 0.01f) mira = Vector2.right;

            // Tres estilhacos em leque.
            Mostrar(QuadroDisparo);
            Sfx.DisparoSentinela.Tocar(bocal);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 direcao = Quaternion.Euler(0f, 0f, i * 14f) * mira;
                ShardShot.Disparar(bocal, direcao * velocidadeFragmento, 14f, spriteFragmento);
            }

            // O proprio tiro faz barulho: acende a sala em volta dele.
            NoiseSystem.Emitir(bocal, 6f, 0.2f);
            yield return new WaitForSeconds(0.08f);
            if (_danificavel.EstaMorto) yield break;
            Mostrar(QuadroDisparo + 1);
            yield return new WaitForSeconds(0.12f);

            for (int i = 0; i < QuadrosFechar; i++)
            {
                if (_danificavel.EstaMorto) yield break;
                Mostrar(QuadroFechar + i);
                yield return new WaitForSeconds(0.1f);
            }

            _prontoEm = Time.time + recarga;
            _disparando = false;
        }

        private void Mostrar(int quadro)
        {
            if (_sprite == null || quadros == null || quadro < 0 || quadro >= quadros.Length || quadros[quadro] == null) return;
            FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }

        /// <summary>A morte: as metades racham e caem pros lados, o miolo estoura em po.</summary>
        private IEnumerator Rachar()
        {
            _disparando = true;
            if (_sprite != null) _sprite.color = Color.white;

            for (int i = 0; i < QuadrosMorte; i++)
            {
                Mostrar(QuadroMorte + i);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
