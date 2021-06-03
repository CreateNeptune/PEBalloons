using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    private GameManager gm;
    private Rigidbody rb;
    
    public float speed;
    public float selfDestructTime;

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
        if (speed > 0f)
        {
            StartCoroutine(BeginSelfDestruct());

            // Added Direction to moveVector.
            Vector3 moveVector = new Vector3(0f, transform.eulerAngles.z, 0f) * Time.deltaTime * speed;

            rb.MovePosition(rb.position + moveVector);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Balloon"))
        {
            // Increment score.
            Score(collision.collider);

            // Decrement arrows.
            gm.DecrementArrows();
        }
    }

    private void Score(Collider balloon)
    {
        gm.CalculateScore(balloon);

        gameObject.SetActive(false);
        balloon.gameObject.SetActive(false);
    }

    private IEnumerator BeginSelfDestruct()
    {
        yield return new WaitForSeconds(selfDestructTime);
        gm.DecrementArrows();

        gameObject.SetActive(false);
    }
}
