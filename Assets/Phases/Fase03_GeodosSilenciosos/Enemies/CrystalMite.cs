using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Crystal Mite: um tatuzinho de pedra com geodo nas costas. No escuro e
    /// no silencio ele fica enrolado e e so mais uma pedra. Quando um som o
    /// alcanca, ele se desenrola e corre pra cima do Scrap aos pulinhos por
    /// alguns segundos; depois se enrola de novo. Fraco — um golpe — mas
    /// aparece em grupo, e estoura alto.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class CrystalMite : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do MiteArt, no editor.
        public const int QuadroEnrolar = 0;
        public const int QuadrosEnrolar = 2;
        public const int QuadroDesenrolar = 2;
        public const int QuadrosDesenrolar = 3;
        public const int QuadroCorrida = 5;
        public const int QuadrosCorrida = 6;
        public const int QuadroMorte = 11;
        public const int QuadrosMorte = 5;

        private const float QpsDesenrolar = 12f;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [SerializeField] private float velocidade = 3.2f;

        [UnityEngine.Serialization.FormerlySerializedAs("awakeTime")] [Tooltip("Segundos acordado depois do ultimo som.")]
        [SerializeField] private float tempoAcordado = 3.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("deathNoiseRadius")] [Tooltip("Ate onde se ouve o estalo dele estilhacando, em tiles.")]
        [SerializeField] private float raioRuidoMorte = 10f;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Tooltip("Limites de onde ele anda, em x de mundo.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        private Transform _jogador;
        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private Vector3 _origem;
        private float _acordadoAte;
        private bool _estavaAcordado;
        private float _mudouEm = -10f;
        private float _relogio;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, Sprite[] novoQuadros = null)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            if (novoQuadros != null) quadros = novoQuadros;
        }

        /// <summary>O mesmo Configure, devolvendo ele: o gerador encadeia.</summary>
        public CrystalMite ConfigurarEDevolver(float novoMinX, float novoMaxX, Sprite[] novoQuadros = null)
        {
            Configurar(novoMinX, novoMaxX, novoQuadros);
            return this;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();
            _origem = transform.position;

            // Cada um respira num ponto do ciclo.
            _relogio = Mathf.Abs(transform.position.x * 0.37f);

            _danificavel.Revivido += Dormir;
            _danificavel.Morreu += Estilhacar;
            _danificavel.AnimacaoMorte = Estourar;
            _danificavel.SomMorte = Sfx.AcaroMorte;
        }

        private void OnDestroy()
        {
            if (_danificavel == null) return;
            _danificavel.Revivido -= Dormir;
            _danificavel.Morreu -= Estilhacar;
        }

        /// <summary>
        /// Morrendo ele estilhaca — e o estalo do cristal e alto: acorda os
        /// outros mites e chama os cacadores da mesma altura.
        /// </summary>
        private void Estilhacar()
        {
            NoiseSystem.Emitir(transform.position + Vector3.up * 0.3f, raioRuidoMorte, 0.7f);
        }

        private void Start()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null) _jogador = controlador.transform;
        }

        private void OnEnable()
        {
            NoiseSystem.Ouvido += AoOuvirRuido;
        }

        private void OnDisable()
        {
            NoiseSystem.Ouvido -= AoOuvirRuido;
        }

        /// <summary>Muda a casa dele (quem nasce em outro chao precisa pular rente a esse chao).</summary>
        public void MudarDeCasa(Vector3 posicao)
        {
            _origem = posicao;
            transform.position = posicao;
        }

        private void Dormir()
        {
            _acordadoAte = 0f;
            _estavaAcordado = false;
            _mudouEm = -10f;
            transform.position = _origem;
        }

        private void AoOuvirRuido(Vector2 pontoOrigem, float raioArea, float volumeRuido2)
        {
            if (volumeRuido2 < 0.3f) return;
            if (Vector2.Distance(pontoOrigem, transform.position) > raioArea) return;

            _acordadoAte = Time.time + tempoAcordado;
        }

        private void Update()
        {
            if (_danificavel.EstaMorto) return;

            bool acordado = Time.time < _acordadoAte;
            if (acordado != _estavaAcordado)
            {
                _estavaAcordado = acordado;
                _mudouEm = Time.time;
                if (acordado) Sfx.DespertarAcaro.Tocar(transform.position);
            }

            // Dormindo ele quase some; acordado, brilha.
            if (_sprite != null)
            {
                Color cor = _sprite.color;
                cor.a = Mathf.MoveTowards(cor.a, acordado ? 1f : 0.35f, 4f * Time.deltaTime);
                _sprite.color = cor;
            }

            Animar(acordado);

            if (!acordado || _jogador == null) return;
            if (Time.time - _danificavel.UltimoTempoGolpe < 0.2f) return;

            // Enquanto se desenrola, ainda nao corre.
            if (Time.time - _mudouEm < QuadrosDesenrolar / QpsDesenrolar) return;

            Vector3 posicao = transform.position;
            float direcao = Mathf.Sign(_jogador.position.x - posicao.x);

            posicao.x = Mathf.Clamp(posicao.x + direcao * velocidade * Time.deltaTime, minX, maxX);

            // Um pulinho a cada passo: ele corre saltitando.
            posicao.y = _origem.y + Mathf.Abs(Mathf.Sin(Time.time * 14f)) * 0.25f;
            transform.position = posicao;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = direcao < 0f;
        }

        /// <summary>Enrolado, desenrolando, correndo — ou se enrolando de volta.</summary>
        private void Animar(bool acordado)
        {
            float desde = Time.time - _mudouEm;
            int desenrolar = Mathf.Min(QuadrosDesenrolar - 1, (int)(desde * QpsDesenrolar));
            bool girando = desde < QuadrosDesenrolar / QpsDesenrolar;

            if (acordado)
            {
                Mostrar(girando ? QuadroDesenrolar + desenrolar : QuadroCorrida + (int)(Time.time * 12f) % QuadrosCorrida);
            }
            else
            {
                _relogio += Time.deltaTime * 2f;
                Mostrar(girando ? QuadroDesenrolar + QuadrosDesenrolar - 1 - desenrolar : QuadroEnrolar + (int)_relogio % QuadrosEnrolar);
            }
        }

        private void Mostrar(int quadro)
        {
            if (_sprite == null || quadros == null || quadro < 0 || quadro >= quadros.Length || quadros[quadro] == null) return;
            FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }

        /// <summary>A morte: racha, acende, estoura em cacos e po.</summary>
        private IEnumerator Estourar()
        {
            if (_sprite != null) _sprite.color = Color.white;

            for (int i = 0; i < QuadrosMorte; i++)
            {
                Mostrar(QuadroMorte + i);
                yield return new WaitForSeconds(1f / 12f);
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
            // Dormindo ele e so pedra: nao machuca.
            if (_danificavel.EstaMorto || Time.time >= _acordadoAte) return;

            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(1, transform.position);
        }
    }
}
