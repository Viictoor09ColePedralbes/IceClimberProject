using MongoDB.Bson.Serialization.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public static class PlayerValues
{
    public static int topHighScore, highScore1, highScore2;
    public static string id_player;
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
    public string id_player;
    public int topHighScore;
    public int highScore1;
    public int highScore2;

    public PlayerValuesSerializable()
    {
        id_player = PlayerValues.id_player;
        topHighScore = PlayerValues.topHighScore;
        highScore1 = PlayerValues.highScore1;
        highScore2 = PlayerValues.highScore2;
    }
}
