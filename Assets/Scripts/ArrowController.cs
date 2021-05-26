using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    private Rigidbody rb;
    public float speed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        speed = 0f;
    }

    private void FixedUpdate()
    {
        Vector3 moveVector = new Vector3(0f, 0f, speed) * Time.deltaTime;

        rb.MovePosition(rb.position + moveVector);
    }
}
