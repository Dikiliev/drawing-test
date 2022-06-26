using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paint
{
    public class Nodes : MonoBehaviour
    {
		private List<List<Node>> _nodes;

		private int _width => _nodes[0].Count; 
		private int _height => _nodes.Count;

		public void Initialize(int width, int height)
        {
			_nodes = new List<List<Node>>();

			for (int y = 0; y < height; y++)
			{
				List<Node> nodes = new List<Node>();

				for (int x = 0; x < width; x++)
				{
					nodes.Add(new Node(y, x));
				}

				_nodes.Add(nodes);
			}

			Clear();
		}
		public void Clear()
        {
			for (int x = 0; x < _width; ++x)
			{
				for (int y = 0; y < _height; ++y)
				{
					Node node = _nodes[x][y];

					if (x > 0)
						node.Left = _nodes[x - 1][y];

					if (x < _width - 1)
						node.Right = _nodes[x + 1][y];

					if (y < _height - 1)
						node.Down = _nodes[x][y + 1];

					if (y > 0)
						node.Up = _nodes[x][y - 1];
				}
			}
		}

		public List<Node> Search(int x, int y, Color32 color, Color32[] textureColors)
		{
			for (int i = 0; i < _width; ++i)
			{
				for (int j = 0; j < _height; ++j)
				{
					Node node = _nodes[i][j];
					node.IsClosed = false;
					node.IsOpened = false;
				}
			}

			List<Node> opened = new List<Node>() { };
			List<Node> closed = new List<Node>() { };

			opened.Add(_nodes[x][y]);

			while (opened.Count > 0)
			{

				Node node = opened[0].Up;
				if (node != null && IsTheSameColor(node, color, textureColors) && !node.IsOpenOrClosed)
				{
					node.IsOpened = true;
					opened.Add(node);
				}

				node = opened[0].Down;
				if (node != null && IsTheSameColor(node, color, textureColors) && !node.IsOpenOrClosed)
				{
					node.IsOpened = true;
					opened.Add(node);
				}

				node = opened[0].Left;
				if (node != null && IsTheSameColor(node, color, textureColors) && !node.IsOpenOrClosed)
				{
					node.IsOpened = true;
					opened.Add(node);
				}

				node = opened[0].Right;
				if (node != null && IsTheSameColor(node, color, textureColors) && !node.IsOpenOrClosed)
				{
					node.IsOpened = true;
					opened.Add(node);
				}

				opened[0].IsClosed = true;
				closed.Add(opened[0]);
				opened.RemoveAt(0);
			}

			return closed;
		}

		private bool IsTheSameColor(Node node, Color32 color, Color32[] textureColors)
		{
			int index = Mathf.FloorToInt(node.UV.y * _width + node.UV.x);
			Color32 coloredTextureColor = textureColors[index];

			return coloredTextureColor.a > 5 && coloredTextureColor.r == color.r && coloredTextureColor.g == color.g && coloredTextureColor.b == color.b;
		}

	}
}
