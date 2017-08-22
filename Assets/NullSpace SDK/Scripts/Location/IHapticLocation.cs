using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.NewLocation
{
	public enum HapticLocationType { Suit, Wearable, Controller, Prop }
	public interface IHapticLocation
	{
		int Where();
		HapticLocationType GetHapticType();
		AreaFlag ConvertToAreaFlag();
		ControllerSide ConvertToControllerSide();
	}

	public abstract class HapticLocationNode : IHapticLocation
	{
		public abstract int Where();
		public abstract HapticLocationType GetHapticType();
		public virtual AreaFlag ConvertToAreaFlag()
		{ return AreaFlag.None; }
		public virtual ControllerSide ConvertToControllerSide()
		{ return ControllerSide.None; }
	}
}