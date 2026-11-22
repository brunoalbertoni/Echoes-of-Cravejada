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
        public delegate void NoiseHandler(Vector2 pontoOrigem, float raioArea, float volumeRuido2);

        public static event NoiseHandler Ouvido;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Ouvido = null;
        }

        public static void Emitir(Vector2 pontoOrigem, float raioArea, float volumeRuido2)
        {
            Ouvido?.Invoke(pontoOrigem, raioArea, volumeRuido2);
        }
    }

    /// <summary>
    /// O aviso de "segure E" que aparece no HUD, com barra. Quem esta sendo
    /// raspado se registra aqui enquanto o Scrap esta perto.
    /// </summary>
    public static class HoldAction
    {
        public static Object Dono { get; private set; }
        public static string Texto2 { get; private set; }
        public static float Progresso2 { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Dono = null;
        }

        public static void Mostrar(Object dono, string texto, float progresso)
        {
            Dono = dono;
            Texto2 = texto;
            Progresso2 = progresso;
        }

        public static void Esconder(Object dono)
        {
            if (Dono == dono) Dono = null;
        }
    }
}
