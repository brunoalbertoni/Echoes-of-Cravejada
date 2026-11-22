using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A ponte entre a logica e o Animator. O cerebro de cada personagem segue
    /// decidindo o quadro (o tempo dos golpes depende disso); aqui esse quadro
    /// vira um estado e um ponto do clipe no Animator Controller.
    ///
    /// Os clipes e esta tabela sao criados por "Echoes/Gerar Animators". Quem
    /// nao tem este componente recebe o sprite direto, como antes.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class FrameAnimator : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("sprites")] [SerializeField] private Sprite[] sprites;
        [UnityEngine.Serialization.FormerlySerializedAs("states")] [SerializeField] private int[] estados;
        [UnityEngine.Serialization.FormerlySerializedAs("times")] [SerializeField] private float[] tempos;

        private Animator _animador;
        private SpriteRenderer _renderizador;
        private Dictionary<Sprite, int> _indice;
        private Sprite _mostrado;

        /// <summary>Mostra um sprite, pelo Animator quando o objeto tem um.</summary>
        public static void Exibir(SpriteRenderer renderizador, Sprite sprite)
        {
            if (renderizador == null || sprite == null) return;

            if (renderizador.TryGetComponent(out FrameAnimator quadros)) quadros.Mostrar(sprite);
            else renderizador.sprite = sprite;
        }

        /// <summary>Usado pelo gerador de Animators.</summary>
        public void Configurar(Sprite[] novoSprites, int[] novoEstados, float[] novoTempos)
        {
            sprites = novoSprites;
            estados = novoEstados;
            tempos = novoTempos;
        }

        private void Awake()
        {
            Preparar();
        }

        private void OnEnable()
        {
            Preparar();

            // Ao religar, o Animator voltaria pro primeiro estado: repete o quadro atual.
            Sprite atual = _mostrado != null ? _mostrado : _renderizador.sprite;
            _mostrado = null;
            if (atual != null) Mostrar(atual);
        }

        private void Preparar()
        {
            if (_indice != null) return;

            _animador = GetComponent<Animator>();
            _renderizador = GetComponent<SpriteRenderer>();
            _animador.speed = 0f;
            _animador.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            _animador.keepAnimatorStateOnDisable = true;

            _indice = new Dictionary<Sprite, int>();
            if (sprites == null) return;

            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null && !_indice.ContainsKey(sprites[i])) _indice.Add(sprites[i], i);
            }
        }

        private void Mostrar(Sprite sprite)
        {
            Preparar();
            if (sprite == _mostrado && _renderizador.sprite == sprite) return;

            _mostrado = sprite;

            // Sprite fora da tabela (a queda da abertura, por exemplo): o Animator sai da frente.
            if (!_indice.TryGetValue(sprite, out int entrada))
            {
                _animador.enabled = false;
                _renderizador.sprite = sprite;
                return;
            }

            if (isActiveAndEnabled)
            {
                _animador.enabled = true;
                _animador.Play(estados[entrada], 0, tempos[entrada]);
                if (_animador.isInitialized) _animador.Update(0f);
            }

            _renderizador.sprite = sprite;
        }
    }
}
