using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransformTest : NetworkBehaviour
{
    void Update()
    {
        if (IsServer)
        {
            float theta = Time.frameCount / 10.0f;

            transform.position = new Vector3(
                x: (float)Math.Cos(theta),
                y: 0.0f,
                z: (float)Math.Sin(theta));
        }
    }
}
