using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PointsGainedScript : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasShowPoints;
    [SerializeField] private TMP_Text actualMountainText;
    [SerializeField] private TMP_Text[] actualPointsTexts, winLose_texts, vegetablesPoints_texts, obtainedVegetables_text, destroyedIce_text, defeatedBirds_text, destroyedBlocks_text; // 0 es P1, 1 es P2
    public Image[] actualVegetable_images;
    [HideInInspector] public bool showingPoints, playerDead, startSounds = false;
    private Color visible = new Vector4(1,1,1,1), invisible = new Vector4(1,1,1,0);

    [HideInInspector] public int pointsVegetables;
    private const int POINTS_BONUS_STAGE = 6000, DESTROY_ICE_POINTS = 400, DEFEATED_BIRDS_POINTS = 800, DESTROY_BLOCKS_POINTS = 10;
    private const string winText = "WINNER BONUS! 6000", loseText = "NO BONUS!";
    [HideInInspector] public Sprite actualVegetableSprite;
    [SerializeField] private Animator popoDoingSmth, nanaDoingSmth;

    private static PointsGainedScript pointsGainedScript;
    [SerializeField] private AudioClip gainPointsClip, cryClip, jumpClip;
    private bool areDoingSound = false;
    public static PointsGainedScript instance
    {
        get
        {
            return RequestPointsGainedScript();
        }
    }
    void Start()
    {
        canvasShowPoints.alpha = 0;
        ChangeVisibilityOfAllTexts(false, 0);
    }

    void Update()
    {
        if (showingPoints)
        {
            StartCoroutine(showPoints());
            showingPoints = false;
        }

        if (!areDoingSound && startSounds)
        {
            StartCoroutine(doSound());
        }
    }

    private IEnumerator showPoints()
    {
        if (canvasShowPoints.alpha == 0)
        {
            Debug.Log("Showing canvas Points");
            actualMountainText.text = GameManager.instance.mountainsCleared >= 99 ? 99.ToString("MOUNTAIN 00") : GameManager.instance.mountainsCleared.ToString("MOUNTAIN 00");
            canvasShowPoints.alpha = 1;
            foreach (TMP_Text i in winLose_texts)
            {
                i.text = GameManager.instance.stageFinished ? winText : loseText;
            }

            foreach(Image i in actualVegetable_images)
            {
                i.sprite = actualVegetableSprite;
            }

            foreach(TMP_Text i in vegetablesPoints_texts)
            {
                i.text = pointsVegetables.ToString(pointsVegetables >= 1000 ? "0000" : "000");
            }
            popoDoingSmth.SetBool("isCelebrating", GameManager.instance.stageFinished);
            popoDoingSmth.SetBool("isCrying", !GameManager.instance.stageFinished);
            nanaDoingSmth.SetBool("isCelebrating", GameManager.instance.stageFinished);
            nanaDoingSmth.SetBool("isCrying", !GameManager.instance.stageFinished);
        }

        yield return new WaitForSecondsRealtime(2);

        for (int i = 0; i < 6; i++) // Bucle para sumar los puntos a los jugadores
        {
            if (i >= 1 && i != 5)
            {
                ChangeVisibilityOfAllTexts(true, i);
            }

            switch (i)
            {
                case 0: // Sumar puntos bonus stage
                    StartCoroutine(sumPuntosBonusStage());
                    break;
                case 1: // Sumar puntos vegetales
                    StartCoroutine(sumPoints(obtainedVegetables_text, (i - 1), pointsVegetables));
                    break;
                case 2: // Sumar puntos hielo
                    StartCoroutine(sumPoints(destroyedIce_text, (i - 1), DESTROY_ICE_POINTS));
                    break;
                case 3: // Sumar puntos aves
                    StartCoroutine(sumPoints(defeatedBirds_text, (i - 1), DEFEATED_BIRDS_POINTS));
                    break;
                case 4: // Sumar puntos bloques
                    StartCoroutine(sumPoints(destroyedBlocks_text, (i - 1), DESTROY_BLOCKS_POINTS));
                    break;
                case 5:
                    if(!playerDead)
                    {
                        canvasShowPoints.alpha = 0;
                        ChangeVisibilityOfAllTexts(false, 0);
                        startSounds = false;
                        GameManager.instance.ChangeMountain();
                    }
                    showingPoints = false;
                    break;
            }
            yield return new WaitForSecondsRealtime(2f);
        }
        popoDoingSmth.SetBool("isCelebrating", false);
        popoDoingSmth.SetBool("isCrying", false);
        nanaDoingSmth.SetBool("isCelebrating", false);
        nanaDoingSmth.SetBool("isCrying", false);
        if(playerDead)
        {
            MongoDB_Script.instance.InsertData(); // Añadimos los datos de telemetria en la base de datos
            GameManager.instance.gameStarted = false;
            StartCoroutine(GameManager.instance.SaveLocalData());
            SceneManager.LoadScene(0);
        }
    }

    private IEnumerator sumPuntosBonusStage()
    {
        bool doSound = false;
        int newScore;
        if (GameManager.instance.stageFinished)
        {
            doSound = true;
            newScore = GameManager.instance.actualScore + POINTS_BONUS_STAGE;
        }
        else
        {
            newScore = GameManager.instance.actualScore;
        }
        
        Debug.Log("New Score: " + newScore);
        for (float i = 0; i < 1.1f; i += 0.1f)
        {
            for (int j = 0; j < actualPointsTexts.Length; j++)
            {
                if (doSound)
                {
                    AudioManager.instance.PlaySFX(gainPointsClip);
                }
                actualPointsTexts[j].text = Mathf.Lerp(GameManager.instance.actualScore, newScore, i).ToString("000000");
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
        GameManager.instance.actualScore = newScore;
        yield return new WaitForSecondsRealtime(1);
    }
    private IEnumerator sumPoints(TMP_Text[] text, int thingPoint, int multPoints)
    {
        bool doSound = false;
        int newScore = GameManager.instance.actualScore + (GameManager.instance.thingsPoints[thingPoint] * multPoints);
        if(GameManager.instance.actualScore < newScore)
        {
            doSound = true;
        }
        Debug.Log("New Score: " + newScore);
        for (float i = 0; i < 1.1f; i += 0.1f)
        {
            for(int j = 0; j < text.Length; j++)
            {
                if (doSound)
                {
                    AudioManager.instance.PlaySFX(gainPointsClip);
                }
                text[j].text = Mathf.Lerp(0, GameManager.instance.thingsPoints[thingPoint], i).ToString("00");
                actualPointsTexts[j].text = Mathf.Lerp(GameManager.instance.actualScore, newScore, i).ToString("000000");
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
        GameManager.instance.actualScore = newScore;
        yield return new WaitForSecondsRealtime(1);
    }

    private void ChangeVisibilityOfAllTexts(bool isVisible, int specificOne)
    {
        if(specificOne == 0)
        {
            foreach (TMP_Text text in obtainedVegetables_text)
            {
                text.color = isVisible ? visible : invisible;
            }

            foreach (TMP_Text text in destroyedIce_text)
            {
                text.color = isVisible ? visible : invisible;
            }

            foreach (TMP_Text text in defeatedBirds_text)
            {
                text.color = isVisible ? visible : invisible;
            }

            foreach (TMP_Text text in destroyedBlocks_text)
            {
                text.color = isVisible ? visible : invisible;
            }
        }
        else
        {
            switch (specificOne)
            {
                case 1:
                    foreach (TMP_Text text in obtainedVegetables_text)
                    {
                        text.color = isVisible ? visible : invisible;
                    }
                    break;
                case 2:
                    foreach (TMP_Text text in destroyedIce_text)
                    {
                        text.color = isVisible ? visible : invisible;
                    }
                    break;
                case 3:
                    foreach (TMP_Text text in defeatedBirds_text)
                    {
                        text.color = isVisible ? visible : invisible;
                    }
                    break;
                case 4:
                    foreach (TMP_Text text in destroyedBlocks_text)
                    {
                        text.color = isVisible ? visible : invisible;
                    }
                    break;
            }
        }
    }

    private static PointsGainedScript RequestPointsGainedScript()
    {
        if (!pointsGainedScript)
        {
            pointsGainedScript = FindObjectOfType<PointsGainedScript>();
        }
        return pointsGainedScript;
    }

    private IEnumerator doSound()
    {
        areDoingSound = true;
        AudioManager.instance.PlaySFX(GameManager.instance.stageFinished ? jumpClip : cryClip);
        yield return new WaitForSecondsRealtime(1f);
        areDoingSound = false;
    }
}
