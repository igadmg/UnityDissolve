UnityDissolve
=============

Unity3D Dissolve extension library.


Usage
-----

Build library dll files and place them in your `Assets/UnityDissolve` folder. Editor dll files should go to `Assets/UnityDissolve/Editor` forlder


Dissolve
--------

Every object can be decomposed to it's components. Decomposition is the process of extracting objects `Components` and passing them as the parameters to decompose function.
This library is build to ease `Component` mangament from code.

Basic Usage
-----------

Every `Component` can have different other `Component` references. And their values are usually setup from Unity Editor or via some initialization code.
This library provides mechanism to tag such component references via attributes and automaticaly fill their values.

For example you have some component `ComponentOne` which references other components in an object.

	public class ComponentOne : MonoBehaviour
	{
		[SerializeField]
		ComponentTwo componentTwo;

		[SerializeField]
		ComponentThree componentThree;
	}

Values of `componentTwo` and `componentThree` variables can be set in Unity Editor inspector, or via some initialization code, usisally done in `Awake()` function.

	void Awake()
	{
		componentTwo = gameObject.GetComponent<ComponentTwo>();
		componentThree = gameObject.GetComponent<ComponentThree>();
	}

Using UnityDissolve library one can tag this code with attributes a little to produce the same result.

	[DissolveInEditor]
	public class ComponentOne : MonoBehaviour
	{
		[SerializeField]
		[Component]
		ComponentTwo componentTwo;

		[SerializeField]
		[Component]
		ComponentThree componentThree;
	}

After that you can use `Unity Dissolve/Dissolve All Objects on a Scene` menu item to fill all such components in a scene and prefabs.

In more complex situations, when components are rtreived form child or parent objects relative path to an object can be set in `ComponentAttribute`

	[DissolveInEditor]
	public class ComponentOne : MonoBehaviour
	{
		[SerializeField]
		[Component("Child")]
		ComponentTwo componentTwo;

		[SerializeField]
		[Component("..")]
		ComponentThree componentThree;
	}

In such object components would be retreived from child GameObject with name `Child` and from parent object referenced by ".." path.

Also this behaviour can be used at runtime

	using UnityDissolve;

	// ...
	
	void Awake()
	{
		gameObject.Dissolve();
	}

Or even on arbitrarry classes, by providing the root object.

	using UnityDissolve;

	// ...
	
	void Awake()
	{
		SomtType collectComponents = new SomeType();
		gameObject.Dissolve(collectComponents);
	}

`SomeType` members should be tagged with attributes the same whay as described for `MonoBehaviour`
