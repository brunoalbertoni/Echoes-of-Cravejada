using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um lugar onde o Scrap pode aparecer ao chegar de outra cena. Quem
    /// escolhe qual e a SceneExit de onde ele veio, pelo Id.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private string id;

        public string Id => id;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(string newId)
        {
            id = newId;
        }
    }
}
