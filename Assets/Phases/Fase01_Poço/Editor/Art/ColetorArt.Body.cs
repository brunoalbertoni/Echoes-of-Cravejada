using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class ColetorArt
    {
        // ------------------------------------------------------------------
        // As costas: a caldeira, a chamine e os pistoes
        // ------------------------------------------------------------------

        private static void DrawMotor(Frame f)
        {
            Pose pose = f.Pose;
            float bx = f.TX - 25f;
            float by = f.TY + 2f;

            // A chamine: sobe da caldeira, um pouco torta pra tras; na queda, dobrada.
            float topX = bx - 5f;
            float topY = f.Top - 20f;
            float midX = bx - 2f;
            float midY = (by + topY) * 0.5f;
            if (pose.ChimneyBent)
            {
                topX = bx - 18f;
                topY = f.Top - 4f;
            }

            ArtKit.Skin chimney = ArtKit.Corroded(ArtKit.Iron, 0.25f, 31, 0.3f, ArtKit.Rust);
            ArtKit.Limb(f.Canvas, bx, by - 8f, midX, midY, 3.4f, chimney, 0.05f);
            ArtKit.Limb(f.Canvas, midX, midY, topX, topY, 3.2f, chimney, 0.05f);
            ArtKit.Ball(f.Canvas, midX, midY, 3.8f, 2.2f, ArtKit.Iron, 0.2f);

            // O chapeu da chamine: aba larga.
            ArtKit.Drum(f.Canvas, topX, topY - 1f, 5.5f, 2.2f, 3f, ArtKit.Plain(ArtKit.Rust), 0.1f);

            // A caldeira: um cilindro de latao com azinhavre, cintas e o manometro.
            ArtKit.Drum(f.Canvas, bx, by, 9f, 14f, 2.6f, ArtKit.Corroded(ArtKit.Brass, 0.3f, 23, 0.22f), 0.1f);
            for (int band = -1; band <= 1; band += 2)
            {
                float y = by + band * 7f;
                for (float x = bx - 8.5f; x <= bx + 8.5f; x += 1f)
                {
                    f.Canvas.Set((int)x, (int)y, ArtKit.Iron[1]);
                    f.Canvas.Set((int)x, (int)y + 1, ArtKit.Iron[0]);
                }

                for (float x = bx - 6f; x <= bx + 6f; x += 4f) f.Canvas.Set((int)x, (int)y, ArtKit.Iron[3]);
            }

            // O manometro: mostrador creme, ponteiro.
            ArtKit.Ball(f.Canvas, bx - 3f, by - 1f, 3.4f, 3.4f, ArtKit.Brass, 0.3f);
            ArtKit.Ball(f.Canvas, bx - 3f, by - 1f, 2.3f, 2.3f, new[] { new Rgba(150, 140, 110), new Rgba(190, 180, 150), new Rgba(222, 214, 186), new Rgba(240, 236, 214) }, 0.4f);
            float needle = -140f + pose.Furnace * 120f;
            (float nx, float ny) = ArtKit.Along(bx - 3f, by - 1f, needle, 2f);
            ArtKit.Line(f.Canvas, (int)(bx - 3f), (int)(by - 1f), (int)nx, (int)ny, new Rgba(160, 40, 30));

            // Os dois pistoes que descem pro quadril de tras.
            float hipX = f.TX - 9f;
            float hipY = f.TY + 19f;
            for (int i = 0; i < 2; i++)
            {
                float sx = bx + 4f + i * 3f;
                float sy = by + 10f;
                ArtKit.Limb(f.Canvas, sx, sy, hipX - 2f + i * 3f, hipY - 2f, 1.6f, ArtKit.Iron, -0.1f);
                ArtKit.Limb(f.Canvas, sx, sy, (sx + hipX) * 0.5f, (sy + hipY) * 0.5f, 2.2f, ArtKit.Brass, 0f);
            }
        }

        // ------------------------------------------------------------------
        // O tambor-cacamba
        // ------------------------------------------------------------------

        private static void DrawBin(Frame f)
        {
            Pose pose = f.Pose;
            PixelCanvas c = f.Canvas;
            float tx = f.TX;
            float ty = f.TY;

            // A chapa: ferro enferrujado, com azinhavre escorrendo.
            ArtKit.Skin sheet = (x, y, light) =>
            {
                float n = ArtKit.Noise(x * 0.24f, y * 0.15f, 5);
                float drip = 0.18f + 0.4f * Math.Max(0f, (y + 10f) / 40f);
                if (n > 1f - drip * 0.4f) return ArtKit.Tone(ArtKit.Verdigris, light - 0.1f);
                if (ArtKit.Hash((int)Math.Floor(x), (int)Math.Floor(y), 8) % 17 == 0) return ArtKit.Tone(ArtKit.Rust, light - 0.35f);
                return ArtKit.Tone(ArtKit.Rust, light);
            };

            ArtKit.Drum(c, tx, ty, f.RX, f.RY, 3.2f, sheet, 0.05f);

            // As costuras verticais da chapa, com rebites.
            foreach (float u in new[] { -0.55f, 0.15f, 0.7f })
            {
                int x = (int)(tx + u * f.RX);
                for (int y = (int)(f.Top + 5f); y < (int)(ty + f.RY - 3f); y++)
                {
                    if (c.Get(x, y).A == 0) continue;
                    c.Set(x, y, ArtKit.Shift(ArtKit.Rust, ArtKit.Tone(ArtKit.Rust, -0.6f * u), -1));
                    if (y % 5 == 0) ArtKit.Rivet(c, x + 1, y, ArtKit.Iron);
                }
            }

            // As cintas de ferro, rebitadas.
            foreach (float v in new[] { -0.5f, 0.62f })
            {
                int y = (int)(ty + v * f.RY);
                float half = f.RX * (float)Math.Pow(1.0 - Math.Pow(Math.Abs(v), 3.2), 1.0 / 3.2);

                for (int x = (int)(tx - half) + 1; x <= (int)(tx + half) - 1; x++)
                {
                    float u = (x - tx) / f.RX;
                    float light = -0.75f * u + 0.2f;
                    c.Set(x, y - 1, ArtKit.Tone(ArtKit.Iron, light + 0.4f));
                    c.Set(x, y, ArtKit.Tone(ArtKit.Iron, light));
                    c.Set(x, y + 1, ArtKit.Tone(ArtKit.Iron, light - 0.5f));
                    if (x % 5 == 0) c.Set(x, y, ArtKit.Iron[3]);
                }
            }

            // Os remendos: um de cobre, atras (arrancado na queda e no rachado),
            // e um de lata amassada, na frente.
            bool patchGone = pose.PlateOff;
            DrawPatch(c, tx - 15f, ty - 7f, 11, 9, ArtKit.Copper, patchGone, f.Cracked);
            DrawPatch(c, tx + 5f, ty - 18f, 10, 7, ArtKit.Tin, false, false);

            // A fornalha na barriga: grade de ferro com o verde aceso atras.
            DrawFurnace(f, tx + 6f, ty + 4f);

            // A boca da cacamba e a sucata saindo dela.
            float mouthY = f.Top + 3f;
            for (int py = (int)(mouthY - 4f); py <= (int)(mouthY + 4f); py++)
            {
                for (int px = (int)(tx - f.RX); px <= (int)(tx + f.RX); px++)
                {
                    float dx = (px + 0.5f - tx) / (f.RX - 1f);
                    float dy = (py + 0.5f - mouthY) / 3.6f;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;
                    c.Set(px, py, d > 0.62f ? ArtKit.Tone(ArtKit.Iron, dy < 0 ? 0.5f : -0.2f) : ArtKit.Iron[0]);
                }
            }

            DrawJunk(f, mouthY);

            // A beirada da frente da boca, por cima da sucata.
            for (int px = (int)(tx - f.RX) + 1; px <= (int)(tx + f.RX) - 1; px++)
            {
                float dx = (px + 0.5f - tx) / (f.RX - 1f);
                if (dx * dx > 1f) continue;
                int py = (int)(mouthY + 3.6f * (float)Math.Sqrt(1f - dx * dx));
                c.Set(px, py, ArtKit.Tone(ArtKit.Iron, 0.3f - dx * 0.5f));
                c.Set(px, py + 1, ArtKit.Tone(ArtKit.Iron, 0.7f - dx * 0.6f));
            }

            if (f.Cracked)
            {
                ArtKit.Crack(c, tx + 14f, f.Top + 8f, 100f, 14, 3, 1f);
                ArtKit.Crack(c, tx - 4f, ty + 15f, 70f, 9, 9, 0.9f);
                ArtKit.Crack(c, tx + 18f, ty + 6f, 120f, 8, 12, 1f);

                // Verde escorrendo pela chapa.
                foreach (float u in new[] { -0.3f, 0.45f })
                {
                    int x = (int)(tx + u * f.RX);
                    int length = 5 + (int)(Math.Abs(u) * 8f);
                    for (int i = 0; i < length; i++) c.Set(x, (int)(f.Top + 6f) + i, i % 3 == 0 ? ArtKit.OxideDeep : ArtKit.OxideGlow);
                }
            }
        }

        private static void DrawPatch(PixelCanvas c, float x, float y, int w, int h, Rgba[] ramp, bool gone, bool cracked)
        {
            int x0 = (int)x;
            int y0 = (int)y;

            if (gone)
            {
                // O buraco: escuro por dentro, com fiacao e o verde la no fundo.
                for (int py = y0; py < y0 + h; py++)
                {
                    for (int px = x0; px < x0 + w; px++)
                    {
                        if (c.Get(px, py).A == 0) continue;
                        bool edge = px == x0 || py == y0 || px == x0 + w - 1 || py == y0 + h - 1;
                        float d = Math.Abs(px - (x0 + w * 0.5f)) / w + Math.Abs(py - (y0 + h * 0.5f)) / h;
                        c.Set(px, py, edge ? ArtKit.Rust[0] : (d < 0.35f ? ArtKit.OxideGlow : ArtKit.Iron[0]));
                    }
                }

                c.Set(x0 + w / 2, y0 + h / 2, ArtKit.OxideCore);
                ArtKit.Line(c, x0 + 1, y0 + 2, x0 + w - 3, y0 + h - 2, ArtKit.Copper[2]);
                return;
            }

            for (int py = y0; py < y0 + h; py++)
            {
                for (int px = x0; px < x0 + w; px++)
                {
                    if (c.Get(px, py).A == 0) continue;
                    float light = py == y0 ? 0.8f : (px == x0 ? 0.4f : (py == y0 + h - 1 || px == x0 + w - 1 ? -0.6f : 0.05f));

                    // Amassados: faixas diagonais um tom abaixo.
                    if ((px - py) % 7 == 0 && light > -0.5f && light < 0.5f) light -= 0.45f;
                    c.Set(px, py, ArtKit.Tone(ramp, light));
                }
            }

            ArtKit.Rivet(c, x0 + 1, y0 + 1, ArtKit.Iron);
            ArtKit.Rivet(c, x0 + w - 3, y0 + 1, ArtKit.Iron);
            ArtKit.Rivet(c, x0 + 1, y0 + h - 3, ArtKit.Iron);
            ArtKit.Rivet(c, x0 + w - 3, y0 + h - 3, ArtKit.Iron);

            if (cracked) ArtKit.Crack(c, x0 + w * 0.5f, y0 + 1, 80f, h - 2, 2, 0.9f);
        }

        private static void DrawFurnace(Frame f, float cx, float cy)
        {
            PixelCanvas c = f.Canvas;
            float glow = f.Pose.Furnace;
            const int w = 15;
            const int h = 11;
            int x0 = (int)(cx - w / 2f);
            int y0 = (int)(cy - h / 2f);

            for (int py = y0; py < y0 + h; py++)
            {
                for (int px = x0; px < x0 + w; px++)
                {
                    bool corner = (px == x0 || px == x0 + w - 1) && (py == y0 || py == y0 + h - 1);
                    if (corner) continue;

                    bool frame = px <= x0 + 1 || px >= x0 + w - 2 || py <= y0 + 1 || py >= y0 + h - 2;
                    if (frame)
                    {
                        float light = py <= y0 + 1 ? 0.5f : (px <= x0 + 1 ? 0.2f : -0.5f);
                        c.Set(px, py, ArtKit.Tone(ArtKit.Iron, light));
                        continue;
                    }

                    // Atras da grade: o fogo verde, mais forte embaixo.
                    float depth = (py - y0) / (float)h;
                    Rgba fire = glow < 0.3f
                        ? ArtKit.Iron[0]
                        : (depth > 0.6f && glow > 0.5f ? ArtKit.OxideCore : (depth > 0.3f ? ArtKit.OxideGlow : ArtKit.OxideDeep));
                    c.Set(px, py, fire);
                }
            }

            // As barras da grade.
            for (int bar = 0; bar < 3; bar++)
            {
                int x = x0 + 4 + bar * 3;
                for (int py = y0 + 2; py < y0 + h - 2; py++) c.Set(x, py, ArtKit.Iron[py == y0 + 2 ? 3 : 1]);
            }

            // A portinhola: dobradica do lado.
            c.Set(x0 - 1, y0 + 3, ArtKit.Iron[2]);
            c.Set(x0 - 1, y0 + h - 4, ArtKit.Iron[2]);
        }

        /// <summary>A sucata saindo da boca: um cano torto, meia engrenagem, uma lata, uma haste e um bracinho de autômato.</summary>
        private static void DrawJunk(Frame f, float mouthY)
        {
            PixelCanvas c = f.Canvas;
            float tx = f.TX;

            // O cano torto, pra tras.
            ArtKit.Limb(c, tx - 11f, mouthY, tx - 17f, mouthY - 11f, 2.2f, ArtKit.Tin, 0.1f);
            ArtKit.Ball(c, tx - 17f, mouthY - 12f, 2.6f, 2.2f, ArtKit.Tin, 0.3f);
            c.Set((int)(tx - 17f), (int)(mouthY - 12f), ArtKit.Iron[0]);

            // Meia engrenagem de latao.
            for (int py = (int)(mouthY - 9f); py <= (int)mouthY; py++)
            {
                for (int px = (int)(tx - 9f); px <= (int)(tx + 5f); px++)
                {
                    float dx = px + 0.5f - (tx - 2f);
                    float dy = py + 0.5f - (mouthY + 1f);
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    bool tooth = Math.Cos(Math.Atan2(dy, dx) * 9) > 0.3;
                    if (r > (tooth ? 7.6f : 6.2f) || r < 2f) continue;
                    c.Set(px, py, ArtKit.Tone(ArtKit.Brass, -(dx * 0.5f + dy * 0.7f) / Math.Max(r, 0.1f)));
                }
            }

            // Uma haste de ferro espetada.
            ArtKit.Limb(c, tx + 5f, mouthY, tx + 7f, mouthY - 13f, 1f, ArtKit.Iron, 0.3f);

            // Uma lata.
            ArtKit.Drum(c, tx + 10f, mouthY - 3f, 3.4f, 4f, 3f, ArtKit.Plain(ArtKit.Tin), 0.1f);
            c.Set((int)(tx + 9f), (int)(mouthY - 4f), new Rgba(170, 60, 50));
            c.Set((int)(tx + 10f), (int)(mouthY - 4f), new Rgba(170, 60, 50));

            // O bracinho de autômato, com a mao aberta pra cima.
            float ax = tx + 1f;
            float ay = mouthY - 2f;
            ArtKit.Limb(c, ax, ay, ax + 2f, ay - 9f, 1.4f, ArtKit.Copper, 0.1f);
            ArtKit.Ball(c, ax + 2f, ay - 10f, 1.8f, 1.6f, ArtKit.Copper, 0.2f);
            c.Set((int)(ax + 1f), (int)(ay - 13f), ArtKit.Copper[2]);
            c.Set((int)(ax + 2f), (int)(ay - 13f), ArtKit.Copper[3]);
            c.Set((int)(ax + 3f), (int)(ay - 13f), ArtKit.Copper[2]);
            c.Set((int)(ax + 4f), (int)(ay - 12f), ArtKit.Copper[1]);
        }

        // ------------------------------------------------------------------
        // Pernas
        // ------------------------------------------------------------------

        private static void DrawLeg(Frame f, bool back)
        {
            Pose pose = f.Pose;
            float hipX = f.TX + (back ? -9f : 8f);
            float hipY = f.TY + f.RY - 6f;

            float footX = TorsoX + pose.X * 0.4f + (back ? -11f + pose.BackFootX : 10f + pose.FrontFootX);
            float footY = GroundY - 3f - (back ? pose.BackLift : pose.FrontLift);

            float bias = back ? -0.4f : 0f;
            ArtKit.Skin metal = ArtKit.Corroded(ArtKit.Iron, 0.2f, back ? 61 : 62, 0.3f, ArtKit.Rust);

            (float kx, float ky) = ArtKit.Joint(hipX, hipY, footX, footY, 13f, 13f, -1);

            // Ajoelhado: o joelho de tras vai ao chao.
            if (back && pose.Kneel)
            {
                kx = hipX + 6f;
                ky = GroundY - 4f;
                footX = kx - 13f;
                footY = GroundY - 3f;
            }

            ArtKit.Limb(f.Canvas, hipX, hipY, kx, ky, 5.2f, metal, bias);
            ArtKit.Ball(f.Canvas, hipX, hipY, 5.5f, 5.5f, ArtKit.Iron, bias + 0.1f);

            // A canela: um pistao, cilindro largo em cima e haste brilhante embaixo.
            (float mx, float my) = (kx + (footX - kx) * 0.55f, ky + (footY - ky) * 0.55f);
            ArtKit.Limb(f.Canvas, kx, ky, mx, my, 4.4f, ArtKit.Corroded(ArtKit.Rust, 0.3f, back ? 63 : 64, 0.3f), bias);
            ArtKit.Limb(f.Canvas, mx, my, footX, footY, 2.6f, ArtKit.Tin, bias + 0.2f);
            ArtKit.Ball(f.Canvas, kx, ky, 4.6f, 4.6f, ArtKit.Brass, bias);
            ArtKit.Rivet(f.Canvas, (int)kx - 1, (int)ky - 1, ArtKit.Iron);

            // O pe: uma sapata pesada, com dois dedos de ferro na frente.
            float shoeX = footX + 3f;
            float shoeY = footY + 0.5f;
            ArtKit.Drum(f.Canvas, shoeX, shoeY, 9f, 3.4f, 3f, ArtKit.Plain(ArtKit.Iron), bias + 0.15f);
            f.Canvas.Fill((int)(shoeX - 8f), (int)(shoeY + 2f), 17, 1, ArtKit.Iron[0]);
            ArtKit.Taper(f.Canvas, shoeX + 7f, shoeY, shoeX + 12f, shoeY + 2.5f, 1.6f, 0.6f, ArtKit.Tin, bias);
            ArtKit.Taper(f.Canvas, shoeX + 5f, shoeY + 1f, shoeX + 9f, shoeY + 3f, 1.4f, 0.6f, ArtKit.Tin, bias - 0.2f);
        }
    }
}
