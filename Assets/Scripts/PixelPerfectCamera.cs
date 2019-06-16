using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPerfectCamera : MonoBehaviour
{
    public static float pixelToUnits = 1.0f;
    public static float scale = 1.0f;

    public static Vector2 nativeResolition = new Vector2(1920, 1080);

    private void Awake()
    {
        var camera = GetComponent<Camera>();

        if (camera.orthographic)
        {
            float xScalar = Screen.width / nativeResolition.x;
            float yScalar = Screen.height / nativeResolition.y;
            pixelToUnits *= yScalar > xScalar ? xScalar : yScalar;
            camera.orthographicSize = (Screen.height / 2.0f) / pixelToUnits;
        }
    }
}
