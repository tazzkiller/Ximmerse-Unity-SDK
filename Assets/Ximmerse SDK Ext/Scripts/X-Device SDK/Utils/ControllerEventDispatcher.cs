using System.Collections.Generic;
using UnityEngine;
using Ximmerse.InputSystem;

public class ControllerEventDispatcher:MonoBehaviour {

	#region Nested Types
	
	public interface IListener {
		bool isActiveAndEnabled{get;}
		void OnControllerUpdate(ControllerEventDispatcher sender,ControllerInput input);
	}

	#endregion Nested Types

	#region Fields

	public TrackedObject trackedObject;
	public ControllerInput input;
	protected List<IListener> m_Listeners=new List<IListener>();

	#endregion Fields

	#region Unity Messages

	protected virtual void Start() {
		// Assign the controller type automatically.
		if(trackedObject==null) {
			Transform t = transform;
			while(t!=null) {
				trackedObject=t.GetComponent<TrackedObject>();
				if(trackedObject!=null) {
					break;
				}
				//
				t=t.parent;
			}
		}
		if(trackedObject!=null) {
			input=ControllerInputManager.instance.GetControllerInput(trackedObject.source);
			MonoBehaviour[] list=trackedObject.GetComponentsInChildren<MonoBehaviour>();
			for(int i=0,imax=list.Length;i<imax;++i) {
				if(list[i] is IListener) {
					m_Listeners.Add(list[i] as IListener);
				}
			}
		}
	}

	protected virtual void Update() {
		if(input==null){
			return;
		}
		input.UpdateState();
		//
		IListener l;
		for(int i=0,imax=m_Listeners.Count;i<imax;++i){
			l=m_Listeners[i];
			//
			if(l!=null) {
			if(l.isActiveAndEnabled) {
				l.OnControllerUpdate(this,input);
			}}
		}
	}

	#endregion Unity Messages

}
