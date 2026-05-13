using System;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace TW.Attributes
{

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class SRAttribute : PropertyAttribute
	{
		private static Dictionary<Type, Type[]> _typeCache = new Dictionary<Type, Type[]>();

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
			Type[] result;
			if (_typeCache.TryGetValue(type, out result))
				return result;

			if (type.IsInterface)
			{
				result = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => p != type && type.IsAssignableFrom(p) && type.IsAbstract == false).ToArray();
			}
			else
			{
				var assem = Assembly.GetAssembly(type);
				result = assem.GetTypes().Where(t => t.IsSubclassOf(type) && t.IsAbstract == false).ToArray();
				if (assem.GetName().Name != "Assembly-CSharp")
				{
					var main = GetAssemblyByName("Assembly-CSharp").GetTypes().Where(t => t.IsSubclassOf(type) && t.IsAbstract == false).ToArray();
#if UNITY_EDITOR
					UnityEditor.ArrayUtility.AddRange(ref result, main);
#endif
				}
			}

			if (result != null)
				_typeCache[type] = result;

			return result;
		}

		static Assembly GetAssemblyByName(string name)
		{
			return AppDomain.CurrentDomain.GetAssemblies().
				   SingleOrDefault(assembly => assembly.GetName().Name == name);
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
