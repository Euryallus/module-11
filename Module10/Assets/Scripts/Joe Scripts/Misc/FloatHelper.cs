using System;

public static class FloatHelper
{
    private const float Tolerance = 0.001f;

    public static bool FloatsAreEqual(float val1, float val2)
    {
        return (Math.Abs(val1 - val2) <= Tolerance);
    }

    public static bool FloatIsGreaterThan(float val1, float val2)
    {
        return (val1 > val2 - Tolerance);
    }

    public static bool FloatIsGreaterThanOrEqualTo(float val1, float val2)
    {
        return (val1 >= val2 - Tolerance);
    }

    public static bool FloatIsLessThan(float val1, float val2)
    {
        return (val1 < val2 + Tolerance);
    }

    public static bool FloatIsLessThanOrEqualTo(float val1, float val2)
    {
        return (val1 <= val2 + Tolerance);
    }
}
