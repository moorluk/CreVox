using UnityEngine;
using System.Collections;

public class vMinMaxRangeAttribute : PropertyAttribute
{
    public float minLimit, maxLimit;

    public vMinMaxRangeAttribute(float minLimit, float maxLimit)
    {
        this.minLimit = minLimit;
        this.maxLimit = maxLimit;
    }
}

[System.Serializable]
public class vMinMaxRange
{
    public float rangeStart, rangeEnd;

    public float GetRandomValue()
    {
        return Random.Range(rangeStart, rangeEnd);
    }
}