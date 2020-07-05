using System.Linq;
using SystemEx;
using UnityEditor;
using UnityEngine;

namespace UnityDissolve
{
	public class DissolvedObjectEditor
	{
		[MenuItem("GameObject/Dissolve", false, -100)]
		static void Dissolve()
		{
			foreach (var component in Selection.activeGameObject.GetComponents<Component>()
				.WhereIsTypeDissolvable())
			{
				component.Dissolve();
			}
		}

		[MenuItem("GameObject/Dissolve", true)]
		static bool ValidateDissolve()
		{
			if (Selection.activeGameObject == null)
				return false;

			return Selection.activeGameObject.GetComponents<Component>()
				.WhereIsTypeDissolvable().Any();
		}

		[MenuItem("Dissolve/Dissolve All Objects on the Scene")]
		private static void DissolveAllObjects()
		{
			foreach (var component in Resources.FindObjectsOfTypeAll<Component>()
				.WhereIsTypeDissolvable())
			{
				component.Dissolve();
			}
		}

		[MenuItem("Dissolve/Dissolve Selected Object")]
		private static void DissolveSelectedObject()
		{
			foreach (var component in Selection.activeGameObject.GetComponents<Component>()
				.WhereIsTypeDissolvable())
			{
				component.Dissolve();
			}
		}

		[MenuItem("Dissolve/Clear Dissolve TypeCahce")]
		private static void ClearDissolveTypeCache()
		{
			DissolveTypeCache.Clear();
		}

		[MenuItem("CONTEXT/Component/Dissolve")]
		static void Dissolve(MenuCommand command)
		{
			var component = command.context as Component;

			if (component == null)
				return;

			component.Dissolve();

			EditorUtility.SetDirty(component);
		}
	}
}
