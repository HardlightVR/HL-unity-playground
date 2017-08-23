using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace NullSpace.SDK.Demos
{
	public class Beehive : VRTK_InteractableObject
	{
		[Header("Beehive")]
		public Vector3 currentVelocity;
		public Vector3 currentAngularVelocity;
		public float angularVelocitySqr;
		public float currentVelocitySqr;
		public bool moving;
		public bool tilting;

		private float radius = 2.5f;
		private float radiusSqr = 6.25f;
		private float playerDistanceSqr = float.MaxValue;

		[SerializeField]
		private float lastAssignedBeeCount = float.MaxValue;
		private float beeCount;
		private float disperseTimer = 1.0f;
		private float stingCounter;
		[SerializeField]
		private ParticleSystem[] particles;
		private DateTime lastAggravation;

		private HapticSequence sting = new HapticSequence();

		private float TimeSinceLastAggro;

		private Rigidbody myRB;

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

		private void FindMyRigidbody()
		{
			myRB = GetComponent<Rigidbody>();
		}

		protected virtual void Start()
		{
			SetBeeVisual();
			sting.LoadFromAsset("Haptics/pain_short");
		}

		protected override void Update()
		{
			UpdateTrackedValues();

			CheckIfMoving();
			CheckIfTilting();

			HandleAngryBees();

			UpdateBeeVisuals();

			HandleStinging();

			base.Update();
		}

		private void UpdateTrackedValues()
		{
			currentAngularVelocity = MyRB.velocity;
			currentVelocity = MyRB.angularVelocity;
			angularVelocitySqr = MyRB.angularVelocity.sqrMagnitude;
			currentVelocitySqr = MyRB.velocity.sqrMagnitude;

			playerDistanceSqr = (HardlightSuit.Find().transform.position - transform.position).sqrMagnitude;
		}

		private void CheckIfMoving()
		{
			if (currentVelocitySqr > 10)
			{
				moving = true;
			}
			else
			{
				moving = false;
			}
		}
		private void CheckIfTilting()
		{
			if (angularVelocitySqr > 10)
			{
				tilting = true;
			}
			else
			{
				tilting = false;
			}
		}

		private void HandleAngryBees()
		{
			if (moving || tilting)
			{
				lastAggravation = DateTime.Now;
				IncrementBeeCount();
			}
			else
			{
				DecrementBeeCount();
			}
		}
		private void IncrementBeeCount()
		{
			beeCount += Mathf.Clamp((500 - beeCount), 0, float.MaxValue) * (float)Time.deltaTime * .1f;
			Mathf.Clamp(beeCount, 0, int.MaxValue);
		}
		private void DecrementBeeCount()
		{
			if (beeCount > 0)
			{
				TimeSinceLastAggro = (float)(DateTime.Now - lastAggravation).TotalSeconds;
				if (TimeSinceLastAggro > Mathf.Min(beeCount / 10, 8))
				{
					if (beeCount > 4)
					{
						//Halve the bee population
						beeCount -= beeCount * TimeSinceLastAggro * .001f;
						Mathf.Clamp(beeCount, 0, int.MaxValue);
					}
					else
					{
						beeCount = 0;
					}
				}
			}
		}

		public void UpdateBeeVisuals()
		{
			if (Mathf.Abs(beeCount - lastAssignedBeeCount) > 10)
			{
				SetBeeVisual(beeCount / 10);
			}
			if (beeCount <= 0.5f && lastAssignedBeeCount > 0)
			{
				SetBeeVisual();
			}
		}
		private void SetBeeVisual(float setToValue = 0)
		{
			for (int i = 0; i < particles.Length; i++)
			{
				if (particles[i] != null)
				{
					var emission = particles[i].emission;
					emission.rateOverTime = setToValue;
					lastAssignedBeeCount = beeCount;
				}
			}
		}

		private void HandleStinging()
		{
			if (playerDistanceSqr < radiusSqr)
			{
				if (stingCounter >= 0)
				{
					stingCounter -= Time.deltaTime;
					if (stingCounter <= 0)
					{
						StingPlayer();
					}
				}
			}
		}

		private void StingPlayer()
		{
			var Where = HardlightSuit.Find().FindRandomLocation().Where;
			sting.Play(Where);

			float floor = Mathf.Max(.02f, .75f * (1 - beeCount / 500));
			float ceiling = Mathf.Max(.25f, 1.3f * (1 - beeCount / 500));
			stingCounter += UnityEngine.Random.Range(floor, ceiling);
			//Debug.Log(floor + "  " + ceiling + "  " + stingCounter + "\n", this);
		}
	}
}
