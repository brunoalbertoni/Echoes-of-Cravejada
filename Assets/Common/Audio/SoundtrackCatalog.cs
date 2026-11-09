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
        public readonly string Clip;

        /// <summary>De onde ela comeca a tocar.</summary>
        public readonly float Start;

        /// <summary>Pra onde ela volta quando da a volta.</summary>
        public readonly float LoopFrom;

        /// <summary>Onde ela da a volta (antes do fim que some). Zero: no fim do arquivo.</summary>
        public readonly float LoopEnd;

        /// <summary>Falso: toca uma vez e acaba.</summary>
        public readonly bool Loop;

        /// <summary>Volume dela em relacao as outras (1 = normal).</summary>
        public readonly float Volume;

        public MusicTrack(string clip, float start = 0f, float loopFrom = 0f, float loopEnd = 0f, bool loop = true, float volume = 1f)
        {
            Clip = clip;
            Start = start;
            LoopFrom = loopFrom;
            LoopEnd = loopEnd;
            Loop = loop;
            Volume = volume;
        }
    }

    /// <summary>O som de uma cena: a musica de explorar, o ambiente e o quanto cada um soa ali.</summary>
    public sealed class SoundPlace
    {
        public readonly MusicTrack Explore;
        public readonly string Ambience;
        public readonly float MusicScale;
        public readonly float AmbienceScale;

        public SoundPlace(MusicTrack explore, string ambience, float musicScale = 1f, float ambienceScale = 1f)
        {
            Explore = explore;
            Ambience = ambience;
            MusicScale = musicScale;
            AmbienceScale = ambienceScale;
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

        public const float MusicVolume = 0.5f;
        public const float AmbienceVolume = 0.8f;
        public const float EmitterVolume = 0.6f;

        /// <summary>Quanto o ambiente abaixa enquanto toca musica de luta.</summary>
        public const float AmbienceUnderBattle = 0.55f;

        /// <summary>Segundos de cada troca de musica.</summary>
        public const float CrossfadeTime = 2.5f;

        /// <summary>Segundos da emenda quando uma musica da a volta.</summary>
        public const float LoopFadeTime = 4f;

        // ------------------------------------------------------------------
        // Exploracao
        // ------------------------------------------------------------------

        public static readonly MusicTrack ScrapYard = new MusicTrack("SignalToNoise", loopEnd: 338f);
        public static readonly MusicTrack BrassPipelines = new MusicTrack("Horizons", loopEnd: 278f);
        public static readonly MusicTrack SilentGeodes = new MusicTrack("Decoherence", start: 10f, loopFrom: 10f, loopEnd: 604f);
        public static readonly MusicTrack AlchemicalAteliers = new MusicTrack("Hiraeth", loopEnd: 320f);
        public static readonly MusicTrack SilverPalace = new MusicTrack("PassageOfTime", loopEnd: 250f);

        /// <summary>O palacio depois que o Rei cai: a calma voltando, a caminho do Altar.</summary>
        public static readonly MusicTrack PalaceAfterKing = new MusicTrack("TheRestoration", loopEnd: 178f, volume: 1.2f);

        // ------------------------------------------------------------------
        // Chefes
        // ------------------------------------------------------------------

        public static readonly MusicTrack RustyScrapper = new MusicTrack("Juggernaut", loopFrom: 25f, loopEnd: 208f);
        public static readonly MusicTrack BoilerMaster = new MusicTrack("Catalyst", start: 15f, loopFrom: 15f, loopEnd: 258f);
        public static readonly MusicTrack TheResonant = new MusicTrack("TheEncounter", start: 30f, loopFrom: 30f, loopEnd: 302f);
        public static readonly MusicTrack GeodeDrill = new MusicTrack("Simulacra", loopFrom: 50f, loopEnd: 228f);
        public static readonly MusicTrack MercuryAlchemist = new MusicTrack("HourOfTheWitch", start: 68f, loopFrom: 68f, loopEnd: 198f);
        public static readonly MusicTrack SilverKingRadiant = new MusicTrack("Goliath", loopFrom: 45f, loopEnd: 368f);
        public static readonly MusicTrack SilverKingCorrupted = new MusicTrack("EyesInTheVoid", start: 48f, loopFrom: 48f, loopEnd: 232f);

        // ------------------------------------------------------------------
        // Momentos
        // ------------------------------------------------------------------

        /// <summary>A fuga do acido que sobe, no poco da fase 2.</summary>
        public static readonly MusicTrack AcidEscape = new MusicTrack("EscapeVelocity", start: 30f, loopFrom: 30f, loopEnd: 232f);

        /// <summary>A purificacao de um distrito: toca uma vez, ate sair da cena.</summary>
        public static readonly MusicTrack Purification = new MusicTrack("HymnToTheDawn", start: 18f, loop: false);

        /// <summary>O Grande Polimento: a ultima musica do jogo, que segue no cartao de FIM.</summary>
        public static readonly MusicTrack Finale = new MusicTrack("Echoes", start: 9f, loop: false, volume: 1.1f);

        // ------------------------------------------------------------------
        // Cenas
        // ------------------------------------------------------------------

        private static readonly Dictionary<string, SoundPlace> Places = new Dictionary<string, SoundPlace>
        {
            { "Phase01_ScrapYard", new SoundPlace(ScrapYard, "Ambience_Phase01") },
            { "Phase02_BrassPipelines", new SoundPlace(BrassPipelines, "Ambience_Phase02") },
            // Nos geodos o barulho do Scrap e mecanica do jogo: musica mais baixa.
            { "Phase03_SilentGeodes", new SoundPlace(SilentGeodes, "Ambience_Phase03", musicScale: 0.7f, ambienceScale: 0.85f) },
            { "Phase04_AlchemicalAteliers", new SoundPlace(AlchemicalAteliers, "Ambience_Phase04") },
            { "Phase05_SilverCity", new SoundPlace(SilverPalace, "Ambience_Phase05", ambienceScale: 1.15f) },
        };

        /// <summary>O som de uma cena pelo nome, ou null se ela nao tem.</summary>
        public static SoundPlace ForScene(string sceneName)
        {
            return sceneName != null && Places.TryGetValue(sceneName, out SoundPlace place) ? place : null;
        }

        // ------------------------------------------------------------------
        // Sons presos a objetos (Resources/Soundtrack/Ambience)
        // ------------------------------------------------------------------

        public const string SteamHiss = "Emitter_SteamHiss";
        public const string BoilerRumble = "Emitter_BoilerRumble";
        public const string AcidBubbles = "Emitter_AcidBubbles";
        public const string CrystalHum = "Emitter_CrystalHum";
    }
}
