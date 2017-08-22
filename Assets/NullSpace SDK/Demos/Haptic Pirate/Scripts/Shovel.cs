using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace NullSpace.SDK.Demos
{
	public class Shovel : VRTK_InteractableObject
	{
		public Rigidbody myRB;
		public Rigidbody MyRB
		{
			get
			{
				if (myRB == null)
				{
					FindMyRigidbody();
				}
				return myRB;
			}
			set
			{
				if (value != null)
				{
					myRB = value;
				}
				else Debug.LogError("Attempted to assign myRB to an invalid value.\n");
			}
		}

		public VRTK_ControllerActions leftActions;
		public VRTK_ControllerActions rightActions;
		public bool PrimaryLeft;

		public bool PrimaryRight;
		public bool Digged = false;

		private Vector3 currentVelocity;
		private Vector3 currentAngularVelocity;
		private void FindMyRigidbody()
		{
			myRB = GetComponent<Rigidbody>();
		}

		protected override void Awake()
		{
			leftActions = VRTK_DeviceFinder.GetControllerLeftHand().GetComponent<VRTK_ControllerActions>();
			rightActions = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_ControllerActions>();
			base.Awake();
		}

		protected override void Update()
		{
			currentVelocity = MyRB.velocity;
			currentAngularVelocity = MyRB.angularVelocity;

			//Debug.Log(MyRB.velocity + "\n" + MyRB.angularVelocity);

			base.Update();
		}

		public void Dig()
		{
			//Debug.Log("Attempted Dig\n", this);
			//Are we held.
			//if (IsGrabbed())
			//{
			HapticSequence seq = new HapticSequence();
			seq.AddEffect(.25f, new HapticEffect(Effect.Buzz));
			AreaFlag Where = PrimaryLeft ? AreaFlag.Forearm_Left : AreaFlag.Forearm_Right;
			seq.Play(Where);
			//}
			Digged = true;

			leftActions.TriggerDecayingHapticPulse(1f, .5f, .02f, .8f);
			rightActions.TriggerDecayingHapticPulse(1f, .5f, .02f, .8f);
			
		}

		public void Unearth()
		{
			if (Digged && currentAngularVelocity.magnitude > .5f)
			{
				//Debug.Log("Unearthed from angle\n");
				//Play haptic
				//Play visual
			}
			Digged = false;
		}
	}
}