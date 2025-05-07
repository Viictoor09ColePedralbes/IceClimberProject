using System.Collections;
using System.Collections.Generic;
using UnityEngine;

<<<<<<< HEAD
[System.Serializable]
public static class PlayerValues
{
    public static int topHighScore, highScore1, highScore2;

    public static void SetScoreP1(int scoreP1)
    {
        if(scoreP1 > topHighScore)
        {
            topHighScore = scoreP1;
        }

        if(scoreP1 > highScore1)
        {
            highScore1 = scoreP1;
        }
    }

    public static void SetScoreP2(int scoreP2)
    {
        if (scoreP2 > topHighScore)
        {
            topHighScore = scoreP2;
        }

        if (scoreP2 > highScore2)
        {
            highScore2 = scoreP2;
        }
    }
    public static int GetTopHighScore()
    {
        return topHighScore;
    }


    public static int GetHighScoreP1()
    {
        return highScore1;
    }

    public static int GetHighScoreP2()
    {
        return highScore2;
    }
}

public class PlayerValuesSerializable
{
    public int topHighScore;
    public int highScore1;
    public int highScore2;

    public PlayerValuesSerializable()
    {
        topHighScore = PlayerValues.topHighScore;
        highScore1 = PlayerValues.highScore1;
        highScore2 = PlayerValues.highScore2;
=======
public static class PlayerValues
{
    [SerializeField] private static int topHighScore, highScore1, highScore2, highMountain;
    [SerializeField] private static int[] timeClearedLevel = new int[32];
    [SerializeField] private static float timePlayed;

    public static void SaveData()
    {
        // string playerInfoAsJSON = JsonUtility.ToJson(info); // info era una variable que referenciaba de PlayerValues
>>>>>>> 824093602cd369ac44b05addc3f40d56efe723d9
    }
}
