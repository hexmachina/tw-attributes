using System.Collections.Generic;
using TW.Attributes;
using UnityEditor;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;

[CustomPropertyDrawer(typeof(AnimatorParameterAttribute))]
public class AnimatorParameterPropertyDrawer : PropertyDrawer
{

	List<AnimatorController> m_assets;
	//Animator _anim;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		//base.OnGUI(position, property, label);
		bool standard = true;
		if (property.propertyType != SerializedPropertyType.String && property.propertyType != SerializedPropertyType.Integer)
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

			if (target.gameObject.TryGetComponent(out Animator _anim))
			{
				var ovr = _anim.runtimeAnimatorController as AnimatorOverrideController;
				if (ovr)
				{
					animatorController = ovr.runtimeAnimatorController as AnimatorController;
				}
				else
				{
					animatorController = _anim.runtimeAnimatorController as AnimatorController;
				}
			}
			else
			{
				animatorController = FindAnimatorController(property.serializedObject);

			}
		}
		else if (property.serializedObject.targetObject.GetType().BaseType == typeof(ScriptableObject))
		{

			animatorController = FindAnimatorController(property.serializedObject);

		}

		if (animatorController)
		{
			standard = false;
			if (property.propertyType == SerializedPropertyType.String)
			{
				StringPopup(position, property, label, animatorController);
			}
			else
			{
				IntPopup(position, property, label, animatorController);
			}
		}
		else
		{
			if (m_assets == null)
			{
				m_assets = new List<AnimatorController>();
				var guids = AssetDatabase.FindAssets($"t:{nameof(AnimatorController)}");
				foreach (var g in guids)
				{
					var path = AssetDatabase.GUIDToAssetPath(g);
					var asset = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
					m_assets.Add(asset);
				}
			}

			if (m_assets.Count > 0)
			{
				standard = false;
				if (property.propertyType == SerializedPropertyType.String)
				{
					StringPopupList(position, property, label, m_assets);
				}
				else
				{
					IntPopupList(position, property, label, m_assets);
				}
			}
		}

		if (standard)
		{
			EditorGUI.PropertyField(position, property);
		}
		EditorGUI.EndProperty();
	}

	private AnimatorController FindAnimatorController(SerializedObject so)
	{
		var iter = so.GetIterator();

		while (iter.NextVisible(true))
		{
			if (iter.propertyType == SerializedPropertyType.ObjectReference)
			{
				if (iter.objectReferenceValue != null && iter.objectReferenceValue.GetType() == typeof(Animator))
				{
					var _anim = iter.objectReferenceValue as Animator;
					var ovr = _anim.runtimeAnimatorController as AnimatorOverrideController;
					if (ovr)
					{
						return ovr.runtimeAnimatorController as AnimatorController;
					}
					else
					{
						return _anim.runtimeAnimatorController as AnimatorController;
					}
				}
				else if (iter.objectReferenceValue != null && iter.objectReferenceValue.GetType().BaseType == typeof(RuntimeAnimatorController))
				{
					var runtime = iter.objectReferenceValue as RuntimeAnimatorController;
					string assetPath = AssetDatabase.GetAssetPath(runtime);
					return AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
				}
			}
		}
		return null;
	}

	private void StringPopup(Rect position, SerializedProperty property, GUIContent label, AnimatorController animatorController)
	{
		var parameterNames = new string[animatorController.parameters.Length];
		var parameterDetails = new string[animatorController.parameters.Length];

		for (int i = 0; i < animatorController.parameters.Length; i++)
		{
			parameterDetails[i] = animatorController.parameters[i].name + " (" + animatorController.parameters[i].type + ")";
			parameterNames[i] = animatorController.parameters[i].name;
		}

		string propertyString = property.stringValue;

		int index = 0;
		if (!string.IsNullOrEmpty(propertyString))
		{
			for (int i = 1; i < parameterNames.Length; i++)
			{
				if (parameterNames[i] == propertyString)
				{
					index = i;
					break;
				}
			}
		}
		index = EditorGUI.Popup(position, label.text, index, parameterDetails);

		property.stringValue = parameterNames[index];
	}

	private void IntPopup(Rect position, SerializedProperty property, GUIContent label, AnimatorController animatorController)
	{
		//var parameterNames = new string[animatorController.parameters.Length];
		var parameterDetails = new string[animatorController.parameters.Length];

		for (int i = 0; i < animatorController.parameters.Length; i++)
		{
			parameterDetails[i] = animatorController.parameters[i].name + " (" + animatorController.parameters[i].type + ")";
			//parameterNames[i] = animatorController.parameters[i].name;
		}

		int propertyInt = property.intValue;

		int index = 0;

		for (int i = 1; i < animatorController.parameters.Length; i++)
		{
			if (animatorController.parameters[i].nameHash == propertyInt)
			{
				index = i;
				break;
			}
		}

		index = EditorGUI.Popup(position, label.text, index, parameterDetails);

		property.intValue = animatorController.parameters[index].nameHash;
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

	private void StringPopupList(Rect position, SerializedProperty property, GUIContent label, List<AnimatorController> animatorController)
	{
		var parameterDetails = new List<string>();
		var parameterNames = new List<string>();
		var att = this.attribute as AnimatorParameterAttribute;

		for (int j = 0; j < animatorController.Count; j++)
		{

			for (int i = 0; i < animatorController[j].parameters.Length; i++)
			{
				if (att.filtered && att.parameterType != animatorController[j].parameters[i].type)
				{
					continue;
				}
				parameterDetails.Add($"{animatorController[j].name}/{animatorController[j].parameters[i].name} ({animatorController[j].parameters[i].type})");
				parameterNames.Add(animatorController[j].parameters[i].name);
			}
		}

		string propertyInt = property.stringValue;

		int index = 0;

		for (int i = 1; i < parameterNames.Count; i++)
		{
			if (parameterNames[i] == propertyInt)
			{
				index = i;
				break;
			}
		}

		index = EditorGUI.Popup(position, label.text, index, parameterDetails.ToArray());

		property.stringValue = parameterNames[index];
	}

	private void IntPopupList(Rect position, SerializedProperty property, GUIContent label, List<AnimatorController> animatorController)
	{
		var parameterDetails = new List<string>();
		var parameterHash = new List<int>();
		var att = this.attribute as AnimatorParameterAttribute;


		for (int j = 0; j < animatorController.Count; j++)
		{
			for (int i = 0; i < animatorController[j].parameters.Length; i++)
			{
				if (att.filtered && att.parameterType != animatorController[j].parameters[i].type)
				{
					continue;
				}
				parameterDetails.Add($"{animatorController[j].name}/{animatorController[j].parameters[i].name} ({animatorController[j].parameters[i].type})");
				parameterHash.Add(animatorController[j].parameters[i].nameHash);
			}
		}

		int propertyInt = property.intValue;

		int index = 0;

		for (int i = 1; i < parameterHash.Count; i++)
		{
			if (parameterHash[i] == propertyInt)
			{
				index = i;
				break;
			}
		}

		index = EditorGUI.Popup(position, label.text, index, parameterDetails.ToArray());
		if (index < 0 || index >= parameterHash.Count)
		{

			return;
		}
		property.intValue = parameterHash[index];

		//return parameterHash[index];
	}
	private string GetParameterNameByHash(int hash, List<AnimatorController> animatorController)
	{
		for (int i = 0; i < animatorController.Count; i++)
		{
			for (int j = 0; j < animatorController[i].parameters.Length; j++)
			{
				if (animatorController[i].parameters[j].nameHash == hash)
				{
					return animatorController[i].parameters[j].name;
				}
			}
		}
		return "Hash Not Found";
	}
}
