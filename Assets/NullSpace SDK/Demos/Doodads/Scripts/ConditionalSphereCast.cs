using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class ConditionalSphereCast : MonoBehaviour
	{
		public bool ObserveVelocity;
		[ConditionalHide("ObserveVelocity", true)]
		public VelocityObserver velocityCheck;
		[ConditionalHide("ObserveVelocity", true)]
		public float velocityThresholdTimeout = .25f;

		public bool ObserveAngularVelocity;
		[ConditionalHide("ObserveAngularVelocity", true)]
		public AngularVelocityObserver angularCheck;
		[ConditionalHide("ObserveAngularVelocity", true)]
		public float angularThresholdTimeout = .25f;

		public HapticSphereCast ConditionalComponent;

		void Start()
		{
			ConditionalComponent.SphereCastActive = false;
		}

		void Update()
		{
			bool valid = false;

			valid = CheckVelocity() || CheckAngular();
			if (valid && !ConditionalComponent.SphereCastActive)
			{
				ConditionalComponent.SphereCastActive = true;
			}
			else if (!valid && ConditionalComponent.SphereCastActive)
			{
				ConditionalComponent.SphereCastActive = false;
			}
		}

		private bool CheckVelocity()
		{
			if (ObserveVelocity)
			{
				return velocityCheck.WasObservedAboveThreshold(velocityThresholdTimeout);
			}
			return false;
		}

		private bool CheckAngular()
		{
			if (ObserveAngularVelocity)
			{

			}
			return false;
		}
	}
}