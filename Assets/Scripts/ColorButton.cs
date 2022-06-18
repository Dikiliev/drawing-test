using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class ColorButton : MonoBehaviour
{
    [SerializeField] private Color _color;

    private void OnValidate()
    {
        GetComponent<Image>().color = _color;
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(Select);
    }

    public void Select()
    {
        FindObjectOfType<Painter>().SetColor(_color);
    }
}
