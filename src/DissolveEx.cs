using System;
using System.Collections;
using System.Collections.Generic;
using SystemEx;
using UnityEngine;

namespace UnityDissolve
{
	public static class DissolveEx
	{
		private static Dictionary<Type, object> DissolveTypeCache = new Dictionary<Type, object>();

		public static T Dissolve<T>(this Transform transform, T o)
		{
			if (o.GetType().HasAttribute<ComponentAttribute>()) {
				foreach (var field in o.GetType().GetFieldsAndAttributes<ComponentAttribute>()) {
					if (!field.Item1.FieldType.IsSubclassOf(typeof(UnityEngine.Object))) {
						Debug.LogWarning(field.Item1.Name + " is not a UnityObject. Only Component and GameObject members can be linked for type.");
						continue;
					}

					field.Item1.SetValue(o, transform.Find(field.Item2.name, field.Item1.FieldType));
				}
			}
			else
				foreach (var field in o.GetType().GetFieldsAndAttributes<ComponentAttribute>()) {
					if (field.Item1.FieldType.IsSubclassOf(typeof(UnityEngine.Object))) {
						field.Item1.SetValue(o, transform.Find(field.Item2.name, field.Item1.FieldType));
					}
					else {
						if (field.Item1.FieldType.IsGenericType && field.Item1.FieldType.GetGenericTypeDefinition() == typeof(IList<>)) {
							Type nodeType = field.Item1.FieldType.GetGenericArguments()[0];
							if (!nodeType.IsVisible) Debug.LogError(nodeType.FullName + " should be declared public or it will break Mono builds.");

							IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(nodeType));

							foreach (Transform child in transform.Find(field.Item2.name)) {
								if (nodeType.IsSubclassOf(typeof(UnityEngine.Object))) {
									list.Add(child.gameObject.GetComponentOrThis(nodeType));
								}
								else {
									object node = Activator.CreateInstance(nodeType);
									list.Add(child.Dissolve(node));
								}
							}

							field.Item1.SetValue(o, list);
						}
						else {
							object node = Activator.CreateInstance(field.Item1.FieldType);
							field.Item1.SetValue(o, transform.Dissolve(node));
						}
					}
				}

			return o;
		}

		public static GameObject Dissolve<T>(this GameObject o, Action<T> i)
		{
			i((T)o.GetComponentOrThis(typeof(T)));
			return o;
		}

		public static GameObject Dissolve(this GameObject o, ActionContainer i)
		{
			var prms = new object[i.args.Length];

			for (int ai = 0; ai < i.args.Length; ai++)
				prms[ai] = o.GetComponentOrThis(i.args[ai]);

			i.DynamicInvoke(prms);

			return o;
		}

		public static GameObject Dissolve(this GameObject o, params ActionContainer[] i)
		{
			for (int ii = 0; ii < i.Length; ii++)
				o.Dissolve(i[ii]);

			return o;
		}

		public static T Dissolve<T>(this GameObject c, T o)
		{
			return c.transform.Dissolve(o);
		}

		public static T Dissolve<T>(this T c) where T : Component
		{
			return c.transform.Dissolve(c);
		}

		public static T Dissolve<T, U>(this T c, Action<U> i) where T : Component
		{
			c.gameObject.Dissolve(i);
			return c;
		}

		public static T Dissolve<T>(this T c, ActionContainer i) where T : Component
		{
			c.gameObject.Dissolve(i);
			return c;
		}

		public static T Dissolve<T>(this T c, params ActionContainer[] i) where T : Component
		{
			c.gameObject.Dissolve(i);
			return c;
		}
	}
}
