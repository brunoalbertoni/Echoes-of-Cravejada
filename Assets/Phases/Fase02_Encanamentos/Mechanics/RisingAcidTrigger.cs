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
        [UnityEngine.Serialization.FormerlySerializedAs("pool")] [SerializeField] private AcidPool poca;

        [UnityEngine.Serialization.FormerlySerializedAs("riseTo")] [Tooltip("Ate que altura o acido sobe, a partir do fundo do tanque.")]
        [SerializeField] private float subidaPara = 44f;

        [UnityEngine.Serialization.FormerlySerializedAs("speed")] [Tooltip("Tiles por segundo. As colunas de vapor levantam a 9, entao ha folga — mas nao muita pra quem para.")]
        [SerializeField] private float velocidade = 1.4f;

        private bool _iniciado;

        /// <summary>Quando o acido comecou a subir. O HUD usa pro aviso.</summary>
        public static float TempoInicio { get; private set; } = -100f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            TempoInicio = -100f;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(AcidPool novoPoca, float novoSubidaPara)
        {
            poca = novoPoca;
            subidaPara = novoSubidaPara;
        }

        private void Awake()
        {
            ScrapHealth.Morreu += Rearmar;
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= Rearmar;
        }

        private void Rearmar()
        {
            _iniciado = false;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (_iniciado || poca == null) return;
            if (!BoilerMaster.Derrotado || AreaPurifier.EstaPurificado) return;
            if (outro.GetComponentInParent<ScrapController>() == null) return;

            _iniciado = true;
            TempoInicio = Time.time;
            Sfx.AlarmeAcido.Tocar();
            poca.SubirAte(subidaPara, velocidade);
        }
    }
}
