using System.IO;
using UnityEngine;

public class Painter : MonoBehaviour
{
	public enum Tool { drawing, pouring, eraser }

	[SerializeField] private Texture2D _brush;
	[SerializeField] private Color _color;
	[SerializeField] private int _interval = 5;

	[Space]
	[SerializeField] private MeshRenderer _renderer;
	[SerializeField] private Texture2D _draweTexture;
	[SerializeField] private Texture2D _coloringTexture;
	[SerializeField] private Texture2D _paintedTexture;

	private const string FOLDER_NAME = "paints";

	private Tool _selectedTool;
	private Color _drawingPartColor;


	private int _width => _coloringTexture.width;
	private int _height => _coloringTexture.height;

	public Tool SelectedTool => _selectedTool;
	public Texture2D Texture => _draweTexture;


	private void Awake()
	{
		ConvertToTextureAndLoad();
		_renderer.sharedMaterial.mainTexture = _draweTexture;
	}

	public void SetDrawingPart(Vector2Int coord)
	{
		if (0 > coord.x || coord.x >= _width || 0 > coord.y || coord.y >= _height)
		{
			_drawingPartColor = new Color(0, 0, 0, 0);
			return;
		}

		_drawingPartColor = _paintedTexture.GetPixel(coord.x, coord.y);
	}
	public void Draw(Vector2 from, Vector2 to)
	{
		float distance = Vector2.Distance(from, to);
		Vector2 differece = to - from;

		if (distance < 1.5f)
		{
			Draw(to);
			_draweTexture.Apply(false);
			return;
		}

		float current = 0;

		while (current < distance)
		{
			float value = current / distance;
			Draw(from + differece * value);

			current += _interval;
		}

		Draw(to);

		_draweTexture.Apply(false);
	}
	public void Pour()
    {
		for (int x = 0; x < _width; ++x)
		{
			for (int y = 0; y < _height; ++y)
			{
				if (_paintedTexture.GetPixel(x, y) == _drawingPartColor)
                {
					_draweTexture.SetPixel(x, y, _color);
				}
			}
		}

		_draweTexture.Apply(false);
	}

	public void Clear()
	{
		for (int x = 0; x < _width; ++x)
		{
			for (int y = 0; y < _height; ++y)
			{
				_draweTexture.SetPixel(x, y, new Color(1, 1, 1, 1));
			}
		}

		_draweTexture.Apply();
	}

	public void SetColor(Color color)
    {
		_color = color;
    }
	public void SelectBrush()
	{
		_selectedTool = Tool.drawing;
	}
	public void SelectEraser()
	{
		_selectedTool = Tool.eraser;
	}
	public void SelectPouirng()
	{
		_selectedTool = Tool.pouring;
	}

	private void Draw(Vector2 coord)
	{
		coord.x -= _brush.width / 2f;
		coord.y -= _brush.height / 2f;

		for (int x = 0; x < _brush.width; ++x)
		{
			for (int y = 0; y < _brush.height; ++y)
			{
				Vector2Int pixelCoord = new Vector2Int((int)coord.x + x, (int)coord.y + y);

				if (0 > pixelCoord.x || pixelCoord.x >= _width || 0 > pixelCoord.y || pixelCoord.y >= _height) continue;

				if (_paintedTexture.GetPixel(pixelCoord.x, pixelCoord.y) != _drawingPartColor) continue;


				Color color = _selectedTool == Tool.eraser ? new Color(1, 1, 1, 1) : _color;
				color.a = _brush.GetPixel(x, y).a;

				Color textureColor = _draweTexture.GetPixel(pixelCoord.x, pixelCoord.y);

				color = color * color.a + textureColor * (1f - color.a);
				color.a = 1f;

				_draweTexture.SetPixel(pixelCoord.x, pixelCoord.y, color);
			}
		}
	}

	private void OnApplicationQuit()
    {
		ConvertToPngAndSave();
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
			ConvertToPngAndSave();
        }
    }

	private string GetSavePath()
	{
		string directoryPath = Application.persistentDataPath + "/" + FOLDER_NAME + "/";

		if (!Directory.Exists(directoryPath))
		{
			//Create if it doesn't exist yet
			Directory.CreateDirectory(directoryPath);
			return directoryPath + "paint.png ";
		}

		return directoryPath + "paint.png ";
	}
	private void ConvertToTextureAndLoad()
	{
		if (!File.Exists(GetSavePath()))
        {
			return;
        }

		byte[] bytes = File.ReadAllBytes(GetSavePath());
		Texture2D loadTexture = new Texture2D(_width, _height);
		loadTexture.LoadImage(bytes);

		_draweTexture = Sprite.Create(loadTexture, new Rect(0, 0, loadTexture.width, loadTexture.height), Vector2.zero).texture;
	}
	private void ConvertToPngAndSave()
	{
		byte[] bytes = _draweTexture.EncodeToPNG();
		File.WriteAllBytes(GetSavePath(), bytes);

		print("Save: " + GetSavePath());
	}
}


