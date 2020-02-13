using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
	public Text debugText;

	// INTERNAL
	private GameObject previousItem;
	private GameObject currentItem;

	// CONTROLLERS
	private ModelController modelController;

	// UI CONTROLS
	private GameObject loading;
	private GameObject toolTip;
	private GameObject controls;
	private GameObject catalog;

	public GameObject CurrentItem
	{
		get
		{
			return currentItem;
		}
		set
		{
			previousItem = currentItem;

			currentItem = value;
			SetUpCurrentItem();
			CloseCatalog();
			OpenControls();
		}
	}

	private void Start()
	{
		modelController = GameObject.Find("ModelController").GetComponent<ModelController>();
		loading = GameObject.Find("Loading");
		toolTip = GameObject.Find("TapScreenToolTip");
		controls = GameObject.Find("Controls");
		catalog = GameObject.Find("Catalog");
	}

	public void InitializeDefaultScene()
	{
		OpenCatalog();
		ShowSurfaceLoading();
	}

	public void ShowSurfaceLoading()
	{
		loading.GetComponent<Animator>().SetBool("isLoading", true);
	}

	public void HideSurfaceLoading()
	{
		loading.GetComponent<Animator>().SetBool("isLoading", false);
	}

	public void ShowTapScreenToolTip()
	{
		toolTip.GetComponent<Animator>().SetBool("isShown", true);
	}

	public void HideTapScreenToolTip()
	{
		toolTip.GetComponent<Animator>().SetBool("isShown", false);
	}

	public void OpenControls()
	{
		controls.GetComponent<Animator>().SetBool("isShown", true);
	}

	public void CloseControls()
	{
		controls.GetComponent<Animator>().SetBool("isShown", false);
	}

	public void OpenCatalog()
	{
		catalog.GetComponent<Animator>().SetBool("isMenuOpen", true);
	}

	public void CloseCatalog()
	{
		catalog.GetComponent<Animator>().SetBool("isMenuOpen", false);
	}

	public void ShowCatalogButton()
	{
		var catalogButton = GameObject.Find("OpenCatalogButton");
		catalogButton.GetComponent<Animator>().SetBool("isMenuOpen", true);
	}

	public void FinishEditing()
	{
		// disable touch scripts on current item
		CurrentItem.GetComponent<LeanPinchScale>().enabled = false;
		CurrentItem.GetComponent<LeanTwistRotateAxis>().enabled = false;

		// override position controls
		modelController.model = null;

		CloseControls();
	}

	public void ProcessHitTestResult()
	{
		HideSurfaceLoading();
		ShowCatalogButton();
	}

	public void OnCatalogItemClicked(string prefabName)
	{
		//load and instantiate selected prefab
		GameObject prefab = Resources.Load($"Prefabs/{prefabName}") as GameObject;
		CurrentItem = Instantiate(prefab);

		//show tooltip
		GameObject.Find("ToolTip").GetComponent<Animator>().SetTrigger("ShouldShow");
	}

	private void SetUpCurrentItem()
	{
		// make prefab a children of ground scene
		CurrentItem.transform.parent = GameObject.Find("Ground Plane Stage").transform;

		// set x, y, z of instantiated item
		CurrentItem.transform.localPosition = new Vector3(0, 0, 0);

		// set scale of instantiated item
		CurrentItem.transform.localScale = new Vector3(1, 1, 1);

		// disable movement scripts on previous item if exists
		if (previousItem != null)
		{
			previousItem.GetComponent<LeanPinchScale>().enabled = false;
			previousItem.GetComponent<LeanTwistRotateAxis>().enabled = false;
		}

		// add movement scripts to current item
		CurrentItem.AddComponent<LeanPinchScale>();
		CurrentItem.AddComponent<LeanTwistRotateAxis>();

		// override position controls
		modelController.model = CurrentItem;
	}
}