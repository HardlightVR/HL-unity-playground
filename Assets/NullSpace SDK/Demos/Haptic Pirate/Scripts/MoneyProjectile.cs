using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using NUnit.Framework;

namespace NullSpace.SDK.Demos
{
	public class Money
	{
		private float _value;
		public float Value
		{
			get { return _value; }
			set { _value = value; }
		}
	}
	public enum MoneyTypes { SingleCoin, MoneyPack, MoneyBag, GoldBar, Gemstone, MinorJunk, MajorJunk }
	public class MoneyProjectile : VRTK_InteractableObject
	{
		//Types of Money
		public enum MoneyState { Available, Collected, Dropped }
		[Header("Money Projectile Variables")]
		public MoneyTypes TypeOfMoney;
		public MoneyState CurrentState;

		public bool CanBreak = true;

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
		public GameObject VisualsParent;
		public Collider[] MyColliders;

		public float collisionAudioVolume = .3f;
		private float angularVelocityMultiplier = 2f;

		public Vector3 BaseScale = Vector3.one * .5f;
		protected float minorBounceForce = 15;
		protected float megaBounceForce = 120;

		//Collision Counter (HP before break?)
		public float DurabilityRemaining = 1;

		private float LastAudio = .25f;

		//State of the money?
		[Header("$$$ BUY GOLD $$$")]
		public int MonetaryValue = 1;

		public bool Collected = false;

		private Transform pushTarget;
		protected virtual void Start()
		{
			FindMyRigidbody();
		}
		protected override void Update()
		{
			if (LastAudio > 0)
			{
				LastAudio -= Time.deltaTime;
			}
			if (CurrentState == MoneyState.Collected)
			{
				if (pushTarget != null)
				{
					PushMoney((pushTarget.transform.position - transform.position).normalized * 1f);
				}
			}
			base.Update();
		}
		private void FindMyRigidbody()
		{
			myRB = GetComponent<Rigidbody>();
		}
		public override void Grabbed(GameObject currentGrabbingObject)
		{
			base.Grabbed(currentGrabbingObject);
		}

		public void OnCollisionEnter(Collision collision)
		{
			//Tell us about the other thing
			Collide(collision);
		}

		public void Collide(Collision collision)
		{
			bool shouldPlayAudio = false;
			//If it is ground.
			if (collision.collider.gameObject.layer == 0)
			{
				shouldPlayAudio = true;
				DamageMoney();
				PushMoney(collision.contacts[0].normal * megaBounceForce);
			}
			else if (collision.collider.gameObject.layer == 31)
			{
				shouldPlayAudio = true;
				DamageMoney();
				PushMoney(collision.contacts[0].normal * megaBounceForce);
			}
			else if (collision.collider.gameObject.layer != 9 || collision.collider.gameObject.layer != 8)
			{
				shouldPlayAudio = true;
				//PushMoney(collision.contacts[0].normal * minorBounceForce);
			}
			else
			{
				//Decrement the collision counter
				shouldPlayAudio = true;

				//Bounce it away
				DamageMoney();
				PushMoney(collision.contacts[0].normal * megaBounceForce);
			}

			if (shouldPlayAudio)
			{
				PlayAudio();
			}
		}

		public void PushMoney(Vector3 direction)
		{
			MyRB.AddForce(direction);
		}

		public virtual void DamageMoney()
		{
			//Display a visual when it takes damage.
			//Debug.Log("Money damaged " + name + "  " + DurabilityRemaining + "\n");
			DurabilityRemaining -= 1;
			if (DurabilityRemaining <= 0)
			{
				if (CanBreak)
				{
					StartCoroutine(WasteMoney());
				}
			}
		}

		public void PrepareForDisposal(MoneyState newState)
		{
			CurrentState = newState;

			//Ungrab myself from anything holding me.

			//Set grabbable to false.
			isGrabbable = false;
		}

		public void DisableVisuals()
		{
			//Turn off our visible object.
			if (VisualsParent != null)
			{
				VisualsParent.SetActive(false);
			}
			//Perhaps lerp it out?
		}
		public void DisableCollision()
		{
			if (MyColliders != null)
			{
				for (int i = 0; i < MyColliders.Length; i++)
				{
					MyColliders[i].enabled = false;
				}
			}
		}
		public void AssignPhysicMaterial(PhysicMaterial material)
		{
			if (MyColliders != null)
			{
				for (int i = 0; i < MyColliders.Length; i++)
				{
					MyColliders[i].material = material;
				}
			}
		}

		private void PlayAudio()
		{
			if (LastAudio <= 0)
			{
				AudioSource src = AudioManager.Inst.MakeSource(HeistManager.Inst.GetRandomCollision(TypeOfMoney));
				src.volume = .3f;
				LastAudio = .25f;
				src.Play();
			}
		}

		protected virtual void OnTriggerEnter(Collider col)
		{

		}

		public override void Ungrabbed(GameObject previousGrabbingObject)
		{
			VRTK_ControllerEvents events = previousGrabbingObject.GetComponent<VRTK_ControllerEvents>();

			var velocity = VRTK_DeviceFinder.GetControllerVelocity(previousGrabbingObject);
			var angularVelocity = VRTK_DeviceFinder.GetControllerAngularVelocity(previousGrabbingObject);

			MyRB.AddForce(angularVelocity * angularVelocityMultiplier);

			base.Ungrabbed(previousGrabbingObject);
		}

		public GameObject CollectionVisuals(MoneyCollectionLocation.MoneyCollection collection)
		{
			GameObject newVisual = HeistManager.Inst.CreateCollectionVisual(MonetaryValue);

			newVisual.transform.position = collection.collisionLocation;
			newVisual.transform.SetParent(collection.collectionBin);
			newVisual.transform.LookAt(newVisual.transform.position);

			Destroy(newVisual, 5.0f);

			return newVisual;
		}

		public GameObject BrokenVisuals()
		{
			GameObject newVisual = HeistManager.Inst.CreateDestructionVisual(MonetaryValue);

			newVisual.transform.position = transform.position;
			newVisual.transform.SetParent(transform.parent);

			Destroy(newVisual, 2.0f);

			return newVisual;
		}

		private void CollectionAudio()
		{
			AudioSource src = AudioManager.Inst.MakeSource(HeistManager.Inst.GetRandomCollectionAudio(TypeOfMoney));
			src.volume = collisionAudioVolume;
			LastAudio = .25f;
			src.Play();
		}

		protected virtual void HandleCollection(MoneyCollectionLocation.MoneyCollection collection)
		{
			if (collection.collectionBin != null && collection.collectionBin.parent != null)
			{
				transform.SetParent(collection.collectionBin.parent);
				MyRB.isKinematic = true;
			}
			else
			{
				DisableVisuals();
				DisableCollision();
			}
		}

		public IEnumerator CollectMoney(MoneyCollectionLocation.MoneyCollection collection)
		{
			if (CurrentState == MoneyState.Available)
			{
				PrepareForDisposal(MoneyState.Collected);

				AssignPhysicMaterial(null);

				Vector3 pushVector = (collection.collectionFloor.transform.position - transform.position).normalized * minorBounceForce;
				//Debug.DrawLine(transform.position, transform.position + pushVector, Color.green, 5.0f);
				PushMoney(pushVector);
				pushTarget = collection.collectionFloor;

				//Debug.Log("$" + MonetaryValue + " Money was collected\n");
				HeistManager.Inst.ScoreDisplay.AddMoney(MonetaryValue);

				//Play collection visuals/audio
				CollectionVisuals(collection);
				CollectionAudio();

				myRB.drag = 0;
				myRB.mass *= 3;

				yield return new WaitForSeconds(2.5f);

				HandleCollection(collection);

				yield return null;
			}
		}
		
		public IEnumerator WasteMoney()
		{
			if (CurrentState == MoneyState.Available)
			{
				PrepareForDisposal(MoneyState.Dropped);

				//Should the player lose points?
				//HeistManager.Inst.ScoreDisplay.AddMoney(-MonetaryValue / 10);
				//Debug.Log("$" + MonetaryValue / 10 + " Money was wasted\n");

				//Play failure visuals

				yield return StartCoroutine(ShrinkOut());

				DisableVisuals();
				DisableCollision();

				Destroy(gameObject, 5.0f);

				yield return null;
			}
		}

		protected IEnumerator ShrinkOut(float time = .4f)
		{
			int frameCount = 20;
			float iMax = frameCount;
			time = Mathf.Clamp(time, .05f, float.MaxValue);
			for (int i = (int)(frameCount / 10); i < frameCount; i++)
			{
				float val = CurveCentral.Instance.alpha.Evaluate(i / iMax);
				transform.localScale = Vector3.Lerp(BaseScale, Vector3.zero, val);
				yield return new WaitForSeconds(time / iMax);
			}

			BrokenVisuals();

		}

	}
}