using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerValues
{
    [SerializeField] private static int topHighScore, highScore1, highScore2, highMountain;
    [SerializeField] private static int[] timeClearedLevel = new int[32];
    [SerializeField] private static float timePlayed;

    public static void SaveData()
    {
        // string playerInfoAsJSON = JsonUtility.ToJson(info); // info era una variable que referenciaba de PlayerValues
    }
}
