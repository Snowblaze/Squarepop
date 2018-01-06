using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            count = value < 0 ? 0 : value;
        }
    }

    public Color TargetColor
    {
        get
        {
            return targetColor;
        }
        set
        {
            targetColor = value;
        }
    }
}
