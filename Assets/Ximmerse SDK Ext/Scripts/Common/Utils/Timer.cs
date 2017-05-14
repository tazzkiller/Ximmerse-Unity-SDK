//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================


#if true
using System.Threading;
#elif UNITY_EDITOR_WIN
using System.Runtime.InteropServices;
#endif

namespace Ximmerse{

	public class Timer {

		public delegate void OnTick(int passedTime);

		public int timerID;
		public int interval;
		public OnTick onTick;

		protected int m_Timestamp;

		public Timer() {
		}

		public Timer(int interval):this(){
			this.interval=interval;
		}

		public virtual void DoTick(){
			if(onTick!=null){
				int t=InputSystem.XDevicePlugin.GetTickCount();
					onTick(t-m_Timestamp);
				m_Timestamp=t;
			}
		}

#if true
		protected System.Threading.Timer m_Timer;

		public virtual void Start(){
			if(m_Timer==null) {
				m_Timer=new System.Threading.Timer(OnTimerCallback,this,interval,interval);
			}
		}

		public virtual void Stop(){
			if(m_Timer!=null) {
				m_Timer.Dispose();
				m_Timer=null;
			}
		}

		public virtual void OnTimerCallback(object state) {
			if(state==this){
				DoTick();
			}
		}

#elif UNITY_EDITOR_WIN

		public delegate void TIMERPROC(
			System.IntPtr hwnd,
			uint uMsg,
			uint idEvent,
			int dwTime
		);

		[DllImport("User32.dll")]
		public static extern int SetTimer(
			System.IntPtr hWnd,
			int nIDEvent,
			int uElapse,
			TIMERPROC lpTimerFunc
		);

		[DllImport("User32.dll")]
		public static extern bool KillTimer(
			System.IntPtr hWnd,
			int uIDEvent
		);

		public virtual void Start() {
			if(timerID==0) {
				timerID=SetTimer(System.IntPtr.Zero,0,interval,OnTimerProc);
			}
		}

		public virtual void Stop() {
			if(timerID!=0) {
				KillTimer(System.IntPtr.Zero,timerID);
				timerID=0;
			}
		}

		public virtual void OnTimerProc(System.IntPtr hwnd,uint uMsg,uint idEvent,int dwTime){
			if(idEvent==timerID){
				DoTick();
			}
		}

#else

		public virtual void Start() {
		}

		public virtual void Stop() {
		}

#endif

	}

}