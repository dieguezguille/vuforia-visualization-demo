using Lean.Touch;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

namespace Assets.Scripts.Controllers
{
	public class CatalogSceneController : MonoBehaviour
	{
		public Text debugText;
		public Camera arCamera;

		private GameObject previousItem;
		private GameObject currentItem;

		private ModelController modelController;

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
			Screen.orientation = ScreenOrientation.Portrait;

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
			catch (Exception) { }
		}

		public void ResetScene()
		{
			SceneManager.LoadScene("CatalogScene", LoadSceneMode.Single);
		}

		public void GoMainMenu()
		{
			SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
		}

		public void OnContentPlaced()
		{
			try
			{
				debugText.text = "Stage deployed succesfully.";
				toolTip.SetActive(false);
				ShowCatalogButton();
				DisableStagePlacement();
			}
			catch (Exception) { }
		}

		private void DisableStagePlacement()
		{
			planeFinder.GetComponent<ContentPositioningBehaviour>().AnchorStage = null;
		}

		public void InitializeDefaultScene()
		{
			try
			{
				OpenCatalog();
				ShowSurfaceLoading();
			}
			catch (Exception) { }
		}

		public void ShowSurfaceLoading()
		{
			try
			{
				loading.GetComponent<Animator>().SetBool("isLoading", true);
			}
			catch (Exception) { }
		}

		public void HideSurfaceLoading()
		{
			try
			{
				loading.GetComponent<Animator>().SetBool("isLoading", false);
			}
			catch (Exception) { }
		}

		public void ShowTapScreenToolTip()
		{
			try
			{
				toolTip.GetComponent<Animator>().SetBool("isShown", true);
			}
			catch (Exception) { }
		}

		public void HideTapScreenToolTip()
		{
			try
			{
				toolTip.GetComponent<Animator>().SetBool("isShown", false);
			}
			catch (Exception) { }
		}

		public void OpenControls()
		{
			try
			{
				controls.GetComponent<Animator>().SetBool("isShown", true);
			}
			catch (Exception) { }
		}

		public void CloseControls()
		{
			try
			{
				controls.GetComponent<Animator>().SetBool("isShown", false);
			}
			catch (Exception) { }
		}

		public void OpenCatalog()
		{
			try
			{
				catalog.GetComponent<Animator>().SetBool("isMenuOpen", true);
			}
			catch (Exception) { }
		}

		public void CloseCatalog()
		{
			try
			{
				catalog.GetComponent<Animator>().SetBool("isMenuOpen", false);
			}
			catch (Exception) { }
		}

		public void ShowCatalogButton()
		{
			try
			{
				var catalogButton = GameObject.Find("OpenCatalogButton");
				catalogButton.GetComponent<Animator>().SetBool("isMenuOpen", true);
			}
			catch (Exception) { }
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
			catch (Exception) { }
		}

		public void ProcessHitTestResult(HitTestResult result)
		{
			try
			{
				HideSurfaceLoading();
				ShowTapScreenToolTip();
			}
			catch (Exception) { }
		}

		public void OnCatalogItemClicked(string prefabName)
		{
			try
			{
				GameObject prefab = Resources.Load($"Prefabs/{prefabName}") as GameObject;
				CurrentItem = Instantiate(prefab);
				GameObject.Find("ToolTip").GetComponent<Animator>().SetTrigger("ShouldShow");
			}
			catch (Exception) { }
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
			catch (Exception) { }
		}
	}
}