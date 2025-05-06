using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Vegetables/New vegetable")]
public class Vegetable : ScriptableObject
{
    [SerializeField] public Sprite vegetable;
    [Range(300, 5000)] public int points;
}
