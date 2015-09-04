using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using SystemEx;

[assembly: InternalsVisibleToAttribute("UnityDissolve.Test")]

namespace UnityDissolve
{
	internal class DissolvedType
	{
		public List<Tuple<string, FieldInfo>> AddComponentFields = new List<Tuple<string, FieldInfo>>();
		public List<Tuple<string, FieldInfo>> ComponentFields = new List<Tuple<string, FieldInfo>>();
		public List<Tuple<string, FieldInfo>> SubComponents = new List<Tuple<string, FieldInfo>>();

		public DissolvedType(Type type)
		{
			if (type.HasAttribute<ComponentAttribute>()) {
				foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
					Type fieldType = field.FieldType;
					bool fieldIsList = fieldType.IsList();
					Type fieldListItemType = fieldIsList ? fieldType.GetListItemType() : null;

					if (fieldType.IsSubclassOf(typeof(UnityEngine.Object))
						|| (fieldIsList && fieldListItemType.IsSubclassOf(typeof(UnityEngine.Object)))) {
						bool processed = false;

						foreach (var attribute in field.GetCustomAttributes(true)) {
							AddComponentAttribute aca = attribute as AddComponentAttribute;
							if (aca != null) {
								AddComponentFields.Add(Tuple.Create(aca.name, field));

								processed = true;
								continue;
							}

							ComponentAttribute ca = attribute as ComponentAttribute;
							if (ca != null) {
								ComponentFields.Add(Tuple.Create(ca.name, field));

								processed = true;
								continue;
							}
						}

						if (!processed) {
							ComponentFields.Add(Tuple.Create(string.Empty, field));
						}
					}
					else {
						ComponentAttribute ca = field.GetAttribute<ComponentAttribute>();
						if (ca != null) {
							SubComponents.Add(Tuple.Create(ca.name, field));
						}
					}
				}
			}
			else {
				foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
					Type fieldType = field.FieldType;
					bool isUnityObject = fieldType.IsSubclassOf(typeof(UnityEngine.Object));

					foreach (var attribute in field.GetCustomAttributes(true)) {
						AddComponentAttribute aca = attribute as AddComponentAttribute;
						if (aca != null) {
							if (isUnityObject) {
								AddComponentFields.Add(Tuple.Create(aca.name, field));
							}

							continue;
						}

						ComponentAttribute ca = attribute as ComponentAttribute;
						if (ca != null) {
							if (isUnityObject) {
								ComponentFields.Add(Tuple.Create(ca.name, field));
							}
							else {
								SubComponents.Add(Tuple.Create(ca.name, field));
							}
						}
					}
				}
			}
		}
	}
}