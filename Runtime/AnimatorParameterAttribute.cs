using UnityEngine;

namespace TW.Attributes
{
	public class AnimatorParameterAttribute : PropertyAttribute
	{
		private bool _filtered = false;
		public bool filtered => _filtered;

		public AnimatorControllerParameterType parameterType;
		public AnimatorParameterAttribute()
		{
			_filtered = false;
		}

		public AnimatorParameterAttribute(AnimatorControllerParameterType type)
		{
			_filtered = true;
			parameterType = type;
		}
	}

}
