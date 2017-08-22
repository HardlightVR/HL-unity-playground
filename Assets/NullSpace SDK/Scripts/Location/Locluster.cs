using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NullSpace.SDK;

public class Locluster
{
	public string Name;
	public NullSpace.SDK.NewLocation.HapticLocationNode Location;
	public Locluster Parent;
	public List<Locluster> _children;
	public List<Locluster> Children
	{
		get
		{
			if (_children == null)
			{
				_children = new List<Locluster>();
			}
			return _children;
		}
		set { _children = value; }
	}

	public Locluster(string name)
	{
		Name = name;
	}

	public override string ToString()
	{
		string output = "Locluster [" + Name + "]\n";
		if (Parent != null)
		{
			output += "\tParent: [" + Parent.Name + "]\n";
		}
		output += "\tChildren: [" + Children.Count + "]\n";
		for (int i = 0; i < Children.Count; i++)
		{
			output += Children[i].Name + "  -  ";
		}
		return output;
	}
	public void AddChild(Locluster newCluster)
	{
		if (Children.Contains(newCluster))
			return;
		Children.Add(newCluster);
	}
}

public class TesterForLocluster
{
	public TesterForLocluster()
	{
		Debug.Log("Beginning Locluster Test\n");

		Locluster Body = new Locluster("Body (Root)");
		Locluster Upper = new Locluster("Upper Body");
		Locluster Lower = new Locluster("Lower Body");
		Body.AddChild(Upper);
		Body.AddChild(Lower);
	}

	void Thing()
	{
		HapticSequence seq = new HapticSequence();
		//seq.Play(AreaFlag.

		//We want an AreaFlag like object.
		//It needs to be enum-like or classlike in behavior?
	}
	private class Touchoid
	{
		//private List<HapticIntent> Intents = new List<Intent>();
		//public bool AddIntent(Intent intent)
		//{
		//	Intents.Add(intent);
		//	return true;
		//}
		//public bool AddFloatIntent(OneFloatIntent intent, float time)
		//{

		//}
	}
	private enum Intent
	{
		Fast,
		Slow,
		Waxing,
		Waning,
		Echoing,
		Weak,
		Intense,
		FadeOut,
		FadeIn,
	}
	private enum OneFloatIntent
	{
		Duration,
		Attenuating,
		Repeating,
	}
	private enum HapticSpace
	{
		LeftDirection,
		RightDirection,
		Upwards,
		Downwards,
		Forwards,
		Backwards,
	}
	private enum HapticEnum
	{


	}
}