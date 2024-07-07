using System.Collections.Generic;

namespace Assets.Utils
{
	public class LinkedListNode<ValueType>
	{
		public ValueType Value;
		public LinkedListNode<ValueType> Next;
		public LinkedListNode<ValueType> Previous;

		public LinkedListNode(ValueType value)
		{
			Value = value;
			Next = null;
			Previous = null;
		}
	}

	public class LinkedList<ValueType>
	{
		public LinkedListNode<ValueType> First;
		public LinkedListNode<ValueType> Last;
		private int _count;
		public int Count
		{
			get { if (!CountValid) RevalidateCount(); return _count; }
			set { _count = value; }
		}

		private bool CountValid;

		public LinkedList()
		{
			_count = 0;
			CountValid = true;
			First = null;
			Last = null;
		}

		public void RevalidateCount()
		{
			int i = 0;
			LinkedListNode<ValueType> node = this.First;
			while (node != null)
			{
				i++;
				if (node == Last) break;
				node = node.Next;
			}
			_count = i;
			CountValid = true;
		}

		public LinkedList(IEnumerable<ValueType> source)
		{
			_count = 0;
			CountValid = true;
			First = null;
			Last = null;
			foreach (ValueType value in source)
			{
				AddLast(value);
			}
		}

		public void AddLast(ValueType value)
		{
			if (_count == 0)
			{
				First = Last = new LinkedListNode<ValueType>(value);
			}
			else
			{
				LinkedListNode<ValueType> V = new LinkedListNode<ValueType>(value);
				Last.Next = V;
				V.Previous = Last;
				Last = V;
			}
			_count++;
		}

		public void AddFirst(ValueType value)
		{
			if (Count == 0)
			{
				First = Last = new LinkedListNode<ValueType>(value);
			}
			else 
			{
				LinkedListNode<ValueType> V = new LinkedListNode<ValueType>(value);
				First.Previous = V;
				V.Next = First;
				First = V;
			}
			_count++;
		}

		/** Split Path in two at value.
         * Stores path from Start to value in this object, And path from value to End in Branch.
         * returns true if successful false otherwise.
         */
		public bool SplitAt(ValueType value, ref LinkedList<ValueType> Branch, bool overlap = false)
		{
			if (First == null || Last == null) return false;
			if (First.Value.Equals(value) || Last.Value.Equals(value)) return false;
			if (Branch == null) Branch = new LinkedList<ValueType>();
			LinkedListNode<ValueType> node = First;
			int i = 0;
			while (node != null)
			{
				if (node.Value.Equals(value))
				{
					if (i == 0) return false;
					if (i == _count - 1) return false;
					Branch.Last = Last;
					Branch.First = node;
					Branch._count = _count - i;
					Branch.CountValid = CountValid;
					Last = Branch.First.Previous;
					Branch.First.Previous = null;
					Last.Next = null;
					_count = i;
					if (overlap) this.AddLast(node.Value);
					return true;
				}
				node = node.Next;
				i++;
			}
			return false;
		}


		/** Split path at given node.
         * WARNING: it will not be tested wheter node is in path.
         */
		public bool SplitAt(ref LinkedListNode<ValueType> At, ref LinkedList<ValueType> Branch, bool overlap = false)
		{
			if (At == null) return false;
			if (First == null || Last == null) return false;
			if (At.Previous == null || At.Next == null) return false;

			if (Branch == null) Branch = new LinkedList<ValueType>();

			Branch.Last = Last;
			Branch.First = At;
			Branch._count = _count;
			Branch.CountValid = false;
			CountValid = false;

			Last = At.Previous;
			Last.Next = null;

			Branch.First.Previous = null;
			if (overlap) AddLast(At.Value);
			return true;
		}
	}
}
