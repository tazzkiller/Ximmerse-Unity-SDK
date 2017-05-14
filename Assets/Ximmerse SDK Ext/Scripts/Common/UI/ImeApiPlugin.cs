using System.Runtime.InteropServices;

public class ImeApiPlugin {

	#region Natives

	public const string LIB_IME="imeapi";

	public const int
		 IME_KEY_ACTION_DOWN =(0x00000000)
		,IME_KEY_ACTION_UP   =(0x00000001)
	;

	[DllImport(LIB_IME)]
	public static extern int ime_init();

	[DllImport(LIB_IME)]
	public static extern int ime_exit();

	[DllImport(LIB_IME)]
	public static extern bool ime_is_open();

	[DllImport(LIB_IME)]
	public static extern int ime_simulate_key(System.IntPtr context,int code,int action);

	[DllImport(LIB_IME)]
	public static extern int ime_c2w(System.IntPtr wdest,System.IntPtr csrc);

	[DllImport(LIB_IME)]
	public static extern int ime_get_composition_string(System.IntPtr data,int length);

	[DllImport(LIB_IME)]
	public static extern int ime_get_candidate_list_count();

	[DllImport(LIB_IME)]
	public static extern int ime_get_candidate_list_item(int index,System.IntPtr data,int length);

	#endregion Natives

	#region Advanced

	static object s_CharLock=new object();
	static System.IntPtr s_CharPtr=Marshal.AllocHGlobal(128);
	static System.IntPtr s_WCharPtr=Marshal.AllocHGlobal(128*2);

	public static string ime_get_composition_string() {
		string ret=null;
		lock(s_CharLock) {
			int size=ime_get_composition_string(s_CharPtr,128);
			if(size>0) {
				size=ime_c2w(s_WCharPtr,s_CharPtr);
				ret=Marshal.PtrToStringUni(s_WCharPtr);
			}
		}
		return ret;
	}

	public static string ime_get_candidate_list_item(int index) {
		string ret=null;
		lock(s_CharLock) {
			int size=ime_get_candidate_list_item(index,s_CharPtr,128);
			if(size>0) {
				size=ime_c2w(s_WCharPtr,s_CharPtr);
				ret=Marshal.PtrToStringUni(s_WCharPtr);
			}
		}
		return ret;
	}

	public static void ime_simulate_key(int code) {
		ime_simulate_key(System.IntPtr.Zero,code,IME_KEY_ACTION_DOWN);
		Juggler.Main.DelayCall(()=>{
			ime_simulate_key(System.IntPtr.Zero,code,IME_KEY_ACTION_UP);
		},.25f);
	}

	#endregion Advanced

}