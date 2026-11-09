using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Uma grade com fechadura. Chegar perto com a chave no bolso gira a
    /// fechadura e abre a grade (a chave e gasta). Sem ela, um aviso — uma
    /// vez por aproximacao, pra nao encher a tela.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KeyGate : MonoBehaviour
    {
        [SerializeField] private string keyId;
        [SerializeField] private string keyName = "a chave";
        [SerializeField] private Gate gate;

        [Tooltip("O desenho da fechadura: some quando abre.")]
        [SerializeField] private SpriteRenderer lockSprite;

        private static readonly System.Collections.Generic.HashSet<string> Opened =
            new System.Collections.Generic.HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Opened.Clear();
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(string newKeyId, string newKeyName, Gate newGate, SpriteRenderer newLock)
        {
            keyId = newKeyId;
            keyName = newKeyName;
            gate = newGate;
            lockSprite = newLock;
        }

        private void Start()
        {
            // Ja aberta numa visita anterior: fica aberta.
            if (Opened.Contains(keyId)) Open(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<ScrapController>() == null) return;
            if (Opened.Contains(keyId)) return;

            if (!KeyItems.Has(keyId))
            {
                Sfx.LockRattle.Play(transform.position);
                HudNotice.Show("Trancada. A fechadura é antiga, de ferro — precisa de uma chave.");
                return;
            }

            KeyItems.Deliver(keyId);
            Sfx.LockUnlock.Play(transform.position);
            HudNotice.Show($"{keyName} gira na fechadura, rangendo.");
            Open(true);
        }

        private void Open(bool loud)
        {
            Opened.Add(keyId);
            if (lockSprite != null) lockSprite.enabled = false;

            // A fechadura rangendo e a grade correndo: quem esta perto escuta.
            if (loud) NoiseSystem.Emit(transform.position, 10f, 0.7f);
            if (gate != null) gate.Open();

            GetComponent<Collider2D>().enabled = false;
        }
    }
}
