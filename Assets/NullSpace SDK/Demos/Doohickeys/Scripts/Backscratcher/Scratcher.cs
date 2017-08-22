using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class Scratcher : MonoBehaviour
	{
		private bool scratching;
		public Vector3 LastHit;
		public float counter;
		public float minScratchTime = .1f;
		public string[] hapticFilesToPlay;
		private HapticSequence[] sequences;
		public VRTK.VRTK_InteractableObject vrtkObject;

		void Start()
		{
			if (hapticFilesToPlay == null)
			{
				hapticFilesToPlay = new string[0];
			}
			sequences = new HapticSequence[hapticFilesToPlay.Length];
			for (int i = 0; i < hapticFilesToPlay.Length; i++)
			{
				HapticSequence seq = new HapticSequence();
				seq.LoadFromAsset(hapticFilesToPlay[i]);
				sequences[i] = seq;
			}
		}
		void Update()
		{
			if (vrtkObject.IsGrabbed())
			{
				if (scratching && counter < 1)
				{
					counter += Time.deltaTime;
				}

				if (scratching && counter > minScratchTime)
				{
					TryScratch();
				}
			}
		}

		void OnTriggerEnter(Collider col)
		{
			if (col.gameObject.layer == 31)
			{
				scratching = true;
			}
		}
		void OnTriggerExit(Collider col)
		{
			if (col.gameObject.layer == 31)
			{
				scratching = false;
				counter = 0;
			}
		}

		void TryScratch()
		{
			if (Vector3.Distance(transform.position, LastHit) > .1f || (Vector3.Distance(transform.position, LastHit) > .05f && counter > .2f))
			{
				PlayHaptic();
			}
		}
		void PlayHaptic()
		{
			counter = 0;
			int index = Random.Range(0, sequences.Length);
			sequences[index].Play(HardlightSuit.Find().FindNearestFlag(transform.position));
		}
	}
}