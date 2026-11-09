using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Monta a cenografia do District 1 na cena (o desenho e o DecorArt; onde
    /// fica cada peca, o Phase01Decor).
    ///
    /// Quatro raizes, uma por camada, mais duas que a purificacao usa:
    ///   Decor_Wall    — na parede, pintada pela cor do TM_Background (TintFollower).
    ///   Decor_Back    — no chao, atras do terreno.
    ///   Decor_Middle  — no chao, na frente do terreno.
    ///   Decor_Front   — na frente do Scrap, em silhueta.
    ///   Decor_Oxide   — pocas e respiros: somem com a purificacao.
    /// As luzes vao pra raiz "Lighting": as verdes com nome OxideGlow_ (a
    /// purificacao apaga) e as dos postes com nome DecorLamp_ (a purificacao acende).
    /// </summary>
    public static class Phase01DecorBuilder
    {
        public const string WallRoot = "Decor_Wall";
        public const string OxideRoot = "Decor_Oxide";
        public const string LampLightPrefix = "DecorLamp_";

        private const int WallOrder = 2;
        private const int BackOrder = 7;
        private const int MiddleOrder = 21;

        // Escura o bastante pra os inimigos (do mesmo metal) nao sumirem na frente dela.
        private static readonly Color BackTint = new Color(0.60f, 0.67f, 0.65f);
        private static readonly Color FrontTint = new Color(0.30f, 0.36f, 0.36f);
        private static readonly Color WallTint = new Color(0.52f, 0.63f, 0.60f);
        private static readonly Color OxideGreen = new Color(0.36f, 0.95f, 0.60f);
        private static readonly Color LampWarm = new Color(1f, 0.82f, 0.55f);

        private const float LampIntensity = 0.8f;

        /// <summary>Desenha e importa todas as pecas.</summary>
        public static void EnsureArt()
        {
            foreach (KeyValuePair<string, PixelCanvas> piece in DecorArt.All())
            {
                DecorArt.Spec spec = DecorArt.SpecOf(piece.Key);
                Phase02Assets.WriteSprite(piece.Key, piece.Value, new Vector2(spec.PivotX, spec.PivotY), spec.Tiled,
                    DecorArt.PixelsPerUnit);
            }
        }

        public static void Build()
        {
            var root = new GameObject("Decor");

            Transform wall = Child(root.transform, WallRoot);

            // Os montes pintados na parede: bem mais apagados que o resto, sao o fundo do fundo.
            Transform far = Child(root.transform, "Decor_WallFar");
            Transform back = Child(root.transform, "Decor_Back");
            Transform middle = Child(root.transform, "Decor_Middle");
            Transform front = Child(root.transform, "Decor_Front");
            Transform oxide = Child(root.transform, OxideRoot);

            GameObject lightingObject = GameObject.Find("Lighting");
            Transform lighting = lightingObject != null ? lightingObject.transform : root.transform;

            var sprites = new Dictionary<string, Sprite>();
            Sprite Load(string name)
            {
                if (!sprites.TryGetValue(name, out Sprite sprite))
                {
                    sprite = Phase02Assets.LoadSprite(name);
                    sprites[name] = sprite;
                }

                return sprite;
            }

            int index = 0;
            int oxideLights = 0;

            foreach (Phase01Decor.Piece piece in Phase01Decor.Pieces)
            {
                bool farHeap = piece.Layer == Phase01Decor.Layer.Wall && piece.Sprite.StartsWith("d1_heap");

                Transform parent = piece.Oxide
                    ? oxide
                    : farHeap
                    ? far
                    : piece.Layer switch
                    {
                        Phase01Decor.Layer.Wall => wall,
                        Phase01Decor.Layer.Back => back,
                        Phase01Decor.Layer.Front => front,
                        _ => middle,
                    };

                SpriteRenderer renderer = Place(parent, $"{piece.Sprite}_{++index}", Load(piece.Sprite),
                    new Vector3(piece.X, piece.Y, 0f), piece.Layer);
                if (renderer == null) continue;

                renderer.flipX = piece.Flip;

                if (piece.Width > 0f || piece.Height > 0f)
                {
                    renderer.drawMode = SpriteDrawMode.Tiled;
                    Vector2 natural = renderer.sprite.bounds.size;
                    renderer.size = new Vector2(piece.Width > 0f ? piece.Width : natural.x, piece.Height > 0f ? piece.Height : natural.y);
                }

                if (piece.Motion == Phase01Decor.Motion.Spin) renderer.gameObject.AddComponent<SlowSpin>().Configure(piece.Speed);

                if (piece.Oxide)
                {
                    // O Oxide aceso: fica na cor dele, nao na da parede (o respiro, preso
                    // nela, so um pouco apagado).
                    renderer.color = piece.Layer == Phase01Decor.Layer.Wall ? new Color(0.72f, 0.82f, 0.80f) : Color.white;
                    Vector2 size = renderer.sprite.bounds.size;
                    Phase01Generator.CreatePointLight(lighting, $"OxideGlow_Decor_{++oxideLights}",
                        (piece.X + size.x * 0.5f, piece.Y + size.y * 0.5f), OxideGreen, 0.5f, 2.4f);
                }
            }

            BuildHangings(back, front, Load);
            BuildLamps(back, lighting, Load);

            // A cenografia da parede muda de cor junto com ela.
            GameObject background = GameObject.Find("TM_Background");
            if (background != null)
            {
                wall.gameObject.AddComponent<TintFollower>().Configure(background.GetComponent<Tilemap>(), 1.15f);
                far.gameObject.AddComponent<TintFollower>().Configure(background.GetComponent<Tilemap>(), 0.72f);
            }
        }

        private static Transform Child(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static SpriteRenderer Place(Transform parent, string name, Sprite sprite, Vector3 position, Phase01Decor.Layer layer)
        {
            if (sprite == null) return null;

            var pieceObject = new GameObject(name);
            pieceObject.transform.SetParent(parent, false);
            pieceObject.transform.position = position;

            var renderer = pieceObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            Style(renderer, layer);
            return renderer;
        }

        private static void Style(SpriteRenderer renderer, Phase01Decor.Layer layer)
        {
            switch (layer)
            {
                case Phase01Decor.Layer.Wall:
                    renderer.sortingLayerName = Phase01Assets.SortingTerrain;
                    renderer.sortingOrder = WallOrder;
                    renderer.color = WallTint;
                    break;

                case Phase01Decor.Layer.Back:
                    renderer.sortingLayerName = Phase01Assets.SortingTerrain;
                    renderer.sortingOrder = BackOrder;
                    renderer.color = BackTint;
                    break;

                case Phase01Decor.Layer.Front:
                    renderer.sortingLayerName = Phase01Assets.SortingForeground;
                    renderer.sortingOrder = 1;
                    renderer.color = FrontTint;
                    break;

                default:
                    renderer.sortingLayerName = Phase01Assets.SortingTerrain;
                    renderer.sortingOrder = MiddleOrder;
                    break;
            }
        }

        /// <summary>
        /// As correntes: um ponto preso no teto que balanca, com a corrente
        /// (repetida no comprimento) e o que tiver na ponta.
        /// </summary>
        private static void BuildHangings(Transform back, Transform front, System.Func<string, Sprite> load)
        {
            Sprite chain = load(DecorArt.Chain);
            int index = 0;

            foreach (Phase01Decor.Hanging hanging in Phase01Decor.Hangings)
            {
                Transform parent = hanging.Front ? front : back;
                Phase01Decor.Layer layer = hanging.Front ? Phase01Decor.Layer.Front : Phase01Decor.Layer.Back;

                var pivot = new GameObject($"Hanging_{++index}");
                pivot.transform.SetParent(parent, false);
                pivot.transform.position = new Vector3(hanging.X, hanging.Top, 0f);

                // Mais curta, balanca mais rapido; cada uma no seu tempo.
                pivot.AddComponent<Sway>().Configure(2.5f, 2.6f + hanging.Length * 0.45f, (hanging.X * 0.37f) % 1f);

                if (chain != null)
                {
                    var chainObject = new GameObject("Chain");
                    chainObject.transform.SetParent(pivot.transform, false);
                    chainObject.transform.localPosition = new Vector3(-0.5f, -hanging.Length, 0f);

                    var chainRenderer = chainObject.AddComponent<SpriteRenderer>();
                    chainRenderer.sprite = chain;
                    chainRenderer.drawMode = SpriteDrawMode.Tiled;
                    chainRenderer.size = new Vector2(1f, hanging.Length);
                    Style(chainRenderer, layer);
                }

                if (hanging.End == null) continue;

                Sprite end = load(hanging.End);
                if (end == null) continue;

                var endObject = new GameObject(hanging.End);
                endObject.transform.SetParent(pivot.transform, false);
                endObject.transform.localPosition = new Vector3(0f, -hanging.Length, 0f);

                var endRenderer = endObject.AddComponent<SpriteRenderer>();
                endRenderer.sprite = end;
                Style(endRenderer, layer);
            }
        }

        /// <summary>
        /// Os postes de lampiao: o poste apagado e, por cima, o vidro aceso.
        /// Os das bancadas ficam acesos; os outros tem uma luz que a
        /// purificacao sobe, e o vidro acompanha (LampGlass).
        /// </summary>
        private static void BuildLamps(Transform back, Transform lighting, System.Func<string, Sprite> load)
        {
            Sprite post = load(DecorArt.LampPost);
            Sprite glow = load(DecorArt.LampPostGlow);
            int index = 0;

            foreach (Phase01Decor.Lamp lamp in Phase01Decor.Lamps)
            {
                index++;
                SpriteRenderer postRenderer = Place(back, $"LampPost_{index}", post, new Vector3(lamp.X, lamp.Y, 0f),
                    Phase01Decor.Layer.Back);
                if (postRenderer == null || glow == null) continue;

                var glassObject = new GameObject("Glass");
                glassObject.transform.SetParent(postRenderer.transform, false);

                var glass = glassObject.AddComponent<SpriteRenderer>();
                glass.sprite = glow;
                glass.sortingLayerName = Phase01Assets.SortingTerrain;
                glass.sortingOrder = BackOrder + 1;

                if (lamp.AlwaysLit) continue;

                Light2D light = Phase01Generator.CreatePointLight(lighting, $"{LampLightPrefix}{index}", lamp.Glass,
                    LampWarm, LampIntensity, 5f);
                glassObject.AddComponent<LampGlass>().Configure(light, LampIntensity);
            }
        }
    }
}
