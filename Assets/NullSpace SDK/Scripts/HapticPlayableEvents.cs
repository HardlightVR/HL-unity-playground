using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK
{
	//[System.Serializable]
	//public class HapticEvent : UnityEngine.Events.UnityEvent<> { }

	[System.Serializable]
	public class HapticInfo
	{
		public enum PlayableType { Sequence, Pattern, Experience, Impulse }
		public enum ImpulseType { None, Emanation, Traversal }
		public PlayableType TypeOfPlayable;

		[ConditionalHide("TypeOfPlayable", true, "Impulse")]
		public string PlayableResourceName = "Haptics/click";

		[ConditionalHide("TypeOfPlayable", "Sequence")]
		public AreaFlag Where;

		[Header("Impulse Attributes")]
		[ConditionalHide("TypeOfPlayable", "Impulse")]
		public ImpulseType TypeOfImpulse = ImpulseType.None;

		[ConditionalHide("TypeOfImpulse", true, "None")]
		public string HapticSequence = "Haptics/click";
		[ConditionalHide("TypeOfImpulse", true, "None")]
		public float Duration;
		[ConditionalHide("TypeOfImpulse", true, "None")]
		public AreaFlag StartLocation;
		[ConditionalHide("TypeOfImpulse", "Traversal")]
		public AreaFlag EndLocation;

		public HapticHandle TryToPlayPlay()
		{
			if (CanPlay())
			{
				return Play();
			}
			return null;
		}

		public bool CanPlay()
		{
			if (TypeOfPlayable == PlayableType.Sequence)
			{
				return PlayableResourceName.Length > 0 && Where != AreaFlag.None;
			}
			else if (TypeOfPlayable == PlayableType.Pattern)
			{
				return PlayableResourceName.Length > 0;
			}
			else if (TypeOfPlayable == PlayableType.Experience)
			{
				return PlayableResourceName.Length > 0;
			}
			else if (TypeOfPlayable == PlayableType.Impulse)
			{
				if (TypeOfImpulse == ImpulseType.None)
					return false;
				else if (TypeOfImpulse == ImpulseType.Emanation)
				{
					return HapticSequence.Length > 0 && StartLocation != AreaFlag.None;
				}
				else if (TypeOfImpulse == ImpulseType.Traversal)
				{
					return HapticSequence.Length > 0 && StartLocation != AreaFlag.None && EndLocation != AreaFlag.None;
				}
			}
			return true;
		}

		private HapticHandle Play()
		{
			return null;
		}
	}

	public class HapticPlayableEvents : MonoBehaviour
	{
		[SerializeField]
		public HapticInfo MyHaptic;

		private HapticHandle handle;
		public void Play()
		{
			handle = MyHaptic.TryToPlayPlay();
		}
		public void StopIfPlaying()
		{
			if (handle != null)
			{
				handle.Stop();
			}
		}
		public void Restart()
		{
			if (handle == null)
			{
				Play();
			}
			else
			{
				handle.Replay();
			}
		}
	}
}