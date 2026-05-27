using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TW.Attributes
{

	[CustomPropertyDrawer(typeof(VisualElementPathAttribute), true)]
	public class VisualElementPathDrawer : PropertyDrawer
	{
		private static readonly int s_TextFieldHash = "EditorTextField".GetHashCode();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var attr = attribute as VisualElementPathAttribute;
			if (property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.PropertyField(position, property, label);
				return;
			}
			Rect rect = new Rect(position.xMax - 19, position.y, 19, position.height);
			bool isOver = rect.Contains(Event.current.mousePosition);
			Event evt = Event.current;
			EventType eventType = evt.type;
			if (evt.type == EventType.Layout)
				return;

			if (eventType == EventType.MouseDown)
			{
				if (isOver)
				{

					GUIUtility.keyboardControl = 0;
					evt.Use();
					Dropdown(rect, property);

				}

			}
			if (isOver)
			{
				EditorGUIUtility.AddCursorRect(rect, MouseCursor.Arrow);
			}

			EditorGUI.PropertyField(position, property, label);
			GUIStyle thumbStyle = EditorStyles.objectFieldMiniThumb;
			switch (eventType)
			{
				case EventType.MouseDown:
					break;
				case EventType.MouseUp:
					break;
				case EventType.MouseMove:
					break;
				case EventType.MouseDrag:
					break;
				case EventType.KeyDown:
					break;
				case EventType.KeyUp:
					break;
				case EventType.ScrollWheel:
					break;
				case EventType.Repaint:
					int id = GUIUtility.GetControlID(s_TextFieldHash, FocusType.Keyboard, position);
					thumbStyle.Draw(rect, GUIContent.none, id, false, false);

					var buttonStyle = new GUIStyle(GUI.skin.FindStyle("ObjectFieldButton"));
					var content = EditorGUIUtility.IconContent("icon dropdown@2x");
					//@2x
					buttonStyle.normal.background = content.image as Texture2D;
					Rect buttonRect = buttonStyle.margin.Remove(rect);
					buttonStyle.Draw(buttonRect, GUIContent.none, id, false, buttonRect.Contains(Event.current.mousePosition));
					break;
				case EventType.Layout:
					break;
				case EventType.DragUpdated:
					break;
				case EventType.DragPerform:
					break;
				case EventType.DragExited:
					break;
				case EventType.Ignore:
					break;
				case EventType.Used:
					break;
				case EventType.ValidateCommand:
					break;
				case EventType.ExecuteCommand:
					break;
				case EventType.ContextClick:
					break;
				case EventType.MouseEnterWindow:
					break;
				case EventType.MouseLeaveWindow:
					break;
			}
		}

		private List<VisualTreeAsset> GetRelativeAssets(SerializedProperty property)
		{
			var m_assets = new List<VisualTreeAsset>();
			if (property.serializedObject.targetObject is MonoBehaviour)
			{
				var go = property.serializedObject.targetObject as MonoBehaviour;
				var comps = go.GetComponents<Component>();
				foreach (var comp in comps)
				{
					var so = new SerializedObject(comp);
					var iter = so.GetIterator();
					while (iter.NextVisible(true))
					{
						if (iter.propertyType != SerializedPropertyType.ObjectReference || iter.objectReferenceValue == null)
						{
							continue;
						}
						if (iter.objectReferenceValue is VisualTreeAsset)
						{
							m_assets.Add(iter.objectReferenceValue as VisualTreeAsset);
						}
					}
				}
				return m_assets;
			}

			var iterator = property.serializedObject.GetIterator();
			while (iterator.NextVisible(true))
			{
				if (iterator.propertyType != SerializedPropertyType.ObjectReference || iterator.objectReferenceValue == null)
				{
					continue;
				}
				if (iterator.objectReferenceValue is VisualTreeAsset)
				{
					m_assets.Add(iterator.objectReferenceValue as VisualTreeAsset);
				}
			}

			return m_assets;
		}

		private List<VisualTreeAsset> GetAssets()
		{
			var guids = AssetDatabase.FindAssets($"t: {typeof(VisualTreeAsset).Name}");
			var m_assets = new List<VisualTreeAsset>();
			foreach (var g in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(g);
				if (path.StartsWith("Packages"))
				{
					continue;
				}
				var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
				m_assets.Add(asset);
			}
			return m_assets;

		}


		private void Dropdown(Rect position, SerializedProperty property)
		{
			var attr = attribute as VisualElementPathAttribute;

			var contextMenu = new GenericMenu();

			var m_assets = attr.isRelative ? GetRelativeAssets(property) : GetAssets();

			foreach (var asset in m_assets)
			{
				var so = new SerializedObject(asset);
				var listProp = so.FindProperty("m_VisualElementAssets");
				for (int i = 0; i < listProp.arraySize; i++)
				{
					var el = listProp.GetArrayElementAtIndex(i);
					var elPathProp = el.FindPropertyRelative("m_FullTypeName");
					if (attr.targetType != null && elPathProp.stringValue != attr.targetType.FullName)
					{
						continue;
					}
					var properies = el.FindPropertyRelative("m_Properties");
					for (int j = 0; j < properies.arraySize; j++)
					{
						var propEl = properies.GetArrayElementAtIndex(j);
						if (propEl.stringValue == "name")
						{
							var nextEl = properies.GetArrayElementAtIndex(j + 1);
							var name = nextEl.stringValue;
							contextMenu.AddItem(new GUIContent($"{asset.name}/{name} ({elPathProp.stringValue.Split('.').Last()})"), property.stringValue == name, () =>
							{
								property.stringValue = name;
								property.serializedObject.ApplyModifiedProperties();

							});
						}
					}
				}
			}
			contextMenu.DropDown(position);

		}
	}
}

