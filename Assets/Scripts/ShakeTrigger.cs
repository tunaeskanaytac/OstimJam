using Unity.Cinemachine;
using UnityEngine;

public class ShakeTrigger : MonoBehaviour
{
    public CinemachineImpulseSource impulseSource;

    public void TriggerShake()
    {
        impulseSource.GenerateImpulse();
    }
}