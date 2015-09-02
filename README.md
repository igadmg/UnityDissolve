UnityDissolve
=============

Unity3D Dissolve extension library.


Usage
-----

Checkout `git submodule` to your `Unity Project/Libraries` folder. Than make directory links of `Libraries\UnityDissolve\src\*` folders to your `Assets\Scripts\UnityDissolve\*` folder.


Dissolve
--------

Every object can be decomposed to it's components. Decomposition is the process of extracting objects `Components` and passing them as the parameters to decompose function.

For example if GameObject `obj` have MeshRenderer, MeshCollider and MyBehaviour components it can be decomposed by calling `Dissolve` function
	
	GameObject obj = ...;
	obj.Dissolve(_.a((MeshRenderer mr, MeshCollider mc, MyBehaviour mb) => {
		// do something...
	}));

If GameObject does not have requested Component `null` will be passed as a parameter. Multiple decompose functions can be applied to GameObject - they will be called in order of declaration.
This behaviour is used in `GameObject.New` funcion.

Another variant of decomposition is decomposition to an object. By calling `Dissolve` with some tagged object as argument it can be decomposed to its `Components` storing each in objects member.

	public class Description : MonoBehaviour
	{
		[Component("Icon")] SpriteRenderer icon;
		[Component("Description")] TextMesh description;
		
		void Awake()
		{
			this.Dissolve(); // this decompose object to itself

			// so
			// "Icon" child node will be found and icon set to it's PriteRenderer component if it exists.
			// equivalent to code 
			// var o = gameObject.Find("Icon");
			// icon = o != null ? o.GetComponent<SpriteRenderer>();
		}

		void Start()
		{
			description.text = "Item description";
		}
	}

list of objects can be decomposed also. `Dissolve` handles list creation by itself.


	public class Page : MonoBehaviour
	{
		[Component("ItemDescriptions")] IList<Description> descriptions;
		
		void Awake()
		{
			this.Dissolve(); // this decompose object to itself

			// equivalent to code
			// descriptions = new List<Description>();
			// foreach (var child in gameObject.Find("ItemDescriptions")) {
			// 	descriptions.Add(child.GetComponent<Description>());
			// }
		}

		void Start()
		{
			foreach (var description in descriptions) {
				description.UpdateDescription();
			}
		}
	}

just not to make many simple MonoBehaviurs for trivial tasks it is possible to decompose GameObjects to standalone classes

	public class Page : MonoBehaviour
	{
		// It's ok to make this class public, private, or internal
		// no struct are allowed here
		class Portrait
		{
			[Component("Icon")] SpriteRenderer icon;			
		}

		// WARN: this class must have public (or internal) visibility or Mono will fail to decompose it in WebPlayer builds.
		// beacuse it is used in IList<> component.
		public class Description
		{
			[Component] GameObject gameObject; // Component without "name" will link to object itself.
			[Component("Icon")] SpriteRenderer icon;
			[Component("Description")] TextMesh description;
		}

		[Component("ItemDescriptions")] IList<Description> descriptions;
		[Component("ItemPortrait")] Portrait portrait;
		
		void Awake()
		{
			this.Dissolve(); // this decompose object to itself
		}

		void Start()
		{
			foreach (var description in descriptions) {
				description.UpdateDescription();
			}
		}
	}

also any GameObject can be decomposed to any class or structure

	public class Description
	{
		[Component] GameObject gameObject; // Component without "name" will link to object itself.
		[Component("Icon")] SpriteRenderer icon;
		[Component("Description")] TextMesh description;
	}

	var description = new Description();
	instance.Dissolve(description);
	// or one in line
	var description = instance.Dissolve(new Description());

and finally GameObject can be decomposed to a structure of its components with simplified syntax


	[Component]
	public class Description
	{
		GameObject gameObject;
		MeshRenderer renderer;
		MeshCollider collider;
	}

	var description = new Description();
	instance.Dissolve(description);
	// or one in line
	var description = instance.Dissolve(new Description());
