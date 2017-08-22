using UnityEngine;
using System.Collections;

namespace VRTK
{
	public class VRTK_FrictionObject : MonoBehaviour
	{
		[Tooltip("The haptic friction the control experiences when within an object.\nY denotes the number of pulses\nX denotes the duration of each pulse.")]
		public Vector2 hapticFriction = new Vector2(.2f, 2);
		[Tooltip("The haptic density for when moving within an object. This is the range of haptic strength available.\nMore movement reaches the upper strength")]
		public Vector2 hapticDensity = new Vector2(200, 3999);
		[Tooltip("The velocity multiplier (which increases your ability to hit the upper density)")]
		public float velocityMultiplier = 2000;
		[Tooltip("The velocity multiplier (which increases your ability to hit the upper density)")]
		public float minimumVelocity = .15f;

		public bool IsFrictionEnabled = true;

		private VRTK_InteractableObject myObject;

		void Start()
		{
			if (myObject == null)
			{
				myObject = GetComponent<VRTK_InteractableObject>();
				if (myObject == null && transform.parent != null)
				{
					myObject = transform.parent.GetComponent<VRTK_InteractableObject>();
				}
			}
		}

		public bool IsGrabbed
		{
			get { if(myObject != null) return myObject.IsGrabbed(); return false; }
		}
	}
}