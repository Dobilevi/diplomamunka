using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using Cpp;

public class MultiMovement : MonoBehaviour
{
    Rigidbody2D rigidbody2D;

    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.centerOfMass = Vector2.zero;
    }

    public void MoveToPoint(Vector2 point, float angle)
    {
        rigidbody2D.velocity = (point - (Vector2)transform.position) / NetConstants.clientUpdateInterval * 1000;
        rigidbody2D.angularVelocity = Mathf.DeltaAngle(transform.rotation.eulerAngles.z,  angle) / NetConstants.clientUpdateInterval * 1000;
    }

    public void Stop()
    {
        rigidbody2D.velocity = Vector2.zero;
        rigidbody2D.angularVelocity = 0;
    }
}
