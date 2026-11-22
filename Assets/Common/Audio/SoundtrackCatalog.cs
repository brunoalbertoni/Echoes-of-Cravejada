using System.Collections.Generic;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma musica da trilha: o arquivo (em Resources/Soundtrack/Music) e por
    /// onde ela passa. Os tempos sao em segundos.
    /// </summary>
    public sealed class MusicTrack
    {
        /// <summary>Nome do arquivo, sem extensao.</summary>
        public readonly string Clipe;

        /// <summary>De onde ela comeca a tocar.</summary>
        public readonly float Inicio;

        /// <summary>Pra onde ela volta quando da a volta.</summary>
        public readonly float RepeticaoDe;

        /// <summary>Onde ela da a volta (antes do fim que some). Zero: no fim do arquivo.</summary>
        public readonly float FimRepeticao;

        /// <summary>Falso: toca uma vez e acaba.</summary>
        public readonly bool Repeticao;

        /// <summary>Volume dela em relacao as outras (1 = normal).</summary>
        public readonly float Volume;

        public MusicTrack(string clipe, float inicio = 0f, float repeticaoDe = 0f, float fimRepeticao = 0f, bool repeticao = true, float volume = 1f)
        {
            Clipe = clipe;
            Inicio = inicio;
            RepeticaoDe = repeticaoDe;
            FimRepeticao = fimRepeticao;
            Repeticao = repeticao;
            Volume = volume;
        }
    }

    /// <summary>O som de uma cena: a musica de explorar, o ambiente e o quanto cada um soa ali.</summary>
    public sealed class SoundPlace
    {
        public readonly MusicTrack Explorar;
        public readonly string Ambiente;
        public readonly float EscalaMusica;
        public readonly float EscalaAmbiente;

        public SoundPlace(MusicTrack explorar, string ambiente, float escalaMusica = 1f, float escalaAmbiente = 1f)
        {
            Explorar = explorar;
            Ambiente = ambiente;
            EscalaMusica = escalaMusica;
            EscalaAmbiente = escalaAmbiente;
        }
    }

    /// <summary>
    /// A tabela da trilha sonora: que musica toca em cada lugar e em cada luta,
    /// e o volume de tudo. Mexer aqui e o jeito de ajustar o som do jogo.
    ///
    /// Musicas: Scott Buckley (scottbuckley.com.au), CC BY 4.0. Ambiente: fontes
    /// CC0 do OpenGameArt, mixadas. Creditos em Assets/Common/Audio/CREDITOS.md.
    ///
    /// Os pontos de comeco e de volta foram escolhidos pelo volume de cada
    /// faixa: as de chefe ja comecam onde a energia esta alta.
    /// </summary>
    public static class SoundtrackCatalog
    {
        // ------------------------------------------------------------------
        // Volumes gerais (0 a 1)
        // ------------------------------------------------------------------

        public const float VolumeMusica = 0.5f;
        public const float VolumeAmbiente = 0.8f;
        public const float VolumeEmissor = 0.6f;

        /// <summary>Quanto o ambiente abaixa enquanto toca musica de luta.</summary>
        public const float AmbienteAbaixoBatalha = 0.55f;

        /// <summary>Segundos de cada troca de musica.</summary>
        public const float TempoTransicao = 2.5f;

        /// <summary>Segundos da emenda quando uma musica da a volta.</summary>
        public const float TempoEsmaecerRepeticao = 4f;

        // ------------------------------------------------------------------
        // Exploracao
        // ------------------------------------------------------------------

        public static readonly MusicTrack PatioSucata = new MusicTrack("SignalToNoise", fimRepeticao: 338f);
        public static readonly MusicTrack TubulacoesLatao = new MusicTrack("Horizons", fimRepeticao: 278f);
        public static readonly MusicTrack GeodosSilencioso = new MusicTrack("Decoherence", inicio: 10f, repeticaoDe: 10f, fimRepeticao: 604f);
        public static readonly MusicTrack AteliesAlquimicos = new MusicTrack("Hiraeth", fimRepeticao: 320f);
        public static readonly MusicTrack PalacioPrata = new MusicTrack("PassageOfTime", fimRepeticao: 250f);

        /// <summary>O palacio depois que o Rei cai: a calma voltando, a caminho do Altar.</summary>
        public static readonly MusicTrack PalacioDepoisRei = new MusicTrack("TheRestoration", fimRepeticao: 178f, volume: 1.2f);

        // ------------------------------------------------------------------
        // Chefes
        // ------------------------------------------------------------------

        public static readonly MusicTrack SucateiroEnferrujado = new MusicTrack("Juggernaut", repeticaoDe: 25f, fimRepeticao: 208f);
        public static readonly MusicTrack MestreCaldeira = new MusicTrack("Catalyst", inicio: 15f, repeticaoDe: 15f, fimRepeticao: 258f);
        public static readonly MusicTrack RessonanteO = new MusicTrack("TheEncounter", inicio: 30f, repeticaoDe: 30f, fimRepeticao: 302f);
        public static readonly MusicTrack BrocaGeodo = new MusicTrack("Simulacra", repeticaoDe: 50f, fimRepeticao: 228f);
        public static readonly MusicTrack AlquimistaMercurio = new MusicTrack("HourOfTheWitch", inicio: 68f, repeticaoDe: 68f, fimRepeticao: 198f);
        public static readonly MusicTrack RadianteReiPrata = new MusicTrack("Goliath", repeticaoDe: 45f, fimRepeticao: 368f);
        public static readonly MusicTrack CorrompidaReiPrata = new MusicTrack("EyesInTheVoid", inicio: 48f, repeticaoDe: 48f, fimRepeticao: 232f);

        // ------------------------------------------------------------------
        // Momentos
        // ------------------------------------------------------------------

        /// <summary>A fuga do acido que sobe, no poco da fase 2.</summary>
        public static readonly MusicTrack FugaAcido = new MusicTrack("EscapeVelocity", inicio: 30f, repeticaoDe: 30f, fimRepeticao: 232f);

        /// <summary>A purificacao de um distrito: toca uma vez, ate sair da cena.</summary>
        public static readonly MusicTrack Purificacao = new MusicTrack("HymnToTheDawn", inicio: 18f, repeticao: false);

        /// <summary>O Grande Polimento: a ultima musica do jogo, que segue no cartao de FIM.</summary>
        public static readonly MusicTrack FinalJogo = new MusicTrack("Echoes", inicio: 9f, repeticao: false, volume: 1.1f);

        // ------------------------------------------------------------------
        // Cenas
        // ------------------------------------------------------------------

        private static readonly Dictionary<string, SoundPlace> Lugares = new Dictionary<string, SoundPlace>
        {
            { "Phase01_ScrapYard", new SoundPlace(PatioSucata, "Ambience_Phase01") },
            { "Phase02_BrassPipelines", new SoundPlace(TubulacoesLatao, "Ambience_Phase02") },
            // Nos geodos o barulho do Scrap e mecanica do jogo: musica mais baixa.
            { "Phase03_SilentGeodes", new SoundPlace(GeodosSilencioso, "Ambience_Phase03", escalaMusica: 0.7f, escalaAmbiente: 0.85f) },
            { "Phase04_AlchemicalAteliers", new SoundPlace(AteliesAlquimicos, "Ambience_Phase04") },
            { "Phase05_SilverCity", new SoundPlace(PalacioPrata, "Ambience_Phase05", escalaAmbiente: 1.15f) },
            { "MainMenu", new SoundPlace(PalacioDepoisRei, "Ambience_Phase01", escalaMusica: 0.9f, escalaAmbiente: 0.4f) },
        };

        /// <summary>O som de uma cena pelo nome, ou null se ela nao tem.</summary>
        public static SoundPlace ParaCena(string nomeCena)
        {
            return nomeCena != null && Lugares.TryGetValue(nomeCena, out SoundPlace lugar) ? lugar : null;
        }

        // ------------------------------------------------------------------
        // Sons presos a objetos (Resources/Soundtrack/Ambience)
        // ------------------------------------------------------------------

        public const string ChiadoVapor = "Emitter_SteamHiss";
        public const string TremorCaldeira = "Emitter_BoilerRumble";
        public const string BolhasAcido = "Emitter_AcidBubbles";
        public const string ZumbidoCristal = "Emitter_CrystalHum";
    }
}
