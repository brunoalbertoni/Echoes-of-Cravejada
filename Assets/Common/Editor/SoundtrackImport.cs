using UnityEditor;
using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Acerta a importacao do audio da trilha sozinho, quando o arquivo entra no
    /// projeto:
    /// - musicas (Soundtrack/Music): em streaming, pra nao carregar minutos de
    ///   audio inteiros na memoria;
    /// - ambiente (Soundtrack/Ambience): comprimido na memoria, pronto pra loop;
    /// - efeitos (Sfx): curtos, descomprimidos ao carregar, pra tocar sem atraso.
    /// </summary>
    public class SoundtrackImport : AssetPostprocessor
    {
        private const string Root = "Assets/Common/Audio/Resources/Soundtrack/";
        private const string SfxRoot = "Assets/Common/Audio/Resources/Sfx/";

        // Mudar o numero faz o Unity reimportar o audio com as regras novas.
        public override uint GetVersion() => 1;

        private void OnPreprocessAudio()
        {
            if (assetPath.StartsWith(SfxRoot))
            {
                PrepareEffect((AudioImporter)assetImporter);
                return;
            }

            if (!assetPath.StartsWith(Root)) return;

            var importer = (AudioImporter)assetImporter;
            bool music = assetPath.StartsWith(Root + "Music/");

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.CompressedInMemory;
            settings.quality = music ? 0.7f : 0.6f;
            settings.preloadAudioData = !music;
            importer.defaultSampleSettings = settings;

            importer.forceToMono = false;
            importer.loadInBackground = music;
        }

        private static void PrepareEffect(AudioImporter importer)
        {
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;

            importer.forceToMono = true;
            importer.loadInBackground = false;
        }
    }
}
