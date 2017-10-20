
/// <summary>
/// A 2D integer coordinate / vector.
/// </summary>
[System.Serializable]
public class Point
{
	public int x;
	public int y;

	public Point()
	{
		x = y = 0;
	}

	public Point(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Point(Point p)
	{
		x = p.x;
		y = p.y;
	}

	public void Set(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public static bool operator ==(Point p1, Point p2)
	{
		if (p1.x == p2.x && p1.y == p2.y)
			return true;
		return false;
	}

	public static bool operator !=(Point p1, Point p2)
	{
		if (p1.x == p2.x && p1.y == p2.y)
			return false;
		return true;
	}

	public static Point operator +(Point p1, Point p2)  
    {  
        return new Point(p1.x + p2.x, p1.y + p2.y);
    }

	public static Point operator -(Point p1, Point p2)  
    {  
        return new Point(p1.x - p2.x, p1.y - p2.y);
    }

	public override bool Equals(object o)
	{
		try
		{
			return this == (Point)o;
		}
		catch
		{
			return false;
		}
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ")";
	}
}
