//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using System.Collections.Generic;
using UnityEngine;

public interface IAnimatable {

	/// <summary>
	/// 
	/// </summary>
	void AdvanceTime(float deltaTime);

}

/// <summary>
/// 
/// </summary>
public class DelayedCall:IAnimatable {

	//
	public static DelayedCall current;

	public const int
		k_OneShot=-1,
		k_EveryFrame=-2;

	public static readonly object[] s_EmptyArgs=new object[0];
	public delegate void Call(
#if DELAYED_CALL_HAS_ARGS
		object[] args
#endif
);
	/// <summary>
	/// 
	/// </summary>
	public static void Free(ref DelayedCall delayedCall){
		if(delayedCall!=null){
		if(delayedCall.m_IsRunning){
			delayedCall.Abort();
		}}
		delayedCall=null;
	}

	#region Pooling

	public static List<DelayedCall> s_Pool=new List<DelayedCall>(16);
	public static int s_LenPool=0;

	/// <summary>
	/// 
	/// </summary>
	public static DelayedCall Pop(){
		DelayedCall ret=null;
		if(s_LenPool==0){
			ret=new DelayedCall();
		}else{
			--s_LenPool;
			ret=DelayedCall.s_Pool[s_LenPool];
			s_Pool.RemoveAt(s_LenPool);
		}
		ret.m_IsRunning=true;
		return ret;
	}

	/// <summary>
	/// 
	/// </summary>
	public static void Push(DelayedCall item){
		int i=s_Pool.IndexOf(item);
		if(i!=-1){
			Ximmerse.Log.w("DelayedCall","Push failed...");
			return;
		}else{
			item.Reset();// ResetToBeginning
			s_Pool.Add(item);++s_LenPool;// Push
		}
	}

	#endregion Pooling

	#region Members

	public Juggler juggler=null;
	public float delay=0f;
	public float duration=1f;
	public int repeatCount=k_OneShot;
	public Call call=null;
	public object[] args=s_EmptyArgs;

	protected bool m_IsRunning=false;
	protected bool m_Started=false;
	protected bool m_IsDelay=false;
	protected float m_StartTime=0f;
	protected float m_Duration=0f;
	protected int m_RepeatCount=0;

	#endregion Members

	#region Built-in

	/// <summary>
	/// 
	/// </summary>
	protected DelayedCall(){
	}

	/// <summary>
	/// 
	/// </summary>
	public virtual void Abort() {
		if(m_Started){if(m_IsRunning){
			juggler.Remove(this);
			Push(this);
			m_IsRunning=false;
		}}
	}

	/// <summary>
	/// 
	/// </summary>
	public virtual void Init(
		float i_delay,float i_duration,int i_repeatCount,
		Call i_call,object[] i_args=null
	) {
		delay=i_delay;
		duration=i_duration;
		repeatCount=i_repeatCount;
		call=i_call;
		args=i_args;
	}

	/// <summary>
	/// 
	/// </summary>
	public virtual void Reset() {

		juggler=null;
		delay=0f;
		duration=1f;
		repeatCount=k_OneShot;
		call=null;
		args=s_EmptyArgs;

		m_IsRunning=false;
		m_Started=false;
		m_IsDelay=false;
		m_StartTime=0f;
		m_Duration=0f;
		m_RepeatCount=0;

	}

	#endregion Built-in

	#region IAnimatable

	/// <summary>
	/// 
	/// </summary>
	public virtual void AdvanceTime(float deltaTime) {
		float delta=deltaTime;
		float time=juggler.elapsedTime;

		if(!m_Started) {// Init Timer
			m_Started=m_IsDelay=true;
			m_StartTime=time+delay;
			m_Duration=0.0f;
			m_RepeatCount=0;
		}

		if(time<m_StartTime){
			return;
		}else if(m_IsDelay){
			m_IsDelay=false;
			if(repeatCount>k_OneShot){// Fix the [Delay Mode : Repeat]
				m_Duration+=delta;
				++m_RepeatCount;
				InvokeCall();
				if(m_RepeatCount>=repeatCount) {
					Abort();
					return;
				}
				return;
			}
		}

		switch(repeatCount){
			case k_OneShot:// Delay Mode : One Shot
				InvokeCall();
				Abort();
				return;
			//break;
			case k_EveryFrame:// Delay Mode : Every Frame
				m_Duration+=delta;
				InvokeCall();
				if(m_Duration>=duration){
					Abort();
					return;
				}
			break;
			default:// Delay Mode : Repeat
				m_Duration+=delta;
				if(m_Duration>=duration){
					m_Duration-=duration;// @ Reserve time error.
					++m_RepeatCount;
					InvokeCall();
					if(m_RepeatCount>=repeatCount){
						Abort();
						return;
					}
				}
			break;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public virtual void InvokeCall(){
		if(call!=null){
			current=this;
			try {
				call.Invoke();
			}catch(System.Exception e) {
				Ximmerse.Log.e("DelayedCall",e.ToString());
			}
			current=null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public virtual float percent {
		get {
			float f=0.0f;
			if(m_Started) {
			switch(repeatCount){
					case k_OneShot:// Delay Mode : One Shot
						f=((juggler.elapsedTime<m_StartTime))?1.0f:0.0f;
					break;
					case k_EveryFrame:// Delay Mode : Every Frame
						f=m_Duration/duration;
					break;
					default:// Delay Mode : Repeat
						f=(float)m_RepeatCount/repeatCount;
					break;
				}
			}
			return f;
		}
	}

	#endregion IAnimatable

}

public class Juggler:MonoBehaviour,IAnimatable {
	
	/// <summary>
	/// 
	/// </summary>
	public static Juggler Main {
		get{
			if(main==null){
				main=new GameObject("New Juggler",typeof(Juggler)).GetComponent<Juggler>();
				Object.DontDestroyOnLoad(main);
			}
			return main;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public static Juggler main;

	public List<IAnimatable> m_List=new List<IAnimatable>(16);
	
	public bool asMain;
	public float elapsedTime=0.0f;
	protected bool m_IsLock;

	#region Unity Messages

	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake() {
		elapsedTime=0.0f;
		if(asMain){
		if(main==null){
			main=this;
			//Object.DontDestroyOnLoad(main);
		}}
	}

	/// <summary>
	/// 
	/// </summary>
	protected virtual void Update() {
		AdvanceTime(Time.deltaTime);
	}

	#endregion Unity Messages

	#region IAnimatable

	/// <summary>
	/// 
	/// </summary>
	public virtual void AdvanceTime(float deltaTime) {
		int imax=m_List.Count;
		int currentIndex=0;
		int i;

		elapsedTime+=deltaTime;
		if(imax==0)
			return;

		// there is a high probability that the "advanceTime" function modifies the list 
		// of animatables. we must not process new objects right now (they will be processed
		// in the next frame), and we need to clean up any empty slots in the list.

		for(i=0;i<imax;++i) {
			IAnimatable item=m_List[i];
			if(item!=null) {
				// shift object into empty slots along the way
				if(currentIndex!=i) {
					m_List[currentIndex]=item;
					m_List[i]=null;
				}

				item.AdvanceTime(deltaTime);
				++currentIndex;
			}
		}

		if(currentIndex!=i) {
			imax=m_List.Count; // count might have changed!
			while(i<imax) {
				m_List[currentIndex++]=m_List[i++];
			}
			m_List.RemoveRange(currentIndex,imax-currentIndex);
		}
	}

	#endregion IAnimatable

	#region APIs

	public virtual void Add(IAnimatable item) {
		//print(Time.realtimeSinceStartup);
		int i=m_List.IndexOf(item);
		if(i==-1) {
			m_List.Add(item);
		}
	}

	public virtual void Remove(IAnimatable item) {
		//print(Time.realtimeSinceStartup);
		int i=m_List.IndexOf(item);
		if(i!=-1) {
			m_List[i]=null;
		}
	}

	public virtual DelayedCall DelayCall(DelayedCall.Call call,float delay) {
		if(call==null)
			return null;
		
		DelayedCall delayedCall=DelayedCall.Pop();
		delayedCall.juggler=this;
		delayedCall.Init(
			delay,0.0f,DelayedCall.k_OneShot,call,null
		);
		Add(delayedCall);
		return delayedCall;
	}

	public virtual DelayedCall RepeatCall(DelayedCall.Call call,float delay,float duration,int repeatCount) {
		if(call==null)
			return null;
		
		DelayedCall delayedCall=DelayedCall.Pop();
		delayedCall.juggler=this;
		delayedCall.Init(
			delay,duration,repeatCount,call,null
		);
		Add(delayedCall);
		return delayedCall;
	}

	public virtual DelayedCall UpdateCall(DelayedCall.Call call,float delay,float duration) {
		if(call==null)
			return null;
		
		DelayedCall delayedCall=DelayedCall.Pop();
		delayedCall.juggler=this;
		delayedCall.Init(
			delay,duration,DelayedCall.k_EveryFrame,call,null
		);
		Add(delayedCall);
		return delayedCall;
	}

	#endregion APIs
}