using UnityEngine;
using Unity.Cinemachine;

public class CameraController3D : MonoBehaviour
{
    private CinemachineCamera _vcam;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineCamera>();
    }

    public void SetTarget(Transform target)
    {
        if (_vcam != null)
        {
            _vcam.Follow = target;
            _vcam.LookAt = target;
        }
    }
}