using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.NewLocation
{
	public class ControllerLocation : HapticLocationNode
	{
		public ControllerSide MyLocation = 0;
		public ControllerLocation(ControllerSide Location)
		{
			MyLocation = Location;
		}
		public override int Where()
		{
			return (int)MyLocation;
		}
		public override HapticLocationType GetHapticType()
		{
			return HapticLocationType.Controller;
		}
		public override ControllerSide ConvertToControllerSide()
		{
			return MyLocation;
		}
		public override string ToString()
		{
			return "Controller [" + MyLocation.ToString() + "]";
		}
	}
}