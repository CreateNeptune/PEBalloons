using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
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
            // Increment score and start explosion flow.
            StartCoroutine(Score(collision.collider));
        }
    }

    private IEnumerator Score(Collider balloon)
    {
        // Delay by a Second to start the chain reaction.
        yield return new WaitForSeconds(1f);

        gm.CalculateScore(balloon);

        balloon.gameObject.SetActive(false);
    }
}
