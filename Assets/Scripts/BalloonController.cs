using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonController : MonoBehaviour
{
    private Rigidbody rb;
    private Transform t;

    public int priceToEarnings;
    public float xSpeed;
    [SerializeField] private float ySpeed;
    private float yDirection;
    private float minY = 4f;
    private float maxY = 6f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        t = transform;
    }

    private void OnEnable()
    {
        StartCoroutine(Float());
    }

    private IEnumerator Float()
    {
        float bounceTime = Random.Range(1f, 5f);

        for (; ;)
        {
            // Current direction of floating
            yDirection = Random.Range(0, 2) * 2 - 1;

            yield return new WaitForSeconds(bounceTime);
        }
    }

    private void FixedUpdate()
    {
        Vector3 moveVector;

        if (yDirection == 1 && t.position.y < maxY)
            moveVector = new Vector3(xSpeed, ySpeed, 0f) * Time.deltaTime;
        else if (yDirection == -1 && t.position.y > minY)
            moveVector = new Vector3(xSpeed, -ySpeed, 0f) * Time.deltaTime;
        else
            moveVector = new Vector3(xSpeed, 0f, 0f) * Time.deltaTime;

        rb.MovePosition(rb.position + moveVector);
    }
}
