using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VRTK
{
	public class VRTK_FrictionController : MonoBehaviour
	{
		public VRTK_ControllerActions controllerActions;
		public VRTK_ControllerEvents controllerEvents;

		public List<VRTK_FrictionObject> frictionObjects;
		private Vector3 myLastTouchingPosition = Vector3.zero;

		public LayerMask AllowedLayers = 0;

		void Start()
		{
			if (controllerActions == null)
			{
				controllerActions = GetComponent<VRTK_ControllerActions>();
				if (controllerActions == null && transform.parent != null)
				{
					controllerActions = transform.parent.GetComponent<VRTK_ControllerActions>();
				}
			}
			if (controllerEvents == null)
			{
				controllerEvents = GetComponent<VRTK_ControllerEvents>();
				if (controllerEvents == null && transform.parent != null)
				{
					controllerEvents = transform.parent.GetComponent<VRTK_ControllerEvents>();
				}
			}

			frictionObjects = new List<VRTK_FrictionObject>();
		}

		public void Update()
		{
			if (frictionObjects != null)
			{
				if (frictionObjects.Count > 0)
				{
					//CheckTouchedObjectList();

					if (frictionObjects[0] != null && !frictionObjects[0].IsGrabbed && frictionObjects[0].enabled)
					{
						VRTK_FrictionObject friction = frictionObjects[0];
						//Debug.Log(frictionObjects[0].name + "\n");

						float dist = (myLastTouchingPosition - transform.position).magnitude * 200;
						//If we're moving
						if (dist > friction.minimumVelocity)
						{
							ushort total = (ushort)Mathf.Clamp(dist * friction.velocityMultiplier, friction.hapticDensity.x, friction.hapticDensity.y);
							//Debug.Log(dist + "\t  " + total + "  \n" + touchedObjectActiveColliders.Count + "  ");
							//Debug.Log(friction.hapticFriction + "\n" + dist + "  " + total + "\n");
							controllerActions.TriggerHapticPulse(total, friction.hapticFriction.x * friction.hapticFriction.y, friction.hapticFriction.y);
						}
					}
				}
				myLastTouchingPosition = transform.position;
			}
		}

		float checkCounter = 0;
		float checkCounterFreq = .5f;
		private void CheckTouchedObjectList()
		{
			if (checkCounter <= 0)
			{
				for (int i = frictionObjects.Count - 1; i > 0; i--)
				{
					//If it is null, disabled or the object is inactive
					if (frictionObjects[i] == null || !frictionObjects[i].enabled || !frictionObjects[i].gameObject.activeSelf || !frictionObjects[i].IsFrictionEnabled)
					{
						//Debug.Log("Flushing\n");
						//Remove it
						frictionObjects.RemoveAt(i);
					}
				}
				checkCounter = checkCounterFreq;
			}
			else
			{
				checkCounter -= Time.deltaTime;
			}
		}

		private void OnTriggerEnter(Collider collider)
		{
			if (IsOnValidLayer(collider.gameObject))
			{
				VRTK_FrictionObject frict;
				frict = collider.GetComponent<VRTK_FrictionObject>();

				if (frict != null && !frictionObjects.Contains(frict) && frict.IsFrictionEnabled)
				{
					frictionObjects.Add(frict);
				}
			}
		}

		private void OnTriggerExit(Collider collider)
		{
			if (IsOnValidLayer(collider.gameObject))
			{
				VRTK_FrictionObject frict;
				frict = collider.GetComponent<VRTK_FrictionObject>();

				if (frict != null && frictionObjects.Contains(frict))
				{
					frictionObjects.Remove(frict);
				}
			}
		}

		private bool IsOnValidLayer(GameObject checkedObject)
		{
			return AllowedLayers.HasLayer(checkedObject.layer);
		}
	}
}
