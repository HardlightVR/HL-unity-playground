using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace NullSpace.SDK.Demos
{
	public class HeistManager : Singleton<HeistManager>
	{
		public Camera VRCamera;
		//Reference to Scoreboard
		public Scoreboards ScoreDisplay;

		//Prefab Dictionary?

		//Reference to Money Pitchers?
		public PitchManager pitcher;

		//Reference to Money Bins?
		private Vector3 storedGravity = Vector3.down;

		public Dictionary<MoneyTypes, AudioClip[]> collisionAudio = new Dictionary<MoneyTypes, AudioClip[]>();
		public Dictionary<MoneyTypes, AudioClip[]> collectionAudio = new Dictionary<MoneyTypes, AudioClip[]>();
		public Dictionary<MoneyTypes, GameObject> moneyProjectiles = new Dictionary<MoneyTypes, GameObject>();
		private Dictionary<MoneyTypes, int> MonetaryValue;

		private Dictionary<string, GameObject> _prefabDict = new Dictionary<string, GameObject>();
		private Dictionary<string, GameObject> PrefabDict
		{
			get
			{
				if (_prefabDict == null)
				{
					_prefabDict = new Dictionary<string, GameObject>();
				}
				return _prefabDict;
			}
			set
			{
				if (value != null)
				{ _prefabDict = value; }
				else
				{
					Debug.LogError("Attempted to set Prefab Dictionary to a null value. Disallowed assignment\n");
				}
			}
		}

		//Timer logic to pitch the actual money objects.
		[SerializeField]
		private List<GameObject> _collectionVisuals = new List<GameObject>();
		public List<GameObject> CollectionVisuals
		{
			get
			{
				return _collectionVisuals;
			}
			set
			{
				Debug.Assert(value != null);
				_collectionVisuals = value;
			}
		}
		[SerializeField]
		private List<GameObject> _destructionVisuals = new List<GameObject>();
		public List<GameObject> DestructionVisuals
		{
			get
			{
				return _destructionVisuals;
			}
			set
			{
				Debug.Assert(value != null);
				_destructionVisuals = value;
			}
		}
		public override void Awake()
		{
			base.Awake();
			collisionAudio = new Dictionary<MoneyTypes, AudioClip[]>();
			collectionAudio = new Dictionary<MoneyTypes, AudioClip[]>();
			moneyProjectiles = new Dictionary<MoneyTypes, GameObject>();
			MonetaryValue = new Dictionary<MoneyTypes, int>();
			storedGravity = Physics.gravity;
			Physics.gravity = Vector3.up * -9.81f / 2;
			VRMimic.Initialize(VRCamera.gameObject);
			SetupMoneyProjectiles();
			SetupMonetaryValues();
			SetupCollisionAudio();
			SetupCollectionAudio();
		}
		private void SetupMoneyProjectiles()
		{
			moneyProjectiles.Add(MoneyTypes.GoldBar, Load("Prefabs/Gold Bar"));
			moneyProjectiles.Add(MoneyTypes.MoneyPack, Load("Prefabs/Cash Pack"));
			moneyProjectiles.Add(MoneyTypes.MoneyBag, Load("Prefabs/Cash Pack"));
			moneyProjectiles.Add(MoneyTypes.SingleCoin, Load("Prefabs/Gold Coin"));
			moneyProjectiles.Add(MoneyTypes.Gemstone, Load("Prefabs/Gemstone"));
		}
		private void SetupMonetaryValues()
		{
			MonetaryValue.Add(MoneyTypes.SingleCoin, 10);
			MonetaryValue.Add(MoneyTypes.MoneyPack, 50);
			MonetaryValue.Add(MoneyTypes.MoneyBag, 0);
			MonetaryValue.Add(MoneyTypes.GoldBar, 400);
			MonetaryValue.Add(MoneyTypes.Gemstone, 2000);
		}
		private void SetupCollisionAudio()
		{
			collisionAudio.Add(MoneyTypes.SingleCoin, new AudioClip[3]
				{
					AudioManager.Inst.FindOrLoadAudioClip("Collision/coin1"),
					AudioManager.Inst.FindOrLoadAudioClip("Collision/coin2"),
					AudioManager.Inst.FindOrLoadAudioClip("Collision/coin3"),
				}
				);
			collisionAudio.Add(MoneyTypes.MoneyPack, new AudioClip[2]
				{
					AudioManager.Inst.FindOrLoadAudioClip("Collision/cloth"),
					AudioManager.Inst.FindOrLoadAudioClip("Collision/cloth-heavy")
				}
				);
			collisionAudio.Add(MoneyTypes.MoneyBag, new AudioClip[3]
				);
			collisionAudio.Add(MoneyTypes.GoldBar, new AudioClip[5]
				{
					AudioManager.Inst.FindOrLoadAudioClip("Collision/GoldThudLight1"),
					AudioManager.Inst.FindOrLoadAudioClip("Collision/GoldThudMedium1"),
					AudioManager.Inst.FindOrLoadAudioClip("Collision/GoldThudMedium2"),
					AudioManager.Inst.FindOrLoadAudioClip("Collision/GoldThudHeavy1"),
					AudioManager.Inst.FindOrLoadAudioClip("Collision/GoldThudMediumReverb")
				}
				);
			collisionAudio.Add(MoneyTypes.Gemstone, new AudioClip[3]
				);
		}
		private void SetupCollectionAudio()
		{
			collectionAudio.Add(MoneyTypes.SingleCoin, new AudioClip[1]
				{
					AudioManager.Inst.FindOrLoadAudioClip("Collection/gem score"),
				}
				);
			collectionAudio.Add(MoneyTypes.MoneyPack, new AudioClip[3]
				{
					AudioManager.Inst.FindOrLoadAudioClip("Collection/Cash1"),
					AudioManager.Inst.FindOrLoadAudioClip("Collection/Cash2"),
					AudioManager.Inst.FindOrLoadAudioClip("Collection/Cash3")
				}
				);
			collectionAudio.Add(MoneyTypes.MoneyBag, new AudioClip[3]
				);
			collectionAudio.Add(MoneyTypes.GoldBar, new AudioClip[1]
				{
					AudioManager.Inst.FindOrLoadAudioClip("Collection/gem score"),
				});
			collectionAudio.Add(MoneyTypes.Gemstone, new AudioClip[1]
				{
					AudioManager.Inst.FindOrLoadAudioClip("Collection/gem score"),
				});
		}
		private GameObject Load(string prefabName)
		{
			return Resources.Load<GameObject>(prefabName);
		}
		public GameObject InstantiatePrefab(string prefabName)
		{
			//If we don't yet have that prefab.
			if (!PrefabDict.ContainsKey(prefabName))
			{
				//Add it
				PrefabDict.Add(prefabName, Load(prefabName));
			}

			//Then instantiate it.
			return Instantiate(PrefabDict[prefabName]);
		}
		public GameObject Instantiate(GameObject go)
		{
			return Instantiate<GameObject>(go);
		}
		public GameObject Instantiate(MoneyTypes type)
		{
			if (moneyProjectiles.ContainsKey(type))
			{
				return Instantiate(moneyProjectiles[type]);
			}
			Debug.LogError("HeistManager did not know about a money projectile prefab for the type" + type.ToString() + "\n\tReturning Null");
			return null;
		}

		public AudioClip GetRandomCollision(MoneyTypes type)
		{
			if (collisionAudio.ContainsKey(type))
			{
				int index = Random.Range(0, collisionAudio[type].Length);
				return collisionAudio[type][index];
			}
			return null;
		}
		public AudioClip GetRandomCollectionAudio(MoneyTypes type)
		{
			if (collisionAudio.ContainsKey(type))
			{
				int index = Random.Range(0, collectionAudio[type].Length);
				return collectionAudio[type][index];
			}
			return null;
		}
		public int GetMonetaryValue(MoneyTypes type)
		{
			if (MonetaryValue.ContainsKey(type))
			{
				return MonetaryValue[type];
			}
			return 0;
		}
		public GameObject CreateCollectionVisual(int monetaryValue)
		{
			GameObject prefab = FindAppropriateCollectionVisual(monetaryValue);
			return Instantiate(prefab);
		}

		private GameObject FindAppropriateCollectionVisual(int monetaryValue)
		{
			int whichVisual = 0;
			if (monetaryValue > 0 && monetaryValue < 25)
			{
				whichVisual = 0;
			}
			else if (monetaryValue >= 25 && monetaryValue < 100)
			{
				whichVisual = 1;
			}
			else if (monetaryValue >= 100)
			{
				whichVisual = 2;
			}
			if (CollectionVisuals[whichVisual] == null)
			{
				whichVisual = 0;
				Debug.LogError("Attempted to use invalid collection visual\n\tDefaulting to earlier collection visual");
			}
			return CollectionVisuals[whichVisual];
		}

		public GameObject CreateDestructionVisual(int monetaryValue)
		{
			GameObject prefab = FindAppropriateDestructionVisual(monetaryValue);
			return Instantiate(prefab);
		}

		private GameObject FindAppropriateDestructionVisual(int monetaryValue)
		{
			int whichVisual = 0;
			if (monetaryValue > 0 && monetaryValue < 25)
			{
				whichVisual = 0;
			}
			else if (monetaryValue >= 25 && monetaryValue < 100)
			{
				whichVisual = 1;
			}
			else if (monetaryValue >= 100)
			{
				whichVisual = 2;
			}
			if (DestructionVisuals[whichVisual] == null)
			{
				whichVisual = 0;
				Debug.LogError("Attempted to use invalid destruction visual\n\tDefaulting to earlier destruction visual");
			}
			return DestructionVisuals[whichVisual];
		}

		public override void OnDestroy()
		{
			Physics.gravity = storedGravity;
			base.OnDestroy();
		}
	}
}