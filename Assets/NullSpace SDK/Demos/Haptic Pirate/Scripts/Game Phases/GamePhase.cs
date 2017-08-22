using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;

namespace NullSpace.SDK.Demos
{
	public class GamePhase : MonoBehaviour
	{
		public static GamePhase CurrentPhase;
		//If there is a finite specific order.
		public GamePhase PreviousPhase;
		public GamePhase NextPhase;

		public List<GameObject> EnableOnStart = new List<GameObject>();

		public GameObject RigMover;
		public VRTK_HeadsetFade fade;

		public string PhaseName;

		//Call for phases to begin by name.

		public enum PhaseEnum { Unstarted, Starting, Running, Ending, Ended }
		public PhaseEnum _phaseState = PhaseEnum.Unstarted;
		public PhaseEnum PhaseState
		{
			get { return _phaseState; }
			set
			{
				//Debug.Log("[" + PhaseName + "] Setting Phase State to: " + value.ToString() + "\n" + Time.timeSinceLevelLoad);
				_phaseState = value;
				name = PhaseName + " Phase [" + _phaseState.ToString() + "]";
			}
		}

		public float StartDuration = 1.0f;
		public float RunningDuration = 0.0f;
		public float EndDuration = 1.0f;

		public float TimeElapsedThisPhase = 0.0f;

		public Coroutine Starting;
		private bool StartingCorout = false;
		public Coroutine Ending;
		private bool EndingCorout = false;

		public virtual bool ShouldEndPhase()
		{
			if (RunningDuration > 0 && TimeElapsedThisPhase > RunningDuration)
			{
				return true;
			}
			return false;
		}

		void Start()
		{
			for (int i = 0; i < EnableOnStart.Count; i++)
			{
				EnableOnStart[i].SetActive(false);
			}
		}

		public void Init(string starterName, GamePhase before, GamePhase after)
		{
			PhaseName = starterName;
			name = starterName + " Phase";
			PreviousPhase = before;
			NextPhase = after;
		}

		public void ResetPhase(bool Begin = true)
		{
			PhaseState = PhaseEnum.Unstarted;
			if (Starting != null)
			{
				StopCoroutine(Starting);
			}
			if (Ending != null)
			{
				StopCoroutine(Ending);
			}
			if (Begin)
			{
				Starting = StartCoroutine(StartPhase());
			}

			TimeElapsedThisPhase = 0;
		}

		public void RegressPhase()
		{
			PhaseState = PhaseEnum.Unstarted;
			if (Starting != null)
			{
				StopCoroutine(Starting);
			}
			if (Ending != null)
			{
				StopCoroutine(Ending);
			}
			PreviousPhase.ResetPhase(true);
		}

		public void ForceAdvancePhase(bool EndFirst)
		{
			Debug.Log("Force advance phase from [" + name + "]\n");
			if (Starting != null)
			{
				StopCoroutine(Starting);
			}

			//We want to mark to begin next phase on end completion.
			//Unintended consequence to consider later - We're setting that value to true and we might want it false later.
			//Should it always get set when called?

			if (EndFirst)
			{
				if (Ending == null)
				{
					//Set us to a reasonable phase.
					PhaseState = PhaseEnum.Running;
					CurrentPhase = this;
					//Start the end
					StartCoroutine(EndPhase());
				}
				//Otherwise do nothing
			}
			else
			{
				if (Ending != null)
				{
					StopCoroutine(Starting);
				}
			}
		}

		IEnumerator StartPhase()
		{
			StartingCorout = true;
			if (PhaseState == PhaseEnum.Unstarted)
			{
				PhaseState = PhaseEnum.Starting;
				yield return OnPhaseStart();
				yield return new WaitForSeconds(StartDuration);
				PhaseState = PhaseEnum.Running;
				CurrentPhase = this;
			}
			StartingCorout = false;
		}

		public virtual IEnumerator OnPhaseStart()
		{
			fade.Fade(Color.black, .25f);
			yield return new WaitForSeconds(.25f);
			for (int i = 0; i < EnableOnStart.Count; i++)
			{
				EnableOnStart[i].SetActive(true);
			}

			RigMover.transform.position = transform.position;
			fade.Fade(Color.clear, .25f);

			yield return null;
		}

		public virtual void UpdatePhase(float deltaTime)
		{
			if (PhaseState == PhaseEnum.Running)
			{
				TimeElapsedThisPhase += deltaTime;

				if (ShouldEndPhase())
				{
					if (Ending != null)
					{
						StopCoroutine(Ending);
					}
					Ending = StartCoroutine(EndPhase());
				}
			}
		}

		IEnumerator EndPhase()
		{
			EndingCorout = true;
			if (PhaseState == PhaseEnum.Running || PhaseState == PhaseEnum.Starting)
			{
				PhaseState = PhaseEnum.Ending;
				yield return OnPhaseEnd();
				yield return new WaitForSeconds(EndDuration);
				PhaseState = PhaseEnum.Ended;

				if (NextPhase != null)
				{
					NextPhase.ResetPhase();
				}
			}
			EndingCorout = false;
		}

		public virtual IEnumerator OnPhaseEnd()
		{
			for (int i = 0; i < EnableOnStart.Count; i++)
			{
				EnableOnStart[i].SetActive(false);
			}
			yield return null;
		}
	}
}