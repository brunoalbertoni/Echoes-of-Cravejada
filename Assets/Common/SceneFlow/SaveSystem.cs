using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O save do jogo: um arquivo so (save.json, na pasta de dados do jogo).
    ///
    /// Grava-se ao salvar numa Bancada de Ourives: a cena, onde fica a
    /// bancada e todo o progresso do Scrap (habilidades, fragmentos, gemas,
    /// chefes derrotados, areas purificadas, itens, compras...). Carregar
    /// devolve tudo isso e poe o Scrap de pe na bancada, acesa.
    ///
    /// O progresso mora em campos estaticos marcados com [SaveProgress]. Eles
    /// sao achados e gravados por reflexao, com a chave Classe.campo: renomear
    /// um deles invalida esse dado nos saves antigos.
    /// </summary>
    public static class SaveSystem
    {
        private const int Versao = 1;

        private const BindingFlags Estaticos = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static (Type Type, string Member)[] _membros;

        /// <summary>O que conta como progresso: todo campo estatico marcado com [SaveProgress].</summary>
        private static (Type Type, string Member)[] Membros => _membros ?? (_membros = Descobrir());

        private static (Type, string)[] Descobrir()
        {
            var encontrado = new List<(Type, string)>();

            foreach (Type tipoDado in typeof(SaveSystem).Assembly.GetTypes())
            {
                foreach (MemberInfo membro in tipoDado.GetMembers(Estaticos | BindingFlags.DeclaredOnly))
                {
                    if (membro.IsDefined(typeof(SaveProgressAttribute), false)) encontrado.Add((tipoDado, membro.Name));
                }
            }

            encontrado.Sort((a, b) => string.CompareOrdinal(Chave(a.Item1, a.Item2), Chave(b.Item1, b.Item2)));
            return encontrado.ToArray();
        }

        [Serializable]
        private class Entry
        {
            public string key;
            public bool flag;
            public int number;
            public List<string> names = new List<string>();
            public List<int> counts = new List<int>();
        }

        [Serializable]
        private class Data
        {
            public int version;
            public string scene;
            public float benchX;
            public float benchY;
            public string savedAt;
            public List<Entry> entries = new List<Entry>();
        }

        private static string CaminhoArquivo => Path.Combine(Application.persistentDataPath, "save.json");

        /// <summary>Onde o Scrap deve aparecer ao abrir a cena carregada (a bancada). Vale uma vez.</summary>
        private static Vector3? _bancadaPendente;

        /// <summary>A bancada do save carregado: ela abre acesa.</summary>
        public static Vector3? BancadaCarregado { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _bancadaPendente = null;
            BancadaCarregado = null;
        }

        /// <summary>Se existe um save pra continuar.</summary>
        public static bool TemSalvar => File.Exists(CaminhoArquivo);

        /// <summary>A cena do save, ou null.</summary>
        public static string CenaSalvo
        {
            get
            {
                Data dados = Ler();
                return dados != null ? dados.scene : null;
            }
        }

        // ------------------------------------------------------------------
        // Gravar
        // ------------------------------------------------------------------

        /// <summary>Grava o jogo, com o Scrap renascendo em <paramref name="bancada2"/> na cena atual.</summary>
        public static void Salvar(Vector3 bancada2)
        {
            var dados = new Data
            {
                version = Versao,
                scene = SceneManager.GetActiveScene().name,
                benchX = bancada2.x,
                benchY = bancada2.y,
                savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            };

            foreach ((Type tipoDado, string membro) in Membros)
            {
                var entrada = new Entry { key = Chave(tipoDado, membro) };
                object valor = Obter(tipoDado, membro);

                switch (valor)
                {
                    case bool marca: entrada.flag = marca; break;
                    case int numero: entrada.number = numero; break;
                    case HashSet<string> conjunto: entrada.names.AddRange(conjunto); break;
                    case Dictionary<string, int> mapa:
                        foreach (KeyValuePair<string, int> par in mapa)
                        {
                            entrada.names.Add(par.Key);
                            entrada.counts.Add(par.Value);
                        }
                        break;
                    default: continue;
                }

                dados.entries.Add(entrada);
            }

            try
            {
                // Grava num arquivo ao lado e troca: um jogo fechado no meio nao corrompe o save.
                string temporario = CaminhoArquivo + ".tmp";
                File.WriteAllText(temporario, JsonUtility.ToJson(dados, true));
                if (File.Exists(CaminhoArquivo)) File.Delete(CaminhoArquivo);
                File.Move(temporario, CaminhoArquivo);
            }
            catch (Exception erro)
            {
                Debug.LogError("[Save] Nao deu pra gravar: " + erro.Message);
            }
        }

        // ------------------------------------------------------------------
        // Carregar e comecar do zero
        // ------------------------------------------------------------------

        /// <summary>Zera todo o progresso (o comeco de um jogo novo). Nao apaga o arquivo.</summary>
        public static void ReiniciarProgresso()
        {
            foreach ((Type tipoDado, string membro) in Membros)
            {
                object valor = Obter(tipoDado, membro);
                switch (valor)
                {
                    case bool _: Definir(tipoDado, membro, false); break;
                    case int _: Definir(tipoDado, membro, 0); break;
                    case HashSet<string> conjunto: conjunto.Clear(); break;
                    case Dictionary<string, int> mapa: mapa.Clear(); break;
                }
            }

            _bancadaPendente = null;
            BancadaCarregado = null;
        }

        /// <summary>
        /// Devolve o progresso do save e diz em que cena continuar (null se nao
        /// houver save). Quem chama troca pra essa cena; la, o Scrap aparece na
        /// bancada.
        /// </summary>
        public static string Carregar()
        {
            Data dados = Ler();
            if (dados == null) return null;

            ReiniciarProgresso();

            foreach (Entry entrada in dados.entries)
            {
                (Type tipoDado, string membro) = Encontrar(entrada.key);
                if (tipoDado == null) continue;

                object valor = Obter(tipoDado, membro);
                switch (valor)
                {
                    case bool _: Definir(tipoDado, membro, entrada.flag); break;
                    case int _: Definir(tipoDado, membro, entrada.number); break;
                    case HashSet<string> conjunto:
                        foreach (string nome in entrada.names) conjunto.Add(nome);
                        break;
                    case Dictionary<string, int> mapa:
                        for (int i = 0; i < entrada.names.Count && i < entrada.counts.Count; i++) mapa[entrada.names[i]] = entrada.counts[i];
                        break;
                }
            }

            var bancada2 = new Vector3(dados.benchX, dados.benchY, 0f);
            _bancadaPendente = bancada2;
            BancadaCarregado = bancada2;
            return dados.scene;
        }

        /// <summary>Se a cena que vai abrir veio de um save (sem consumir o recado).</summary>
        public static bool TemBancadaPendente => _bancadaPendente.HasValue;

        /// <summary>A SceneArrival pergunta: se o Scrap veio de um save, onde ele aparece.</summary>
        public static bool PegarBancadaPendente(out Vector3 bancada2)
        {
            bancada2 = _bancadaPendente ?? Vector3.zero;
            bool tem = _bancadaPendente.HasValue;
            _bancadaPendente = null;
            return tem;
        }

        private static Data Ler()
        {
            try
            {
                if (!File.Exists(CaminhoArquivo)) return null;
                var dados = JsonUtility.FromJson<Data>(File.ReadAllText(CaminhoArquivo));
                return dados != null && !string.IsNullOrEmpty(dados.scene) ? dados : null;
            }
            catch (Exception erro)
            {
                Debug.LogError("[Save] Save ilegivel: " + erro.Message);
                return null;
            }
        }

        // ------------------------------------------------------------------
        // Reflexao
        // ------------------------------------------------------------------

        private static string Chave(Type tipoDado, string membro)
        {
            MemberInfo info = tipoDado.GetMember(membro, Estaticos)[0];
            var marca = info.GetCustomAttribute<SaveProgressAttribute>();
            return tipoDado.Name + "." + (marca != null ? marca.ChaveDoSave : membro);
        }

        private static (Type, string) Encontrar(string chave)
        {
            foreach ((Type tipoDado, string membro) in Membros)
            {
                if (Chave(tipoDado, membro) == chave) return (tipoDado, membro);
            }

            return (null, null);
        }

        private static object Obter(Type tipoDado, string membro)
        {
            FieldInfo campo = tipoDado.GetField(membro, Estaticos);
            if (campo != null) return campo.GetValue(null);

            PropertyInfo propriedade = tipoDado.GetProperty(membro, Estaticos);
            if (propriedade != null) return propriedade.GetValue(null);

            Debug.LogWarning($"[Save] {tipoDado.Name}.{membro} nao existe mais: tirar da lista do SaveSystem.");
            return null;
        }

        private static void Definir(Type tipoDado, string membro, object valor)
        {
            FieldInfo campo = tipoDado.GetField(membro, Estaticos);
            if (campo != null && !campo.IsInitOnly)
            {
                campo.SetValue(null, valor);
                return;
            }

            PropertyInfo propriedade = tipoDado.GetProperty(membro, Estaticos);
            MethodInfo definidor = propriedade?.GetSetMethod(true);
            definidor?.Invoke(null, new[] { valor });
        }
    }
}
