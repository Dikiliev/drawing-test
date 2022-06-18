using UnityEngine;

[RequireComponent(typeof(Painter))]
public class PainterConroller : MonoBehaviour
{
	private Painter _painter;
	private Camera _camera;

	private Vector2 _lastCoord;

	private void Start()
	{
		_camera = Camera.main;

		_painter = GetComponent<Painter>();
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
        {
			TouchDown();
		}

		if (Input.GetMouseButton(0))
		{
			Touched();
		}

		else if (Input.GetMouseButtonUp(0))
		{
			TouchUp();
		}
	}


	private void TouchDown()
    {
		Vector2Int pixelPoint = Vector2Int.RoundToInt(PixelPoint());
		_lastCoord = pixelPoint;

		_painter.SetDrawingPart(pixelPoint);

		if (_painter.SelectedTool == Painter.Tool.pouring)
		{
			_painter.Pour();
		}
	}
	private void Touched()
    {
		Vector2 pixelPoint = PixelPoint();

		if (pixelPoint.x <= -1) return;

		if (_painter.SelectedTool == Painter.Tool.drawing)
        {
			_painter.Draw(_lastCoord, pixelPoint);
		}

		_lastCoord = pixelPoint;
	}
	private void TouchUp()
    {
		_painter.SetDrawingPart(new Vector2Int(-1, -1));
	}

	private Vector2 PixelPoint()
    {
		RaycastHit hit;

		if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, 100))
		{
			Renderer renderer = hit.transform.GetComponent<Renderer>();

			if (renderer == null || renderer.sharedMaterial.mainTexture != _painter.Texture) return new Vector2(-1, -1);

			Texture2D texture = _painter.Texture;
			Vector2 pixelPoint = hit.textureCoord;

			pixelPoint.x *= texture.width;
			pixelPoint.y *= texture.height;

			return pixelPoint;
		}

		return new Vector2(-1, -1);
	}
}
