using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class PhaseManager : MonoBehaviour
	{
		public Dictionary<string, GamePhase> Phases = new Dictionary<string, GamePhase>();

		void Awake()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				var trans = transform.GetChild(i);
				if (trans)
				{
					var phase = trans.GetComponent<GamePhase>();

					Phases.Add(phase.PhaseName, phase);

					if (i == 0)
					{
						GamePhase.CurrentPhase = phase;
						phase.ResetPhase(true);
					}
				}
			}
		}
		void Update()
		{
			name = "Phase Manager [" + GamePhase.CurrentPhase.PhaseName + "]";
			if (GamePhase.CurrentPhase != null)
			{
				GamePhase.CurrentPhase.UpdatePhase(Time.deltaTime);
			}
		}
	}
}