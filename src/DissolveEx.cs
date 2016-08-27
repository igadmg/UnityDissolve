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
		private static T DissolveImpl<T>(GameObject go, T o)
		{
			Transform transform = go.transform;

			DissolvedType dissolvedType;
			if (!DissolveTypeCache.TypeCache.TryGetValue( o.GetType(), out dissolvedType)) {
				dissolvedType = new DissolvedType(o.GetType());
				DissolveTypeCache.TypeCache.Add(o.GetType(), dissolvedType);
			}

			///////////////////////////////////////////////////////////////////////////////////
			// Process AddComponents first.
			foreach (var fieldDescription in dissolvedType.AddComponentFields) {
				string objectPath = fieldDescription.Item1;
				FieldInfo field = fieldDescription.Item2;

				Component c = go.AddComponent(field.FieldType);
				field.SetValue(o, c);
			}

			///////////////////////////////////////////////////////////////////////////////////
			// Process ComponentFields
			foreach (var fieldDescription in dissolvedType.ComponentFields) {
				string objectPath = fieldDescription.Item1;
				FieldInfo field = fieldDescription.Item2;
				GameObject fieldGameObject = transform.FindGameObject(objectPath);

				if (field.FieldType.IsSubclassOf(typeof(Component))) {
					field.SetValue(o, fieldGameObject.GetComponent(field.FieldType));
				}
				else if (field.FieldType == typeof(GameObject)) {
					field.SetValue(o, fieldGameObject);
				}
				else if (field.FieldType.IsList()) {
					Type nodeType = field.FieldType.GetListItemType();
					if (!nodeType.IsVisible) Debug.LogError(nodeType.FullName + " should be declared public or it will break Mono builds.");

					IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(nodeType));

					foreach (Component co in fieldGameObject.GetComponents(field.FieldType)) {
						list.Add(co);
					}

					field.SetValue(o, list);
				}
			}

			foreach (var fieldDescription in dissolvedType.SubComponents) {
				string objectPath = fieldDescription.Item1;
				FieldInfo field = fieldDescription.Item2;
				GameObject fieldGameObject = transform.FindGameObject(objectPath);

				object node = Activator.CreateInstance(field.FieldType);
				field.SetValue(o, fieldGameObject.Dissolve(node));
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