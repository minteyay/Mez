
/// <summary>
/// An integer range.
/// </summary>
[System.Serializable]
public class Range : Point
{
	public Range()
	{
		x = y = 0;
	}

	public Range(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Range(Range p)
	{
		x = p.x;
		y = p.y;
	}

	public override string ToString()
	{
		if (x == y)
			return x.ToString();
		return x + " - " + y;
	}
}
