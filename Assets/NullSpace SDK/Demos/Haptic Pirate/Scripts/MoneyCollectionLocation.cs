using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class MoneyCollectionLocation : MonoBehaviour
	{
		private Collider _collider;
		private Collider MyCollider
		{
			get
			{
				if (_collider == null)
				{
					_collider = GetComponent<Collider>();
				}
				return _collider;
			}
			set { _collider = value; }
		}
		public Vector3 OutDirection = Vector3.up;
		public GameObject Floor;

		public class MoneyCollection
		{
			public Collider moneyCollide;
			public Transform collectionBin;
			public Transform collectionFloor;
			public Vector3 collisionLocation;
			public Vector3 dirToOutOfBin;
		}
		public MoneyTypes AllowedMoneys;

		void OnTriggerEnter(Collider col)
		{
			//Don't do a getcomponent if it isn't on our layer.
			if (col.gameObject.layer == 8)
			{
				MoneyProjectile money = col.GetComponent<MoneyProjectile>();
				if (money != null && CanWeCollectThisMoney(money))
				{
					MoneyCollection collection = new MoneyCollection();
					collection.collisionLocation = MyCollider.ClosestPoint(col.transform.position);
					collection.moneyCollide = col;
					collection.collectionBin = transform;
					collection.collectionFloor = Floor.transform;
					collection.dirToOutOfBin = OutDirection;

					CollectMoney(money, collection);
				}
			}
		}

		public bool CanWeCollectThisMoney(MoneyProjectile money)
		{
			return money.CurrentState == MoneyProjectile.MoneyState.Available;
		}

		public void CollectMoney(MoneyProjectile money, MoneyCollection OutDirection)
		{
			money.StartCoroutine(money.CollectMoney(OutDirection));
		}
	}
}