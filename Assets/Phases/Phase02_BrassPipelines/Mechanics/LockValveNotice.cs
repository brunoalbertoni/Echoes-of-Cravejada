using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Vai numa valvula do District 2 cujo efeito acontece longe da vista de
    /// quem gira: as tres do fim das alas (que abrem as travas da Escadaria da
    /// Praca) e as que religam um atalho em outra sala. Quando o volante gira,
    /// o HUD avisa o que mudou e, nas de trava, conta quantas ja foram.
    /// </summary>
    [RequireComponent(typeof(Valve))]
    public class LockValveNotice : MonoBehaviour
    {
        private static readonly List<LockValveNotice> Locks = new List<LockValveNotice>();

        [Tooltip("Se abre uma das travas da Escadaria e entra na contagem.")]
        [SerializeField] private bool countsAsLock = true;

        [Tooltip("O que mais ela muda longe dali, dito no aviso. Vazio: nada.")]
        [SerializeField] private string extra;

        private Valve _valve;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(bool newCountsAsLock, string newExtra)
        {
            countsAsLock = newCountsAsLock;
            extra = newExtra;
        }

        private void Awake()
        {
            _valve = GetComponent<Valve>();
        }

        private void OnEnable()
        {
            if (countsAsLock) Locks.Add(this);
            _valve.Turned += OnTurned;
        }

        private void OnDisable()
        {
            Locks.Remove(this);
            _valve.Turned -= OnTurned;
        }

        private void OnTurned(Valve valve)
        {
            string text = extra;

            if (countsAsLock)
            {
                int open = 0;
                foreach (LockValveNotice notice in Locks)
                {
                    if (notice._valve.IsTurned) open++;
                }

                string count = open < Locks.Count
                    ? $"Uma trava da Escadaria se abriu ({open}/{Locks.Count})."
                    : $"A última trava da Escadaria se abriu ({open}/{Locks.Count}). A sala das caldeiras espera.";

                text = string.IsNullOrEmpty(extra) ? count : count + " " + extra;
            }

            if (!string.IsNullOrEmpty(text)) HudNotice.Show(text, 6f);
        }
    }
}
