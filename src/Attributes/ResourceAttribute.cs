using System;

namespace UnityDissolve
{
	public class ResourceAttribute : Attribute
	{
		public string name = string.Empty;

		public ResourceAttribute(string name)
		{
			this.name = name;
		}
	}
}
