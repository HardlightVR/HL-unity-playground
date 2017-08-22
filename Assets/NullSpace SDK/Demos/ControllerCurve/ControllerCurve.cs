using UnityEngine;
using System.Collections;
using VRTK;

namespace NullSpace.SDK.Demos
{
	/// <summary>
	/// [Incomplete] This is an incomplete class for handling haptic curves.
	/// It still has some minor bugs. In other words...
	/// DO NOT USE.
	/// </summary>
	public class ControllerCurve : MonoBehaviour
	{
		VRTK_ControllerActions[] actions;

		[Range(0.0f, 5, order = 1)]
		public float timeDelay = 0f;
		[Space(-10, order = 2)]

		[Header("The curve the haptic volume will follow", order = 3)]
		public AnimationCurve MyCurve;

		[Range(0.1f, 5)]
		public float Duration = 0.5f;
		[Range(0.0f, 5)]
		public float endingSustain = 0f;
		[Space(10)]

		[Header("Volume Range", order = 4)]
		[Range(0.0f, 1)]
		public float minVolume = 0;
		[Range(0.0f, 1)]
		public float maxVolume = 1;

		[Header("Variation", order = 5)]
		[Range(0.0f, 1)]
		public float maxVariation = 0;

		[Range(0.0f, .1f)]
		public float stepLength = .01f;

		public KeyCode TriggerKey = KeyCode.None;

		public bool InMainCoroutineWhile = false;
		public bool PlaySustainAtEnd = true;

		private float counter = 0.0f;
		private bool _playing;
		private bool Playing
		{
			get { return _playing; }
			set
			{
				_playing = value;
			}

		}
		private bool StartPlaying;
		private Coroutine PlayRoutine;

		void Start()
		{
			//MyCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
			//MyCurve.preWrapMode = WrapMode.PingPong;
			//MyCurve.postWrapMode = WrapMode.PingPong;
			
			actions = FindObjectsOfType<VRTK_ControllerActions>();
			//Debug.Log(actions.name, actions);
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Home) || Input.GetKeyDown(TriggerKey))
			{
				Stop();
			}
			if (Input.GetKeyDown(TriggerKey))
			{
				Play();
			}

			if (StartPlaying && !Playing)
			{
				PlayRoutine = StartCoroutine(ExecuteCurvePlaying(timeDelay));
			}
		}

		public void Stop()
		{
			if (Playing)
			{
				StopCoroutine(PlayRoutine);
				Silence();
			}
		}

		public void Silence()
		{
			Playing = false;
			InMainCoroutineWhile = false;
			counter = Duration;
			PlaySustainAtEnd = false;

			for (int i = 0; i < actions.Length; i++)
			{
				if (actions[i] != null)
				{
					actions[i].CancelHapticPulse();
				}
			}
		}

		public void Play()
		{
			StartPlaying = true;
		}

		private float GetStrengthVariation()
		{
			if (maxVariation == 0.0f)
			{
				return 0;
			}
			return Random.Range(-maxVariation, maxVariation);
		}

		IEnumerator ExecuteCurvePlaying(float delayBeforeStart = 0.0f)
		{
			WaitForSeconds wait = new WaitForSeconds(stepLength);
			InMainCoroutineWhile = false;
			PlaySustainAtEnd = true;
			if (delayBeforeStart > 0)
			{
				yield return new WaitForSeconds(delayBeforeStart);
			}

			Playing = true;
			StartPlaying = false;
			float eval = 0.0f;
			float vol = 0.0f;
			counter = 0;
			Duration = Mathf.Clamp(Duration, 0, float.MaxValue);
			//AreaFlag[] areas = Where.ToArray();

			while (counter < Duration)
			{
				InMainCoroutineWhile = true;
				//Debug.Log("Curve: " + eval + "\n" + vol + "   " + Duration + "   " + counter);

				//for (int i = 0; i < areas.Length; i++)
				//{
					//NSManager.Instance.ControlDirectly(areas[i], vol);
				//}
				counter = Mathf.Clamp(counter + Time.deltaTime, 0.0f, Duration);
				eval = MyCurve.Evaluate(counter / Duration) + GetStrengthVariation();
				vol = minVolume + eval * (maxVolume - minVolume);

				for (int i = 0; i < actions.Length; i++)
				{
					actions[i].TriggerHapticPulse(vol);
				}

				//Wait a frame
				yield return wait;
			}

			if (endingSustain > 0 && PlaySustainAtEnd)
			{
				eval = MyCurve.Evaluate(1.0f) + GetStrengthVariation();
				vol = minVolume + eval * (maxVolume - minVolume);

				for (float i = 0; i < endingSustain; i += stepLength)
				{
					for (int k = 0; k < actions.Length; k++)
					{
						actions[k].TriggerHapticPulse(vol);
					}
					yield return wait;
				}
			}

			Silence();
		}

		private void OnDestroy()
		{
			Silence();
		}

		//public static HapticCurve CreateHapticCurve(AnimationCurve newCurve, float duration = .5f, float minVolume = .1f, float maxVolume = 1.0f)
		//{
		//	//Make a new GameObject
		//	//GameObject go = new GameObject();

		//	//HapticCurve curve = go.AddComponent<HapticCurve>();


		//	return null;

		//}
	}
}