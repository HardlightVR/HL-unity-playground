using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace NullSpace.SDK.Demos
{
	public class HumanHeart : VRTK_InteractableObject
	{
		private HapticPattern[] heartBeat = { new HapticPattern(), new HapticPattern(), new HapticPattern() };
		private string[] names = { "beating_heart", "beating_heart_fast", "beating_heart_very_fast" };
		public int index = -1;
		private HapticHandle[] heartbeatHandles = new HapticHandle[3];

		public bool Increase = false;
		public bool Decrease = false;
		public bool Held = false;
		private bool WasHeld = false;
		private bool CurrentlyPlaying = false;
		public float heartbeatDuration = 9;
		public float durationCounter = 9;
		void Start()
		{
			for (int i = 0; i < heartBeat.Length; i++)
			{
				heartBeat[i].LoadFromAsset("Haptics/" + names[i]);
				heartbeatHandles[i] = heartBeat[i].CreateHandle();
			}
		}

		protected override void Update()
		{
			HandleInactivity();

			if (durationCounter > 0)
			{
				durationCounter -= Time.deltaTime;
				if (durationCounter <= 0)
				{
					//Restart effect.
					RestartHeartbeatAtIndex(index);
				}
			}

			HandleEditorIntensityShift();

			if (Held != WasHeld)
			{
				if (!Held)
				{

				}
			}
		}

		private void HandleInactivity()
		{
			if (CurrentlyPlaying && index == -1)
			{
				CurrentlyPlaying = false;

				for (int i = 0; i < heartbeatHandles.Length; i++)
				{
					StopHeartbeatAtIndex(i);
				}
			}
		}
		private void HandleEditorIntensityShift()
		{
			if (Increase)
			{
				Increase = false;
				IncreaseHeartbeatRate();
			}
			if (Decrease)
			{
				Decrease = false;
				DecreaseHeartbeatRate();
			}
		}

		public void IncreaseHeartbeatRate()
		{
			var oldIndex = index;
			index++;
			index = Mathf.Clamp(index, -1, heartBeat.Length-1);
			Debug.Log("Increase\n");
			if (oldIndex != index)
			{
				StopHeartbeatAtIndex(oldIndex);

				PlayHeartbeatAtIndex(index);
			}
		}
		public void DecreaseHeartbeatRate()
		{
			var oldIndex = index;
			index--;
			index = Mathf.Clamp(index, -1, heartBeat.Length-1);

			Debug.Log("Increase\n");
			if (oldIndex != index)
			{
				StopHeartbeatAtIndex(oldIndex);

				PlayHeartbeatAtIndex(index);
			}
		}

		private void StopHeartbeatAtIndex(int index)
		{
			if (heartbeatHandles[index] != null)
			{
				heartbeatHandles[index].Stop();
			}
		}
		private void PlayHeartbeatAtIndex(int index)
		{
			if (heartbeatHandles[index] != null)
			{
				CurrentlyPlaying = true;
				heartbeatHandles[index].Play();
			}
		}
		private void RestartHeartbeatAtIndex(int index)
		{
			if (heartbeatHandles[index] != null)
			{
				CurrentlyPlaying = true;
				heartbeatHandles[index].Replay();
			}
		}
	}
}