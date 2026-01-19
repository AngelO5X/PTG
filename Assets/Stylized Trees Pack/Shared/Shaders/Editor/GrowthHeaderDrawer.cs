using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class GrowthHeaderDrawer : MaterialPropertyDrawer
{
	public override void OnGUI(Rect rect, MaterialProperty prop, string label, MaterialEditor editor)
	{
		EditorGUILayout.Space(5);
		rect.y += 5;

		Rect headerRect = rect;
		headerRect.height += 10;

#if UNITY_2022_1_OR_NEWER
		headerRect.x -= 16f;
		headerRect.width += 16f;
#endif

		headerRect.xMin -= 8f;
		headerRect.width += 8f;

		Color startColor = new Color(0.2f, 0.2f, 0.2f, 1f);
		Color endColor = new Color(0.15f, 0.15f, 0.15f, 0f);

		Texture2D gradientTex = new Texture2D(2, 1);
		gradientTex.wrapMode = TextureWrapMode.Clamp;
		gradientTex.SetPixels(new[] { startColor, endColor });
		gradientTex.Apply();

		GUI.DrawTexture(headerRect, gradientTex, ScaleMode.StretchToFill);

		Handles.color = new Color(0f, 0f, 0f, .5f);
		Handles.DrawLine(new Vector2(headerRect.x, headerRect.yMin), new Vector2(headerRect.xMax, headerRect.yMin));
		Handles.color = new Color(1f, 1f, 1f, .125f);
		Handles.DrawLine(new Vector2(headerRect.x, headerRect.yMax), new Vector2(headerRect.xMax, headerRect.yMax));

		GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
		{
			normal = { textColor = Color.white },
			alignment = TextAnchor.MiddleLeft,
		};

		Rect labelRect = new Rect(headerRect.x + 10, headerRect.y, headerRect.width - 15, headerRect.height);
		GUI.Label(labelRect, label, labelStyle);

		EditorGUILayout.Space(10);
	}
}
#endif