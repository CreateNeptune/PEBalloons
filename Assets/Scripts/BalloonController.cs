using UnityEngine;
using TMPro;

public class BalloonController : MonoBehaviour
{
    private Rigidbody rb;

    public int priceToEarnings;
    public float ySpeed;

    public TextMeshPro stockNameTMP;
    public TextMeshPro peText;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 moveVector;
        
        // Move along Y axis.
        moveVector = new Vector3(0f, ySpeed, 0f) * Time.deltaTime;
        rb.MovePosition(rb.position + moveVector);
    }
}
