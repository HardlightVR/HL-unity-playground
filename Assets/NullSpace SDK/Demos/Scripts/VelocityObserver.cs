using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class VelocityObserver : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody myRB;
		[SerializeField]
		private Vector3 rigidbodyVelocity;

		[SerializeField]
		private float rigidbodyVelocitySqrMag;

		private Vector3 lastPosition;
		[SerializeField]
		public Vector3 calculatedVelocity;
		[SerializeField]
		private float calculatedVelocitySqrMag;

		public float ThresholdMagnitude = 10;

		public bool wasAboveThreshold = false;
		public bool AboveThreshhold = false;

		[SerializeField]
		private float TimeSinceLastAboveThreshold = float.MaxValue;
		 
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

		void Start()
		{
			lastPosition = transform.rotation.eulerAngles;
		}
		void Update()
		{
			calculatedVelocity = (transform.position - lastPosition) / Time.deltaTime;
			calculatedVelocitySqrMag = calculatedVelocity.sqrMagnitude;

			lastPosition = transform.position;

			rigidbodyVelocity = MyRB.velocity;
			rigidbodyVelocitySqrMag = rigidbodyVelocity.sqrMagnitude;

			CheckThreshold();
		}

		private void CheckThreshold()
		{
			AboveThreshhold = (rigidbodyVelocitySqrMag > ThresholdMagnitude) || (calculatedVelocitySqrMag > ThresholdMagnitude);
			if (!AboveThreshhold)
			{
				TimeSinceLastAboveThreshold += Time.deltaTime;
			}
			else if (!wasAboveThreshold)
			{
				TimeSinceLastAboveThreshold = 0;
			}

			wasAboveThreshold = AboveThreshhold;
		}

		public bool WasObservedAboveThreshold(float maxTimeSinceAboveThreshold = 0)
		{
			return AboveThreshhold || TimeSinceLastAboveThreshold <= maxTimeSinceAboveThreshold;
		}

		private void FindMyRigidbody()
		{
			myRB = GetComponent<Rigidbody>();
		}
	}
}