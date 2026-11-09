using UnityEngine;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Fica no tilemap do terreno e garante que o colisor dele exista assim que
    /// a cena comeca.
    ///
    /// Quando a cena e carregada no meio do jogo (vindo de outra fase), o
    /// colisor composto do terreno chegava vazio e o Scrap atravessava o chao.
    /// Dando Play direto na cena isso nao acontecia, porque o editor ja tinha a
    /// geometria montada. Aqui ela e refeita na hora, sempre.
    ///
    /// Se mesmo assim o composto ficar sem forma nenhuma, o terreno passa a
    /// colidir tile a tile — menos liso nas emendas, mas com chao.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(TilemapCollider2D))]
    public class GroundColliderRefresh : MonoBehaviour
    {
        private void Awake()
        {
            var tiles = GetComponent<TilemapCollider2D>();
            var composite = GetComponent<CompositeCollider2D>();

            tiles.ProcessTilemapChanges();

            if (composite == null) return;

            composite.GenerateGeometry();
            Physics2D.SyncTransforms();

            if (composite.shapeCount > 0) return;

            Debug.LogWarning("[GroundColliderRefresh] O colisor composto do terreno ficou vazio; " +
                             "usando a colisao tile a tile.", this);

            tiles.compositeOperation = Collider2D.CompositeOperation.None;
            tiles.ProcessTilemapChanges();
        }
    }
}
