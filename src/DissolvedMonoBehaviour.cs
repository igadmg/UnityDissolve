using UnityEngine;



namespace UnityDissolve
{
	public class DissolvedMonoBehaviour : MonoBehaviour
	{
		protected virtual void Awake()
		{
			this.Dissolve();
		}
	}
}
