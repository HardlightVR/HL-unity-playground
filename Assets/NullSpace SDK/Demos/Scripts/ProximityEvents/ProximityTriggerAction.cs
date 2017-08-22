using UnityEngine;
using System.Collections;

public class ProximityTriggerAction : MonoBehaviour
{
	public LayerMask CollideWith;
	public string NameRequires;

	public bool CanTrigger = true;
	public bool TriggerOnce = true;
	public int TriggerCount = 0;

	public delegate void OnTriggerEnterDelegate(Collider c);
	public OnTriggerEnterDelegate triggerEnterDelegate;
	public delegate void OnTriggerStayDelegate(Collider c);
	public OnTriggerStayDelegate triggerStayDelegate;
	public delegate void OnTriggerExitDelegate(Collider c);
	public OnTriggerExitDelegate triggerExitDelegate;

	public void Reset()
	{
		CanTrigger = true;
		TriggerCount = 0;
	}

	void OnTriggerEnter(Collider c)
	{
		if (triggerEnterDelegate != null)
		{
			if (CollideWith.value == (CollideWith.value | (1 << c.gameObject.layer)))
			{
				if (CanTrigger && (!TriggerOnce || TriggerCount <= 0))
				{
					if (NameRequires.Length == 0 || c.gameObject.name.Contains(NameRequires))
					{
						Debug.Log(name + " was triggered by " + c.gameObject.name + " for the " + TriggerCount + "th time\n");

						TriggerCount++;
						//Call our assigned delegate
						triggerEnterDelegate(c);
					}
				}
			}
		}
	}

	void OnTriggerStay(Collider c)
	{
		if (triggerStayDelegate != null)
		{
			if (CollideWith.value == (CollideWith.value | (1 << c.gameObject.layer)))
			{
				if (CanTrigger && (!TriggerOnce || TriggerCount <= 0))
				{
					if (NameRequires.Length == 0 || c.gameObject.name.Contains(NameRequires))
					{
						//Debug.Log(name + " was triggered by " + c.gameObject.name + " for the " + TriggerCount + "th time\n");

						TriggerCount++;
						//Call our assigned delegate
						triggerStayDelegate(c);
					}
				}
			}
		}
	}

	void OnTriggerExit(Collider c)
	{
		//Debug.Log(c.name + "\n" + (triggerExitDelegate == null));
		if (triggerExitDelegate != null)
		{
			if (CollideWith.value == (CollideWith.value | (1 << c.gameObject.layer)))
			{
				if (CanTrigger)
				{
					if (NameRequires.Length == 0 || c.gameObject.name.Contains(NameRequires))
					{
						//Call our assigned delegate
						triggerExitDelegate(c);
					}
				}
			}
		}
	}
}