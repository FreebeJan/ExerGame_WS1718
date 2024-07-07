using UnityEngine;
using System.Collections;

public class UnionFindNode<ValueType>
{
	private UnionFindNode<ValueType> Parent;
	private int Rank;
	public ValueType Value;

	public UnionFindNode(ValueType value)
	{
		Rank = 0;
		Parent = this;
		Value = value;
	}

	// Flattens the UnionFind Tree
	public UnionFindNode<ValueType> Find()
	{
		if (Parent != this)
		{
			this.Parent = Parent.Find();
		}
		return this.Parent;
	}

	public bool IsConnected(UnionFindNode<ValueType> other)
	{
		this.Find();
		other.Find();
		return (this.Parent == other.Parent || this == other.Parent || this.Parent == other);
	}

	public static void Union(UnionFindNode<ValueType> x, UnionFindNode<ValueType> y)
	{
		UnionFindNode<ValueType> xRoot = x.Find();
		UnionFindNode<ValueType> yRoot = y.Find();

		if (xRoot == yRoot) return;
		if (xRoot.Rank > yRoot.Rank)
		{
			yRoot.Parent = xRoot;
		}
		else if (yRoot.Rank > xRoot.Rank)
		{
			xRoot.Parent = yRoot;
		}
		else
		{
			yRoot.Parent = xRoot;
			xRoot.Rank++;
		}

	}
}
