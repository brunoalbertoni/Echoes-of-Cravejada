using UnityEditor;
using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Grava e importa os quadros da cutscene desenhada da purificacao
    /// (PurificationCartoonArt) e monta a lista que o PurificationCartoon toca.
    ///
    /// Cada quadro vira shot{i}_before.png, shot{i}_after.png, shot{i}_order.png
    /// e, se tiver, shot{i}_loop{k}.png, em Assets/Generated/Cutscene. As
    /// texturas sao importadas legiveis, sem compressao e com filtro de pixel:
    /// o tocador mistura os pixels na CPU a cada quadro.
    ///
    /// Os PNGs podem ser trocados por arte feita a mao, desde que fiquem em
    /// 320x180 — mas gerar a Phase 1 de novo grava os desenhos por cima.
    /// </summary>
    public static class CartoonAssets
    {
        public const string Folder = Phase01Assets.GeneratedRoot + "/Cutscene";

        private static string PathOf(int shot, string part) => $"{Folder}/shot{shot}_{part}.png";

        /// <summary>Desenha e importa todos os quadros.</summary>
        public static void EnsureArt()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder(Phase01Assets.GeneratedRoot, "Cutscene");
            }

            PurificationCartoonArt.Shot[] shots = PurificationCartoonArt.All();

            for (int i = 0; i < shots.Length; i++)
            {
                PurificationCartoonArt.Shot shot = shots[i];
                Write(PathOf(i, "before"), shot.Before);
                Write(PathOf(i, "after"), shot.After);
                Write(PathOf(i, "order"), shot.Order);

                if (shot.Loop == null) continue;
                for (int k = 0; k < shot.Loop.Length; k++) Write(PathOf(i, "loop" + k), shot.Loop[k]);
            }
        }

        /// <summary>Carrega os quadros ja gravados, com duracao, camera e particulas.</summary>
        public static CartoonShot[] LoadShots()
        {
            PurificationCartoonArt.Shot[] art = PurificationCartoonArt.All();
            var shots = new CartoonShot[art.Length];

            for (int i = 0; i < art.Length; i++)
            {
                PurificationCartoonArt.Shot source = art[i];

                Texture2D[] loop = null;
                if (source.Loop != null)
                {
                    loop = new Texture2D[source.Loop.Length];
                    for (int k = 0; k < loop.Length; k++) loop[k] = Load(PathOf(i, "loop" + k));
                }

                var emitters = new CartoonEmitter[source.Emitters?.Length ?? 0];
                for (int k = 0; k < emitters.Length; k++)
                {
                    PurificationCartoonArt.Emitter e = source.Emitters[k];
                    emitters[k] = new CartoonEmitter
                    {
                        kind = (CartoonEmitter.Kind)(int)e.Kind,
                        area = new RectInt(e.X, e.Y, e.W, e.H),
                        startAt = e.StartAt,
                    };
                }

                shots[i] = new CartoonShot
                {
                    before = Load(PathOf(i, "before")),
                    after = Load(PathOf(i, "after")),
                    order = Load(PathOf(i, "order")),
                    loop = loop,
                    emitters = emitters,
                    duration = source.Duration,
                    pan = new Vector2(source.PanX, source.PanY),
                    zoomFrom = source.ZoomFrom,
                    zoomTo = source.ZoomTo,
                };
            }

            return shots;
        }

        private static void Write(string path, PixelCanvas canvas)
        {
            Phase02Assets.WriteCanvas(path, canvas);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[Cutscene] Nao consegui importar {path}.");
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.isReadable = true;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = false;
            importer.sRGBTexture = true;
            importer.SaveAndReimport();
        }

        private static Texture2D Load(string path)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) Debug.LogWarning($"[Cutscene] Quadro {path} nao existe.");
            return texture;
        }
    }
}
