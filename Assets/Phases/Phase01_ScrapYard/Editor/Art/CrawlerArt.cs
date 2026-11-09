using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O rastejante de engrenagem, no mesmo detalhe do Scrap: um bicho de
    /// relojoaria jogado no poco. O casco e uma engrenagem grande de cobre
    /// rebitada, a cabeca e um pinhao de ferro com duas pincas, nas costas
    /// ainda esta a chave de dar corda, e ele anda em seis patas de arame.
    ///
    /// O Oxide comeu o cobre: azinhavre escorrendo pra baixo, dois dentes
    /// partidos, uma rachadura que vaza verde e o olho aceso da mesma cor.
    ///
    /// Cada quadro tem 72x48, olha pra direita e e importado com 32 pixels
    /// por tile, como o Scrap. Os pes ficam na penultima linha; o pivo fica
    /// meio tile acima deles (ver PivotY), onde ficava o do desenho antigo.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class CrawlerArt
    {
        public const int Width = 72;
        public const int Height = 48;
        public const int PixelsPerUnit = 32;

        /// <summary>O pivo, em fracao da altura: meio tile (16 px) acima da sola (que fica 1 px acima da base).</summary>
        public const float PivotY = 17f / Height;

        // A ordem dos quadros. O CrawlerEnemy, no jogo, usa os mesmos numeros.
        public const int Walk = 0;
        public const int WalkFrames = 8;
        public const int Windup = 8;
        public const int WindupFrames = 2;
        public const int Bite = 10;
        public const int BiteFrames = 3;
        public const int Hurt = 13;
        public const int Death = 14;
        public const int DeathFrames = 6;
        public const int FrameCount = 20;

        private static readonly Rgba[] Steel = ArtKit.Tin;
        private static readonly Rgba[] SteelDark = ArtKit.Iron;

        private sealed class Pose
        {
            /// <summary>Deslocamento do corpo inteiro (positivo: pra frente e pra baixo).</summary>
            public float X;
            public float Y;

            /// <summary>Inclinacao do casco, em graus (negativo: a frente sobe).</summary>
            public float Tilt;

            public float HeadX;
            public float HeadY;

            /// <summary>Abertura das pincas: 0 fechadas, 1 escancaradas.</summary>
            public float Jaw = 0.15f;

            /// <summary>Os pes: 0-2 do lado de la (tras, meio, frente), 3-5 do lado de ca.</summary>
            public float[] FootX = new float[6];
            public float[] FootLift = new float[6];

            /// <summary>0 em pe; 1 as patas abertas e deitadas no chao.</summary>
            public float Buckle;

            /// <summary>A volta da chave (0 a 1) e a das varetas do miolo, em radianos.</summary>
            public float Key;
            public float Spin;

            /// <summary>O olho: 0 apagado, 1 aceso, 2 faiscando.</summary>
            public float Eye = 1f;
            public bool Squint;

            public float CrackGlow = 0.6f;
            public float Antenna;
            public bool Smear;
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();

            // A morte: a chave e a cabeca soltas, e as pecinhas no chao.
            public bool KeyLoose;
            public float KeyX;
            public float KeyY;
            public bool HeadLoose;
            public float HeadRot;
            public List<(float X, float Y, float R)> Cogs = new List<(float, float, float)>();
            public List<(float X, float Y, int Alpha)> Smoke = new List<(float, float, int)>();
        }

        /// <summary>Todos os quadros, na ordem das constantes.</summary>
        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            // Andando: tripe alternado, como inseto. Cada pe fica cinco quadros
            // no chao, empurrando pra tras, e tres no ar, voltando.
            for (int i = 0; i < WalkFrames; i++)
            {
                var pose = new Pose
                {
                    Y = (i % 4) < 2 ? 0f : -1f,
                    Key = i / (float)WalkFrames,
                    Spin = i * (float)Math.PI / 8f,
                    Antenna = (float)Math.Sin(i * Math.PI / 4.0),
                };

                for (int leg = 0; leg < 6; leg++)
                {
                    // Grupo A: tras e frente de la, meio de ca. Grupo B: os outros.
                    bool groupA = leg == 0 || leg == 2 || leg == 4;
                    int step = (i + (groupA ? 0 : 4)) % 8;
                    Step(step, out pose.FootX[leg], out pose.FootLift[leg]);
                }

                frames[Walk + i] = Draw(pose);
            }

            // O bote: ele recua, ergue a frente e escancara as pincas...
            frames[Windup + 0] = Draw(Planted(new Pose
            {
                X = -2f, Y = 1f, HeadY = -1f, Tilt = -5f, Jaw = 0.65f, Eye = 1.5f, Antenna = -1f, Key = 0.1f,
            }, 1f));
            frames[Windup + 1] = Draw(Planted(new Pose
            {
                X = -4f, Y = 1.5f, HeadY = -2f, Tilt = -10f, Jaw = 1f, Eye = 2f, Antenna = -1.5f, Key = 0.2f,
            }, 2f));

            // ...e da o bote, fechando as pincas com uma faisca.
            frames[Bite + 0] = Draw(Planted(new Pose
            {
                X = 3f, HeadX = 2f, Tilt = 4f, Jaw = 0.55f, Eye = 2f, Smear = true, Antenna = 1.5f, Key = 0.3f,
            }, -2f));
            Pose snap = Planted(new Pose
            {
                X = 5f, HeadX = 3f, HeadY = 1f, Tilt = 6f, Jaw = 0f, Eye = 2f, Antenna = 2f, Key = 0.35f,
            }, -3f);
            snap.Sparks.Add((66, 34, 2));
            snap.Sparks.Add((63, 38, 1));
            frames[Bite + 1] = Draw(snap);
            frames[Bite + 2] = Draw(Planted(new Pose
            {
                X = 3f, HeadX = 1f, Tilt = 2f, Jaw = 0.1f, Eye = 1.2f, Antenna = 1f, Key = 0.4f,
            }, -1f));

            // Apanhando: jogado pra tras, a frente pra cima, faiscas no casco.
            Pose hurt = Planted(new Pose
            {
                X = -3f, Y = -1f, HeadX = -1f, Tilt = -12f, Jaw = 0.35f, Eye = 0.6f, Squint = true, Antenna = -2f, Key = 0.05f,
                CrackGlow = 1f,
            }, 2f);
            hurt.Sparks.Add((30, 12, 2));
            hurt.Sparks.Add((24, 16, 1));
            frames[Hurt] = Draw(hurt);

            DrawDeath(frames);
            return frames;
        }

        /// <summary>Onde fica o pe num passo do ciclo de oito.</summary>
        private static void Step(int step, out float x, out float lift)
        {
            if (step < 5)
            {
                x = 3f - step * 1.5f;
                lift = 0f;
                return;
            }

            float[] swingX = { -1.5f, 1f, 2.5f };
            float[] swingLift = { 2f, 3f, 1.5f };
            x = swingX[step - 5];
            lift = swingLift[step - 5];
        }

        /// <summary>Os pes fincados no chao, contra o corpo que se mexe: so o corpo vai.</summary>
        private static Pose Planted(Pose pose, float brace)
        {
            for (int leg = 0; leg < 6; leg++)
            {
                int kind = leg % 3;
                pose.FootX[leg] = -pose.X * 0.6f + (kind == 0 ? -brace : (kind == 2 ? brace * 0.5f : 0f));
                pose.FootLift[leg] = 0f;
            }

            return pose;
        }

        /// <summary>
        /// A morte: um tranco que solta a chave, as patas cedem, o casco
        /// racha e tomba, a cabeca rola pra frente e as pecinhas se espalham.
        /// </summary>
        private static void DrawDeath(PixelCanvas[] frames)
        {
            // 0. O tranco: o olho estoura em branco, a chave salta.
            Pose jolt = Planted(new Pose
            {
                X = -1f, Y = -1f, Tilt = -8f, Jaw = 1f, Eye = 2f, CrackGlow = 1f, Antenna = -2f,
                KeyLoose = true, KeyX = 12f, KeyY = 12f, Key = 0.25f,
            }, 1f);
            jolt.Sparks.Add((28, 10, 2));
            jolt.Sparks.Add((40, 14, 1));
            jolt.Sparks.Add((14, 18, 1));
            frames[Death + 0] = Draw(jolt);

            // 1. As patas cedem; a chave no alto, girando.
            Pose sag = Planted(new Pose
            {
                X = -1f, Y = 3f, Tilt = 4f, HeadY = 1f, Jaw = 0.6f, Eye = 0.5f, CrackGlow = 1f, Buckle = 0.5f, Antenna = 1.5f,
                KeyLoose = true, KeyX = 8f, KeyY = 6f, Key = 0.5f,
            }, 0f);
            sag.Smoke.Add((32f, 10f, 90));
            sag.Sparks.Add((44, 20, 1));
            frames[Death + 1] = Draw(sag);

            // 2. Deitado no chao, a cabeca caida, verde subindo da rachadura.
            Pose down = Planted(new Pose
            {
                X = -1f, Y = 5f, Tilt = 10f, HeadY = 3f, Jaw = 0.4f, Eye = 0.4f, CrackGlow = 1f, Buckle = 1f, Antenna = 2.5f,
                KeyLoose = true, KeyX = 6f, KeyY = 38f, Key = 0.8f,
            }, 0f);
            down.Smoke.Add((31f, 8f, 110));
            down.Smoke.Add((35f, 3f, 70));
            down.Cogs.Add((20f, 43f, 2f));
            frames[Death + 2] = Draw(down);

            // 3. O casco tomba pra tras; a cabeca se solta e rola.
            Pose topple = Planted(new Pose
            {
                X = -3f, Y = 6.5f, Tilt = -22f, Jaw = 0.8f, Eye = 0.3f, CrackGlow = 0.8f, Buckle = 1f,
                HeadLoose = true, HeadX = 7f, HeadY = 10f, HeadRot = 35f,
                KeyLoose = true, KeyX = 6f, KeyY = 42f, Key = 0.95f,
            }, 0f);
            topple.Smoke.Add((30f, 12f, 80));
            topple.Cogs.Add((20f, 43f, 2f));
            topple.Cogs.Add((47f, 44f, 1.6f));
            frames[Death + 3] = Draw(topple);

            // 4. Tudo no chao, o olho quase apagado.
            Pose scattered = Planted(new Pose
            {
                X = -4f, Y = 7.5f, Tilt = -40f, Jaw = 1f, Eye = 0.15f, CrackGlow = 0.4f, Buckle = 1f,
                HeadLoose = true, HeadX = 10f, HeadY = 12f, HeadRot = 80f,
                KeyLoose = true, KeyX = 6f, KeyY = 42f, Key = 0f,
            }, 0f);
            scattered.Smoke.Add((29f, 16f, 50));
            scattered.Cogs.Add((19f, 43f, 2f));
            scattered.Cogs.Add((50f, 44f, 1.6f));
            scattered.Cogs.Add((40f, 45f, 1.3f));
            frames[Death + 4] = Draw(scattered);

            // 5. Apagado.
            Pose dead = Planted(new Pose
            {
                X = -4f, Y = 7.5f, Tilt = -40f, Jaw = 1f, Eye = 0f, CrackGlow = 0f, Buckle = 1f,
                HeadLoose = true, HeadX = 10f, HeadY = 12f, HeadRot = 80f,
                KeyLoose = true, KeyX = 6f, KeyY = 42f, Key = 0f,
            }, 0f);
            dead.Cogs.Add((19f, 43f, 2f));
            dead.Cogs.Add((50f, 44f, 1.6f));
            dead.Cogs.Add((40f, 45f, 1.3f));
            frames[Death + 5] = Draw(dead);
        }

        // ------------------------------------------------------------------
        // Montagem de um quadro
        // ------------------------------------------------------------------

        private const float ShellX = 32.5f;
        private const float ShellY = 24.5f;
        private const float ToothTip = 13.5f;
        private const float ToothRoot = 11.6f;
        private const int Teeth = 10;
        private const float HipY = 34f;
        private const float FootY = 45.5f;

        private static readonly float[] HipX = { 25f, 33f, 41f };
        private static readonly float[] FootBase = { -7f, 0f, 7f };

        private static PixelCanvas Draw(Pose pose)
        {
            var canvas = new PixelCanvas(Width, Height);

            float sx = ShellX + pose.X;
            float sy = ShellY + pose.Y;

            // A cabeca: presa na frente do casco, girando junto com ele; solta, por conta propria.
            double tilt = pose.Tilt * Math.PI / 180.0;
            float hx, hy;
            if (pose.HeadLoose)
            {
                // Solta, ela rola no chao: o centro fica um raio acima dos pes.
                hx = sx + 17f + pose.HeadX;
                hy = FootY - 6.8f;
            }
            else
            {
                float ox = 17f;
                float oy = 4f;
                hx = sx + ox * (float)Math.Cos(tilt) - oy * (float)Math.Sin(tilt) + pose.HeadX;
                hy = sy + ox * (float)Math.Sin(tilt) + oy * (float)Math.Cos(tilt) + pose.HeadY;
            }

            for (int leg = 0; leg < 3; leg++) DrawLeg(canvas, pose, leg, true);
            DrawChassis(canvas, pose, sx, sy);
            if (!pose.KeyLoose) DrawKey(canvas, pose, sx - 15f, sy - 3f, true);
            DrawShell(canvas, pose, sx, sy, tilt);
            for (int leg = 0; leg < 3; leg++) DrawLeg(canvas, pose, leg, false);
            DrawHead(canvas, pose, hx, hy);
            if (pose.KeyLoose) DrawKey(canvas, pose, pose.KeyX, pose.KeyY, false);

            foreach ((float X, float Y, float R) cog in pose.Cogs) DrawCog(canvas, cog.X, cog.Y, cog.R);

            ArtKit.AddOutline(canvas);

            // O que fica sem contorno: antenas de arame, rastro, faiscas, fumaca, brilho.
            if (!pose.HeadLoose) DrawAntennae(canvas, pose, hx, hy);
            DrawEyeGlow(canvas, pose, hx, hy);
            if (pose.Smear) DrawSmear(canvas, hx, hy);
            foreach ((float X, float Y, int Alpha) puff in pose.Smoke)
            {
                ArtKit.Puff(canvas, puff.X + pose.X, puff.Y, 3.5f, ArtKit.OxideCore, ArtKit.OxideGlow, puff.Alpha / 2);
                ArtKit.Puff(canvas, puff.X + pose.X + 3f, puff.Y + 2f, 2.5f, ArtKit.OxideCore, ArtKit.OxideGlow, puff.Alpha / 3);
            }

            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(canvas, spark.X, spark.Y, spark.Size);

            return canvas;
        }

        /// <summary>A barra de ferro embaixo do casco, onde as patas se prendem.</summary>
        private static void DrawChassis(PixelCanvas canvas, Pose pose, float sx, float sy)
        {
            float y = sy + 9.5f;
            ArtKit.Limb(canvas, sx - 9f, y, sx + 10f, y, 2.4f, SteelDark, 0.1f);

            for (int i = 0; i < 4; i++) ArtKit.Rivet(canvas, (int)(sx - 7f + i * 5f), (int)y - 1, Steel);
        }

        /// <summary>Uma pata de arame: do quadril sobe ate o joelho e desce pontuda ate o chao.</summary>
        private static void DrawLeg(PixelCanvas canvas, Pose pose, int kind, bool far)
        {
            int index = (far ? 0 : 3) + kind;

            float hipX = HipX[kind] + pose.X + (far ? 1.5f : 0f);
            float hipY = HipY + pose.Y + (far ? -1f : 0f);

            float footX = HipX[kind] + FootBase[kind] + pose.FootX[index] + (far ? 2f : 0f);
            float footY = FootY - pose.FootLift[index] - (far ? 1f : 0f);

            // Cedendo: os pes escorregam pra fora e o joelho deita.
            footX += FootBase[kind] * pose.Buckle * 0.9f + (kind == 1 ? (far ? 3f : -3f) * pose.Buckle : 0f);

            // Pata de tras dobra pra tras; as outras, pra frente.
            int bend = kind == 0 ? 1 : -1;
            (float kx, float ky) = ArtKit.Joint(hipX, hipY, footX, footY, 7.5f, 8.5f, bend);

            // Deitada, o joelho fica quase na altura do pe.
            ky += (footY - 2f - ky) * pose.Buckle * 0.7f;

            Rgba[] ramp = far ? SteelDark : Steel;
            float bias = far ? -0.35f : 0.05f;

            ArtKit.Limb(canvas, hipX, hipY, kx, ky, 1.2f, ramp, bias);
            ArtKit.Taper(canvas, kx, ky, footX, footY, 1.1f, 0.45f, ramp, bias + 0.15f);
            ArtKit.Ball(canvas, kx, ky, 1.5f, 1.5f, far ? ArtKit.Iron : ArtKit.Brass, bias - 0.1f);
        }

        /// <summary>
        /// O casco: a engrenagem de cobre, de frente pra quem olha. O aro
        /// rebitado com azinhavre escorrendo, o sulco, o miolo escuro com as
        /// varetas de latao girando, e o eixo.
        /// </summary>
        private static void DrawShell(PixelCanvas canvas, Pose pose, float sx, float sy, double tilt)
        {
            ArtKit.Skin rim = ArtKit.Corroded(ArtKit.Copper, 0f, 3);

            for (int py = (int)(sy - ToothTip) - 1; py <= (int)(sy + ToothTip) + 1; py++)
            {
                for (int px = (int)(sx - ToothTip) - 1; px <= (int)(sx + ToothTip) + 1; px++)
                {
                    float dx = px + 0.5f - sx;
                    float dy = py + 0.5f - sy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    double angle = Math.Atan2(dy, dx) - tilt;

                    // Coordenadas presas no casco (giram com ele).
                    float lx = r * (float)Math.Cos(angle);
                    float ly = r * (float)Math.Sin(angle);

                    bool tooth = Math.Cos(angle * Teeth) > 0.3;
                    int toothIndex = ((int)Math.Round(angle * Teeth / (2.0 * Math.PI)) % Teeth + Teeth) % Teeth;

                    float edge = tooth ? ToothTip : ToothRoot;

                    // Dois dentes partidos, em cima e atras.
                    if (tooth && toothIndex == 7) edge = ToothRoot;
                    if (tooth && toothIndex == 6) edge = ToothRoot + 0.9f;

                    if (r > edge) continue;

                    float light = -(dx * 0.55f + dy * 0.65f) / Math.Max(r, 0.01f);
                    Rgba color;

                    if (r < 2.3f)
                    {
                        // O eixo.
                        color = ArtKit.Tone(ArtKit.Brass, light * 0.6f + 0.5f - r * 0.2f);
                    }
                    else if (r < 7.6f)
                    {
                        // O miolo: o escuro de dentro, as varetas e um pinhao la no fundo.
                        double nearest = Math.Abs(Math.IEEERemainder(angle - pose.Spin, Math.PI / 2.0));
                        bool spoke = nearest * r < 1.1;

                        if (spoke) color = ArtKit.Tone(ArtKit.Brass, light * 0.5f + 0.1f);
                        else
                        {
                            bool cog = r < 5.2f && Math.Cos((angle + pose.Spin * 2.0) * 7) > 0.2 && r > 3.8f;
                            color = cog ? ArtKit.Iron[2] : (r > 6.6f ? ArtKit.Iron[0] : ArtKit.Iron[1]);
                        }
                    }
                    else if (r < 8.8f)
                    {
                        // O sulco: a borda de dentro do aro, na sombra em cima e com luz embaixo.
                        color = ArtKit.Tone(ArtKit.Copper, -light * 0.5f - 0.4f);
                    }
                    else
                    {
                        // O aro e os dentes: bisel claro em cima, escuro embaixo.
                        float bevel = r > ToothRoot - 0.6f ? light * 0.9f : light * 0.6f + 0.05f;
                        color = rim(lx, ly, bevel);

                        // O azinhavre escorre: mais denso embaixo (no casco, nao na tela).
                        float drip = ArtKit.Noise(lx * 0.35f, ly * 0.22f, 11);
                        float amount = 0.15f + 0.5f * Math.Max(0f, (ly + 4f) / 16f);
                        if (drip > 1f - amount && r <= ToothRoot + 0.2f) color = ArtKit.Tone(ArtKit.Verdigris, bevel - 0.1f);
                    }

                    canvas.Set(px, py, color);
                }
            }

            // Rebites no aro.
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4.0 + Math.PI / 8.0 + tilt;
                int rx = (int)Math.Floor(sx + Math.Cos(a) * 10.1);
                int ry = (int)Math.Floor(sy + Math.Sin(a) * 10.1);
                canvas.Set(rx, ry, ArtKit.Copper[3]);
                canvas.Set(rx + 1, ry + 1, ArtKit.Copper[0]);
            }

            // A rachadura, de cima e de tras pra dentro.
            double crackAngle = -115.0 * Math.PI / 180.0 + tilt;
            float cx = sx + (float)Math.Cos(crackAngle) * 12.5f;
            float cy = sy + (float)Math.Sin(crackAngle) * 12.5f;
            ArtKit.Crack(canvas, cx, cy, (float)(crackAngle * 180.0 / Math.PI) + 180f, 7, 5, pose.CrackGlow);
            if (pose.CrackGlow > 0.5f) ArtKit.Glow(canvas, cx + 2f, cy + 3f, 4f, ArtKit.OxideGlow, 0.35f * pose.CrackGlow);
        }

        /// <summary>A cabeca: um pinhao de ferro com o olho aceso e as duas pincas embaixo.</summary>
        private static void DrawHead(PixelCanvas canvas, Pose pose, float hx, float hy)
        {
            double rot = (pose.HeadLoose ? pose.HeadRot : pose.Tilt) * Math.PI / 180.0;

            // As pincas por tras da cabeca, saindo da frente de baixo.
            if (!pose.HeadLoose || pose.HeadRot < 60f) DrawMandibles(canvas, pose, hx, hy, rot);

            const float tip = 7.2f;
            const float root = 6.1f;

            for (int py = (int)(hy - tip) - 1; py <= (int)(hy + tip) + 1; py++)
            {
                for (int px = (int)(hx - tip) - 1; px <= (int)(hx + tip) + 1; px++)
                {
                    float dx = px + 0.5f - hx;
                    float dy = py + 0.5f - hy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    double angle = Math.Atan2(dy, dx) - rot;
                    bool tooth = Math.Cos(angle * 8) > 0.25;

                    if (r > (tooth ? tip : root)) continue;

                    // Domo: mais claro no alto, com bisel nos dentes.
                    float nz = (float)Math.Sqrt(Math.Max(0f, 1f - (r / tip) * (r / tip)));
                    float light = -0.55f * dx / tip - 0.65f * dy / tip + 0.5f * nz - 0.2f;
                    if (r > root - 0.5f) light -= 0.15f;

                    Rgba color = ArtKit.Tone(Steel, light);

                    // Uma faixa de ferrugem na borda de baixo.
                    float ly = r * (float)Math.Sin(angle);
                    if (ly > 3.5f && ArtKit.Noise(px * 0.4f, py * 0.4f, 17) > 0.5f) color = ArtKit.Tone(ArtKit.Rust, light);

                    canvas.Set(px, py, color);
                }
            }

            // O parafuso do meio.
            ArtKit.Rivet(canvas, (int)(hx - 1.5f), (int)(hy - 1.5f), ArtKit.Brass);

            // O olho: uma fresta na frente, virada com a cabeca.
            float ex = hx + (float)Math.Cos(rot) * 3.5f - (float)Math.Sin(rot) * -1.5f;
            float ey = hy + (float)Math.Sin(rot) * 3.5f + (float)Math.Cos(rot) * -1.5f;

            int eyeX = (int)Math.Floor(ex);
            int eyeY = (int)Math.Floor(ey);
            int height = pose.Squint ? 1 : 2;

            canvas.Fill(eyeX - 1, eyeY - 1, 5, height + 2, ArtKit.Outline);
            Rgba glow = pose.Eye <= 0.05f
                ? ArtKit.Iron[1]
                : (pose.Eye < 0.5f ? ArtKit.OxideDeep : ArtKit.OxideGlow);

            canvas.Fill(eyeX, eyeY, 3, height, glow);
            if (pose.Eye >= 1f) canvas.Set(eyeX + 2, eyeY, ArtKit.OxideCore);
            if (pose.Eye >= 1.8f) canvas.Fill(eyeX, eyeY, 3, 1, ArtKit.OxideCore);
        }

        /// <summary>As duas pincas: ganchos de ferro que abrem pra cima e pra baixo.</summary>
        private static void DrawMandibles(PixelCanvas canvas, Pose pose, float hx, float hy, double rot)
        {
            float deg = (float)(rot * 180.0 / Math.PI);
            (float bx, float by) = ArtKit.Along(hx, hy, deg + 35f, 5f);

            float spread = 8f + pose.Jaw * 34f;

            // De cima: sai pra frente e pra cima, a ponta curva pra baixo.
            float upper = deg - spread + 18f;
            (float ux, float uy) = ArtKit.Along(bx, by, upper, 5f);
            (float utx, float uty) = ArtKit.Along(ux, uy, upper + 70f, 3.5f);

            // De baixo: sai pra frente e pra baixo, a ponta curva pra cima.
            float lower = deg + spread + 18f;
            (float lx, float ly) = ArtKit.Along(bx, by, lower, 5f);
            (float ltx, float lty) = ArtKit.Along(lx, ly, lower - 70f, 3.5f);

            ArtKit.Taper(canvas, bx, by, lx, ly, 1.4f, 1.1f, SteelDark, -0.1f);
            ArtKit.Taper(canvas, lx, ly, ltx, lty, 1.1f, 0.4f, SteelDark, 0.1f);
            ArtKit.Taper(canvas, bx, by, ux, uy, 1.4f, 1.1f, Steel, 0.1f);
            ArtKit.Taper(canvas, ux, uy, utx, uty, 1.1f, 0.4f, Steel, 0.3f);
        }

        /// <summary>A chave de dar corda: a haste e a argola, que alarga e afina conforme gira.</summary>
        private static void DrawKey(PixelCanvas canvas, Pose pose, float kx, float ky, bool attached)
        {
            float turn = (float)Math.Abs(Math.Cos(pose.Key * Math.PI * 2.0));
            float rx = 0.9f + 3.1f * turn;

            if (attached) ArtKit.Limb(canvas, kx + 4.5f, ky, kx + 1f, ky, 1.1f, ArtKit.Brass, 0.1f);

            ArtKit.Ball(canvas, kx - rx * 0.4f, ky, rx, 4.6f, ArtKit.Brass, 0.15f);

            // A argola e vazada quando esta de frente.
            if (rx > 2.4f) canvas.Fill((int)(kx - rx * 0.4f) - 1, (int)ky - 1, 2, 3, Rgba.Clear);

            if (!attached)
            {
                // Solta: a haste aparece, curta, do lado.
                ArtKit.Limb(canvas, kx - rx * 0.4f + rx, ky, kx - rx * 0.4f + rx + 3f, ky + 1f, 0.9f, ArtKit.Brass, 0f);
            }
        }

        /// <summary>Uma engrenagenzinha solta no chao.</summary>
        private static void DrawCog(PixelCanvas canvas, float x, float y, float r)
        {
            for (int py = (int)(y - r) - 2; py <= (int)(y + r) + 2; py++)
            {
                for (int px = (int)(x - r) - 2; px <= (int)(x + r) + 2; px++)
                {
                    float dx = px + 0.5f - x;
                    float dy = py + 0.5f - y;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    bool tooth = Math.Cos(Math.Atan2(dy, dx) * 6) > 0.2;
                    if (d > (tooth ? r + 0.9f : r)) continue;
                    if (d < 0.7f) continue;

                    canvas.Set(px, py, ArtKit.Tone(ArtKit.Brass, -(dx + dy) / Math.Max(d, 0.1f) * 0.6f));
                }
            }
        }

        /// <summary>Duas antenas de arame com a pontinha acesa.</summary>
        private static void DrawAntennae(PixelCanvas canvas, Pose pose, float hx, float hy)
        {
            float sway = pose.Antenna;
            int bx = (int)Math.Floor(hx - 2f);
            int by = (int)Math.Floor(hy - 6f);

            int t1x = (int)Math.Round(hx + 4f + sway);
            int t1y = (int)Math.Round(hy - 13f + Math.Abs(sway) * 0.3f);
            ArtKit.Line(canvas, bx, by, t1x, t1y, Steel[2]);
            canvas.Set(t1x + 1, t1y, ArtKit.OxideGlow);
            canvas.Set(t1x + 1, t1y - 1, ArtKit.OxideCore.WithAlpha(160));

            int t2x = (int)Math.Round(hx + 7f + sway * 0.7f);
            int t2y = (int)Math.Round(hy - 10f);
            ArtKit.Line(canvas, bx + 2, by + 1, t2x, t2y, Steel[1]);
            canvas.Set(t2x + 1, t2y, ArtKit.Verdigris[3]);
        }

        /// <summary>O halo do olho aceso, por cima de tudo.</summary>
        private static void DrawEyeGlow(PixelCanvas canvas, Pose pose, float hx, float hy)
        {
            if (pose.Eye < 1.4f) return;

            double rot = (pose.HeadLoose ? pose.HeadRot : pose.Tilt) * Math.PI / 180.0;
            float ex = hx + (float)Math.Cos(rot) * 4.5f + (float)Math.Sin(rot) * 1.5f;
            float ey = hy + (float)Math.Sin(rot) * 4.5f - (float)Math.Cos(rot) * 1.5f + 0.5f;
            ArtKit.Glow(canvas, ex, ey, 5f, ArtKit.OxideGlow, 0.5f * (pose.Eye - 1f), true);
        }

        /// <summary>O rastro do bote: tres riscos claros na frente das pincas.</summary>
        private static void DrawSmear(PixelCanvas canvas, float hx, float hy)
        {
            var light = new Rgba(235, 255, 240);

            for (int i = 0; i < 3; i++)
            {
                int y = (int)(hy - 2f + i * 3f);
                int x0 = (int)(hx + 9f);
                for (int x = 0; x < 7 - i; x++)
                {
                    canvas.Blend(x0 + x, y + (i == 1 ? 0 : (i == 0 ? -x / 3 : x / 3)), light.WithAlpha(60 + x * 22));
                }
            }
        }
    }
}
