using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um raio de luz alquimica saindo de um emissor. Vai reto ate bater em
    /// parede; num espelho de prata, dobra a esquina; numa fechadura de
    /// cristal, a acende.
    ///
    /// O raio queima: o Scrap perde vida ao cruza-lo, e inimigos tambem.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LightBeam : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("direction")] [Tooltip("Pra onde o raio sai: (1,0), (-1,0), (0,1) ou (0,-1).")]
        [SerializeField] private Vector2 direcao = Vector2.right;

        [UnityEngine.Serialization.FormerlySerializedAs("maxLength")] [SerializeField] private float comprimentoMax = 80f;
        [UnityEngine.Serialization.FormerlySerializedAs("maxBounces")] [SerializeField] private int quiquesMax = 8;

        [UnityEngine.Serialization.FormerlySerializedAs("burnInterval")] [Tooltip("Segundos entre uma queimadura e outra no mesmo inimigo.")]
        [SerializeField] private float intervaloQueima = 0.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("onTime")] [Header("Pulso (0 = sempre aceso)")]
        [SerializeField] private float ligadoTempo;
        [UnityEngine.Serialization.FormerlySerializedAs("offTime")] [SerializeField] private float desligadoTempo;
        [UnityEngine.Serialization.FormerlySerializedAs("offset")] [SerializeField] private float deslocamento;

        [UnityEngine.Serialization.FormerlySerializedAs("burnsFoes")] [Tooltip("Se queima tambem os inimigos (os raios-perigo da Optica so queimam o Scrap).")]
        [SerializeField] private bool inimigosQueima = true;

        [UnityEngine.Serialization.FormerlySerializedAs("burnsScrap")] [Tooltip("Se queima o Scrap. A luz-guia dos enigmas de espelho e fria: so aponta o caminho.")]
        [SerializeField] private bool sucataQueima = true;

        private LineRenderer _linha;
        private readonly List<Vector3> _pontos = new List<Vector3>();
        private readonly RaycastHit2D[] _golpes = new RaycastHit2D[32];
        private readonly Dictionary<IDamageable, float> _queimado = new Dictionary<IDamageable, float>();

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Vector2 novoDirecao)
        {
            direcao = novoDirecao;
        }

        /// <summary>Usado pelos geradores de fase: o raio liga e desliga em ritmo.</summary>
        public void ConfigurarPulso(float novoLigadoTempo, float novoDesligadoTempo, float novoDeslocamento, bool novoInimigosQueima)
        {
            ligadoTempo = novoLigadoTempo;
            desligadoTempo = novoDesligadoTempo;
            deslocamento = novoDeslocamento;
            inimigosQueima = novoInimigosQueima;
        }

        /// <summary>
        /// Usado pelos geradores de fase: a luz-guia dos enigmas. Sempre acesa,
        /// fria (nao queima ninguem): so serve pra ser dobrada nos espelhos ate
        /// a fechadura de cristal.
        /// </summary>
        public void ConfigurarGuia()
        {
            ligadoTempo = 0f;
            desligadoTempo = 0f;
            deslocamento = 0f;
            inimigosQueima = false;
            sucataQueima = false;
        }

        /// <summary>Se o raio esta aceso agora.</summary>
        public bool EstaLigado
        {
            get
            {
                if (ligadoTempo <= 0f || desligadoTempo <= 0f) return true;
                float t = Mathf.Repeat(Time.time + deslocamento, ligadoTempo + desligadoTempo);
                return t < ligadoTempo;
            }
        }

        private void Awake()
        {
            _linha = GetComponent<LineRenderer>();
            _linha.useWorldSpace = true;
        }

        private void Update()
        {
            if (!EstaLigado)
            {
                _linha.positionCount = 0;
                return;
            }

            _pontos.Clear();

            Vector2 pontoOrigem = transform.position;
            Vector2 rumo = direcao.normalized;

            _pontos.Add(pontoOrigem);

            for (int quique = 0; quique <= quiquesMax; quique++)
            {
                SilverMirror espelho = Tracar(pontoOrigem, rumo, out Vector2 fim);
                _pontos.Add(fim);

                if (espelho == null) break;

                // Dobra no centro do espelho, pra o raio seguir alinhado.
                pontoOrigem = espelho.Centro;
                rumo = espelho.Refletir(rumo);
                _pontos[_pontos.Count - 1] = pontoOrigem;
            }

            _linha.positionCount = _pontos.Count;
            _linha.SetPositions(_pontos.ToArray());

            // Um tremor de leve na largura: luz viva, nao um risco.
            float largura = 0.16f + Mathf.Sin(Time.time * 18f) * 0.03f;
            _linha.startWidth = largura;
            _linha.endWidth = largura;
        }

        /// <summary>
        /// Segue o raio de um ponto numa direcao. Devolve o espelho em que ele
        /// bateu (ou null), e onde o trecho termina.
        /// </summary>
        private SilverMirror Tracar(Vector2 pontoOrigem, Vector2 rumo, out Vector2 fim)
        {
            // Um passo adiante, pra nao bater de novo no espelho de onde saiu.
            Vector2 inicio = pontoOrigem + rumo * 0.6f;
            int quantidade = Physics2D.RaycastNonAlloc(inicio, rumo, _golpes, comprimentoMax);

            System.Array.Sort(_golpes, 0, quantidade, DistanceOrder.Instancia);

            for (int i = 0; i < quantidade; i++)
            {
                Collider2D golpe = _golpes[i].collider;
                if (golpe == null) continue;

                var espelho = golpe.GetComponentInParent<SilverMirror>();
                if (espelho != null)
                {
                    // So a face reflete; a area de alcance e atravessada.
                    if (golpe != espelho.Rosto) continue;

                    fim = _golpes[i].point;
                    return espelho;
                }

                var cristal = golpe.GetComponent<CrystalLock>();
                if (cristal != null)
                {
                    cristal.Iluminar();
                    fim = _golpes[i].point;
                    return null;
                }

                var vida = golpe.GetComponentInParent<ScrapHealth>();
                if (vida != null)
                {
                    if (sucataQueima) vida.LevarDano(1, _golpes[i].point);
                    continue;
                }

                var danificavel = golpe.GetComponentInParent<IDamageable>();
                if (danificavel != null && !inimigosQueima) continue;
                if (danificavel != null)
                {
                    Queimar(danificavel, _golpes[i].point);
                    continue;
                }

                // Gatilho qualquer nao segura luz; parede, sim.
                if (golpe.isTrigger) continue;

                fim = _golpes[i].point;
                return null;
            }

            fim = inicio + rumo * comprimentoMax;
            return null;
        }

        private void Queimar(IDamageable alvo, Vector2 ponto)
        {
            if (_queimado.TryGetValue(alvo, out float ultimo) && Time.time - ultimo < intervaloQueima) return;

            _queimado[alvo] = Time.time;
            alvo.ReceberDano(1, ponto);
        }

        private sealed class DistanceOrder : IComparer<RaycastHit2D>
        {
            public static readonly DistanceOrder Instancia = new DistanceOrder();

            public int Compare(RaycastHit2D a, RaycastHit2D b) => a.distance.CompareTo(b.distance);
        }
    }
}
