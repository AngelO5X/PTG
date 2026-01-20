using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class GrowthOptionalSliderDrawer : MaterialPropertyDrawer
{
	private readonly float Min, Max;
	private readonly float Value;
	private readonly bool Enabled;

	public GrowthOptionalSliderDrawer() : this(0, 1, 0, 0) { }
	public GrowthOptionalSliderDrawer(float min, float max) : this(min, max, 0, 0) { }
	public GrowthOptionalSliderDrawer(float min, float max, float value) : this(min, max, value, 0) { }
	public GrowthOptionalSliderDrawer(float min, float max, float value, float enabled)
	{
		Min = min;
		Max = max;
		Value = value;
		Enabled = enabled == 1;
	}

	public override void OnGUI(Rect rect, MaterialProperty prop, GUIContent label, MaterialEditor editor)
	{
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = prop.hasMixedValue;

		var values = prop.vectorValue;

		float toggleSize = EditorGUIUtility.singleLineHeight;
		float spacing = 4f;
		Rect sliderRect = new Rect(rect.x, rect.y, rect.width - toggleSize - spacing, rect.height);
		Rect toggleRect = new Rect(rect.x + rect.width - toggleSize, rect.y, toggleSize, toggleSize);

		bool toggle = values.w == 0 ? Enabled : values.z == 1;
		EditorGUI.BeginDisabledGroup(!toggle);
		values.y = EditorGUI.Slider(sliderRect, label, values.w == 0 ? Value : values.y, Min, Max);
		EditorGUI.EndDisabledGroup();
		var newToggle = EditorGUI.Toggle(toggleRect, toggle);
		values.z = newToggle ? 1 : 0;
		values.x = newToggle ? values.y : 0;

		if (values.w == 0) values.w = 1;

		if (EditorGUI.EndChangeCheck())
			prop.vectorValue = new Vector4(values.x, values.y, values.z, values.w);

		EditorGUI.showMixedValue = false;
	}
}
#endif