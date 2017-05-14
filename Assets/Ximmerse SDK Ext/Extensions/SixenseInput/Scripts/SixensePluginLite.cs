//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using System.Runtime.InteropServices;

namespace Ximmerse.ThirdParty {

	/// <summary>
	/// Ripped from SixenseUnityPlugin.
	/// You need copy sixense.dll from https://www.assetstore.unity3d.com/en/#!/content/7953
	/// to the folder "Assets\Plugins" before using this scripts
	/// </summary>
	public partial class SixensePluginLite{
		[StructLayout( LayoutKind.Sequential )]
		public struct sixenseControllerData
		{
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 )]
			public float[] pos;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 9 )]
			public float[] rot_mat;
			public float joystick_x;
			public float joystick_y;
			public float trigger;
			public uint buttons;
			public byte sequence_number;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
			public float[] rot_quat;
			public ushort firmware_revision;
			public ushort hardware_revision;
			public ushort packet_type;
			public ushort magnetic_frequency;
			public int enabled;
			public int controller_index;
			public byte is_docked;
			public byte which_hand;
			public byte hemi_tracking_enabled;
		}
	
		[StructLayout( LayoutKind.Sequential )]
		public struct sixenseAllControllerData
		{
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
			public sixenseControllerData[] controllers;
		}
	
		[DllImport( "sixense", EntryPoint = "sixenseInit" )]
		public static extern int sixenseInit();
	
		[DllImport( "sixense", EntryPoint = "sixenseExit" )]
		public static extern int sixenseExit();
	
		[DllImport( "sixense", EntryPoint = "sixenseGetMaxControllers" )]
		public static extern int sixenseGetMaxControllers();

		[DllImport( "sixense", EntryPoint = "sixenseGetNewestData" )]
		public static extern int sixenseGetNewestData( int which, ref sixenseControllerData cd );

	}

}