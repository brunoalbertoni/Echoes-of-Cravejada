using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um efeito sonoro: o arquivo (em Resources/Sfx), o volume e como ele
    /// varia a cada vez. Tocar e <c>Sfx.Jump.Play()</c>.
    /// </summary>
    public sealed class SfxCue
    {
        /// <summary>Nome do arquivo, sem extensao. Com variacoes, o nome sem o "_1", "_2"...</summary>
        public readonly string Clip;

        /// <summary>Quantas variacoes o arquivo tem (Clip_1 ... Clip_N). 1: so o Clip.</summary>
        public readonly int Variants;

        /// <summary>Volume dele em relacao aos outros (0 a 1).</summary>
        public readonly float Volume;

        /// <summary>Quanto o tom varia pra cima e pra baixo a cada vez (0,05 = 5%).</summary>
        public readonly float PitchJitter;

        /// <summary>Segundos minimos entre duas vezes: o mesmo som no mesmo quadro nao dobra.</summary>
        public readonly float Cooldown;

        /// <summary>Som de interface ou de coleta: o aviso do topo da tela nao tilinta junto com ele.</summary>
        public readonly bool Interface;

        /// <summary>O tom dele (1 = o do arquivo). Deixa um arquivo servir a dois bichos: o pequeno, mais agudo.</summary>
        public readonly float Pitch;

        public SfxCue(string clip, float volume, int variants = 1, float pitchJitter = 0.04f, float cooldown = 0.04f,
            bool ui = false, float pitch = 1f)
        {
            Clip = clip;
            Volume = volume;
            Variants = Mathf.Max(1, variants);
            PitchJitter = pitchJitter;
            Cooldown = cooldown;
            Interface = ui;
            Pitch = pitch;
        }

        /// <summary>
        /// Toca. Sem <paramref name="at"/> o som e da tela (do Scrap, da
        /// interface); com, ele vem daquele ponto do mapa e some com a distancia.
        /// </summary>
        public void Play(Vector2? at = null, float volume = 1f, float pitch = 1f, float delay = 0f)
        {
            SfxPlayer.Play(this, at, volume, pitch, delay);
        }
    }

    /// <summary>
    /// A tabela dos efeitos sonoros: cada som do jogo e o volume dele. Mexer
    /// aqui e o jeito de ajustar a mistura.
    ///
    /// Os arquivos sao sintetizados por Tools/Audio/sfx_synth.py (sem
    /// gravacao de terceiros) e ficam em Resources/Sfx.
    /// </summary>
    public static class Sfx
    {
        /// <summary>O volume geral dos efeitos (a musica fica em SoundtrackCatalog).</summary>
        public const float Volume = 0.85f;

        // ------------------------------------------------------------------
        // O Scrap
        // ------------------------------------------------------------------

        public static readonly SfxCue Step = new SfxCue("Scrap_Step", 0.32f, variants: 4, pitchJitter: 0.07f, cooldown: 0.1f);
        public static readonly SfxCue Jump = new SfxCue("Scrap_Jump", 0.45f, variants: 2, cooldown: 0.08f);
        public static readonly SfxCue WallJump = new SfxCue("Scrap_WallJump", 0.45f, cooldown: 0.08f);
        public static readonly SfxCue WallGrab = new SfxCue("Scrap_WallGrab", 0.4f, cooldown: 0.3f);
        public static readonly SfxCue PressureJump = new SfxCue("Scrap_PressureJump", 0.5f, cooldown: 0.08f);
        public static readonly SfxCue Land = new SfxCue("Scrap_Land", 0.5f, variants: 2, pitchJitter: 0.06f, cooldown: 0.12f);
        public static readonly SfxCue LandHeavy = new SfxCue("Scrap_LandHeavy", 0.7f, cooldown: 0.12f);

        public static readonly SfxCue Swing = new SfxCue("Scrap_Swing", 0.5f, variants: 3, pitchJitter: 0.06f);
        public static readonly SfxCue Hit = new SfxCue("Scrap_Hit", 0.75f, variants: 3, pitchJitter: 0.06f);
        public static readonly SfxCue ChargeReady = new SfxCue("Scrap_ChargeReady", 0.35f, pitchJitter: 0f, cooldown: 0.2f);
        public static readonly SfxCue Resonance = new SfxCue("Scrap_Resonance", 0.8f, pitchJitter: 0.02f, cooldown: 0.2f);
        public static readonly SfxCue Dash = new SfxCue("Scrap_Dash", 0.55f, pitchJitter: 0.05f, cooldown: 0.1f);

        public static readonly SfxCue Hurt = new SfxCue("Scrap_Hurt", 0.75f, pitchJitter: 0.05f, cooldown: 0.2f);
        public static readonly SfxCue Death = new SfxCue("Scrap_Death", 0.85f, pitchJitter: 0f, cooldown: 0.5f);
        public static readonly SfxCue Respawn = new SfxCue("Scrap_Respawn", 0.5f, pitchJitter: 0f, cooldown: 0.5f);

        /// <summary>A lixa raspando: em loop, tocado pelo ScrapSounds enquanto ele raspa.</summary>
        public static readonly SfxCue ScrapeLoop = new SfxCue("Scrap_ScrapeLoop", 0.35f);
        public static readonly SfxCue ScrapeDone = new SfxCue("Scrap_ScrapeDone", 0.5f, pitchJitter: 0f, cooldown: 0.3f);

        // ------------------------------------------------------------------
        // Coleta
        // ------------------------------------------------------------------

        public static readonly SfxCue Shard = new SfxCue("Pickup_Shard", 0.35f, pitchJitter: 0f, cooldown: 0.03f, ui: true);
        public static readonly SfxCue ShardsRecovered = new SfxCue("Pickup_ShardsRecovered", 0.5f, pitchJitter: 0f, cooldown: 0.3f, ui: true);
        public static readonly SfxCue Gem = new SfxCue("Pickup_Gem", 0.45f, pitchJitter: 0f, cooldown: 0.3f, ui: true);
        public static readonly SfxCue KeyItem = new SfxCue("Pickup_KeyItem", 0.55f, pitchJitter: 0f, cooldown: 0.3f, ui: true);
        public static readonly SfxCue Chassis = new SfxCue("Pickup_Chassis", 0.6f, pitchJitter: 0f, cooldown: 0.3f, ui: true);

        // ------------------------------------------------------------------
        // Interface
        // ------------------------------------------------------------------

        public static readonly SfxCue AbilityGet = new SfxCue("Ui_AbilityGet", 0.7f, pitchJitter: 0f, cooldown: 0.5f, ui: true);
        public static readonly SfxCue Save = new SfxCue("Ui_Save", 0.5f, pitchJitter: 0f, cooldown: 1f, ui: true);
        public static readonly SfxCue GemSocket = new SfxCue("Ui_GemSocket", 0.5f, pitchJitter: 0f, cooldown: 0.5f, ui: true);
        public static readonly SfxCue DialogueOpen = new SfxCue("Ui_DialogueOpen", 0.25f, pitchJitter: 0.02f, ui: true);
        public static readonly SfxCue DialogueNext = new SfxCue("Ui_DialogueNext", 0.18f, pitchJitter: 0.03f, ui: true);
        public static readonly SfxCue DialogueClose = new SfxCue("Ui_DialogueClose", 0.2f, pitchJitter: 0.02f, ui: true);
        public static readonly SfxCue ShopMove = new SfxCue("Ui_ShopMove", 0.2f, pitchJitter: 0.02f, cooldown: 0.02f, ui: true);
        public static readonly SfxCue ShopBuy = new SfxCue("Ui_ShopBuy", 0.45f, pitchJitter: 0f, ui: true);
        public static readonly SfxCue ShopDeny = new SfxCue("Ui_ShopDeny", 0.22f, pitchJitter: 0f, cooldown: 0.15f, ui: true);
        public static readonly SfxCue Notice = new SfxCue("Ui_Notice", 0.25f, pitchJitter: 0f, cooldown: 1f, ui: true);

        // ------------------------------------------------------------------
        // Inimigos (tocam de onde estao: os longe soam baixo)
        // ------------------------------------------------------------------

        /// <summary>O golpe aparado: o Soldador de frente, o Sentinela em prata.</summary>
        public static readonly SfxCue Block = new SfxCue("Enemy_Block", 0.6f, pitchJitter: 0.06f);

        // As mortes. Cada inimigo escolhe a sua no Damageable; sem escolha, a de metal.
        public static readonly SfxCue DeathMetal = new SfxCue("Enemy_DeathMetal", 0.55f, pitchJitter: 0.06f);
        public static readonly SfxCue DeathHunter = new SfxCue("Enemy_DeathMetal", 0.6f, pitchJitter: 0.04f, pitch: 0.8f);
        public static readonly SfxCue DeathSteam = new SfxCue("Enemy_DeathSteam", 0.55f, pitchJitter: 0.05f);
        public static readonly SfxCue DeathMite = new SfxCue("Enemy_DeathCrystal", 0.5f, pitchJitter: 0.08f, pitch: 1.35f);
        public static readonly SfxCue DeathSentinel = new SfxCue("Enemy_DeathCrystal", 0.65f, pitchJitter: 0.04f, pitch: 0.75f);
        public static readonly SfxCue DeathGlass = new SfxCue("Enemy_DeathGlass", 0.65f, pitchJitter: 0.04f);
        public static readonly SfxCue DeathDeflate = new SfxCue("Enemy_DeathDeflate", 0.5f, pitchJitter: 0.06f);
        public static readonly SfxCue DeathGoo = new SfxCue("Enemy_DeathGoo", 0.5f, pitchJitter: 0.08f, pitch: 1.2f);

        public static readonly SfxCue CrawlerWindup = new SfxCue("Crawler_Windup", 0.5f, pitchJitter: 0.06f);
        public static readonly SfxCue CrawlerBite = new SfxCue("Crawler_Bite", 0.5f, pitchJitter: 0.06f);

        public static readonly SfxCue BloaterWindup = new SfxCue("Bloater_Windup", 0.35f, pitchJitter: 0.05f);
        public static readonly SfxCue BloaterSpray = new SfxCue("Bloater_Spray", 0.45f, pitchJitter: 0.05f);
        public static readonly SfxCue BloaterSwell = new SfxCue("Bloater_Swell", 0.45f, pitchJitter: 0.04f);
        public static readonly SfxCue BloaterBurst = new SfxCue("Bloater_Burst", 0.7f, pitchJitter: 0.04f);

        public static readonly SfxCue WelderWindup = new SfxCue("Welder_Windup", 0.4f, pitchJitter: 0.05f);
        public static readonly SfxCue WelderFlame = new SfxCue("Welder_Flame", 0.5f, pitchJitter: 0.05f);

        public static readonly SfxCue LeechDrop = new SfxCue("Leech_Drop", 0.45f, pitchJitter: 0.06f);
        public static readonly SfxCue LeechLand = new SfxCue("Leech_Land", 0.45f, pitchJitter: 0.06f);
        public static readonly SfxCue LeechWindup = new SfxCue("Leech_Windup", 0.3f, pitchJitter: 0.06f);
        public static readonly SfxCue LeechBite = new SfxCue("Leech_Bite", 0.45f, pitchJitter: 0.06f);

        public static readonly SfxCue SentryCharge = new SfxCue("Sentry_Charge", 0.35f, pitchJitter: 0.03f);
        public static readonly SfxCue SentryFire = new SfxCue("Sentry_Fire", 0.5f, pitchJitter: 0.05f);

        public static readonly SfxCue MiteWake = new SfxCue("Mite_Wake", 0.4f, pitchJitter: 0.1f, cooldown: 0.08f);
        public static readonly SfxCue HunterAlert = new SfxCue("Hunter_Alert", 0.5f, pitchJitter: 0.04f, cooldown: 0.2f);
        public static readonly SfxCue SentinelOpen = new SfxCue("Sentinel_Open", 0.35f, pitchJitter: 0.04f);
        public static readonly SfxCue SentinelFire = new SfxCue("Sentinel_Fire", 0.45f, pitchJitter: 0.05f);

        public static readonly SfxCue ExperimentCharge = new SfxCue("Experiment_Charge", 0.4f, pitchJitter: 0.05f);
        public static readonly SfxCue ExperimentLunge = new SfxCue("Experiment_Lunge", 0.45f, pitchJitter: 0.05f);
        public static readonly SfxCue ThrowerWindup = new SfxCue("Thrower_Windup", 0.35f, pitchJitter: 0.06f);
        public static readonly SfxCue ThrowerThrow = new SfxCue("Thrower_Throw", 0.4f, pitchJitter: 0.06f);
        public static readonly SfxCue FlaskBreak = new SfxCue("Flask_Break", 0.55f, pitchJitter: 0.06f);
        public static readonly SfxCue SlimeSplit = new SfxCue("Slime_Split", 0.55f, pitchJitter: 0.05f);
        public static readonly SfxCue Transmute = new SfxCue("Glass_Transmute", 0.45f, pitchJitter: 0.03f, cooldown: 0.15f);

        // ------------------------------------------------------------------
        // Mecanicas das fases
        // ------------------------------------------------------------------

        public static readonly SfxCue GateOpen = new SfxCue("Gate_Open", 0.6f, pitchJitter: 0.04f, cooldown: 0.1f);
        public static readonly SfxCue ValveTurn = new SfxCue("Valve_Turn", 0.55f, pitchJitter: 0.03f);
        public static readonly SfxCue AcidDrain = new SfxCue("Acid_Drain", 0.55f, pitchJitter: 0.04f, cooldown: 0.2f);
        public static readonly SfxCue AcidSizzle = new SfxCue("Acid_Sizzle", 0.5f, pitchJitter: 0.06f, cooldown: 0.7f);
        public static readonly SfxCue AcidAlarm = new SfxCue("Acid_Alarm", 0.45f, pitchJitter: 0f, cooldown: 2f);
        public static readonly SfxCue BoilerPressure = new SfxCue("Boiler_Pressure", 0.6f, pitchJitter: 0.02f);
        public static readonly SfxCue BoilerFuse = new SfxCue("Boiler_Fuse", 0.5f, pitchJitter: 0f);
        public static readonly SfxCue BoilerExplode = new SfxCue("Boiler_Explode", 0.85f, pitchJitter: 0.03f);
        public static readonly SfxCue SteamRefresh = new SfxCue("Steam_Refresh", 0.3f, pitchJitter: 0.05f, cooldown: 0.4f);

        public static readonly SfxCue CrystalCrack = new SfxCue("Crystal_Crack", 0.45f, pitchJitter: 0.06f);
        public static readonly SfxCue CrystalBreak = new SfxCue("Crystal_Break", 0.5f, pitchJitter: 0.06f);
        public static readonly SfxCue BarrierShatter = new SfxCue("Crystal_BarrierShatter", 0.85f, pitchJitter: 0.03f);

        /// <summary>A nota de um cristal ressonante. O tom de cada cristal sai de <see cref="CrystalNotePitch"/>.</summary>
        public static readonly SfxCue CrystalNote = new SfxCue("Crystal_Note", 0.55f, pitchJitter: 0f, cooldown: 0f);
        public static readonly SfxCue CrystalDull = new SfxCue("Crystal_Dull", 0.5f, pitchJitter: 0.05f);

        public static readonly SfxCue StatuePress = new SfxCue("Statue_Press", 0.6f, pitchJitter: 0.03f, cooldown: 0.15f);
        public static readonly SfxCue PuzzleRight = new SfxCue("Puzzle_Right", 0.35f, pitchJitter: 0f, cooldown: 0.1f);
        public static readonly SfxCue PuzzleWrong = new SfxCue("Puzzle_Wrong", 0.4f, pitchJitter: 0f, cooldown: 0.2f);
        public static readonly SfxCue PuzzleSolved = new SfxCue("Puzzle_Solved", 0.5f, pitchJitter: 0f, cooldown: 0.5f);
        public static readonly SfxCue LockRattle = new SfxCue("Lock_Rattle", 0.45f, pitchJitter: 0.05f, cooldown: 0.5f);
        public static readonly SfxCue LockUnlock = new SfxCue("Lock_Unlock", 0.6f, pitchJitter: 0.03f);

        public static readonly SfxCue MercuryDrip = new SfxCue("Mercury_Drip", 0.35f, pitchJitter: 0.12f, cooldown: 0.1f);
        public static readonly SfxCue MercurySplat = new SfxCue("Mercury_Splat", 0.5f, pitchJitter: 0.06f, cooldown: 0.2f);
        public static readonly SfxCue MercuryStep = new SfxCue("Mercury_Step", 0.32f, variants: 2, pitchJitter: 0.08f, cooldown: 0.1f);
        public static readonly SfxCue MirrorTurn = new SfxCue("Mirror_Turn", 0.5f, pitchJitter: 0.05f, cooldown: 0.1f);
        public static readonly SfxCue CrystalLockOpen = new SfxCue("CrystalLock_Open", 0.55f, pitchJitter: 0f);
        public static readonly SfxCue AlembicInsert = new SfxCue("Alembic_Insert", 0.5f, pitchJitter: 0.03f);
        public static readonly SfxCue AlembicBoil = new SfxCue("Alembic_Boil", 0.55f, pitchJitter: 0f);
        public static readonly SfxCue CircleActivate = new SfxCue("Circle_Activate", 0.6f, pitchJitter: 0.02f);

        public static readonly SfxCue ElevatorStart = new SfxCue("Elevator_Start", 0.6f, pitchJitter: 0f);
        /// <summary>O motor do elevador: em loop, tocado pelo proprio ElevatorRide durante a subida.</summary>
        public static readonly SfxCue ElevatorLoop = new SfxCue("Elevator_Loop", 0.4f);

        /// <summary>O zumbido dos raios de luz da fase 4: em loop, preso a cada emissor pelo SoundDirector.</summary>
        public static readonly SfxCue BeamHum = new SfxCue("Beam_Hum", 0.3f);

        public static readonly SfxCue PurifyWave = new SfxCue("Purify_Wave", 0.55f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue OxideBump = new SfxCue("Oxide_Bump", 0.4f, pitchJitter: 0.05f, cooldown: 0.5f);

        // ------------------------------------------------------------------
        // Chefes: o que e de todos
        // ------------------------------------------------------------------

        /// <summary>Um jato de Oxide na queda. O BossFall sobe o tom a cada um.</summary>
        public static readonly SfxCue BossFallJet = new SfxCue("Boss_FallJet", 0.6f, pitchJitter: 0.04f, cooldown: 0.08f);
        public static readonly SfxCue BossFallBurst = new SfxCue("Boss_FallBurst", 1f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue BossSlam = new SfxCue("Boss_Slam", 0.85f, pitchJitter: 0.04f, cooldown: 0.1f);
        public static readonly SfxCue BossStep = new SfxCue("Boss_Step", 0.45f, variants: 2, pitchJitter: 0.05f, cooldown: 0.12f);
        public static readonly SfxCue BossHurtMetal = new SfxCue("Boss_HurtMetal", 0.55f, pitchJitter: 0.06f, cooldown: 0.08f);
        public static readonly SfxCue BossHurtSilver = new SfxCue("Boss_HurtSilver", 0.55f, pitchJitter: 0.05f, cooldown: 0.08f);
        public static readonly SfxCue BossJump = new SfxCue("Boss_Jump", 0.6f, pitchJitter: 0.04f);
        public static readonly SfxCue BossWallHit = new SfxCue("Boss_WallHit", 0.8f, pitchJitter: 0.04f, cooldown: 0.2f);
        public static readonly SfxCue ArenaSealDown = new SfxCue("Arena_SealDown", 0.7f, pitchJitter: 0.03f, cooldown: 0.3f);
        public static readonly SfxCue ArenaSealUp = new SfxCue("Arena_SealUp", 0.6f, pitchJitter: 0.03f, cooldown: 0.3f);
        public static readonly SfxCue ShockWave = new SfxCue("Shock_Wave", 0.5f, pitchJitter: 0.05f, cooldown: 0.1f);
        public static readonly SfxCue ChunkLand = new SfxCue("Chunk_Land", 0.45f, pitchJitter: 0.1f, cooldown: 0.05f);
        public static readonly SfxCue PolishShine = new SfxCue("Polish_Shine", 0.6f, pitchJitter: 0f, cooldown: 1f, ui: true);

        // O Coletor (fase 1)
        public static readonly SfxCue CollectorWake = new SfxCue("Collector_Wake", 0.75f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue CollectorRoar = new SfxCue("Collector_Roar", 0.85f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue CollectorClawWindup = new SfxCue("Collector_ClawWindup", 0.5f);
        public static readonly SfxCue CollectorClawStrike = new SfxCue("Collector_ClawStrike", 0.6f);
        public static readonly SfxCue CollectorThrowWindup = new SfxCue("Collector_ThrowWindup", 0.5f);
        public static readonly SfxCue CollectorThrow = new SfxCue("Collector_Throw", 0.55f);

        // O Mestre das Caldeiras (fase 2)
        public static readonly SfxCue BoilerMasterWake = new SfxCue("BoilerMaster_Wake", 0.75f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue BoilerMasterOverpressure = new SfxCue("BoilerMaster_Overpressure", 0.85f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue BoilerMasterHammerWindup = new SfxCue("BoilerMaster_HammerWindup", 0.5f);
        public static readonly SfxCue BoilerMasterChargeWindup = new SfxCue("BoilerMaster_ChargeWindup", 0.55f);
        public static readonly SfxCue BoilerMasterChargeRun = new SfxCue("BoilerMaster_ChargeRun", 0.6f, pitchJitter: 0.02f);
        public static readonly SfxCue BoilerMasterBurstWindup = new SfxCue("BoilerMaster_BurstWindup", 0.5f);
        public static readonly SfxCue BoilerMasterBurstFire = new SfxCue("BoilerMaster_BurstFire", 0.5f, pitchJitter: 0.08f, cooldown: 0.06f);
        public static readonly SfxCue GeyserWarn = new SfxCue("Geyser_Warn", 0.45f, pitchJitter: 0.06f, cooldown: 0.1f);
        public static readonly SfxCue GeyserBurst = new SfxCue("Geyser_Burst", 0.6f, pitchJitter: 0.05f, cooldown: 0.1f);
        public static readonly SfxCue ModuleDrop = new SfxCue("Module_Drop", 0.6f, pitchJitter: 0f, cooldown: 0.5f);

        // O Capataz (fase 3): ele caca pelo som, entao os sons dele dizem onde esta e o que vai fazer.
        public static readonly SfxCue ForemanWake = new SfxCue("Foreman_Wake", 0.7f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue ForemanListen = new SfxCue("Foreman_Listen", 0.35f, pitchJitter: 0.05f, cooldown: 2.5f);
        public static readonly SfxCue ForemanStep = new SfxCue("Foreman_Step", 0.4f, variants: 2, pitchJitter: 0.06f, cooldown: 0.1f);
        public static readonly SfxCue ForemanPickWindup = new SfxCue("Foreman_PickWindup", 0.6f);
        public static readonly SfxCue ForemanPickSlam = new SfxCue("Foreman_PickSlam", 0.8f);
        public static readonly SfxCue ForemanPickStuck = new SfxCue("Foreman_PickStuck", 0.5f);
        public static readonly SfxCue ForemanRamWindup = new SfxCue("Foreman_RamWindup", 0.6f);
        public static readonly SfxCue ForemanRamWall = new SfxCue("Foreman_RamWall", 0.9f, pitchJitter: 0.03f);
        public static readonly SfxCue ForemanWhistle = new SfxCue("Foreman_Whistle", 0.6f, pitchJitter: 0.02f, cooldown: 0.5f);
        public static readonly SfxCue ForemanScream = new SfxCue("Foreman_Scream", 0.9f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue ForemanWarn = new SfxCue("Foreman_Warn", 0.4f, pitchJitter: 0.03f);
        public static readonly SfxCue ForemanHatch = new SfxCue("Foreman_Hatch", 0.5f, pitchJitter: 0.08f, cooldown: 0.1f);
        public static readonly SfxCue ForemanHurt = new SfxCue("Foreman_Hurt", 0.6f, variants: 2, pitchJitter: 0.06f, cooldown: 0.1f);
        public static readonly SfxCue ForemanDeath = new SfxCue("Foreman_Death", 0.9f, pitchJitter: 0f, cooldown: 2f);
        public static readonly SfxCue ShardLoosen = new SfxCue("Shard_Loosen", 0.3f, pitchJitter: 0.12f, cooldown: 0.08f);
        public static readonly SfxCue ShardLand = new SfxCue("Shard_Land", 0.45f, pitchJitter: 0.1f, cooldown: 0.06f);
        public static readonly SfxCue PebblePop = new SfxCue("Pebble_Pop", 0.25f, pitchJitter: 0.15f, cooldown: 0.07f);

        // A Broca (fase 3)
        /// <summary>O motor: em loop, preso a broca pelo proprio GeodeDrill.</summary>
        public static readonly SfxCue DrillMotor = new SfxCue("Drill_Motor", 0.45f);
        public static readonly SfxCue DrillEmerge = new SfxCue("Drill_Emerge", 0.7f, pitchJitter: 0.04f);
        public static readonly SfxCue DrillRumble = new SfxCue("Drill_Rumble", 0.6f, pitchJitter: 0.05f);
        public static readonly SfxCue DrillErupt = new SfxCue("Drill_Erupt", 0.85f, pitchJitter: 0.04f);
        public static readonly SfxCue DrillPillarCrack = new SfxCue("Drill_PillarCrack", 0.6f, pitchJitter: 0.04f);
        public static readonly SfxCue PillarShatter = new SfxCue("Crystal_Break", 0.75f, pitchJitter: 0.04f, pitch: 0.8f);
        public static readonly SfxCue DrillImpale = new SfxCue("Drill_Impale", 0.8f, pitchJitter: 0.03f);
        public static readonly SfxCue DrillShakeFree = new SfxCue("Drill_ShakeFree", 0.75f, pitchJitter: 0.03f);
        public static readonly SfxCue DrillCollapse = new SfxCue("Drill_Collapse", 1f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue DrillHurt = new SfxCue("Drill_Hurt", 0.6f, pitchJitter: 0.06f, cooldown: 0.08f);
        public static readonly SfxCue DrillDie = new SfxCue("Drill_Die", 0.9f, pitchJitter: 0f, cooldown: 2f);

        // O Alquimista de Mercurio (fase 4)
        public static readonly SfxCue AlchemistLaugh = new SfxCue("Alchemist_Laugh", 0.7f, pitchJitter: 0.03f, cooldown: 0.5f);
        public static readonly SfxCue AlchemistGlide = new SfxCue("Alchemist_Glide", 0.35f, pitchJitter: 0.06f, cooldown: 0.5f);
        public static readonly SfxCue AlchemistBellCall = new SfxCue("Alchemist_BellCall", 0.5f, pitchJitter: 0.02f);
        public static readonly SfxCue AlchemistDistill = new SfxCue("Alchemist_Distill", 0.6f);
        public static readonly SfxCue AlchemistCondense = new SfxCue("Alchemist_Condense", 0.6f);
        public static readonly SfxCue AlchemistWhipWindup = new SfxCue("Alchemist_WhipWindup", 0.5f);
        public static readonly SfxCue AlchemistWhip = new SfxCue("Alchemist_Whip", 0.65f);
        public static readonly SfxCue AlchemistPour = new SfxCue("Alchemist_Pour", 0.5f, cooldown: 0.3f);
        public static readonly SfxCue AlchemistGeyserWarn = new SfxCue("Geyser_Warn", 0.4f, pitchJitter: 0.06f, cooldown: 0.1f, pitch: 1.3f);
        public static readonly SfxCue AlchemistGeyserBurst = new SfxCue("Alchemist_GeyserBurst", 0.6f, pitchJitter: 0.06f, cooldown: 0.1f);
        public static readonly SfxCue AlchemistTransmute = new SfxCue("Alchemist_Transmute", 0.7f, pitchJitter: 0f, cooldown: 0.5f);
        public static readonly SfxCue AlchemistCracking = new SfxCue("Crystal_Crack", 0.55f, pitchJitter: 0.04f, pitch: 0.7f);
        public static readonly SfxCue AlchemistBurst = new SfxCue("Alchemist_Burst", 0.75f, pitchJitter: 0.03f);
        public static readonly SfxCue AlchemistSplash = new SfxCue("Mercury_Splat", 0.45f, pitchJitter: 0.1f, cooldown: 0.08f, pitch: 0.8f);
        public static readonly SfxCue AlchemistFlood = new SfxCue("Alchemist_Flood", 0.85f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue AlchemistDrain = new SfxCue("Alchemist_Drain", 0.8f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue JarCreak = new SfxCue("Jar_Creak", 0.5f);
        public static readonly SfxCue JarDrop = new SfxCue("Jar_Drop", 0.75f);
        /// <summary>O cinzel na campanula: cada rachadura sobe o tom.</summary>
        public static readonly SfxCue JarHit = new SfxCue("Jar_Hit", 0.6f, pitchJitter: 0.03f);
        public static readonly SfxCue JarShatter = new SfxCue("Jar_Shatter", 0.75f, pitchJitter: 0.04f);
        public static readonly SfxCue JarSuffocate = new SfxCue("Jar_Suffocate", 0.55f, pitchJitter: 0.03f, cooldown: 0.5f);
        public static readonly SfxCue BeadLand = new SfxCue("Mercury_Splat", 0.25f, pitchJitter: 0.15f, cooldown: 0.07f, pitch: 1.6f);
        public static readonly SfxCue RunePolished = new SfxCue("Rune_Polished", 0.6f, pitchJitter: 0f, cooldown: 0.3f);

        // O Rei de Prata (fase 5)
        public static readonly SfxCue KingWake = new SfxCue("King_Wake", 0.75f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue KingScepterPlant = new SfxCue("King_ScepterPlant", 0.75f, pitchJitter: 0f);
        public static readonly SfxCue KingArmor = new SfxCue("King_Armor", 0.4f, pitchJitter: 0.06f, cooldown: 0.15f);
        public static readonly SfxCue KingStep = new SfxCue("King_Step", 0.5f, variants: 2, pitchJitter: 0.05f, cooldown: 0.12f);
        public static readonly SfxCue KingPoint = new SfxCue("King_Point", 0.6f, pitchJitter: 0f);
        public static readonly SfxCue KingSweepWindup = new SfxCue("King_SweepWindup", 0.5f);
        public static readonly SfxCue KingSweep = new SfxCue("King_Sweep", 0.7f);
        /// <summary>O fio de mira e o raio aceso: em loop, tocados pelo proprio KingBeam.</summary>
        public static readonly SfxCue KingBeamAim = new SfxCue("King_BeamAim", 0.25f);
        public static readonly SfxCue KingBeamLoop = new SfxCue("King_BeamLoop", 0.5f);
        public static readonly SfxCue KingBeamFire = new SfxCue("King_BeamFire", 0.65f, cooldown: 0.1f);
        public static readonly SfxCue KingSkyCharge = new SfxCue("King_SkyCharge", 0.55f);
        public static readonly SfxCue KingSkyLance = new SfxCue("King_SkyLance", 0.85f);
        public static readonly SfxCue KingCast = new SfxCue("King_Cast", 0.5f, pitchJitter: 0.02f);
        public static readonly SfxCue FacetLaunch = new SfxCue("Facet_Launch", 0.45f, pitchJitter: 0.06f, cooldown: 0.1f);
        public static readonly SfxCue FacetBounce = new SfxCue("Facet_Bounce", 0.3f, pitchJitter: 0.15f, cooldown: 0.05f);
        public static readonly SfxCue FacetGlint = new SfxCue("Facet_Glint", 0.45f, pitchJitter: 0.04f, cooldown: 0.3f);
        public static readonly SfxCue FacetRain = new SfxCue("Facet_Rain", 0.6f, pitchJitter: 0.05f, cooldown: 0.3f);
        public static readonly SfxCue KingInhale = new SfxCue("King_Inhale", 0.65f, pitchJitter: 0f);
        public static readonly SfxCue KingBreath = new SfxCue("King_Breath", 0.85f, pitchJitter: 0.02f);
        public static readonly SfxCue KingGlide = new SfxCue("King_Glide", 0.7f);
        public static readonly SfxCue KingCrownCrack = new SfxCue("King_CrownCrack", 0.9f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue KingShatter = new SfxCue("King_Shatter", 0.9f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue KingRise = new SfxCue("King_Rise", 0.9f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue KingFall = new SfxCue("King_Fall", 0.9f, pitchJitter: 0f, cooldown: 2f);
        public static readonly SfxCue KingLieDown = new SfxCue("King_LieDown", 0.7f, pitchJitter: 0f, cooldown: 1f);

        // ------------------------------------------------------------------
        // Cutscenes, projeteis, perigos e interface
        // ------------------------------------------------------------------

        public static readonly SfxCue EyeFlicker = new SfxCue("Scrap_EyeFlicker", 0.35f, pitchJitter: 0.1f, cooldown: 0.05f);
        public static readonly SfxCue ScrapBoot = new SfxCue("Scrap_Boot", 0.55f, pitchJitter: 0f, cooldown: 1f);
        /// <summary>A abertura: algo descendo chacoalhando pela Calha N.º 1, cada vez mais perto.</summary>
        public static readonly SfxCue IntroChute = new SfxCue("Intro_ChuteRattle", 0.7f, pitchJitter: 0f, cooldown: 1f);
        /// <summary>A abertura: o Scrap se esborrachando no monte de sucata.</summary>
        public static readonly SfxCue IntroCrash = new SfxCue("Intro_Crash", 0.95f, pitchJitter: 0f, cooldown: 1f);
        public static readonly SfxCue CartoonShot = new SfxCue("Cartoon_Shot", 0.35f, pitchJitter: 0.04f, cooldown: 0.2f, ui: true);
        public static readonly SfxCue CartoonReveal = new SfxCue("Cartoon_Reveal", 0.5f, pitchJitter: 0f, cooldown: 0.2f, ui: true);
        public static readonly SfxCue CartoonFlash = new SfxCue("Cartoon_Flash", 0.65f, pitchJitter: 0f, cooldown: 1f, ui: true);
        /// <summary>Segurando E no Altar: em loop, tocado pelo proprio HeartChoice, subindo de tom.</summary>
        public static readonly SfxCue HeartHold = new SfxCue("Heart_HoldLoop", 0.4f);
        public static readonly SfxCue HeartGive = new SfxCue("Heart_Give", 0.7f, pitchJitter: 0f, cooldown: 1f, ui: true);
        public static readonly SfxCue FinaleLight = new SfxCue("Finale_Light", 0.75f, pitchJitter: 0f, cooldown: 1f, ui: true);
        public static readonly SfxCue SteamBoltHit = new SfxCue("SteamBolt_Hit", 0.4f, pitchJitter: 0.08f, cooldown: 0.06f);
        public static readonly SfxCue ShardShotHit = new SfxCue("Shard_Land", 0.4f, pitchJitter: 0.1f, cooldown: 0.06f, pitch: 1.3f);
        public static readonly SfxCue SpikeHit = new SfxCue("Spike_Hit", 0.45f, pitchJitter: 0.06f, cooldown: 0.5f);
        public static readonly SfxCue SignOpen = new SfxCue("Ui_SignOpen", 0.22f, pitchJitter: 0f, cooldown: 0.2f, ui: true);
        public static readonly SfxCue SignClose = new SfxCue("Ui_SignClose", 0.18f, pitchJitter: 0f, cooldown: 0.2f, ui: true);
        public static readonly SfxCue CeilingDrip = new SfxCue("Mercury_Drip", 0.15f, pitchJitter: 0.12f, cooldown: 0.1f, pitch: 1.5f);
        public static readonly SfxCue StageAppear = new SfxCue("Stage_Appear", 0.6f, pitchJitter: 0f, cooldown: 1f);

        // A Bancada de Ourives: salvar e as tres marteladas, a gema acendendo e a gravura.
        public static readonly SfxCue BenchStrike = new SfxCue("Bench_Strike", 0.6f, pitchJitter: 0.01f, cooldown: 0.1f);
        public static readonly SfxCue BenchIgnite = new SfxCue("Bench_Ignite", 0.75f, pitchJitter: 0f, cooldown: 0.5f);
        public static readonly SfxCue EngraveEtch = new SfxCue("Engrave_Etch", 0.4f, pitchJitter: 0.03f, cooldown: 0.5f, ui: true);
        public static readonly SfxCue EngraveStamp = new SfxCue("Engrave_Stamp", 0.5f, pitchJitter: 0.02f, cooldown: 0.5f, ui: true);

        // Os cristais tocam uma escala pentatonica: qualquer ordem soa bem, e cada nota se distingue.
        private static readonly int[] Pentatonic = { 0, 2, 4, 7, 9, 12, 14, 16, 19, 21 };

        /// <summary>O tom do arquivo da nota pra um cristal de nota <paramref name="note"/>.</summary>
        public static float CrystalNotePitch(int note)
        {
            int step = Pentatonic[((note % Pentatonic.Length) + Pentatonic.Length) % Pentatonic.Length];
            return Mathf.Pow(2f, step / 12f);
        }
    }
}
