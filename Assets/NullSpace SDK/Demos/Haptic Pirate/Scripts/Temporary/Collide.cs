using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide : MonoBehaviour 
{
	void OnTriggerEnter(Collider col)
	{
		var vel = col.GetComponent<InitialVelocity>();
		if (vel != null)
		{
			Debug.Log((System.DateTime.Now - vel.start).TotalSeconds + "\n");

		}
	}
}
