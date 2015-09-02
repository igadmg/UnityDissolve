using System;
using SystemEx;
using UnityEngine;
using UnityEngineEx;

[RequireComponent(typeof(MeshCollider))]
public class Button : MonoBehaviour
{
	public Action OnCommand;

	void Start()
	{
		this.Dissolve(_.a((MeshCollider mc, MeshFilter mf) => {
			if (mf != null) {
				mc.sharedMesh = mf.sharedMesh;
			}
		}));
	}
}
