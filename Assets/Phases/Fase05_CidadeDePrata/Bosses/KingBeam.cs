using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O raio do cetro do Rei de Prata: uma linha de luz que sai de um ponto e
    /// vai reto ate bater no terreno (as sacadas fazem sombra). Primeiro so o
    /// fio fino, que mira; depois o raio grosso, que queima quem cruzar.
    ///
    /// O dash atravessa (o Scrap nao apanha durante o dash).
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class KingBeam : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("maxLength")] [SerializeField] private float comprimentoMax = 70f;
        [UnityEngine.Serialization.FormerlySerializedAs("aimColor")] [SerializeField] private Color corMira = new Color(1f, 0.97f, 0.80f, 0.35f);
        [UnityEngine.Serialization.FormerlySerializedAs("fireColor")] [SerializeField] private Color corDisparo = new Color(1f, 0.96f, 0.70f, 1f);

        private LineRenderer _linha;
        private ScrapHealth _vida;
        private Transform _jogador;
        private bool _disparando;
        private int _mascara;

        // O fio de mira assobia e o raio aceso zumbe; a cena de cada golpe so diz Aim, Fire e Hide.
        private SfxLoop _somMira;
        private SfxLoop _somDisparo;
        private bool _estavaDisparando;
        private float _mostradoEm = -100f;

        private void Awake()
        {
            _linha = GetComponent<LineRenderer>();
            _linha.useWorldSpace = true;
            _linha.positionCount = 0;
            _mascara = LayerMask.GetMask("Ground");

            _somMira = SfxLoop.Prender(gameObject, Sfx.MiraRaioRei);
            _somDisparo = SfxLoop.Prender(gameObject, Sfx.RepeticaoRaioRei);
            if (_somMira != null) _somMira.Esmaecimento = 8f;
            if (_somDisparo != null) _somDisparo.Esmaecimento = 8f;
        }

        private void Update()
        {
            // Quem mira chama Aim/Fire todo quadro: parou de chamar, o raio apagou.
            bool mostrado = Time.time - _mostradoEm < 0.1f;
            if (_somMira != null) _somMira.Nivel = mostrado && !_disparando ? 1f : 0f;
            if (_somDisparo != null) _somDisparo.Nivel = mostrado && _disparando ? 1f : 0f;
        }

        private void Start()
        {
            var controlador = ScrapRef.Controlador;
            if (controlador != null)
            {
                _jogador = controlador.transform;
                _vida = controlador.GetComponent<ScrapHealth>();
            }
        }

        /// <summary>Mostra o fio fino que mira.</summary>
        public void Mirar(Vector2 pontoOrigem, float angulo) => Mostrar(pontoOrigem, angulo, false);

        /// <summary>O raio grosso, queimando.</summary>
        public void Disparar(Vector2 pontoOrigem, float angulo) => Mostrar(pontoOrigem, angulo, true);

        public void Esconder()
        {
            _disparando = false;
            _estavaDisparando = false;
            _mostradoEm = -100f;
            if (_linha != null) _linha.positionCount = 0;
        }

        private void Mostrar(Vector2 pontoOrigem, float angulo, bool disparando)
        {
            _disparando = disparando;
            _mostradoEm = Time.time;

            // O som vem da ponta do cetro (a linha e em coordenadas de mundo: mover o objeto nao a mexe).
            transform.position = pontoOrigem;
            if (disparando && !_estavaDisparando) Sfx.DisparoRaioRei.Tocar(pontoOrigem);
            _estavaDisparando = disparando;

            Vector2 direcao = new Vector2(Mathf.Cos(angulo * Mathf.Deg2Rad), Mathf.Sin(angulo * Mathf.Deg2Rad));
            Vector2 fim = pontoOrigem + direcao * comprimentoMax;

            RaycastHit2D[] golpes = Physics2D.RaycastAll(pontoOrigem, direcao, comprimentoMax, _mascara);
            float melhor = comprimentoMax;
            foreach (RaycastHit2D golpe in golpes)
            {
                if (golpe.collider == null || golpe.collider.isTrigger) continue;
                if (SilverKing.EPassavel(golpe.point)) continue;
                if (golpe.distance < melhor) melhor = golpe.distance;
            }

            fim = pontoOrigem + direcao * melhor;

            float largura = disparando ? 0.55f + Mathf.Sin(Time.time * 30f) * 0.06f : 0.06f;
            _linha.positionCount = 2;
            _linha.SetPosition(0, pontoOrigem);
            _linha.SetPosition(1, fim);
            _linha.startWidth = largura;
            _linha.endWidth = largura * 0.8f;
            _linha.startColor = disparando ? corDisparo : corMira;
            _linha.endColor = disparando ? corDisparo : corMira;

            if (!disparando || _jogador == null || _vida == null) return;

            // Queima quem estiver perto do segmento (o meio do corpo do Scrap).
            Vector2 corpo = (Vector2)_jogador.position + Vector2.up * 0.6f;
            Vector2 segmento = fim - pontoOrigem;
            float t = Mathf.Clamp01(Vector2.Dot(corpo - pontoOrigem, segmento) / Mathf.Max(0.01f, segmento.sqrMagnitude));
            float distancia = Vector2.Distance(corpo, pontoOrigem + segmento * t);
            if (distancia < 0.6f) _vida.LevarDano(1, pontoOrigem);
        }
    }
}
