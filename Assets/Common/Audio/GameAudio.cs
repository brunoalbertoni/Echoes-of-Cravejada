using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Os volumes escolhidos pelo jogador, de 0 a 10, guardados entre sessoes.
    /// Musica e um canal; efeitos, ambiente e sons do mapa sao o outro.
    /// </summary>
    public static class GameAudio
    {
        public const int PassosMax = 10;

        private const string ChaveMusica = "audio.music";
        private const string ChaveEfeitos = "audio.effects";

        public static int PassosMusica
        {
            get => PlayerPrefs.GetInt(ChaveMusica, PassosMax);
            set => Guardar(ChaveMusica, value);
        }

        public static int PassosEfeitos
        {
            get => PlayerPrefs.GetInt(ChaveEfeitos, PassosMax);
            set => Guardar(ChaveEfeitos, value);
        }

        public static float NivelMusica => PassosMusica / (float)PassosMax;

        public static float NivelEfeitos => PassosEfeitos / (float)PassosMax;

        private static void Guardar(string chave, int passos)
        {
            PlayerPrefs.SetInt(chave, Mathf.Clamp(passos, 0, PassosMax));
            PlayerPrefs.Save();
        }
    }

    /// <summary>Marca um campo estatico como parte do progresso gravado no save.</summary>
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public sealed class SaveProgressAttribute : System.Attribute
    {
        /// <summary>O nome gravado no save. Nao muda, mesmo que a variavel seja renomeada.</summary>
        public string ChaveDoSave { get; }

        public SaveProgressAttribute(string chaveDoSave)
        {
            ChaveDoSave = chaveDoSave;
        }
    }
}
