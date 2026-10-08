using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Uma cor de 8 bits por canal. Existe pra a arte gerada nao depender de UnityEngine.</summary>
    public readonly struct Rgba
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public Rgba(int r, int g, int b, int a = 255)
        {
            R = (byte)Clamp(r);
            G = (byte)Clamp(g);
            B = (byte)Clamp(b);
            A = (byte)Clamp(a);
        }

        public static readonly Rgba Clear = new Rgba(0, 0, 0, 0);

        public Rgba WithAlpha(int alpha) => new Rgba(R, G, B, alpha);

        /// <summary>Mistura duas cores. 0 devolve a primeira, 1 a segunda.</summary>
        public static Rgba Lerp(Rgba from, Rgba to, float amount)
        {
            amount = amount < 0f ? 0f : (amount > 1f ? 1f : amount);

            return new Rgba(
                (int)Math.Round(from.R + (to.R - from.R) * amount),
                (int)Math.Round(from.G + (to.G - from.G) * amount),
                (int)Math.Round(from.B + (to.B - from.B) * amount),
                (int)Math.Round(from.A + (to.A - from.A) * amount));
        }

        /// <summary>Escurece (fator menor que 1) ou clareia a cor, sem mexer no alfa.</summary>
        public Rgba Scaled(float factor)
        {
            return new Rgba(
                (int)Math.Round(R * factor),
                (int)Math.Round(G * factor),
                (int)Math.Round(B * factor),
                A);
        }

        private static int Clamp(int value) => value < 0 ? 0 : (value > 255 ? 255 : value);
    }

    /// <summary>
    /// Uma imagem em memoria pra desenhar pixel art por codigo.
    ///
    /// A origem e o canto de CIMA, a esquerda, e o y cresce pra baixo — igual a
    /// um editor de imagem. A Unity guarda textura de baixo pra cima; quem
    /// converte e o Phase02Assets, na hora de gravar.
    /// </summary>
    public sealed class PixelCanvas
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Rgba[] Pixels;

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Rgba[width * height];
        }

        public bool Inside(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public Rgba Get(int x, int y) => Inside(x, y) ? Pixels[x + y * Width] : Rgba.Clear;

        public void Set(int x, int y, Rgba color)
        {
            if (Inside(x, y)) Pixels[x + y * Width] = color;
        }

        /// <summary>Pinta por cima respeitando o alfa da cor nova.</summary>
        public void Blend(int x, int y, Rgba color)
        {
            if (!Inside(x, y) || color.A == 0) return;

            Rgba under = Pixels[x + y * Width];
            if (color.A == 255 || under.A == 0)
            {
                Pixels[x + y * Width] = color;
                return;
            }

            float top = color.A / 255f;
            float bottom = under.A / 255f * (1f - top);
            float alpha = top + bottom;

            Pixels[x + y * Width] = new Rgba(
                (int)Math.Round((color.R * top + under.R * bottom) / alpha),
                (int)Math.Round((color.G * top + under.G * bottom) / alpha),
                (int)Math.Round((color.B * top + under.B * bottom) / alpha),
                (int)Math.Round(alpha * 255f));
        }

        public void Fill(int x, int y, int width, int height, Rgba color)
        {
            for (int py = y; py < y + height; py++)
            {
                for (int px = x; px < x + width; px++)
                {
                    Set(px, py, color);
                }
            }
        }

        /// <summary>Copia outra imagem pra ca, respeitando a transparencia dela.</summary>
        public void Blit(PixelCanvas source, int x, int y)
        {
            for (int py = 0; py < source.Height; py++)
            {
                for (int px = 0; px < source.Width; px++)
                {
                    Blend(x + px, y + py, source.Pixels[px + py * source.Width]);
                }
            }
        }

        /// <summary>A mesma imagem girada um quarto de volta no sentido horario.</summary>
        public PixelCanvas RotatedClockwise()
        {
            var rotated = new PixelCanvas(Height, Width);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    rotated.Set(Height - 1 - y, x, Pixels[x + y * Width]);
                }
            }

            return rotated;
        }

        /// <summary>Os pixels em RGBA, linha a linha, de cima pra baixo.</summary>
        public byte[] ToBytes()
        {
            var bytes = new byte[Pixels.Length * 4];

            for (int i = 0; i < Pixels.Length; i++)
            {
                bytes[i * 4] = Pixels[i].R;
                bytes[i * 4 + 1] = Pixels[i].G;
                bytes[i * 4 + 2] = Pixels[i].B;
                bytes[i * 4 + 3] = Pixels[i].A;
            }

            return bytes;
        }

        /// <summary>Monta uma imagem a partir de RGBA, linha a linha, de cima pra baixo.</summary>
        public static PixelCanvas FromBytes(int width, int height, byte[] bytes)
        {
            var canvas = new PixelCanvas(width, height);

            for (int i = 0; i < canvas.Pixels.Length && i * 4 + 3 < bytes.Length; i++)
            {
                canvas.Pixels[i] = new Rgba(bytes[i * 4], bytes[i * 4 + 1], bytes[i * 4 + 2], bytes[i * 4 + 3]);
            }

            return canvas;
        }
    }
}
