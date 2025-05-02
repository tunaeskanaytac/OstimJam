using UnityEngine;

public static class Utils
{
    public static int LayerMaskToLayerIndex(LayerMask layerMask)
    {
        return (int)Mathf.Log(layerMask.value, 2);
    }
}