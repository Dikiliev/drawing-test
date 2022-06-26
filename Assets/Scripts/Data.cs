using UnityEngine;

public class Data
{
    public static int Level
    {
        get
        {
            return PlayerPrefs.GetInt("Level", 1);
        }
        set
        {
            PlayerPrefs.GetInt("Level", value);
        }
    }

    public const int Width = 1000;
    public const int Height = 1000;
}
