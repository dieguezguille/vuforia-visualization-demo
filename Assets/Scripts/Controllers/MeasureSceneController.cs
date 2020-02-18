using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

namespace Assets.Scripts.Controllers
{
	public class MeasureSceneController : MonoBehaviour, ISceneController
	{
		public Text debugText;
		public Camera arCamera;

		private GameObject loading;
		private GameObject toolTip;

		private GameObject currentItem;
		private GameObject previousItem;
		private GameObject groundPlane;
		private GameObject planeFinder;

		public List<GameObject> ARMarkerList { get; set; }

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
			}
		}

		void Start()
		{
			try
			{
				loading = GameObject.Find("Loading");
				toolTip = GameObject.Find("TapScreenToolTip");
				groundPlane = GameObject.Find("Ground Plane Stage");
				planeFinder = GameObject.Find("Plane Finder");

				ARMarkerList = new List<GameObject>();
			}
			catch (Exception ex) { }
		}

		void Update()
		{
			if (ARMarkerList != null && ARMarkerList.Count > 1)
			{
				CalculateDistance();
			}
		}

		public void ResetScene()
		{
			SceneManager.LoadScene("MeasureScene", LoadSceneMode.Single);
		}

		public void GoMainMenu()
		{
			SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
		}

		public void InstantiateMarker()
		{
			GameObject prefab = Resources.Load($"Prefabs/Bandera") as GameObject;
			ARMarkerList.Add(CurrentItem);
			CurrentItem = Instantiate(prefab);
			debugText.text = $"There are {groundPlane.transform.childCount - 1} markers in total.";
		}

		public void InitializeDefaultScene()
		{
			try
			{
				ShowSurfaceLoading();
			}
			catch (Exception) { }
		}

		public void OnContentPlaced()
		{
			try
			{
				debugText.text = "Stage deployed succesfully.";
				toolTip.SetActive(false);
				DisableStagePlacement();
				ShowAddMarkerButton();
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

		private void CalculateDistance()
		{
			var distance = Vector3.Distance(CurrentItem.transform.position, previousItem.transform.position);

			if (distance > 1)
			{
				debugText.text = $"Distance: {Math.Round(distance, 2)} meters.";
			}
			else
			{
				debugText.text = $"Distance: {Math.Round(distance, 2) * 100} centimeters.";
			}
		}

		private void DisableStagePlacement()
		{
			planeFinder.GetComponent<ContentPositioningBehaviour>().AnchorStage = null;
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

		public void ShowAddMarkerButton()
		{
			try
			{
				var button = GameObject.Find("AddMarkerButton");
				button.GetComponent<Animator>().SetBool("isShown", true);
			}
			catch (Exception) { }
		}

		public void HideAddMarkerButton()
		{
			try
			{
				var button = GameObject.Find("AddMarkerButton");
				button.GetComponent<Animator>().SetBool("isShown", false);
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
			}
			catch (Exception ex) { }
		}
	}
}
