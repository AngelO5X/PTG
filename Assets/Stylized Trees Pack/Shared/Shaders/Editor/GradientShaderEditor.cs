using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using GPFS.Editor;
#endif
using System.Collections.Generic;

#if UNITY_EDITOR
public class GradientShaderEditor : ShaderGUI
{
	private Dictionary<string, GradientGUIDrawer> gradientGUIDrawers;

	public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
	{
		gradientGUIDrawers ??= new Dictionary<string, GradientGUIDrawer>();

		EditorGUI.BeginChangeCheck();

		foreach (MaterialProperty property in properties)
		{
			if (property.flags.HasFlag(MaterialProperty.PropFlags.HideInInspector)) continue;
			if (GradientUtility.IsGradient(property))
			{
				gradientGUIDrawers.TryGetValue(property.name, out GradientGUIDrawer drawer);
				if (drawer == null)
				{
					drawer = new GradientGUIDrawer();
					gradientGUIDrawers[property.name] = drawer;
				}
				drawer.OnGUI(EditorGUILayout.GetControlRect(), property, new GUIContent(property.displayName, ""), editor);
			}
			else
				editor.ShaderProperty(property, property.displayName);
		}

		base.OnGUI(editor, new MaterialProperty[0]);

		if (EditorGUI.EndChangeCheck())
			foreach (Object obj in editor.targets)
				EditorUtility.SetDirty(obj);
	}
}
#endif