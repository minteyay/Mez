using UnityEngine;

/// <summary>
/// Static class for using a single System.Random instance for RNG stuff.
/// If not explicitly seeded, the static instance will be created when it's first accessed.
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
    /// Initialise the static instance of System.Random with a specific seed value.
    /// </summary>
    /// <param name="seed"></param>
    public static void Seed(int seed)
    {
        _instance = new System.Random(seed);
    }
}
