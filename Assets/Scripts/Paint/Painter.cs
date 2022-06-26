using UnityEngine;
using System.Collections.Generic;
using System.Threading;

namespace Paint
{
	public class Painter : MonoBehaviour
	{
		public Texture2D _coloredTexture;

		[SerializeField] private bool _isAutoInit = false;
		[Space]
		[SerializeField] private Color32 _color = new Color32(255, 0, 0, 255);
		[SerializeField] private float _lerpDamp = 0.01f;
		[SerializeField] private int _brushSize = 16;
		[Space]
		[SerializeField] private bool _isEraser = false;
		[SerializeField] private bool _haveMaskColor = false;

		#region private

		private Nodes _nodes;
		private Camera _camera;

		private Texture2D _baseTexture;
		private byte[] _pixels;
		private Color32[] _coloredTextureColors;

		private int _sourceWidth;
		private int _sourceHeight;

		private bool _isDown = false;
		private Vector2 _lastPosition;

		private Color32 _downColor;
		private bool _isInitialize = false;

		private float _fillColorTime;
		private float _drawApplyTime;

		private bool _applyPixelsNextFrame = false;

		#endregion

		public bool IsInitialize => _isInitialize;
		public Texture2D PaintTexture => _baseTexture;

		public Color Color { set { _color = value; } }
		public bool IsEraser { get { return _isEraser; } set { _isEraser = value; } }


		private void Start()
		{
			_nodes = gameObject.AddComponent<Nodes>();
			_camera = Camera.main;

			if (_isAutoInit)
			{
				Initialize();
			}
		}
		private void Update()
		{
			if (_applyPixelsNextFrame)
			{
				Apply();
				_applyPixelsNextFrame = false;
			}
		}


		#region public functions

		public void Initialize(Texture2D drawnTex = null)
		{

			if (_baseTexture != null)
			{
				Destroy(_baseTexture);
				_baseTexture = null;
			}

			_sourceWidth = _coloredTexture.width;
			_sourceHeight = _coloredTexture.height;

			_baseTexture = new Texture2D(_coloredTexture.width, _coloredTexture.height, TextureFormat.RGBA32, false);
			_baseTexture.wrapMode = TextureWrapMode.Clamp;

			Renderer render = GetComponent<Renderer>();
			render.SetPropertyBlock(null);
			render.material.mainTexture = _baseTexture;

			_pixels = new byte[_coloredTexture.width * _coloredTexture.height * 4];
			_coloredTextureColors = _coloredTexture.GetPixels32();

			// copy
			if (drawnTex)
			{
				Color32[] drawnColors = drawnTex.GetPixels32();

				for (int i = 0; i < drawnColors.Length; ++i)
                {
					int index = i * 4;
					_pixels[index + 0] = drawnColors[i].r;
					_pixels[index + 1] = drawnColors[i].g;
					_pixels[index + 2] = drawnColors[i].b;
					_pixels[index + 3] = drawnColors[i].a;
				}
			}

			_nodes.Initialize(_sourceWidth, _sourceHeight);

			Apply();

			_isInitialize = true;
		}


		public void Draw(Vector3 screenPos)
		{
			RaycastHit hit;
			Ray ray = _camera.ScreenPointToRay(screenPos);

			if (Physics.Raycast(ray, out hit))
			{
				Vector2 uv = SpriteHitPointToUV(hit.point);

				if (!_isDown)
				{
					_isDown = true;
					_lastPosition = uv;

					if (_haveMaskColor)
					{
						int x = (int)(uv.x * _baseTexture.width);
						int y = (int)(uv.y * _baseTexture.height);
						x = Mathf.Clamp(x, 0, _baseTexture.width - 1);
						y = Mathf.Clamp(y, 0, _baseTexture.height - 1);

						_downColor = _coloredTextureColors[y * _baseTexture.width + x]; //source.GetPixel (x, y);
					}
				}

				LerpDraw(uv, _lastPosition);

				if (Time.realtimeSinceStartup - _drawApplyTime > 0.05f)
				{
					Apply();
					_drawApplyTime = Time.realtimeSinceStartup;
				}

				_lastPosition = uv;
			}
		}
		public void Fill(Vector3 pos)
		{
			Color32 color = new Color32();
			CheckDrawnColor(Input.mousePosition, ref color);

			if (color.a < 10)
				return;

			if (Time.realtimeSinceStartup - _fillColorTime < 0.25f)
				return;

			_fillColorTime = Time.realtimeSinceStartup;

			RaycastHit hit;
			Ray ray = _camera.ScreenPointToRay(pos);

			if (Physics.Raycast(ray, out hit))
			{
				Vector2 uv = SpriteHitPointToUV(hit.point);
				if (_haveMaskColor)
				{
					int x = (int)(uv.x * _baseTexture.width);
					int y = (int)(uv.y * _baseTexture.height);
					x = Mathf.Clamp(x, 0, (int)_sourceWidth - 1);
					y = Mathf.Clamp(y, 0, (int)_sourceHeight - 1);

					_downColor = _coloredTextureColors[y * _baseTexture.width + x];//source.GetPixel (x, y);

					if (_downColor.a > 250)
					{

						System.Threading.Thread thread = new System.Threading.Thread(delegate (object obj)
						{
							lock (_pixels)
							{
								DrawByClosedList(_nodes.Search(x, y, _downColor, _coloredTextureColors), _downColor, _color);
							}
						});

						thread.Start();
					}
				}
			}
		}

		public void DrawEnd()
		{
			_isDown = false;

			Apply();
		}

		public void ClearImage()
		{
			for (int i = 0; i < _pixels.Length; ++i)
            {
				_pixels[i] = 0;
            }

			_nodes.Clear();

			Apply();
		}
        #endregion public functions

        #region private functions

        private void LerpDraw(Vector2 current, Vector2 previous)
		{
			if (current == previous) return;

			if (_haveMaskColor && _downColor.a == 0) return;

			Thread thread = new Thread(() =>
			{
				lock (_pixels)
				{
					float distance = Vector2.Distance(current, previous);
					Vector2 difference = current - previous;

					for (float i = 0; i < distance; i += _lerpDamp)
					{
						Draw(previous + difference * (i / distance));
					}
				}

			});

			thread.Start();
		}

		private void Draw(Vector2 textureCoord)
		{
			int startX = Mathf.FloorToInt((textureCoord.x * _sourceWidth) - _brushSize / 2);
			int startY = Mathf.FloorToInt((textureCoord.y * _sourceHeight) - _brushSize / 2);

			DrawCircle(startX, startY);
		}

		private void DrawCircle(int x, int y)
		{
			int radiusSquare = _brushSize * _brushSize;

			for (int a = -_brushSize; a <= _brushSize; ++a)
			{
				for (int b = -_brushSize; b <= _brushSize; ++b)
                {
					int currentX = x + a;
					int currentY = y + b;

					if (0 > currentX || currentX >= _sourceWidth || 0 > currentY || currentY >= _sourceHeight) continue;
					if ((a * a) + (b * b) > radiusSquare) continue;

					int index = (_sourceWidth * (currentY) + currentX) * 4;

					if (_isEraser)
					{
						_pixels[index + 3] = 0;
					}
					else
					{
						if (_haveMaskColor)
						{
							Color32 coloredPixel = _coloredTextureColors[index / 4];

							if (coloredPixel.r != _downColor.r || coloredPixel.g != _downColor.g || coloredPixel.b != _downColor.b)
								continue;
						}

						_pixels[index] = _color.r;
						_pixels[index + 1] = _color.g;
						_pixels[index + 2] = _color.b;
						_pixels[index + 3] = _color.a;
					}
				}
			}
		}


		private Vector2 SpriteHitPointToUV(Vector3 hitPoint)
		{
			Vector3 localPos = transform.InverseTransformPoint(hitPoint);

			localPos *= 100f;
			localPos.x += _sourceWidth * 0.5f;
			localPos.y += _sourceHeight * 0.5f;

			return new Vector2(localPos.x / _sourceWidth, localPos.y / _sourceHeight);
		}
		private bool CheckDrawnColor(Vector3 screenPosition, ref Color32 color)
		{

			RaycastHit hit;
			Ray ray = _camera.ScreenPointToRay(screenPosition);

			if (Physics.Raycast(ray, out hit))
			{
				Vector2 uv = SpriteHitPointToUV(hit.point);

				int x = (int)(uv.x * _baseTexture.width);
				int y = (int)(uv.y * _baseTexture.height);
				x = Mathf.Clamp(x, 0, _baseTexture.width - 1);
				y = Mathf.Clamp(y, 0, _baseTexture.height - 1);

				color = _coloredTextureColors[y * _baseTexture.width + x];
				return true;
			}
			return false;
		}
		private void DrawByClosedList(List<Node> closedList, Color32 defaultColor, Color32 fillColor)
		{
			if (closedList.Count <= 0) return;

			for (int i = 0; i < closedList.Count; ++i)
			{
				Node node = closedList[i];
				int index = Mathf.FloorToInt(node.UV.y * _sourceWidth + node.UV.x) * 4;

				if (_isEraser)
				{
					_pixels[index + 3] = 0;
				}
				else
				{
					_pixels[index] = fillColor.r;
					_pixels[index + 1] = fillColor.g;
					_pixels[index + 2] = fillColor.b;
					_pixels[index + 3] = defaultColor.a;
				}
			}

			_applyPixelsNextFrame = true;
		}

		private void Apply()
        {
			_baseTexture.LoadRawTextureData(_pixels);
			_baseTexture.Apply(false);
		}

        #endregion

	}
}