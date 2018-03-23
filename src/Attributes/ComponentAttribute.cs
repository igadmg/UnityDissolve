using System;

namespace UnityDissolve
{
	public class ComponentAttribute : Attribute
	{
		public string name = string.Empty;

		public ComponentAttribute()
		{
		}

		public ComponentAttribute(string name)
		{
			this.name = name;
		}
	}

	public class AddComponentAttribute : Attribute
	{
		public string name = string.Empty;

		public AddComponentAttribute()
		{
		}

		public AddComponentAttribute(string name)
		{
			this.name = name;
		}
	}
}
