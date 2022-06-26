using UnityEngine;
using Paint;

public class DrawManager : MonoBehaviour
{
	public static DrawManager Instance;
	void Awake()
	{
		Instance = this;
	}

	public enum DrawMode
	{
		Draw,
		Fill,
	}

	[SerializeField] private SpriteRenderer _lineRenderer;
	[SerializeField] private SpriteRenderer _colorRenderer;
	[SerializeField] private Painter _painter;

	private DrawMode _drawMode = DrawMode.Draw;

	private Camera _camera;


	public DrawMode SelectedDrawMode => _drawMode;
	public bool IsEraser => _painter.IsEraser;

	private string _linePath => $"Level {Data.Level}/PictureLine";
	private string _colorPath => $"Level {Data.Level}/PictureColor";


	void Start()
	{
		_camera = Camera.main;

		Initialize();

		UI.Instance.RefreshBottomPanel();
	}
	void OnDisable()
	{
		Input.multiTouchEnabled = false;
	}


	private void Initialize()
	{
		_lineRenderer.sprite = Sprite.Create(Resources.Load(_linePath) as Texture2D, new Rect(0, 0, Data.Width, Data.Height), Vector2.one * 0.5f);
		_colorRenderer.sprite = Sprite.Create(Resources.Load(_colorPath) as Texture2D, new Rect(0, 0, Data.Width, Data.Height), Vector2.one * 0.5f);

		Texture2D drawn = ResourcesManager.LoadPaint(Data.Level);

		Texture2D texture = Resources.Load(_colorPath) as Texture2D;

		_painter._coloredTexture = texture;
		_painter.Initialize(drawn);

		SpriteRenderer spriteRenderer = _painter.GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = Sprite.Create(spriteRenderer.material.mainTexture as Texture2D, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
	}


	void Update()
	{
		if (!_painter.IsInitialize)
			return;

		if (Input.GetMouseButtonDown(0) && Input.touchCount < 2)
		{
			RaycastHit hit;
			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit))
			{
				if (_drawMode == DrawMode.Draw)
				{
					_painter.Draw(Input.mousePosition);
				}
				else if (_drawMode == DrawMode.Fill)
				{
					_painter.Fill(Input.mousePosition);
				}
			}
		}

		else if (Input.GetMouseButton(0))
		{
			if (Input.touchCount < 2)
			{
				if (_drawMode == DrawMode.Draw)
				{
					_painter.Draw(Input.mousePosition);
				}
				else if (_drawMode == DrawMode.Fill)
				{
					_painter.Fill(Input.mousePosition);
				}
			}
		}

		else if (Input.GetMouseButtonUp(0))
		{
			if (_drawMode == DrawMode.Draw)
			{
				_painter.DrawEnd();
			}
			else if (_drawMode == DrawMode.Fill)
			{

			}
		}
	}

	#region public functions

	public void SetColor(Color color)
    {
		_painter.Color = color;
    }
	public void SelectDrawMode()
    {
		_drawMode = DrawMode.Draw;
		UI.Instance.RefreshBottomPanel();
	}
	public void SelectFillMode()
	{
		_drawMode = DrawMode.Fill;
		UI.Instance.RefreshBottomPanel();
	}
	public void SelectEraser()
    {
		_painter.IsEraser = !_painter.IsEraser;
		UI.Instance.RefreshBottomPanel();
	}
	public void Clear()
    {
		_painter.ClearImage();
    }
	public void SavePaint()
	{
		ResourcesManager.SavePaint(_painter.PaintTexture);
	}

    #endregion

    private void OnApplicationQuit()
	{
		SavePaint();
	}
	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			SavePaint();
		}
	}
}
