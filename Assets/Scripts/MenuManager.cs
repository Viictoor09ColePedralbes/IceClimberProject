using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
<<<<<<< HEAD
using System.IO;
=======
>>>>>>> 824093602cd369ac44b05addc3f40d56efe723d9

public class MenuManager : MonoBehaviour
{
    private int selectedMountain = 1, gameModeSelection = 0;
    private int topHighScore, highScore1, highScore2;

    [SerializeField] private TMP_Text textMountainSelected, textTopHighScore, textHighScore1, textHighScore2;
    [SerializeField] private InputActionAsset inputMenuMap;
    [SerializeField] private RectTransform selectPlayerIcon;
    [SerializeField] private RectTransform[] totalTransIcon;
    private InputAction inputPlayersSelection, inputIniMount, inputStartGame;

    void Awake()
    {
        inputMenuMap.Enable();
    }
    void Start()
    {
        inputPlayersSelection = inputMenuMap.FindActionMap("Menu").FindAction("PlayerSelection");
        inputIniMount = inputMenuMap.FindActionMap("Menu").FindAction("InitialMountain");
        inputStartGame = inputMenuMap.FindActionMap("Menu").FindAction("StartGame");
        textMountainSelected.text = selectedMountain.ToString("00");
        textTopHighScore.text = topHighScore.ToString("000000");
        textHighScore1.text = highScore1.ToString("000000");
        textHighScore2.text = highScore2.ToString("000000");
        selectPlayerIcon.position = totalTransIcon[0].position;
<<<<<<< HEAD
        LoadData();
=======
>>>>>>> 824093602cd369ac44b05addc3f40d56efe723d9
    }

    void Update()
    {
        if (inputStartGame.triggered)
        {
            StartGame();
        }

        if(inputIniMount.ReadValue<float>() >= 1 && inputIniMount.triggered)
        {
            selectedMountain++;
            if(selectedMountain > 32)
            {
                selectedMountain = 1;
            }
            textMountainSelected.text = selectedMountain.ToString("00");
        }
        else if(inputIniMount.ReadValue<float>() <= -1 && inputIniMount.triggered)
        {
            selectedMountain--;
            if (selectedMountain < 1)
            {
                selectedMountain = 32;
            }
            textMountainSelected.text = selectedMountain.ToString("00");
        }

        if (inputPlayersSelection.ReadValue<float>() >= 1 && inputPlayersSelection.triggered)
        {
            gameModeSelection++;
            if (gameModeSelection > (totalTransIcon.Length - 1))
            {
                gameModeSelection = 0;
            }
            selectPlayerIcon.position = totalTransIcon[gameModeSelection].position;
        }
        else if(inputPlayersSelection.ReadValue<float>() <= -1 && inputPlayersSelection.triggered)
        {
            gameModeSelection--;
            if (gameModeSelection < 0)
            {
                gameModeSelection = (totalTransIcon.Length - 1);
            }
            selectPlayerIcon.position = totalTransIcon[gameModeSelection].position;
        }
    }

    private void StartGame()
    {
        GameManager.instance.initialMountain = selectedMountain;
        if(gameModeSelection == 1)
        {
            GameManager.instance.areSecondPlayer = true;
        }
        GameManager.instance.gameStarted = true;
        SceneManager.LoadScene(1);
    }
<<<<<<< HEAD

    private void LoadData()
    {
        string ruta = Application.persistentDataPath + "/datosJugador.json";

        if (File.Exists(ruta))
        {
            string json = File.ReadAllText(ruta);

            PlayerValuesSerializable datosRestaurados = JsonUtility.FromJson<PlayerValuesSerializable>(json);

            PlayerValues.topHighScore = datosRestaurados.topHighScore;
            PlayerValues.highScore1 = datosRestaurados.highScore1;
            PlayerValues.highScore2 = datosRestaurados.highScore2;

            textTopHighScore.text = datosRestaurados.topHighScore.ToString("000000");
            textHighScore1.text = datosRestaurados.highScore1.ToString("000000");
            textHighScore2.text = datosRestaurados.highScore2.ToString("000000");

            Debug.Log("Datos cargados desde: " + ruta);
        }
        else
        {
            Debug.Log("No se encontró el archivo de datos.");
        }
    }
=======
>>>>>>> 824093602cd369ac44b05addc3f40d56efe723d9
}
