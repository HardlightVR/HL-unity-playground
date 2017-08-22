using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitchLocation : MonoBehaviour
{
	public enum PitchLocationType { Ground, Balcony }
	public PitchLocationType MyType = PitchLocationType.Ground;
	
	public void DisplayFiringEffect()
	{
		if (FiringEffect != null)
		{
			FiringEffect.Play();
		}
	}
	public ParticleSystem FiringEffect;

	protected void Start()
	{
		//Debug.Log(transform.name + "\n");
		//Debug.Log(transform.GetChild(0).name + "\n");
		//Debug.Log(transform.GetChild(0).GetChild(0).name + "\n");
		FiringEffect = transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position, 0.05f);
	}
}
