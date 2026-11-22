using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A carcaca do Coletor, no fosso da arena depois que ele cai: o corpo
    /// tombado pra tras e afundado num monte de sucata, a cabeca (o olho
    /// apagado) arrancada e caida a direita, a chamine deitada a esquerda.
    /// Lataria chamuscada, frestas com brasa e fumaca saindo dos rombos.
    ///
    /// Nasce do proprio quadro do Coletor rachado (ColetorArt), cortado e
    /// remontado pelo WreckKit: o mesmo traco, 32 px por tile. Folha de
    /// FrameCount quadros em loop, mesmo pivo do Coletor, so mais larga.
    /// </summary>
    public static class ColetorWreckArt
    {
        public const int Width = 176;
        public const int Height = ColetorArt.Height;
        public const int PixelsPerUnit = ColetorArt.PixelsPerUnit;
        public const float PivotX = 0.5f;
        public const float PivotY = ColetorArt.PivotY;
        public const int FrameCount = 8;

        private const int Shift = (Width - ColetorArt.Width) / 2;
        private const int Ground = Height - 2;

        public static PixelCanvas[] Frames()
        {
            PixelCanvas source = ColetorArt.Frames(true)[ColetorArt.Death + 2];

            PixelCanvas head = WreckKit.Cut(source, (x, y) => x >= 72 && x <= 100 && y >= 18 && y <= 74);
            PixelCanvas chimney = WreckKit.Cut(source, (x, y) => x <= 34 && y >= 20 && y <= 58);
            PixelCanvas body = WreckKit.Cut(source, (x, y) => !(x >= 72 && x <= 100 && y >= 18 && y <= 74) && !(x <= 34 && y >= 20 && y <= 58) && y <= 100);

            var c = new PixelCanvas(Width, Height);
            Rgba[][] ramps = { ArtKit.Rust, ArtKit.Iron, ArtKit.Brass, ArtKit.Rust, ArtKit.Copper };

            // Atras de tudo: o monte do fundo, onde o corpo afunda.
            WreckKit.Rubble(c, Shift + 14, Shift + 104, Ground, 12, ramps, 101);

            WreckKit.Place(c, chimney, -84f, 28f, 40f, 22f, Ground - 6f);
            WreckKit.Place(c, body, -16f, 54f, 98f, Shift + 50f, Ground + 1f);
            WreckKit.Place(c, head, 104f, 86f, 50f, Shift + 132f, Ground - 14f);

            WreckKit.Scorch(c, 7, 0.32f, 0.2f);

            var embers = new List<(int X, int Y)>();
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 26f, 74f, 20f, 20, 3));
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 62f, 62f, 100f, 16, 5));
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 40f, 92f, -15f, 14, 8));
            WreckKit.Hole(c, Shift + 50f, 62f, 4.5f, embers);
            WreckKit.Hole(c, Shift + 24f, 86f, 3.2f, embers);
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 126f, Ground - 20f, 60f, 10, 11));

            // Na frente: mais sucata, a garra, parafusos.
            WreckKit.Rubble(c, Shift + 4, Shift + 122, Ground, 6, ramps, 202);
            WreckKit.Gear(c, Shift + 108f, Ground - 5f, 5f, ArtKit.Brass, 8, 0.3f);
            WreckKit.Plate(c, Shift + 14f, Ground - 3f, 12f, 5f, 12f, ArtKit.Rust, 9);
            WreckKit.Pipe(c, Shift + 84f, Ground - 2f, Shift + 98f, Ground - 6f, 1.6f, ArtKit.Iron);
            WreckKit.Scatter(c, 6, Width - 6, Ground, ramps, 303, 12);

            ArtKit.AddOutline(c);

            var vents = new[]
            {
                new WreckKit.Vent(Shift + 50f, 60f, 54f, 6f),
                new WreckKit.Vent(Shift + 128f, Ground - 26f, 30f, 3.5f),
                new WreckKit.Vent(Shift + 24f, 84f, 40f, 4f),
            };

            return WreckKit.Animate(c, vents, embers, FrameCount, 17, 6);
        }
    }
}
