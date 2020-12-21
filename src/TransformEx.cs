using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SystemEx;
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

		public static IEnumerable<GameObject> FindGameObject(this Transform transform, string name)
			=> transform.FindGameObject(name.ToPath());

		public static IEnumerable<GameObject> FindGameObject(this Transform transform, string[] path)
		{
			GameObject root = transform.Elvis(t => t.gameObject);

			if (path.IsEmptyPath())
			{
				yield return root;
				yield break;
			}

			foreach (var pathi in path)
			{
				if (pathi == ".")
				{
					continue;
				}

				if (pathi == "..")
				{
					if (root == null)
						yield break;

					root = root.transform.parent.Elvis(t => t.gameObject);
					continue;
				}

				if (pathi.null_ws_())
				{
					root = null;
					continue;
				}

				if (pathi == "*")
				{
					foreach (Transform t in root.transform)
					{
						yield return t.gameObject;
					}
					yield break;
				}

				var rePathi = pathi.Replace("*", "*?");
				if (rePathi.Length > pathi.Length)
				{
					var re = new Regex(rePathi);
					foreach (Transform t in root.transform)
					{
						if (re.Match(t.gameObject.name).Success)
							yield return t.gameObject;
					}
					yield break;
				}

				if (root == null)
				{
					root = GameObject.Find("/" + pathi);
				}
				else
				{
					root = root.transform.Find(pathi).Elvis(t => t.gameObject);
				}

				if (root == null)
					yield break;
			}

			if (root != null)
				yield return root;

			Debug.LogWarning(string.Format("No child GameObject '{0}' found.", path.FromPath()));
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
