using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Flask Thrower: um assistente de laboratorio de cobre, corrompido. Nao
    /// chega perto: fica onde esta e arremessa frascos em arco, que estouram
    /// no chao e deixam uma poca de Oxide por alguns segundos. Fragil — o
    /// problema e chegar nele.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class FlaskThrower : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do ThrowerArt, no editor.
        public const int QuadroOcioso = 0;
        public const int QuadrosOcioso = 4;
        public const int QuadroPreparo = 4;
        public const int QuadrosPreparo = 3;
        public const int QuadroArremesso = 7;
        public const int QuadrosArremesso = 2;
        public const int Ferido = 9;
        public const int QuadroMorte = 10;
        public const int QuadrosMorte = 6;

        [UnityEngine.Serialization.FormerlySerializedAs("range")] [Tooltip("Ate onde ele enxerga o Scrap, em tiles.")]
        [SerializeField] private float alcance = 11f;

        [UnityEngine.Serialization.FormerlySerializedAs("cooldown")] [SerializeField] private float recarga = 2.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("windup")] [Tooltip("Segundos de aviso: ele puxa o frasco e joga o braco pra tras.")]
        [SerializeField] private float preparo = 0.45f;

        [UnityEngine.Serialization.FormerlySerializedAs("handHeight")] [Tooltip("De onde o frasco sai, em tiles acima dos pes.")]
        [SerializeField] private float alturaMao = 1.3f;

        [UnityEngine.Serialization.FormerlySerializedAs("flaskSprite")] [SerializeField] private Sprite spriteFrasco;
        [UnityEngine.Serialization.FormerlySerializedAs("puddleSprite")] [SerializeField] private Sprite spritePoca;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private Transform _jogador;
        private float _prontoEm;
        private bool _arremessando;
        private float _relogio;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Sprite novoSpriteFrasco, Sprite novoSpritePoca, Sprite[] novoQuadros = null, float novoAlturaMao = 1.3f)
        {
            spriteFrasco = novoSpriteFrasco;
            spritePoca = novoSpritePoca;
            if (novoQuadros != null) quadros = novoQuadros;
            alturaMao = novoAlturaMao;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();
            _relogio = Mathf.Abs(transform.position.x * 0.23f);

            _danificavel.AnimacaoMorte = Desabar;
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

        private void OnEnable()
        {
            _arremessando = false;
            _prontoEm = Time.time + 1f;
        }

        private void ReiniciarEstado()
        {
            StopAllCoroutines();
            _arremessando = false;
            _prontoEm = Time.time + 1f;
        }

        private void Update()
        {
            if (_danificavel.EstaMorto || _arremessando) return;

            if (Time.time - _danificavel.UltimoTempoGolpe < 0.2f)
            {
                Mostrar(Ferido);
                return;
            }

            // Parado, mexendo na bolsa.
            _relogio += Time.deltaTime * 4f;
            Mostrar(QuadroOcioso + (int)_relogio % QuadrosOcioso);

            if (_jogador == null || Time.time < _prontoEm) return;

            Vector2 deslocamento = _jogador.position - transform.position;
            if (Mathf.Abs(deslocamento.x) > alcance || Mathf.Abs(deslocamento.y) > 6f) return;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = deslocamento.x < 0f;

            StartCoroutine(ArremessarFrasco());
        }

        private IEnumerator ArremessarFrasco()
        {
            _arremessando = true;
            Sfx.PreparoArremessador.Tocar(transform.position);

            // O aviso: puxa o frasco da bandoleira e joga o braco pra tras.
            for (int i = 0; i < QuadrosPreparo; i++)
            {
                Mostrar(QuadroPreparo + i);
                yield return new WaitForSeconds(preparo / QuadrosPreparo);
                if (_danificavel.EstaMorto) yield break;
            }

            Mostrar(QuadroArremesso);
            if (_jogador != null)
            {
                Vector2 mao = (Vector2)transform.position + Vector2.up * alturaMao;
                Flask.Arremessar(mao, _jogador.position, spriteFrasco, spritePoca);
                Sfx.ArremessoArremessador.Tocar(mao);
            }

            yield return new WaitForSeconds(0.1f);
            if (_danificavel.EstaMorto) yield break;
            Mostrar(QuadroArremesso + 1);
            yield return new WaitForSeconds(0.25f);

            _prontoEm = Time.time + recarga;
            _arremessando = false;
        }

        private void Mostrar(int quadro)
        {
            if (_sprite == null || quadros == null || quadro < 0 || quadro >= quadros.Length || quadros[quadro] == null) return;
            FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }

        /// <summary>A morte: cambaleia, cai de joelhos, os frascos estouram e ele tomba.</summary>
        private IEnumerator Desabar()
        {
            _arremessando = true;
            if (_sprite != null) _sprite.color = Color.white;

            // Os frascos da bandoleira estouram quando ele cai de joelhos.
            Sfx.QuebraFrasco.Tocar(transform.position, volume: 0.7f, atraso: 0.3f);

            float[] segurar = { 0.12f, 0.15f, 0.18f, 0.18f, 0.2f, 0.25f };
            for (int i = 0; i < QuadrosMorte; i++)
            {
                Mostrar(QuadroMorte + i);
                yield return new WaitForSeconds(segurar[i]);
            }
        }
    }
}
