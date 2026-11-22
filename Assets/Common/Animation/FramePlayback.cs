using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O relogio de animacao dos chefes: guarda qual sequencia da folha esta
    /// tocando e diz qual quadro mostrar agora. Quem exibe o quadro e o chefe,
    /// porque cada um tem as proprias regras (dano, fase dois, morte).
    /// </summary>
    public sealed class FramePlayback
    {
        private int _quantidade;
        private float _quadrosPorSegundo;
        private bool _repeticao = true;
        private float _desde;

        public FramePlayback(int quantidade = 1, float quadrosPorSegundo = 1f)
        {
            _quantidade = quantidade;
            _quadrosPorSegundo = quadrosPorSegundo;
        }

        /// <summary>O primeiro quadro da sequencia atual.</summary>
        public int Inicio { get; private set; }

        /// <summary>O quadro que deve aparecer agora.</summary>
        public int Quadro
        {
            get
            {
                int passo = (int)((Time.time - _desde) * _quadrosPorSegundo);
                passo = _repeticao ? passo % _quantidade : Mathf.Min(passo, _quantidade - 1);
                return Inicio + passo;
            }
        }

        /// <summary>Toca uma sequencia. Em loop, pedir a mesma de novo nao reinicia.</summary>
        public void Tocar(int inicio, int quantidade, float quadrosPorSegundo, bool repeticao = true)
        {
            if (repeticao && _repeticao && Inicio == inicio && _quantidade == quantidade)
            {
                _quadrosPorSegundo = quadrosPorSegundo;
                return;
            }

            Inicio = inicio;
            _quantidade = Mathf.Max(1, quantidade);
            _quadrosPorSegundo = Mathf.Max(0.01f, quadrosPorSegundo);
            _repeticao = repeticao;
            _desde = Time.time;
        }
    }
}
