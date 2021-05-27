using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    private GameManager gm;
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

    private void Start()
    {
        gm = GameObject.FindWithTag("gm").GetComponent<GameManager>();
    }

    private void FixedUpdate()
    {
        Vector3 moveVector = new Vector3(0f, 0f, speed) * Time.deltaTime;

        rb.MovePosition(rb.position + moveVector);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Balloon"))
        {
            // Increment score.
            Score(collision.collider);
        }
    }

    private void Score(Collider balloon)
    {
        gm.CalculateScore(balloon);

        gameObject.SetActive(false);
        balloon.gameObject.SetActive(false);
    }
}
