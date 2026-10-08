using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Desloca uma camada de fundo conforme a camera anda, criando profundidade.
    ///
    /// A repeticao horizontal nao e feita aqui: o gerador configura o
    /// SpriteRenderer da camada em modo Tiled com uma largura bem maior que o
    /// mapa, entao a arte ja se repete sozinha e nunca acaba. Aqui so cuidamos
    /// do deslocamento.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxLayer : MonoBehaviour
    {
        [Tooltip("0 = infinitamente longe (gruda na camera e nunca parece se mover). " +
                 "1 = no plano do jogo (parada no mundo, passa na velocidade total).")]
        [SerializeField, Range(0f, 1f)] private float parallaxFactor = 0.5f;

        [Tooltip("Se a camada tambem acompanha o movimento vertical da camera.")]
        [SerializeField] private bool followVertically = true;

        [Tooltip("Fator vertical. Costuma ficar melhor mais fraco que o horizontal.")]
        [SerializeField, Range(0f, 1f)] private float verticalFactor = 0.25f;

        [Tooltip("Deixe vazio pra usar a Main Camera.")]
        [SerializeField] private Transform target;

        private Vector3 _layerStart;
        private Vector3 _targetStart;
        private bool _initialized;

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (target == null && Camera.main != null)
            {
                target = Camera.main.transform;
            }

            _layerStart = transform.position;
            if (target != null) _targetStart = target.position;

            _initialized = target != null;
        }

        private void LateUpdate()
        {
            if (!_initialized)
            {
                Initialize();
                if (!_initialized) return;
            }

            Vector3 travel = target.position - _targetStart;

            // Um fator menor significa mais longe, entao a camada acompanha mais
            // a camera e se desloca menos em relacao a ela.
            float x = _layerStart.x + travel.x * (1f - parallaxFactor);
            float y = followVertically
                ? _layerStart.y + travel.y * (1f - verticalFactor)
                : _layerStart.y;

            transform.position = new Vector3(x, y, _layerStart.z);
        }
    }
}
