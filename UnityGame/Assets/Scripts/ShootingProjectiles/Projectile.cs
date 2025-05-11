using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to make projectiles move
/// </summary>
public class Projectile : MonoBehaviour
{
    [Tooltip("The distance this projectile will move each second.")]
    public float projectileSpeed = 3.0f;

    private void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(transform.up * projectileSpeed);
    }
}