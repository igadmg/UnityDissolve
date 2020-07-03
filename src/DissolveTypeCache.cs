using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SystemEx;

namespace UnityDissolve
{
	public static class DissolveTypeCache
	{
		internal static Dictionary<Type, DissolvedType> TypeCache = new Dictionary<Type, DissolvedType>();

		public static void Clear()
		{
			TypeCache.Clear();
		}

		public static IEnumerable<(FieldInfo field, DissolveAttribute[] attributes)> EnumDissolveFields(this Type type)
		{
			if (type.HasAttribute<ComponentAttribute>())
			{
				foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					yield return (field, field.GetCustomAttributes<DissolveAttribute>().ToArray());
				}
			}
			else
			{
				foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					var attributes = field.GetCustomAttributes<DissolveAttribute>().ToArray();
					if (attributes.Length > 0)
						yield return (field, attributes);
				}
			}
		}
	}
}
