using UnityEngine;

/// <summary>
/// A singleton class for a System.Random instance.
/// </summary>
public class Random : MonoBehaviour
{
    private static System.Random _instance = null;
	public static System.Random instance
    {
        get
        {
            if (_instance == null)
                _instance = new System.Random();
            return _instance;
        }
        private set { _instance = value; }
    }

    /// <summary>
    /// (Re)initialise the System.Random instance with a specific seed value.
    /// </summary>
    public static void Seed(int seed)
    {
        _instance = new System.Random(seed);
    }

    /// <summary>
    /// Calculates a random binary choice.
    /// </summary>
    /// <param name="chance">Chance of success [0-1].</param>
    public static bool YesOrNo(double chance)
    {
        if (instance.NextDouble() < chance)
            return true;
        return false;
    }
}
