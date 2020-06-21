using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityDissolve
{
	public static class TransformEx
	{
		/// <summary>
		/// Finds GameObject by path name. And returns it's Component T if it exists.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="transform"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T Find<T>(this Transform transform, string name) where T : Component
		{
			var t = transform.Find(name);

			if (t != null)
				return t.gameObject.GetComponent<T>();

			Debug.LogWarning(string.Format("No child GameObject '{0}' found.", name));

			return null;
		}

		public static GameObject FindGameObject(this Transform transform, string name)
		{
			if (transform == null)
				return GameObject.Find("/" + name);

			if (name.StartsWith("/"))
				return GameObject.Find(name);

			if (name.StartsWith("../"))
				return FindGameObject(transform.parent, name.Substring(3));

			var t = !string.IsNullOrEmpty(name) ? transform.Find(name) : transform;

			if (t != null)
				return t.gameObject;

			Debug.LogWarning(string.Format("No child GameObject '{0}' found.", name));

			return null;
		}

		public static object Find(this Transform transform, string name, Type type)
		{
			var t = !string.IsNullOrEmpty(name) ? transform.Find(name) : transform;

			if (t != null)
				return t.gameObject.GetComponentOrThis(type);

			Debug.LogWarning(string.Format("No child GameObject '{0}' found.", name));

			return null;
		}

		public static IEnumerable<Transform> Find(this Transform transform, Func<Transform, bool> f)
		{
			foreach (Transform child in transform)
			{
				if (f(child))
					yield return child;
			}

			yield break;
		}
	}
}
