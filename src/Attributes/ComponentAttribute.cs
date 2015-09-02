using System;

namespace UnityDissolve
{
	public class ComponentAttribute : Attribute
	{
		public string name = null;
		
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
		public AddComponentAttribute()
		{
		}
	}
}