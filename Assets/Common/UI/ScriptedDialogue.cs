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
        private static string[] _lines;
        private static int _index;

        /// <summary>Se ha uma fala de cena na tela.</summary>
        public static bool Active => _lines != null && _index < _lines.Length;

        /// <summary>Quem fala.</summary>
        public static string Speaker { get; private set; }

        /// <summary>O desenho de quem fala, pro retrato na caixa.</summary>
        public static SpriteRenderer Portrait { get; private set; }

        /// <summary>A linha na tela agora.</summary>
        public static string CurrentLine => Active ? _lines[_index] : null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _lines = null;
            _index = 0;
            Speaker = null;
            Portrait = null;
        }

        /// <summary>Mostra as falas, uma por vez, e termina quando o Scrap passou a ultima.</summary>
        public static IEnumerator Play(string speaker, SpriteRenderer portrait, params string[] lines)
        {
            if (lines == null || lines.Length == 0) yield break;

            Speaker = speaker;
            Portrait = portrait;
            _lines = lines;
            _index = 0;
            Sfx.DialogueOpen.Play();

            // O aperto que abriu a cena nao pula a primeira linha.
            yield return null;

            while (_index < _lines.Length)
            {
                if (InteractPressed())
                {
                    _index++;
                    if (_index < _lines.Length) Sfx.DialogueNext.Play();
                    else Sfx.DialogueClose.Play();
                }

                yield return null;
            }

            _lines = null;
            Speaker = null;
            Portrait = null;

            // Esse mesmo aperto nao vale pra mais nada.
            yield return null;
        }

        /// <summary>Some com a fala na hora (a cena foi interrompida).</summary>
        public static void Stop()
        {
            _lines = null;
            Speaker = null;
            Portrait = null;
        }

        private static bool InteractPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame)) return true;

            Gamepad pad = Gamepad.current;
            return pad != null && pad.buttonNorth.wasPressedThisFrame;
        }
    }
}
