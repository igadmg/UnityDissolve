using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SystemEx;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("UnityDissolve.Test")]

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
			foreach (var f in type.EnumDissolveFields())
			{
				Type fieldType = f.field.FieldType;
				if (fieldType.Namespace == "UniRx") // HACK to add support of reactive properties.
					fieldType = fieldType.GenericTypeArguments[0];
				bool fieldIsList = fieldType.IsList();
				bool fieldIsInterface = fieldType.IsInterface;
				Type fieldListItemType = fieldIsList ? fieldType.GetListItemType() : null;
				bool isUnityObject = fieldType.IsSubclassOf(typeof(UnityEngine.Object))
						|| (fieldIsList && fieldListItemType.IsSubclassOf(typeof(UnityEngine.Object)));

				if (isUnityObject || fieldIsInterface)
				{
					bool processed = true;

					foreach (var attribute in f.attributes)
					{
						switch (attribute)
						{
							case AddComponentAttribute aca:
								if (isUnityObject) AddComponentFields.Add(MakeAddComponentDissolveFieldDescription(aca.name, f.field));
								continue;
							case ComponentAttribute ca:
								ComponentFields.Add(MakeDissolveFieldDescription(ca.name, f.field));
								continue;
							case ResourceAttribute ra:
								if (isUnityObject) ResourceFields.Add(MakeResourceDissolveFieldDescription(ra.name, f.field));
								continue;
						}

						processed = false;
					}

					if (!processed)
					{
						ComponentFields.Add(MakeDissolveFieldDescription(string.Empty, f.field));
					}
				}
				else
				{
					ComponentAttribute ca = f.field.GetAttribute<ComponentAttribute>();
					if (ca != null)
					{
						SubComponents.Add(MakeDissolveSubComponentFieldDescription(ca.name, f.field));
					}
				}
			}
		}

		DissolveFieldDescription MakeAddComponentDissolveFieldDescription(string name, FieldInfo field)
		{
			DissolveFieldDescription fd = new DissolveFieldDescription();
			fd.Name = name;
			fd.Field = field;

			if (field.FieldType.IsSubclassOf(typeof(Component)))
			{
				fd.DissolveFn = (o, s, f, go) => {
					Component c = go.AddComponent(f.FieldType);
					f.SetValue(o, c);
				};
			}
			else
			{
				fd.DissolveFn = (o, s, f, go) => { Debug.LogWarningFormat("AddComponent: DissolveFn is not defined for field {0} type {1}", f.Name, f.FieldType.Name); };
			}

			return fd;
		}

		DissolveFieldDescription MakeDissolveFieldDescription(string name, FieldInfo field)
		{
			DissolveFieldDescription fd = new DissolveFieldDescription();
			fd.Name = name;
			fd.Field = field;
			if (fd.Field.FieldType.IsSubclassOf(typeof(Component)) || fd.Field.FieldType.IsInterface)
			{
				fd.DissolveFn = (o, s, f, go) => {
					GameObject fieldGameObject = go.transform.FindGameObject(s).FirstOrDefault();

					if (fieldGameObject != null)
					{
						f.SetValue(o, fieldGameObject.GetComponent(f.FieldType));
					}
					else
					{
						Debug.LogWarning($"'{go.name}': Component '{s}' not found.");
					}
				};
			}
			else if (fd.Field.FieldType == typeof(GameObject))
			{
				fd.DissolveFn = (o, s, f, go) => {
					GameObject fieldGameObject = go.transform.FindGameObject(s).FirstOrDefault();

					if (fieldGameObject != null)
					{
						f.SetValue(o, fieldGameObject);
					}
					else
					{
						Debug.LogWarning($"Component '{s}' not found.");
					}
				};
			}
			else if (fd.Field.FieldType.IsList())
			{
				fd.DissolveFn = (o, s, f, go) => {
					Type nodeType = f.FieldType.GetListItemType();
					if (!nodeType.IsVisible) Debug.LogError(nodeType.FullName + " should be declared public or it will break Mono builds.");

					IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(nodeType));

					if (s.null_ws_())
					{
						foreach (Component co in go.GetComponents(f.FieldType))
						{
							list.Add(co);
						}
					}
					else
					{
						var fieldGameObjects = go.transform.FindGameObject(s);

						foreach (var fgo in fieldGameObjects)
						{
							Component co = fgo.GetComponent(nodeType);
							if (co != null)
							{
								list.Add(co);
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
			else if (fd.Field.FieldType.Namespace == "UniRx") // HACK to add support of reactive properties.
			{
				var dfd = MakeDissolveFieldDescription(name, fd.Field.FieldType.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic));
				fd.DissolveFn = (o, s, f, go) => {
					Debug.LogWarningFormat("Component: DissolveFn is not defined for field {0} type {1}", f.Name, f.FieldType.Name);
					dfd.DissolveFn(f.GetValue(o), s, dfd.Field, go);
				};
			}
			else
			{
				fd.DissolveFn = (o, s, f, go) => {
					Debug.LogWarningFormat("Component: DissolveFn is not defined for field {0} type {1}", f.Name, f.FieldType.Name);
				};
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
				fd.DissolveFn = (o, s, f, go) => {
					Type nodeType = f.FieldType.GetListItemType();
					bool isComponent = nodeType.IsA<Component>();
					if (!nodeType.IsVisible) Debug.LogError(nodeType.FullName + " should be declared public or it will break Mono builds.");

					IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(nodeType));

					if (string.IsNullOrEmpty(s))
					{
						Debug.LogErrorFormat("SubComponent field dissolved without child path. Taht is definitely error.");
					}
					else
					{
						for (int i = 0; i < go.transform.childCount; i++)
						{
							Transform child = go.transform.GetChild(i);
							if (isComponent)
							{
								var childComponent = child.GetComponent(nodeType);
								if (childComponent != null)
									list.Add(childComponent);
							}
							else
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
				fd.DissolveFn = (o, s, f, go) => {
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

			if (field.FieldType == typeof(GameObject))
			{
				fd.DissolveFn = (o, s, f, go) => {
					f.SetValue(o, Resources.Load(s));
				};
			}
			else
			{
				fd.DissolveFn = (o, s, f, go) => { Debug.LogWarningFormat("Resource: DissolveFn is not defined for field {0} type {1}", f.Name, f.FieldType.Name); };
			}

			return fd;
		}
	}
}
