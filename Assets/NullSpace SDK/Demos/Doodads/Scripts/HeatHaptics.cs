using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class HeatHaptics : MonoBehaviour
	{

		public float maxRadius;
		public float minRadius;
		public AnimationCurve MyCurve;

		[Header("Variation", order = 5)]
		[Range(0.0f, 1)]
		public float maxVariation = .2f;

		List<HardlightCollider> refs;
		void Start()
		{
			refs = HardlightSuit.Find().Definition.SceneReferences;

		}
		void Update()
		{
			var directionOfFlicker = 0.0f;
			float hapticStrength, dist;
			string output = "";
			for (int i = 0; i < refs.Count; i++)
			{
				dist = Vector3.Distance(refs[i].transform.position, transform.position);
				hapticStrength = EvaluateCurve(dist);
				output += refs[i].name + "  " + dist + "  -  " + hapticStrength + "\n";

				NSManager.Instance.ControlDirectly(refs[i].regionID, hapticStrength);
			}

			Debug.Log(output + "\n", this);
		}


		public float EvaluatePoint(Vector3 point)
		{
			var dist = Vector3.Distance(point, transform.position);

			//Dist = 10, MaxRadius = 3, MinRadius = 1 -> Result is 0.
			//Dist = 5, MaxRadius = 8, MinRadius = 0 -> Result is 5/8.
			//Dist = 4, MaxRadius = 7, MinRadius = 1.25 -> Result is (4-1.25) / 7.

			return EvaluateCurve(dist);
		}

		private float EvaluateCurve(float dist)
		{
			float val = Mathf.Clamp((dist - minRadius) / maxRadius, 0.0f, 1.0f);

			return MyCurve.Evaluate(val);
		}

		private float GetStrengthVariation()
		{
			if (maxVariation == 0.0f)
			{
				return 0;
			}
			return Random.Range(-maxVariation, maxVariation);
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, minRadius);
			Gizmos.color = Color.red - new Color(0, 0, 0, .5f);
			Gizmos.DrawWireSphere(transform.position, maxRadius);
		}
	}
}