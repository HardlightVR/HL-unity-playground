using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class PitchManager : MonoBehaviour
	{
		//We have four locations.
		public List<int> GroundLocationIndices;
		public List<int> OverheadLocationIndices;
		public PitchLocation[] SpawnLocations;
		//Array of the different money types.

		public GameObject MoneyFolder;

		[Header("Chain Pitches")]
		public ChainPitchInfo coinPitch;
		public ChainPitchInfo overhead;
		public ChainPitchInfo growingValue;
		public ChainPitchInfo simpleTriple;
		public ChainPitchInfo doubleGold;
		public ChainPitchInfo bigMoney;
		public ChainPitchInfo doubleDiamond;
		public ChainPitchInfo coinShower;

		public float RunningPossibleScore = 0;

		HardlightSuit suit;
		//Variable for my pitching area.
		void Start()
		{
			suit = HardlightSuit.Find();
			MoneyFolder = new GameObject();
			MoneyFolder.name = "Money Folder";
			MoneyFolder.transform.position = Vector3.zero;
			if (transform.parent != null)
			{
				MoneyFolder.transform.SetParent(transform.parent);
			}
			StartCoroutine(StandardGame());
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				StartChainedPitch(coinPitch);
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				StartChainedPitch(overhead);
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				StartChainedPitch(growingValue);
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				StartChainedPitch(simpleTriple);
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				StartChainedPitch(bigMoney);
			}
			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				StartChainedPitch(coinShower);
			}
			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				StartChainedPitch(doubleGold);
			}
			if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				StartChainedPitch(doubleDiamond);
			}
		}

		private IEnumerator StandardGame()
		{
			RunningPossibleScore = 0;
			//Say we're starting
			WaitForSeconds wait = new WaitForSeconds(5f);

			yield return new WaitForSeconds(2.5f);
			StartChainedPitch(simpleTriple);

			yield return new WaitForSeconds(3.5f);
			StartChainedPitch(coinPitch);

			yield return wait;
			StartChainedPitch(overhead);

			yield return new WaitForSeconds(4.5f);
			StartChainedPitch(growingValue);

			wait = new WaitForSeconds(4.5f);
			yield return wait;
			StartChainedPitch(simpleTriple);

			yield return wait;
			StartChainedPitch(bigMoney);

			yield return wait;
			StartChainedPitch(simpleTriple);

			yield return wait;
			StartChainedPitch(doubleGold);
			yield return new WaitForSeconds(3.5f);
			StartChainedPitch(doubleGold);

			yield return wait;
			StartChainedPitch(doubleDiamond);

			yield return wait;
			StartChainedPitch(coinShower);
			yield return wait;
			StartChainedPitch(coinShower);

			Debug.Log("Game Over! " + RunningPossibleScore + "\n");
		}

		public enum TypeOfPitch { FromSameSource, FromSpreadSources, FromAbove, FromAnywhere }

		public int RequestPitchLocation(TypeOfPitch type, int lastSourceIndex = -1)
		{
			//Default Case
			//if(type == TypeOfPitch.FromAnywhere)
			int nextLocationIndex = Random.Range(0, SpawnLocations.Length);

			if (type == TypeOfPitch.FromAbove)
			{
				//Pick a random overhead location
				nextLocationIndex = OverheadLocationIndices[Random.Range(0, OverheadLocationIndices.Count)];
			}
			else if (type == TypeOfPitch.FromSameSource)
			{
				//If we don't have a last source, we're the first pitch, otherwise return the last source
				return lastSourceIndex > -1 ? lastSourceIndex : nextLocationIndex;
			}
			else if (type == TypeOfPitch.FromSpreadSources)
			{
				if (lastSourceIndex > -1)
				{
					GroundLocationIndices.Remove(lastSourceIndex);
				}
				//If we don't have a last source, we're the first pitch, otherwise return the last source
				nextLocationIndex = GroundLocationIndices[Random.Range(0, GroundLocationIndices.Count)];

				if (lastSourceIndex > -1)
				{
					GroundLocationIndices.Add(lastSourceIndex);
				}
			}

			return nextLocationIndex;
		}

		public void StartChainedPitch(ChainPitchInfo info)
		{
			ChainedPitch newChainedPitch = new ChainedPitch(info);

			int lastLocationIndex = -1;
			for (int i = 0; i < newChainedPitch.info.NumberOfPitches; i++)
			{
				//Give it a source
				newChainedPitch.Pitches[i].PitchLocationIndex = RequestPitchLocation(newChainedPitch.info.pitchType, lastLocationIndex);
				newChainedPitch.Pitches[i].Source = SpawnLocations[newChainedPitch.Pitches[i].PitchLocationIndex];

				//Store it for the next one
				lastLocationIndex = newChainedPitch.Pitches[i].PitchLocationIndex;

				//Find it's target
				newChainedPitch.Pitches[i].intendedTarget = suit.FindRandomLocation().gameObject;

				//Set the type of money we are pitching
				newChainedPitch.Pitches[i].TypeOfMoneyPitched = newChainedPitch.info.PitchContents[i];

				//Maybe give it an intended arc height?
				//Perhaps the air time?

				RunningPossibleScore += HeistManager.Inst.GetMonetaryValue(newChainedPitch.info.PitchContents[i]);
			}

			StartCoroutine(InitiateChainedPitch(newChainedPitch));
		}

		public void StartChainedPitch(ChainedPitch pitches)
		{
			StartCoroutine(InitiateChainedPitch(pitches));
		}

		IEnumerator InitiateChainedPitch(ChainedPitch chainedPitch)
		{
			//Debug.Log("Starting chained pitch\n");
			//Play the audio clip.
			//PlayChainedPitchSFX(chainedPitch);

			//multiPitch.info.pitchSFXNames
			WaitForSeconds waitTime = new WaitForSeconds(chainedPitch.info.StartDelay);
			yield return waitTime;

			for (int i = 0; i < chainedPitch.info.NumberOfPitches; i++)
			{
				//Pitch this one
				chainedPitch.Pitches[i] = PerformPitch(chainedPitch.Pitches[i]);

				//multiPitch.Pitches[i];
				if (i < chainedPitch.info.delayBetweenPitches.Length)
				{
					yield return new WaitForSeconds(chainedPitch.info.delayBetweenPitches[i]);
				}
			}
		}

		private void PlayChainedPitchSFX(ChainedPitch chainedPitch)
		{
			AudioClip SFX = chainedPitch.info.GetRandomSFX();
			if (SFX != null)
			{
				AudioManager.Inst.MakeSource(SFX, chainedPitch.GetSFXLocation()).Play();
			}
		}

		public Pitch PerformPitch(Pitch pitch)
		{
			pitch.thrownMoney = CreateMoneyProjectile(pitch.TypeOfMoneyPitched);
			pitch.thrownMoney.transform.SetParent(MoneyFolder.transform);
			//Debug.Log(pitch.Report());
			pitch.SolveAndServe();

			return pitch;
		}

		public MoneyProjectile CreateMoneyProjectile(MoneyTypes moneyType)
		{
			GameObject go = HeistManager.Inst.Instantiate(moneyType);

			if (go != null)
			{
				return go.GetComponent<MoneyProjectile>();
			}
			return null;
		}

		public class ChainedPitch
		{
			public ChainPitchInfo info;
			public Pitch[] Pitches;
			public AudioSource source;

			public Vector3 GetSFXLocation()
			{
				if (Pitches.Length > 0)
				{
					return Pitches[Random.Range(0, Pitches.Length)].Source.transform.position;
				}
				return Vector3.one * 1000;
			}
			public ChainedPitch(ChainPitchInfo pitchInfo)
			{
				info = pitchInfo;
				Pitches = new Pitch[info.NumberOfPitches];


				for (int i = 0; i < Pitches.Length; i++)
				{
					Pitches[i] = new Pitch();
				}
			}
		}
		[System.Serializable]
		public class ChainPitchInfo
		{
			[Header("Pitch Info")]
			public TypeOfPitch pitchType;
			public int NumberOfPitches = 0;
			public float StartDelay;
			public float[] delayBetweenPitches;
			public MoneyTypes[] PitchContents;

			public string[] pitchSFXNames = { };
			public AudioClip[] pitchSFX = { };

			public AudioClip GetRandomSFX()
			{
				if (pitchSFX.Length > 0)
				{
					return pitchSFX[Random.Range(0, pitchSFX.Length)];
				}
				return null;
			}

			public string GetRandomSFXName()
			{
				if (pitchSFXNames.Length > 0)
				{
					return pitchSFXNames[Random.Range(0, pitchSFXNames.Length)];
				}
				return string.Empty;
			}

			public float GetTotalTime()
			{
				float timeCount = StartDelay;
				for (int i = 0; i < delayBetweenPitches.Length; i++)
				{
					timeCount += delayBetweenPitches[i];
				}
				return timeCount;
			}

			public ChainPitchInfo(int pitchCount = 1, float delayBetweenEachPitch = .5f, TypeOfPitch type = TypeOfPitch.FromSameSource, params MoneyTypes[] pitchedMoneyTypes)
			{
				StartDelay = 0;
				NumberOfPitches = pitchCount;

				delayBetweenPitches = new float[NumberOfPitches];
				for (int i = 0; i < NumberOfPitches; i++)
				{
					delayBetweenPitches[i] = delayBetweenEachPitch;
				}
				PitchContents = new MoneyTypes[NumberOfPitches];
				for (int i = 0; i < NumberOfPitches; i++)
				{
					if (pitchedMoneyTypes.Length > i)
					{
						PitchContents[i] = pitchedMoneyTypes[i];
					}
					else
					{
						PitchContents[i] = MoneyTypes.MoneyPack;
					}
				}


			}
			public ChainPitchInfo(int pitchCount = 1, MoneyTypes onlyMoneyTypePitched = MoneyTypes.GoldBar, TypeOfPitch type = TypeOfPitch.FromSameSource, float delayBetweenEachPitch = .5f)
			{
				StartDelay = 0;
				NumberOfPitches = pitchCount;

				pitchType = type;
				delayBetweenPitches = new float[NumberOfPitches];
				for (int i = 0; i < NumberOfPitches; i++)
				{
					delayBetweenPitches[i] = delayBetweenEachPitch;
				}
				PitchContents = new MoneyTypes[NumberOfPitches];
				for (int i = 0; i < NumberOfPitches; i++)
				{
					PitchContents[i] = onlyMoneyTypePitched;
				}
			}
		}

		public class Pitch
		{
			public MoneyProjectile thrownMoney;
			public float Distance;
			public float ThrowStrength;
			public int PitchLocationIndex;
			public PitchLocation Source;
			public MoneyTypes TypeOfMoneyPitched;
			public GameObject intendedTarget;

			public void SolveAndServe()
			{
				Vector3 StartToEnd = intendedTarget.transform.position - Source.transform.position;
				Vector3 StartToEndFlat = new Vector3(StartToEnd.x, 0, StartToEnd.z);

				//Reduce the forward push based on the y difference. If it's much higher, then we will forward push less
				float forwardPush = (1.7f + StartToEnd.y / 20);

				//4.5 is the upward component for nice arcs
				float upPush = (4.5f + StartToEnd.y / 10);

				//Debug.Log(upPush + "  " + StartToEnd.y + "\n");

				Vector3 serveVector = StartToEndFlat * forwardPush + Vector3.up * upPush;
				DrawServe(serveVector);
				Serve(serveVector);
				Source.DisplayFiringEffect();
			}
			public string Report()
			{
				return "[Pitch Report]  " + Source.transform.position + "  [" + PitchLocationIndex + "]   " + intendedTarget.name + "  \t" + TypeOfMoneyPitched.ToString() + "\n";
			}
			private void DrawServe(Vector3 serveForce)
			{
				Debug.DrawLine(Source.transform.position, Source.transform.position + serveForce.normalized * 3, Color.yellow, 5.0f);
				Debug.DrawLine(Source.transform.position + Vector3.up * .25f, Source.transform.position + serveForce.normalized * 3, Color.green, 5.0f);
			}
			private void Serve(Vector3 serveForce)
			{
				//Set start position and zero out values
				thrownMoney.transform.position = Source.transform.position;
				Rigidbody rb = thrownMoney.MyRB;
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;

				//Fling in the given direction
				rb.AddForce(serveForce, ForceMode.VelocityChange);

				Vector3 torque = Random.onUnitSphere * 5;

				if (TypeOfMoneyPitched == MoneyTypes.SingleCoin)
				{
					torque *= 5;
				}
				rb.AddTorque(torque);
			}
		}

		public class Pitch2
		{
			public Rigidbody thrownObj;
			public Vector3 StartPoint;
			public Vector3 MidPoint;
			public Vector3 EndPoint;
			public Vector3 InitialVelocity;
			public float AirTime;

			public Pitch2(Vector3 StartPoint, Vector3 EndPoint, Vector3 PitchVelocity, Rigidbody thrownObj)
			{
				this.thrownObj = thrownObj;
				this.StartPoint = StartPoint;
				this.EndPoint = EndPoint;

				MidPoint = StartPoint + EndPoint / 2;
				InitialVelocity = PitchVelocity;

				Vector3 XZDistance = EndPoint - StartPoint;
				XZDistance.y = 0;
				float distance = XZDistance.magnitude;

				Vector3 initVel = PitchVelocity;
				float initialYVelocity = initVel.y;

				initVel.y = 0;
				float initialXZAccel = initVel.magnitude;

				float theta = Vector3.Angle(initVel, PitchVelocity);


				//z = height off ground
				//z = z0 + v0*t + .5*g*t^2

				float initialHeight = StartPoint.y - EndPoint.y;
				AirTime = (2 * initialYVelocity * Mathf.Sin(theta)) / Physics.gravity.y;
				AirTime = Mathf.Abs(AirTime);

				//Vertical Displacement:
				float displacementY = (InitialVelocity.y + 0) / 2 * AirTime;

				Debug.Log("[New Pitch] at " + StartPoint + " to " + EndPoint + "\nHeightDifference: " + initialHeight + " and journey took " + AirTime + " seconds from initial height: " + initialHeight + " at angle: " + theta);
			}

			public Pitch2(Vector3 MidPoint, Vector3 EndPoint, float time)
			{
				Vector3 midToEnd = EndPoint - MidPoint;
				StartPoint = MidPoint - midToEnd;
				//This is where the start of the throw is.
				StartPoint.y = 0;
				this.MidPoint = MidPoint;
				this.EndPoint = EndPoint;
			}
		}
	}
}