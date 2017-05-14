//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using System.Runtime.InteropServices;

namespace Ximmerse.ThirdParty {

	public class XInputPlugin {

		#region Const

		public const string LIB_KERNEL="kernel32";

		public enum Version {
			Xinput9_1_0,
			Xinput1_3,
			Xinput1_4,
		}

		#endregion Const

		#region Nested Types

		[StructLayout(LayoutKind.Sequential)]
		public struct XINPUT_STATE {
			public int dwPacketNumber;
			public XINPUT_GAMEPAD Gamepad;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct XINPUT_GAMEPAD {
			public short  wButtons;
			public byte  bLeftTrigger;
			public byte  bRightTrigger;
			public short sThumbLX;
			public short sThumbLY;
			public short sThumbRX;
			public short sThumbRY;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct XINPUT_VIBRATION {
			public ushort wLeftMotorSpeed;
			public ushort wRightMotorSpeed;
		}

		/// <summary>
		/// Taken from: https://msdn.microsoft.com/en-us/library/windows/apps/microsoft.directx_sdk.reference.xinput_gamepad
		/// </summary>
		public enum XINPUT_GAMEPAD_BUTTON {
			XINPUT_GAMEPAD_DPAD_UP        = 0x0001,
			XINPUT_GAMEPAD_DPAD_DOWN      = 0x0002,
			XINPUT_GAMEPAD_DPAD_LEFT      = 0x0004,
			XINPUT_GAMEPAD_DPAD_RIGHT     = 0x0008,
			XINPUT_GAMEPAD_START          = 0x0010,
			XINPUT_GAMEPAD_BACK           = 0x0020,
			XINPUT_GAMEPAD_LEFT_THUMB     = 0x0040,
			XINPUT_GAMEPAD_RIGHT_THUMB    = 0x0080,
			XINPUT_GAMEPAD_LEFT_SHOULDER  = 0x0100,
			XINPUT_GAMEPAD_RIGHT_SHOULDER = 0x0200,
			XINPUT_GAMEPAD_A              = 0x1000,
			XINPUT_GAMEPAD_B              = 0x2000,
			XINPUT_GAMEPAD_X              = 0x4000,
			XINPUT_GAMEPAD_Y              = 0x8000,
			// Taken from: https://code.google.com/p/x360ce/issues/detail?id=417
			XINPUT_GAMEPAD_GUIDE          = 0x0400,
		}

		public enum XINPUT_BATTERY_TYPE:byte{
			BATTERY_TYPE_DISCONNECTED,//The device is not connected. 
			BATTERY_TYPE_WIRED,//The device is a wired device and does not have a battery. 
			BATTERY_TYPE_ALKALINE,//The device has an alkaline battery. 
			BATTERY_TYPE_NIMH,//The device has a nickel metal hydride battery. 
			BATTERY_TYPE_UNKNOWN,//The device has an unknown battery type. 
		}

		public enum XINPUT_BATTERY_LEVEL:byte {
			BATTERY_LEVEL_EMPTY,
			BATTERY_LEVEL_LOW,
			BATTERY_LEVEL_MEDIUM,
			BATTERY_LEVEL_FULL,
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct XINPUT_BATTERY_INFORMATION {
			public XINPUT_BATTERY_TYPE BatteryType;
			public XINPUT_BATTERY_LEVEL BatteryLevel;
		}

		#endregion Nested Types

		#region Natives

		[DllImport(LIB_KERNEL)]
        private extern static System.IntPtr LoadLibrary(string path);

		[DllImport(LIB_KERNEL)]
        private extern static System.IntPtr GetProcAddress(System.IntPtr lib, string funcName);

		[DllImport(LIB_KERNEL)]
        private extern static bool FreeLibrary(System.IntPtr lib);

		public delegate int XInputGetStateImpl(int dwUserIndex,ref XINPUT_STATE pState);
		public delegate int XInputSetStateImpl(int dwUserIndex,ref XINPUT_VIBRATION pVibration);
		public delegate int XInputGetBatteryInformationImpl(int dwUserIndex,byte devType,ref XINPUT_BATTERY_INFORMATION pBatteryInformation);
		
		#endregion Natives

		#region Nested Types

		public class Impl:System.IDisposable {
			protected System.IntPtr m_Lib;
			public Version version;
			public XInputGetStateImpl XInputGetState;
			public XInputSetStateImpl XInputSetState;
			public XInputGetBatteryInformationImpl XInputGetBatteryInformation;

			public Impl(Version version) {
				this.version=version;
				m_Lib=LoadLibrary(this.version.ToString());
				if(m_Lib!=System.IntPtr.Zero) {
					System.IntPtr api;
					api=GetProcAddress(m_Lib,"XInputGetState");
					XInputGetState=(XInputGetStateImpl)Marshal.GetDelegateForFunctionPointer(api,typeof(XInputGetStateImpl));
					api=GetProcAddress(m_Lib,"XInputSetState");
					XInputSetState=(XInputSetStateImpl)Marshal.GetDelegateForFunctionPointer(api,typeof(XInputSetStateImpl));
					api=GetProcAddress(m_Lib,"XInputGetBatteryInformation");
					XInputGetBatteryInformation=(XInputGetBatteryInformationImpl)Marshal.GetDelegateForFunctionPointer(api,typeof(XInputGetBatteryInformationImpl));
				}
			}

			public void Dispose() {
				if(m_Lib!=System.IntPtr.Zero) {
					FreeLibrary(m_Lib);
					m_Lib=System.IntPtr.Zero;
				}
			}
		}

		#endregion Nested Types

	}
}
