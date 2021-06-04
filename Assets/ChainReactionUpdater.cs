using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChainReactionUpdater : MonoBehaviour
{
    private GameManager gm;
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    private void Start()
    {
        gm = GameObject.FindWithTag("gm").GetComponent<GameManager>();

        StartCoroutine(CheckForHigherScore());
    }

    private IEnumerator CheckForHigherScore()
    {
        for (; ;)
        {
            if (gm.gameState == GameManager.GameState.endgame)
            {
                yield return new WaitForSeconds(0.1f);

                text.text = "You earned $" + gm.score + "!";
            }
            else
            {
                yield return null;
            }
        }
    }
}
