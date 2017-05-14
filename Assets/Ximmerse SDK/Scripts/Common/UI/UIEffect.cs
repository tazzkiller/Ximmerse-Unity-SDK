using UnityEngine;
using UnityEngine.EventSystems;

namespace Ximmerse.UI{

	/// <summary>
	/// A class help you handling ui effects more easily.
	/// </summary>
	public class UIEffect:MonoBehaviour,
		IPointerEnterHandler,
		IPointerExitHandler,
		IPointerClickHandler
	{

		#region Nested Types

		[System.Serializable]
		public class TweenSetting{
			public float durationIn=1.0f;
			public float durationOut=1.0f;
			public System.Action<float> onUpdate;

			[System.NonSerialized]protected bool m_IsForward;
			[System.NonSerialized]protected float m_Value;

			public void Awake(System.Action<float> action) {
				//
				m_IsForward=false;
				m_Value=0.0f;
				//
				onUpdate=action;
			}

			public bool Update() {
				if(m_IsForward) {
					if(m_Value<1.0f) {
						onUpdate(m_Value=Mathf.Clamp01(m_Value+Time.deltaTime/durationIn));
						return true;
					}
				}else {
					if(m_Value>0.0f) {
						onUpdate(m_Value=Mathf.Clamp01(m_Value-Time.deltaTime/durationOut));
						return true;
					}
				}
				return false;
			}

			public void Play(bool isForward) {
				m_IsForward=isForward;
			}
		}

		#endregion Nested Types

		#region Fields

		[SerializeField]protected TweenSetting m_Visible;
		[SerializeField]protected TweenSetting m_Highlight;

		public TweenSetting highlight{
			get{
				return m_Highlight;
			}
		}

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			m_Visible.Awake(OnVisible);
			m_Highlight.Awake(OnHighlight);
		}

		protected virtual void Start() {
			m_Visible.Play(true);
		}

		protected virtual void Update() {
			if(!m_Visible.Update()) {
				m_Highlight.Update();
			}
		}

		public virtual void OnPointerEnter(PointerEventData eventData) {
			m_Highlight.Play(true);
		}

		public virtual void OnPointerExit(PointerEventData eventData) {
			m_Highlight.Play(false);
		}

		public virtual void OnPointerClick(PointerEventData eventData) {
		}

		#endregion Unity Messages

		#region Methods

		// <!-- Handle your ui effects by overiding those methods.

		public virtual void OnVisible(float value) {
		}

		public virtual void OnHighlight(float value) {
		}

		// Handle your ui effects by overiding those methods.-->

		#endregion Methods

	}

}