using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGem", menuName = "Scriptable Objects/New Gem", order = 0)]
public class Gem : ScriptableObject
{
    public int ID;
    public Sprite sprite;
}
