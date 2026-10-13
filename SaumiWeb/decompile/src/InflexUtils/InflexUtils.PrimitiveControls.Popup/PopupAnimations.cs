using System;

namespace InflexUtils.PrimitiveControls.Popup;

[Flags]
public enum PopupAnimations
{
	None = 0,
	LeftToRight = 1,
	RightToLeft = 2,
	TopToBottom = 4,
	BottomToTop = 8,
	Center = 0x10,
	Slide = 0x40000,
	Blend = 0x80000,
	Roll = 0x100000,
	SystemDefault = 0x200000
}
