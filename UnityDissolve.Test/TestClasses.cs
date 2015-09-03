using UnityEngine;

namespace UnityDissolve.Test
{
	[Component]
	public class SimpleDissolveClass
	{
		public GameObject gameObject;
		public MeshFilter meshFilter;
		private MeshCollider meshCollider;

		public string ommitedFieldString;
		private int ommitedFieldInt;
	}
}