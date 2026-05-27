using System;
using UnityEngine;

namespace TW.Attributes
{
	public class VisualElementPathAttribute : PropertyAttribute
	{
		public bool isRelative = false;
		public Type targetType = null;
		public VisualElementPathAttribute() { }
		public VisualElementPathAttribute(bool findRelative)
		{
			isRelative = findRelative;
		}
		public VisualElementPathAttribute(bool findRelative, Type target)
		{
			isRelative = findRelative;
			targetType = target;
		}

		public VisualElementPathAttribute(Type target)
		{
			targetType = target;
		}
	}
}
