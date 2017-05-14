using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Ximmerse.InputSystem;

public class RawDataUIManager : MonoBehaviour {

    

    private int m_trackingCameraHandle;
    private int m_leftControllerHandle;
    private int m_rightControllerHandle;

    private XDevicePlugin.ControllerState m_leftControllerState;
    private XDevicePlugin.ControllerState m_rightControllerState;
    private XDevicePlugin.TrackerState m_trackingCameraState;

    private void Awake()
    {
        XDevicePlugin.Init();
    }

    private void Start()
    {
        m_trackingCameraHandle = XDevicePlugin.GetInputDeviceHandle("XHawk-0");
        m_leftControllerHandle = XDevicePlugin.GetInputDeviceHandle("XCobra-0");
        m_rightControllerHandle = XDevicePlugin.GetInputDeviceHandle("XCobra-1");
    }

    private void Update()
    {
        XDevicePlugin.UpdateInputState(m_trackingCameraHandle);
        XDevicePlugin.UpdateInputState(m_leftControllerHandle);
        XDevicePlugin.UpdateInputState(m_rightControllerHandle);

        UpdateLeftController();
    }

    void UpdateLeftController()
    {
        XDevicePlugin.GetInputState(m_leftControllerHandle, ref m_leftControllerState);
    }

    void UpdateRightController()
    {
        XDevicePlugin.GetInputState(m_rightControllerHandle, ref m_rightControllerState);
    }
    
    void UpdateTrackingCameraState()
    {
        XDevicePlugin.GetInputState(m_trackingCameraHandle, ref m_trackingCameraState);
    }
}
