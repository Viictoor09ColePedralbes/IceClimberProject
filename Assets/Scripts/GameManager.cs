using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public int initialMountain = 1;
    [HideInInspector] public bool areSecondPlayer = false, gameStarted = false, isOnBonusStage = false;
    private bool stageFinished = false, showingPoints = false, hasEnterOnBS = true;
    private GameManager instance;
    private int actualScore, actualMountain;
    private int highScore, actualScene;
    private CameraUpScript[] camerasUP = new CameraUpScript[15];
    [SerializeField] private TMP_Text timeBonusStage_down, timeBonusStage_up;
    private float timeBonusStage = 40.0f;
    private PlayerMovement playerMovement;
    private Camera playerCamera;
    private Transform playerTransform;

    [SerializeField] private GameObject[] mountainsDestruibleBlocks = new GameObject[32], mountainsNotDestruibleBlocks = new GameObject[32], mountainsExtras = new GameObject[32];
    private List<GameObject> enemiesInScene;
    private GameObject parentMountains, parentExtras, actualDestruible, actualNotDestruible, actualExtras;

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // BORRAR DESPUES, SOLO PARA TEST
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        GetAllCamerasUp();
        timeBonusStage_down = GameObject.FindGameObjectWithTag("bs_time_down").GetComponent<TMP_Text>();
        timeBonusStage_up = GameObject.FindGameObjectWithTag("bs_time_up").GetComponent<TMP_Text>();

        parentMountains = GameObject.FindGameObjectWithTag("ParentMountains");
        parentExtras = GameObject.FindGameObjectWithTag("ParentExtras");
    }

    void Update()
    {
        if (isOnBonusStage)
        {
            if(hasEnterOnBS)
            {
                // Eliminar todos los enemigos activos de la escena
                
                playerMovement.animator.SetBool("hasHammer", false);
                timeBonusStage = 40.0f;
                hasEnterOnBS = false;
            }
            else
            {
                if (timeBonusStage > 0)
                {
                    timeBonusStage -= Time.deltaTime;
                    timeBonusStage_down.text = timeBonusStage.ToString("00.0");
                    timeBonusStage_up.text = timeBonusStage.ToString("00.0");
                }
                else if (timeBonusStage < 0 || stageFinished)
                {
                    playerMovement.animator.SetBool("hasHammer", true);
                    showingPoints = true; // Para mostrar los puntos obtenidos y ver como se suman a los ya acumulados
                }
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

            parentMountains = GameObject.FindGameObjectWithTag("ParentMountains");
            parentExtras = GameObject.FindGameObjectWithTag("ParentExtras");

            actualDestruible = Instantiate(mountainsDestruibleBlocks[initialMountain], Vector2.zero, Quaternion.identity, parentMountains.transform);
            actualNotDestruible = Instantiate(mountainsNotDestruibleBlocks[initialMountain], Vector2.zero, Quaternion.identity, parentMountains.transform);
            actualExtras = Instantiate(mountainsExtras[initialMountain], Vector2.zero, Quaternion.identity, parentExtras.transform);
        }
    }

    private void ChangeMountain()
    {
        playerCamera.transform.position = new Vector3(0, 0, -10);
        playerTransform.position = new Vector2(0, -2.53f);
        ReactivateCamerasUp();
        hasEnterOnBS = true;
        stageFinished = false;

        actualMountain++;
        if (actualDestruible)
            Destroy(actualDestruible);
        if(actualNotDestruible)
            Destroy(actualNotDestruible);
        if(actualExtras)
            Destroy(actualExtras);

        actualDestruible = Instantiate(mountainsDestruibleBlocks[actualMountain], Vector2.zero, Quaternion.identity, parentMountains.transform);
        actualNotDestruible = Instantiate(mountainsNotDestruibleBlocks[actualMountain], Vector2.zero, Quaternion.identity, parentMountains.transform);
        actualExtras = Instantiate(mountainsExtras[actualMountain], Vector2.zero, Quaternion.identity, parentExtras.transform);
    }

    private void GetAllCamerasUp()
    {
        for(int i = 0; i < camerasUP.Length; i++)
        {
            GameObject camera = GameObject.FindGameObjectWithTag("CameraUp");
            camera.tag = "Untagged";
            camerasUP[i] = camera.GetComponent<CameraUpScript>();
        }
    }

    private void ReactivateCamerasUp()
    {
        foreach(CameraUpScript cameraUp in camerasUP)
        {
            cameraUp.boxCollider.enabled = true;
        }
    }
}
