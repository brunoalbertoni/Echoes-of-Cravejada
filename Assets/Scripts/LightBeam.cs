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
        [Tooltip("Pra onde o raio sai: (1,0), (-1,0), (0,1) ou (0,-1).")]
        [SerializeField] private Vector2 direction = Vector2.right;

        [SerializeField] private float maxLength = 80f;
        [SerializeField] private int maxBounces = 8;

        [Tooltip("Segundos entre uma queimadura e outra no mesmo inimigo.")]
        [SerializeField] private float burnInterval = 0.6f;

        private LineRenderer _line;
        private readonly List<Vector3> _points = new List<Vector3>();
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[32];
        private readonly Dictionary<IDamageable, float> _burned = new Dictionary<IDamageable, float>();

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Vector2 newDirection)
        {
            direction = newDirection;
        }

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
        }

        private void Update()
        {
            _points.Clear();

            Vector2 origin = transform.position;
            Vector2 heading = direction.normalized;

            _points.Add(origin);

            for (int bounce = 0; bounce <= maxBounces; bounce++)
            {
                SilverMirror mirror = Trace(origin, heading, out Vector2 end);
                _points.Add(end);

                if (mirror == null) break;

                // Dobra no centro do espelho, pra o raio seguir alinhado.
                origin = mirror.Center;
                heading = mirror.Reflect(heading);
                _points[_points.Count - 1] = origin;
            }

            _line.positionCount = _points.Count;
            _line.SetPositions(_points.ToArray());

            // Um tremor de leve na largura: luz viva, nao um risco.
            float width = 0.16f + Mathf.Sin(Time.time * 18f) * 0.03f;
            _line.startWidth = width;
            _line.endWidth = width;
        }

        /// <summary>
        /// Segue o raio de um ponto numa direcao. Devolve o espelho em que ele
        /// bateu (ou null), e onde o trecho termina.
        /// </summary>
        private SilverMirror Trace(Vector2 origin, Vector2 heading, out Vector2 end)
        {
            // Um passo adiante, pra nao bater de novo no espelho de onde saiu.
            Vector2 start = origin + heading * 0.6f;
            int count = Physics2D.RaycastNonAlloc(start, heading, _hits, maxLength);

            System.Array.Sort(_hits, 0, count, DistanceOrder.Instance);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _hits[i].collider;
                if (hit == null) continue;

                var mirror = hit.GetComponentInParent<SilverMirror>();
                if (mirror != null)
                {
                    // So a face reflete; a area de alcance e atravessada.
                    if (hit != mirror.Face) continue;

                    end = _hits[i].point;
                    return mirror;
                }

                var crystal = hit.GetComponent<CrystalLock>();
                if (crystal != null)
                {
                    crystal.Illuminate();
                    end = _hits[i].point;
                    return null;
                }

                var health = hit.GetComponentInParent<ScrapHealth>();
                if (health != null)
                {
                    health.TakeDamage(1, _hits[i].point);
                    continue;
                }

                var damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    Burn(damageable, _hits[i].point);
                    continue;
                }

                // Gatilho qualquer nao segura luz; parede, sim.
                if (hit.isTrigger) continue;

                end = _hits[i].point;
                return null;
            }

            end = start + heading * maxLength;
            return null;
        }

        private void Burn(IDamageable target, Vector2 point)
        {
            if (_burned.TryGetValue(target, out float last) && Time.time - last < burnInterval) return;

            _burned[target] = Time.time;
            target.Damage(1, point);
        }

        private sealed class DistanceOrder : IComparer<RaycastHit2D>
        {
            public static readonly DistanceOrder Instance = new DistanceOrder();

            public int Compare(RaycastHit2D a, RaycastHit2D b) => a.distance.CompareTo(b.distance);
        }
    }
}
