using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um aviso curto no meio da tela, pra quem nao tem um aviso proprio no
    /// HUD: pegar uma ferramenta, o motor do Coletor, a gema, esbarrar na
    /// nevoa. Um por vez; o mais novo substitui o anterior.
    /// </summary>
    public static class HudNotice
    {
        public static string Texto2 { get; private set; }

        private static float _ate = -100f;

        /// <summary>Se ha um aviso na tela agora.</summary>
        public static bool Mostrando => Texto2 != null && Time.time < _ate;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Texto2 = null;
            _ate = -100f;
        }

        public static void Mostrar(string texto, float segundos = 5f)
        {
            // Tilinta so com aviso novo de verdade: nao com o mesmo repetido a
            // cada quadro (o "segure E" do elevador), nem junto com o som da
            // coisa que acabou de ser pega.
            bool novo = !Mostrando || Texto2 != texto;
            if (novo && segundos >= 1f && !SfxPlayer.InterfaceSoouHa(0.2f)) Sfx.Aviso.Tocar();

            Texto2 = texto;
            _ate = Time.time + segundos;
        }
    }
}
