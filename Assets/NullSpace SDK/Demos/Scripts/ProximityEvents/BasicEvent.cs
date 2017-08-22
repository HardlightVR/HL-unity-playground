using UnityEngine;
using System.Collections;

namespace DesertOfDanger
{
	public class BasicEvent : MonoBehaviour
	{
		public ProximityTriggerAction trigger;

		void Start()
		{
			if (trigger == null)
				trigger = GetComponent<ProximityTriggerAction>();
			trigger.triggerEnterDelegate += EventOccurred;
		}

		public virtual void EventOccurred(Collider col)
		{

		}
	}
}