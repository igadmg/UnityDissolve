using System;

namespace UnityDissolve
{
	public class DissolveAttribute : Attribute
	{
	}

	public class ComponentAttribute : DissolveAttribute
	{
		public string name = string.Empty;
		public bool isDefault = false;
		public bool isOptional = false;

		public ComponentAttribute()
		{
		}

		public ComponentAttribute(string name)
		{
			this.name = name;
		}
	}

	public class DefaultComponentAttribute : ComponentAttribute
	{
		public DefaultComponentAttribute()
			: base()
		{
			isDefault = true;
		}

		public DefaultComponentAttribute(string name)
			: base(name)
		{
			isDefault = true;
		}
	}

	public class OptionalComponentAttribute : ComponentAttribute
	{
		public OptionalComponentAttribute()
			: base()
		{
			isOptional = true;
		}

		public OptionalComponentAttribute(string name)
			: base(name)
		{
			isOptional = true;
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
