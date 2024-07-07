using UnityEngine;
using System;

public struct Location2D : IEquatable<Location2D> // 2 * 4 Byte
{
	public int x, y; // 2 * 4 Byte
	public bool valid;

	public override bool Equals(object obj)
	{
		return obj is PathNode && this == (Location2D)obj;
	}

	public bool Equals(Location2D other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		unchecked // overflow is not a problem.
		{
			int hash = x;
			return hash * 881 + y; // 881 is prime
		}
	}

	public static bool operator ==(Location2D x, Location2D y)
	{
		return x.x == y.x && x.y == y.y;
	}

	public static bool operator !=(Location2D x, Location2D y)
	{
		return !(x == y);
	}

	public static Location2D FromVector2Int(Vector2Int vector)
	{
		Location2D a = new Location2D
		{
			x = vector.x,
			y = vector.y
		};
		return a;
	}
}