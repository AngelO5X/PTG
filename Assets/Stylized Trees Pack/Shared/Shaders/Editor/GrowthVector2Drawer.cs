using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class GrowthVector2Drawer : MaterialPropertyDrawer
{
	public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
	{
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = prop.hasMixedValue;

		Vector2 value = EditorGUI.Vector2Field(position, label, prop.vectorValue);

		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck())
			prop.vectorValue = value;
	}
}
#endif