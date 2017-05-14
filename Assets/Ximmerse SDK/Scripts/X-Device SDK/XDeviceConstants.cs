//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

namespace Ximmerse.InputSystem {

	public partial class XDevicePlugin {

		public const int
			// Bool
			kField_CtxCanProcessInputEvent = 0,
			kField_IsAbsRotation           =10,
            kField_IsDeviceSleep           =11,
            // Int
            kField_CtxSdkVersion           = 0,
			kField_CtxDeviceVersion        = 1,
			kField_CtxPlatformVersion      = 2,
			kField_ErrorCode               = 2,
			kField_ConnectionState         = 3,
			kField_BatteryLevel            = 4,
			kField_TrackingResult          = 5,
			kField_TrackingOrigin          = 6,
			// Float
			kField_PositionScale           = 0,
			kField_TrackerHeight           = 1,
			kField_TrackerDepth            = 2,
			kField_TrackerPitch            = 3,
			// Object
			kField_TRS                     = 0,
			// Message
			kMessage_TriggerVibration      = 1,
			kMessage_RecenterSensor        = 2,
            kMessage_SleepMode             = 3,
            //
        OK                             = 0
		;
	}

	// Reference from GoogleVR.

	/// <summary>
	/// Represents the device's current connection state.
	/// </summary>
	public enum DeviceConnectionState {
		/// <summary>
		/// Indicates that the device is disconnected.
		/// </summary>
		Disconnected,
		/// <summary>
		/// Indicates that the host is scanning for devices.
		/// </summary>
		Scanning,
		/// <summary>
		/// Indicates that the device is connecting.
		/// </summary>
		Connecting,
		/// <summary>
		/// Indicates that the device is connected.
		/// </summary>
		Connected,
		/// <summary>
		/// Indicates that an error has occurred.
		/// </summary>
		Error,
	};

	[System.Flags]
	public enum TrackingResult{
		NotTracked      =    0,
		RotationTracked = 1<<0,
		PositionTracked = 1<<1,
		PoseTracked     = (RotationTracked|PositionTracked),
	};
}
