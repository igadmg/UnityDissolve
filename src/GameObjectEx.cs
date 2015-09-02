using System;
using UnityEngine;

namespace UnityDissolve
{
	public static class GameObjectEx
	{
		public static object GetComponentOrThis(this GameObject o, Type type)
		{
			if (type != typeof(GameObject))
				return o.GetComponent(type);
			else
				return o;
		}

		public static object GetComponentOrAdd(this GameObject o, Type type)
		{
			if (type != typeof(GameObject)) {
				var c = o.GetComponent(type);
				if (c == null)
					c = o.AddComponent(type);
				return c;
			}
			else
				return o;
		}

	}
}
