using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class GrowthSingleTileTextureDrawer : MaterialPropertyDrawer
{
	public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
	{
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = prop.hasMixedValue;

		float lineHeight = EditorGUIUtility.singleLineHeight;
		position.height = lineHeight;

		float tilingWidth = EditorGUIUtility.fieldWidth;
		float spacing = 4f;

		Rect textureRect = new Rect(
			position.x,
			position.y,
			position.width - tilingWidth - spacing,
			lineHeight
		);
		Rect tilingRect = new Rect(
			textureRect.xMax + spacing,
			position.y,
			tilingWidth,
			lineHeight
		);

		Texture texture = editor.TexturePropertyMiniThumbnail(textureRect, prop, label, string.Empty);

		float tiling = EditorGUI.FloatField(tilingRect, GUIContent.none, prop.textureScaleAndOffset.x);

		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck())
		{
			prop.textureValue = texture;
			prop.textureScaleAndOffset = new Vector4(
				tiling,
				tiling,
				prop.textureScaleAndOffset.z,
				prop.textureScaleAndOffset.w
			);
		}
	}
}
#endif