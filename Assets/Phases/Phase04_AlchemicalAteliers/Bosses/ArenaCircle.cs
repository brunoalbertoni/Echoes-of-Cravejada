using System;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um circulo de transmutacao no estrado do Grande Laboratorio. A runa do
    /// centro acende com golpes de cinzel: cada golpe a enche um pouco de luz
    /// (dois bastam), sem aviso nenhum na tela. Se o Alquimista estiver perto
    /// quando ela acender, ele vira prata.
    ///
    /// Depois de usado, o Alquimista o afoga em mercurio (Smother): a runa
    /// apaga e fica inutil por um tempo, e o Scrap tem que ir a outro.
    /// </summary>
    public class ArenaCircle : MonoBehaviour, IDamageable
    {
        [Tooltip("Quantos golpes ate a runa acender.")]
        [SerializeField] private int hitsToLight = 2;

        [Tooltip("Quanto tempo um golpe fica valendo; parado demais, a luz da runa se apaga.")]
        [SerializeField] private float hitMemory = 2.5f;

        [SerializeField] private SpriteRenderer rune;
        [SerializeField] private Color idleColor = new Color(0.62f, 0.68f, 0.82f);
        [SerializeField] private Color litColor = new Color(0.80f, 1f, 1f);
        [SerializeField] private Color smotheredColor = new Color(0.30f, 0.32f, 0.40f);

        private int _hits;
        private float _lastHit;
        private float _smotheredUntil;
        private float _flashUntil;
        private float _bump;

        /// <summary>Avisa quando a runa acende.</summary>
        public event Action<ArenaCircle> Polished;

        /// <summary>So funciona com a luta acontecendo.</summary>
        public bool Armed { get; set; }

        public bool Usable => Armed && Time.time >= _smotheredUntil;

        /// <summary>Usado pelos geradores de fase. O segundo valor sobrou de quando se segurava o botao.</summary>
        public void Configure(SpriteRenderer newRune, float unused)
        {
            rune = newRune;
        }

        /// <summary>Afogado em mercurio: inutil por tantos segundos.</summary>
        public void Smother(float seconds)
        {
            _smotheredUntil = Time.time + seconds;
            _hits = 0;
        }

        /// <summary>Volta como no comeco da luta.</summary>
        public void ResetCircle()
        {
            _smotheredUntil = 0f;
            _flashUntil = 0f;
            _hits = 0;
        }

        public void Damage(int amount, Vector2 from)
        {
            if (!Usable) return;

            if (Time.time - _lastHit > hitMemory) _hits = 0;
            _lastHit = Time.time;
            _hits++;
            _bump = 1f;

            if (_hits < hitsToLight) return;

            _hits = 0;
            _flashUntil = Time.time + 0.5f;
            NoiseSystem.Emit(transform.position, 10f, 0.5f);
            Sfx.RunePolished.Play(transform.position);
            Polished?.Invoke(this);
        }

        private void Update()
        {
            if (_hits > 0 && Time.time - _lastHit > hitMemory) _hits = 0;
            _bump = Mathf.MoveTowards(_bump, 0f, Time.deltaTime * 4f);

            if (rune == null) return;

            // A runa enche de luz a cada golpe, e pulsa no golpe.
            Color target = Time.time < _flashUntil ? litColor : Usable ? idleColor : smotheredColor;
            if (Usable && _hits > 0) target = Color.Lerp(idleColor, litColor, _hits / (float)hitsToLight);
            target = Color.Lerp(target, Color.white, _bump * 0.5f);
            rune.color = Color.Lerp(rune.color, target, 12f * Time.deltaTime);
        }
    }
}
