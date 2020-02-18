using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
	public void GoPoolVisualizationScene()
	{
		SceneManager.LoadScene("CatalogScene", LoadSceneMode.Single);
	}
	public void GoArRuler()
	{
		SceneManager.LoadScene("MeasureScene", LoadSceneMode.Single);
	}
}
