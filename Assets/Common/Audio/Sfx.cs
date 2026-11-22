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
        public readonly string Clipe;

        /// <summary>Quantas variacoes o arquivo tem (Clip_1 ... Clip_N). 1: so o Clip.</summary>
        public readonly int Variacoes;

        /// <summary>Volume dele em relacao aos outros (0 a 1).</summary>
        public readonly float Volume;

        /// <summary>Quanto o tom varia pra cima e pra baixo a cada vez (0,05 = 5%).</summary>
        public readonly float VariacaoTom;

        /// <summary>Segundos minimos entre duas vezes: o mesmo som no mesmo quadro nao dobra.</summary>
        public readonly float Recarga;

        /// <summary>Som de interface ou de coleta: o aviso do topo da tela nao tilinta junto com ele.</summary>
        public readonly bool Interface;

        /// <summary>O tom dele (1 = o do arquivo). Deixa um arquivo servir a dois bichos: o pequeno, mais agudo.</summary>
        public readonly float Tom;

        public SfxCue(string clipe, float volume, int variacoes = 1, float variacaoTom = 0.04f, float recarga = 0.04f,
            bool ui = false, float tom2 = 1f)
        {
            Clipe = clipe;
            Volume = volume;
            Variacoes = Mathf.Max(1, variacoes);
            VariacaoTom = variacaoTom;
            Recarga = recarga;
            Interface = ui;
            Tom = tom2;
        }

        /// <summary>
        /// Toca. Sem <paramref name="at"/> o som e da tela (do Scrap, da
        /// interface); com, ele vem daquele ponto do mapa e some com a distancia.
        /// </summary>
        public void Tocar(Vector2? at = null, float volume = 1f, float tom2 = 1f, float atraso = 0f)
        {
            SfxPlayer.Tocar(this, at, volume, tom2, atraso);
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

        public static readonly SfxCue Passo2 = new SfxCue("Scrap_Step", 0.32f, variacoes: 4, variacaoTom: 0.07f, recarga: 0.1f);
        public static readonly SfxCue Pulo = new SfxCue("Scrap_Jump", 0.45f, variacoes: 2, recarga: 0.08f);
        public static readonly SfxCue PuloParede = new SfxCue("Scrap_WallJump", 0.45f, recarga: 0.08f);
        public static readonly SfxCue AgarradaParede = new SfxCue("Scrap_WallGrab", 0.4f, recarga: 0.3f);
        public static readonly SfxCue PuloPressao = new SfxCue("Scrap_PressureJump", 0.5f, recarga: 0.08f);
        public static readonly SfxCue Pousar = new SfxCue("Scrap_Land", 0.5f, variacoes: 2, variacaoTom: 0.06f, recarga: 0.12f);
        public static readonly SfxCue PesadoPousar = new SfxCue("Scrap_LandHeavy", 0.7f, recarga: 0.12f);

        public static readonly SfxCue Balanco = new SfxCue("Scrap_Swing", 0.5f, variacoes: 3, variacaoTom: 0.06f);
        public static readonly SfxCue Golpe = new SfxCue("Scrap_Hit", 0.75f, variacoes: 3, variacaoTom: 0.06f);
        public static readonly SfxCue ProntoCarga = new SfxCue("Scrap_ChargeReady", 0.35f, variacaoTom: 0f, recarga: 0.2f);
        public static readonly SfxCue Ressonancia = new SfxCue("Scrap_Resonance", 0.8f, variacaoTom: 0.02f, recarga: 0.2f);
        public static readonly SfxCue QuadroDash = new SfxCue("Scrap_Dash", 0.55f, variacaoTom: 0.05f, recarga: 0.1f);

        public static readonly SfxCue Ferido = new SfxCue("Scrap_Hurt", 0.75f, variacaoTom: 0.05f, recarga: 0.2f);
        public static readonly SfxCue QuadroMorte = new SfxCue("Scrap_Death", 0.85f, variacaoTom: 0f, recarga: 0.5f);
        public static readonly SfxCue Renascimento = new SfxCue("Scrap_Respawn", 0.5f, variacaoTom: 0f, recarga: 0.5f);

        /// <summary>A lixa raspando: em loop, tocado pelo ScrapSounds enquanto ele raspa.</summary>
        public static readonly SfxCue RepeticaoRaspagem = new SfxCue("Scrap_ScrapeLoop", 0.35f);
        public static readonly SfxCue ProntoRaspagem = new SfxCue("Scrap_ScrapeDone", 0.5f, variacaoTom: 0f, recarga: 0.3f);

        // ------------------------------------------------------------------
        // Coleta
        // ------------------------------------------------------------------

        public static readonly SfxCue Fragmento = new SfxCue("Pickup_Shard", 0.35f, variacaoTom: 0f, recarga: 0.03f, ui: true);
        public static readonly SfxCue RecuperadosFragmentos = new SfxCue("Pickup_ShardsRecovered", 0.5f, variacaoTom: 0f, recarga: 0.3f, ui: true);
        public static readonly SfxCue Gema = new SfxCue("Pickup_Gem", 0.45f, variacaoTom: 0f, recarga: 0.3f, ui: true);
        public static readonly SfxCue ItemChave = new SfxCue("Pickup_KeyItem", 0.55f, variacaoTom: 0f, recarga: 0.3f, ui: true);
        public static readonly SfxCue Chassi = new SfxCue("Pickup_Chassis", 0.6f, variacaoTom: 0f, recarga: 0.3f, ui: true);

        // ------------------------------------------------------------------
        // Interface
        // ------------------------------------------------------------------

        public static readonly SfxCue ObtidoHabilidade = new SfxCue("Ui_AbilityGet", 0.7f, variacaoTom: 0f, recarga: 0.5f, ui: true);
        public static readonly SfxCue Salvar2 = new SfxCue("Ui_Save", 0.5f, variacaoTom: 0f, recarga: 1f, ui: true);
        public static readonly SfxCue EncaixeGema = new SfxCue("Ui_GemSocket", 0.5f, variacaoTom: 0f, recarga: 0.5f, ui: true);
        public static readonly SfxCue AbertoDialogo = new SfxCue("Ui_DialogueOpen", 0.25f, variacaoTom: 0.02f, ui: true);
        public static readonly SfxCue ProximoDialogo = new SfxCue("Ui_DialogueNext", 0.18f, variacaoTom: 0.03f, ui: true);
        public static readonly SfxCue FecharDialogo = new SfxCue("Ui_DialogueClose", 0.2f, variacaoTom: 0.02f, ui: true);
        public static readonly SfxCue MoverLoja = new SfxCue("Ui_ShopMove", 0.2f, variacaoTom: 0.02f, recarga: 0.02f, ui: true);
        public static readonly SfxCue ComprarLoja = new SfxCue("Ui_ShopBuy", 0.45f, variacaoTom: 0f, ui: true);
        public static readonly SfxCue NegarLoja = new SfxCue("Ui_ShopDeny", 0.22f, variacaoTom: 0f, recarga: 0.15f, ui: true);
        public static readonly SfxCue Aviso = new SfxCue("Ui_Notice", 0.25f, variacaoTom: 0f, recarga: 1f, ui: true);

        // ------------------------------------------------------------------
        // Inimigos (tocam de onde estao: os longe soam baixo)
        // ------------------------------------------------------------------

        /// <summary>O golpe aparado: o Soldador de frente, o Sentinela em prata.</summary>
        public static readonly SfxCue QuadroBloqueio = new SfxCue("Enemy_Block", 0.6f, variacaoTom: 0.06f);

        // As mortes. Cada inimigo escolhe a sua no Damageable; sem escolha, a de metal.
        public static readonly SfxCue MetalMorte = new SfxCue("Enemy_DeathMetal", 0.55f, variacaoTom: 0.06f);
        public static readonly SfxCue CacadorMorte = new SfxCue("Enemy_DeathMetal", 0.6f, variacaoTom: 0.04f, tom2: 0.8f);
        public static readonly SfxCue VaporMorte = new SfxCue("Enemy_DeathSteam", 0.55f, variacaoTom: 0.05f);
        public static readonly SfxCue AcaroMorte = new SfxCue("Enemy_DeathCrystal", 0.5f, variacaoTom: 0.08f, tom2: 1.35f);
        public static readonly SfxCue SentinelaMorte = new SfxCue("Enemy_DeathCrystal", 0.65f, variacaoTom: 0.04f, tom2: 0.75f);
        public static readonly SfxCue VidroMorte = new SfxCue("Enemy_DeathGlass", 0.65f, variacaoTom: 0.04f);
        public static readonly SfxCue MurcharMorte = new SfxCue("Enemy_DeathDeflate", 0.5f, variacaoTom: 0.06f);
        public static readonly SfxCue GosmaMorte = new SfxCue("Enemy_DeathGoo", 0.5f, variacaoTom: 0.08f, tom2: 1.2f);

        public static readonly SfxCue PreparoRastejante = new SfxCue("Crawler_Windup", 0.5f, variacaoTom: 0.06f);
        public static readonly SfxCue MordidaRastejante = new SfxCue("Crawler_Bite", 0.5f, variacaoTom: 0.06f);

        public static readonly SfxCue PreparoInflado = new SfxCue("Bloater_Windup", 0.35f, variacaoTom: 0.05f);
        public static readonly SfxCue JatoInflado = new SfxCue("Bloater_Spray", 0.45f, variacaoTom: 0.05f);
        public static readonly SfxCue IncharInflado = new SfxCue("Bloater_Swell", 0.45f, variacaoTom: 0.04f);
        public static readonly SfxCue RajadaInflado = new SfxCue("Bloater_Burst", 0.7f, variacaoTom: 0.04f);

        public static readonly SfxCue PreparoSoldador = new SfxCue("Welder_Windup", 0.4f, variacaoTom: 0.05f);
        public static readonly SfxCue ChamaSoldador = new SfxCue("Welder_Flame", 0.5f, variacaoTom: 0.05f);

        public static readonly SfxCue QuedaSanguessuga = new SfxCue("Leech_Drop", 0.45f, variacaoTom: 0.06f);
        public static readonly SfxCue PousarSanguessuga = new SfxCue("Leech_Land", 0.45f, variacaoTom: 0.06f);
        public static readonly SfxCue PreparoSanguessuga = new SfxCue("Leech_Windup", 0.3f, variacaoTom: 0.06f);
        public static readonly SfxCue MordidaSanguessuga = new SfxCue("Leech_Bite", 0.45f, variacaoTom: 0.06f);

        public static readonly SfxCue CargaGuarda = new SfxCue("Sentry_Charge", 0.35f, variacaoTom: 0.03f);
        public static readonly SfxCue DisparoGuarda = new SfxCue("Sentry_Fire", 0.5f, variacaoTom: 0.05f);

        public static readonly SfxCue DespertarAcaro = new SfxCue("Mite_Wake", 0.4f, variacaoTom: 0.1f, recarga: 0.08f);
        public static readonly SfxCue AlertaCacador = new SfxCue("Hunter_Alert", 0.5f, variacaoTom: 0.04f, recarga: 0.2f);
        public static readonly SfxCue AbertoSentinela = new SfxCue("Sentinel_Open", 0.35f, variacaoTom: 0.04f);
        public static readonly SfxCue DisparoSentinela = new SfxCue("Sentinel_Fire", 0.45f, variacaoTom: 0.05f);

        public static readonly SfxCue CargaExperimento = new SfxCue("Experiment_Charge", 0.4f, variacaoTom: 0.05f);
        public static readonly SfxCue BoteExperimento = new SfxCue("Experiment_Lunge", 0.45f, variacaoTom: 0.05f);
        public static readonly SfxCue PreparoArremessador = new SfxCue("Thrower_Windup", 0.35f, variacaoTom: 0.06f);
        public static readonly SfxCue ArremessoArremessador = new SfxCue("Thrower_Throw", 0.4f, variacaoTom: 0.06f);
        public static readonly SfxCue QuebraFrasco = new SfxCue("Flask_Break", 0.55f, variacaoTom: 0.06f);
        public static readonly SfxCue DivisaoGosma = new SfxCue("Slime_Split", 0.55f, variacaoTom: 0.05f);
        public static readonly SfxCue Transmutar = new SfxCue("Glass_Transmute", 0.45f, variacaoTom: 0.03f, recarga: 0.15f);

        // ------------------------------------------------------------------
        // Mecanicas das fases
        // ------------------------------------------------------------------

        public static readonly SfxCue AbertoPortao = new SfxCue("Gate_Open", 0.6f, variacaoTom: 0.04f, recarga: 0.1f);
        public static readonly SfxCue GiroValvula = new SfxCue("Valve_Turn", 0.55f, variacaoTom: 0.03f);
        public static readonly SfxCue DrenoAcido = new SfxCue("Acid_Drain", 0.55f, variacaoTom: 0.04f, recarga: 0.2f);
        public static readonly SfxCue ChiarAcido = new SfxCue("Acid_Sizzle", 0.5f, variacaoTom: 0.06f, recarga: 0.7f);
        public static readonly SfxCue AlarmeAcido = new SfxCue("Acid_Alarm", 0.45f, variacaoTom: 0f, recarga: 2f);
        public static readonly SfxCue PressaoCaldeira = new SfxCue("Boiler_Pressure", 0.6f, variacaoTom: 0.02f);
        public static readonly SfxCue PavioCaldeira = new SfxCue("Boiler_Fuse", 0.5f, variacaoTom: 0f);
        public static readonly SfxCue ExplosaoCaldeira = new SfxCue("Boiler_Explode", 0.85f, variacaoTom: 0.03f);
        public static readonly SfxCue RecarregarVapor = new SfxCue("Steam_Refresh", 0.3f, variacaoTom: 0.05f, recarga: 0.4f);

        public static readonly SfxCue RachaduraCristal = new SfxCue("Crystal_Crack", 0.45f, variacaoTom: 0.06f);
        public static readonly SfxCue QuebraCristal = new SfxCue("Crystal_Break", 0.5f, variacaoTom: 0.06f);
        public static readonly SfxCue EstilhacoBarreira = new SfxCue("Crystal_BarrierShatter", 0.85f, variacaoTom: 0.03f);

        /// <summary>A nota de um cristal ressonante. O tom de cada cristal sai de <see cref="TomDaNotaDoCristal"/>.</summary>
        public static readonly SfxCue NotaCristal = new SfxCue("Crystal_Note", 0.55f, variacaoTom: 0f, recarga: 0f);
        public static readonly SfxCue OpacoCristal = new SfxCue("Crystal_Dull", 0.5f, variacaoTom: 0.05f);

        public static readonly SfxCue ApertarEstatua = new SfxCue("Statue_Press", 0.6f, variacaoTom: 0.03f, recarga: 0.15f);
        public static readonly SfxCue DireitaEnigma = new SfxCue("Puzzle_Right", 0.35f, variacaoTom: 0f, recarga: 0.1f);
        public static readonly SfxCue ErradoEnigma = new SfxCue("Puzzle_Wrong", 0.4f, variacaoTom: 0f, recarga: 0.2f);
        public static readonly SfxCue ResolvidoEnigma = new SfxCue("Puzzle_Solved", 0.5f, variacaoTom: 0f, recarga: 0.5f);
        public static readonly SfxCue ChocalharTrava = new SfxCue("Lock_Rattle", 0.45f, variacaoTom: 0.05f, recarga: 0.5f);
        public static readonly SfxCue DesbloquearTrava = new SfxCue("Lock_Unlock", 0.6f, variacaoTom: 0.03f);

        public static readonly SfxCue GotejarMercurio = new SfxCue("Mercury_Drip", 0.35f, variacaoTom: 0.12f, recarga: 0.1f);
        public static readonly SfxCue EsparramarMercurio = new SfxCue("Mercury_Splat", 0.5f, variacaoTom: 0.06f, recarga: 0.2f);
        public static readonly SfxCue PassoMercurio = new SfxCue("Mercury_Step", 0.32f, variacoes: 2, variacaoTom: 0.08f, recarga: 0.1f);
        public static readonly SfxCue GiroEspelho = new SfxCue("Mirror_Turn", 0.5f, variacaoTom: 0.05f, recarga: 0.1f);
        public static readonly SfxCue AbertoTravaCristal = new SfxCue("CrystalLock_Open", 0.55f, variacaoTom: 0f);
        public static readonly SfxCue InserirAlambique = new SfxCue("Alembic_Insert", 0.5f, variacaoTom: 0.03f);
        public static readonly SfxCue FerverAlambique = new SfxCue("Alembic_Boil", 0.55f, variacaoTom: 0f);
        public static readonly SfxCue AtivarCirculo = new SfxCue("Circle_Activate", 0.6f, variacaoTom: 0.02f);

        public static readonly SfxCue InicioElevador = new SfxCue("Elevator_Start", 0.6f, variacaoTom: 0f);
        /// <summary>O motor do elevador: em loop, tocado pelo proprio ElevatorRide durante a subida.</summary>
        public static readonly SfxCue RepeticaoElevador = new SfxCue("Elevator_Loop", 0.4f);

        /// <summary>O zumbido dos raios de luz da fase 4: em loop, preso a cada emissor pelo SoundDirector.</summary>
        public static readonly SfxCue ZumbidoRaio = new SfxCue("Beam_Hum", 0.3f);

        public static readonly SfxCue OndaPurificar = new SfxCue("Purify_Wave", 0.55f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue SolavancoOxido = new SfxCue("Oxide_Bump", 0.4f, variacaoTom: 0.05f, recarga: 0.5f);

        // ------------------------------------------------------------------
        // Chefes: o que e de todos
        // ------------------------------------------------------------------

        /// <summary>Um jato de Oxide na queda. O BossFall sobe o tom a cada um.</summary>
        public static readonly SfxCue JatoQuedaChefe = new SfxCue("Boss_FallJet", 0.6f, variacaoTom: 0.04f, recarga: 0.08f);
        public static readonly SfxCue RajadaQuedaChefe = new SfxCue("Boss_FallBurst", 1f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue PancadaChefe = new SfxCue("Boss_Slam", 0.85f, variacaoTom: 0.04f, recarga: 0.1f);
        public static readonly SfxCue PassoChefe = new SfxCue("Boss_Step", 0.45f, variacoes: 2, variacaoTom: 0.05f, recarga: 0.12f);
        public static readonly SfxCue MetalFeridoChefe = new SfxCue("Boss_HurtMetal", 0.55f, variacaoTom: 0.06f, recarga: 0.08f);
        public static readonly SfxCue PrataFeridoChefe = new SfxCue("Boss_HurtSilver", 0.55f, variacaoTom: 0.05f, recarga: 0.08f);
        public static readonly SfxCue PuloChefe = new SfxCue("Boss_Jump", 0.6f, variacaoTom: 0.04f);
        public static readonly SfxCue GolpeParedeChefe = new SfxCue("Boss_WallHit", 0.8f, variacaoTom: 0.04f, recarga: 0.2f);
        public static readonly SfxCue BaixoSeloArena = new SfxCue("Arena_SealDown", 0.7f, variacaoTom: 0.03f, recarga: 0.3f);
        public static readonly SfxCue CimaSeloArena = new SfxCue("Arena_SealUp", 0.6f, variacaoTom: 0.03f, recarga: 0.3f);
        public static readonly SfxCue OndaChoque = new SfxCue("Shock_Wave", 0.5f, variacaoTom: 0.05f, recarga: 0.1f);
        public static readonly SfxCue PousarPedaco = new SfxCue("Chunk_Land", 0.45f, variacaoTom: 0.1f, recarga: 0.05f);
        public static readonly SfxCue LustroPolimento = new SfxCue("Polish_Shine", 0.6f, variacaoTom: 0f, recarga: 1f, ui: true);

        // O Coletor (fase 1)
        public static readonly SfxCue DespertarColetor = new SfxCue("Collector_Wake", 0.75f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue RugidoColetor = new SfxCue("Collector_Roar", 0.85f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue PreparoGarraColetor = new SfxCue("Collector_ClawWindup", 0.5f);
        public static readonly SfxCue GolpeGarraColetor = new SfxCue("Collector_ClawStrike", 0.6f);
        public static readonly SfxCue PreparoArremessoColetor = new SfxCue("Collector_ThrowWindup", 0.5f);
        public static readonly SfxCue ArremessoColetor = new SfxCue("Collector_Throw", 0.55f);

        // O Mestre das Caldeiras (fase 2)
        public static readonly SfxCue DespertarMestreCaldeira = new SfxCue("BoilerMaster_Wake", 0.75f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue SobrepressaoMestreCaldeira = new SfxCue("BoilerMaster_Overpressure", 0.85f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue PreparoMarteloMestreCaldeira = new SfxCue("BoilerMaster_HammerWindup", 0.5f);
        public static readonly SfxCue PreparoCargaMestreCaldeira = new SfxCue("BoilerMaster_ChargeWindup", 0.55f);
        public static readonly SfxCue CorridaCargaMestreCaldeira = new SfxCue("BoilerMaster_ChargeRun", 0.6f, variacaoTom: 0.02f);
        public static readonly SfxCue PreparoRajadaMestreCaldeira = new SfxCue("BoilerMaster_BurstWindup", 0.5f);
        public static readonly SfxCue DisparoRajadaMestreCaldeira = new SfxCue("BoilerMaster_BurstFire", 0.5f, variacaoTom: 0.08f, recarga: 0.06f);
        public static readonly SfxCue AlertaGeiser = new SfxCue("Geyser_Warn", 0.45f, variacaoTom: 0.06f, recarga: 0.1f);
        public static readonly SfxCue RajadaGeiser = new SfxCue("Geyser_Burst", 0.6f, variacaoTom: 0.05f, recarga: 0.1f);
        public static readonly SfxCue QuedaModulo = new SfxCue("Module_Drop", 0.6f, variacaoTom: 0f, recarga: 0.5f);

        // O Capataz (fase 3): ele caca pelo som, entao os sons dele dizem onde esta e o que vai fazer.
        public static readonly SfxCue DespertarCapataz = new SfxCue("Foreman_Wake", 0.7f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue EscutarCapataz = new SfxCue("Foreman_Listen", 0.35f, variacaoTom: 0.05f, recarga: 2.5f);
        public static readonly SfxCue PassoCapataz = new SfxCue("Foreman_Step", 0.4f, variacoes: 2, variacaoTom: 0.06f, recarga: 0.1f);
        public static readonly SfxCue PreparoPicaretaCapataz = new SfxCue("Foreman_PickWindup", 0.6f);
        public static readonly SfxCue PancadaPicaretaCapataz = new SfxCue("Foreman_PickSlam", 0.8f);
        public static readonly SfxCue PresoPicaretaCapataz = new SfxCue("Foreman_PickStuck", 0.5f);
        public static readonly SfxCue PreparoInvestidaCapataz = new SfxCue("Foreman_RamWindup", 0.6f);
        public static readonly SfxCue ParedeInvestidaCapataz = new SfxCue("Foreman_RamWall", 0.9f, variacaoTom: 0.03f);
        public static readonly SfxCue ApitoCapataz = new SfxCue("Foreman_Whistle", 0.6f, variacaoTom: 0.02f, recarga: 0.5f);
        public static readonly SfxCue GritoCapataz = new SfxCue("Foreman_Scream", 0.9f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue AlertaCapataz = new SfxCue("Foreman_Warn", 0.4f, variacaoTom: 0.03f);
        public static readonly SfxCue EclosaoCapataz = new SfxCue("Foreman_Hatch", 0.5f, variacaoTom: 0.08f, recarga: 0.1f);
        public static readonly SfxCue FeridoCapataz = new SfxCue("Foreman_Hurt", 0.6f, variacoes: 2, variacaoTom: 0.06f, recarga: 0.1f);
        public static readonly SfxCue MorteCapataz = new SfxCue("Foreman_Death", 0.9f, variacaoTom: 0f, recarga: 2f);
        public static readonly SfxCue SoltarFragmento = new SfxCue("Shard_Loosen", 0.3f, variacaoTom: 0.12f, recarga: 0.08f);
        public static readonly SfxCue PousarFragmento = new SfxCue("Shard_Land", 0.45f, variacaoTom: 0.1f, recarga: 0.06f);
        public static readonly SfxCue EstaloPedrinha = new SfxCue("Pebble_Pop", 0.25f, variacaoTom: 0.15f, recarga: 0.07f);

        // A Broca (fase 3)
        /// <summary>O motor: em loop, preso a broca pelo proprio GeodeDrill.</summary>
        public static readonly SfxCue MotorBroca = new SfxCue("Drill_Motor", 0.45f);
        public static readonly SfxCue EmergirBroca = new SfxCue("Drill_Emerge", 0.7f, variacaoTom: 0.04f);
        public static readonly SfxCue TremorBroca = new SfxCue("Drill_Rumble", 0.6f, variacaoTom: 0.05f);
        public static readonly SfxCue ErupcaoBroca = new SfxCue("Drill_Erupt", 0.85f, variacaoTom: 0.04f);
        public static readonly SfxCue RachaduraPilarBroca = new SfxCue("Drill_PillarCrack", 0.6f, variacaoTom: 0.04f);
        public static readonly SfxCue EstilhacoPilar = new SfxCue("Crystal_Break", 0.75f, variacaoTom: 0.04f, tom2: 0.8f);
        public static readonly SfxCue EmpalarBroca = new SfxCue("Drill_Impale", 0.8f, variacaoTom: 0.03f);
        public static readonly SfxCue LivreTremorBroca = new SfxCue("Drill_ShakeFree", 0.75f, variacaoTom: 0.03f);
        public static readonly SfxCue ColapsoBroca = new SfxCue("Drill_Collapse", 1f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue FeridoBroca = new SfxCue("Drill_Hurt", 0.6f, variacaoTom: 0.06f, recarga: 0.08f);
        public static readonly SfxCue MorrerBroca = new SfxCue("Drill_Die", 0.9f, variacaoTom: 0f, recarga: 2f);

        // O Alquimista de Mercurio (fase 4)
        public static readonly SfxCue RisadaAlquimista = new SfxCue("Alchemist_Laugh", 0.7f, variacaoTom: 0.03f, recarga: 0.5f);
        public static readonly SfxCue PlanarAlquimista = new SfxCue("Alchemist_Glide", 0.35f, variacaoTom: 0.06f, recarga: 0.5f);
        public static readonly SfxCue ChamadaSinoAlquimista = new SfxCue("Alchemist_BellCall", 0.5f, variacaoTom: 0.02f);
        public static readonly SfxCue DestilarAlquimista = new SfxCue("Alchemist_Distill", 0.6f);
        public static readonly SfxCue CondensarAlquimista = new SfxCue("Alchemist_Condense", 0.6f);
        public static readonly SfxCue PreparoChicoteAlquimista = new SfxCue("Alchemist_WhipWindup", 0.5f);
        public static readonly SfxCue ChicoteAlquimista = new SfxCue("Alchemist_Whip", 0.65f);
        public static readonly SfxCue DespejarAlquimista = new SfxCue("Alchemist_Pour", 0.5f, recarga: 0.3f);
        public static readonly SfxCue AlertaGeiserAlquimista = new SfxCue("Geyser_Warn", 0.4f, variacaoTom: 0.06f, recarga: 0.1f, tom2: 1.3f);
        public static readonly SfxCue RajadaGeiserAlquimista = new SfxCue("Alchemist_GeyserBurst", 0.6f, variacaoTom: 0.06f, recarga: 0.1f);
        public static readonly SfxCue TransmutarAlquimista = new SfxCue("Alchemist_Transmute", 0.7f, variacaoTom: 0f, recarga: 0.5f);
        public static readonly SfxCue RachandoAlquimista = new SfxCue("Crystal_Crack", 0.55f, variacaoTom: 0.04f, tom2: 0.7f);
        public static readonly SfxCue RajadaAlquimista = new SfxCue("Alchemist_Burst", 0.75f, variacaoTom: 0.03f);
        public static readonly SfxCue RespingoAlquimista = new SfxCue("Mercury_Splat", 0.45f, variacaoTom: 0.1f, recarga: 0.08f, tom2: 0.8f);
        public static readonly SfxCue InundacaoAlquimista = new SfxCue("Alchemist_Flood", 0.85f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue DrenoAlquimista = new SfxCue("Alchemist_Drain", 0.8f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue RangidoRedoma = new SfxCue("Jar_Creak", 0.5f);
        public static readonly SfxCue QuedaRedoma = new SfxCue("Jar_Drop", 0.75f);
        /// <summary>O cinzel na campanula: cada rachadura sobe o tom.</summary>
        public static readonly SfxCue GolpeRedoma = new SfxCue("Jar_Hit", 0.6f, variacaoTom: 0.03f);
        public static readonly SfxCue EstilhacoRedoma = new SfxCue("Jar_Shatter", 0.75f, variacaoTom: 0.04f);
        public static readonly SfxCue SufocarRedoma = new SfxCue("Jar_Suffocate", 0.55f, variacaoTom: 0.03f, recarga: 0.5f);
        public static readonly SfxCue PousarGota = new SfxCue("Mercury_Splat", 0.25f, variacaoTom: 0.15f, recarga: 0.07f, tom2: 1.6f);
        public static readonly SfxCue PolidoRuna = new SfxCue("Rune_Polished", 0.6f, variacaoTom: 0f, recarga: 0.3f);

        // O Rei de Prata (fase 5)
        public static readonly SfxCue DespertarRei = new SfxCue("King_Wake", 0.75f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue FincarCetroRei = new SfxCue("King_ScepterPlant", 0.75f, variacaoTom: 0f);
        public static readonly SfxCue ArmaduraRei = new SfxCue("King_Armor", 0.4f, variacaoTom: 0.06f, recarga: 0.15f);
        public static readonly SfxCue PassoRei = new SfxCue("King_Step", 0.5f, variacoes: 2, variacaoTom: 0.05f, recarga: 0.12f);
        public static readonly SfxCue PontoRei = new SfxCue("King_Point", 0.6f, variacaoTom: 0f);
        public static readonly SfxCue PreparoVarridaRei = new SfxCue("King_SweepWindup", 0.5f);
        public static readonly SfxCue VarridaRei = new SfxCue("King_Sweep", 0.7f);
        /// <summary>O fio de mira e o raio aceso: em loop, tocados pelo proprio KingBeam.</summary>
        public static readonly SfxCue MiraRaioRei = new SfxCue("King_BeamAim", 0.25f);
        public static readonly SfxCue RepeticaoRaioRei = new SfxCue("King_BeamLoop", 0.5f);
        public static readonly SfxCue DisparoRaioRei = new SfxCue("King_BeamFire", 0.65f, recarga: 0.1f);
        public static readonly SfxCue CargaCeuRei = new SfxCue("King_SkyCharge", 0.55f);
        public static readonly SfxCue LancaCeuRei = new SfxCue("King_SkyLance", 0.85f);
        public static readonly SfxCue LancamentoRei = new SfxCue("King_Cast", 0.5f, variacaoTom: 0.02f);
        public static readonly SfxCue LancamentoFaceta = new SfxCue("Facet_Launch", 0.45f, variacaoTom: 0.06f, recarga: 0.1f);
        public static readonly SfxCue QuiqueFaceta = new SfxCue("Facet_Bounce", 0.3f, variacaoTom: 0.15f, recarga: 0.05f);
        public static readonly SfxCue BrilhoFaceta = new SfxCue("Facet_Glint", 0.45f, variacaoTom: 0.04f, recarga: 0.3f);
        public static readonly SfxCue ChuvaFaceta = new SfxCue("Facet_Rain", 0.6f, variacaoTom: 0.05f, recarga: 0.3f);
        public static readonly SfxCue InspirarRei = new SfxCue("King_Inhale", 0.65f, variacaoTom: 0f);
        public static readonly SfxCue SoproRei = new SfxCue("King_Breath", 0.85f, variacaoTom: 0.02f);
        public static readonly SfxCue PlanarRei = new SfxCue("King_Glide", 0.7f);
        public static readonly SfxCue RachaduraCoroaRei = new SfxCue("King_CrownCrack", 0.9f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue EstilhacoRei = new SfxCue("King_Shatter", 0.9f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue SubidaRei = new SfxCue("King_Rise", 0.9f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue QuedaRei = new SfxCue("King_Fall", 0.9f, variacaoTom: 0f, recarga: 2f);
        public static readonly SfxCue BaixoDeitarRei = new SfxCue("King_LieDown", 0.7f, variacaoTom: 0f, recarga: 1f);

        // ------------------------------------------------------------------
        // Cutscenes, projeteis, perigos e interface
        // ------------------------------------------------------------------

        public static readonly SfxCue TremeluzirOlho = new SfxCue("Scrap_EyeFlicker", 0.35f, variacaoTom: 0.1f, recarga: 0.05f);
        public static readonly SfxCue PartidaSucata = new SfxCue("Scrap_Boot", 0.55f, variacaoTom: 0f, recarga: 1f);
        /// <summary>A abertura: algo descendo chacoalhando pela Calha N.º 1, cada vez mais perto.</summary>
        public static readonly SfxCue CalhaIntroducao = new SfxCue("Intro_ChuteRattle", 0.7f, variacaoTom: 0f, recarga: 1f);
        /// <summary>A abertura: o Scrap se esborrachando no monte de sucata.</summary>
        public static readonly SfxCue ImpactoIntroducao = new SfxCue("Intro_Crash", 0.95f, variacaoTom: 0f, recarga: 1f);
        public static readonly SfxCue PlanoDesenho = new SfxCue("Cartoon_Shot", 0.35f, variacaoTom: 0.04f, recarga: 0.2f, ui: true);
        public static readonly SfxCue RevelarDesenho = new SfxCue("Cartoon_Reveal", 0.5f, variacaoTom: 0f, recarga: 0.2f, ui: true);
        public static readonly SfxCue ClaraoDesenho = new SfxCue("Cartoon_Flash", 0.65f, variacaoTom: 0f, recarga: 1f, ui: true);
        /// <summary>Segurando E no Altar: em loop, tocado pelo proprio HeartChoice, subindo de tom.</summary>
        public static readonly SfxCue SegurarCoracao = new SfxCue("Heart_HoldLoop", 0.4f);
        public static readonly SfxCue DarCoracao = new SfxCue("Heart_Give", 0.7f, variacaoTom: 0f, recarga: 1f, ui: true);
        public static readonly SfxCue LuzFinalJogo = new SfxCue("Finale_Light", 0.75f, variacaoTom: 0f, recarga: 1f, ui: true);
        public static readonly SfxCue GolpeProjetilVapor = new SfxCue("SteamBolt_Hit", 0.4f, variacaoTom: 0.08f, recarga: 0.06f);
        public static readonly SfxCue GolpePlanoFragmento = new SfxCue("Shard_Land", 0.4f, variacaoTom: 0.1f, recarga: 0.06f, tom2: 1.3f);
        public static readonly SfxCue GolpeEspinho = new SfxCue("Spike_Hit", 0.45f, variacaoTom: 0.06f, recarga: 0.5f);
        public static readonly SfxCue AbertoPlaca = new SfxCue("Ui_SignOpen", 0.22f, variacaoTom: 0f, recarga: 0.2f, ui: true);
        public static readonly SfxCue FecharPlaca = new SfxCue("Ui_SignClose", 0.18f, variacaoTom: 0f, recarga: 0.2f, ui: true);
        public static readonly SfxCue GotejarTeto = new SfxCue("Mercury_Drip", 0.15f, variacaoTom: 0.12f, recarga: 0.1f, tom2: 1.5f);
        public static readonly SfxCue AparecerPalco = new SfxCue("Stage_Appear", 0.6f, variacaoTom: 0f, recarga: 1f);

        // A Bancada de Ourives: salvar e as tres marteladas, a gema acendendo e a gravura.
        public static readonly SfxCue GolpeBancada = new SfxCue("Bench_Strike", 0.6f, variacaoTom: 0.01f, recarga: 0.1f);
        public static readonly SfxCue AcenderBancada = new SfxCue("Bench_Ignite", 0.75f, variacaoTom: 0f, recarga: 0.5f);
        public static readonly SfxCue EntalheGravar = new SfxCue("Engrave_Etch", 0.4f, variacaoTom: 0.03f, recarga: 0.5f, ui: true);
        public static readonly SfxCue CarimboGravar = new SfxCue("Engrave_Stamp", 0.5f, variacaoTom: 0.02f, recarga: 0.5f, ui: true);

        // Os cristais tocam uma escala pentatonica: qualquer ordem soa bem, e cada nota se distingue.
        private static readonly int[] Pentatonica = { 0, 2, 4, 7, 9, 12, 14, 16, 19, 21 };

        /// <summary>O tom do arquivo da nota pra um cristal de nota <paramref name="nota"/>.</summary>
        public static float TomDaNotaDoCristal(int nota)
        {
            int passo = Pentatonica[((nota % Pentatonica.Length) + Pentatonica.Length) % Pentatonica.Length];
            return Mathf.Pow(2f, passo / 12f);
        }
    }
}
