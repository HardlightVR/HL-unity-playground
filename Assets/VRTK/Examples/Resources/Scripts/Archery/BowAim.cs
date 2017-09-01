namespace VRTK.Examples.Archery
{
	using UnityEngine;
	using System.Collections;
	using NullSpace.SDK;

	public class BowAim : MonoBehaviour
	{
		public float powerMultiplier;
		public float pullMultiplier;
		public float pullOffset;
		public float maxPullDistance = 1.1f;
		public float bowVibration = 0.062f;
		public float stringVibration = 0.087f;

		private BowAnimation bowAnimation;
		private GameObject currentArrow;
		private BowHandle handle;

		private VRTK_InteractableObject interact;

		private VRTK_ControllerEvents holdControl;
		private VRTK_ControllerEvents stringControl;

		private VRTK_ControllerActions stringActions;
		private VRTK_ControllerActions holdActions;

		private Quaternion releaseRotation;
		private Quaternion baseRotation;
		private bool fired;
		private float fireOffset;
		private float currentPull;
		private float previousPull;

		[Header("Pulled Arrow Haptics")]
		public bool HapticEffectWhilePulled = false;
		private HapticPattern hapticwhilePulled = new HapticPattern();
		private HapticHandle pulledHandle;
		private float pulledHapticDuration = 8;
		private float pulledHapticCounter = 0;
		public string pulledHapticName = "Haptics/beating_heart_very_fast";
		[Range(0, 1.0f)]
		public float pulledHapticPullThreshold = 0.6f;

		[Header("Haptic On Draw and Release")]
		public bool HapticEffectWhenDrawAndReleased = false;
		public AreaFlag WhichSide = AreaFlag.Forearm_Right;
		private HapticSequence hapticOnDrawback = new HapticSequence();
		private HapticSequence hapticOnRelease = new HapticSequence();
		public string drawHaptic = "Haptics/click";
		public string releaseHaptic = "Haptics/double_click";
		//[Range(0, 1.0f)]
		//public float pullHapticPullThreshold = 0.6f;
		bool pulled = false;

		public VRTK_ControllerEvents GetPullHand()
		{
			return stringControl;
		}

		public bool IsHeld()
		{
			return interact.IsGrabbed();
		}

		public bool HasArrow()
		{
			return currentArrow != null;
		}

		public void SetArrow(GameObject arrow)
		{
			currentArrow = arrow;
		}

		private void Start()
		{
			hapticOnDrawback.LoadFromAsset(drawHaptic);
			hapticOnRelease.LoadFromAsset(releaseHaptic);
			hapticwhilePulled.LoadFromAsset(pulledHapticName);
			pulledHandle = hapticwhilePulled.CreateHandle();
			bowAnimation = GetComponent<BowAnimation>();
			handle = GetComponentInChildren<BowHandle>();
			interact = GetComponent<VRTK_InteractableObject>();
			interact.InteractableObjectGrabbed += new InteractableObjectEventHandler(DoObjectGrab);
		}

		private void DoObjectGrab(object sender, InteractableObjectEventArgs e)
		{
			if (VRTK_DeviceFinder.IsControllerLeftHand(e.interactingObject))
			{
				holdControl = VRTK_DeviceFinder.GetControllerLeftHand().GetComponent<VRTK_ControllerEvents>();
				stringControl = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_ControllerEvents>();

				holdActions = VRTK_DeviceFinder.GetControllerLeftHand().GetComponent<VRTK_ControllerActions>();
				stringActions = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_ControllerActions>();
			}
			else
			{
				stringControl = VRTK_DeviceFinder.GetControllerLeftHand().GetComponent<VRTK_ControllerEvents>();
				holdControl = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_ControllerEvents>();

				stringActions = VRTK_DeviceFinder.GetControllerLeftHand().GetComponent<VRTK_ControllerActions>();
				holdActions = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_ControllerActions>();
			}
			StartCoroutine("GetBaseRotation");
		}

		private IEnumerator GetBaseRotation()
		{
			yield return new WaitForEndOfFrame();
			baseRotation = transform.localRotation;
		}

		private void Update()
		{
			if (currentArrow != null && IsHeld())
			{
				AimArrow();
				AimBow();
				PullString();
				if (!stringControl.grabPressed)
				{
					currentArrow.GetComponent<Arrow>().Fired();
					fired = true;
					releaseRotation = transform.localRotation;
					Release();
				}
			}
			else if (IsHeld())
			{
				if (fired)
				{
					fired = false;
					fireOffset = Time.time;
				}
				if (!releaseRotation.Equals(baseRotation))
				{
					transform.localRotation = Quaternion.Lerp(releaseRotation, baseRotation, (Time.time - fireOffset) * 8);
				}
			}

			if (!IsHeld())
			{
				if (currentArrow != null)
				{
					Release();
				}
			}
		}

		private void Release()
		{
			bowAnimation.SetFrame(0);
			currentArrow.transform.SetParent(null);
			Collider[] arrowCols = currentArrow.GetComponentsInChildren<Collider>();
			Collider[] BowCols = GetComponentsInChildren<Collider>();
			foreach (var c in arrowCols)
			{
				c.enabled = true;
				foreach (var C in BowCols)
				{
					Physics.IgnoreCollision(c, C);
				}
			}
			currentArrow.GetComponent<Rigidbody>().isKinematic = false;
			currentArrow.GetComponent<Rigidbody>().velocity = currentPull * powerMultiplier * currentArrow.transform.TransformDirection(Vector3.forward);
			currentArrow.GetComponent<Arrow>().inFlight = true;
			currentArrow = null;
			currentPull = 0;

			ReleaseArrow();
		}

		private void ReleaseArrow()
		{
			if (stringControl.gameObject.GetComponent<VRTK_InteractGrab>())
			{
				stringControl.gameObject.GetComponent<VRTK_InteractGrab>().ForceRelease();
			}
			ReleaseArrowHaptics();
		}

		private void ReleaseArrowHaptics()
		{
			if (HapticEffectWhenDrawAndReleased)
			{
				pulledHandle.Stop();

				if (pulled)
				{
					pulled = false;
					var impulse = ImpulseGenerator.BeginEmanatingEffect(WhichSide, 2);
					impulse.WithEffect(hapticOnRelease).WithDuration(.25f).WithAttenuation(.8f);
					impulse.Play();
				}
			}
			ResetPulledHaptic();
		}

		private void AimArrow()
		{
			currentArrow.transform.localPosition = Vector3.zero;
			currentArrow.transform.LookAt(handle.nockSide.position);
		}

		private void AimBow()
		{
			transform.rotation = Quaternion.LookRotation(holdControl.transform.position - stringControl.transform.position, holdControl.transform.TransformDirection(Vector3.forward));
		}

		private void PullString()
		{
			currentPull = Mathf.Clamp((Vector3.Distance(holdControl.transform.position, stringControl.transform.position) - pullOffset) * pullMultiplier, 0, maxPullDistance);
			BowPullbackHaptics();
			BowFullyDrawnHaptic();

			bowAnimation.SetFrame(currentPull);

			if (!currentPull.ToString("F2").Equals(previousPull.ToString("F2")))
			{
				holdActions.TriggerHapticPulse(bowVibration);
				stringActions.TriggerHapticPulse(stringVibration);
			}
			previousPull = currentPull;
		}

		private void BowPullbackHaptics()
		{
			if (HapticEffectWhilePulled)
			{
				if (currentPull > maxPullDistance * pulledHapticPullThreshold)
				{
					if (pulledHapticCounter <= 0)
					{
						pulledHandle.Play();
					}

					pulledHapticCounter += Time.deltaTime;
					if (pulledHapticCounter >= pulledHapticDuration)
					{
						ResetPulledHaptic();
					}
				}
				else if (pulledHapticCounter >= 0)
				{
					ResetPulledHaptic();
				}
			}
		}

		private void BowFullyDrawnHaptic()
		{
			if (HapticEffectWhenDrawAndReleased)
			{
				if (currentPull > maxPullDistance * pulledHapticPullThreshold && !pulled)
				{
					pulled = true;
					var impulse = ImpulseGenerator.BeginEmanatingEffect(WhichSide, 2);
					impulse.WithEffect(hapticOnDrawback).WithDuration(.25f).WithAttenuation(.5f);
					pulledHandle = impulse.Play();
				}
				else if (currentPull > maxPullDistance * pulledHapticPullThreshold && pulled)
				{
					pulledHapticCounter += Time.deltaTime;
					if (pulledHapticCounter > .25f)
					{
						pulledHapticCounter = 0;
						pulledHandle.Replay();
					}
				}
				else if (currentPull < maxPullDistance * pulledHapticPullThreshold * .5f)
				{
					pulled = false;
				}
			}
		}

		private void ResetPulledHaptic()
		{
			if (HapticEffectWhilePulled)
			{
				pulledHandle.Stop();
				pulledHapticCounter = 0;
			}
		}
	}
}