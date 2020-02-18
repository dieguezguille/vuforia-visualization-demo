using Lean.Touch;
using System;
using System.Collections;
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
		}
		catch (Exception ex)
		{
			//debugText.text = ex.Message;
		}
	}

	private void Update()
	{
		if ((Input.GetTouch(0).phase == TouchPhase.Stationary) || (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(0).deltaPosition.magnitude < 1.2f))
		{
			Ray ray = arCamera.ScreenPointToRay(arCamera.transform.rotation.eulerAngles);
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

		if (isMeasureModeEnabled)
		{
			// use a coroutine to load the Scene in the background
			StartCoroutine(LoadMeasureSceneAsync());
		}
	}

	public void InstantiateMarker()
	{
		GameObject prefab = Resources.Load($"Prefabs/Bandera") as GameObject;
		CurrentItem = Instantiate(prefab);

		debugText.text = $"{groundPlane.transform.childCount} elementos en total";
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
			debugText.text = "CONTENT PLACED!";
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
		planeFinder.GetComponent<ContentPositioningBehaviour>().enabled = false;
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

			// TODO ubicar el item instanciado exactamente sobre el ground plane indicator (ground plane position?)

			var pos = groundPlane.transform.localPosition;
			CurrentItem.transform.position = planeFinder.GetComponent<PlaneFinderBehaviour>().PlaneIndicator.transform.localPosition;

			debugText.text = CurrentItem.transform.position.ToString();

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