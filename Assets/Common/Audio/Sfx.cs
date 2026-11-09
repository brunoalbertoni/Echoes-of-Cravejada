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
