using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Prepara os assets que a Phase 2 consome. A arte de latao nao existe em
    /// nenhum pack: ela e desenhada pelo BrassArt e gravada aqui como PNG, em
    /// Assets/Generated/Brass. Pode apagar a pasta — gerar a fase de novo
    /// reconstroi tudo.
    /// </summary>
    public static class Phase02Assets
    {
        public const string BrassFolder = Phase01Assets.GeneratedRoot + "/Brass";

        public const string TilesPath = BrassFolder + "/brass_tiles.png";
        public const string PipesPath = BrassFolder + "/pipe_tiles.png";
        public const string AcidPath = BrassFolder + "/acid_tiles.png";

        public const string TilePrefix = "brass";
        public const string PipePrefix = "pipe";
        public const string AcidPrefix = "acid";

        /// <summary>A animacao de agua do pack, que vira a superficie do acido.</summary>
        public const string WaterPath =
            "Assets/Cave Platformer Tileset/Enviroment sprite sheets/water_animated.png";

        /// <summary>Fogueira, baus e espinhos animados. So a fogueira (linha 3) e usada, nas fornalhas.</summary>
        public const string AnimatedObjectsPath =
            "Assets/Cave Platformer Tileset/Enviroment sprite sheets/objects_misc_animated.png";

        /// <summary>A parede de caverna azulada, pro trecho dos Silent Geodes.</summary>
        public const string BlueBackgroundPath =
            "Assets/Cave Platformer Tileset/Enviroment sprite sheets/bg_cave_blueish_16x16.png";

        public const string AnimatedObjectsPrefix = "miscanim";
        public const string BlueBackgroundPrefix = "bgblue";

        private const int FireRow = 3;
        private const int FireFrames = 6;

        // Nomes das pecas avulsas. Cada uma vira um PNG com esse nome.
        public const string ValveRusty = "valve_rusty";
        public const string ValveClean = "valve_clean";
        public const string GaugeBig = "gauge_big";
        public const string GaugeSmall = "gauge_small";
        public const string NeedleBig = "needle_big";
        public const string NeedleSmall = "needle_small";
        public const string Boiler = "boiler";
        public const string BoilerGlow = "boiler_glow";
        public const string Furnace = "furnace";
        public const string Hatch = "hatch";
        public const string Workbench = "workbench";
        public const string ScrapPolished = "scrap_polished";

        // A decoracao dos operarios.
        public const string HangingLamp = "decor_lamp";
        public const string Locker = "decor_locker";
        public const string LockerOpen = "decor_locker_open";
        public const string MaskRack = "decor_mask_rack";
        public const string EmptyShelf = "decor_shelf";
        public const string Barrel = "decor_barrel";
        public const string Railing = "decor_railing";
        public const string CrownPoster = "decor_crown_poster";
        public const string RoomSign = "decor_sign";
        public const string Chain = "decor_chain";
        public const string ElevatorCage = "elevator_cage";
        public const string ElevatorRail = "elevator_rail";
        public const string SkylineFar = "skyline_far";
        public const string SkylineNear = "skyline_near";

        // As pecas das mecanicas do Scrap, que as duas fases usam.
        public const string ChiselSlash = "chisel_slash";
        public const string Shard = "opaque_shard";
        public const string OldScrap = "old_scrap";
        public const string LeverRusty = "lever_rusty";
        public const string LeverClean = "lever_clean";
        public const string GateBars = "gate_bars";
        public const string ScrapChunk = "scrap_chunk";
        public const string ShockWave = "shock_wave";
        public const string DashModule = "dash_module";

        // O que o District 2 tem de vivo, e as pecas das mecanicas dele.
        public const string PipeLeech = "pipe_leech";
        public const string ValveSentry = "valve_sentry";
        public const string BrassWelder = "brass_welder";
        public const string AcidBloater = "acid_bloater";
        public const string SteamBolt = "steam_bolt";
        public const string BoilerMaster = "boiler_master";
        public const string Crisol = "crisol";
        public const string AcidBody = "acid_body";
        public const string AcidSurface = "acid_surface";
        public const string SteamColumn = "steam_column";
        public const string CrackedWall = "cracked_wall";

        // O que se le pelo mapa.
        public const string LoosePage = "loose_page";
        public const string ScribbleText = "scribble_text";
        public const string ScribbleMask = "scribble_mask";
        public const string Plaque = "plaque";

        /// <summary>
        /// As folhas de quadros do District 1, no detalhe do Scrap: o
        /// rastejante, o Coletor (inteiro e rachado) e os descartados.
        /// </summary>
        public const string CrawlerSheet = "crawler_sheet";
        public const string ColetorSheet = "coletor_sheet";
        public const string ColetorCrackedSheet = "coletor_cracked_sheet";

        /// <summary>
        /// As folhas de quadros do District 2, no detalhe do Scrap: os quatro
        /// inimigos, o Mestre das Caldeiras (normal e em sobrepressao) e quem
        /// se conversa (ver District2NpcArt).
        /// </summary>
        public const string LeechSheet = "d2_leech_sheet";
        public const string SentrySheet = "d2_sentry_sheet";
        public const string WelderSheet = "d2_welder_sheet";
        public const string BloaterSheet = "d2_bloater_sheet";
        public const string MasterSheet = "d2_master_sheet";
        public const string MasterHotSheet = "d2_master_hot_sheet";

        /// <summary>0 o velho encanador, 1 Solda, 2 Crisol.</summary>
        public static string District2NpcSheet(int variant) => "d2_npc_" + variant + "_sheet";

        /// <summary>Um descartado por variacao: 0 Refugo, 1 Rebite, 2 Ferrolho, 3 Pequena.</summary>
        public static string DiscardedSheet(int variant) => "discarded_" + variant + "_sheet";

        /// <summary>As placas de tutorial: 0 pulo de parede, 1 golpe pra baixo.</summary>
        public static string TutorialSignName(int icon) => "tutorial_sign_" + icon;

        /// <summary>O nome de um quadro do Scrap: sucata ou polido, e o numero do quadro.</summary>
        public static string HeroFrameName(bool polished, int frame)
        {
            return (polished ? "hero_polished_" : "hero_scrap_") + frame.ToString("00");
        }

        /// <summary>Os quadros do Scrap, na ordem do HeroArt (e do ScrapAnimator).</summary>
        public static Sprite[] HeroFrames(bool polished)
        {
            var frames = new Sprite[HeroArt.FrameCount];
            for (int i = 0; i < frames.Length; i++) frames[i] = LoadSprite(HeroFrameName(polished, i));
            return frames;
        }

        /// <summary>As ferramentas do mestre do Rebite: 0 buril, 1 lupa, 2 pinca.</summary>
        public static string MasterToolName(int kind) => "master_tool_" + kind;

        /// <summary>As recompensas escondidas: a gema lapidada e o cofre de fragmentos.</summary>
        public const string CutGem = "cut_gem";
        public const string ShardCache = "shard_cache";

        public static string NozzleName(int widthTiles) => "nozzle_" + widthTiles;

        public static string SpritePath(string name) => $"{BrassFolder}/{name}.png";

        public static Sprite LoadSprite(string name)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath(name));
            if (sprite == null) Debug.LogWarning($"[Phase 2] Sprite '{name}' nao existe em {BrassFolder}.");
            return sprite;
        }

        /// <summary>
        /// Desenha e importa toda a arte de latao. Devolve falso se alguma
        /// folha nao pode ser fatiada.
        /// </summary>
        public static bool EnsureArt(IEnumerable<int> nozzleWidths)
        {
            Phase01Assets.EnsureFolders();
            if (!AssetDatabase.IsValidFolder(BrassFolder))
            {
                AssetDatabase.CreateFolder(Phase01Assets.GeneratedRoot, "Brass");
            }

            WriteCanvas(TilesPath, BrassArt.BuildTiles());
            WriteCanvas(PipesPath, BrassArt.BuildPipes());

            PixelCanvas water = ReadCanvas(WaterPath);
            if (water == null)
            {
                Debug.LogError($"[Phase 2] Nao consegui ler {WaterPath} pra fazer o acido.");
                return false;
            }

            WriteCanvas(AcidPath, BrassArt.BuildAcid(water));

            var center = new Vector2(0.5f, 0.5f);
            var corner = Vector2.zero;

            // As pecas que giram (volante, mostrador) tem o pivo no centro; o
            // ponteiro, na base; o resto, no canto de baixo, pra a posicao do
            // objeto ser a celula onde ele apoia.
            WriteSprite(ValveRusty, BrassArt.ValveWheel(false), center);
            WriteSprite(ValveClean, BrassArt.ValveWheel(true), center);
            WriteSprite(GaugeBig, BrassArt.GaugeFace(32), center);
            WriteSprite(GaugeSmall, BrassArt.GaugeFace(16), center);
            WriteSprite(NeedleBig, BrassArt.GaugeNeedle(12), new Vector2(0.5f, 0f));
            WriteSprite(NeedleSmall, BrassArt.GaugeNeedle(6), new Vector2(0.5f, 0f));
            WriteSprite(Boiler, BrassArt.Boiler(), corner);
            WriteSprite(BoilerGlow, BrassArt.BoilerGlow(), corner);
            WriteSprite(Furnace, BrassArt.FurnaceFrame(), corner);
            WriteSprite(Hatch, BrassArt.Hatch(), corner);
            WriteSprite(Workbench, BrassArt.Workbench(), corner);
            WriteSprite(ScrapPolished, BrassArt.ScrapPolished(), new Vector2(0.5f, 0f));

            WriteSprite(HangingLamp, BrassArt.HangingLamp(), corner);
            WriteSprite(Locker, BrassArt.Locker(false), corner);
            WriteSprite(LockerOpen, BrassArt.Locker(true), corner);
            WriteSprite(MaskRack, BrassArt.MaskRack(), corner);
            WriteSprite(EmptyShelf, BrassArt.EmptyShelf(), corner);
            WriteSprite(Barrel, BrassArt.Barrel(), corner);
            WriteSprite(CrownPoster, BrassArt.CrownPoster(), corner);
            WriteSprite(RoomSign, BrassArt.RoomSign(), corner);
            WriteSprite(Railing, BrassArt.Railing(), corner, true);
            WriteSprite(Chain, BrassArt.Chain(), corner, true);
            WriteSprite(ElevatorCage, BrassArt.ElevatorCage(), corner);
            WriteSprite(ElevatorRail, BrassArt.ElevatorRail(), corner, true);

            foreach (int width in nozzleWidths.Distinct())
            {
                WriteSprite(NozzleName(width), BrassArt.Nozzle(width), corner);
            }

            var middle = new Vector2(0.5f, 0.5f);
            var feet = new Vector2(0.5f, 0f);

            WriteSprite(LoosePage, ScrapArt.LoosePage(), feet);
            WriteSprite(ScribbleText, ScrapArt.Scribble(0), middle);
            WriteSprite(ScribbleMask, ScrapArt.Scribble(1), middle);
            WriteSprite(Plaque, ScrapArt.Plaque(), middle);

            WriteSprite(PipeLeech, EnemyArt.PipeLeech(), middle);
            WriteSprite(ValveSentry, EnemyArt.ValveSentry(), feet);
            WriteSprite(BrassWelder, EnemyArt.BrassWelder(), feet);
            WriteSprite(AcidBloater, EnemyArt.AcidBloater(), middle);
            WriteSprite(SteamBolt, EnemyArt.SteamBolt(), middle);
            WriteSprite(BoilerMaster, EnemyArt.BoilerMaster(), feet);
            WriteSprite(Crisol, EnemyArt.Crisol(), feet);

            // Os que repetem (modo Tiled) precisam de malha inteira.
            WriteSprite(AcidBody, EnemyArt.AcidBody(), corner, true);
            WriteSprite(AcidSurface, EnemyArt.AcidSurface(), corner, true);
            WriteSprite(SteamColumn, EnemyArt.SteamColumn(), feet, true);
            WriteSprite(CrackedWall, EnemyArt.CrackedWall(), corner, true);

            WriteSprite(SkylineFar, BrassArt.Skyline(false), corner, true);
            WriteSprite(SkylineNear, BrassArt.Skyline(true), corner, true);

            return Phase01Assets.SliceGrid(TilesPath, TilePrefix)
                && Phase01Assets.SliceGrid(PipesPath, PipePrefix)
                && Phase01Assets.SliceGrid(AcidPath, AcidPrefix)
                && Phase01Assets.SliceGrid(AnimatedObjectsPath, AnimatedObjectsPrefix)
                && Phase01Assets.SliceGrid(BlueBackgroundPath, BlueBackgroundPrefix);
        }

        /// <summary>
        /// A arte que o Scrap e as mecanicas dele precisam em qualquer fase: o
        /// sprite polido, o risco do cinzel, o shard, a old scrap, a alavanca,
        /// o portao, a bancada e o inimigo rastejante.
        /// </summary>
        public static bool EnsureMechanicsArt()
        {
            Phase01Assets.EnsureFolders();
            if (!AssetDatabase.IsValidFolder(BrassFolder))
            {
                AssetDatabase.CreateFolder(Phase01Assets.GeneratedRoot, "Brass");
            }

            var feet = new Vector2(0.5f, 0f);

            WriteSprite(ScrapPolished, BrassArt.ScrapPolished(), feet);
            WriteSprite(ChiselSlash, ScrapArt.ChiselSlash(), new Vector2(0.5f, 0.5f));
            WriteSprite(Shard, ScrapArt.Shard(), new Vector2(0.5f, 0.5f));
            WriteSprite(OldScrap, ScrapArt.OldScrap(), feet);
            WriteSprite(LeverRusty, ScrapArt.Lever(false), feet);
            WriteSprite(LeverClean, ScrapArt.Lever(true), feet);
            WriteSprite(GateBars, ScrapArt.GateBars(), Vector2.zero, true);
            WriteSprite(Workbench, BrassArt.Workbench(), Vector2.zero);
            WriteSprite(ScrapChunk, ScrapArt.ScrapChunk(), new Vector2(0.5f, 0.5f));
            WriteSprite(ShockWave, ScrapArt.ShockWave(), feet);
            WriteSprite(DashModule, ScrapArt.DashModule(), new Vector2(0.5f, 0.5f));
            PixelCanvas[] scrapFrames = HeroArt.Frames(HeroArt.Scrap());
            PixelCanvas[] polishedFrames = HeroArt.Frames(HeroArt.Polished());

            for (int i = 0; i < HeroArt.FrameCount; i++)
            {
                // O Scrap tem o dobro de pixels por tile do mundo: mais detalhe num corpo menor.
                WriteSprite(HeroFrameName(false, i), scrapFrames[i], feet, false, HeroArt.PixelsPerUnit);
                WriteSprite(HeroFrameName(true, i), polishedFrames[i], feet, false, HeroArt.PixelsPerUnit);
            }

            WriteSprite(NozzleName(4), BrassArt.Nozzle(4), Vector2.zero);

            // O que se le pelo mapa: o District 1 tambem tem.
            var middle = new Vector2(0.5f, 0.5f);
            WriteSprite(LoosePage, ScrapArt.LoosePage(), feet);
            WriteSprite(ScribbleText, ScrapArt.Scribble(0), middle);
            WriteSprite(Plaque, ScrapArt.Plaque(), middle);

            for (int icon = 0; icon < ScrapArt.TutorialSignIcons; icon++)
            {
                WriteSprite(TutorialSignName(icon), ScrapArt.TutorialSign(icon), feet);
            }

            for (int kind = 0; kind < ScrapArt.MasterToolCount; kind++)
            {
                WriteSprite(MasterToolName(kind), ScrapArt.MasterTool(kind), middle);
            }

            WriteSprite(CutGem, ScrapArt.CutGem(), middle);
            WriteSprite(ShardCache, ScrapArt.ShardCache(), middle);

            // O bocal do vapor que leva da arena de volta a Antessala, de 3 tiles.
            WriteSprite(NozzleName(3), BrassArt.Nozzle(3), Vector2.zero);

            return true;
        }

        /// <summary>
        /// A arte em alta que so o District 1 usa: os descartados, o
        /// rastejante e o Coletor. Fica fora do EnsureMechanicsArt, que as
        /// outras fases tambem chamam.
        /// </summary>
        public static void EnsureDistrict1Art()
        {
            Phase01Assets.EnsureFolders();
            if (!AssetDatabase.IsValidFolder(BrassFolder))
            {
                AssetDatabase.CreateFolder(Phase01Assets.GeneratedRoot, "Brass");
            }

            var feet = new Vector2(0.5f, 0f);

            // Os descartados, o rastejante e o Coletor: folhas de quadros com
            // 32 pixels por tile, como o Scrap.
            for (int variant = 0; variant < DiscardedArt.Count; variant++)
            {
                WriteSheet(DiscardedSheet(variant), DiscardedArt.Frames(variant), feet, DiscardedArt.PixelsPerUnit);
            }

            // O rastejante tem o pivo meio tile acima dos pes, como o desenho antigo.
            WriteSheet(CrawlerSheet, CrawlerArt.Frames(), new Vector2(0.5f, CrawlerArt.PivotY), CrawlerArt.PixelsPerUnit);
            var soles = new Vector2(0.5f, ColetorArt.PivotY);
            WriteSheet(ColetorSheet, ColetorArt.Frames(false), soles, ColetorArt.PixelsPerUnit);
            WriteSheet(ColetorCrackedSheet, ColetorArt.Frames(true), soles, ColetorArt.PixelsPerUnit);
        }

        /// <summary>
        /// A arte em alta que so o District 2 usa: os inimigos, o boss e quem
        /// se conversa. Os desenhos antigos (brass_welder, crisol...) ficam,
        /// porque as outras fases ainda os usam.
        /// </summary>
        public static void EnsureDistrict2Art()
        {
            Phase01Assets.EnsureFolders();
            if (!AssetDatabase.IsValidFolder(BrassFolder))
            {
                AssetDatabase.CreateFolder(Phase01Assets.GeneratedRoot, "Brass");
            }

            WriteSheet(LeechSheet, LeechArt.Frames(), new Vector2(0.5f, LeechArt.PivotY), LeechArt.PixelsPerUnit);
            WriteSheet(SentrySheet, SentryArt.Frames(), new Vector2(SentryArt.PivotX, SentryArt.PivotY), SentryArt.PixelsPerUnit);
            WriteSheet(WelderSheet, WelderArt.Frames(), new Vector2(WelderArt.PivotX, WelderArt.PivotY), WelderArt.PixelsPerUnit);
            WriteSheet(BloaterSheet, BloaterArt.Frames(), new Vector2(BloaterArt.PivotX, BloaterArt.PivotY), BloaterArt.PixelsPerUnit);

            var bossPivot = new Vector2(BoilerMasterArt.PivotX, BoilerMasterArt.PivotY);
            WriteSheet(MasterSheet, BoilerMasterArt.Frames(false), bossPivot, BoilerMasterArt.PixelsPerUnit);
            WriteSheet(MasterHotSheet, BoilerMasterArt.Frames(true), bossPivot, BoilerMasterArt.PixelsPerUnit);

            for (int variant = 0; variant < District2NpcArt.Count; variant++)
            {
                WriteSheet(District2NpcSheet(variant), District2NpcArt.Frames(variant), new Vector2(0.5f, 0f), District2NpcArt.PixelsPerUnit);
            }
        }

        /// <summary>Os quadros de uma folha, na ordem em que foram desenhados.</summary>
        public static Sprite[] LoadSheet(string name, int count)
        {
            string path = SpritePath(name);
            Dictionary<string, Sprite> sprites = SpritesOf(path);
            var frames = new Sprite[count];

            for (int i = 0; i < count; i++)
            {
                if (!sprites.TryGetValue(SheetFrameName(name, i), out frames[i]))
                {
                    Debug.LogWarning($"[Phase 2] Quadro {i} da folha '{name}' nao existe em {path}.");
                }
            }

            return frames;
        }

        private static string SheetFrameName(string sheet, int frame) => sheet + "_" + frame.ToString("00");

        /// <summary>
        /// Grava varios quadros do mesmo tamanho numa folha so (em grade, oito
        /// por linha) e a fatia, um sprite por quadro, todos com o mesmo pivo.
        /// </summary>
        internal static void WriteSheet(string name, PixelCanvas[] frames, Vector2 pivot, int pixelsPerUnit)
        {
            const int columns = 8;
            int width = frames[0].Width;
            int height = frames[0].Height;
            int rows = (frames.Length + columns - 1) / columns;

            var sheet = new PixelCanvas(width * Mathf.Min(columns, frames.Length), height * rows);
            for (int i = 0; i < frames.Length; i++)
            {
                sheet.Blit(frames[i], (i % columns) * width, (i / columns) * height);
            }

            string path = SpritePath(name);
            WriteCanvas(path, sheet);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();

            var factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            // Reaproveita o id de quem ja existia, pra cena nao perder a referencia.
            var previous = new Dictionary<string, GUID>();
            foreach (SpriteRect old in provider.GetSpriteRects()) previous[old.name] = old.spriteID;

            var rects = new List<SpriteRect>();
            var pairs = new List<SpriteNameFileIdPair>();

            for (int i = 0; i < frames.Length; i++)
            {
                string frameName = SheetFrameName(name, i);
                int x = (i % columns) * width;
                int y = (i / columns) * height;

                var rect = new SpriteRect
                {
                    name = frameName,
                    spriteID = previous.TryGetValue(frameName, out GUID id) ? id : GUID.Generate(),
                    // O Rect da textura tem origem embaixo; a grade e medida de cima.
                    rect = new Rect(x, sheet.Height - y - height, width, height),
                    alignment = SpriteAlignment.Custom,
                    pivot = pivot,
                };

                rects.Add(rect);
                pairs.Add(new SpriteNameFileIdPair(rect.name, rect.spriteID));
            }

            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(pairs);
            provider.Apply();
            importer.SaveAndReimport();
        }

        /// <summary>
        /// Os tiles da Phase 2: os da Phase 1 (caverna, sucata, miolo de rocha),
        /// os de latao, cano e acido, e os animados.
        /// </summary>
        public static Dictionary<string, TileBase> BuildTiles()
        {
            Dictionary<string, TileBase> tiles = Phase01Assets.BuildTiles();

            Phase01Assets.AddTilesFrom(TilesPath, tiles);
            Phase01Assets.AddTilesFrom(PipesPath, tiles);
            Phase01Assets.AddTilesFrom(AcidPath, tiles);
            Phase01Assets.AddTilesFrom(BlueBackgroundPath, tiles);

            Dictionary<string, Sprite> acid = SpritesOf(AcidPath);
            for (int column = 0; column < BrassArt.AcidWaveColumns; column++)
            {
                var frames = new List<Sprite>();
                for (int frame = 0; frame < BrassArt.AcidFrames; frame++)
                {
                    if (acid.TryGetValue(Phase01Assets.GridTileName(AcidPrefix, column, frame), out Sprite sprite))
                    {
                        frames.Add(sprite);
                    }
                }

                // Todas as colunas na mesma velocidade, senao a onda se desfaz.
                AddAnimatedTile(tiles, $"acid_surface_{column}", frames, 5f);
            }

            Dictionary<string, Sprite> animated = SpritesOf(AnimatedObjectsPath);
            var fire = new List<Sprite>();
            for (int frame = 0; frame < FireFrames; frame++)
            {
                if (animated.TryGetValue(Phase01Assets.GridTileName(AnimatedObjectsPrefix, frame, FireRow), out Sprite sprite))
                {
                    fire.Add(sprite);
                }
            }

            AddAnimatedTile(tiles, Phase02Layout.FireTile, fire, 9f);

            AssetDatabase.SaveAssets();
            Debug.Log($"[Phase 2] {tiles.Count} Tile assets prontos em {Phase01Assets.TilesFolder}.");
            return tiles;
        }

        private static Dictionary<string, Sprite> SpritesOf(string path)
        {
            var sprites = new Dictionary<string, Sprite>();

            foreach (Sprite sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
            {
                sprites[sprite.name] = sprite;
            }

            return sprites;
        }

        private static void AddAnimatedTile(Dictionary<string, TileBase> tiles, string name,
            List<Sprite> frames, float framesPerSecond)
        {
            if (frames.Count == 0)
            {
                Debug.LogWarning($"[Phase 2] Tile animado '{name}' ficou sem quadros.");
                return;
            }

            string assetPath = $"{Phase01Assets.TilesFolder}/{name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<AnimatedTile>(assetPath);

            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<AnimatedTile>();
                AssetDatabase.CreateAsset(tile, assetPath);
            }

            tile.m_AnimatedSprites = frames.ToArray();
            tile.m_MinSpeed = framesPerSecond;
            tile.m_MaxSpeed = framesPerSecond;
            tile.m_TileColliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);

            tiles[name] = tile;
        }

        // ------------------------------------------------------------------
        // Leitura e escrita de imagem
        // ------------------------------------------------------------------

        /// <summary>
        /// Le um PNG do projeto direto do arquivo. Assim nao depende de a
        /// textura estar marcada como legivel no importador.
        /// </summary>
        private static PixelCanvas ReadCanvas(string path)
        {
            if (!System.IO.File.Exists(path)) return null;

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(System.IO.File.ReadAllBytes(path)))
            {
                Object.DestroyImmediate(texture);
                return null;
            }

            var canvas = new PixelCanvas(texture.width, texture.height);
            Color32[] pixels = texture.GetPixels32();

            // A Unity guarda de baixo pra cima; o PixelCanvas, de cima pra baixo.
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color32 pixel = pixels[x + (texture.height - 1 - y) * texture.width];
                    canvas.Set(x, y, new Rgba(pixel.r, pixel.g, pixel.b, pixel.a));
                }
            }

            Object.DestroyImmediate(texture);
            return canvas;
        }

        internal static void WriteCanvas(string path, PixelCanvas canvas)
        {
            var pixels = new Color32[canvas.Width * canvas.Height];

            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    Rgba pixel = canvas.Get(x, y);
                    pixels[x + (canvas.Height - 1 - y) * canvas.Width] =
                        new Color32(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }

            var texture = new Texture2D(canvas.Width, canvas.Height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();

            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        internal static void WriteSprite(string name, PixelCanvas canvas, Vector2 pivot, bool tiled = false,
            int pixelsPerUnit = Phase01Assets.PixelsPerUnit)
        {
            string path = SpritePath(name);
            WriteCanvas(path, canvas);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            // Pra repetir em modo Tiled o sprite precisa de malha inteira e
            // textura que da a volta.
            importer.wrapMode = tiled ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = tiled ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
            if (tiled) settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }
    }
}
