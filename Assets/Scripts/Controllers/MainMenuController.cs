using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
	private void Start()
	{
		Screen.orientation = ScreenOrientation.Portrait;
	}

	public void GoPoolVisualizationScene()
	{
		SceneManager.LoadScene("CatalogScene", LoadSceneMode.Single);
	}
	public void GoArRuler()
	{
		SceneManager.LoadScene("MeasureScene", LoadSceneMode.Single);
	}

	public void GoGalleryScene()
	{
		SceneManager.LoadScene("GalleryScene", LoadSceneMode.Single);
	}
}
