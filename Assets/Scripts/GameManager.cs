using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CreateNeptune;

public class GameManager : MonoBehaviour
{
    // Controls
    [SerializeField] private bool phoneTesting;
    private bool playerTouching;
    private GameObject activeArrow;
    [SerializeField] private Transform arrowGeneratorT;
    private bool arrowAvailable = true;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private float reloadTime;

    public int totalBalloons;
    [SerializeField] private GameObject balloon;
    private List<GameObject> balloons = new List<GameObject>();
    [SerializeField] private GameObject arrow;
    private List<GameObject> arrows = new List<GameObject>();
    private LayerMask defaultLayer;

    // Balloon generation
    [SerializeField] private float timeBetweenBalloons;
    [SerializeField] private Transform balloonGeneratorT;
    [SerializeField] private float minZOffset;
    [SerializeField] private float maxZOffset;
    [SerializeField] private float minPE;
    [SerializeField] private float maxPE;
    [SerializeField] private float minBalloonSpeed;
    [SerializeField] private float maxBalloonSpeed;
    [SerializeField] private Material[] balloonMaterials;

    // Scoring
    [SerializeField] private int peScoreMultiplier;
    private int score;
    [SerializeField] private GameObject scoreText;
    [SerializeField] private GameObject dollarBillExplosion;
    private List<GameObject> dollarBillExplosions = new List<GameObject>();
    [SerializeField] private Canvas encouragingCanvas;
    [SerializeField] private Text encouragingText;
    [SerializeField] private int gameOverScore;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Canvas gameOverCanvas;

    // Audio
    [SerializeField] private AudioSource camAudio;
    [SerializeField] private AudioClip arrowShot;

    // Game states
    GameState gameState = GameState.pregame;

    public enum GameState
    {
        pregame, gameplay, endgame
    }

    private void Awake()
    {
        defaultLayer = LayerMask.NameToLayer("Default");

        CNExtensions.CreateObjectPool(balloons, balloon, 3, defaultLayer);
        CNExtensions.CreateObjectPool(arrows, arrow, 10, defaultLayer);
        CNExtensions.CreateObjectPool(dollarBillExplosions, dollarBillExplosion, 3, defaultLayer);
    }

    private void Start()
    {
        // Start with first arrow.
        ReloadArrow();

        // Reset score.
        scoreText.GetComponent<Text>().text = "$0";

        StartCoroutine(GenerateBalloons());
        StartCoroutine(BeginGame());
    }

    private IEnumerator BeginGame()
    {
        yield return new WaitForSeconds(1f);

        gameState = GameState.gameplay;

        StartCoroutine(CheckForGameOver());
    }

    private IEnumerator CheckForGameOver()
    {
        for (; ;)
        {
            yield return new WaitForSeconds(0.5f);

            if (score >= gameOverScore)
            {
                gameState = GameState.endgame;
                gameOverText.text = "You earned $" + score + "!";
                gameOverCanvas.enabled = true;
                yield break;
            }
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(0);
    }

    private IEnumerator GenerateBalloons()
    {
        for (; ;)
        {
            if (gameState == GameState.endgame)
                yield break;
            else if (gameState == GameState.gameplay)
            {
                // Set P/E on balloon, which will determine its distance from the arrow.
                float randomPE = Random.Range(minPE, maxPE);
                float zOffset = maxZOffset + (minZOffset - maxZOffset) / (maxPE - minPE) * randomPE;

                GameObject newBalloon = CNExtensions.GetPooledObject(balloons, balloon, defaultLayer, balloonGeneratorT,
                    new Vector3(0f, 0f, zOffset), Quaternion.identity, false);

                // Set random skin.
                newBalloon.transform.GetChild(0).GetComponent<MeshRenderer>().material = balloonMaterials[Random.Range(0, balloonMaterials.Length)];

                // Set P/E on balloon.
                BalloonController bc = newBalloon.GetComponent<BalloonController>();
                bc.priceToEarnings = (int)randomPE;

                // Set speed on balloon based on P/E.
                bc.xSpeed = maxBalloonSpeed + (minBalloonSpeed - maxBalloonSpeed) / (maxPE - minPE) * randomPE;

                // Set balloon text based on P/E.
                bc.peText.text = ((int)randomPE).ToString() + "x P/E";

                // Set balloon stock name.
                bc.stockNameTMP.text = GetStockName(randomPE);
            }

            yield return new WaitForSeconds(timeBetweenBalloons);
        }
    }

    private string GetStockName(float priceToEarnings)
    {
        int bestValue = (int)(minPE + ((minPE + maxPE) * 0.5f - minPE) / 3f);
        int niceValue = (int)(minPE + ((minPE + maxPE) * 0.5f - minPE) * 2f / 3f);
        int decentValue = (int)((minPE + maxPE) * 0.5f);

        if (priceToEarnings < bestValue)
            return bestValueStocks[Random.Range(0, bestValueStocks.Length)];
        else if (priceToEarnings < niceValue)
            return niceValueStocks[Random.Range(0, niceValueStocks.Length)];
        else if (priceToEarnings < decentValue)
            return decentValueStocks[Random.Range(0, decentValueStocks.Length)];

        return otherStocks[Random.Range(0, otherStocks.Length)];
    }

    private string[] bestValueStocks =
    {
        "MESA", "GTN", "SKYW", "UFS", "RRD", "SFL", "CHTR"
    };

    private string[] niceValueStocks =
    {
        "MARA", "JD", "DAL", "AAL", "UAL", "SPG", "BXP", "EQR", "AZO", "BA", "C"
    };

    private string[] decentValueStocks =
    {
        "FB", "MSFT", "L", "CCIV", "RDFN", "AMD", "NVDA", "MMM", "GOOG", "AXP", "CMG"
    };

    private string[] otherStocks =
    {
        "QS", "CRWD", "PTON", "BOWX", "U", "W", "CPNG", "TSLA", "ATVI", "AMZN", "KMX", "DISCA"
    };

    private void Update()
    {
        if (gameState == GameState.gameplay)
            GetInput();
    }

    private void FixedUpdate()
    {
        // TODO: If the arrow is available and player touches, fire and make the arrow immediately unavailable until it can reload.
        if (playerTouching && arrowAvailable)
        {
            print("fire");
            playerTouching = false;
            arrowAvailable = false;

            FireArrow();
        }
    }

    private void FireArrow()
    {
        activeArrow.GetComponent<ArrowController>().speed = arrowSpeed;

        // Make arrow sound.
        camAudio.PlayOneShot(arrowShot);

        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        yield return new WaitForSeconds(reloadTime);

        // Reload the arrow and make arrow available to fire again.
        ReloadArrow();
        arrowAvailable = true;

        print("arrow available");
    }

    private void ReloadArrow()
    {
        GameObject newArrow = CNExtensions.GetPooledObject(arrows, arrow, defaultLayer, arrowGeneratorT, Vector3.zero, Quaternion.identity, false);
        activeArrow = newArrow;
    }

    public void CalculateScore(Collider balloon)
    {
        int priceToEarnings = balloon.GetComponent<BalloonController>().priceToEarnings;
        int startScore = score;

        score += ((int)maxPE - priceToEarnings) * peScoreMultiplier;

        StartCoroutine(MPAction.CountUpObject(scoreText, startScore, score, 1f, "$", "", false, false, false));

        // Create explosion where hit takes place.
        StartCoroutine(ExplodeAndDisable(balloon.transform));

        // Encourage the user with encouraging statements.
        StartCoroutine(EncourageUser(priceToEarnings));
    }

    private IEnumerator EncourageUser(int priceToEarnings)
    {
        int bestValue = (int)(minPE + ((minPE + maxPE) * 0.5f - minPE) / 3f);
        int niceValue = (int)(minPE + ((minPE + maxPE) * 0.5f - minPE) * 2f / 3f);
        int decentValue = (int)((minPE + maxPE) * 0.5f);

        if (priceToEarnings < bestValue)
            encouragingText.text = "Great value!";
        else if (priceToEarnings < niceValue)
            encouragingText.text = "Nice value!";
        else if (priceToEarnings < decentValue)
            encouragingText.text = "Decent value";

        if (priceToEarnings < decentValue)
            encouragingCanvas.enabled = true;

        yield return new WaitForSeconds(3f);

        encouragingCanvas.enabled = false;
    }

    private IEnumerator ExplodeAndDisable(Transform balloonT)
    {
        GameObject explosion = CNExtensions.GetPooledObject(dollarBillExplosions,
            dollarBillExplosion, defaultLayer, balloonT, new Vector3(0f, 2f, 0f), Quaternion.identity, false);

        explosion.GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(5f);

        explosion.SetActive(false);
    }

    private void GetInput()
    {
#if UNITY_EDITOR
        if (phoneTesting)
            GetTouchInput();
        else
            playerTouching = Input.anyKey;
#else
        GetTouchInput();
#endif
    }

    private void GetTouchInput()
    {
        if (Input.touchCount > 0)
        {
            // Get the first touch
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    playerTouching = true;
                    break;
                case TouchPhase.Moved:
                    break;
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Canceled:
                    break;
                case TouchPhase.Ended:
                    break;
            }
        }
    }
}
