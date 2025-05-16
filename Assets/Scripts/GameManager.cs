using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using MongoDB.Bson;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public int initialMountain = 1, gainedPoints = 0;
    [HideInInspector] public bool areSecondPlayer = false, gameStarted = false, isOnBonusStage = false, stageFinished = false;
    private bool hasEnterOnBS = true, moveCloudContr = false;
    private static GameManager gameManager;
    public static GameManager instance
    {
        get
        {
            return RequestGameManager();
        }
    }
    private int actualMountain;
    public int actualScore;
    private CameraUpScript[] camerasUP = new CameraUpScript[15];
    [SerializeField] private TMP_Text timeBonusStage_down, timeBonusStage_up;
    public float timeBonusStage = 40.0f, timeToShowPoints = 0;
    private float timeCloudMov = 0;
    private PlayerMovement playerMovement;
    private Camera playerCamera;
    private Transform playerTransform, cloudContrTransf;

    [SerializeField] private GameObject[] mountains = new GameObject[32], mountainsExtras = new GameObject[32];
    private List<GameObject> enemiesInScene = new List<GameObject>();
    private GameObject parentMountains, parentExtras, actualDestruible, actualExtras;
    private CanvasGroup blackPanel;
    [HideInInspector] public int[] thingsPoints = new int[4]; // 0 = vegetables, 1 = ice, 2 = birds, 3 = blocks
    public GameObject[] vegetablesPrefabs = new GameObject[10];
    private GameObject[] vegetablesInScene = new GameObject[4];
    private Transform[] vegetablesScenePosition = new Transform[4];
    private Vector2 cloudPosDown = new Vector2(0, -5.8f), cloudPosUp = new Vector2(0, 5.8f);

    // Enemies variables
    [SerializeField] private GameObject[] enemiesPrefabs = new GameObject[3]; // 0 = yeti, 1 = oso, 2 = pajaro
    private const float MIN_BIRD_SPAWN_TIME = 20f, MAX_BIRD_SPAWN_TIME = 25f, MAX_BIRD_X = 8.2f, BIRD_Y = 4.3f;
    [HideInInspector] public bool birdAlive = false;
    private bool yetiSpawning = false;
    private const float MIN_YETI_SPAWN_TIME = 5, MAX_YETI_SPAWN_TIME = 5;

    // User telemetry variables
    public bool oneLifeGamemode = false;
    private float gameSessionTime = 0;
    [HideInInspector] public int pressedJump = 0;
    [HideInInspector] public int mountainsCleared = 0;
    [HideInInspector] public int enemiesDefeated = 0;

    // AudioClips
    [SerializeField] private AudioClip menuMusic, levelMusic;

    void Awake()
    {
        if (gameManager)
        {
            Destroy(gameObject);
        }
        RequestGameManager();
    }

    void Update()
    {
        if (isOnBonusStage)
        {
            if(hasEnterOnBS)
            {
                StopAllCoroutines();
                AudioManager.instance.PlayBGM(menuMusic, true);
                playerMovement.animator.SetBool("hasHammer", false);
                timeToShowPoints = 0;
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
                else if (timeBonusStage <= 0 || stageFinished)
                {
                    AudioManager.instance.StopBGM();
                    playerMovement.FreezingControl(true);
                    if(timeToShowPoints < 5)
                    {
                        timeToShowPoints += Time.deltaTime;
                    } 
                    else if(timeToShowPoints >= 5)
                    {
                        mountainsCleared++;
                        timeBonusStage_down.text = 0.0f.ToString("00.0");
                        timeBonusStage_up.text = 0.0f.ToString("00.0");
                        playerMovement.animator.SetBool("hasHammer", true);
                        blackPanel.alpha = 1;
                        PointsGainedScript.instance.startSounds = true;
                        PointsGainedScript.instance.showingPoints = true; // Para mostrar los puntos obtenidos y ver como se suman a los ya acumulados
                        isOnBonusStage = false;
                    }
                }
            }
        }

        if (gameStarted)
        {
            UserTelemetry();
            if (!isOnBonusStage)
            {
                if (!birdAlive)
                {
                    StartCoroutine(BirdSpawn());
                }

                if (!yetiSpawning)
                {
                    StartCoroutine(YetiSpawn());
                }
            }
        }

        if (moveCloudContr)
        {
            if(timeCloudMov < 1)
            {
                cloudContrTransf.position = Vector2.Lerp(cloudPosDown, cloudPosUp, timeCloudMov);
                timeCloudMov += Time.deltaTime * 2;
            }
            else if(timeCloudMov >= 1)
            {
                cloudContrTransf.position = cloudPosUp;
                moveCloudContr = false;
                timeCloudMov = 0;
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        actualMountain = initialMountain;
        
        if(scene.buildIndex == 1)
        {
            gameStarted = true;
            playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            GetAllCamerasUp();
            timeBonusStage_down = GameObject.FindGameObjectWithTag("bs_time_down").GetComponent<TMP_Text>();
            timeBonusStage_up = GameObject.FindGameObjectWithTag("bs_time_up").GetComponent<TMP_Text>();
            blackPanel = GameObject.FindGameObjectWithTag("blackPanel").GetComponent<CanvasGroup>();

            parentMountains = GameObject.FindGameObjectWithTag("ParentMountains");
            parentExtras = GameObject.FindGameObjectWithTag("ParentExtras");
            cloudContrTransf = GameObject.FindGameObjectWithTag("StartCloudMovement").GetComponent<Transform>();
            moveCloudContr = true;

            actualDestruible = Instantiate(mountains[initialMountain-1], Vector2.zero, Quaternion.identity, parentMountains.transform);
            actualExtras = Instantiate(mountainsExtras[initialMountain-1], Vector2.zero, Quaternion.identity, parentExtras.transform);
            playerMovement.destruibleTiles = GameObject.FindGameObjectWithTag("Destruible_block").GetComponent<Tilemap>();
            InstantiateVegetables();
            if (oneLifeGamemode)
            {
                playerMovement.OneLifeGamemode();
            }
            AudioManager.instance.PlayBGM(levelMusic, true);
        }
        else if (scene.buildIndex == 0)
        {
            ResetVariables();
            AudioManager.instance.PlayBGM(menuMusic, false);
        }
    }

    public void ChangeMountain()
    {
        playerMovement.destruibleTiles = null;
        DeleteVegetables();
        playerCamera.transform.position = new Vector3(0, 0, -10);
        playerTransform.position = new Vector2(-1.1f, -2.53f);
        ReactivateCamerasUp();
        hasEnterOnBS = true;
        stageFinished = false;
        timeBonusStage = 40;

        for(int i = 0; i < thingsPoints.Length; i++)
        {
            thingsPoints[i] = 0;
        }

        actualMountain++;
        if (actualDestruible)
            Destroy(actualDestruible);
        if(actualExtras)
            Destroy(actualExtras);

        if(actualMountain > 32)
        {
            actualMountain = 1;
        }

        actualDestruible = Instantiate(mountains[actualMountain-1], Vector2.zero, Quaternion.identity, parentMountains.transform);
        actualExtras = Instantiate(mountainsExtras[actualMountain-1], Vector2.zero, Quaternion.identity, parentExtras.transform);
        InstantiateVegetables();
        blackPanel.alpha = 0;
        AudioManager.instance.PlayBGM(levelMusic, true);
        playerMovement.FreezingControl(false);
        moveCloudContr = true;
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
        gameStarted = false;
        blackPanel.alpha = 1;
        AudioManager.instance.StopBGM();
        PointsGainedScript.instance.playerDead = true;
        PointsGainedScript.instance.showingPoints = true;
    }

    private IEnumerator BirdSpawn()
    {
        if (birdAlive)
        {
            yield break;
        }
        birdAlive = true;
        float timeToSpawn = Random.Range(MIN_BIRD_SPAWN_TIME, MAX_BIRD_SPAWN_TIME);
        yield return new WaitForSecondsRealtime(timeToSpawn);

        Vector2[] spawnPositions = new Vector2[2];
        spawnPositions[0] = new Vector2(MAX_BIRD_X, BIRD_Y);
        spawnPositions[1] = new Vector2(-MAX_BIRD_X, BIRD_Y);

        int spawnSelected = Random.Range(0, spawnPositions.Length - 1);
        GameObject newEnemy = Instantiate(enemiesPrefabs[2], spawnPositions[spawnSelected], Quaternion.identity);
        enemiesInScene.Add(newEnemy);
    }

    public IEnumerator SaveLocalData()
    {
        if(oneLifeGamemode)
        {
            PlayerValues.SetScoreP2(actualScore);
        }
        else
        {
            PlayerValues.SetScoreP1(actualScore);
        }
        SaveJSON.SaveData();
        yield return new WaitForSecondsRealtime(3);
    }

    private IEnumerator YetiSpawn()
    {
        if (yetiSpawning)
        {
            yield break;
        }
        yetiSpawning = true;
        float timeToSpawn = Random.Range(MIN_YETI_SPAWN_TIME, MAX_YETI_SPAWN_TIME);
        yield return new WaitForSecondsRealtime(timeToSpawn);

        Vector2[] spawnPositions = new Vector2[7];
        spawnPositions[0] = new Vector2(-8.94f, 0.409f);
        spawnPositions[1] = new Vector2(-8.94f, 3.708f);
        spawnPositions[2] = new Vector2(-8.94f, 7.011f);
        spawnPositions[3] = new Vector2(-8.94f, 10.306f);
        spawnPositions[4] = new Vector2(-8.94f, 13.62f);
        spawnPositions[5] = new Vector2(-8.94f, 16.911f);
        spawnPositions[6] = new Vector2(-8.94f, 20.197f);

        int spawnSelected = Random.Range(0, spawnPositions.Length - 1);
        int rightOrLeft = Random.Range(0, 2); // 0 = left, 1 = right

        Vector2 spawnModified = rightOrLeft == 0 ? spawnPositions[spawnSelected] : new Vector2(spawnPositions[spawnSelected].x * -1, spawnPositions[spawnSelected].y);

        GameObject newEnemy = Instantiate(enemiesPrefabs[0], spawnModified, Quaternion.identity);
        enemiesInScene.Add(newEnemy);
        yetiSpawning = false;
    }

    private void UserTelemetry() // Función ir calculando datos del usuario
    {
        gameSessionTime += Time.deltaTime;
    }

    public BsonDocument CreateUserTelemetryBSON()
    {
        BsonDocument bsonDoc = new BsonDocument
        {
            { "InitialMountain", initialMountain },
            { "OneLifeGamemode", oneLifeGamemode },
            { "MountainsCleared", mountainsCleared },
            { "GameSessionTime", gameSessionTime },
            { "PointsObtained", actualScore },
            { "EnemiesDefeated", enemiesDefeated},
            { "PressedJump", pressedJump },
        };
        return bsonDoc;
    }

    private void ResetVariables()
    {
        isOnBonusStage = false;
        stageFinished = false;
        gainedPoints = 0;
        actualScore = 0;
        birdAlive = false;
        yetiSpawning = false;
        oneLifeGamemode = false;
        gameSessionTime = 0;
        pressedJump = 0;
        mountainsCleared = 0;
        enemiesDefeated = 0;
        for (int i = 0; i < thingsPoints.Length; i++)
        {
            thingsPoints[i] = 0;
        }
    }

    public void GoToMenuButton()
    {
        mountainsCleared++;
        Time.timeScale = 1;
        playerMovement.FreezingControl(true);
        StopAllCoroutines();
        gameStarted = false;
        blackPanel.alpha = 1;
        PointsGainedScript.instance.playerDead = true;
        PointsGainedScript.instance.showingPoints = true;
    }
}
