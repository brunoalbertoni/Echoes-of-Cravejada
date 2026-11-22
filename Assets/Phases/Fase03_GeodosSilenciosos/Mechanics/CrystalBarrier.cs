using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma barreira de cristal macico. O cinzel comum so tira lascas; quem a
    /// quebra e o Resonance Strike, a onda que o Scrap ganha do Geode Drill.
    ///
    /// Quebrada, fica quebrada ate o jogo fechar — cada uma se guarda pelo id.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CrystalBarrier : MonoBehaviour
    {
        [Tooltip("Nome unico da barreira, pra lembrar que ja foi quebrada.")]
        [SerializeField] private string id;

        [SaveProgress("Broken")]
        private static readonly HashSet<string> Quebrados = new HashSet<string>();

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Quebrados.Clear();
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(string novoId)
        {
            id = novoId;
        }

        private void Start()
        {
            if (Quebrados.Contains(id)) gameObject.SetActive(false);
        }

        /// <summary>Chamado pelo Resonance Strike quando a onda a alcanca.</summary>
        public void Estilhacar()
        {
            Quebrados.Add(id);

            // O cristal estourando e um barulho e tanto.
            NoiseSystem.Emitir(transform.position, 16f, 1f);
            Sfx.EstilhacoBarreira.Tocar(transform.position);
            gameObject.SetActive(false);
        }
    }
}
