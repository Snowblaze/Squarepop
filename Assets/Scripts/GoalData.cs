using System;
using UnityEngine;

[Serializable]
public class GoalData
{
    [SerializeField]
    private int count;
    [SerializeField]
    private Color targetColor;

    public int Count
    {
        get
        {
            return count;
        }
        set
        {
            count = value > 0 
                ? value 
                : 0;
        }
    }
    public Color TargetColor { get; set; }
}
