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
        [SaveProgress("Held")]
        private static readonly HashSet<string> Segurado = new HashSet<string>();
        [SaveProgress("Delivered")]
        private static readonly HashSet<string> Entregues = new HashSet<string>();
        [SaveProgress("Flags")]
        private static readonly HashSet<string> Marcas = new HashSet<string>();

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Segurado.Clear();
            Entregues.Clear();
            Marcas.Clear();
        }

        public static void Adicionar(string id)
        {
            if (!Entregues.Contains(id)) Segurado.Add(id);
        }

        /// <summary>Se ele esta com isso na mao agora.</summary>
        public static bool Tem(string id) => Segurado.Contains(id);

        /// <summary>Se isso ja foi entregue a alguem.</summary>
        public static bool FoiEntregue(string id) => Entregues.Contains(id);

        /// <summary>Se ele ja pegou isso alguma vez, esteja na mao ou entregue.</summary>
        public static bool FoiEncontrado(string id) => Segurado.Contains(id) || Entregues.Contains(id);

        /// <summary>Entrega: sai da mao e fica lembrado. Falso se ele nao estava com isso.</summary>
        public static bool Entregar(string id)
        {
            if (!Segurado.Remove(id)) return false;

            Entregues.Add(id);
            return true;
        }

        /// <summary>Quantas coisas com esse comeco de id ele tem na mao.</summary>
        public static int ContarGuardados(string prefixo) => Contar(Segurado, prefixo);

        /// <summary>Quantas coisas com esse comeco de id ele ja achou, na mao ou entregues.</summary>
        public static int ContarEncontrados(string prefixo) => Contar(Segurado, prefixo) + Contar(Entregues, prefixo);

        /// <summary>Entrega tudo o que ele tem na mao com esse comeco de id.</summary>
        public static void EntregarTudo(string prefixo)
        {
            var ids = new List<string>();
            foreach (string id in Segurado)
            {
                if (id.StartsWith(prefixo)) ids.Add(id);
            }

            foreach (string id in ids) Entregar(id);
        }

        public static void DefinirMarca(string marca) => Marcas.Add(marca);

        public static bool TemMarca(string marca) => Marcas.Contains(marca);

        private static int Contar(HashSet<string> conjunto, string prefixo)
        {
            int quantidade = 0;
            foreach (string id in conjunto)
            {
                if (id.StartsWith(prefixo)) quantidade++;
            }

            return quantidade;
        }
    }
}
