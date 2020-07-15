using System.Collections.Generic;
using System.Linq;
using SystemEx;
using UnityEngine;



namespace UnityDissolve
{
	public class DissolvedMonoBehaviour : MonoBehaviour
	{
		protected virtual void Awake()
		{
			this.Dissolve();
		}

		protected virtual void OnEnable()
		{
#if UNITY_EDITOR
			this.Dissolve();
#endif
		}
	}

	public static class DissolvedMonoBehaviourEx
	{
		public static IEnumerable<T> WhereIsTypeDissolvable<T>(this IEnumerable<T> en)
			where T : Component
			=> en.Where(component
				=> component.GetType().HasAttribute<DissolveInEditorAttribute>()
				|| component.GetType().HasAttribute<ExecuteInEditMode>());
	}
}
