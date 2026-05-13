using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace TW.Attributes
{

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class SRAttribute : PropertyAttribute
	{

		public class TypeInfo
		{
			public Type Type;
			public string Path;
		}

		public TypeInfo[] Types { get; private set; }

		public SRAttribute()
		{
			Types = null;
		}

		public SRAttribute(Type baseType)
		{
			if (baseType == null)
			{
				Debug.LogError("[SRAttribute] Incorrect type.");
			}

			Types = GetTypeInfos(GetChildTypes(baseType));
		}

		public SRAttribute(params Type[] types)
		{
			if (types == null || types.Length <= 0)
			{
				Debug.LogError("[SRAttribute] Incorrect types.");
			}

			Types = GetTypeInfos(types);
		}

		public void SetTypeByName(string typeName)
		{
			if (string.IsNullOrEmpty(typeName))
			{
				Debug.LogError("[SRAttribute] Incorrect type name.");
			}
			var type = GetTypeByName(typeName);
			if (type == null)
			{
				Debug.LogError("[SRAttribute] Incorrect type.");
			}

			Types = GetTypeInfos(GetChildTypes(type));
		}

		public TypeInfo TypeInfoByPath(string path)
		{
			return Types != null ? Array.Find(Types, p => p.Path == path) : null;
		}

		public static TypeInfo[] GetTypeInfos(Type[] types)
		{
			if (types == null)
				return null;

			TypeInfo[] result = new TypeInfo[types.Length];

			for (int i = 0; i < types.Length; ++i)
			{
				result[i] = new TypeInfo { Type = types[i], Path = types[i].FullName };
			}

			return result;
		}

		public static Type[] GetChildTypes(Type type)
		{
			Type[] result = new Type[] { };
#if UNITY_EDITOR
			var collection = TypeCache.GetTypesDerivedFrom(type);
			for (int i = 0; i < collection.Count; i++)
			{
				var t = collection[i];
				if (t.IsAbstract)
				{
					continue;
				}
				UnityEditor.ArrayUtility.Add(ref result, t);

			}
#endif
			return result;
		}

		public static Type GetTypeByName(string typeName)
		{
			if (string.IsNullOrEmpty(typeName))
				return null;

			var typeSplit = typeName.Split(char.Parse(" "));
			var typeAssembly = typeSplit[0];
			var typeClass = typeSplit[1];

			return Type.GetType(typeClass + ", " + typeAssembly);
		}

		public virtual void OnCreate(object instance)
		{

		}

		public virtual void OnChange(object instance)
		{

		}
	}
}
