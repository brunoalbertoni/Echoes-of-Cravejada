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
        [UnityEngine.Serialization.FormerlySerializedAs("statues")] [SerializeField] private StatueButton[] estatuas;
        [UnityEngine.Serialization.FormerlySerializedAs("gate")] [SerializeField] private Gate portao;

        [UnityEngine.Serialization.FormerlySerializedAs("cracks")] [Tooltip("Onde os mites nascem: o pe de cada rachadura.")]
        [SerializeField] private Transform[] rachaduras;

        [UnityEngine.Serialization.FormerlySerializedAs("crackArt")] [Tooltip("O desenho de cada rachadura, na mesma ordem.")]
        [SerializeField] private SpriteRenderer[] arteRachadura;

        [UnityEngine.Serialization.FormerlySerializedAs("crackFrames")] [Tooltip("Fechada, rachando e aberta.")]
        [SerializeField] private Sprite[] quadrosRachadura;

        [UnityEngine.Serialization.FormerlySerializedAs("pool")] [Tooltip("Os mites que podem nascer, desligados na cena.")]
        [SerializeField] private GameObject[] poca;

        [UnityEngine.Serialization.FormerlySerializedAs("spawnOnRight")] [SerializeField] private int nascimentoLigadoDireita = 0;
        [UnityEngine.Serialization.FormerlySerializedAs("spawnOnWrong")] [SerializeField] private int nascimentoLigadoErrado = 0;
        [UnityEngine.Serialization.FormerlySerializedAs("maxAlive")] [SerializeField] private int vivoMax = 4;

        [UnityEngine.Serialization.FormerlySerializedAs("crackTime")] [Tooltip("Quanto tempo a rachadura treme antes de o bicho sair.")]
        [SerializeField] private float tempoRachadura = 0.6f;

        private int _proximo = 1;
        private bool _aberto;
        private int _proximoRachadura;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(StatueButton[] novoEstatuas, Gate novoPortao, Transform[] novoRachaduras,
            SpriteRenderer[] novoArteRachadura, Sprite[] novoQuadrosRachadura, GameObject[] novoPoca)
        {
            estatuas = novoEstatuas;
            portao = novoPortao;
            rachaduras = novoRachaduras;
            arteRachadura = novoArteRachadura;
            quadrosRachadura = novoQuadrosRachadura;
            poca = novoPoca;
        }

        private void OnEnable()
        {
            ScrapHealth.Morreu += AoScrapMorrer;

            if (estatuas == null) return;
            foreach (StatueButton estatua in estatuas)
            {
                if (estatua != null) estatua.Apertado += AoPressionar;
            }
        }

        private void OnDisable()
        {
            ScrapHealth.Morreu -= AoScrapMorrer;

            if (estatuas == null) return;
            foreach (StatueButton estatua in estatuas)
            {
                if (estatua != null) estatua.Apertado -= AoPressionar;
            }
        }

        private void AoPressionar(StatueButton estatua)
        {
            if (_aberto) return;

            // Purificado, ninguem mais mora nas rachaduras — e a porta ja nao
            // precisa de ninguem: a broca parou.
            if (AreaPurifier.EstaPurificado) return;

            bool direita = !estatua.EstaAceso && estatua.Ordem == _proximo;

            if (direita)
            {
                estatua.DefinirAceso(true);
                _proximo++;
                if (_proximo <= estatuas.Length) Sfx.DireitaEnigma.Tocar(estatua.transform.position, atraso: 0.3f);
            }
            else
            {
                Sfx.ErradoEnigma.Tocar(estatua.transform.position, atraso: 0.3f);

                // Errou: tudo apaga. Se a errada e a primeira da historia, ela
                // ja vale como recomeco.
                foreach (StatueButton outro in estatuas)
                {
                    if (outro != null) outro.DefinirAceso(false);
                }

                _proximo = 1;
                if (estatua.Ordem == 1)
                {
                    estatua.DefinirAceso(true);
                    _proximo = 2;
                }
            }

            if (_proximo > estatuas.Length)
            {
                _aberto = true;
                foreach (StatueButton outro in estatuas)
                {
                    if (outro != null) outro.Habilitado = false;
                }

                Sfx.ResolvidoEnigma.Tocar(estatua.transform.position, atraso: 0.3f);
                if (portao != null) portao.Abrir();
            }

            Criar(direita ? nascimentoLigadoDireita : nascimentoLigadoErrado);
        }

        private void Criar(int quantidade)
        {
            if (poca == null || rachaduras == null || rachaduras.Length == 0) return;

            for (int i = 0; i < quantidade; i++)
            {
                if (QuantidadeViva() >= vivoMax) return;

                GameObject acaro = AcaroLivre();
                if (acaro == null) return;

                int rachadura = _proximoRachadura;
                _proximoRachadura = (_proximoRachadura + 1) % rachaduras.Length;

                StartCoroutine(Chocar(acaro, rachadura, i * 0.35f));
            }
        }

        private int QuantidadeViva()
        {
            int vivo = 0;
            foreach (GameObject acaro in poca)
            {
                if (acaro != null && (acaro.activeSelf || _eclodindo.Contains(acaro))) vivo++;
            }

            return vivo;
        }

        private readonly System.Collections.Generic.HashSet<GameObject> _eclodindo =
            new System.Collections.Generic.HashSet<GameObject>();

        private GameObject AcaroLivre()
        {
            foreach (GameObject acaro in poca)
            {
                if (acaro != null && !acaro.activeSelf && !_eclodindo.Contains(acaro)) return acaro;
            }

            return null;
        }

        private IEnumerator Chocar(GameObject acaro, int rachadura, float atraso)
        {
            _eclodindo.Add(acaro);

            if (atraso > 0f) yield return new WaitForSeconds(atraso);

            SpriteRenderer arte = arteRachadura != null && rachadura < arteRachadura.Length ? arteRachadura[rachadura] : null;

            // A pedra treme e racha antes: da tempo de ver de onde vem.
            for (float t = 0f; t < tempoRachadura; t += Time.deltaTime)
            {
                if (arte != null && quadrosRachadura != null && quadrosRachadura.Length >= 3)
                {
                    arte.sprite = quadrosRachadura[1];
                    arte.transform.localPosition = new Vector3(Mathf.Sin(t * 70f) * 0.04f, 0f, 0f);
                }

                yield return null;
            }

            if (arte != null)
            {
                arte.transform.localPosition = Vector3.zero;
                if (quadrosRachadura != null && quadrosRachadura.Length >= 3) arte.sprite = quadrosRachadura[2];
            }

            _eclodindo.Remove(acaro);

            // Morreu no meio, ou o distrito foi purificado: nao nasce.
            if (AreaPurifier.EstaPurificado || !isActiveAndEnabled) yield break;

            acaro.transform.position = rachaduras[rachadura].position;
            acaro.SetActive(true);

            // A pedra estourando acorda o bicho que saiu dela.
            NoiseSystem.Emitir(rachaduras[rachadura].position, 4f, 0.5f);

            yield return new WaitForSeconds(1.2f);

            if (arte != null && quadrosRachadura != null && quadrosRachadura.Length >= 3) arte.sprite = quadrosRachadura[0];
        }

        /// <summary>
        /// O Scrap morreu: a sequencia zera e os nascidos voltam pra pedra. O
        /// Damageable de cada um os religa nesse mesmo aviso, entao o desligar
        /// fica pro frame seguinte.
        /// </summary>
        private void AoScrapMorrer()
        {
            if (!isActiveAndEnabled) return;

            StopAllCoroutines();
            _eclodindo.Clear();
            StartCoroutine(Recolocar());
        }

        private IEnumerator Recolocar()
        {
            yield return null;

            foreach (GameObject acaro in poca)
            {
                if (acaro != null) acaro.SetActive(false);
            }

            if (arteRachadura != null && quadrosRachadura != null && quadrosRachadura.Length >= 3)
            {
                foreach (SpriteRenderer arte in arteRachadura)
                {
                    if (arte == null) continue;
                    arte.sprite = quadrosRachadura[0];
                    arte.transform.localPosition = Vector3.zero;
                }
            }

            if (_aberto) yield break;

            _proximo = 1;
            foreach (StatueButton estatua in estatuas)
            {
                if (estatua != null) estatua.DefinirAceso(false);
            }
        }
    }
}
