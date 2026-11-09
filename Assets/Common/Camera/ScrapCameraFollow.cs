using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Camera que segue o Scrap com amortecimento e trava nas bordas do mapa,
    /// pra nunca mostrar o vazio fora do tilemap.
    ///
    /// Nao confunda com o CameraFollow2D que veio junto com o pack 2D Dungeon
    /// Tilemap: aquele e top-down e nao tem limites de mundo.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ScrapCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [Tooltip("Deslocamento em relacao ao alvo. Um Y positivo mostra mais do que vem pela frente em cima.")]
        [SerializeField] private Vector2 offset = new Vector2(0f, 1.5f);

        [Tooltip("Quanto menor, mais colada a camera fica. Zero remove o amortecimento.")]
        [SerializeField] private float smoothTime = 0.18f;

        [Header("Limites do mundo")]
        [SerializeField] private bool clampToBounds = true;

        [Tooltip("Retangulo do mapa em unidades de mundo. O gerador preenche com o tamanho da Phase 1.")]
        [SerializeField] private Rect worldBounds = new Rect(0f, 0f, 80f, 24f);

        private Camera _camera;
        private Vector3 _velocity;
        private float _baseSize;

        // A camera dirigida: numa cena, quem dirige diz pra onde ela olha e o
        // quanto aproxima (1 = o normal; menor fecha, maior abre). Estatico
        // pra a cena nao precisar achar a camera.
        private static bool _directed;
        private static Vector2 _focus;
        private static float _zoom = 1f;
        private static float _directSmooth = 0.4f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDirection()
        {
            _directed = false;
            _zoom = 1f;
        }

        /// <summary>
        /// Toma a camera numa cena: ela desliza ate focus (em unidades de mundo)
        /// e aproxima ou afasta ate zoom vezes o tamanho normal. smooth e o
        /// tempo do deslize: maior, mais lento e mais cinematografico.
        /// </summary>
        public static void Direct(Vector2 focus, float zoom, float smooth = 0.4f)
        {
            _directed = true;
            _focus = focus;
            _zoom = Mathf.Max(0.3f, zoom);
            _directSmooth = Mathf.Max(0.01f, smooth);
        }

        /// <summary>Devolve a camera ao Scrap, voltando ao tamanho normal devagar.</summary>
        public static void Release()
        {
            _directed = false;
            _zoom = 1f;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _baseSize = _camera.orthographicSize;
        }

        private void OnEnable()
        {
            if (_camera == null) _camera = GetComponent<Camera>();

            // Comeca ja enquadrado, senao a camera faz um deslize feio no primeiro frame.
            if (target != null) transform.position = Resolve(DesiredPosition());
        }

        // O tremor: forca e ate quando. Estatico pra quem treme a tela nao
        // precisar achar a camera.
        private static float _shakeStrength;
        private static float _shakeUntil;
        private static float _shakeLength = 1f;
        private Vector3 _shakeOffset;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetShake()
        {
            _shakeStrength = 0f;
            _shakeUntil = 0f;
        }

        /// <summary>
        /// Treme a tela por um tempo, em tiles de deslocamento, diminuindo ate
        /// parar. Um tremor mais fraco nao corta um mais forte em andamento.
        /// </summary>
        public static void Shake(float strength, float duration)
        {
            float remaining = Mathf.Max(0f, _shakeUntil - Time.time);
            float current = _shakeLength > 0f ? _shakeStrength * remaining / _shakeLength : 0f;
            if (strength < current) return;

            _shakeStrength = strength;
            _shakeLength = Mathf.Max(0.01f, duration);
            _shakeUntil = Time.time + duration;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // O tremor do quadro anterior sai antes de seguir o alvo, senao ele
            // entra no amortecimento e a camera escorrega.
            transform.position -= _shakeOffset;
            _shakeOffset = Vector3.zero;

            // O tamanho vai devagar ate o pedido (ou volta ao normal).
            if (_baseSize <= 0f) _baseSize = _camera.orthographicSize;
            float size = _baseSize * (_directed ? _zoom : 1f);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, size, 1f - Mathf.Exp(-Time.deltaTime * 2.6f));

            Vector3 goal = _directed ? new Vector3(_focus.x, _focus.y, transform.position.z) : DesiredPosition();
            Vector3 desired = Vector3.SmoothDamp(
                transform.position, goal, ref _velocity, _directed ? _directSmooth : smoothTime);

            transform.position = Resolve(desired);

            float left = _shakeUntil - Time.time;
            if (left > 0f)
            {
                float strength = _shakeStrength * left / _shakeLength;
                _shakeOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * strength;
                transform.position += _shakeOffset;
            }
        }

        private Vector3 DesiredPosition()
        {
            return new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                transform.position.z);
        }

        /// <summary>Aplica os limites do mundo, mantendo o Z da camera.</summary>
        private Vector3 Resolve(Vector3 position)
        {
            position.z = transform.position.z;
            if (!clampToBounds || _camera == null) return position;

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            // Se o mapa e menor que a tela naquele eixo, centraliza em vez de travar.
            position.x = worldBounds.width < halfWidth * 2f
                ? worldBounds.center.x
                : Mathf.Clamp(position.x, worldBounds.xMin + halfWidth, worldBounds.xMax - halfWidth);

            position.y = worldBounds.height < halfHeight * 2f
                ? worldBounds.center.y
                : Mathf.Clamp(position.y, worldBounds.yMin + halfHeight, worldBounds.yMax - halfHeight);

            return position;
        }

        /// <summary>Usado pelo gerador da Phase 1 pra configurar tudo de uma vez.</summary>
        public void Configure(Transform newTarget, Rect bounds)
        {
            target = newTarget;
            worldBounds = bounds;
            clampToBounds = true;
        }

        /// <summary>
        /// Pula direto pro enquadramento do alvo, sem amortecer. Pra quando o
        /// alvo e teleportado — ao chegar de outra cena, por exemplo.
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null) return;
            if (_camera == null) _camera = GetComponent<Camera>();

            _velocity = Vector3.zero;
            transform.position = Resolve(DesiredPosition());
        }

        private void OnDrawGizmosSelected()
        {
            if (!clampToBounds) return;

            Gizmos.color = new Color(0.3f, 1f, 0.6f, 0.75f);
            Gizmos.DrawWireCube(
                new Vector3(worldBounds.center.x, worldBounds.center.y, 0f),
                new Vector3(worldBounds.width, worldBounds.height, 0f));
        }
    }
}
