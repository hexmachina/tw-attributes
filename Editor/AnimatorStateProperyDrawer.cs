using System.Collections.Generic;
using TW.Attributes;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;
[CustomPropertyDrawer(typeof(AnimatorStateAttribute))]
public class AnimatorStateProperyDrawer : PropertyDrawer
{
	Animator _anim;

	private int[] mTargetStates;
	private string[] mTargetStateNames;
	private Dictionary<int, int> mStateIndexLookup;

	List<AnimatorController> m_assets;

	class StateCollector
	{
		public List<int> mStates;
		public List<string> mStateNames;
		public Dictionary<int, int> mStateIndexLookup;
		public Dictionary<int, int> mStateParentLookup;

		public void CollectStates(AnimatorController ac, bool addName = false)
		{
			mStates = new List<int>();
			mStateNames = new List<string>();
			mStateIndexLookup = new Dictionary<int, int>();
			mStateParentLookup = new Dictionary<int, int>();

			mStateIndexLookup[0] = mStates.Count;
			mStateNames.Add("(default)");
			mStates.Add(0);

			if (ac == null)
				return;

			for (int i = 0; i < ac.layers.Length; i++)
			{
				AnimatorStateMachine fsm = ac.layers[i].stateMachine;
				string name = fsm.name;
				int hash = Animator.StringToHash(name);
				var index = mStateNames.Count;

				CollectStatesFromFSM(fsm, name + ".", hash, string.Empty);
				for (int j = index; j < mStateNames.Count; j++)
				{
					mStateNames[j] = mStateNames[j].Insert(0, ac.layers[i].name + "/");
					if (addName)
					{
						mStateNames[j] = mStateNames[j].Insert(0, ac.name + "/");

					}
				}
			}

		}

		void CollectStatesFromFSM(
			AnimatorStateMachine fsm, string hashPrefix, int parentHash, string displayPrefix)
		{
			ChildAnimatorState[] states = fsm.states;
			for (int i = 0; i < states.Length; i++)
			{
				AnimatorState state = states[i].state;
				int hash = AddState(Animator.StringToHash(hashPrefix + state.name),
					parentHash, displayPrefix + state.name);

				// Also process clips as pseudo-states, if more than 1 is present.
				// Since they don't have hashes, we can manufacture some.
				var clips = CollectClips(state.motion);
				if (clips.Count > 1)
				{
					string substatePrefix = displayPrefix + state.name + ".";
					foreach (AnimationClip c in clips)
						AddState(
							Animator.StringToHash(hash.ToString() + "_" + c.name),
							hash, substatePrefix + c.name);
				}
			}

			ChildAnimatorStateMachine[] fsmChildren = fsm.stateMachines;
			foreach (var child in fsmChildren)
			{
				string name = hashPrefix + child.stateMachine.name;
				string displayName = displayPrefix + child.stateMachine.name;
				int hash = AddState(Animator.StringToHash(name), parentHash, displayName);
				CollectStatesFromFSM(child.stateMachine, name + ".", hash, displayName + ".");
			}
		}

		List<AnimationClip> CollectClips(Motion motion)
		{
			var clips = new List<AnimationClip>();
			AnimationClip clip = motion as AnimationClip;
			if (clip != null)
				clips.Add(clip);
			UnityEditor.Animations.BlendTree tree = motion as UnityEditor.Animations.BlendTree;
			if (tree != null)
			{
				ChildMotion[] children = tree.children;
				foreach (var child in children)
					clips.AddRange(CollectClips(child.motion));
			}
			return clips;
		}

		int AddState(int hash, int parentHash, string displayName)
		{
			if (parentHash != 0)
				mStateParentLookup[hash] = parentHash;
			mStateIndexLookup[hash] = mStates.Count;
			mStateNames.Add(displayName);
			mStates.Add(hash);
			return hash;
		}
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		AnimatorController controller = null;
		var smb = property.serializedObject.targetObject as StateMachineBehaviour;
		var target = property.serializedObject.targetObject as MonoBehaviour;
		if (smb != null)
		{

			var context = AnimatorController.FindStateMachineBehaviourContext(smb);
			if (context != null && context.Length > 0)
			{
				controller = context[0].animatorController;
			}

		}
		else
		if (target != null)
		{

			if (target.gameObject.TryGetComponent(out _anim))
			{
				controller = GetControllerFromAnimator(_anim);

			}
			else
			{
				controller = FindAnimatorController(property.serializedObject);

			}

		}
		else
		{
			controller = FindAnimatorController(property.serializedObject);
		}
		if (controller != null)
		{
			//position.height = EditorGUIUtility.singleLineHeight;
			//EditorGUI.LabelField(position, label);
			//position.y += GetDefaultSpaceBetweenElements();
			UpdateTargetStates(controller);
			int currentState = GetStateHashIndex(property.intValue);
			int stateSelection = EditorGUI.Popup(position, label.text, currentState, mTargetStateNames);
			if (currentState != stateSelection)
				property.intValue = mTargetStates[stateSelection];
		}
		else
		{
			EditorGUI.PropertyField(position, property, label);
		}
		EditorGUI.EndProperty();
	}

	private float GetDefaultSpaceBetweenElements()
	{
		return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
	}

	private void UpdateTargetStates(AnimatorController controller)
	{

		//var ac = GetControllerFromAnimator(animator);
		StateCollector collector = new StateCollector();
		collector.CollectStates(controller);
		mTargetStates = collector.mStates.ToArray();
		mTargetStateNames = collector.mStateNames.ToArray();
		mStateIndexLookup = collector.mStateIndexLookup;
	}

	AnimatorController GetControllerFromAnimator(Animator animator)
	{
		if (animator == null)
			return null;
		var ovr = animator.runtimeAnimatorController as AnimatorOverrideController;
		if (ovr)
			return ovr.runtimeAnimatorController as AnimatorController;
		return animator.runtimeAnimatorController as AnimatorController;
	}

	private int GetStateHashIndex(int stateHash)
	{
		if (stateHash == 0)
			return 0;
		if (!mStateIndexLookup.ContainsKey(stateHash))
			return 0;
		return mStateIndexLookup[stateHash];
	}

	private AnimatorController GetAnimatorController(SerializedProperty property)
	{
		var iter = property.serializedObject.GetIterator();
		bool found = false;
		while (!found && iter.NextVisible(true))
		{
			if (iter.propertyType == SerializedPropertyType.ObjectReference)
			{
				if (iter.objectReferenceValue != null && iter.objectReferenceValue.GetType().BaseType == typeof(RuntimeAnimatorController))
				{
					//Debug.Log(iter.name);
					found = true;
					var runtime = iter.objectReferenceValue as RuntimeAnimatorController;
					string assetPath = AssetDatabase.GetAssetPath(runtime);
					return AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
				}
			}
			//iter.objectReferenceValue.GetType()
		}
		return null;
	}

	private void GetAssets()
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

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		//return GetDefaultSpaceBetweenElements() * 2;
		return base.GetPropertyHeight(property, label);
	}

}
