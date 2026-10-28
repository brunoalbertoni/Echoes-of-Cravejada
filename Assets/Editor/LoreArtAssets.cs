using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Grava e carrega os desenhos do LoreArt, e liga um deles num objeto de
    /// leitura: o desenho do mapa no filho "Art" e o grande no Npc.
    /// </summary>
    public static class LoreArtAssets
    {
        /// <summary>Grava os desenhos: cada desenho de mapa uma vez so, e o grande de cada um.</summary>
        public static void Write(IEnumerable<LoreArt.Piece> pieces)
        {
            var written = new HashSet<string>();
            foreach (LoreArt.Piece piece in pieces)
            {
                if (written.Add(piece.MapName))
                {
                    Phase02Assets.WriteSprite(piece.MapName, piece.Map, new Vector2(0.5f, 0f));
                }

                Phase02Assets.WriteSprite(LoreArt.ViewSprite(piece.Lore), piece.View, new Vector2(0.5f, 0.5f));

                if (piece.PurifiedView != null)
                {
                    Phase02Assets.WriteSprite(LoreArt.ViewSprite(piece.Lore, true), piece.PurifiedView, new Vector2(0.5f, 0.5f));
                }
            }
        }

        /// <summary>
        /// Poe o desenho do mapa no filho Art (na parede fica atras do terreno;
        /// no chao, na frente dele) e entrega o desenho grande pro Npc.
        /// </summary>
        public static void Apply(LoreArt.Piece piece, GameObject art, Npc readable, string sortingLayer)
        {
            art.transform.localPosition = new Vector3(0f, piece.Lift, 0f);

            SpriteRenderer renderer = art.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = art.AddComponent<SpriteRenderer>();
            renderer.sprite = Phase02Assets.LoadSprite(piece.MapName);
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = piece.OnWall ? 6 : 22;

            readable.ConfigureDocument(
                Phase02Assets.LoadSprite(LoreArt.ViewSprite(piece.Lore)),
                piece.PurifiedView != null ? Phase02Assets.LoadSprite(LoreArt.ViewSprite(piece.Lore, true)) : null);
        }
    }
}
