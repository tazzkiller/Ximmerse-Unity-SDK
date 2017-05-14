//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Ximmerse.UI;

public class UIVibrateOnHover : MonoBehaviour, IPointerEnterHandler
{
    public float vibrationTime = .1f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        
         VRInputModule.TriggerVibration(eventData, vibrationTime);
    }
}
