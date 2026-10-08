using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O gatilho do acido ascendente. Depois que o Boiler Master cai, entrar no
    /// poco de fuga estoura a valvula de emergencia: o acido comeca a subir e
    /// nao para ate o topo. Antes do boss, entrar aqui nao faz nada.
    ///
    /// Se o Scrap morrer na subida, o acido volta pro fundo e o gatilho arma de novo.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RisingAcidTrigger : MonoBehaviour
    {
        [SerializeField] private AcidPool pool;

        [Tooltip("Ate que altura o acido sobe, a partir do fundo do tanque.")]
        [SerializeField] private float riseTo = 44f;

        [Tooltip("Tiles por segundo. As colunas de vapor levantam a 9, entao ha folga — mas nao muita pra quem para.")]
        [SerializeField] private float speed = 1.4f;

        private bool _started;

        /// <summary>Quando o acido comecou a subir. O HUD usa pro aviso.</summary>
        public static float StartTime { get; private set; } = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            StartTime = -100f;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(AcidPool newPool, float newRiseTo)
        {
            pool = newPool;
            riseTo = newRiseTo;
        }

        private void Awake()
        {
            ScrapHealth.Died += Rearm;
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= Rearm;
        }

        private void Rearm()
        {
            _started = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_started || pool == null) return;
            if (!BoilerMaster.Defeated || AreaPurifier.IsPurified) return;
            if (other.GetComponentInParent<ScrapController>() == null) return;

            _started = true;
            StartTime = Time.time;
            pool.RiseTo(riseTo, speed);
        }
    }
}
