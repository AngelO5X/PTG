#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

namespace GPFS.Editor
{
    public static class GradientUtility
    {
        public static bool IncludesKey(string value, string key) => value.Contains(key, StringComparison.OrdinalIgnoreCase);
        public static bool IsGradient(MaterialProperty property)
            => property.type == MaterialProperty.PropType.Texture
            && IncludesKey(property.name, Settings.GradientTextureKeyword);

        public static IEnumerable<Texture> GetTextures(Material target)
        {
            string assetPath = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(assetPath))
                yield break;

            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (UnityEngine.Object obj in subAssets)
            {
                if (obj is Texture texture)
                    yield return texture;
            }
        }

        public static void ApplyClean(Material target, MaterialProperty property) => ApplyClean(target, MaterialEditor.GetMaterialProperties(property.targets));
        public static void ApplyClean(Material target, MaterialProperty[] properties)
        {
            var gradients = properties.Where(p => IsGradient(p)).ToList();
            var allTextures = GetTextures(target).Where(t => IncludesKey(t.name, Settings.GradientTextureKeyword)).ToList();
            var used = allTextures.Where(t => gradients.Any(g => target.GetTexture(g.name)?.name.Split()[0].Equals(t.name.Split()[0], StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            var unused = allTextures.Where(t => !used.Any(t2 => t2.name.Equals(t.name))).ToList();
            var usedGroups = used.GroupBy(t => t.name).ToList();
            foreach (var group in usedGroups)
            {
                if (group.Count() > 1)
                {
                    foreach (var duplicate in group.Skip(1))
                        unused = unused.Append(duplicate).ToList();
                }
            }
            unused = unused.ToList();

            foreach (var texture in unused)
            {
                AssetDatabase.RemoveObjectFromAsset(texture);
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.Refresh();
            GUI.changed = true;
        }
    }
}
#endif