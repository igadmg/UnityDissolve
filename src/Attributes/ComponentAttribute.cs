using System;

namespace UnityDissolve
{
	public class DissolveAttribute : Attribute
	{
	}

	public class ComponentAttribute : DissolveAttribute
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

	public class AddComponentAttribute : DissolveAttribute
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
