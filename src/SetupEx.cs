using System;
using System.Collections.Generic;
using System.Reflection;
using SystemEx;
using UnityEngine;

namespace UnityDissolve {
	public static class SetupEx {
		/// <summary>
		/// Sets SerializeFields of the Component to values form parameters object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="c"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static T Setup<T>(this T c, object parameters) where T : Component {
			var fields = new Dictionary<string, FieldInfo>();
			foreach (var field in c.GetType().GetFields<SerializeField>()) {
				fields.Add(field.Name, field);
			}
			foreach (var property in parameters.GetType().GetProperties()) {
				if (fields.ContainsKey(property.Name)) {
					var field = fields[property.Name];
					field.SetValue(c, property.GetValue(parameters, null));
				}
				else {
					Debug.LogWarning(String.Format("Property [{0}] not found int type [{1}]", property.Name, typeof(T).Name));
				}
			}

			return c;
		}

		/// <summary>
		/// Sets SerializeFields of the Component to values form parameters dictionary.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="c"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static T Setup<T>(this T c, IDictionary<string, object> parameters) where T : Component {
			foreach (var field in c.GetType().GetFields<SerializeField>()) {
				object value;
				if (parameters.TryGetValue(field.Name, out value)) {
					field.SetValue(c, value);
				}
			}

			return c;
		}
	}
}
