using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GallerySceneController : MonoBehaviour
{
	[SerializeField]
	GameObject image;
	string[] files = null;
	int whichScreenShotIsShown = 0;

	public void GoMainMenu()
	{
		SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
	}

	void Start()
	{
		Screen.orientation = ScreenOrientation.Portrait;

		files = Directory.GetFiles(Application.persistentDataPath + "/MyScreenshots/", "*.jpg");
		if (files.Length > 0)
		{
			GetPictureAndShowIt();
		}
	}

	void GetPictureAndShowIt()
	{
		string pathToFile = files[whichScreenShotIsShown];
		Texture2D texture = GetScreenshotImage(pathToFile);
		Sprite sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
			new Vector2(0.5f, 0.5f));
		image.GetComponent<Image>().sprite = sp;
	}

	Texture2D GetScreenshotImage(string filePath)
	{
		Texture2D texture = null;
		byte[] fileBytes;
		if (File.Exists(filePath))
		{
			fileBytes = File.ReadAllBytes(filePath);
			texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
			texture.LoadImage(fileBytes);
		}
		return texture;
	}


	public void LoadPreviousScreenshot()
	{
		if (files.Length > 0)
		{
			whichScreenShotIsShown -= 1;
			if (whichScreenShotIsShown < 0)
				whichScreenShotIsShown = files.Length - 1;
			GetPictureAndShowIt();
		}
	}
	public void LoadNextScreenshot()
	{
		if (files.Length > 0)
		{
			whichScreenShotIsShown += 1;
			if (whichScreenShotIsShown > files.Length - 1)
				whichScreenShotIsShown = 0;
			GetPictureAndShowIt();
		}
	}
}
