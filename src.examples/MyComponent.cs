using UnityEngine;

public class MyComponent : MonoBehaviour
{
	void Awake()
	{
		Debug.Log(string.Format("*** {0}:MyComponent: Awake", name));
	}

	void Start()
	{
		Debug.Log(string.Format("*** {0}:MyComponent: Start", name));
	}


	bool bUpdated = false;
	void Update()
	{
		if (!bUpdated) {
			Debug.Log(string.Format("*** {0}:MyComponent: Update", name));
			bUpdated = true;
		}
	}
}
