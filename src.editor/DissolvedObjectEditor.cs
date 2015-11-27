using SystemEx;
using UnityEditor;
using UnityEngine;

namespace UnityDissolve
{
	public class DissolvedObjectEditor
	{
		[MenuItem("Unity Dissolve/Dissolve All Objects on a Scene" )]
		private static void DissolveAllObjects()
		{
			foreach (var component in Resources.FindObjectsOfTypeAll<Component>()) {
				if (component.GetType().HasAttribute<DissolveInEditorAttribute>()) {
					component.Dissolve();
				}
			}
		}

		[MenuItem( "Unity Dissolve/Dissolve selected Object" )]
		private static void DissolveSelectedObject()
		{
			foreach ( var component in Resources.FindObjectsOfTypeAll<Component>() ) {
				if ( component.GetType().HasAttribute<DissolveInEditorAttribute>() ) {
					component.Dissolve();
				}
			}
		}
	}
}