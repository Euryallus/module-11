using System;

// ||=======================================================================||
// || FloatHelper: A static class used to easily compare float values       ||
// ||   and check if they are equal/less/greater than within a small        ||
// ||   tolerance. Prevents floats that are almost exactly equal from       ||
// ||   returning false when using standard '==' operator or similar.       ||
// ||=======================================================================||
// || Used on various prefabs.  						                    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public static class FloatHelper
{
    private const float Tolerance = 0.001f; // How close floats have to be in value to be considered 'equal'

    // The following functions are essentially equivelant to '==', '>', '>=' '<', '<=' operators
    //   but allow for floats within the tolerance range to be considered equal:

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
