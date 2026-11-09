using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O que o Scrap carrega pra entregar a alguem: o motor do Coletor (pro
    /// Ferrolho) e as ferramentas do ourives (pro Rebite). Cada coisa tem um
    /// id; pegar guarda o id, entregar tira da mao e lembra que ja foi.
    ///
    /// Guarda tambem marcas soltas de conversa, como "o Rebite ja fez o pedido".
    ///
    /// Estatico pra atravessar a troca de cena; sem save, zera quando o jogo fecha.
    /// </summary>
    public static class KeyItems
    {
        private static readonly HashSet<string> Held = new HashSet<string>();
        private static readonly HashSet<string> Delivered = new HashSet<string>();
        private static readonly HashSet<string> Flags = new HashSet<string>();

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Held.Clear();
            Delivered.Clear();
            Flags.Clear();
        }

        public static void Add(string id)
        {
            if (!Delivered.Contains(id)) Held.Add(id);
        }

        /// <summary>Se ele esta com isso na mao agora.</summary>
        public static bool Has(string id) => Held.Contains(id);

        /// <summary>Se isso ja foi entregue a alguem.</summary>
        public static bool WasDelivered(string id) => Delivered.Contains(id);

        /// <summary>Se ele ja pegou isso alguma vez, esteja na mao ou entregue.</summary>
        public static bool WasFound(string id) => Held.Contains(id) || Delivered.Contains(id);

        /// <summary>Entrega: sai da mao e fica lembrado. Falso se ele nao estava com isso.</summary>
        public static bool Deliver(string id)
        {
            if (!Held.Remove(id)) return false;

            Delivered.Add(id);
            return true;
        }

        /// <summary>Quantas coisas com esse comeco de id ele tem na mao.</summary>
        public static int CountHeld(string prefix) => Count(Held, prefix);

        /// <summary>Quantas coisas com esse comeco de id ele ja achou, na mao ou entregues.</summary>
        public static int CountFound(string prefix) => Count(Held, prefix) + Count(Delivered, prefix);

        /// <summary>Entrega tudo o que ele tem na mao com esse comeco de id.</summary>
        public static void DeliverAll(string prefix)
        {
            var ids = new List<string>();
            foreach (string id in Held)
            {
                if (id.StartsWith(prefix)) ids.Add(id);
            }

            foreach (string id in ids) Deliver(id);
        }

        public static void SetFlag(string flag) => Flags.Add(flag);

        public static bool HasFlag(string flag) => Flags.Contains(flag);

        private static int Count(HashSet<string> set, string prefix)
        {
            int count = 0;
            foreach (string id in set)
            {
                if (id.StartsWith(prefix)) count++;
            }

            return count;
        }
    }
}
