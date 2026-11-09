using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Salao das Estatuas, o enigma principal do District 3. Quatro
    /// estatuas, cada uma uma cena da escavacao; apertadas na ordem da
    /// historia gravada na galeria, abrem a porta que leva a broca.
    ///
    /// Cada botao e um clangor que chama quem estiver perto. Errar apaga todas
    /// as estatuas. Aqui nao nasce bicho nenhum: a cria (pool) fica vazia na
    /// Phase 3, e o Spawn so age se o gerador entregar bichos.
    /// </summary>
    public class StatueHall : MonoBehaviour
    {
        [SerializeField] private StatueButton[] statues;
        [SerializeField] private Gate gate;

        [Tooltip("Onde os mites nascem: o pe de cada rachadura.")]
        [SerializeField] private Transform[] cracks;

        [Tooltip("O desenho de cada rachadura, na mesma ordem.")]
        [SerializeField] private SpriteRenderer[] crackArt;

        [Tooltip("Fechada, rachando e aberta.")]
        [SerializeField] private Sprite[] crackFrames;

        [Tooltip("Os mites que podem nascer, desligados na cena.")]
        [SerializeField] private GameObject[] pool;

        [SerializeField] private int spawnOnRight = 0;
        [SerializeField] private int spawnOnWrong = 0;
        [SerializeField] private int maxAlive = 4;

        [Tooltip("Quanto tempo a rachadura treme antes de o bicho sair.")]
        [SerializeField] private float crackTime = 0.6f;

        private int _next = 1;
        private bool _open;
        private int _nextCrack;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(StatueButton[] newStatues, Gate newGate, Transform[] newCracks,
            SpriteRenderer[] newCrackArt, Sprite[] newCrackFrames, GameObject[] newPool)
        {
            statues = newStatues;
            gate = newGate;
            cracks = newCracks;
            crackArt = newCrackArt;
            crackFrames = newCrackFrames;
            pool = newPool;
        }

        private void OnEnable()
        {
            ScrapHealth.Died += OnScrapDied;

            if (statues == null) return;
            foreach (StatueButton statue in statues)
            {
                if (statue != null) statue.Pressed += OnPressed;
            }
        }

        private void OnDisable()
        {
            ScrapHealth.Died -= OnScrapDied;

            if (statues == null) return;
            foreach (StatueButton statue in statues)
            {
                if (statue != null) statue.Pressed -= OnPressed;
            }
        }

        private void OnPressed(StatueButton statue)
        {
            if (_open) return;

            // Purificado, ninguem mais mora nas rachaduras — e a porta ja nao
            // precisa de ninguem: a broca parou.
            if (AreaPurifier.IsPurified) return;

            bool right = !statue.IsLit && statue.Order == _next;

            if (right)
            {
                statue.SetLit(true);
                _next++;
                if (_next <= statues.Length) Sfx.PuzzleRight.Play(statue.transform.position, delay: 0.3f);
            }
            else
            {
                Sfx.PuzzleWrong.Play(statue.transform.position, delay: 0.3f);

                // Errou: tudo apaga. Se a errada e a primeira da historia, ela
                // ja vale como recomeco.
                foreach (StatueButton other in statues)
                {
                    if (other != null) other.SetLit(false);
                }

                _next = 1;
                if (statue.Order == 1)
                {
                    statue.SetLit(true);
                    _next = 2;
                }
            }

            if (_next > statues.Length)
            {
                _open = true;
                foreach (StatueButton other in statues)
                {
                    if (other != null) other.Enabled = false;
                }

                Sfx.PuzzleSolved.Play(statue.transform.position, delay: 0.3f);
                if (gate != null) gate.Open();
            }

            Spawn(right ? spawnOnRight : spawnOnWrong);
        }

        private void Spawn(int count)
        {
            if (pool == null || cracks == null || cracks.Length == 0) return;

            for (int i = 0; i < count; i++)
            {
                if (Alive() >= maxAlive) return;

                GameObject mite = FreeMite();
                if (mite == null) return;

                int crack = _nextCrack;
                _nextCrack = (_nextCrack + 1) % cracks.Length;

                StartCoroutine(Hatch(mite, crack, i * 0.35f));
            }
        }

        private int Alive()
        {
            int alive = 0;
            foreach (GameObject mite in pool)
            {
                if (mite != null && (mite.activeSelf || _hatching.Contains(mite))) alive++;
            }

            return alive;
        }

        private readonly System.Collections.Generic.HashSet<GameObject> _hatching =
            new System.Collections.Generic.HashSet<GameObject>();

        private GameObject FreeMite()
        {
            foreach (GameObject mite in pool)
            {
                if (mite != null && !mite.activeSelf && !_hatching.Contains(mite)) return mite;
            }

            return null;
        }

        private IEnumerator Hatch(GameObject mite, int crack, float delay)
        {
            _hatching.Add(mite);

            if (delay > 0f) yield return new WaitForSeconds(delay);

            SpriteRenderer art = crackArt != null && crack < crackArt.Length ? crackArt[crack] : null;

            // A pedra treme e racha antes: da tempo de ver de onde vem.
            for (float t = 0f; t < crackTime; t += Time.deltaTime)
            {
                if (art != null && crackFrames != null && crackFrames.Length >= 3)
                {
                    art.sprite = crackFrames[1];
                    art.transform.localPosition = new Vector3(Mathf.Sin(t * 70f) * 0.04f, 0f, 0f);
                }

                yield return null;
            }

            if (art != null)
            {
                art.transform.localPosition = Vector3.zero;
                if (crackFrames != null && crackFrames.Length >= 3) art.sprite = crackFrames[2];
            }

            _hatching.Remove(mite);

            // Morreu no meio, ou o distrito foi purificado: nao nasce.
            if (AreaPurifier.IsPurified || !isActiveAndEnabled) yield break;

            mite.transform.position = cracks[crack].position;
            mite.SetActive(true);

            // A pedra estourando acorda o bicho que saiu dela.
            NoiseSystem.Emit(cracks[crack].position, 4f, 0.5f);

            yield return new WaitForSeconds(1.2f);

            if (art != null && crackFrames != null && crackFrames.Length >= 3) art.sprite = crackFrames[0];
        }

        /// <summary>
        /// O Scrap morreu: a sequencia zera e os nascidos voltam pra pedra. O
        /// Damageable de cada um os religa nesse mesmo aviso, entao o desligar
        /// fica pro frame seguinte.
        /// </summary>
        private void OnScrapDied()
        {
            if (!isActiveAndEnabled) return;

            StopAllCoroutines();
            _hatching.Clear();
            StartCoroutine(PutBack());
        }

        private IEnumerator PutBack()
        {
            yield return null;

            foreach (GameObject mite in pool)
            {
                if (mite != null) mite.SetActive(false);
            }

            if (crackArt != null && crackFrames != null && crackFrames.Length >= 3)
            {
                foreach (SpriteRenderer art in crackArt)
                {
                    if (art == null) continue;
                    art.sprite = crackFrames[0];
                    art.transform.localPosition = Vector3.zero;
                }
            }

            if (_open) yield break;

            _next = 1;
            foreach (StatueButton statue in statues)
            {
                if (statue != null) statue.SetLit(false);
            }
        }
    }
}
