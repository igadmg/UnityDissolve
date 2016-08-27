using System;

namespace UnityDissolve
{
	class ResourceAttributecs : Attribute
	{
		public string name = string.Empty;
		
		public ResourceAttributecs(string name)
		{
			this.name = name;
		}
	}
}
