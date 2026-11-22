using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Onde fica cada peca da cenografia do District 1 (ver DecorArt).
    ///
    /// Coordenadas em tiles, do canto de baixo a esquerda do sprite (menos as
    /// engrenagens, que contam do eixo). Cada trecho do mapa conta uma parte
    /// da historia:
    ///
    ///   O POCO        — a Calha N.º 1 saindo do teto em cima do monte onde o
    ///                   Scrap acorda, as vigas e canos da obra da Coroa, os
    ///                   postes de lampiao da Avenida que desceram junto.
    ///   O ANDAR 1     — a oficina do Rebite: o cano do teto, os ganchos em
    ///                   cima dos espinhos.
    ///   O ANDAR 2 E A TRIAGEM — o alto do distrito, quase deserto: so a
    ///                   esteira parada e o
    ///                   que restou dos ganchos do Coletor.
    ///   A ANTESSALA   — o canto quente da bancada e o corredor dos que nao
    ///                   chegaram a ela.
    ///   A ARENA       — a toca do Coletor: as gaiolas, a contagem riscada na
    ///                   parede, as marcas de garra. Nada em volta das
    ///                   plataformas, pra luta ficar legivel.
    ///   O ABISMO      — os cabos partidos por cima do vao.
    ///   A OBRA        — a obra da Coroa, largada: andaime, viga, canos e o carrinho.
    ///
    /// Os canos sempre vao de rocha a rocha (ou ate uma curva que segue em
    /// frente): as pontas ficam enterradas no terreno, que e desenhado por
    /// cima. O Validate confere.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class Phase01Decor
    {
        public enum Layer
        {
            /// <summary>Presa na parede do fundo: tingida junto com ela.</summary>
            Wall,

            /// <summary>No chao, atras do terreno e do Scrap, um pouco esmaecida.</summary>
            Back,

            /// <summary>No chao, logo atras do terreno (o chao passa na frente), apagada.</summary>
            Middle,

            /// <summary>Antiga camada da frente: hoje vai pro fundo, como a Back.</summary>
            Front,
        }

        public enum Motion
        {
            None,

            /// <summary>Gira devagar em volta do eixo (as engrenagens da parede). Speed em graus por segundo.</summary>
            Spin,
        }

        public readonly struct Piece
        {
            public readonly string Sprite;
            public readonly float X;
            public readonly float Y;
            public readonly Layer Layer;
            public readonly bool Flip;

            /// <summary>Pras pecas que repetem: o tamanho, em tiles. Zero: o sprite do tamanho dele.</summary>
            public readonly float Width;
            public readonly float Height;

            public readonly Motion Motion;
            public readonly float Speed;

            /// <summary>E Oxide (poca, respiro): acende verde e some com a purificacao.</summary>
            public readonly bool Oxide;

            public Piece(string sprite, float x, float y, Layer layer, bool flip = false, float width = 0f, float height = 0f,
                Motion motion = Motion.None, float speed = 0f, bool oxide = false)
            {
                Sprite = sprite;
                X = x;
                Y = y;
                Layer = layer;
                Flip = flip;
                Width = width;
                Height = height;
                Motion = motion;
                Speed = speed;
                Oxide = oxide;
            }
        }

        /// <summary>
        /// Uma corrente pendurada do teto, com o que tiver na ponta (gancho,
        /// gaiola ou nada). Balanca devagar em volta do ponto de cima.
        /// </summary>
        public readonly struct Hanging
        {
            public readonly float X;
            public readonly float Top;
            public readonly float Length;
            public readonly string End;
            public readonly bool Front;

            public Hanging(float x, float top, float length, string end, bool front = false)
            {
                X = x;
                Top = top;
                Length = length;
                End = end;
                Front = front;
            }
        }

        /// <summary>
        /// Um poste de lampiao de cristal. Apagado, acende com a purificacao;
        /// os das bancadas ja estao acesos (sao a luz quente delas).
        /// </summary>
        public readonly struct Lamp
        {
            public readonly float X;
            public readonly float Y;
            public readonly bool AlwaysLit;

            public Lamp(float x, float y, bool alwaysLit = false)
            {
                X = x;
                Y = y;
                AlwaysLit = alwaysLit;
            }

            /// <summary>Onde fica o cristal, pra luz.</summary>
            public (float X, float Y) Glass => (X + 0.6f, Y + 2.3f);
        }

        public static readonly Piece[] Pieces = BuildPieces();

        public static readonly Hanging[] Hangings =
        {
            // Poco: um gancho perto da calha e uma gaiola em cima da subida.
            new Hanging(16.5f, 28f, 4f, DecorArt.Hook),
            new Hanging(28.8f, 28f, 4.5f, DecorArt.Cage),

            // Andar 1: ganchos em cima dos dois fossos de espinhos, e uma
            // gaiola vazia por cima do decreto.
            new Hanging(18.5f, 46f, 2.5f, DecorArt.Hook),
            new Hanging(27.5f, 46f, 1.5f, DecorArt.Hook),
            new Hanging(33.5f, 46f, 2f, DecorArt.Cage),

            // Andar 2: o comeco da fileira de ganchos que levava a sucata pra
            // Triagem.
            new Hanging(30.5f, 56f, 2f, DecorArt.Hook),
            new Hanging(45.5f, 56f, 3f, DecorArt.Cage),

            // A Triagem: a fileira de ganchos do Coletor, por cima da esteira
            // e dos fossos.
            new Hanging(50.5f, 56f, 2.5f, DecorArt.Hook),
            new Hanging(54.5f, 56f, 1.8f, DecorArt.Cage),
            new Hanging(61.5f, 56f, 3f, DecorArt.Hook),
            new Hanging(66.5f, 56f, 2f, DecorArt.Cage),
            new Hanging(72.0f, 56f, 2.5f, DecorArt.Hook),

            // A Antessala: em cima do fosso e no fundo do corredor.
            new Hanging(67.5f, 43f, 2f, DecorArt.Hook),
            new Hanging(84.5f, 43f, 2f, DecorArt.Cage),

            // A arena: as gaiolas da toca, de corrente curta, coladas no teto —
            // mais baixas elas passavam por cima das plataformas.
            new Hanging(57.5f, 32f, 1.2f, DecorArt.Cage),
            new Hanging(70.5f, 32f, 1.2f, DecorArt.Cage),

            // O abismo.
            new Hanging(90.5f, 31f, 3f, DecorArt.Hook),

            // A obra: o gancho do guincho do elevador, parado em cima dos ferros.
            new Hanging(55.0f, 15f, 3f, DecorArt.Hook),
        };

        public static readonly Lamp[] Lamps =
        {
            // Poco: os postes da Avenida que desceram pela calha.
            new Lamp(16.0f, 5f),
            new Lamp(26.3f, 5f),
            new Lamp(9.1f, 4f),
            new Lamp(36.2f, 22f),

            // A bancada do Andar 1: aceso, e a luz dela.
            new Lamp(4.05f, 40f, true),

            // Os andares e a Triagem: postes que os descartados arrastaram la
            // pra cima, pra ter alguma luz longe do Coletor.
            new Lamp(16.1f, 40f),
            new Lamp(87.8f, 50f),
            new Lamp(70.2f, 37f),

            // A arena: acendem quando ele cai. Ficam nos vaos entre as
            // plataformas (e fora da coluna do vapor da volta, 53 a 55), e nao
            // embaixo delas.
            new Lamp(49.0f, 20f),
            new Lamp(71.4f, 20f),
        };

        private static Piece[] BuildPieces()
        {
            var p = new List<Piece>();

            // Na parede. Os montes de sucata postos aqui viram fundo apagado (ver
            // Phase01DecorBuilder): e onde ficam os que estao no caminho de um
            // inimigo, pra ele nao sumir na frente do monte.
            void Wall(string sprite, float x, float y, bool flip = false) => p.Add(new Piece(sprite, x, y, Layer.Wall, flip));
            void Back(string sprite, float x, float y, bool flip = false) => p.Add(new Piece(sprite, x, y, Layer.Back, flip));
            void Mid(string sprite, float x, float y, bool flip = false) => p.Add(new Piece(sprite, x, y, Layer.Middle, flip));

            void GirderV(float x, float y0, float y1) => p.Add(new Piece(DecorArt.GirderV, x, y0, Layer.Wall, width: 1f, height: y1 - y0));
            void GirderH(float x0, float x1, float y) => p.Add(new Piece(DecorArt.GirderH, x0, y, Layer.Wall, width: x1 - x0, height: 1f));
            void PipeV(float x, float y0, float y1) => p.Add(new Piece(DecorArt.PipeV, x, y0, Layer.Wall, width: 1f, height: y1 - y0));
            void PipeH(float x0, float x1, float y) => p.Add(new Piece(DecorArt.PipeH, x0, y, Layer.Wall, width: x1 - x0, height: 1f));
            void Conveyor(float x0, float x1, float y) => p.Add(new Piece(DecorArt.Conveyor, x0, y, Layer.Wall, width: x1 - x0, height: 1f));
            void Gear(string sprite, float cx, float cy, float speed) =>
                p.Add(new Piece(sprite, cx, cy, Layer.Wall, motion: Motion.Spin, speed: speed));

            // As pocas ficam um quarto de tile pra dentro do chao, pra assentar na beirada.
            void Puddle(float x, float y, bool small = false) =>
                p.Add(new Piece(small ? DecorArt.PuddleSmall : DecorArt.Puddle, x, y - 0.25f, Layer.Middle, oxide: true));
            void Vent(float x, float y) => p.Add(new Piece(DecorArt.Vent, x, y, Layer.Wall, oxide: true));

            // ------------------------------------------------------------------
            // O poco: o fundo da calha, onde tudo cai. E o trecho mais cheio do
            // distrito: sucata, descartados que nao levantaram, postes da
            // Avenida e as pecas soltas de quem caiu.
            // ------------------------------------------------------------------

            // A Calha N.º 1, saindo do teto bem em cima do monte onde o Scrap acorda.
            Wall(DecorArt.Chute, 19.5f, 25.2f);

            // A obra: uma coluna de viga e o cano que corre pela parede oeste,
            // do chao da saliencia ate o teto.
            GirderV(14.2f, 16f, 26f);
            PipeV(4f, 13.5f, 19.5f);

            // As engrenagens presas na rocha.
            Gear(DecorArt.GearBig, 32f, 25f, 5f);
            Gear(DecorArt.GearMid, 6.5f, 25.5f, -9f);
            Gear(DecorArt.GearMid, 37.5f, 13.5f, 7f);

            // Um cartaz, a seta pra chamine, o cabo caido, os respiros.
            Wall(DecorArt.PosterCrown, 9.5f, 17.5f);
            Wall(DecorArt.ArrowUp, 35.0f, 23.5f);
            Wall(DecorArt.Cable, 10.9f, 23.9f);
            Vent(18f, 13.5f);
            Vent(34.5f, 18f);
            Vent(27.5f, 14.5f);

            // As lanternas de servico da Coroa, pregadas na rocha quando a
            // calha foi aberta. Apagadas: ninguem desce pra trocar.
            Wall(DecorArt.WallLamp, 24.6f, 12.4f);
            Wall(DecorArt.WallLamp, 7.6f, 20.6f);
            Wall(DecorArt.WallLamp, 30.2f, 21.0f);
            Wall(DecorArt.Hazard, 28.6f, 16.2f);
            Wall(DecorArt.Tally, 6.4f, 8.2f);

            // Montanhas de sucata no fundo, apagadas na parede: o poco e mais fundo do que parece.
            Wall(DecorArt.Heap(2, 1), 27.6f, 4f);
            Wall(DecorArt.Heap(1, 2), 35.4f, 6f);

            // O monte de sucata embaixo da calha (em cima do degrau onde o
            // Scrap acorda), e mais lixo no chao.
            Back(DecorArt.Heap(1, 0), 18.2f, 6f);
            Back(DecorArt.Heap(1, 1), 22.6f, 6f, true);
            Back(DecorArt.Heap(0, 2), 9.6f, 4f);
            Back(DecorArt.Wheel, 28.1f, 4f);
            Back(DecorArt.PipePile, 29.5f, 4f);
            Back(DecorArt.Barrel, 33.4f, 4f);
            Back(DecorArt.Carcass(1), 4.05f, 14f);
            Back(DecorArt.Carcass(0), 35.1f, 6f);
            Back(DecorArt.Heap(0, 0), 20.2f, 18f);
            Back(DecorArt.Barrel, 12.2f, 12f);
            Mid(DecorArt.LoosePart(2), 39.2f, 6f);
            Mid(DecorArt.LoosePart(0), 13.6f, 16f);
            Mid(DecorArt.LoosePart(1), 27.4f, 20f, true);
            Mid(DecorArt.Bolts, 18.4f, 6f);
            Mid(DecorArt.Bolts, 5.6f, 14f);
            Mid(DecorArt.Bolts, 28.5f, 8f);
            Puddle(11.8f, 4f, true);
            Puddle(33.0f, 4f, true);
            Puddle(21.2f, 10f, true);
            Puddle(42.3f, 6f);

            // ------------------------------------------------------------------
            // Andar 1: a oficina do Rebite
            // ------------------------------------------------------------------

            // O cano do teto: sai da parede oeste, corre rente ao teto e, na
            // ponta leste, faz a curva e desce pela chamine ate o patamar do topo.
            PipeH(8.5f, 37.2f, 44.8f);
            Wall(DecorArt.PipeFlange, 16.5f, 44.8f);
            Wall(DecorArt.PipeFlange, 21.5f, 44.8f);
            Wall(DecorArt.PipeFlange, 31.5f, 44.8f);
            Wall(DecorArt.PipeElbowDownLeft, 37.2f, 44.8f);
            PipeV(37.2f, 21.5f, 44.8f);
            Vent(21.5f, 42.5f);

            // O quadro de ferramentas do mestre, pregado perto da oficina:
            // tres contornos vazios, o que o Rebite pede pra achar.
            Wall(DecorArt.ToolRack, 14.4f, 42.9f);
            Wall(DecorArt.PosterCrown, 23.0f, 41.6f);
            Gear(DecorArt.GearMid, 27.0f, 43.0f, -6f);
            Wall(DecorArt.Heap(1, 0), 20.3f, 40f);
            Wall(DecorArt.Carcass(2), 13.6f, 40f);
            Back(DecorArt.BarrelTipped, 29.0f, 40f);
            Mid(DecorArt.Bolts, 30.4f, 40f);
            Puddle(23.8f, 40f, true);

            // ------------------------------------------------------------------
            // Andar 2: o canto do Ferrolho e o caminho da sucata pra Triagem
            // ------------------------------------------------------------------

            // A caldeira furada que o Ferrolho (mecanico de caldeira) desmonta
            // pra tirar peca.
            Back(DecorArt.BoilerWreck, 8.2f, 50f);
            Wall(DecorArt.Heap(1, 2), 26.0f, 50f);
            Wall(DecorArt.PosterDecree, 29.0f, 52.2f);
            Gear(DecorArt.GearMid, 38.5f, 53.3f, 8f);
            Wall(DecorArt.Carcass(1), 43.0f, 50f, true);

            // A Triagem: a esteira parada, os cestos de "boa" e "ruim" no fim
            // dela e o que o Coletor separou e nunca subiu.
            Conveyor(46f, 58.4f, 50f);
            Wall(DecorArt.ConveyorMotor, 58.2f, 50f);
            Back(DecorArt.SortingBins, 48.6f, 50f);
            Mid(DecorArt.LoosePart(1), 52.6f, 50f);
            Mid(DecorArt.Bolts, 56.6f, 50f);
            Wall(DecorArt.Heap(2, 0), 80.6f, 50f);
            Wall(DecorArt.Carcass(0), 84.0f, 50f);
            Wall(DecorArt.WallLamp, 18.6f, 53.0f);
            Wall(DecorArt.WallLamp, 62.0f, 52.6f);
            Wall(DecorArt.PosterCrown, 63.4f, 52.0f);
            Vent(90.0f, 53.0f);

            // ------------------------------------------------------------------
            // A Antessala: o canto quente da bancada e o corredor dos que nao
            // chegaram a ela
            // ------------------------------------------------------------------

            // O cano sai da parede oeste, corre rente ao teto e, antes do poco
            // que desce da Triagem, faz a curva e entra no teto.
            PipeH(48.5f, 88.8f, 41.8f);
            Wall(DecorArt.PipeFlange, 63.5f, 41.8f);
            Wall(DecorArt.PipeFlange, 78.5f, 41.8f);
            Wall(DecorArt.PipeElbowUpLeft, 88.8f, 41.8f);
            PipeV(88.8f, 42.8f, 43.5f);
            Wall(DecorArt.PosterDecree, 88.6f, 38.2f);
            Vent(74f, 39.3f);
            Back(DecorArt.Carcass(1), 62.6f, 37f);
            Wall(DecorArt.Heap(1, 2), 75.8f, 37f);
            Wall(DecorArt.Carcass(2), 79.6f, 37f);
            Back(DecorArt.BarrelTipped, 88.0f, 37f);
            Back(DecorArt.Carcass(0), 91.0f, 37f, true);
            Back(DecorArt.Carcass(0), 93.3f, 37f, true);
            Mid(DecorArt.Bolts, 71.5f, 37f);
            Puddle(83.4f, 37f, true);
            Puddle(78.6f, 37f);

            // ------------------------------------------------------------------
            // A arena: a toca do Coletor
            // ------------------------------------------------------------------

            // Nada em volta das plataformas: o que enche a toca fica na parede
            // (montes apagados, a contagem, as marcas de garra). No chao, so um
            // monte baixo no canto leste, longe da beirada do degrau.
            Gear(DecorArt.GearBig, 49.6f, 30.2f, -3f);
            Gear(DecorArt.GearMid, 77.2f, 28.6f, 6f);
            Wall(DecorArt.ClawMarks, 53.5f, 26.3f);
            Wall(DecorArt.ClawMarks, 66.2f, 22.6f);
            Wall(DecorArt.Tally, 48.3f, 24.0f);
            Wall(DecorArt.Tally, 48.5f, 25.1f);
            Wall(DecorArt.Heap(2, 2), 64.4f, 20f);
            Back(DecorArt.Heap(0, 0), 75.8f, 20f, true);
            Back(DecorArt.Carcass(2), 58.4f, 20f);
            Mid(DecorArt.Bolts, 65.8f, 20f);
            Puddle(60.2f, 20f);
            Puddle(66.4f, 20f, true);

            // ------------------------------------------------------------------
            // O abismo
            // ------------------------------------------------------------------

            Wall(DecorArt.Cable, 85.4f, 29.55f);
            Wall(DecorArt.Cable, 91.4f, 29.55f, true);
            Wall(DecorArt.Hazard, 85.8f, 24.2f);
            Wall(DecorArt.ArrowUp, 102.2f, 24.5f);
            Wall(DecorArt.WallLamp, 86.6f, 26.0f);
            Wall(DecorArt.WallLamp, 95.6f, 25.4f);
            Wall(DecorArt.Tally, 93.0f, 27.0f);
            Gear(DecorArt.GearMid, 102.6f, 28f, -8f);
            Back(DecorArt.Carcass(2), 83.2f, 22f);
            Mid(DecorArt.LoosePart(2), 85.6f, 22f, true);

            // O poco que sobe pro District 2: a viga e o cano da obra, do chao
            // da beirada de la ate o teto do poco.
            GirderV(98f, 22f, 49f);
            PipeV(100.6f, 21.5f, 49.5f);

            // ------------------------------------------------------------------
            // A obra abandonada: o Elevador de Carga N.º 2, que ia levar a
            // sucata boa de volta. O material que a Coroa trouxe e largou no dia
            // em que o verde desceu: as torres de viga do elevador em pe sobre
            // os ferros, o guincho parado, o andaime, os canos, as caixas de
            // peca nova (com o 925) e o carrinho da carga.
            // ------------------------------------------------------------------

            Back(DecorArt.PipePile, 43.1f, 6f);
            Wall(DecorArt.Hazard, 46.5f, 11f);
            GirderV(50.2f, 5f, 15f);
            GirderV(57.5f, 6f, 15f);
            GirderH(50.2f, 58.5f, 13f);
            Back(DecorArt.Scaffold, 52.5f, 5f);
            Back(DecorArt.Barrel, 59.1f, 7f);
            Back(DecorArt.Crate, 60.3f, 7f);
            Back(DecorArt.CrateOpen, 61.9f, 7f);
            Back(DecorArt.Crate, 67.3f, 13f);
            Back(DecorArt.Cart, 76.0f, 13f, true);
            Wall(DecorArt.WallLamp, 44.4f, 10.4f);
            Wall(DecorArt.PosterCrown, 63.4f, 10.2f);
            Wall(DecorArt.Hazard, 77.0f, 15.6f);

            return p.ToArray();
        }

        /// <summary>
        /// Confere que nenhum cano termina no ar: cada ponta tem que estar
        /// enterrada na rocha ou encostada numa curva, e cada curva tem que
        /// ligar dois canos.
        /// </summary>
        public static List<string> Validate()
        {
            var problems = new List<string>();
            var elbows = new List<Piece>();
            foreach (Piece piece in Pieces)
            {
                if (piece.Sprite.StartsWith("d1_pipe_elbow")) elbows.Add(piece);
            }

            var linked = new int[elbows.Count];

            // Um quarto de tile pra dentro da ponta tem que ser rocha: e o que
            // garante que o terreno, desenhado por cima, esconde o corte do cano.
            bool Buried(float x, float y) => Phase01Layout.IsRock((int)Math.Floor(x), (int)Math.Floor(y));

            bool AtElbow(float x, float y)
            {
                for (int i = 0; i < elbows.Count; i++)
                {
                    Piece elbow = elbows[i];
                    if (x >= elbow.X - 0.05f && x <= elbow.X + 1.05f && y >= elbow.Y - 0.05f && y <= elbow.Y + 1.05f)
                    {
                        linked[i]++;
                        return true;
                    }
                }

                return false;
            }

            void CheckEnd(Piece pipe, float x, float y, float inwardX, float inwardY)
            {
                if (AtElbow(x, y) || Buried(x + inwardX * 0.25f, y + inwardY * 0.25f)) return;
                problems.Add($"O cano em ({pipe.X}, {pipe.Y}) termina no ar em ({x}, {y}): enterre a ponta na rocha ou ligue numa curva.");
            }

            foreach (Piece piece in Pieces)
            {
                if (piece.Sprite == DecorArt.PipeV)
                {
                    float x = piece.X + 0.5f;
                    CheckEnd(piece, x, piece.Y, 0f, 1f);
                    CheckEnd(piece, x, piece.Y + piece.Height, 0f, -1f);
                }
                else if (piece.Sprite == DecorArt.PipeH)
                {
                    float y = piece.Y + 0.5f;
                    CheckEnd(piece, piece.X, y, 1f, 0f);
                    CheckEnd(piece, piece.X + piece.Width, y, -1f, 0f);
                }
            }

            for (int i = 0; i < elbows.Count; i++)
            {
                if (linked[i] < 2)
                {
                    problems.Add($"A curva de cano em ({elbows[i].X}, {elbows[i].Y}) so liga {linked[i]} cano(s): sobra uma boca no ar.");
                }
            }

            CheckPhysics(problems);
            return problems;
        }

        /// <summary>
        /// A fisica de quem esta parado: o que fica no chao (Back e Middle)
        /// tem que ter apoio embaixo de toda a base e nao pode entrar na rocha;
        /// os postes, o mesmo; as correntes tem que sair do teto e descer por
        /// ar livre, sem atravessar plataforma nem chao.
        /// </summary>
        private static void CheckPhysics(List<string> problems)
        {
            Dictionary<string, PixelCanvas> art = DecorArt.All();
            const int ppu = DecorArt.PixelsPerUnit;

            bool Solid(float x, float y) => Phase01Layout.IsSolid((int)Math.Floor(x), (int)Math.Floor(y));
            bool Rock(float x, float y) => Phase01Layout.IsRock((int)Math.Floor(x), (int)Math.Floor(y));

            // Pecas de chao: base apoiada, corpo fora da rocha. As pocas e os
            // parafusos sao rentes e afundam um pouco de proposito.
            void Grounded(string what, string sprite, float x, float y)
            {
                if (!art.TryGetValue(sprite, out PixelCanvas c)) return;
                if (Math.Abs(y - Math.Round(y)) > 0.01f)
                {
                    problems.Add($"{what} em ({x}, {y}) nao assenta numa linha de chao.");
                    return;
                }

                int bottom = -1;
                for (int py = c.Height - 1; py >= 0 && bottom < 0; py--)
                {
                    for (int px = 0; px < c.Width; px++)
                    {
                        if (c.Get(px, py).A > 0) { bottom = py; break; }
                    }
                }

                if (bottom < 0) return;
                if (bottom < c.Height - 1)
                {
                    problems.Add($"{what} em ({x}, {y}): o desenho nao encosta na base, fica flutuando {c.Height - 1 - bottom}px.");
                }

                int unsupported = 0;
                int baseWidth = 0;
                for (int px = 0; px < c.Width; px++)
                {
                    bool foot = false;
                    for (int py = c.Height - 2; py < c.Height; py++) foot |= c.Get(px, py).A > 0;
                    if (!foot) continue;
                    baseWidth++;
                    if (!Solid(x + (px + 0.5f) / ppu, y - 0.5f)) unsupported++;
                }

                if (unsupported > 1)
                {
                    problems.Add($"{what} em ({x}, {y}) tem {unsupported} de {baseWidth}px da base no ar.");
                }

                int buried = 0;
                for (int py = 0; py < c.Height - 2; py++)
                {
                    for (int px = 0; px < c.Width; px++)
                    {
                        if (c.Get(px, py).A == 0) continue;
                        if (Rock(x + (px + 0.5f) / ppu, y + (c.Height - py - 0.5f) / ppu)) buried++;
                    }
                }

                if (buried > 2) problems.Add($"{what} em ({x}, {y}) entra na rocha ({buried}px).");
            }

            // O que fica pregado na parede: nada pela metade dentro da rocha
            // nem tampando uma janela. Engrenagens e canos entram na rocha de
            // proposito (o eixo e a ponta ficam enterrados).
            foreach (Piece piece in Pieces)
            {
                if (piece.Layer != Layer.Wall || piece.Width > 0f || piece.Height > 0f || piece.Motion == Motion.Spin) continue;
                if (piece.Sprite.StartsWith("d1_pipe") || piece.Sprite == DecorArt.Chute) continue;
                if (!art.TryGetValue(piece.Sprite, out PixelCanvas c)) continue;

                int total = 0, inRock = 0, inWindow = 0;
                for (int py = 0; py < c.Height; py++)
                {
                    for (int px = 0; px < c.Width; px++)
                    {
                        if (c.Get(px, py).A == 0) continue;
                        total++;
                        float wx = piece.X + (px + 0.5f) / ppu;
                        float wy = piece.Y + (c.Height - py - 0.5f) / ppu;
                        if (Rock(wx, wy)) inRock++;
                        if (Phase01Layout.IsWindow((int)Math.Floor(wx), (int)Math.Floor(wy))) inWindow++;
                    }
                }

                if (inRock > total / 8) problems.Add($"{piece.Sprite} na parede em ({piece.X}, {piece.Y}) entra na rocha ({inRock}/{total}px).");
                if (inWindow > total / 8) problems.Add($"{piece.Sprite} na parede em ({piece.X}, {piece.Y}) tampa uma janela.");
            }

            foreach (Piece piece in Pieces)
            {
                if (piece.Layer == Layer.Wall || piece.Layer == Layer.Front) continue;
                if (piece.Width > 0f || piece.Height > 0f) continue;
                if (piece.Sprite == DecorArt.Puddle || piece.Sprite == DecorArt.PuddleSmall) continue;
                Grounded(piece.Sprite, piece.Sprite, piece.X, piece.Y);
            }

            foreach (Lamp lamp in Lamps) Grounded("O poste", DecorArt.LampPost, lamp.X, lamp.Y);

            foreach (Hanging hanging in Hangings)
            {
                if (!Rock(hanging.X, hanging.Top + 0.25f) || Solid(hanging.X, hanging.Top - 0.25f))
                {
                    problems.Add($"A corrente em ({hanging.X}, {hanging.Top}) nao sai do teto.");
                }

                float endHeight = 0f;
                float endHalf = 0.25f;
                if (hanging.End != null && art.TryGetValue(hanging.End, out PixelCanvas end))
                {
                    endHeight = end.Height / (float)ppu;
                    endHalf = end.Width / (float)ppu * 0.5f;
                }

                // Folga pro balanco: a ponta varre um pouco pros lados.
                float sway = (float)Math.Sin(2.5 * Math.PI / 180.0) * (hanging.Length + endHeight);
                for (float y = hanging.Top - 0.25f; y > hanging.Top - hanging.Length - endHeight; y -= 0.25f)
                {
                    float half = y < hanging.Top - hanging.Length ? endHalf + sway : 0.2f;
                    if (Solid(hanging.X - half, y) || Solid(hanging.X + half, y) || Solid(hanging.X, y))
                    {
                        problems.Add($"A corrente em ({hanging.X}, {hanging.Top}) atravessa terreno em y={y}.");
                        break;
                    }
                }
            }
        }
    }
}
