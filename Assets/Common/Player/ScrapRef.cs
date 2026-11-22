using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O caminho unico ate o Scrap da cena. Guarda o que achou e so procura de
    /// novo quando a cena troca ou o objeto deixa de estar ativo.
    /// </summary>
    public static class ScrapRef
    {
        private static ScrapController _controlador;
        private static ScrapAnimator _animador;
        private static ScrapHealth _vida;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _controlador = null;
            _animador = null;
            _vida = null;
        }

        public static ScrapController Controlador => Resolver(ref _controlador);

        public static ScrapAnimator Animador => Resolver(ref _animador);

        public static ScrapHealth Vida => Resolver(ref _vida);

        private static T Resolver<T>(ref T guardado) where T : Component
        {
            if (guardado == null || !guardado.gameObject.activeInHierarchy)
            {
                guardado = Object.FindAnyObjectByType<T>();
            }

            return guardado;
        }
    }
}
