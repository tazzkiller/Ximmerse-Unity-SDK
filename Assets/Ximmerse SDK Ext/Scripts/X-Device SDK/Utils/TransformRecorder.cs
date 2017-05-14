//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TransformRecorder:MonoBehaviour {

	#region Nested Types

	public class KeyFrame {
		public float time;
		public int pluginPassedFrames;
		public Vector3 position;
		public Quaternion rotation;

		public virtual void Deserialize(BinaryReader reader) {
			time=reader.ReadSingle();
			pluginPassedFrames=reader.ReadInt32();
			position.Set(
				 reader.ReadSingle(),
				 reader.ReadSingle(),
				-reader.ReadSingle()
			);
			rotation.Set(
				-reader.ReadSingle(),
				-reader.ReadSingle(),
				 reader.ReadSingle(),
				 reader.ReadSingle()
			);
		}

		public virtual void Serialize(BinaryWriter writer) {
			int i;
			writer.Write(time);
			writer.Write(pluginPassedFrames);
			i=0;
			writer.Write( position[i++]);
			writer.Write( position[i++]);
			writer.Write(-position[i++]);
			i=0;
			writer.Write(-rotation[i++]);
			writer.Write(-rotation[i++]);
			writer.Write( rotation[i++]);
			writer.Write( rotation[i++]);
		}

		public virtual string ToCsvString() {
			return time.ToString()+","+pluginPassedFrames+","+
				position.x.ToString("0.000000")+","+position.y.ToString("0.000000")+","+position.z.ToString("0.000000")+","+
				""
			;
		}
	}

	public class RecordObject {

		public TransformRecorder recorder;

		public string name;
		public TrackedObject trackedObject;
		public Transform transform;
		public List<KeyFrame> keyFrames;

		public RecordObject(TransformRecorder recorder,Transform transform) {
			this.name="";
			this.keyFrames=new List<KeyFrame>();
			//
			this.recorder=recorder;
			this.transform=transform;
			if(this.transform!=null) {
				this.name=this.transform.name;
				//this.trackedObject=this.transform.GetComponent<TrackedObject>();
			}
		}

		public virtual void DoRecord(float time) {
			if(transform==null) {return;}
			//
			KeyFrame keyFrame=new KeyFrame();
			keyFrame.time=time;
			keyFrame.pluginPassedFrames=getPluginPassedFrames();
			keyFrame.position=transform.position;
			keyFrame.rotation=transform.rotation;
			/*if(trackedObject==null) {
				keyFrame.position=transform.localPosition;
				keyFrame.rotation=transform.localRotation;
			}else {
				trackedObject.GetTransform(ref keyFrame.position,ref keyFrame.rotation);
				if(!trackedObject.trackPosition) {
					keyFrame.position=Vector3.zero;
				}
				if(!trackedObject.trackRotation) {
					keyFrame.rotation=Quaternion.identity;
				}
			}*/
			keyFrames.Add(keyFrame);
		}

		public virtual void DoPlay(int frame){
			if(transform==null) {return;}
			//
			if(frame>=0 && frame<keyFrames.Count) {
				KeyFrame keyFrame=keyFrames[frame];
				transform.name=this.name+"@"+keyFrame.time+
					((keyFrame.pluginPassedFrames==-1)?"":" By pass plugin "+keyFrame.pluginPassedFrames+" frame(s)");
				/*transform.localPosition=keyFrame.position;
				transform.localRotation=keyFrame.rotation;*/
				transform.position=keyFrame.position;
				transform.rotation=keyFrame.rotation;
			}
		}

		public virtual void Deserialize(BinaryReader reader) {
			name=reader.ReadString();
			//
			int i=0,imax=reader.ReadInt32();
			keyFrames=new List<KeyFrame>(imax);
			KeyFrame keyFrame;
			for(;i<imax;++i) {
				keyFrame=new KeyFrame();
				keyFrame.Deserialize(reader);
				keyFrames.Add(keyFrame);
			}
		}

		public virtual void Serialize(BinaryWriter writer) {
			writer.Write(name);
			//
			int i=0,imax=keyFrames.Count;
			writer.Write(imax);
			for(;i<imax;++i) {
				keyFrames[i].Serialize(writer);
			}
		}

		public virtual string ToCsvString() {
			System.Text.StringBuilder sb=new System.Text.StringBuilder();
			int i=0,imax=keyFrames.Count;
			for(;i<imax;++i) {
				sb.AppendLine(keyFrames[i].ToCsvString());
			}
			return sb.ToString();
		}

	}

	public enum RecordState {
		Idle,
		Record,
		Play,
	}

	#endregion Nested Types

	#region Fields

	public static System.Func<int> getPluginPassedFrames=()=>(-1);

	public RecordState recordState=RecordState.Idle;
	public WrapMode wrapMode=WrapMode.Loop;
	public float fps=60.0f;
	public string fileName;
	[SerializeField]protected Transform[] m_Transforms=new Transform[0];
	[System.NonSerialized]public RecordObject[] recordObjects=new RecordObject[0];
	[System.NonSerialized]protected int m_CurrentFrame,m_NumFrames,m_FrameStep;
	[System.NonSerialized]protected float m_Time,m_TimePerFrame;

	public ButtonEvent buttonRecord=new ButtonEvent(new ButtonEvent.PersistentListener{type=ButtonEvent.Type.Input_Key,intArg=(int)KeyCode.O});
	public ButtonEvent buttonPlay=new ButtonEvent(new ButtonEvent.PersistentListener{type=ButtonEvent.Type.Input_Key,intArg=(int)KeyCode.P});

	#endregion Fields

	#region Unity Messages

	protected virtual void Start() {
		recordState=RecordState.Idle;
		if(fps<=0f) {
			m_TimePerFrame=0.0f;
		}else {
			m_TimePerFrame=1.0f/fps;
		}
		//
		int i=0,imax=m_Transforms.Length;
		recordObjects=new RecordObject[imax];
		for(;i<imax;++i) {
			recordObjects[i]=new RecordObject(this,m_Transforms[i]); 
		}
		//
		if(!string.IsNullOrEmpty(fileName)) {
#if UNITY_EDITOR
			Load(fileName);
#endif
		}
	}

	protected virtual void Update() {
		if(buttonRecord.GetAnyDown()) {
			if(recordState==RecordState.Record){
				recordState=RecordState.Idle;
			}else {
				recordState=RecordState.Record;
				m_Time=0f;
			}
		}
		if(buttonPlay.GetAnyDown()) {
			if(recordState==RecordState.Play){
				recordState=RecordState.Idle;
			}else {
				recordState=RecordState.Play;
				m_Time=0f;
				m_FrameStep=1;
				m_CurrentFrame=m_NumFrames=0;
				if(recordObjects.Length>0) {
					m_NumFrames=recordObjects[0].keyFrames.Count;
				}
			}
		}
		//
		m_Time-=Time.deltaTime;
		if(m_Time<=0f) {
			UpdateRecorder();
			m_Time+=m_TimePerFrame;
		}
	}

	protected virtual void OnDestroy() {
		if(!string.IsNullOrEmpty(fileName)) {
			Save(fileName);
			for(int i=0,imax=recordObjects.Length;i<imax;++i) {
				File.WriteAllText(fileName+"_"+i+".csv",recordObjects[i].ToCsvString());
			}
		}
	}

	[ContextMenu("Print")]
	protected virtual void Print() {
		for(int i=0,imax=recordObjects.Length;i<imax;++i) {
			print(recordObjects[i].ToCsvString());
		}
	}

	#endregion Unity Messages

	#region Methods

	public virtual void Save(string path){
		BinaryWriter writer=new BinaryWriter(File.OpenWrite(path)); 
		writer.Write(new byte[4]{(byte)'d',(byte)'a',(byte)'t',(byte)0});
		int i=0,imax=recordObjects.Length;
		writer.Write(imax);
		for(;i<imax;++i) {
			recordObjects[i].Serialize(writer);
		}
		writer.Flush();
		writer.Close();
		writer.BaseStream.Close();
		//
#if UNITY_EDITOR
		AssetDatabase.Refresh();
#endif
	}

	public virtual void Load(string path){
		if(!File.Exists(path)) {return;}
		//
		BinaryReader reader=new BinaryReader(File.OpenRead(path));
		reader.ReadInt32();
		int i=0,imax=recordObjects.Length;
		reader.ReadInt32();
		for(;i<imax;++i) {
			recordObjects[i].Deserialize(reader);
		}
		reader.BaseStream.Close();
		reader.Close();
	}

	protected virtual void UpdateRecorder() {
		//
		switch(recordState) {
			case RecordState.Record:
				float time=Time.time;
				for(int i=0,imax=recordObjects.Length;i<imax;++i) {
					recordObjects[i].DoRecord(time);
				}
			break;
			case RecordState.Play:
				for(int i=0,imax=recordObjects.Length;i<imax;++i) {
					recordObjects[i].DoPlay(m_CurrentFrame);
				}
				m_CurrentFrame+=m_FrameStep;
				m_CurrentFrame=Ximmerse.MathUtil.Repeat(0,m_NumFrames-1,m_CurrentFrame);// Next frame.
			break;
		}
	}

	#endregion Methods

	#region Properties

	public virtual int currentFrame {
		get {
			return m_CurrentFrame;
		}
	}
	
	public virtual int numFrames {
		get {
			return m_NumFrames;
		}
	}

	#endregion Properties

}

#if UNITY_EDITOR

[CustomEditor(typeof(TransformRecorder))]
public class TransformRecorderEditor:Editor{

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		TransformRecorder target_=target as TransformRecorder;
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Record")) {
			}
			if(GUILayout.Button("Play")) {
			}
			if(GUILayout.Button("Stop")) {
			}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Slider(target_.currentFrame,0,target_.numFrames);
	}
}

#endif