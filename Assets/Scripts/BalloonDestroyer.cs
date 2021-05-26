using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonDestroyer : MonoBehaviour
{
    private GameManager gm;

    private void Awake()
    {
        gm = GameObject.FindWithTag("gm").GetComponent<GameManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Balloon"))
        {
            gm.totalBalloons++;

            collision.gameObject.SetActive(false);
        }
    }
}
