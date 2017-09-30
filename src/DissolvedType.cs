using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using SystemEx;
using UnityEngine;

[assembly: InternalsVisibleToAttribute("UnityDissolve.Test")]

namespace UnityDissolve
{
	internal struct DissolveFieldDescription
	{
		public string Name;
		public FieldInfo Field;
		public Action<object, string, FieldInfo, GameObject> DissolveFn;
	}

	internal class DissolvedType
	{
		public List<DissolveFieldDescription> AddComponentFields = new List<DissolveFieldDescription>();
		public List<DissolveFieldDescription> ComponentFields = new List<DissolveFieldDescription>();
		public List<DissolveFieldDescription> ResourceFields = new List<DissolveFieldDescription>();
		public List<DissolveFieldDescription> SubComponents = new List<DissolveFieldDescription>();

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
								AddComponentFields.Add(MakeAddComponentDissolveFieldDescription(aca.name, field));

								processed = true;
								continue;
							}

							ComponentAttribute ca = attribute as ComponentAttribute;
							if (ca != null) {
								ComponentFields.Add(MakeDissolveFieldDescription(ca.name, field));

								processed = true;
								continue;
							}

							ResourceAttribute ra = attribute as ResourceAttribute;
							if (ra != null) {
								ResourceFields.Add(MakeResourceDissolveFieldDescription(ra.name, field));

								processed = true;
								continue;
							}
						}

						if (!processed) {
							ComponentFields.Add(MakeDissolveFieldDescription(string.Empty, field));
						}
					}
					else {
						ComponentAttribute ca = field.GetAttribute<ComponentAttribute>();
						if (ca != null) {
							SubComponents.Add(MakeDissolveSubComponentFieldDescription(ca.name, field));
						}
					}
				}
			}
			else {
				foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
					Type fieldType = field.FieldType;
					bool isUnityObject = fieldType.IsSubclassOf(typeof(UnityEngine.Object))
						|| (fieldType.IsArray && fieldType.GetElementType().IsSubclassOf(typeof(UnityEngine.Object)));

					foreach (var attribute in field.GetCustomAttributes(true)) {
						AddComponentAttribute aca = attribute as AddComponentAttribute;
						if (aca != null) {
							if (isUnityObject) {
								AddComponentFields.Add(MakeAddComponentDissolveFieldDescription(aca.name, field));
							}

							continue;
						}

						ComponentAttribute ca = attribute as ComponentAttribute;
						if (ca != null) {
							if (isUnityObject) {
								ComponentFields.Add(MakeDissolveFieldDescription(ca.name, field));
							}
							else {
								SubComponents.Add(MakeDissolveSubComponentFieldDescription(ca.name, field));
							}

							continue;
						}

						ResourceAttribute ra = attribute as ResourceAttribute;
						if (ra != null) {
							ResourceFields.Add(MakeResourceDissolveFieldDescription(ra.name, field));

							continue;
						}
					}
				}
			}
		}

		DissolveFieldDescription MakeAddComponentDissolveFieldDescription(string name, FieldInfo field)
		{
			DissolveFieldDescription fd = new DissolveFieldDescription();
			fd.Name = name;
			fd.Field = field;

			if (field.FieldType.IsSubclassOf(typeof(Component))) {
				fd.DissolveFn = (o, s, f, go) => {
					Component c = go.AddComponent(f.FieldType);
					f.SetValue(o, c);
				};
			}
			else {
				fd.DissolveFn = (o, s, f, go) => { Debug.LogWarningFormat("AddComponent: DissolveFn is not defined for field {0} type {1}", f.Name, f.FieldType.Name); };
			}

			return fd;
		}

		DissolveFieldDescription MakeDissolveFieldDescription(string name, FieldInfo field)
		{
			DissolveFieldDescription fd = new DissolveFieldDescription();
			fd.Name = name;
			fd.Field = field;
			if (field.FieldType.IsSubclassOf(typeof(Component))) {
				fd.DissolveFn = (o, s, f, go) => { f.SetValue(o, go.GetComponent(f.FieldType)); };
			}
			else if (field.FieldType == typeof(GameObject)) {
				fd.DissolveFn = (o, s, f, go) => { f.SetValue(o, go); };
			}
			else if (field.FieldType.IsList()) {
				fd.DissolveFn = (o, s, f, go) => {
					Type nodeType = f.FieldType.GetListItemType();
					if (!nodeType.IsVisible) Debug.LogError(nodeType.FullName + " should be declared public or it will break Mono builds.");

					IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(nodeType));

					if (string.IsNullOrEmpty(s))
					{
						foreach (Component co in go.GetComponents(f.FieldType))
						{
							list.Add(co);
						}
					}
					else
					{
						Component c = o as Component;
						for (int i = 0; i < c.transform.childCount; i++)
						{
							Transform child = c.transform.GetChild(i);
							if (child.gameObject.name.StartsWith(s))
							{
								Component co = child.gameObject.GetComponent(nodeType);
								if (co != null)
								{
									list.Add(co);
								}
							}
						}
					}

					if (f.FieldType.IsArray)
					{
						f.SetValue(o, list.GetType().GetMethod("ToArray").Invoke(list, null));
					}
					else
					{
						f.SetValue(o, list);
					}
				};
			}
			else {
				fd.DissolveFn = (o, s, f, go) => { Debug.LogWarningFormat("Component: DissolveFn is not defined for field {0} type {1}", f.Name, f.FieldType.Name); };
			}


			return fd;
		}

		DissolveFieldDescription MakeDissolveSubComponentFieldDescription(string name, FieldInfo field)
		{
			DissolveFieldDescription fd = new DissolveFieldDescription();
			fd.Name = name;
			fd.Field = field;

			if (field.FieldType.IsList())
			{
				fd.DissolveFn = (o, s, f, go) =>
				{
					Type nodeType = f.FieldType.GetListItemType();
					if (!nodeType.IsVisible) Debug.LogError(nodeType.FullName + " should be declared public or it will break Mono builds.");

					IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(nodeType));

					if (string.IsNullOrEmpty(s))
					{
						Debug.LogErrorFormat("SubComponent field dissolved without child path. Taht is definitely error.");
					}
					else
					{
						Component c = o as Component;
						for (int i = 0; i < c.transform.childCount; i++)
						{
							Transform child = c.transform.GetChild(i);
							if (child.gameObject.name.StartsWith(s))
							{
								list.Add(child.gameObject.Dissolve(Activator.CreateInstance(nodeType)));
							}
						}
					}

					if (f.FieldType.IsArray)
					{
						f.SetValue(o, list.GetType().GetMethod("ToArray").Invoke(list, null));
					}
					else
					{
						f.SetValue(o, list);
					}
				};
			}
			else
			{
				fd.DissolveFn = (o, s, f, go) =>
				{
					object node = Activator.CreateInstance(f.FieldType);
					f.SetValue(o, go.Dissolve(node));
				};
			}

			return fd;
		}

		DissolveFieldDescription MakeResourceDissolveFieldDescription(string name, FieldInfo field)
		{
			DissolveFieldDescription fd = new DissolveFieldDescription();
			fd.Name = name;
			fd.Field = field;

			if (field.FieldType == typeof(GameObject)) {
				fd.DissolveFn = (o, s, f, go) => {
					f.SetValue(o, Resources.Load(s));
				};
			}
			else {
				fd.DissolveFn = (o, s, f, go) => { Debug.LogWarningFormat("Resource: DissolveFn is not defined for field {0} type {1}", f.Name, f.FieldType.Name); };
			}

			return fd;
		}
	}
}