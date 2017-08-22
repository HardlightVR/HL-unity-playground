using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NullSpace.SDK.Demos
{
	public class CatchingPhase : GamePhase
	{
		//Have an update function.

		private float SpawnTimer = 15.0f;
		float timerMax = 12;
		float timerMin = 7;

		public override IEnumerator OnPhaseStart()
		{
			yield return base.OnPhaseStart();
		}

		public override bool ShouldEndPhase()
		{
			//At X kills, advance.
			//if (KillCount >= ProgressPhaseAtKills)
			//{
			//	return true;
			//}

			return base.ShouldEndPhase();
		}

		public override void UpdatePhase(float deltaTime)
		{
			if (PhaseState == PhaseEnum.Running)
			{
				//if (ShouldSpawnScorpion(deltaTime))
				//{
				//	RequestScorpionSpawns();
				//}
			}
			base.UpdatePhase(deltaTime);
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(transform.position, .2f);
		}
	}
}