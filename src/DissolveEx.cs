using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SystemEx;
using UnityEngine;

namespace UnityDissolve
{
	public static class DissolveEx
	{
		private static Dictionary<Type, DissolvedType> DissolveTypeCache = new Dictionary<Type, DissolvedType>();

		private static T DissolveImpl<T>(GameObject go, T o)
		{
			Transform transform = go.transform;

			DissolvedType dissolvedType;
			if (!DissolveTypeCache.TryGetValue(o.GetType(), out dissolvedType)) {
				dissolvedType = new DissolvedType(o.GetType());
				DissolveTypeCache.Add(o.GetType(), dissolvedType);
			}

			foreach (var fieldDescription in dissolvedType.AddComponentFields) {
				string objectPath = fieldDescription.Item1;
				FieldInfo field = fieldDescription.Item2;

				Component c = go.AddComponent(field.FieldType);
				field.SetValue(o, c);
            }

			foreach (var fieldDescription in dissolvedType.ComponentFields) {
				string objectPath = fieldDescription.Item1;
				FieldInfo field = fieldDescription.Item2;

				if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object))) {
					field.SetValue(o, transform.Find(objectPath, field.FieldType));
				}
				else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(IList<>)) {
					Type nodeType = field.FieldType.GetGenericArguments()[0];
					if (!nodeType.IsVisible) Debug.LogError(nodeType.FullName + " should be declared public or it will break Mono builds.");

					IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(nodeType));

					foreach (Transform child in transform.Find(objectPath)) {
						if (nodeType.IsSubclassOf(typeof(UnityEngine.Object))) {
							list.Add(child.gameObject.GetComponentOrThis(nodeType));
						}
						else {
							object node = Activator.CreateInstance(nodeType);
							list.Add(child.gameObject.Dissolve(node));
						}
					}

					field.SetValue(o, list);
				}
				else {
					object node = Activator.CreateInstance(field.FieldType);
					field.SetValue(o, go.Dissolve(node));
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

		public static T Dissolve<T>(this GameObject go, T o)
		{
			return DissolveImpl(go, o);
		}

		public static T Dissolve<T>(this T c) where T : Component
		{
			return DissolveImpl(c.gameObject, c);
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
