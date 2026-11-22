using System;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class HeroArt
    {
        // ------------------------------------------------------------------
        // A capa
        // ------------------------------------------------------------------

        /// <summary>
        /// A parte de tras da capa: um sino de pano que desce do pescoco e
        /// passa por tras do corpo. Com vento (Flutter) a barra vai pra tras e
        /// sobe; a barra tem dentes esfarrapados que mudam com CapeWave.
        /// </summary>
        private static void DrawCapeBack(Frame f)
        {
            Pose pose = f.Pose;
            Rgba[] cloth = f.Look.Cloth;

            float neckX = f.BodyX - 0.5f;
            float neckY = f.BodyY - 6.5f;
            float hemY = FootY - 3f + Math.Min(0f, -pose.Flutter * 5f);
            float back = 8f + Math.Max(0f, pose.Flutter) * 8f;

            for (int py = (int)neckY; py <= (int)hemY + 2; py++)
            {
                float t = Math.Max(0f, Math.Min(1f, (py - neckY) / Math.Max(1f, hemY - neckY)));

                // A capa abre como um sino, e com vento vai mais pra tras embaixo.
                float left = neckX - 3f - back * (float)Math.Pow(t, 0.75);
                float right = neckX + 2f - t * 3f - Math.Max(0f, pose.Flutter) * t * 3f;

                for (int px = (int)left; px <= (int)right; px++)
                {
                    // Barra esfarrapada.
                    int tooth = Hash(px + pose.CapeWave * 3, 1, 9) % 4;
                    if (py > hemY - tooth + 1) continue;

                    float u = (px + 0.5f - left) / Math.Max(1f, right - left);
                    float fold = (float)Math.Sin((px + pose.CapeWave) * 0.9f) * 0.25f;
                    float light = (u - 0.6f) * 0.6f - t * 0.4f + fold - 0.1f;
                    f.Canvas.Set(px, py, Tone(cloth, light));
                }
            }
        }

        /// <summary>As abas da capa por cima dos ombros, caindo dos lados do corpo, e o broche.</summary>
        private static void DrawCapeFront(Frame f)
        {
            Rgba[] cloth = f.Look.Cloth;
            float cx = f.BodyX;
            float top = f.BodyY - 6.5f;

            // Aba de tras (a do ombro esquerdo): larga, cobre o lado do corpo.
            for (int py = (int)top; py <= (int)(f.BodyY + 1f); py++)
            {
                float t = (py - top) / 8f;
                int left = (int)(cx - 8f - t * 1.0f);
                int right = (int)(cx - 4f - t * 2.5f);

                for (int px = left; px <= right; px++)
                {
                    if (py > f.BodyY + 2f && Hash(px, py, 4) % 3 == 0) continue;
                    float light = (px - left) / 5f * 0.5f - 0.3f + (py == (int)top ? 0.5f : 0f);
                    f.Canvas.Set(px, py, Tone(cloth, light));
                }
            }

            // Aba da frente: so a beirada, no ombro direito.
            for (int py = (int)top; py <= (int)(f.BodyY - 1f); py++)
            {
                float t = (py - top) / 6f;
                int left = (int)(cx + 3f + t * 2f);
                int right = (int)(cx + 6f + t * 1.2f);

                for (int px = left; px <= right; px++)
                {
                    float light = 0.35f - (px - left) * 0.2f;
                    f.Canvas.Set(px, py, Tone(cloth, light));
                }
            }

            // A barra decorada do couro (so no polido).
            if (f.Look.Brooch)
            {
                for (int px = (int)(cx - 8f); px <= (int)(cx - 4f); px++) f.Canvas.Set(px, (int)(f.BodyY + 2f), f.Look.Trim);
            }
        }

        /// <summary>O no do lenco no pescoco, com as duas pontas caindo — ou o broche de engrenagem.</summary>
        private static void DrawScarf(Frame f)
        {
            Rgba[] cloth = f.Look.Cloth;
            int x = (int)(f.BodyX + 2f);
            int y = (int)(f.BodyY - 6f);

            // A faixa do pescoco.
            for (int px = x - 6; px <= x + 3; px++)
            {
                f.Canvas.Set(px, y, Tone(cloth, 0.4f - (x - px) * 0.05f));
                f.Canvas.Set(px, y + 1, Tone(cloth, -0.1f - (x - px) * 0.05f));
            }

            if (f.Look.Brooch)
            {
                // O broche redondo de metal, como uma engrenagem pequena.
                Ball(f.Canvas, x + 1.5f, y + 1.5f, 2.2f, 2.2f, f.Look.Metal, 0.2f);
                f.Canvas.Set(x + 1, y + 1, f.Look.Metal[0]);
                f.Canvas.Set(x + 1, y - 1, f.Look.Metal[2]);
                f.Canvas.Set(x + 4, y + 1, f.Look.Metal[1]);
                f.Canvas.Set(x - 1, y + 2, f.Look.Metal[1]);
                return;
            }

            // O no e as pontas.
            f.Canvas.Fill(x, y, 3, 3, cloth[2]);
            f.Canvas.Set(x + 1, y + 1, cloth[3]);
            f.Canvas.Set(x, y + 2, cloth[1]);
            for (int i = 0; i < 4; i++)
            {
                f.Canvas.Set(x - 1 + i / 2, y + 3 + i, i < 3 ? cloth[2] : cloth[1]);
                f.Canvas.Set(x + 2 + i / 3, y + 3 + i, i < 2 ? cloth[2] : cloth[1]);
            }

            f.Canvas.Set(x - 1, y + 7, cloth[0]);
            f.Canvas.Set(x + 4, y + 6, cloth[0]);
        }

        // ------------------------------------------------------------------
        // O corpo
        // ------------------------------------------------------------------

        private static void DrawLeg(Frame f, float footX, float lift, bool back)
        {
            Rgba[] metal = f.Look.Metal;
            float hipX = f.BodyX + (back ? -2.5f : 2.5f);
            float hipY = f.BodyY + 4f;
            float fx = CenterX + footX + f.Pose.BodyX * 0.3f;
            float fy = FootY - 1f - lift;

            float bias = back ? -0.45f : 0f;
            Limb(f.Canvas, hipX, hipY, fx, fy - 1f, 1.7f, metal, bias);

            // O pe: uma bota redonda, virada pra frente.
            Ball(f.Canvas, fx + 1f, fy, 3f, 1.9f, metal, bias - 0.05f);
            f.Canvas.Fill((int)(fx - 1f), (int)(fy + 1f), 5, 1, metal[0]);
        }

        /// <summary>O tronco: um barril de chapa, com cinta rebitada e uma placa no peito.</summary>
        private static void DrawBody(Frame f)
        {
            Rgba[] metal = f.Look.Metal;
            float cx = f.BodyX;
            float cy = f.BodyY;
            float rx = BodyRX / (float)Math.Sqrt(f.Pose.Squash);
            float ry = BodyRY * f.Pose.Squash;

            Ball(f.Canvas, cx, cy, rx, ry, metal, 0.05f);

            // A cinta, com rebites.
            int belt = (int)(cy + 2f);
            for (int px = (int)(cx - rx) + 1; px <= (int)(cx + rx) - 1; px++)
            {
                float dx = (px + 0.5f - cx) / rx;
                if (dx * dx > 0.9f) continue;

                f.Canvas.Set(px, belt, metal[0]);
                f.Canvas.Set(px, belt + 1, (px & 1) == 0 ? metal[2] : metal[1]);
            }

            // A placa do peito e o rebite do meio.
            for (int py = (int)(cy - 3f); py <= (int)(cy); py++)
            {
                f.Canvas.Set((int)(cx + 2f), py, metal[0]);
            }

            f.Canvas.Set((int)(cx + 4f), (int)(cy - 2f), metal[3]);
            f.Canvas.Set((int)(cx - 1f), (int)(cy - 3f), metal[3]);
            f.Canvas.Set((int)(cx + 4f), (int)(cy - 1f), metal[1]);

            // Ferrugem ou risco de uso.
            if (!f.Look.Crystal)
            {
                f.Canvas.Set((int)(cx - 3f), (int)(cy + 4f), new Rgba(76, 112, 92));
                f.Canvas.Set((int)(cx - 2f), (int)(cy + 4f), new Rgba(64, 96, 80));
            }
        }

        // ------------------------------------------------------------------
        // A cabeca
        // ------------------------------------------------------------------

        /// <summary>
        /// A cabeca: uma pedra bruta (ou um quartzo), facetada. Cada pixel pega
        /// o tom da faceta mais proxima, conforme ela olha pra luz; onde duas
        /// facetas se encontram fica um vinco. No quartzo, um brilho interno e
        /// faiscas.
        /// </summary>
        private static void DrawHead(Frame f)
        {
            Look look = f.Look;
            float cx = f.HeadX;
            float cy = f.HeadY;
            float rx = HeadRX;
            float ry = HeadRY * (0.5f + 0.5f * f.Pose.Squash);

            // Centros das facetas (relativos ao centro) e o "lado" que cada uma olha.
            float[,] facets =
            {
                { -5.5f, -5.0f, 0.9f }, { 0.5f, -6.5f, 0.75f }, { 6.0f, -4.0f, 0.35f },
                { -8.0f, 0.5f, 0.3f }, { -2.5f, -1.0f, 0.55f }, { 3.5f, -0.5f, 0.2f },
                { 8.0f, 1.5f, -0.05f }, { -5.0f, 5.5f, -0.1f }, { 1.0f, 5.0f, -0.15f }, { 6.5f, 6.0f, -0.35f },
            };

            for (int py = (int)(cy - ry) - 2; py <= (int)(cy + ry) + 2; py++)
            {
                for (int px = (int)(cx - rx) - 2; px <= (int)(cx + rx) + 2; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;

                    // Contorno de pedra: o raio varia com o angulo, achatado embaixo.
                    double angle = Math.Atan2(dy, dx);
                    float bump = 1f + 0.06f * (float)Math.Sin(angle * 5.0 + 1.1) + 0.04f * (float)Math.Sin(angle * 9.0 + 0.4);
                    float ex = dx / (rx * bump);
                    float ey = dy / (ry * bump * (dy > 0 ? 0.94f : 1f));
                    float d = ex * ex + ey * ey;
                    if (d > 1f) continue;

                    float nearest = 999f;
                    float second = 999f;
                    float facing = 0f;

                    for (int i = 0; i < facets.GetLength(0); i++)
                    {
                        float fx = dx - facets[i, 0];
                        float fy = dy - facets[i, 1];
                        float distance = fx * fx + fy * fy;

                        if (distance < nearest)
                        {
                            second = nearest;
                            nearest = distance;
                            facing = facets[i, 2];
                        }
                        else if (distance < second)
                        {
                            second = distance;
                        }
                    }

                    // A borda escurece (volume), as facetas dao o tom.
                    float light = facing * 1.25f - d * 0.4f + 0.05f;
                    Rgba color = Tone(look.Stone, light);

                    // Vinco entre facetas: um tom abaixo, nao preto.
                    bool crease = Math.Sqrt(second) - Math.Sqrt(nearest) < 0.6;
                    if (crease)
                    {
                        int index = Array.IndexOf(look.Stone, color);
                        color = look.Stone[Math.Max(0, index - 1)];
                    }

                    // A borda de baixo e de tras fica na sombra.
                    if (d > 0.9f && (dy > 4f || dx < -7f)) color = look.Stone[0];

                    int hx = (int)Math.Floor(dx + 16f);
                    int hy = (int)Math.Floor(dy + 16f);

                    if (look.Crystal)
                    {
                        // Brilho interno: o miolo do quartzo e mais claro.
                        if (d < 0.35f && !crease && Hash(hx, hy, 3) % 5 == 0) color = look.Stone[3];
                        if (Hash(hx, hy, 21) % 41 == 0) color = look.Fleck;
                    }
                    else
                    {
                        // Lasquinhas e grao da pedra.
                        if (Hash(hx, hy, 21) % 19 == 0) color = look.Fleck;
                        else if (Hash(hx, hy, 22) % 11 == 0) color = look.Stone[Math.Max(0, Array.IndexOf(look.Stone, color) - 1)];
                    }

                    f.Canvas.Set(px, py, color);
                }
            }

            // O brilho grande do quartzo, no alto a esquerda.
            if (look.Crystal)
            {
                f.Canvas.Set((int)(cx - 5f), (int)(cy - 6f), look.Fleck);
                f.Canvas.Set((int)(cx - 4f), (int)(cy - 6f), look.Fleck);
                f.Canvas.Set((int)(cx - 5f), (int)(cy - 5f), look.Fleck);
                f.Canvas.Set((int)(cx - 6f), (int)(cy - 4f), look.Stone[3]);
            }

            DrawEyes(f, cx, cy);
        }

        /// <summary>
        /// Os olhos, virados pra frente (pra direita). Na sucata, um redondo e
        /// um meio fechado — a cara de teimoso da referencia. No polido, os dois
        /// redondos e azuis. Em volta, um halo aceso na pedra.
        /// </summary>
        private static void DrawEyes(Frame f, float cx, float cy)
        {
            Look look = f.Look;
            Pose pose = f.Pose;

            int y = (int)(cy + 0.5f);
            int x0 = (int)(cx - 2.5f);
            int x1 = (int)(cx + 4.5f);

            // O de tras e redondo; o da frente, na sucata, meio fechado (a cara
            // de teimoso). No polido, os dois redondos.
            int open0 = pose.EyesShut ? 0 : (pose.Squint ? 3 : 5);
            int open1 = pose.EyesShut ? 0 : (pose.Squint || !look.RoundEyes ? 3 : 5);

            DrawEye(f, x0, y, open0, false);
            DrawEye(f, x1, y, open1, !look.RoundEyes || pose.Squint);
        }

        /// <summary>
        /// Um olho aceso: um oval de 4 de largura e h de altura, com miolo
        /// claro, borda de brilho e um halo na pedra em volta. Lidded corta o
        /// alto em diagonal, como uma palpebra caida. h 0 = fechado.
        /// </summary>
        private static void DrawEye(Frame f, int x, int y, int h, bool lidded)
        {
            Look look = f.Look;
            const int w = 4;

            // O halo, na pedra em volta.
            for (int py = y - 5; py <= y + 5; py++)
            {
                for (int px = x - 3; px <= x + w + 2; px++)
                {
                    float dx = (px + 0.5f - (x + w * 0.5f)) / 4.2f;
                    float dy = (py + 0.5f - (y + 0.5f)) / 4.4f;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;

                    Rgba under = f.Canvas.Get(px, py);
                    if (under.A == 0) continue;
                    f.Canvas.Set(px, py, Rgba.Lerp(under, look.EyeHalo, 0.5f * (1f - d)));
                }
            }

            if (h == 0)
            {
                f.Canvas.Fill(x - 1, y, w + 2, 1, look.Stone[0]);
                f.Canvas.Fill(x, y, w, 1, look.EyeGlow);
                return;
            }

            float cx = x + w * 0.5f;
            float cy = y + 0.5f + (lidded ? 0.5f : 0f);
            float ry = h * 0.5f;

            // Um contorno escuro fino, so em volta do oval.
            for (int py = y - 4; py <= y + 4; py++)
            {
                for (int px = x - 1; px <= x + w; px++)
                {
                    float dx = (px + 0.5f - cx) / (w * 0.5f + 0.9f);
                    float dy = (py + 0.5f - cy) / (ry + 0.9f);
                    if (dx * dx + dy * dy <= 1f) f.Canvas.Set(px, py, look.Stone[0]);
                }
            }

            for (int py = y - 4; py <= y + 4; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    float dx = (px + 0.5f - cx) / (w * 0.5f);
                    float dy = (py + 0.5f - cy) / ry;
                    float d = dx * dx + dy * dy;
                    if (d > 1.05f) continue;

                    // A palpebra: corta o alto do olho, mais do lado de dentro.
                    if (lidded && py < cy - ry * 0.1f + (px - x) * -0.35f) continue;

                    f.Canvas.Set(px, py, d < 0.45f ? look.EyeCore : look.EyeGlow);
                }
            }

            // O reflexo.
            if (!lidded) f.Canvas.Set(x + 1, (int)(cy - ry + 1f), look.EyeCore);
        }

        /// <summary>A mola: um espiral de quatro voltas, saindo do alto da cabeca, a direita.</summary>
        private static void DrawSpring(Frame f)
        {
            Rgba[] ramp = f.Look.Spring;
            float baseX = f.HeadX + 3.5f;
            float baseY = f.HeadY - HeadRY + 1.5f;
            float height = 9f * f.Pose.SpringStretch;
            const int turns = 4;

            int steps = 60;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                double a = t * turns * Math.PI * 2.0;

                // A ponta balanca mais que a base.
                float sway = f.Pose.SpringSway * t * t;
                float px = baseX + (float)Math.Cos(a) * 1.8f + sway;
                float py = baseY - t * height;

                bool front = Math.Sin(a) > 0;
                Rgba color = front ? (Math.Cos(a) < 0 ? ramp[3] : ramp[2]) : ramp[0];
                f.Canvas.Set((int)px, (int)py, color);
            }

            // O pino que prende a mola na pedra.
            f.Canvas.Set((int)baseX, (int)baseY + 1, ramp[1]);
            f.Canvas.Set((int)baseX + 1, (int)baseY + 1, ramp[0]);
        }

        // ------------------------------------------------------------------
        // Os bracos e o cinzel
        // ------------------------------------------------------------------

        private static (float X, float Y) Along(float x, float y, float angle, float distance)
        {
            double a = angle * Math.PI / 180.0;
            return (x + (float)Math.Cos(a) * distance, y + (float)Math.Sin(a) * distance);
        }

        /// <summary>
        /// O cinzel: o cabo de madeira, com a mao da frente em Grip e a de tras
        /// mais abaixo no cabo; a lamina larga, de ponta reta e chanfrada.
        /// </summary>
        private static void DrawWeapon(Frame f)
        {
            Look look = f.Look;
            float angle = f.Pose.Angle;

            (float bx, float by) = Along(f.GripX, f.GripY, angle, -9f);
            (float nx, float ny) = Along(f.GripX, f.GripY, angle, 11f);
            (float tx, float ty) = Along(f.GripX, f.GripY, angle, 21.5f);

            Limb(f.Canvas, bx, by, nx, ny, 1.05f, look.Wood);

            // A virola de metal entre o cabo e a lamina.
            (float fx, float fy) = Along(f.GripX, f.GripY, angle, 11.5f);
            Ball(f.Canvas, fx, fy, 1.8f, 1.8f, look.Metal, 0.1f);

            // A lamina: alarga da virola ate a ponta.
            float dx = tx - nx;
            float dy = ty - ny;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / length;
            float uy = dy / length;
            float perpX = -uy;
            float perpY = ux;

            for (int py = (int)Math.Min(ny, ty) - 6; py <= (int)Math.Max(ny, ty) + 6; py++)
            {
                for (int px = (int)Math.Min(nx, tx) - 6; px <= (int)Math.Max(nx, tx) + 6; px++)
                {
                    float rx = px + 0.5f - nx;
                    float ry = py + 0.5f - ny;
                    float along = (rx * ux + ry * uy) / length;
                    if (along < 0f || along > 1f) continue;

                    float across = rx * perpX + ry * perpY;
                    float half = 2.6f + along * 1.0f;
                    if (Math.Abs(across) > half) continue;

                    // Fio da lamina claro, costas escuras, chanfro na ponta.
                    float u = across / half;
                    float light = (perpY < 0 ? u : -u) * 0.7f + 0.1f;
                    if (along > 0.84f) light += 0.5f;

                    // A lombada: uma linha escura no meio da lamina.
                    if (Math.Abs(u) < 0.18f && along < 0.8f) light -= 0.6f;
                    Rgba color = Tone(look.Blade, light);

                    int hx = (int)Math.Floor(along * 10f);
                    int hy = (int)Math.Floor(u * 3f + 3f);
                    if (Hash(hx, hy, 31) % 7 == 0) color = look.BladeSpot;

                    f.Canvas.Set(px, py, color);
                }
            }

            // A mao de tras, no cabo.
            (float hx2, float hy2) = Along(f.GripX, f.GripY, angle, -5.5f);
            Ball(f.Canvas, hx2, hy2, 2.1f, 2.1f, look.Metal, -0.35f);
        }

        /// <summary>O braco de tras: do ombro ate a mao de tras, no cabo.</summary>
        private static void DrawBackArm(Frame f)
        {
            (float hx, float hy) = Along(f.GripX, f.GripY, f.Pose.Angle, -5.5f);
            Limb(f.Canvas, f.BodyX - 3f, f.BodyY - 3f, hx, hy, 1.4f, f.Look.Metal, -0.45f);
        }

        /// <summary>O braco da frente e a luva, por cima do cabo.</summary>
        private static void DrawFrontArm(Frame f)
        {
            Rgba[] metal = f.Look.Metal;
            float sx = f.BodyX + 4.5f;
            float sy = f.BodyY - 3f;

            Ball(f.Canvas, sx, sy, 2.2f, 2.2f, metal, 0.1f);
            Limb(f.Canvas, sx, sy, f.GripX, f.GripY, 1.5f, metal);
            Ball(f.Canvas, f.GripX, f.GripY, 2.4f, 2.4f, metal, 0.15f);

            // Os dedos fechados no cabo.
            f.Canvas.Set((int)(f.GripX + 1f), (int)(f.GripY + 1f), metal[0]);
            f.Canvas.Set((int)(f.GripX), (int)(f.GripY + 1f), metal[1]);
        }

        /// <summary>O rastro do golpe: um arco claro e translucido, do angulo inicial ao final.</summary>
        private static void DrawSmear(Frame f)
        {
            Pose pose = f.Pose;
            if (pose.SmearFrom == pose.SmearTo) return;

            float from = Math.Min(pose.SmearFrom, pose.SmearTo);
            float to = Math.Max(pose.SmearFrom, pose.SmearTo);
            bool newerAtHigh = pose.SmearTo > pose.SmearFrom;

            for (int py = 0; py < Size; py++)
            {
                for (int px = 0; px < Size; px++)
                {
                    float dx = px + 0.5f - f.GripX;
                    float dy = py + 0.5f - f.GripY;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (r < 10f || r > 21f) continue;

                    float a = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                    if (a < from || a > to) continue;

                    // Mais forte perto da lamina e na ponta; some no comeco do arco.
                    float k = (a - from) / Math.Max(1f, to - from);
                    if (!newerAtHigh) k = 1f - k;
                    float edge = (r - 10f) / 11f;
                    int alpha = (int)(40 + 150 * k * edge);
                    if (alpha < 30) continue;

                    Rgba glow = Rgba.Lerp(f.Look.Blade[3], new Rgba(255, 255, 255), 0.5f).WithAlpha(alpha);
                    f.Canvas.Blend(px, py, glow);
                }
            }
        }

        /// <summary>
        /// Contorna tudo de escuro. E o que faz o sprite ler bem em cima de
        /// qualquer fundo — e o fundo deste jogo e quase preto.
        /// </summary>
        private static void AddOutline(PixelCanvas canvas)
        {
            var filled = new bool[Size * Size];
            for (int i = 0; i < filled.Length; i++) filled[i] = canvas.Pixels[i].A > 128;

            for (int py = 0; py < Size; py++)
            {
                for (int px = 0; px < Size; px++)
                {
                    if (filled[px + py * Size]) continue;

                    bool touches = (px > 0 && filled[px - 1 + py * Size])
                        || (px < Size - 1 && filled[px + 1 + py * Size])
                        || (py > 0 && filled[px + (py - 1) * Size])
                        || (py < Size - 1 && filled[px + (py + 1) * Size]);

                    if (touches) canvas.Set(px, py, Outline);
                }
            }
        }
    }
}
