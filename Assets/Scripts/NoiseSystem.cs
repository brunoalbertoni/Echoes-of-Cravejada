using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O barulho do mundo. Quem faz som chama Emit; quem reage a som escuta
    /// Heard. E o coracao do District 3: o mesmo ruido que acende os cristais
    /// acorda o que vive no escuro.
    ///
    /// Um ruido tem tres coisas: de onde veio, ate onde se ouve (o raio, em
    /// tiles) e o quao forte foi (de 0 a 1). O raio decide quem escuta; a forca
    /// decide o que quebra.
    /// </summary>
    public static class NoiseSystem
    {
        public delegate void NoiseHandler(Vector2 origin, float radius, float loudness);

        public static event NoiseHandler Heard;

        /// <summary>O ultimo ruido feito, e quando. O HUD usa pro medidor.</summary>
        public static float LastLoudness { get; private set; }
        public static float LastTime { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Heard = null;
            LastLoudness = 0f;
            LastTime = -100f;
        }

        public static void Emit(Vector2 origin, float radius, float loudness)
        {
            LastLoudness = loudness;
            LastTime = Time.time;
            Heard?.Invoke(origin, radius, loudness);
        }
    }

    /// <summary>
    /// O aviso de "segure E" que aparece no HUD, com barra. Quem esta sendo
    /// raspado se registra aqui enquanto o Scrap esta perto.
    /// </summary>
    public static class HoldAction
    {
        public static Object Owner { get; private set; }
        public static string Text { get; private set; }
        public static float Progress { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Owner = null;
        }

        public static void Show(Object owner, string text, float progress)
        {
            Owner = owner;
            Text = text;
            Progress = progress;
        }

        public static void Hide(Object owner)
        {
            if (Owner == owner) Owner = null;
        }
    }
}
