using TW.Attributes;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnimatorLayerAttribute))]
public class AnimatorLayerPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		bool standard = true;
		if (property.propertyType != SerializedPropertyType.Integer)
		{
			var height = position.height;
			position.height = EditorGUIUtility.singleLineHeight * 2;
			EditorGUI.HelpBox(position, "This Attribute requires a string or int property.", MessageType.Warning);
			position.height = height - (EditorGUIUtility.singleLineHeight * 2);
			position.y += EditorGUIUtility.singleLineHeight * 2;
			//var rect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height + EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField(position, property);
			return;
		}

		AnimatorController animatorController = null;
		if (property.serializedObject.targetObject.GetType().BaseType == typeof(StateMachineBehaviour))
		{

			var smb = property.serializedObject.targetObject as StateMachineBehaviour;
			var context = AnimatorController.FindStateMachineBehaviourContext(smb);
			if (context != null && context.Length > 0)
			{
				animatorController = context[0].animatorController;
			}

		}
		else if (property.serializedObject.targetObject.GetType().BaseType == typeof(MonoBehaviour))
		{
			var target = property.serializedObject.targetObject as MonoBehaviour;
			var anim = target.gameObject.GetComponent<Animator>();
			if (anim)
			{
				var ovr = anim.runtimeAnimatorController as AnimatorOverrideController;
				if (ovr)
				{
					animatorController = ovr.runtimeAnimatorController as AnimatorController;
				}
				else
				{
					animatorController = anim.runtimeAnimatorController as AnimatorController;
				}
			}
		}
		//else if (property.serializedObject.targetObject.GetType().BaseType == typeof(ScriptableObject))
		//{
		//	var iter = property.serializedObject.GetIterator();
		//	bool found = false;
		//	while (!found && iter.NextVisible(true))
		//	{
		//		if (iter.propertyType == SerializedPropertyType.ObjectReference)
		//		{
		//			if (iter.objectReferenceValue != null && iter.objectReferenceValue.GetType().BaseType == typeof(RuntimeAnimatorController))
		//			{
		//				//Debug.Log(iter.name);
		//				found = true;
		//				var runtime = iter.objectReferenceValue as RuntimeAnimatorController;
		//				string assetPath = AssetDatabase.GetAssetPath(runtime);
		//				animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
		//			}
		//		}
		//		//iter.objectReferenceValue.GetType()
		//	}
		//}
		if (!animatorController)
		{
			animatorController = AnimatorEditorUtility.FindAnimatorController(property.serializedObject);
		}

		if (animatorController)
		{
			standard = false;
			EditorGUI.BeginProperty(position, label, property);
			IntPopup(position, property, label, animatorController);

			EditorGUI.EndProperty();
		}

		if (standard)
		{
			EditorGUI.PropertyField(position, property);
		}
	}

	private void IntPopup(Rect position, SerializedProperty property, GUIContent label, AnimatorController animatorController)
	{
		//var parameterDetails = new string[animatorController.parameters.Length];

		var layerNames = new string[animatorController.layers.Length + 1];
		layerNames[0] = $"<ANY LAYER>";
		for (int i = 1; i < layerNames.Length; i++)
		{
			//parameterDetails[i] = animatorController.parameters[i].name + " (" + animatorController.parameters[i].type + ")";
			layerNames[i] = animatorController.layers[i - 1].name;
		}

		//int propertyInt = ;

		int index = property.intValue + 1;


		index = EditorGUI.Popup(position, label.text, index, layerNames);

		property.intValue = index - 1;
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (property.propertyType != SerializedPropertyType.String && property.propertyType != SerializedPropertyType.Integer)
		{
			var height = base.GetPropertyHeight(property, label);
			return height + (EditorGUIUtility.singleLineHeight * 2);
		}
		else
		{
			return base.GetPropertyHeight(property, label);
		}
	}
}
