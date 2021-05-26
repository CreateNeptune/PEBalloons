using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private Material[] balloonMaterials;

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
    }

    private void Start()
    {
        // Start with first arrow.
        ReloadArrow();

        StartCoroutine(GenerateBalloons());
        StartCoroutine(BeginGame());
    }

    private IEnumerator BeginGame()
    {
        yield return new WaitForSeconds(1f);

        gameState = GameState.gameplay;
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
                float zOffset = minZOffset + (maxZOffset - minZOffset) / (maxPE - minPE) * randomPE;

                GameObject newBalloon = CNExtensions.GetPooledObject(balloons, balloon, defaultLayer, balloonGeneratorT,
                    new Vector3(0f, 0f, zOffset), Quaternion.identity, false);
                newBalloon.transform.GetChild(0).GetComponent<MeshRenderer>().material = balloonMaterials[Random.Range(0, balloonMaterials.Length)];
            }

            yield return new WaitForSeconds(timeBetweenBalloons);
        }
    }

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
