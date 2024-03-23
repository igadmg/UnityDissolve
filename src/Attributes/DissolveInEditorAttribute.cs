using System;

namespace UnityDissolve
{
	/// <summary>
	/// Mark object as dissolvable for Unity menu -> Dissolve All objects on Scene
	/// All objects marked with this attributre or with ExecuteInEditor attribute will be dissolved.
	/// </summary>
	public class DissolveInEditorAttribute : Attribute
	{
	}
}
