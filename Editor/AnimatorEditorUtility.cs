using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;


public class AnimatorEditorUtility
{
	public static AnimatorController FindAnimatorController(SerializedObject so)
	{
		var iter = so.GetIterator();

		while (iter.NextVisible(true))
		{
			if (iter.propertyType != SerializedPropertyType.ObjectReference || iter.objectReferenceValue == null)
			{
				continue;
			}
			if (iter.objectReferenceValue.GetType() == typeof(Animator))
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
			else if (iter.objectReferenceValue.GetType().BaseType == typeof(RuntimeAnimatorController))
			{
				var runtime = iter.objectReferenceValue as RuntimeAnimatorController;
				string assetPath = AssetDatabase.GetAssetPath(runtime);
				return AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
			}
		}
		return null;
	}

	public static void GetAllAnimatorControllers(List<AnimatorController> controllers)
	{
		if (controllers == null)
		{
			controllers = new List<AnimatorController>();
		}
		var guids = AssetDatabase.FindAssets($"t:{nameof(AnimatorController)}");
		foreach (var g in guids)
		{
			var path = AssetDatabase.GUIDToAssetPath(g);
			var asset = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
			if (!controllers.Contains(asset))
			{
				controllers.Add(asset);
			}
		}
	}
}

