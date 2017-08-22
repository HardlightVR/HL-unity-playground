using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	public class SingleCoin : MoneyProjectile
	{
		//Override our grab functionality to immediately collect.

		protected override void HandleCollection(MoneyCollectionLocation.MoneyCollection collection)
		{
			DisableVisuals();
			DisableCollision();
		}

		protected override void OnTriggerEnter(Collider col)
		{
			//We only check for Ignore Raycast (as that is where the HandObject is located)
			if (col.gameObject.layer == 2)
			{
				HandObject hand = col.GetComponent<HandObject>();
				if (hand != null)
				{
					MoneyCollectionLocation.MoneyCollection collection = new MoneyCollectionLocation.MoneyCollection();
					collection.moneyCollide = col;
					collection.dirToOutOfBin = Vector3.up;
					collection.collectionFloor = col.transform;
					StartCoroutine(CollectMoney(collection));
					Destroy(gameObject, 5);
				}
			}
			base.OnTriggerEnter(col);
		}
	}
}