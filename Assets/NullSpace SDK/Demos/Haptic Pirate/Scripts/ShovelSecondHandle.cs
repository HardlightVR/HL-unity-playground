using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace NullSpace.SDK.Demos
{
	public class ShovelSecondHandle : VRTK_InteractableObject
	{
		public Shovel shovel;
		public float breakDistance = 0.12f;

		protected override void Update()
		{
			base.Update();
			if (grabbingObjects.Count > 0)
			{
				if (Vector3.Distance(grabbingObjects[0].transform.position, shovel.transform.position) > breakDistance)
				{
					ForceStopInteracting();
				}
			}
		}
	}
}