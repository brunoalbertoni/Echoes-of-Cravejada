using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A carcaca do Mestre das Caldeiras, no chao da arena depois que ele cai:
    /// a caldeira do corpo tombada pra frente num monte de chapas e canos, o
    /// elmo (a fenda do visor apagada) rolado pra esquerda, as duas chamines
    /// arrancadas e deitadas a direita, e o manometro do peito solto no chao,
    /// o vidro quebrado e o ponteiro torto. A fornalha ainda tem brasa: as
    /// frestas brilham e o vapor sai pelos rombos.
    ///
    /// Nasce do quadro em sobrepressao do proprio Mestre (BoilerMasterArt),
    /// cortado e remontado pelo WreckKit. 32 px por tile, mesmo chao do Mestre.
    /// </summary>
    public static class MasterWreckArt
    {
        public const int Width = 208;
        public const int Height = BoilerMasterArt.Height;
        public const int PixelsPerUnit = BoilerMasterArt.PixelsPerUnit;
        public const int FrameCount = 8;

        private const int Shift = (Width - BoilerMasterArt.Width) / 2;
        private const int Ground = Height - 2;

        /// <summary>O mesmo ponto do Mestre: o pivo dele, com a folha mais larga.</summary>
        public const float PivotX = (BoilerMasterArt.PivotX * BoilerMasterArt.Width + Shift) / Width;
        public const float PivotY = BoilerMasterArt.PivotY;

        private static readonly Rgba Cream = new Rgba(232, 222, 190);

        public static PixelCanvas[] Frames()
        {
            PixelCanvas source = BoilerMasterArt.Frames(true)[BoilerMasterArt.Death + 3];

            bool Head(int x, int y) => x >= 74 && x <= 104 && y >= 38 && y <= 70;
            bool Stacks(int x, int y) => x >= 42 && x <= 72 && y <= 50;

            PixelCanvas head = WreckKit.Cut(source, Head);
            PixelCanvas stacks = WreckKit.Cut(source, Stacks);
            PixelCanvas body = WreckKit.Cut(source, (x, y) => !Head(x, y) && !Stacks(x, y));

            var c = new PixelCanvas(Width, Height);
            Rgba[][] ramps = { ArtKit.Brass, ArtKit.Iron, ArtKit.Copper, ArtKit.Brass, ArtKit.Rust };

            WreckKit.Rubble(c, Shift + 26, Shift + 120, Ground, 14, ramps, 404);

            WreckKit.Place(c, stacks, 78f, 57f, 36f, Shift + 146f, Ground - 10f);
            WreckKit.Place(c, body, 15f, 72f, 124f, Shift + 70f, Ground + 3f);
            WreckKit.Place(c, head, -38f, 88f, 56f, Shift + 4f, Ground - 14f);

            WreckKit.Scorch(c, 13, 0.3f, 0.15f);

            var embers = new List<(int X, int Y)>();
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 52f, 78f, 15f, 24, 21));
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 70f, 64f, 95f, 20, 22));
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 48f, 100f, -20f, 18, 23));
            WreckKit.Hole(c, Shift + 62f, 70f, 5f, embers);
            WreckKit.Hole(c, Shift + 88f, 92f, 3.5f, embers);
            embers.AddRange(WreckKit.EmberCrack(c, Shift + 140f, Ground - 12f, 0f, 12, 24));

            WreckKit.Rubble(c, Shift + 10, Shift + 140, Ground, 6, ramps, 505);
            Gauge(c, Shift + 124f, Ground - 7f);
            WreckKit.Gear(c, Shift + 30f, Ground - 5f, 5f, ArtKit.Brass, 9, 0.5f);
            WreckKit.Pipe(c, Shift + 96f, Ground - 3f, Shift + 112f, Ground - 8f, 2f, ArtKit.Copper);
            WreckKit.Plate(c, Shift + 150f, Ground - 3f, 14f, 5f, -8f, ArtKit.Brass, 31);
            WreckKit.Scatter(c, 4, Width - 4, Ground, ramps, 606, 14);

            ArtKit.AddOutline(c);

            var steam = new Rgba(222, 222, 226);
            var steamDark = new Rgba(150, 150, 160);
            var vents = new[]
            {
                new WreckKit.Vent(Shift + 62f, 68f, 60f, 6f, steam, steamDark),
                new WreckKit.Vent(Shift + 88f, 90f, 44f, 4f),
                new WreckKit.Vent(Shift + 166f, Ground - 12f, 36f, 4f),
            };

            return WreckKit.Animate(c, vents, embers, FrameCount, 29, 6);
        }

        /// <summary>O manometro do peito, solto: aro de latao, mostrador rachado, ponteiro torto.</summary>
        private static void Gauge(PixelCanvas c, float cx, float cy)
        {
            ArtKit.Ball(c, cx, cy, 7f, 6f, ArtKit.Brass);
            ArtKit.Ball(c, cx - 0.5f, cy - 0.5f, 5f, 4.2f, ArtKit.Plain(new[] { new Rgba(170, 160, 132), new Rgba(204, 194, 162), Cream, Cream }));

            // A faixa vermelha e as marcas.
            for (int i = 0; i < 4; i++) c.Set((int)cx + 2 + i / 2, (int)cy - 3 + i, new Rgba(190, 50, 40));
            c.Set((int)cx - 4, (int)cy, ArtKit.Outline);
            c.Set((int)cx, (int)cy - 4, ArtKit.Outline);

            // O ponteiro, torto pra baixo, e o vidro rachado.
            ArtKit.Line(c, (int)cx, (int)cy, (int)cx - 3, (int)cy + 3, ArtKit.Outline);
            ArtKit.Line(c, (int)cx - 3, (int)cy - 3, (int)cx + 1, (int)cy - 1, new Rgba(250, 250, 255));
            ArtKit.Line(c, (int)cx + 1, (int)cy - 1, (int)cx + 3, (int)cy + 2, new Rgba(250, 250, 255));
            ArtKit.Rivet(c, (int)cx - 6, (int)cy - 1, ArtKit.Brass);
        }
    }
}
