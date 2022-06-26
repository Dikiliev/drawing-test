using System.IO;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    public static string StreamingAssetPath
    {
        get
        {
            #if UNITY_EDITOR
            return "file://" + Application.streamingAssetsPath + "/";

            #elif UNITY_ANDROID
            return Application.streamingAssetsPath + "/";

            #else
            return "file://" + Application.streamingAssetsPath + "/";

            #endif
        }
    }

    public static string PersistentDataPath(int level, int itemIndex = 0)
    {
        return $"{Application.persistentDataPath}/{level}_{itemIndex}.png";
    }

	public static Texture2D LoadPaint(int level)
	{
		string path = PersistentDataPath(Data.Level);

		if (!File.Exists(path))
			return null;

		byte[] bytes = File.ReadAllBytes(path);
		Texture2D loadTexture = new Texture2D(Data.Width, Data.Height);
		loadTexture.LoadImage(bytes);

		return loadTexture;
	}

	public static void SavePaint(Texture2D texture)
	{
		if (texture == null) return;

		byte[] bytes = texture.EncodeToPNG();
		string path = PersistentDataPath(Data.Level);

		File.WriteAllBytes(path, bytes);

		Debug.LogWarning($"Save: {path}");
	}
}
