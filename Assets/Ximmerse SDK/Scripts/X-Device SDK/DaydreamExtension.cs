//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;

namespace Ximmerse.InputSystem {

	public enum DaydreamButton {
		// Daydream Standard
		Touch   = ControllerRawButton.LeftThumbMove,
		Click   = ControllerRawButton.LeftThumb,
		App     = ControllerRawButton.Back,
		Home    = ControllerRawButton.Start,
		// Touchpad To Dpad
		DpadUp    = ControllerRawButton.DpadUp,
		DpadDown  = ControllerRawButton.DpadDown,
		DpadLeft  = ControllerRawButton.DpadLeft,
		DpadRight = ControllerRawButton.DpadRight,
		// Ximmerse Ex
		Trigger = ControllerRawButton.LeftTrigger,
		GripL   = ControllerRawButton.LeftShoulder,
		GripR   = ControllerRawButton.RightShoulder,
		Grip    = GripL|GripR,

	}

	public static class DaydreamExtension {

		public static bool GetButton(this ControllerInput thiz,DaydreamButton  buttonMask) {
			if(thiz==null) {
				return false;
			}
			return thiz.GetButton((uint)buttonMask);
		}

		public static bool GetButtonDown(this ControllerInput thiz,DaydreamButton  buttonMask) {
			if(thiz==null) {
				return false;
			}
			return thiz.GetButtonDown((uint)buttonMask);
		}

		public static bool GetButtonUp(this ControllerInput thiz,DaydreamButton  buttonMask) {
			if(thiz==null) {
				return false;
			}
			return thiz.GetButtonUp((uint)buttonMask);
		}

		/// <summary>
		/// In Daydream standard,if the user is touching the touchpad, this is the touch position in
		/// normalized coordinates, where (0,0) is the top-left of the touchpad
		/// and (1,1) is the bottom right. If the user is not touching the touchpad,
		/// then this is the position of the last touch.
		/// </summary>
		public static Vector2 GetTouchPos(this ControllerInput thiz) {
			if(thiz==null) {
				return Vector2.zero;
			}
			float x=thiz.GetAxis(ControllerRawAxis.RightThumbX),
				  y=thiz.GetAxis(ControllerRawAxis.RightThumbY);
			x=Mathf.Clamp01((x+1.0f)/ 2.0f);
			y=Mathf.Clamp01((y-1.0f)/-2.0f);
			return new Vector2(x,y);
		}
	}
}