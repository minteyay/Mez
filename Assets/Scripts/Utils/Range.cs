
/// <summary>
/// An integer range.
/// </summary>
[System.Serializable]
public class Range : Point
{
	public override string ToString()
	{
		if (x == y)
			return x.ToString();
		return x + " - " + y;
	}
}
