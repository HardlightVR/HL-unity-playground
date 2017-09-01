using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	[ExecuteInEditMode]
	public class HapticLineCastTest : MonoBehaviour
	{
		public GameObject start;
		public GameObject end;

		public Vector3 startLocation;
		public Vector3 endLocation;
		public Vector3 direction;

		public HapticSequence mySequence = new HapticSequence();
		public string sequenceFileName = "Haptics/pain_short";

		[Range(0, 30)]
		public int sphereCount = 30;

		[Range(0.01f, 5)]
		public float sphereCastRadius = .25f;
		private int sphereCastDrawCount = 10;

		HardlightSuit suit;
		void Start()
		{
			suit = HardlightSuit.Find();
			mySequence.LoadFromAsset(sequenceFileName);
		}

		void Update()
		{
			if (start != null && end != null)
			{
				startLocation = start.transform.position;
				endLocation = end.transform.position;
				direction = endLocation - startLocation;
				if (Application.isPlaying)
				{
					var Where = suit.HapticSphereCastForAreas(startLocation, direction, sphereCastRadius, 100);

					var singles = Where.ToArray();
					//string result = string.Empty;
					//for (int i = 0; i < singles.Length; i++)
					//{
					//	result += singles[i] + " ";
					//}
					//Debug.Log("Sphere Cast currently hitting [" + singles.Length + "] areas : " + result + "\n", this);
					mySequence.Play(Where);
				}
			}
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(startLocation, startLocation + direction);

			float total = sphereCount;
			for (int i = 0; i < total; i++)
			{
				if (i == 0)
				{
					Gizmos.color = Color.white - new Color(.75f, .75f, 0, .25f);
				}
				else if (i == total - 1)
				{
					Gizmos.color = Color.white - new Color(0, .75f, .75f, .15f);
				}
				else
				{
					Gizmos.color = Color.white - new Color(.5f, 0, .5f, .85f);
				}

				Gizmos.DrawSphere(Vector3.Lerp(startLocation, endLocation, i / total), sphereCastRadius);
			}
		}
	}
}