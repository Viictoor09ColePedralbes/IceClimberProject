using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public int initialMountain = 1, gainedPoints = 0;
    [HideInInspector] public bool areSecondPlayer = false, gameStarted = false, isOnBonusStage = false, stageFinished = false;
    private bool hasEnterOnBS = true;
    private static GameManager gameManager;
    public static GameManager instance
    {
        get
        {
            return RequestGameManager();
        }
    }
    private int actualMountain;
    public int actualScore, mountainsCleared = 0;
    private int highScore, actualScene;
    private CameraUpScript[] camerasUP = new CameraUpScript[15];
    [SerializeField] private TMP_Text timeBonusStage_down, timeBonusStage_up;
    private float timeBonusStage = 40.0f;
    private PlayerMovement playerMovement;
    private Camera playerCamera;
    private Transform playerTransform;

    [SerializeField] private GameObject[] mountainsDestruibleBlocks = new GameObject[32], mountainsNotDestruibleBlocks = new GameObject[32], mountainsExtras = new GameObject[32];
    private List<GameObject> enemiesInScene = new List<GameObject>();
    private GameObject parentMountains, parentExtras, actualDestruible, actualNotDestruible, actualExtras;
    private CanvasGroup blackPanel;
    [HideInInspector] public int[] thingsPoints = new int[4]; // 0 = vegetables, 1 = ice, 2 = birds, 3 = blocks
    public GameObject[] vegetablesPrefabs = new GameObject[10];
    private GameObject[] vegetablesInScene = new GameObject[4];
    private Transform[] vegetablesScenePosition = new Transform[4];

    // Bird spawn times
    [SerializeField] private GameObject[] enemiesPrefabs = new GameObject[3]; // 0 = yeti, 1 = oso, 2 = pajaro
    private const float MIN_BIRD_SPAWN_TIME = 10f, MAX_BIRD_SPAWN_TIME = 15f, MAX_BIRD_X = 8.2f, BIRD_Y = 4.3f;
    private bool birdAlive = false, birdSpawning = false;
    

    void Awake()
    {
        if (gameManager)
        {
            Destroy(gameObject);
        }
        RequestGameManager();
        /* BORRAR LO DE ABAJO DESPUES, SOLO PARA TEST
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        GetAllCamerasUp();
        timeBonusStage_down = GameObject.FindGameObjectWithTag("bs_time_down").GetComponent<TMP_Text>();
        timeBonusStage_up = GameObject.FindGameObjectWithTag("bs_time_up").GetComponent<TMP_Text>();
        blackPanel = GameObject.FindGameObjectWithTag("blackPanel").GetComponent<CanvasGroup>();
        actualMountain = 1;

        parentMountains = GameObject.FindGameObjectWithTag("ParentMountains");
        parentExtras = GameObject.FindGameObjectWithTag("ParentExtras");
        InstantiateVegetables();
        // BORRAR LO DE ARRIBA DESPUES, SOLO PARA TEST*/
    }

    void Update()
    {
        if (isOnBonusStage)
        {
            if(hasEnterOnBS)
            {
                DestroyAllEnemies();
                playerMovement.animator.SetBool("hasHammer", false);
                timeBonusStage = 40.0f;
                hasEnterOnBS = false;
            }
            else
            {
                if (timeBonusStage > 0 && !stageFinished)
                {
                    timeBonusStage -= Time.deltaTime;
                    timeBonusStage_down.text = timeBonusStage.ToString("00.0");
                    timeBonusStage_up.text = timeBonusStage.ToString("00.0");
                }
                else if (timeBonusStage < 0 || stageFinished)
                {
                    playerMovement.animator.SetBool("hasHammer", true);
                    playerMovement.FreezingControl(true);
                    blackPanel.alpha = 1;
                    PointsGainedScript.instance.showingPoints = true; // Para mostrar los puntos obtenidos y ver como se suman a los ya acumulados
                    isOnBonusStage = false;
                }
            }
        }
        else
        {
            if (!birdAlive && !birdSpawning)
            {
                StartCoroutine(BirdSpawn());
            }
            
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        actualScene = scene.buildIndex;
        actualMountain = initialMountain;
        if(scene.buildIndex == 1)
        {
            playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            GetAllCamerasUp();
            timeBonusStage_down = GameObject.FindGameObjectWithTag("bs_time_down").GetComponent<TMP_Text>();
            timeBonusStage_up = GameObject.FindGameObjectWithTag("bs_time_up").GetComponent<TMP_Text>();
            blackPanel = GameObject.FindGameObjectWithTag("blackPanel").GetComponent<CanvasGroup>();

            parentMountains = GameObject.FindGameObjectWithTag("ParentMountains");
            parentExtras = GameObject.FindGameObjectWithTag("ParentExtras");
            
            /*actualDestruible = Instantiate(mountainsDestruibleBlocks[initialMountain], Vector2.zero, Quaternion.identity, parentMountains.transform);
            actualNotDestruible = Instantiate(mountainsNotDestruibleBlocks[initialMountain], Vector2.zero, Quaternion.identity, parentMountains.transform);
            actualExtras = Instantiate(mountainsExtras[initialMountain], Vector2.zero, Quaternion.identity, parentExtras.transform);*/
            InstantiateVegetables();
        }
    }

    public void ChangeMountain()
    {
        DeleteVegetables();
        playerCamera.transform.position = new Vector3(0, 0, -10);
        playerTransform.position = new Vector2(0, -2.53f);
        ReactivateCamerasUp();
        hasEnterOnBS = true;
        stageFinished = false;

        for(int i = 0; i < thingsPoints.Length; i++)
        {
            thingsPoints[i] = 0;
        }

        actualMountain++;
        if (actualDestruible)
            Destroy(actualDestruible);
        if(actualNotDestruible)
            Destroy(actualNotDestruible);
        if(actualExtras)
            Destroy(actualExtras);

        /*actualDestruible = Instantiate(mountainsDestruibleBlocks[actualMountain], Vector2.zero, Quaternion.identity, parentMountains.transform);
        actualNotDestruible = Instantiate(mountainsNotDestruibleBlocks[actualMountain], Vector2.zero, Quaternion.identity, parentMountains.transform);
        actualExtras = Instantiate(mountainsExtras[actualMountain], Vector2.zero, Quaternion.identity, parentExtras.transform);*/
        InstantiateVegetables();
        blackPanel.alpha = 0;
        playerMovement.FreezingControl(false);
    }

    private void GetAllCamerasUp()
    {
        GameObject[] cameras = GameObject.FindGameObjectsWithTag("CameraUp");
        Debug.Log(cameras.Length + " " + camerasUP.Length);
        for (int i = 0; i < camerasUP.Length; i++)
        {
            cameras[i].tag = "Untagged";
            camerasUP[i] = cameras[i].GetComponent<CameraUpScript>();
        }
    }

    private void ReactivateCamerasUp()
    {
        foreach(CameraUpScript cameraUp in camerasUP)
        {
            cameraUp.boxCollider.enabled = true;
        }
    }

    private static GameManager RequestGameManager()
    {
        if (!gameManager)
        {
            gameManager = FindObjectOfType<GameManager>();
            DontDestroyOnLoad(gameManager);
        }
        return gameManager;
    }

    private void InstantiateVegetables()
    {
        int index = (actualMountain - 1) % vegetablesPrefabs.Length;
        GameObject vegetableToUse = vegetablesPrefabs[index];
        vegetablesScenePosition = GameObject.FindGameObjectWithTag("vegetables_position").GetComponentsInChildren<Transform>();

        int i = 0;
        foreach(Transform vegTransform in vegetablesScenePosition)
        {
            if(i > 0)
            {
                vegetablesInScene[i-1] = Instantiate(vegetableToUse, vegTransform.position, Quaternion.identity);
            }
            i++;
        }

        PointsGainedScript.instance.actualVegetableSprite = vegetableToUse.GetComponent<VegetableScript>().GetVegetable().vegetable;
        PointsGainedScript.instance.pointsVegetables = vegetableToUse.GetComponent<VegetableScript>().GetVegetable().points;
    }
    
    private void DeleteVegetables()
    {
        foreach(GameObject i in vegetablesInScene)
        {
            Destroy(i);
        }
    }

    public void PlayerHasDead()
    {
        blackPanel.alpha = 1;
        PointsGainedScript.instance.playerDead = true;
        PointsGainedScript.instance.showingPoints = true;
    }

    private void DestroyAllEnemies()
    {
        for(int i = 0; i < enemiesInScene.Count; i++)
        {
            Destroy(enemiesInScene[i].gameObject);
            enemiesInScene.RemoveAt(i);
        }
    }

    private IEnumerator BirdSpawn()
    {
        birdSpawning = true;
        float timeToSpawn = Random.Range(MIN_BIRD_SPAWN_TIME, MAX_BIRD_SPAWN_TIME);
        yield return new WaitForSecondsRealtime(timeToSpawn);

        Vector2[] spawnPositions = new Vector2[2];
        spawnPositions[0] = new Vector2(MAX_BIRD_X, BIRD_Y);
        spawnPositions[1] = new Vector2(-MAX_BIRD_X, BIRD_Y);

        int spawnSelected = Random.Range(0, spawnPositions.Length - 1);
        GameObject newEnemy = Instantiate(enemiesPrefabs[2], spawnPositions[spawnSelected], Quaternion.identity);
        enemiesInScene.Add(newEnemy);
        birdAlive = true;
        birdSpawning = false;
    }

    public void BirdKilled()
    {
        thingsPoints[2] += 1;
        for (int i = 0; i < enemiesInScene.Count; i++)
        {
            if (enemiesInScene[i].GetComponent<BirdBehaviour>())
            {
                Destroy(enemiesInScene[i].gameObject);
                enemiesInScene.RemoveAt(i);
            }
        }
        birdAlive = false;
    }

    public IEnumerator SaveLocalData()
    {
        PlayerValues.SetScoreP1(actualScore);
        SaveJSON.SaveData();
        yield return new WaitForSecondsRealtime(3);
    }
}
