using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>O que os sistemas gerais precisam saber de um chefe em luta.</summary>
    public interface IBossFight
    {
        bool EstaDerrubado { get; }

        /// <summary>A musica da luta agora. Null: silencio.</summary>
        MusicTrack MusicaBatalha { get; }

        /// <summary>Se a musica toca mesmo durante uma cena (o Rei cuida da propria).</summary>
        bool CinematicasIgnoraMusica { get; }
    }

    /// <summary>
    /// A lista dos chefes do jogo. Cada um se registra sozinho, entao um chefe
    /// novo nao precisa ser acrescentado no som nem no selo da arena.
    /// </summary>
    public static class BossRoster
    {
        private static readonly SortedDictionary<int, Func<MonoBehaviour>> Chefes =
            new SortedDictionary<int, Func<MonoBehaviour>>();

        /// <summary>Registra um chefe. Ordem menor tem prioridade se dois lutarem juntos.</summary>
        public static void Adicionar(int ordem, Func<MonoBehaviour> lutando)
        {
            Chefes[ordem] = lutando;
        }

        /// <summary>O chefe lutando agora, ou null.</summary>
        public static MonoBehaviour Atual
        {
            get
            {
                foreach (Func<MonoBehaviour> lutando in Chefes.Values)
                {
                    MonoBehaviour chefe = lutando();
                    if (chefe != null) return chefe;
                }

                return null;
            }
        }

        /// <summary>Se este objeto e um chefe em luta.</summary>
        public static bool EstaLutando(UnityEngine.Object chefe)
        {
            if (chefe == null) return false;

            foreach (Func<MonoBehaviour> lutando in Chefes.Values)
            {
                if (lutando() == chefe) return true;
            }

            return false;
        }
    }
}
