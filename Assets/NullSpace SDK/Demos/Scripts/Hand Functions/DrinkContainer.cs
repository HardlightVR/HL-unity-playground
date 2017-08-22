using NullSpace.SDK;
using System.Collections;
using System.Collections.Generic;
using VRTK;
using UnityEngine;

namespace NullSpace.SDK.Demos
{

	public class DrinkContainer : VRTK_InteractableObject
	{
		public ParticleSystem liquor;

		public AudioSource gulpSource;
		//public RotationTracker swishCalculator;
		public Explosion myExplosion;
		public Explodable myExplodable;
		public Destroyable myDestroyable;

		private VRTK_ControllerEvents controllerEvents;
		private ProximityTriggerAction ThirstQuencher;
		private float upsideDown;
		private bool BottleDestroyed = false;

		private float swishCounter= .35f;
		private float swishCooldown = .15f;

		public override void Grabbed(GameObject currentGrabbingObject)
		{
			if (BottleDestroyed)
				return;
			gameObject.transform.SetParent(null);
			gameObject.transform.localScale = Vector3.one;
			base.Grabbed(currentGrabbingObject);
			//controllerActions = grabbingObject.GetComponent<VRTK_ControllerActions>();
			
			controllerEvents = GetGrabbingObject().GetComponent<VRTK_ControllerEvents>();
		}

		public override void Ungrabbed(GameObject previousGrabbingObject)
		{
			base.Ungrabbed(previousGrabbingObject);
			Debug.LogError("Toggle kinematic\n");
			//ToggleKinematic(false);
		}

		protected override void Update()
		{
			if (Input.GetKeyDown(KeyCode.T))
			{
				StartCoroutine(Shatter(.15f));
			}
			base.Update();

			upsideDown = Vector3.Angle(transform.up, Vector3.up);

			if (upsideDown > 110 && !BottleDestroyed)
			{
				//Debug.Log("POURING\n");
				if (liquor != null)
				{
					liquor.emissionRate = Mathf.Clamp((int)(upsideDown - 110), 0, 50);
					//liquor.gameObject.SetActive(true);
					//Turn on the booze particles.

					//Set the rate based on the angle from 150 to 170.
				}
			}
			else
			{
				if (liquor != null)
				{
					liquor.emissionRate = 0;
					//liquor.gameObject.SetActive(false);
				}
			}

			//if (swishCalculator != null && DesertManager.Inst.GameOptions.WhiskeySwishes)
			//{
			//	if (swishCounter < 0)
			//	{
			//		float swishMag = swishCalculator.angularVelocity.magnitude;
			//		if (swishMag > 10)
			//		{
			//			//Debug.Log("Light Swish AngVel: " + swishCalculator.angularVelocity + "\t\n");
			//			WhiskeySwish("medium");
			//		}
			//		else if (swishMag > 5)
			//		{
			//			//Debug.Log("Medium Swish AngVel: " + swishCalculator.angularVelocity + "\t\n");
			//			WhiskeySwish("light");
			//		}
			//	}
			//	swishCounter -= Time.deltaTime;
			//}
		}

		public void CheckDrinkingWhiskey(Collider col)
		{
			if (upsideDown > 115 && !BottleDestroyed)
			{
				if (Rumbling == null)
				{
					Rumbling = StartCoroutine(StomacheRumble());
				}
			}
			else
			{
				StopDrinking();
			}
			//Debug.Log(col.gameObject.name + " is drinking whiskey\n" + upsideDown);
		}
		public void StopDrinking(Collider col)
		{
			StopDrinking();
		}

		public void StopDrinking()
		{
			if (!StoppingRumble && Rumbling != null)
			{
				StartCoroutine(StopRumble());


			}
		}

		private Coroutine Rumbling;
		private bool StoppingRumble;
		private bool Shattering = false;

		void Start()
		{
			ThirstQuencher = GetComponentInChildren<ProximityTriggerAction>();
			ThirstQuencher.triggerStayDelegate += CheckDrinkingWhiskey;
			ThirstQuencher.triggerExitDelegate += StopDrinking;

			gulpSource = AudioManager.Inst.MakeSource("Bottle/DrinkingInGulps");
			gulpSource.transform.SetParent(VRMimic.Instance.VRCamera.transform);
			gulpSource.transform.localPosition = Vector3.up * -.4f;
			gulpSource.volume = 0;
			gulpSource.loop = true;
			gulpSource.Play();

			myDestroyable = GetComponent<Destroyable>();
			myExplodable = GetComponent<Explodable>();
			myExplosion = GetComponent<Explosion>();

			//if (swishCalculator != null)
			//{
				//swishCalculator = transform.FindChild("bottle2").FindChild("Whiskey Swish").GetComponent<Swish>();
			//}
			//myExplodable.HitByExplosionDelegate += HitByExplosion;
		}

		public void StartShatter(float initialWait)
		{
			if (IsGrabbed())
			{
				GetGrabbingObject().GetComponent<VRTK_ControllerActions>().ToggleControllerModel(true, gameObject);
			}
			StopDrinking();
			if (!Shattering)
			{
				StartCoroutine(Shatter(initialWait));
			}
		}

		private void WhiskeySwish(string swishSize = "light")
		{
			AudioSource src = AudioManager.Inst.MakeSource("Bottle Swig/swish_" + swishSize);
			src.transform.SetParent(VRMimic.Instance.VRCamera.transform);
			src.transform.localPosition = Vector3.zero;
			src.Play();
			Destroy(src.gameObject, 2.0f);
			swishCounter = swishCooldown;
		}

		IEnumerator StomacheRumble()
		{
			gulpSource.volume = .85f;

			List<AreaFlag> locs = new List<AreaFlag>();
			locs.Add(AreaFlag.Lower_Ab_Left);
			locs.Add(AreaFlag.Lower_Ab_Right);
			locs.Add(AreaFlag.Mid_Ab_Left);
			locs.Add(AreaFlag.Mid_Ab_Right);

			while (true)
			{
				AreaFlag loc = locs[Random.Range(0, locs.Count)];
				string file = Random.Range(0, 1.0f) > .8f ? "Haptics/StomachRumbleHeavy" : "Haptics/StomachRumble";

				HapticSequence seq = new HapticSequence(file);
				seq.CreateHandle(loc).Play();

				float waitDur = Random.Range(.05f, .15f);

				yield return new WaitForSeconds(waitDur);
			}
		}

		IEnumerator StopRumble()
		{
			gulpSource.volume = 0f;
			StoppingRumble = true;
			yield return new WaitForSeconds(.85f);
			StopCoroutine(Rumbling);
			Rumbling = null;
			StoppingRumble = false;
		}

		IEnumerator Shatter(float initialWait)
		{
			AudioSource src = AudioManager.Inst.MakeSourceAtPos("Bottle Break/bottle_break_" + Random.Range(0, 3), transform.position);
			src.Play();

			//GameObject go = DesertManager.Inst.prefabDict.CreatePrefab("Effects/Glass/Broken Bottle");
			//go.transform.position = transform.position;
			//Destroy(go, 2.5f);
			yield return new WaitForSeconds(initialWait);
			Shattering = true;
			float delay = .15f;
			GetComponent<VRTK_RespawnObject>().StartDespawn(0.0f, delay);

			//NOTE: Keep this above the despawn delay (you don't want them shooting non-existant bottles
			yield return new WaitForSeconds(delay + 0.2f);

			//This allows the bottle to be shot again
			Shattering = false;
			myExplosion.TimesExploded = 0;
			if (myDestroyable != null)
			{
				myDestroyable.Reset();
			}

			//This resets our touching element if the hand was already in the bottle.
			if (IsTouched())
			{
				for (int i = 0; i < touchingObjects.Count; i++)
				{
					Ungrabbed(touchingObjects[i]);
				}
			}

			//Ensure the drinking coroutine ended.
			StopDrinking();
		}
	}
}