using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma fechadura de cristal: abre o portao dela quando um raio de luz
    /// alquimica fica em cima por um instante. Aberta, fica aberta.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CrystalLock : MonoBehaviour
    {
        [SerializeField] private Gate gate;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Light2D glow;

        [Tooltip("Segundos de luz ate abrir.")]
        [SerializeField] private float chargeTime = 0.6f;

        private float _charge;
        private float _litUntil;
        private bool _open;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Gate newGate, SpriteRenderer newSprite, Light2D newGlow)
        {
            gate = newGate;
            sprite = newSprite;
            glow = newGlow;
        }

        /// <summary>Chamado pelo raio, a cada quadro em que ele bate aqui.</summary>
        public void Illuminate()
        {
            _litUntil = Time.time + 0.1f;
        }

        private void Update()
        {
            bool lit = _open || Time.time < _litUntil;

            if (!_open)
            {
                _charge = lit ? _charge + Time.deltaTime : 0f;

                if (_charge >= chargeTime)
                {
                    _open = true;
                    if (gate != null) gate.Open();
                    NoiseSystem.Emit(transform.position, 12f, 0.5f);
                }
            }

            if (sprite != null) sprite.color = lit ? Color.white : new Color(0.45f, 0.45f, 0.55f);
            if (glow != null) glow.intensity = Mathf.MoveTowards(glow.intensity, lit ? 1.2f : 0.15f, 4f * Time.deltaTime);
        }
    }
}
