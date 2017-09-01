using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class AngularVelocityObserver : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody _myRB;
		[SerializeField]
		private Vector3 rigidbodyAngularVelocity;

		[SerializeField]
		private float rigidbodyAngularVelocitySqrMag;

		private Vector3 lastRotation;
		private Vector3 rotationDelta;
		[SerializeField]
		public Vector3 calculatedAngularVelocity;
		[SerializeField]
		private float calculatedAngularVelocitySqrMag;

		public float ThresholdMagnitude = 10;

		public bool wasAboveThreshold = false;
		public bool AboveThreshhold = false;

		[SerializeField]
		private float TimeSinceLastAboveThreshold = float.MaxValue;

		public Rigidbody MyRB
		{
			get
			{
				if (_myRB == null)
				{
					FindMyRigidbody();
				}
				return _myRB;
			}
			set
			{
				if (value != null)
				{
					_myRB = value;
				}
				else Debug.LogError("Attempted to assign myRB to an invalid value.\n");
			}
		}
		void Start()
		{
			lastRotation = transform.rotation.eulerAngles;
		}
		void Update()
		{
			rotationDelta = transform.rotation.eulerAngles - lastRotation;
			calculatedAngularVelocity = rotationDelta / Time.deltaTime;
			calculatedAngularVelocitySqrMag = calculatedAngularVelocity.sqrMagnitude;
			lastRotation = transform.rotation.eulerAngles;

			rigidbodyAngularVelocity = MyRB.angularVelocity;
			rigidbodyAngularVelocitySqrMag = rigidbodyAngularVelocity.sqrMagnitude;

			CheckThreshold();
		}

		private void CheckThreshold()
		{
			AboveThreshhold = (rigidbodyAngularVelocitySqrMag > ThresholdMagnitude) || (calculatedAngularVelocitySqrMag > ThresholdMagnitude);

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
			_myRB = GetComponent<Rigidbody>();
		}
	}
}