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
        [UnityEngine.Serialization.FormerlySerializedAs("parallaxFactor")] [Tooltip("0 = infinitamente longe (gruda na camera e nunca parece se mover). " +
                 "1 = no plano do jogo (parada no mundo, passa na velocidade total).")]
        [SerializeField, Range(0f, 1f)] private float fatorParalaxe = 0.5f;

        [UnityEngine.Serialization.FormerlySerializedAs("followVertically")] [Tooltip("Se a camada tambem acompanha o movimento vertical da camera.")]
        [SerializeField] private bool verticalmenteSeguir = true;

        [UnityEngine.Serialization.FormerlySerializedAs("verticalFactor")] [Tooltip("Fator vertical. Costuma ficar melhor mais fraco que o horizontal.")]
        [SerializeField, Range(0f, 1f)] private float fatorVertical = 0.25f;

        [UnityEngine.Serialization.FormerlySerializedAs("target")] [Tooltip("Deixe vazio pra usar a Main Camera.")]
        [SerializeField] private Transform alvo;

        private Vector3 _inicioCamada;
        private Vector3 _inicioAlvo;
        private bool _inicializado;

        private void OnEnable()
        {
            PrepararInicio();
        }

        private void PrepararInicio()
        {
            if (alvo == null && Camera.main != null)
            {
                alvo = Camera.main.transform;
            }

            _inicioCamada = transform.position;
            if (alvo != null) _inicioAlvo = alvo.position;

            _inicializado = alvo != null;
        }

        private void LateUpdate()
        {
            if (!_inicializado)
            {
                PrepararInicio();
                if (!_inicializado) return;
            }

            Vector3 viagem = alvo.position - _inicioAlvo;

            // Um fator menor significa mais longe, entao a camada acompanha mais
            // a camera e se desloca menos em relacao a ela.
            float x = _inicioCamada.x + viagem.x * (1f - fatorParalaxe);
            float y = verticalmenteSeguir
                ? _inicioCamada.y + viagem.y * (1f - fatorVertical)
                : _inicioCamada.y;

            transform.position = new Vector3(x, y, _inicioCamada.z);
        }
    }
}
