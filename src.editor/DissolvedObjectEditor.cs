using UnityEditor;
using UnityEngine;

namespace UnityDissolve
{
	[CustomEditor(typeof(Transform))]
	public class DissolvedObjectEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Dissolve")) {
			}
		}
	}
}