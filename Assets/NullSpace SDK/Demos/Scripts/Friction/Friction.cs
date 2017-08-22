using UnityEngine;
using System.Collections.Generic;
using NullSpace.SDK;

namespace DesertOfDanger
{
	public class Friction : MonoBehaviour
	{
		//public HandSwitcher switcher;
		private SteamVR_TrackedObject trackedObject;
		public Camera hmdCam;
		private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObject.index); } }
		public GameObject Hand;
		//Head Mounted Display
		private SteamVR_TrackedObject hmd;
		private Vector3 _lastPosition;
		public float Limit = .05f;
		public bool Left = false;
		public bool ShowDebugTotals = false;

		public enum FrictionLocation { Hand, Forearm }
		public FrictionLocation FrictionLoc = FrictionLocation.Hand;

		public int numWithin = 0;
		public List<Collider> within;

		void Start()
		{
			Setup();
		}

		public void Setup()
		{
			//trackedObject = switcher.GetComponent<SteamVR_TrackedObject>();
			hmd = hmdCam.GetComponent<SteamVR_TrackedObject>();
			CheckLeftOrRight();
			within = new List<Collider>();
			//if (switcher == null)
			//{
				//switcher = transform.parent.GetComponent<HandSwitcher>();
			//}
		}

		public float CheckLeftOrRight()
		{
			//Debug.Log("Checking left or right\n");
			var right = Vector3.Cross(hmd.transform.rotation * Vector3.forward, Vector3.up);
			var controllerPos = trackedObject.transform.position - hmd.transform.position;

			//Debug.Log(Vector3.Dot(right, controllerPos) + "\n");

			//if (Vector3.Dot(right, controllerPos) < 0)
			//{
			//If right of it.
			return Vector3.Dot(right, controllerPos);
			//}
			//return false;
		}

		void Update()
		{
			if (within.Count > 0)
			{
				var currentPosition = Hand.gameObject.transform.position;
				var mag =
				Vector3.Distance(_lastPosition, currentPosition) / Time.deltaTime;

				var isMoving = mag > Limit;
				if (isMoving)
				{
					ushort total = (ushort)Mathf.Min((Mathf.Exp(mag * 4.8f) + 500), 3999);
					if (ShowDebugTotals && total > 600)
					{
						Debug.Log(total + "\n");
					}

					if (FrictionLoc == FrictionLocation.Hand)
					{
						controller.TriggerHapticPulse(total);

					//	HapticSequence seq = new HapticSequence(Left ? AreaFlag.Forearm_Left : //AreaFlag.Forearm_Right, .25f);
						//seq.AddEffect(0.0f, new HapticEffect("buzz", 0.0f, .1f));

						//NSManager.Instance.Suit.PlayEffect(Left ? AreaFlag.Forearm_Left : AreaFlag.Forearm_Right, Effect.Buzz_20);
					}
					else
					{
						//Effect eff = 
						//NSManager.Instance.Suit.PlayEffect(Left ? Location.Forearm_Left : Location.Forearm_Right, 
					}

				}
				_lastPosition = currentPosition;
			}
		}

		void OnTriggerEnter(Collider col)
		{
			if (col.gameObject.layer == 12)
			{
				//Debug.Log("Collided with\n" + col.gameObject.name);
				if (!within.Contains(col))
				{
					within.Add(col);
				}
				_lastPosition = Hand.gameObject.transform.position;
			}
		}

		void OnTriggerExit(Collider col)
		{
			if (col.gameObject.layer == 12)
			{
				if (within.Contains(col))
				{
					within.Remove(col);
				}
			}
		}
	}
}