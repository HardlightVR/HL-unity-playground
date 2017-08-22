using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialVelocity : MonoBehaviour
{
	public Vector3 startVelocity;
	public Rigidbody rb;
	public System.DateTime start;
	void Start()
	{
		start = System.DateTime.Now;
		rb.AddForce(startVelocity, ForceMode.VelocityChange);
	}

	void Update()
	{

	}
}
