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
    /// O progresso mora em campos estaticos espalhados pelo jogo (os que
    /// zeram ao dar Play). A lista <see cref="Members"/> diz quais contam;
    /// eles sao lidos e escritos por reflexao. Campo novo de progresso:
    /// acrescentar aqui.
    /// </summary>
    public static class SaveSystem
    {
        private const int Version = 1;

        /// <summary>O que conta como progresso: (classe, campo ou propriedade estatica).</summary>
        private static readonly (Type Type, string Member)[] Members =
        {
            (typeof(ScrapWallet), "Shards"),
            (typeof(GemPurse), "Gems"),
            (typeof(ScrapController), "_pressureJump"),
            (typeof(ScrapDash), "_unlocked"),
            (typeof(ScrapChisel), "_resonance"),
            (typeof(ScrapAppearance), "_polished"),
            (typeof(ScrapHealth), "_bonusHealth"),
            (typeof(ScrapGems), "HasRuby"),
            (typeof(ScrapGems), "RubySocketed"),
            (typeof(ScrapGems), "HasSapphire"),
            (typeof(ScrapGems), "SapphireSocketed"),
            (typeof(KeyItems), "Held"),
            (typeof(KeyItems), "Delivered"),
            (typeof(KeyItems), "Flags"),
            (typeof(AreaPurifier), "Purified"),
            (typeof(RewardPickup), "Taken"),
            (typeof(Shop), "Bought"),
            (typeof(CrystalBarrier), "Broken"),
            (typeof(KeyGate), "Opened"),
            (typeof(WakeUpCutscene), "_played"),
            (typeof(BoilerMaster), "_defeated"),
            (typeof(TheResonant), "_defeated"),
            (typeof(GeodeDrill), "_defeated"),
            (typeof(MercuryAlchemist), "_defeated"),
            (typeof(SilverKing), "_defeated"),
            (typeof(SilverKing), "_introHeard"),
            (typeof(SilverKing), "_transformationSeen"),
            (typeof(SaveEngraving), "Count"),
        };

        private const BindingFlags Statics = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

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

        private static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

        /// <summary>Onde o Scrap deve aparecer ao abrir a cena carregada (a bancada). Vale uma vez.</summary>
        private static Vector3? _pendingBench;

        /// <summary>A bancada do save carregado: ela abre acesa.</summary>
        public static Vector3? LoadedBench { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _pendingBench = null;
            LoadedBench = null;
        }

        /// <summary>Se existe um save pra continuar.</summary>
        public static bool HasSave => File.Exists(FilePath);

        /// <summary>A cena do save, ou null.</summary>
        public static string SavedScene
        {
            get
            {
                Data data = Read();
                return data != null ? data.scene : null;
            }
        }

        // ------------------------------------------------------------------
        // Gravar
        // ------------------------------------------------------------------

        /// <summary>Grava o jogo, com o Scrap renascendo em <paramref name="bench"/> na cena atual.</summary>
        public static void Save(Vector3 bench)
        {
            var data = new Data
            {
                version = Version,
                scene = SceneManager.GetActiveScene().name,
                benchX = bench.x,
                benchY = bench.y,
                savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            };

            foreach ((Type type, string member) in Members)
            {
                var entry = new Entry { key = Key(type, member) };
                object value = Get(type, member);

                switch (value)
                {
                    case bool flag: entry.flag = flag; break;
                    case int number: entry.number = number; break;
                    case HashSet<string> set: entry.names.AddRange(set); break;
                    case Dictionary<string, int> map:
                        foreach (KeyValuePair<string, int> pair in map)
                        {
                            entry.names.Add(pair.Key);
                            entry.counts.Add(pair.Value);
                        }
                        break;
                    default: continue;
                }

                data.entries.Add(entry);
            }

            try
            {
                // Grava num arquivo ao lado e troca: um jogo fechado no meio nao corrompe o save.
                string temp = FilePath + ".tmp";
                File.WriteAllText(temp, JsonUtility.ToJson(data, true));
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(temp, FilePath);
            }
            catch (Exception error)
            {
                Debug.LogError("[Save] Nao deu pra gravar: " + error.Message);
            }
        }

        // ------------------------------------------------------------------
        // Carregar e comecar do zero
        // ------------------------------------------------------------------

        /// <summary>Zera todo o progresso (o comeco de um jogo novo). Nao apaga o arquivo.</summary>
        public static void ResetProgress()
        {
            foreach ((Type type, string member) in Members)
            {
                object value = Get(type, member);
                switch (value)
                {
                    case bool _: Set(type, member, false); break;
                    case int _: Set(type, member, 0); break;
                    case HashSet<string> set: set.Clear(); break;
                    case Dictionary<string, int> map: map.Clear(); break;
                }
            }

            _pendingBench = null;
            LoadedBench = null;
        }

        /// <summary>
        /// Devolve o progresso do save e diz em que cena continuar (null se nao
        /// houver save). Quem chama troca pra essa cena; la, o Scrap aparece na
        /// bancada.
        /// </summary>
        public static string Load()
        {
            Data data = Read();
            if (data == null) return null;

            ResetProgress();

            foreach (Entry entry in data.entries)
            {
                (Type type, string member) = Find(entry.key);
                if (type == null) continue;

                object value = Get(type, member);
                switch (value)
                {
                    case bool _: Set(type, member, entry.flag); break;
                    case int _: Set(type, member, entry.number); break;
                    case HashSet<string> set:
                        foreach (string name in entry.names) set.Add(name);
                        break;
                    case Dictionary<string, int> map:
                        for (int i = 0; i < entry.names.Count && i < entry.counts.Count; i++) map[entry.names[i]] = entry.counts[i];
                        break;
                }
            }

            var bench = new Vector3(data.benchX, data.benchY, 0f);
            _pendingBench = bench;
            LoadedBench = bench;
            return data.scene;
        }

        /// <summary>Se a cena que vai abrir veio de um save (sem consumir o recado).</summary>
        public static bool HasPendingBench => _pendingBench.HasValue;

        /// <summary>A SceneArrival pergunta: se o Scrap veio de um save, onde ele aparece.</summary>
        public static bool TakePendingBench(out Vector3 bench)
        {
            bench = _pendingBench ?? Vector3.zero;
            bool has = _pendingBench.HasValue;
            _pendingBench = null;
            return has;
        }

        private static Data Read()
        {
            try
            {
                if (!File.Exists(FilePath)) return null;
                var data = JsonUtility.FromJson<Data>(File.ReadAllText(FilePath));
                return data != null && !string.IsNullOrEmpty(data.scene) ? data : null;
            }
            catch (Exception error)
            {
                Debug.LogError("[Save] Save ilegivel: " + error.Message);
                return null;
            }
        }

        // ------------------------------------------------------------------
        // Reflexao
        // ------------------------------------------------------------------

        private static string Key(Type type, string member) => type.Name + "." + member;

        private static (Type, string) Find(string key)
        {
            foreach ((Type type, string member) in Members)
            {
                if (Key(type, member) == key) return (type, member);
            }

            return (null, null);
        }

        private static object Get(Type type, string member)
        {
            FieldInfo field = type.GetField(member, Statics);
            if (field != null) return field.GetValue(null);

            PropertyInfo property = type.GetProperty(member, Statics);
            if (property != null) return property.GetValue(null);

            Debug.LogWarning($"[Save] {type.Name}.{member} nao existe mais: tirar da lista do SaveSystem.");
            return null;
        }

        private static void Set(Type type, string member, object value)
        {
            FieldInfo field = type.GetField(member, Statics);
            if (field != null && !field.IsInitOnly)
            {
                field.SetValue(null, value);
                return;
            }

            PropertyInfo property = type.GetProperty(member, Statics);
            MethodInfo setter = property?.GetSetMethod(true);
            setter?.Invoke(null, new[] { value });
        }
    }
}
