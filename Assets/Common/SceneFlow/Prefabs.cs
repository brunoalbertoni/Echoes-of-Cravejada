using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Cria projeteis, efeitos e coletaveis a partir dos prefabs em
    /// Resources/Prefabs. O prefab traz a estrutura (sprite, colisor, corpo);
    /// quem cria define os valores daquele disparo.
    /// </summary>
    public static class Prefabs
    {
        private static readonly Dictionary<string, GameObject> Cache = new Dictionary<string, GameObject>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Cache.Clear();
        }

        /// <summary>Instancia um prefab pelo nome. Sem o prefab, cria um objeto vazio.</summary>
        public static GameObject Criar(string prefab, string nomeDoObjeto = null)
        {
            if (!Cache.TryGetValue(prefab, out GameObject modelo))
            {
                modelo = Resources.Load<GameObject>("Prefabs/" + prefab);
                Cache[prefab] = modelo;
            }

            GameObject objeto = modelo != null ? Object.Instantiate(modelo) : new GameObject();
            objeto.name = nomeDoObjeto ?? prefab;
            return objeto;
        }

        /// <summary>Pega um componente do prefab, ou o acrescenta se ele nao vier.</summary>
        public static T Componente<T>(GameObject objeto) where T : Component
        {
            return objeto.TryGetComponent(out T componente) ? componente : objeto.AddComponent<T>();
        }
    }
}
