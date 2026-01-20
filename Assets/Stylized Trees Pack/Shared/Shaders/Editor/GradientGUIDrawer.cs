using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class GradientGUIDrawer : MaterialPropertyDrawer
{
	private readonly int resolution = 256;
	private static readonly Dictionary<string, Texture2D> lastKnownTextures = new System.Collections.Generic.Dictionary<string, Texture2D>();


	public override void OnGUI(Rect rect, MaterialProperty property, GUIContent label, MaterialEditor editor)
	{
		if (property.targets.Length != 1) return;
		var target = (Material)property.targets[0];
		string path = AssetDatabase.GetAssetPath(target);

		// Create a unique key for this material property
		string materialKey = $"{target.GetInstanceID()}_{property.name}";

		EditorGUI.BeginChangeCheck();

		// {AssetDatabase.AssetPathToGUID(path)}
		string textureName = $"{property.name} Texture";

		Gradient gradient = null;
		bool textureChanged = false;

		if (property.targets.Length == 1)
		{
			// Check if material has a texture assigned to this property
#if UNITY_2021_1_OR_NEWER
			var currentTexture = target.HasTexture(property.name) ? target.GetTexture(property.name) as Texture2D : null;
#else
			var currentTexture = target.GetTexture(property.name) as Texture2D;
#endif

			// Check if the texture reference has changed (e.g., from pasting material properties)
			if (lastKnownTextures.TryGetValue(materialKey, out Texture2D lastTexture))
			{
				if (lastTexture != currentTexture)
				{
					textureChanged = true;
					lastKnownTextures[materialKey] = currentTexture;
				}
			}
			else
			{
				lastKnownTextures[materialKey] = currentTexture;
				textureChanged = true;
			}

			if (currentTexture != null && currentTexture.name.StartsWith(textureName))
			{
				// Try to decode gradient from the current texture
				gradient = Decode(property, currentTexture.name, textureName);
			}
			else
			{
				// Fallback to loading texture from asset path
				var textureAsset = LoadTexture(path, textureName);
				if (textureAsset != null)
					gradient = Decode(property, textureAsset.name, textureName);
			}

			gradient ??= GetNewGradient();

			EditorGUI.showMixedValue = false;
		}
		else
			EditorGUI.showMixedValue = true;

		// Calculate rects for gradient field and button
		float buttonWidth = 60f;
		float spacing = 2f;
		Rect gradientRect = new Rect(rect.x, rect.y, rect.width - buttonWidth - spacing, rect.height);
		Rect buttonRect = new Rect(rect.x + rect.width - buttonWidth, rect.y, buttonWidth, rect.height);

		gradient = EditorGUI.GradientField(gradientRect, label, gradient);

		if (GUI.Button(buttonRect, "Clean"))
		{
			bool IncludesKey(string value, string key) => value.ToLower().Contains(key);
			bool IsGradient(MaterialProperty property) =>
				property.type == MaterialProperty.PropType.Texture
				&& IncludesKey(property.name, "gradienttexture");

			IEnumerable<Texture> GetTextures(Material target)
			{
				// Get all assets that are stored inside the material file
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

			var gradients = MaterialEditor.GetMaterialProperties(editor.targets).Where(p => IsGradient(p)).ToList();
			var allTextures = GetTextures(target).Where(t => IncludesKey(t.name, "gradienttexture")).ToList();
			var used = allTextures.Where(t => gradients.Any(g => target.GetTexture(g.name)?.name.Split()[0].Equals(t.name.Split()[0], StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
			var unused = allTextures.Where(t => !used.Any(t2 => t2.name.Equals(t.name))).ToList();
			// Add duplicate textures in 'used' to 'unused', keeping only one instance
			var usedGroups = used.GroupBy(t => t.name).ToList();
			foreach (var group in usedGroups)
			{
				if (group.Count() > 1)
				{
					// Skip the first, add the rest to unused
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
			GUI.changed = true; // Mark as changed to trigger update
		}

		// Force repaint if texture changed (e.g., from pasting material properties)
		if (textureChanged)
		{
			editor.Repaint();
		}

		if (EditorGUI.EndChangeCheck())
		{
			string encodedGradient = Encode(gradient);
			string fullAssetName = textureName + encodedGradient;
			if (!AssetDatabase.Contains(target)) return;

			FilterMode filterMode = gradient.mode == GradientMode.Blend ? FilterMode.Bilinear : FilterMode.Point;
			Texture2D textureAsset = GetTexture(path, textureName, filterMode);
			Undo.RecordObject(textureAsset, "Change Material Gradient");
			textureAsset.name = fullAssetName;
			BakeGradient(gradient, textureAsset);

			Material material = (Material)target;
			material.SetTexture(property.name, textureAsset);
			EditorUtility.SetDirty(material);

			// 🔁 Apply to all variants referencing this material
			string[] guids = AssetDatabase.FindAssets("t:Material");
			foreach (string guid in guids)
			{
				string matPath = AssetDatabase.GUIDToAssetPath(guid);
				Material possibleVariant = AssetDatabase.LoadAssetAtPath<Material>(matPath);
				if (possibleVariant == null || possibleVariant == material)
					continue;

				// Match if using the same shader and same base texture name
#if UNITY_2021_1_OR_NEWER
				var currentTexture = possibleVariant.HasTexture(property.name) ? possibleVariant.GetTexture(property.name) as Texture2D : null;
#else
				var currentTexture = possibleVariant.GetTexture(property.name) as Texture2D;
#endif

				if (currentTexture != null && possibleVariant.shader == material.shader &&
					currentTexture.name.StartsWith(textureName) &&
					currentTexture == material.GetTexture(property.name))
				{
					Undo.RecordObject(possibleVariant, "Update Variant Gradient");
					possibleVariant.SetTexture(property.name, textureAsset);
					EditorUtility.SetDirty(possibleVariant);
				}
			}
		}

		EditorGUI.showMixedValue = false;
	}


	private Gradient GetNewGradient()
	{
		var colorKeys = new GradientColorKey[2];
		var alphaKeys = new GradientAlphaKey[2];
		colorKeys[0] = new GradientColorKey(Color.black, 0f);
		alphaKeys[0] = new GradientAlphaKey(1, 0f);
		colorKeys[1] = new GradientColorKey(Color.white, 1f);
		alphaKeys[1] = new GradientAlphaKey(1, 1f);

		return new Gradient { colorKeys = colorKeys, alphaKeys = alphaKeys };
	}


	private Texture2D GetTexture(string path, string name, FilterMode filterMode)
	{
		Texture2D textureAsset = LoadTexture(path, name);

		if (textureAsset == null)
			textureAsset = CreateTexture(path, name, filterMode);

		textureAsset.filterMode = filterMode;

		if (textureAsset.width != resolution)
		{
#if UNITY_2021_2_OR_NEWER
			textureAsset.Reinitialize(resolution, 1);
#else
            textureAsset.Resize(resolution, 1);
#endif
		}

		return textureAsset;
	}

	private Texture2D CreateTexture(string path, string name, FilterMode filterMode)
	{
		Texture2D textureAsset = new Texture2D(resolution, 1, TextureFormat.ARGB32, false)
		{
			name = name,
			wrapMode = TextureWrapMode.Clamp,
			filterMode = filterMode
		};
		AssetDatabase.AddObjectToAsset(textureAsset, path);
		AssetDatabase.SaveAssets();
		AssetDatabase.ImportAsset(path);

		return textureAsset;
	}

	private string Encode(Gradient gradient) => gradient == null ? null : JsonUtility.ToJson(new GradientRepresentation(gradient));

	private Gradient Decode(MaterialProperty prop, string name, string textureName)
	{
		if (prop == null)
			return null;

#pragma warning disable 0168
		string json = name.Substring(textureName.Length);
		try
		{
			GradientRepresentation gradientRepresentation = JsonUtility.FromJson<GradientRepresentation>(json);
			return gradientRepresentation?.ToGradient(new Gradient());
		}
		catch (Exception _)
		{
			return null;
		}
#pragma warning restore 0168
	}

	private Texture2D LoadTexture(string path, string name)
	{
		var textures = AssetDatabase.LoadAllAssetsAtPath(path);
		var texture = textures.FirstOrDefault(asset => asset?.name?.StartsWith(name) ?? false);
		return texture == null ? null : texture as Texture2D;
	}

	private void BakeGradient(Gradient gradient, Texture2D texture)
	{
		if (gradient == null)
			return;

		for (int x = 0; x < texture.width; x++)
		{
			Color color = gradient.Evaluate((float)x / (texture.width - 1));
			for (int y = 0; y < texture.height; y++)
				texture.SetPixel(x, y, color);
		}

		texture.Apply();
	}


	[Serializable]
	class GradientRepresentation
	{
		public GradientMode mode;
		public ColorKey[] colorKeys;
		public AlphaKey[] alphaKeys;

		public GradientRepresentation()
		{

		}

		public GradientRepresentation(Gradient source)
		{
			FromGradient(source);
		}

		public void FromGradient(Gradient source)
		{
			mode = source.mode;
			colorKeys = source.colorKeys.Select(key => new ColorKey(key)).ToArray();
			alphaKeys = source.alphaKeys.Select(key => new AlphaKey(key)).ToArray();
		}

		public Gradient ToGradient(Gradient gradient)
		{
			gradient.mode = mode;
			gradient.colorKeys = colorKeys.Select(key => key.ToGradientKey()).ToArray();
			gradient.alphaKeys = alphaKeys.Select(key => key.ToGradientKey()).ToArray();

			return gradient;
		}

		[Serializable]
		public struct ColorKey
		{
			public Color color;
			public float time;

			public ColorKey(GradientColorKey source)
			{
				color = default;
				time = default;
				FromGradientKey(source);
			}

			public void FromGradientKey(GradientColorKey source)
			{
				color = source.color;
				time = source.time;
			}

			public GradientColorKey ToGradientKey()
			{
				GradientColorKey key;
				key.color = color;
				key.time = time;
				return key;
			}
		}

		[Serializable]
		public struct AlphaKey
		{
			public float alpha;
			public float time;

			public AlphaKey(GradientAlphaKey source)
			{
				alpha = default;
				time = default;
				FromGradientKey(source);
			}

			public void FromGradientKey(GradientAlphaKey source)
			{
				alpha = source.alpha;
				time = source.time;
			}

			public GradientAlphaKey ToGradientKey()
			{
				GradientAlphaKey key;
				key.alpha = alpha;
				key.time = time;
				return key;
			}
		}
	}
}
#endif