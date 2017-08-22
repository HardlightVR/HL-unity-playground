using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace NullSpace.SDK.Demos
{
	public class ShovelBlade : MonoBehaviour
	{
		public Shovel myShovel;
		public bool OnEnter;
		public bool OnExit;

		void OnTriggerEnter(Collider col)
		{
			if (OnEnter)
			{
				//Debug.Log(col.gameObject.name + "  " + col.gameObject.layer + "\n", this);
				if (col.gameObject.layer == 11)
				{
					//Debug.Log(col.gameObject.name + "  " + col.gameObject.layer + "\n");

					if (myShovel)
					{
						myShovel.Dig();
					}
				}
			}
		}

		void OnTriggerExit(Collider col)
		{
			if (OnExit)
			{
				if (col.gameObject.layer == 11)
				{
					if (myShovel)
					{
						myShovel.Unearth();
					}
				}
			}
		}
	}
}