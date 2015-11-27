using System;
using System.Collections.Generic;

namespace UnityDissolve
{
	public static class DissolveTypeCache
	{
		internal static Dictionary<Type, DissolvedType> TypeCache = new Dictionary<Type, DissolvedType>();

		public static void Clear()
		{
			TypeCache.Clear();
		}
	}
}