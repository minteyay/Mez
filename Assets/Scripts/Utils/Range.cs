
/// <summary>
/// An integer range.
/// </summary>
[System.Serializable]
public class Range : Point
{
	// TODO: Don't allow y to be smaller than x.
	public Range()
	{
		x = y = 0;
	}

	public Range(int x, int y)
	{
		this.x = x;
		this.y = y;
		Validate();
	}

	public Range(Range p)
	{
		x = p.x;
		y = p.y;
		Validate();
	}

	public override void Set(int x, int y)
	{
		base.Set(x, y);
		Validate();
	}

	private void Validate()
	{
		if (x > y)
			Utils.Swap(ref x, ref y);
	}

	public override string ToString()
	{
		if (x == y)
			return x.ToString();
		return x + " - " + y;
	}
}
