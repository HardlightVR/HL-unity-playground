using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class Shockwave : MonoBehaviour
	{
		public float RateOfExpansion = 10;
		public float DistanceTraveled = 0;
		bool hasReachedPlayer = false;
		private HardlightSuit suit;
		void Start()
		{
			suit = HardlightSuit.Find();
		}

		void Update()
		{
			if (!hasReachedPlayer)
			{
				var dist = Vector3.Distance(suit.transform.position, transform.position);
				if (DistanceTraveled > dist)
				{
					hasReachedPlayer = true;
					PlayShockwave();
				}
			}
		}

		private void PlayShockwave()
		{
			suit.GetSequence("hum").Play(AreaFlag.All_Areas, .5);
		}

		public static Shockwave CreateShockwaveAtLocation()
		{
			return null;
		}
	}
}