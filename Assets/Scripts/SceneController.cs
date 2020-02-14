using Lean.Touch;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
	// UI
	public Text debugText;
	public Camera arCamera;

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

	private void Update()
	{
		// check touch raycasts
		if ((Input.GetTouch(0).phase == TouchPhase.Stationary) || (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(0).deltaPosition.magnitude < 1.2f))
		{
			Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo))
			{
				// distancia entre la cámara y el objeto con el que colisiona el hit del raycast
				debugText.text = $"{Math.Round(hitInfo.distance, 2, MidpointRounding.AwayFromZero).ToString()} meters.";
			}
			else
			{
				debugText.text = "";
			}
		}
	}

	public void OnContentPlaced()
	{
		debugText.text = "CONTENT PLACED!";
		toolTip.SetActive(false);
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
		CurrentItem.GetComponent<LeanPinchScale>().enabled = false;
		CurrentItem.GetComponent<LeanTwistRotateAxis>().enabled = false;
		modelController.model = null;
		CloseControls();
	}

	public void ProcessHitTestResult()
	{
		HideSurfaceLoading();
		ShowCatalogButton();
		ShowTapScreenToolTip();
	}

	public void OnCatalogItemClicked(string prefabName)
	{
		GameObject prefab = Resources.Load($"Prefabs/{prefabName}") as GameObject;
		CurrentItem = Instantiate(prefab);

		GameObject.Find("ToolTip").GetComponent<Animator>().SetTrigger("ShouldShow");
	}

	private void SetUpCurrentItem()
	{
		CurrentItem.transform.parent = GameObject.Find("Ground Plane Stage").transform;
		CurrentItem.transform.localPosition = new Vector3(0, 0, 0);
		CurrentItem.transform.localScale = new Vector3(1, 1, 1);

		if (previousItem != null)
		{
			previousItem.GetComponent<LeanPinchScale>().enabled = false;
			previousItem.GetComponent<LeanTwistRotateAxis>().enabled = false;
		}

		CurrentItem.AddComponent<LeanPinchScale>();
		CurrentItem.AddComponent<LeanTwistRotateAxis>();
		modelController.model = CurrentItem;
	}
}