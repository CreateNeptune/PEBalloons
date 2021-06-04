using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowDestroyer : MonoBehaviour
{
    private GameManager gm;

    private void Awake()
    {
        gm = GameObject.FindWithTag("gm").GetComponent<GameManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Arrow"))
        {
            gm.DecrementArrows();

            collision.gameObject.SetActive(false);
        }
    }
}
