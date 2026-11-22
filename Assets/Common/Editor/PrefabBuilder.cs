using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Cria os prefabs de projeteis, efeitos e coletaveis em Resources/Prefabs.</summary>
    public static class PrefabBuilder
    {
        private const string Pasta = "Assets/Common/Resources/Prefabs";

        private static readonly Dictionary<string, Type[]> Catalogo = new Dictionary<string, Type[]>
        {
            { "AcidPuddle", new[] { typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D) } },
            { "AncientKey", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D) } },
            { "BenchSpark", new[] { typeof(SpriteRenderer) } },
            { "BloaterBurst", new[] { typeof(SpriteRenderer) } },
            { "DrillPebble", new[] { typeof(SpriteRenderer) } },
            { "FacetGlint", new[] { typeof(SpriteRenderer) } },
            { "FacetShard", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D) } },
            { "FallingShard", new[] { typeof(SpriteRenderer) } },
            { "Flask", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D) } },
            { "MercuryBead", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D) } },
            { "MercuryDrop", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D) } },
            { "OldScrap", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D) } },
            { "OpaqueShard", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D) } },
            { "PressureJumpModule", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D) } },
            { "Reward", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D) } },
            { "ScrapChunk", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D) } },
            { "ShardShot", new[] { typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D) } },
            { "ShockWave", new[] { typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D) } },
            { "SteamBolt", new[] { typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D) } },
            { "SteamGeyser", new[] { typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D) } }
        };

        [MenuItem("Echoes/Gerar Prefabs", false, 40)]
        public static void Gerar()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Common/Resources")) AssetDatabase.CreateFolder("Assets/Common", "Resources");
            if (!AssetDatabase.IsValidFolder(Pasta)) AssetDatabase.CreateFolder("Assets/Common/Resources", "Prefabs");

            foreach (KeyValuePair<string, Type[]> item in Catalogo)
            {
                var objeto = new GameObject(item.Key);
                foreach (Type tipo in item.Value) objeto.AddComponent(tipo);
                PrefabUtility.SaveAsPrefabAsset(objeto, $"{Pasta}/{item.Key}.prefab");
                UnityEngine.Object.DestroyImmediate(objeto);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Prefabs] PRONTO {Catalogo.Count} prefabs");
        }
    }
}
