using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma fala de cena: alguem fala sem o Scrap ter puxado conversa (o chefe
    /// antes da luta, na troca de fase, no fim). Aparece na mesma caixa de
    /// dialogo dos NPCs (o GameHud desenha), com o nome e o retrato de quem
    /// fala, uma linha por vez; o botao de interagir passa pra proxima.
    ///
    /// Quem chama espera acabar: <c>yield return ScriptedDialogue.Play(...)</c>.
    /// Quem chama tambem trava o controle do Scrap (normalmente ja esta numa
    /// Cinematic).
    ///
    /// Teclas: E ou K; no controle, o botao de cima.
    /// </summary>
    public static class ScriptedDialogue
    {
        private static string[] _linhas;
        private static int _indice;

        /// <summary>Se ha uma fala de cena na tela.</summary>
        public static bool Ativo => _linhas != null && _indice < _linhas.Length;

        /// <summary>Quem fala.</summary>
        public static string Falante { get; private set; }

        /// <summary>O desenho de quem fala, pro retrato na caixa.</summary>
        public static SpriteRenderer Retrato { get; private set; }

        /// <summary>O busto de quem fala na caixa (<see cref="SpeakerPortraits"/>).</summary>
        public static string IdRetrato { get; private set; }

        /// <summary>A linha na tela agora.</summary>
        public static string LinhaAtual => Ativo ? _linhas[_indice] : null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _linhas = null;
            _indice = 0;
            Falante = null;
            Retrato = null;
            IdRetrato = null;
        }

        /// <summary>Mostra as falas, uma por vez, e termina quando o Scrap passou a ultima.</summary>
        public static IEnumerator Tocar(string falante, SpriteRenderer retrato, params string[] linhas2)
        {
            return TocarComo(SpeakerPortraits.IdDe(falante), falante, retrato, linhas2);
        }

        /// <summary>
        /// O mesmo, escolhendo o busto (o Rei corrompido fala com o rosto
        /// tomado pelo Oxide, com o mesmo nome).
        /// </summary>
        public static IEnumerator TocarComo(string idRetrato, string falante, SpriteRenderer retrato, params string[] linhas2)
        {
            if (linhas2 == null || linhas2.Length == 0) yield break;

            IdRetrato = idRetrato;
            Falante = falante;
            Retrato = retrato;
            _linhas = linhas2;
            _indice = 0;
            Sfx.AbertoDialogo.Tocar();

            // O aperto que abriu a cena nao pula a primeira linha.
            yield return null;

            while (_indice < _linhas.Length)
            {
                if (InteracaoPressionada())
                {
                    _indice++;
                    if (_indice < _linhas.Length) Sfx.ProximoDialogo.Tocar();
                    else Sfx.FecharDialogo.Tocar();
                }

                yield return null;
            }

            _linhas = null;
            Falante = null;
            Retrato = null;
            IdRetrato = null;

            // Esse mesmo aperto nao vale pra mais nada.
            yield return null;
        }

        /// <summary>Some com a fala na hora (a cena foi interrompida).</summary>
        public static void Parar()
        {
            _linhas = null;
            Falante = null;
            Retrato = null;
            IdRetrato = null;
        }

        private static bool InteracaoPressionada()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.eKey.wasPressedThisFrame || teclado.kKey.wasPressedThisFrame)) return true;

            Gamepad controle = Gamepad.current;
            return controle != null && controle.buttonNorth.wasPressedThisFrame;
        }
    }
}
