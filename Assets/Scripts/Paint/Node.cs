using UnityEngine;

namespace Paint
{
	public class Node
	{
		public Node Left, Right, Up, Down;

		public Vector2 UV = Vector2.zero;

		public bool IsClosed = false;
		public bool IsOpened = false;
		public bool IsOpenOrClosed => IsClosed || IsOpened;

		public Node(int x, int y)
		{
			UV.x = x;
			UV.y = y;
		}
	}
}
