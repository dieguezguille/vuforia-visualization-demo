using Lean.Touch;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

public class SceneController : MonoBehaviour
{
	// UI
	public Text debugText;
	public Camera arCamera;

	// GO
	//public GameObject terrain;

	// INTERNAL
	private GameObject previousItem;
	private GameObject currentItem;
	private bool isMeasureModeEnabled = false;
	private Vector3 hitTestResultPosition;

	// CONTROLLERS
	private ModelController modelController;

	// UI CONTROLS
	private GameObject loading;
	private GameObject toolTip;
	private GameObject controls;
	private GameObject catalog;
	private GameObject groundPlane;
	private GameObject planeFinder;

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

	public List<GameObject> ARMarkerList { get; set; }

	private void Start()
	{
		try
		{
			modelController = GameObject.Find("ModelController").GetComponent<ModelController>();
			loading = GameObject.Find("Loading");
			toolTip = GameObject.Find("TapScreenToolTip");
			controls = GameObject.Find("Controls");
			catalog = GameObject.Find("Catalog");
			groundPlane = GameObject.Find("Ground Plane Stage");
			planeFinder = GameObject.Find("Plane Finder");

			ARMarkerList = new List<GameObject>();
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	private void Update()
	{
		if (isMeasureModeEnabled)
		{
			StartCoroutine(LoadMeasureSceneAsync());
		}

		if (ARMarkerList != null && ARMarkerList.Count > 1)
		{
			var distance = Vector3.Distance(CurrentItem.transform.position, previousItem.transform.position);
			debugText.text = distance.ToString();
		}
	}

	public void InstantiateMarker()
	{
		GameObject prefab = Resources.Load($"Prefabs/Bandera") as GameObject;
		CurrentItem = Instantiate(prefab);
		ARMarkerList.Add(CurrentItem);
		debugText.text = $"There are {groundPlane.transform.childCount - 1} markers in total.";
	}

	IEnumerator LoadMeasureSceneAsync()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MeasureScene", LoadSceneMode.Single);

		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

	public void OnContentPlaced()
	{
		try
		{
			debugText.text = "Stage deployed succesfully.";
			toolTip.SetActive(false);
			ShowCatalogButton();
			ShowStartMeasureButton();
			DisableStagePlacement();
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	private void DisableStagePlacement()
	{
		planeFinder.GetComponent<ContentPositioningBehaviour>().AnchorStage = null;
	}

	public void EnableMeasureMode()
	{
		isMeasureModeEnabled = true;
	}

	public void InitializeDefaultScene()
	{
		try
		{
			OpenCatalog();
			ShowSurfaceLoading();
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void ShowSurfaceLoading()
	{
		try
		{
			loading.GetComponent<Animator>().SetBool("isLoading", true);
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void HideSurfaceLoading()
	{
		try
		{
			loading.GetComponent<Animator>().SetBool("isLoading", false);
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void ShowTapScreenToolTip()
	{
		try
		{
			toolTip.GetComponent<Animator>().SetBool("isShown", true);
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void HideTapScreenToolTip()
	{
		try
		{
			toolTip.GetComponent<Animator>().SetBool("isShown", false);
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void OpenControls()
	{
		try
		{
			controls.GetComponent<Animator>().SetBool("isShown", true);
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void CloseControls()
	{
		try
		{
			controls.GetComponent<Animator>().SetBool("isShown", false);
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void OpenCatalog()
	{
		try
		{
			catalog.GetComponent<Animator>().SetBool("isMenuOpen", true);
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void CloseCatalog()
	{
		try
		{
			catalog.GetComponent<Animator>().SetBool("isMenuOpen", false);
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void ShowCatalogButton()
	{
		try
		{
			var catalogButton = GameObject.Find("OpenCatalogButton");
			catalogButton.GetComponent<Animator>().SetBool("isMenuOpen", true);
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void ShowStartMeasureButton()
	{
		try
		{
			var startMeasureButton = GameObject.Find("StartMeasureButton");
			startMeasureButton.GetComponent<Animator>().SetBool("isShown", true);
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void FinishEditing()
	{
		try
		{
			CurrentItem.GetComponent<LeanPinchScale>().enabled = false;
			CurrentItem.GetComponent<LeanTwistRotateAxis>().enabled = false;
			modelController.model = null;
			CloseControls();
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void ProcessHitTestResult(HitTestResult result)
	{
		try
		{
			hitTestResultPosition = result.Position;
			HideSurfaceLoading();
			ShowTapScreenToolTip();
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	public void OnCatalogItemClicked(string prefabName)
	{
		try
		{
			GameObject prefab = Resources.Load($"Prefabs/{prefabName}") as GameObject;
			CurrentItem = Instantiate(prefab);
			GameObject.Find("ToolTip").GetComponent<Animator>().SetTrigger("ShouldShow");
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	private void SetUpCurrentItem()
	{
		try
		{
			CurrentItem.transform.parent = groundPlane.transform;
			CurrentItem.transform.position = planeFinder.GetComponent<PlaneFinderBehaviour>().PlaneIndicator.transform.localPosition;
			CurrentItem.transform.localScale = CurrentItem.transform.lossyScale;

			if (previousItem != null)
			{
				previousItem.GetComponent<LeanPinchScale>().enabled = false;
				previousItem.GetComponent<LeanTwistRotateAxis>().enabled = false;
			}

			CurrentItem.AddComponent<LeanPinchScale>();
			CurrentItem.AddComponent<LeanTwistRotateAxis>();

			modelController.model = CurrentItem;
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}
}