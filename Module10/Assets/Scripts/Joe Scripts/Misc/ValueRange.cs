// ||=======================================================================||
// || ValueRange: A simple generic class that holds two values, a min and   ||
// ||   max, of any type. Ideally made to be used with int/float types.     ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[System.Serializable]
public struct ValueRange<T> where T : notnull
{
    public T Min;
    public T Max;

    // Constructor
    public ValueRange(T min, T max)
    {
        Min = min;
        Max = max;
    }
}