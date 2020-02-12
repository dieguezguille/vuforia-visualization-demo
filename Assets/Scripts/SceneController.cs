using Lean.Touch;
using System;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class SceneController : MonoBehaviour
{
	public Text debugText;

	private GameObject previousItem;
	private GameObject currentItem;
	private ModelController modelController;

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
	}

	public void InitializeDefaultScene()
	{
		OpenCatalog();
		ShowSurfaceLoading();
	}

	public void ShowSurfaceLoading()
	{
		var loading = GameObject.Find("Loading");
		loading.GetComponent<Animator>().SetBool("isLoading", true);
	}

	public void HideSurfaceLoading()
	{
		var loading = GameObject.Find("Loading");
		loading.GetComponent<Animator>().SetBool("isLoading", false);
	}

	public void ShowTapScreenToolTip()
	{
		var tooltip = GameObject.Find("TapScreenToolTip");
		tooltip.GetComponent<Animator>().SetBool("isShown", true);
	}

	public void HideTapScreenToolTip()
	{
		var tooltip = GameObject.Find("TapScreenToolTip");
		tooltip.GetComponent<Animator>().SetBool("isShown", false);
	}

	public void OpenControls()
	{
		var controls = GameObject.Find("Controls");
		controls.GetComponent<Animator>().SetBool("isMenuOpen", true);
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

	public void OpenCatalog()
	{
		var catalog = GameObject.Find("Catalog");
		catalog.GetComponent<Animator>().SetBool("isMenuOpen", true);
	}

	public void CloseCatalog()
	{
		var catalog = GameObject.Find("Catalog");
		catalog.GetComponent<Animator>().SetBool("isMenuOpen", false);
	}

	public void ProcessHitTestResult(HitTestResult result)
	{
		HideSurfaceLoading();
		ShowTapScreenToolTip();
	}

	public void ShowCatalogButton()
	{
		var catalogButton = GameObject.Find("OpenCatalogButton");
		catalogButton.GetComponent<Animator>().SetBool("isMenuOpen", true);
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

	private void CloseControls()
	{
		var controls = GameObject.Find("Controls");
		controls.GetComponent<Animator>().SetBool("isMenuOpen", false);
	}
}
