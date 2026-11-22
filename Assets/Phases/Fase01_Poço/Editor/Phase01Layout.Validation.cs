using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class Phase01Layout
    {
        // ------------------------------------------------------------------
        // Validacao
        // ------------------------------------------------------------------

        /// <summary>
        /// Confere se o mapa e realmente navegavel, sem precisar jogar.
        ///
        /// Solta o LevelMap.Explore a partir de onde o Scrap nasce — ele anda,
        /// pula, cai e pega carona no vapor da chamine, com as mesmas regras de
        /// alcance pra todas as fases — e confere que todo trecho de chao foi
        /// alcancado.
        ///
        /// Devolve a lista de problemas. Vazia quer dizer que da pra andar pelo
        /// mapa inteiro: do monte de sucata ate a saida da arena e ate a
        /// chamine que leva ao District 2.
        /// </summary>
        public static List<string> Validate()
        {
            List<string> problems = LevelMap.RowProblems(Rows, Width, Height);
            if (problems.Count > 0) return problems;

            // A sala e fechada: se a borda tiver ar, a camera mostra o vazio.
            for (int x = 0; x < Width; x++)
            {
                if (!IsRock(x, 0) || !IsRock(x, Height - 1))
                {
                    problems.Add($"A coluna {x} tem buraco no chao ou no teto da sala.");
                }
            }

            for (int y = 0; y < Height; y++)
            {
                if (!IsRock(0, y) || !IsRock(Width - 1, y))
                {
                    problems.Add($"A linha {y} tem buraco numa das paredes da sala.");
                }
            }

            // A partir do poco vertical o caminho conta com o pulo de parede, e
            // o pogo nos espinhos ele tem desde o comeco. Sem o Steam Dash o
            // abismo tem que barrar; com ele (que soma uns 3 tiles a um pulo) e
            // com o vapor da saida, o mapa todo abre.
            var noDash = new LevelMap.JumpRules { WallJump = true, Pogo = true };
            var rules = new LevelMap.JumpRules { WallJump = true, Pogo = true, MaxRun = 8 };

            LevelMap.Reach beforeBoss = Map.Explore(SpawnTile, noDash);
            // Depois do boss: o vapor pro District 2 e o da volta da arena.
            LevelMap.Lift[] purifiedLifts = { District2Vent, ArenaReturnVent };
            LevelMap.Reach reach = Map.Explore(SpawnTile, rules, purifiedLifts);

            if (beforeBoss.CanStand(FarLedgeTile.X, FarLedgeTile.Y))
            {
                problems.Add("Da pra cruzar o abismo sem o Steam Dash: ele nao esta barrando nada.");
            }

            if (!beforeBoss.CanStand(BossSpawnTile.X, BossSpawnTile.Y))
            {
                problems.Add("Nao da pra chegar na arena sem o Steam Dash, e e la que ele se ganha.");
            }

            bool exitTouched = false;
            for (int y = District2Exit.Y; y < District2Exit.Y + District2Exit.Height; y++)
            {
                for (int x = District2Exit.X; x < District2Exit.X + District2Exit.Width; x++)
                {
                    exitTouched |= reach.CanTouch(x, y);
                }
            }

            if (!exitTouched)
            {
                problems.Add("Nem com o Steam Dash e o vapor da pra chegar na saida pro District 2.");
            }

            List<LevelMap.Perch> perches = Map.FindPerches();

            foreach (LevelMap.Perch perch in perches)
            {
                // Um toco de 1 tile nao e lugar que o desenho precise: sao as
                // pontas das saliencias das paredes.
                if (perch.Length < 2) continue;

                bool reached = false;
                for (int x = perch.X; x <= perch.Right && !reached; x++)
                {
                    reached = reach.CanStand(x, perch.Y);
                }

                if (!reached)
                {
                    problems.Add($"O trecho de chao em {perch} esta fora de alcance a partir de onde o Scrap nasce.");
                }
            }

            void MustReach((int X, int Y) tile, string what)
            {
                if (!IsStandable(tile.X, tile.Y))
                {
                    problems.Add($"{what} ({tile.X}, {tile.Y}) nao e um lugar onde da pra ficar em pe.");
                }
                else if (!reach.CanStand(tile.X, tile.Y))
                {
                    problems.Add($"{what} ({tile.X}, {tile.Y}) esta fora de alcance a partir de onde o Scrap nasce.");
                }
            }

            MustReach(LandingTile, "O patamar do topo do poco");
            MustReach(ReturnTile, "A chegada de quem volta do District 2");
            MustReach(LeverTile, "A alavanca");
            MustReach((WorkbenchTile.X + 1, WorkbenchTile.Y), "A bancada");
            MustReach((PreBossWorkbenchTile.X + 1, PreBossWorkbenchTile.Y), "A bancada da Antessala");

            // A da Antessala existe pra quem morre pro boss: tem que dar pra
            // chegar nela antes de ganhar o Steam Dash.
            if (!beforeBoss.CanStand(PreBossWorkbenchTile.X + 1, PreBossWorkbenchTile.Y))
            {
                problems.Add("A bancada da Antessala so e alcancada com o Steam Dash, depois do boss.");
            }
            MustReach(EntryGateTile, "A entrada da arena");

            foreach (NpcSpot npc in Npcs)
            {
                MustReach((npc.X, npc.Y), $"O personagem {npc.Name}");
            }

            foreach (LoreSpot lore in Lore)
            {
                MustReach((lore.X, lore.Y), $"O texto {lore.Name}");
            }

            foreach (TutorialSpot sign in Tutorials)
            {
                MustReach((sign.X, sign.Y), $"A placa {sign.Name}");
            }

            // A placa do pulo de parede tem que estar embaixo da chamine que ela ensina.
            foreach (TutorialSpot sign in Tutorials)
            {
                if (sign.Kind != TutorialKind.WallJump) continue;
                if (sign.AreaX1 < Chimney.X || sign.AreaX0 > Chimney.X + Chimney.Width - 1)
                {
                    problems.Add($"A area de {sign.Name} nao fica embaixo da chamine.");
                }
            }

            // E a do golpe pra baixo, na beirada dos espinhos da obra.
            foreach (TutorialSpot sign in Tutorials)
            {
                if (sign.Kind != TutorialKind.Pogo) continue;
                if (!IsSpike(sign.X + 1, sign.Y - 1)) problems.Add($"A placa {sign.Name} nao fica na beirada dos espinhos.");
            }

            // Tudo o que se le ou com quem se conversa: o HUD so mostra um
            // aviso por vez, entao as areas nao podem se encostar. E conversar
            // trava o Scrap, entao nada disso pode ficar no caminho de uma patrulha.
            var talkSpots = new List<(string Name, int X, int Y)>();
            foreach (NpcSpot npc in Npcs) talkSpots.Add((npc.Name, npc.X, npc.Y));
            foreach (LoreSpot lore in Lore) talkSpots.Add((lore.Name, lore.X, lore.Y));

            for (int i = 0; i < talkSpots.Count; i++)
            {
                for (int j = i + 1; j < talkSpots.Count; j++)
                {
                    if (Math.Abs(talkSpots[i].X - talkSpots[j].X) < 3 && Math.Abs(talkSpots[i].Y - talkSpots[j].Y) < 3)
                    {
                        problems.Add($"{talkSpots[i].Name} e {talkSpots[j].Name} estao perto demais: os avisos se sobrepoem.");
                    }
                }

                foreach (Patrol crawler in Crawlers)
                {
                    if (talkSpots[i].Y == crawler.Y && talkSpots[i].X >= crawler.X0 - 2 && talkSpots[i].X <= crawler.X1 + 2)
                    {
                        problems.Add($"{talkSpots[i].Name} fica na patrulha de {crawler.Name}.");
                    }
                }
            }

            foreach (Patrol crawler in Crawlers)
            {
                for (int x = crawler.X0; x <= crawler.X1; x++)
                {
                    if (!IsStandable(x, crawler.Y))
                    {
                        problems.Add($"{crawler.Name} patrulha ({x}, {crawler.Y}), que nao e chao.");
                    }
                }
            }

            // O portao tem que importar: com ele fechado da pra chegar na
            // alavanca, e nao da pra chegar na bancada.
            Map.Fill(Gate.X, Gate.Y, Gate.X, Gate.Y + Gate.Height - 1, LevelMap.Rock);
            LevelMap.Reach locked = Map.Explore(SpawnTile, noDash);
            Map.Fill(Gate.X, Gate.Y, Gate.X, Gate.Y + Gate.Height - 1, LevelMap.Air);

            if (!locked.CanStand(LeverTile.X, LeverTile.Y))
            {
                problems.Add("Com o portao fechado nao da pra chegar na alavanca que o abre.");
            }

            if (locked.CanStand(WorkbenchTile.X + 1, WorkbenchTile.Y))
            {
                problems.Add("Da pra chegar na bancada com o portao fechado: ele nao esta fechando nada.");
            }
            MustReach(BossSpawnTile, "O ponto do mini boss");
            MustReach(ExitGateTile, "A saida da arena");
            MustReach(FarLedgeTile, "A beirada de la do abismo");

            foreach (ItemSpot item in Items)
            {
                MustReach((item.X, item.Y), $"O item {item.Id}");
            }

            // A obra abandonada: com a nevoa no lugar, nao se entra.
            (int X, int Y, int Width, int Height) veil = TrialVeil;
            Map.Fill(veil.X, veil.Y, veil.X + veil.Width - 1, veil.Y + veil.Height - 1, LevelMap.Rock);
            LevelMap.Reach veiled = Map.Explore(SpawnTile, rules);
            Map.Fill(veil.X, veil.Y, veil.X + veil.Width - 1, veil.Y + veil.Height - 1, LevelMap.Air);

            if (veiled.CanStand(TrialInsideTile.X, TrialInsideTile.Y))
            {
                problems.Add("Da pra entrar na obra abandonada com a nevoa no lugar: ela nao esta fechando nada.");
            }

            // A primeira parte da obra e de pogo: sem ele, nao se passa.
            var noPogo = new LevelMap.JumpRules { WallJump = true, MaxRun = 8 };
            LevelMap.Reach withoutPogo = Map.Explore(SpawnTile, noPogo, purifiedLifts);

            foreach (ItemSpot item in Items)
            {
                bool inTrial = !veiled.CanStand(item.X, item.Y);

                // O que fica fora da obra e nao depende do boss tem que dar
                // pra pegar antes dele, sem o Steam Dash.
                if (!inTrial && !item.AfterBoss && !beforeBoss.CanStand(item.X, item.Y))
                {
                    problems.Add($"O item {item.Id} so e alcancado com o Steam Dash, e ele nao depende do boss.");
                }

                // As ferramentas vem depois do pedido: atras do portao do
                // Rebite. So a da obra fica antes dele no mapa.
                if (item.Kind == ItemKind.MasterTool && !inTrial && locked.CanStand(item.X, item.Y))
                {
                    problems.Add($"A ferramenta {item.Id} da pra pegar antes de passar pelo Rebite.");
                }

                // O premio da obra e de quem ja tem o motor, e sabe quicar.
                if (item.Kind == ItemKind.Chassis && inTrial && beforeBoss.CanStand(item.X, item.Y))
                {
                    problems.Add($"Da pra chegar no premio da obra ({item.Id}) sem o Steam Dash.");
                }

                if (inTrial && withoutPogo.CanStand(item.X, item.Y))
                {
                    problems.Add($"Da pra chegar em {item.Id}, na obra, sem o pogo: os espinhos nao estao cobrando ele.");
                }

                // E de qualquer lugar da pra voltar pro poco.
                LevelMap.Reach back = Map.Explore((item.X, item.Y), rules, purifiedLifts);
                if (!back.CanStand(Npcs[0].X, Npcs[0].Y))
                {
                    problems.Add($"Quem pega o item {item.Id} nao consegue voltar pro poco.");
                }
            }

            // Depois do boss da pra voltar da arena ate o poco e ate o Ferrolho,
            // mesmo antes de ele montar o motor.
            LevelMap.Reach fromArena = Map.Explore(BossSpawnTile, noDash, purifiedLifts);
            foreach (NpcSpot npc in Npcs)
            {
                if (!fromArena.CanStand(npc.X, npc.Y))
                {
                    problems.Add($"Da arena, sem o Steam Dash, nao se volta ate {npc.Name}.");
                }
            }

            // Mas durante a luta o selo fecha o poco: nao se volta pra bancada.
            (int X, int Y, int Width) seal = ArenaSeal;
            Map.Fill(seal.X, seal.Y, seal.X + seal.Width - 1, seal.Y, LevelMap.Rock);
            LevelMap.Reach duringFight = Map.Explore(BossSpawnTile, rules);
            Map.Fill(seal.X, seal.Y, seal.X + seal.Width - 1, seal.Y, LevelMap.Air);

            if (duringFight.CanStand(PreBossWorkbenchTile.X + 1, PreBossWorkbenchTile.Y))
            {
                problems.Add("Com o selo fechado ainda da pra sair da arena pelo poco.");
            }

            void MustBeOpen((int X, int Y, int Width, int Height) area, string what)
            {
                for (int y = area.Y; y < area.Y + area.Height; y++)
                {
                    for (int x = area.X; x < area.X + area.Width; x++)
                    {
                        if (IsSolid(x, y)) problems.Add($"{what} esta entupida em ({x}, {y}).");
                    }
                }
            }

            MustBeOpen(Chimney, "O poco vertical");
            MustBeOpen(ExitStub, "A saida da arena");

            // O mini boss precisa de espaco pra andar: o fosso tem que ser um
            // chao so, sem degrau no meio.
            LevelMap.Perch arenaFloor = perches.Find(perch => perch.Contains(BossSpawnTile.X, BossSpawnTile.Y));
            if (arenaFloor.Length < 16)
            {
                problems.Add($"O fosso da arena tem so {arenaFloor.Length} tiles de chao continuo: e pouco pra uma luta.");
            }

            // Nada de patamar na altura da cabeca dele fora de onde o corpo dele
            // passa: dali se bate no Coletor sem que ele alcance. Dentro do
            // caminho dele pode (os degraus): quando ele passa por baixo, encosta.
            // Mais alto (as plataformas do meio) pode, que o salto com pancada
            // dele chega la.
            (int X, int Y, int Width, int Height) arenaArea = ArenaBounds;
            const float bossHalfWidth = 1.5f;
            for (int y = arenaArea.Y + 1; y <= arenaArea.Y + 3; y++)
            {
                for (int x = arenaArea.X; x < arenaArea.X + arenaArea.Width; x++)
                {
                    bool underBoss = x + 1f > BossRange.Min - bossHalfWidth && x < BossRange.Max + bossHalfWidth;
                    if (IsStandable(x, y) && !underBoss)
                    {
                        problems.Add($"Ha um patamar na altura da cabeca do Coletor, fora do alcance dele, em ({x}, {y}).");
                    }
                }
            }

            // E no meio da luta (o selo fechado, sem o vapor da volta) da pra
            // subir nas plataformas de cima: e por elas que se passa por cima dele.
            Map.Fill(seal.X, seal.Y, seal.X + seal.Width - 1, seal.Y, LevelMap.Rock);
            LevelMap.Reach inFight = Map.Explore(BossSpawnTile, noDash);
            Map.Fill(seal.X, seal.Y, seal.X + seal.Width - 1, seal.Y, LevelMap.Air);

            foreach (LevelMap.Perch perch in perches)
            {
                bool inArena = perch.Y > arenaArea.Y + 3 && perch.Y < arenaArea.Y + arenaArea.Height
                    && perch.X >= arenaArea.X && perch.Right < arenaArea.X + arenaArea.Width;
                if (!inArena) continue;

                bool reached = false;
                for (int x = perch.X; x <= perch.Right && !reached; x++) reached = inFight.CanStand(x, perch.Y);

                if (!reached) problems.Add($"A plataforma da arena em {perch} nao se alcanca do chao durante a luta.");
            }

            for (int i = 0; i < Windows.Length; i++)
            {
                Window window = Windows[i];

                // Uma janela atras de rocha aparece cortada, e some sem aviso.
                for (int y = window.Y; y < window.Y + window.Height; y++)
                {
                    for (int x = window.X; x < window.X + window.Width; x++)
                    {
                        if (IsRock(x, y))
                        {
                            problems.Add($"A janela {window.Sprite} em ({window.X}, {window.Y}) " +
                                         $"fica atras de rocha em ({x}, {y}).");
                        }
                    }
                }

                // E duas janelas uma em cima da outra desenham dois pedacos de
                // parede no mesmo lugar.
                for (int j = i + 1; j < Windows.Length; j++)
                {
                    Window other = Windows[j];

                    bool apart = window.X + window.Width <= other.X
                        || other.X + other.Width <= window.X
                        || window.Y + window.Height <= other.Y
                        || other.Y + other.Height <= window.Y;

                    if (!apart)
                    {
                        problems.Add($"As janelas em ({window.X}, {window.Y}) e ({other.X}, {other.Y}) se sobrepoem.");
                    }
                }
            }

            // Os geodos so dentro da rocha, com um tile de folga em volta.
            foreach (var (_, gx, gy) in Geodes)
            {
                bool buried = true;
                for (int y = gy - 1; y <= gy + 2 && buried; y++)
                {
                    for (int x = gx - 1; x <= gx + 2 && buried; x++)
                    {
                        buried = IsRock(x, y);
                    }
                }

                if (!buried)
                {
                    problems.Add($"O geodo em ({gx}, {gy}) nao esta inteiro dentro da rocha: parece uma pedra solta.");
                }
            }

            var taken = new HashSet<(int, int, PropLayer)>();
            foreach (Prop prop in Props)
            {
                if (!taken.Add((prop.X, prop.Y, prop.Layer)))
                {
                    problems.Add($"Dois objetos na mesma celula ({prop.X}, {prop.Y}) da camada {prop.Layer}.");
                }

                if (!prop.Grounded) continue;

                if (IsSolid(prop.X, prop.Y))
                {
                    problems.Add($"{prop.Tile} em ({prop.X}, {prop.Y}) esta dentro do terreno.");
                }
                else if (!IsSolid(prop.X, prop.Y - 1))
                {
                    problems.Add($"{prop.Tile} em ({prop.X}, {prop.Y}) esta flutuando: nao ha chao embaixo.");
                }
            }

            return problems;
        }
    }
}
