using UnityEngine;

public class UI : MonoBehaviour
{
	public static UI Instance;
	void Awake()
	{
		Instance = this;
	}

	[SerializeField] private GameObject _eraser;
	[SerializeField] private GameObject _brush;
	[SerializeField] private GameObject _pouring;
	[Space]
	[SerializeField] private RectTransform _selectColorFrame;


	public void RefreshBottomPanel()
    {
		_eraser.transform.GetChild(0).gameObject.SetActive(DrawManager.Instance.IsEraser);

		_brush.transform.GetChild(0).gameObject.SetActive(DrawManager.Instance.SelectedDrawMode == DrawManager.DrawMode.Draw);
		_pouring.transform.GetChild(0).gameObject.SetActive(DrawManager.Instance.SelectedDrawMode == DrawManager.DrawMode.Fill);
	}

	public void SelectColor(RectTransform buttonTransform)
    {
		_selectColorFrame.position = buttonTransform.position;
		_selectColorFrame.parent = buttonTransform;
    }
}
