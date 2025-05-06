using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PointsGainedScript : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasShowPoints;
    [SerializeField] private TMP_Text[] actualPointsTexts, winLose_texts, vegetablesPoints_texts, obtainedVegetables_text, destroyedIce_text, defeatedBirds_text, destroyedBlocks_text; // 0 es P1, 1 es P2
    public Image[] actualVegetable_images;
    [HideInInspector] public bool showingPoints, playerDead;
    private Color visible = new Vector4(1,1,1,1), invisible = new Vector4(1,1,1,0);

    [HideInInspector] public int pointsVegetables;
    private const int POINTS_BONUS_STAGE = 6000, DESTROY_ICE_POINTS = 400, DEFEATED_BIRDS_POINTS = 800, DESTROY_BLOCKS_POINTS = 10;
    private const string winText = "WINNER BONUS! 6000", loseText = "NO BONUS!";
    [HideInInspector] public Sprite actualVegetableSprite;
    [SerializeField] private Animator popoDoingSmth, nanaDoingSmth;

    private static PointsGainedScript pointsGainedScript;
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
    }

    private IEnumerator showPoints()
    {
        if (canvasShowPoints.alpha == 0)
        {
            Debug.Log("Showing canvas Points");
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
            StartCoroutine(GameManager.instance.SaveLocalData());
            SceneManager.LoadScene(0);
        }
    }

    private IEnumerator sumPuntosBonusStage()
    {
        int newScore;
        if (GameManager.instance.stageFinished)
        {
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
                actualPointsTexts[j].text = Mathf.Lerp(GameManager.instance.actualScore, newScore, i).ToString("000000");
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
        GameManager.instance.actualScore = newScore;
        yield return new WaitForSecondsRealtime(1);
    }
    private IEnumerator sumPoints(TMP_Text[] text, int thingPoint, int multPoints)
    {
        int newScore = GameManager.instance.actualScore + (GameManager.instance.thingsPoints[thingPoint] * multPoints);
        Debug.Log("New Score: " + newScore);
        for (float i = 0; i < 1.1f; i += 0.1f)
        {
            for(int j = 0; j < text.Length; j++)
            {
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
}
