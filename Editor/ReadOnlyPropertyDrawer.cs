using UnityEditor;
using UnityEngine;

namespace TW.Attributes
{

	/// <summary>
	/// This class contain custom drawer for ReadOnly attribute.
	/// </summary>
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute), true)]
	public class ReadOnlyDrawer : PropertyDrawer
	{
		/// <summary>
		/// Unity method for drawing GUI in Editor
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{

			EditorGUI.BeginDisabledGroup(true);
			// Drawing Property
			EditorGUI.PropertyField(position, property, label);
			EditorGUI.EndDisabledGroup();


		}
	}
}
