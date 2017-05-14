using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ximmerse.UI{

	public class UILevelButton:UIEffect{

		#region Fields
		
		[Header("Common")]
		[System.NonSerialized]protected UILevelManager.UILevel m_Level;
		[SerializeField]protected Transform m_TypeRoot;
		[SerializeField]protected Text m_Label;

		[Header("Front Icon")]
		[SerializeField]protected Image m_FrontIcon;
		[SerializeField]protected Transform m_FrontIconDummy;
		[SerializeField]protected float m_FrontIconDepth=10;
		

		[Header("Back Icon")]
		[SerializeField]protected Image m_BackIcon;
		[SerializeField]protected Transform m_BackIconDummy;
		[SerializeField]protected float m_BackIconDepth=5;

		[System.NonSerialized]protected Vector3 m_FrontIconDummyOri,m_BackIconDummyOri;
		[System.NonSerialized]protected Transform m_Transform;

		#endregion Fields

		#region Unity Messages

		protected override void Awake() {
			base.Awake();
			//
			m_Transform=transform;
			if(m_FrontIconDummy!=null) {
				m_FrontIconDummyOri=m_FrontIconDummy.localPosition;
			}
			if(m_BackIconDummy!=null) {
				m_BackIconDummyOri=m_BackIconDummy.localPosition;
			}
		}

		public override void OnPointerClick(PointerEventData eventData) {
			base.OnPointerClick(eventData);
			if(m_Level!=null) {
				m_Level.LoadLevel();
			}
		}

		#endregion Unity Messages

		#region Methods

		public override void OnVisible(float value) {
			m_Transform.localScale=Vector3.one*value;
		}

		public override void OnHighlight(float value) {
			if(m_FrontIconDummy!=null) {
				m_FrontIconDummy.localPosition=Vector3.Lerp(m_FrontIconDummyOri,m_FrontIconDummyOri+Vector3.back*m_FrontIconDepth,value);
			}
			if(m_BackIconDummy!=null) {
				m_BackIconDummy.localPosition=Vector3.Lerp(m_BackIconDummyOri,m_BackIconDummyOri+Vector3.back*m_BackIconDepth,value);
			}
		}

		public virtual void LoadUILevel(UILevelManager.UILevel level) {
			m_Level=level;
			//
			if(m_Label!=null) {
				m_Label.text=m_Level.name;
			}
			if(level is UILevelManager.ExternalLevel) {
				UILevelManager.ExternalLevel exLevel=(level as UILevelManager.ExternalLevel);
				exLevel.LoadSprite(m_FrontIcon,false);
				exLevel.LoadSprite(m_BackIcon,true);
				//
				if(m_TypeRoot!=null) {
					Transform t=m_TypeRoot.Find(exLevel.vrPlatform.ToString());
					if(t!=null) {
						t.gameObject.SetActive(true);
					}
				}
			}else {
				if(m_FrontIcon!=null) {
					m_FrontIcon.sprite=m_Level.frontIcon;
				}
				if(m_BackIcon!=null) {
					m_BackIcon.sprite=m_Level.backIcon;
				}
				//
				if(m_TypeRoot!=null) {
					Transform t=m_TypeRoot.Find("Unity");
					if(t!=null) {
						t.gameObject.SetActive(true);
					}
				}
			}
		}

		#endregion Methods

	}

}