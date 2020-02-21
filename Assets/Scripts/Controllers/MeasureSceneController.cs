using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
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
		private Color lineRendererMaterial;

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
				lineRendererMaterial = Color.red;
			}
			catch (Exception ex) { }
		}

		void Update()
		{
			if (ARMarkerList != null && ARMarkerList.Count > 1)
			{
				ShowLastSegmentDistance();
				CreateLineRenderer();
			}
		}

		private void CreateLineRenderer()
		{
			GameObject newLine = new GameObject("Line");
			newLine.AddComponent<LineRenderer>();
			newLine.transform.parent = groundPlane.transform;

			var lineRenderer = newLine.GetComponent<LineRenderer>();
			lineRenderer.material.color = lineRendererMaterial;
			lineRenderer.startWidth = 0.02f;
			lineRenderer.endWidth = 0.02f;
			lineRenderer.useWorldSpace = true;
			lineRenderer.alignment = LineAlignment.TransformZ;
			lineRenderer.SetPosition(0, CurrentItem.GetComponent<Marker>().Position);
			lineRenderer.SetPosition(1, previousItem.GetComponent<Marker>().Position);
		}

		private void ShowLastSegmentDistance()
		{
			var lastSegmentDistance = CurrentItem.GetComponent<Marker>().LastSegmentDistance;

			if (lastSegmentDistance > 1)
			{
				debugText.text = $"Last segment is {Math.Round(lastSegmentDistance, 2)} meters.";
			}
			else
			{
				debugText.text = $"Last segment is: {Math.Round(lastSegmentDistance, 2) * 100} centimeters.";
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
			// instantiate prefab
			GameObject prefab = Resources.Load($"Prefabs/Marker") as GameObject;
			CurrentItem = Instantiate(prefab);

			// set prefab marker properties
			var marker = CurrentItem.GetComponent<Marker>();

			if (marker != null && previousItem != null)
			{
				marker.PreviousMarker = previousItem.GetComponent<Marker>();
			}

			CurrentItem.GetComponent<Marker>().Position = CurrentItem.transform.position;

			// add marker to list
			ARMarkerList.Add(CurrentItem);
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
