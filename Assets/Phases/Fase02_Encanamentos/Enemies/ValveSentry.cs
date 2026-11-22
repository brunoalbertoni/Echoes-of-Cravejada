using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Valve Sentry: uma torre corrompida, parada, que cospe jatos de vapor
    /// escaldante em linha reta, num ritmo fixo. Da pra contar o tempo e passar
    /// entre um jato e outro, ou pular por cima.
    ///
    /// O aviso e o desenho: antes de cada tiro o volante gira, o ponteiro do
    /// manometro sobe e o bocal esquenta; o tiro da coice. Ao morrer, o volante
    /// salta e ela tomba soltando vapor.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ValveSentry : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do SentryArt, no editor.
        public const int QuadroOcioso = 0;
        public const int QuadrosOcioso = 4;
        public const int QuadroCarga = 4;
        public const int QuadrosCarga = 3;
        public const int QuadroDisparo = 7;
        public const int QuadrosDisparo = 3;
        public const int Ferido = 10;
        public const int QuadroMorte = 11;
        public const int QuadrosMorte = 6;

        [UnityEngine.Serialization.FormerlySerializedAs("direction")] [Tooltip("+1 atira pra direita, -1 pra esquerda.")]
        [SerializeField] private int direcao = 1;

        [UnityEngine.Serialization.FormerlySerializedAs("interval")] [Tooltip("Segundos entre um jato e o proximo.")]
        [SerializeField] private float intervalo = 2.2f;

        [UnityEngine.Serialization.FormerlySerializedAs("chargeTime")] [Tooltip("Quanto antes do tiro ela comeca a carregar.")]
        [SerializeField] private float tempoCarga = 0.55f;

        [UnityEngine.Serialization.FormerlySerializedAs("range")] [Tooltip("Ate onde o jato vai, em tiles.")]
        [SerializeField] private float alcance = 16f;

        [UnityEngine.Serialization.FormerlySerializedAs("boltSpeed")] [SerializeField] private float velocidadeProjetil = 8f;
        [UnityEngine.Serialization.FormerlySerializedAs("boltSprite")] [SerializeField] private Sprite spriteProjetil;

        [UnityEngine.Serialization.FormerlySerializedAs("muzzle")] [Tooltip("A boca do bocal, em tiles, a partir do pe (olhando pra direita).")]
        [SerializeField] private Vector2 bocal = new Vector2(0.66f, 1.06f);

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        private SpriteRenderer _sprite;
        private Damageable _danificavel;
        private float _cronometro;
        private float _disparadoEm = -100f;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(int novoDirecao, float novoAlcance, Sprite novoSpriteProjetil, Sprite[] novoQuadros = null, Vector2? novoBocal = null)
        {
            direcao = novoDirecao;
            alcance = novoAlcance;
            spriteProjetil = novoSpriteProjetil;
            quadros = novoQuadros;
            if (novoBocal.HasValue) bocal = novoBocal.Value;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _danificavel = GetComponent<Damageable>();
            if (_danificavel != null)
            {
                _danificavel.AnimacaoMorte = Desabar;
                _danificavel.SomMorte = Sfx.VaporMorte;
            }

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = direcao < 0;

            // Cada torre comeca num ponto diferente do ciclo.
            _cronometro = Mathf.Repeat(transform.position.x * 0.37f, intervalo);
        }

        private void Update()
        {
            if (_danificavel != null && _danificavel.EstaMorto) return;

            float antes = _cronometro;
            _cronometro += Time.deltaTime;

            // O volante comeca a girar: o aviso do tiro.
            float carga = intervalo - tempoCarga;
            if (antes < carga && _cronometro >= carga) Sfx.CargaGuarda.Tocar(transform.position);

            if (_cronometro >= intervalo)
            {
                _cronometro = 0f;
                _disparadoEm = Time.time;

                Vector3 boca = transform.position + new Vector3(direcao * bocal.x, bocal.y, 0f);
                SteamBolt.Disparar(boca, direcao * velocidadeProjetil, alcance, spriteProjetil);
                Sfx.DisparoGuarda.Tocar(boca);
            }

            EscolherQuadro();
        }

        private void EscolherQuadro()
        {
            if (_sprite == null || quadros == null || quadros.Length == 0) return;

            int quadro;
            float desdeDisparo = Time.time - _disparadoEm;
            float paraDisparo = intervalo - _cronometro;

            if (_danificavel != null && Time.time - _danificavel.UltimoTempoGolpe < 0.15f) quadro = Ferido;
            else if (desdeDisparo < 0.3f) quadro = QuadroDisparo + Mathf.Min(QuadrosDisparo - 1, (int)(desdeDisparo / 0.1f));
            else if (paraDisparo < tempoCarga) quadro = QuadroCarga + Mathf.Min(QuadrosCarga - 1, (int)((1f - paraDisparo / tempoCarga) * QuadrosCarga));
            else quadro = QuadroOcioso + (int)(Time.time * 5f + transform.position.x) % QuadrosOcioso;

            Mostrar(quadro);
        }

        private void Mostrar(int quadro)
        {
            if (quadros != null && quadro >= 0 && quadro < quadros.Length && quadros[quadro] != null) FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }

        /// <summary>A morte: o volante salta, o vapor escapa, a cabeca tomba no chao.</summary>
        private IEnumerator Desabar()
        {
            float[] segurar = { 0.12f, 0.12f, 0.14f, 0.14f, 0.25f, 0.45f };
            for (int i = 0; i < QuadrosMorte; i++)
            {
                if (_sprite != null) Mostrar(QuadroMorte + i);
                yield return new WaitForSeconds(segurar[i]);
            }

            if (_sprite == null) yield break;
            Color inicio = _sprite.color;
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                _sprite.color = new Color(inicio.r, inicio.g, inicio.b, inicio.a * (1f - t / 0.35f));
                yield return null;
            }
        }
    }
}
