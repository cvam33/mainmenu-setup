using UnityEngine;
using Unity.Cinemachine;

public class CameraController2D : MonoBehaviour
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
        }
    }
}