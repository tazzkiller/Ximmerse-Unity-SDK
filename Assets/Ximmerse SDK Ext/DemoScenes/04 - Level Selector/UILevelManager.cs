using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ximmerse.UI {

	/// <summary>
	/// A helper class for managing game levels.
	/// </summary>
	public class UILevelManager:MonoBehaviour {

		#region Nested Types

		/// <summary>
		/// Class for describing an new level(scene,or application).
		/// </summary>
		public class UILevel {
			public string name;
			public Sprite frontIcon;
			public Sprite backIcon;
			public string description;

			public virtual void LoadLevel(){
			}
		}

		/// <summary>
		/// Class for loading other level in the same Unity application.
		/// </summary>
		[System.Serializable]
		public class InternalLevel:UILevel {
			public string levelName;
			public int levelIndex;

			public override void LoadLevel(){
				if(string.IsNullOrEmpty(levelName)) {
					Application.LoadLevel(levelIndex);
				}else {
					Application.LoadLevel(levelName);
				}
			}
		}

		/// <summary>
		/// <para>NOTE:It only works on Android.</para>
		/// Class for launching other vr app.It is very useful 
		/// if you use UILevelManager as a VR Launcher(or VR Home).
		/// </summary>
		public class ExternalLevel:UILevel {

			protected static Dictionary<string,Sprite> s_SpritePool=new Dictionary<string,Sprite>();
			protected AndroidJavaObject m_ExternalPtr;

			public ExternalLevel(AndroidJavaObject appInfo) {
				m_ExternalPtr=appInfo;
				//
				name=m_ExternalPtr.Get<string>("mName");
			}

			public override void LoadLevel(){
				m_ExternalPtr.Call("startActivity");
			}

			public virtual void LoadSprite(Image image,bool isBackground) {
				Sprite sprite=null;
				if(sprite==null) {
					if(s_SpritePool.TryGetValue(m_ExternalPtr.Get<string>("mPackageName")+(isBackground?"@BackIcon":"@FrontIcon"),out sprite)){
						if(isBackground) {
							backIcon=sprite;
						}else {
							frontIcon=sprite;
						}
					}
				}
				if(sprite==null) {
					string path=isBackground?m_ExternalPtr.Call<string>("getBackIconPath"):m_ExternalPtr.Call<string>("getFrontIconPath");
					image.StartCoroutine(LoadSprite(path,(x,y)=>{
						image.sprite=y;
						s_SpritePool.Add(m_ExternalPtr.Get<string>("mPackageName")+(isBackground?"@BackIcon":"@FrontIcon"),y);
						//
						if(isBackground) {
							backIcon=y;
						}else {
							frontIcon=y;
						}
					}));
				}else {
					image.sprite=sprite;
				}
			}

			public virtual System.Collections.IEnumerator LoadSprite(string path,System.Action<string,Sprite> onSpriteLoaded) {
				//
				if(string.IsNullOrEmpty(path)) {
					yield break;
				}
				//
				path="file://"+Application.persistentDataPath+"/"+path;
				Log.i("",path);
				WWW www=new WWW(path);
				yield return www;
				if(www.isDone){
					Log.i("",www.error);
					Texture2D texture=www.texture;
					Sprite sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f);
					onSpriteLoaded.Invoke(path,sprite);
				}
			}

			public virtual VrPlatform vrPlatform {
				get {
					return (VrPlatform)m_ExternalPtr.Get<int>("mVrPlatform");
				}
			}
		}

		[System.Flags]
		public enum VrPlatform {
			Cardboard =1<<0,
			GearVR    =1<<1,
			Daydream  =1<<2,
		}

#if UNITY_EDITOR
		[UnityEditor.CustomPropertyDrawer(typeof(VrPlatform))]
		public class VrPlatformDrawer:UnityEditor.PropertyDrawer {
			public override void OnGUI(Rect position,UnityEditor.SerializedProperty property,GUIContent label) {
				property.intValue=UnityEditor.EditorGUI.MaskField(position,label,property.intValue,property.enumNames);
			}
		}
#endif

		#endregion Nested Types

		#region Fields

		[SerializeField]protected Transform m_ButtonRoot;
		[SerializeField]protected UILevelButton m_ButtonPrefab;
		[SerializeField]protected int m_LevelsPerPage=15;
		[SerializeField]protected InternalLevel[] m_Levels;
		[System.NonSerialized]public List<UILevel> levels;

		[Header("Android")]
		public VrPlatform vrAppMask;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			levels=new List<UILevel>(m_Levels);
#if !UNITY_EDITOR && UNITY_ANDROID
			BuildExternalLevels();
#endif
			BuildUIButtons();
		}

		#endregion Unity Messages

		#region Methods

		protected virtual void BuildExternalLevels() {
#if UNITY_ANDROID
			using(AndroidJavaClass jc=new AndroidJavaClass("com.unity3d.player.UnityPlayer")){
			using(AndroidJavaObject currentActivity=jc.GetStatic<AndroidJavaObject>("currentActivity")){
				using(AndroidJavaObject appManager=new AndroidJavaObject("com/ximmerse/os/AppManager",currentActivity)){
					using(AndroidJavaObject list=appManager.Call<AndroidJavaObject>("getVrAppInfoList",(int)vrAppMask)) {
						int i=0,imax=list.Call<int>("size");
						Log.i("TEST",imax.ToString());
						for(;i<imax;++i) {
							try {
								AndroidJavaObject appInfo=list.Call<AndroidJavaObject>("get",i);
								levels.Add(new ExternalLevel(appInfo));
							}catch {
									return;
							}
						}
					}
				}
			}}
#endif
		}

		protected virtual void BuildUIButtons() {
			Transform t=m_ButtonPrefab.transform,newT;
			UILevelButton button;
			for(int i=0,imax=levels.Count;i<imax;++i) {
				button=Object.Instantiate(m_ButtonPrefab);
				//
				button.LoadUILevel(levels[i]);
				//
				newT=button.transform;
				newT.SetParent(m_ButtonRoot);
				newT.localPosition=t.localPosition;
				newT.localRotation=t.localRotation;
				newT.localScale=t.localScale;
			}
		}

		#endregion Methods

	}

}