using System.Collections.Generic;
using SystemEx;
using UnityEngine;
using UnityEngineEx;

public class Example : MonoBehaviour
{
	[Component("Lamps")] IList<MeshRenderer> lamps;
	[Component("Button")] Button button;

	bool lampsAreOn = false;
	Color[] lampColors = new Color[] {
		Color.red,
		Color.green,
		Color.blue,
	};

	// Use this for initialization
	void Start()
	{
		this.Dissolve();

		button.OnCommand = () => {
			if (!lampsAreOn) {
				for (int i = 0; i < lamps.Count; i++) {
					lamps[i].material.color = lampColors[MathfEx.Repeat(i, lamps.Count)];
				}

				lampsAreOn = true;
			}
			else {
				for (int i = 0; i < lamps.Count; i++) {
					lamps[i].material.color = Color.white;
				}

				lampsAreOn = false;
			}
		};


		lamps[0].AddComponent<MyComponent>().Dissolve((GameObject go) => Debug.Log(string.Format("*** {0} is dissolved.", go.name)));
		lamps[1].AddComponent<MyComponent>(_.a((GameObject go) => Debug.Log(string.Format("*** {0} is dissolved.", go.name))));
	}


	// Update is called once per frame
	void Update()
	{
	
	}
}
