using Assets.Scripts.Models;
using Assets.Scripts.Support;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

namespace Assets.Scripts.Controllers
{
	public class MeasureSceneController : MonoBehaviour
	{
		public Text debugText;
		public Camera arCamera;

		private GameObject loading;
		private GameObject toolTip;

		private GameObject currentMarker;
		private GameObject PreviousMarker;
		private GameObject groundPlane;
		private GameObject planeFinder;
		private bool shouldUpdateMetrics = false;
		private Color lineRendererMaterial;

		public List<Marker> MarkerList { get; set; }

		public GameObject CurrentMarker
		{
			get
			{
				return currentMarker;
			}
			set
			{
				PreviousMarker = currentMarker;
				currentMarker = value;
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
				MarkerList = new List<Marker>();
				lineRendererMaterial = Color.red;
			}
			catch (Exception ex) { }
		}

		void Update()
		{
			if (shouldUpdateMetrics && MarkerList != null && MarkerList.Count > 1)
			{
				StartCoroutine(UpdateMetrics());
			}
			else
			{
				StopCoroutine(UpdateMetrics());
			}
		}



		private void CreateLineRenderer(Vector3 initialPos, Vector3 finalPos)
		{
			try
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
				lineRenderer.SetPosition(0, new Vector3(initialPos.x, initialPos.y + 0.07f, initialPos.z));
				lineRenderer.SetPosition(1, new Vector3(finalPos.x, finalPos.y + 0.07f, finalPos.z));
			}
			catch (Exception ex)
			{
				debugText.text = $"{ex.Message}";
			}
		}

		IEnumerator UpdateMetrics()
		{
			// last segment distance
			var lastSegmentDistance = MarkerList.Last().LastSegmentDistance;
			debugText.text = lastSegmentDistance > 1 ? $"LAST: {Math.Round(lastSegmentDistance, 2)} mts." : $"LAST: {Math.Round(lastSegmentDistance, 2) * 100} cms.";

			// total distance
			var perimeterDistance = 0.0f;
			foreach (var marker in MarkerList)
			{
				var dist = marker.LastSegmentDistance;
				perimeterDistance += dist;
			}

			debugText.text += perimeterDistance > 1 ? $" TOTAL: {Math.Round(perimeterDistance, 2)} mts." : $" TOTAL: {Math.Round(perimeterDistance, 2) * 100} cms.";

			// angle
			var from = CurrentMarker.transform.position - PreviousMarker.transform.position;
			var to = Vector3.ProjectOnPlane(from, -CurrentMarker.transform.up);
			var angle = Vector3.Angle(from, to);

			debugText.text += $" ANGLE: {Math.Round(angle, 2)} dgs.";

			yield return new WaitForSeconds(.1f);
		}

		public void ResetScene()
		{
			SceneManager.LoadScene("MeasureScene_Area", LoadSceneMode.Single);
		}

		public void GoMainMenu()
		{
			SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
		}

		public void CreateMarker()
		{
			try
			{
				GameObject prefab = Resources.Load($"Prefabs/Marker") as GameObject;
				CurrentMarker = Instantiate(prefab);

				var newMarker = new Marker()
				{
					Previous = (MarkerList != null && MarkerList.Count > 0) ? MarkerList.Last() : null,
					Position = CurrentMarker.transform.position,
				};

				if (MarkerList != null && MarkerList.Count > 1)
				{
					CreateLineRenderer(PreviousMarker.transform.position, CurrentMarker.transform.position);
				}

				MarkerList.Add(newMarker);
				shouldUpdateMetrics = true;
			}
			catch (Exception ex)
			{
				debugText.text = $"{ex.Message}";
			}
		}

		public void FinishAddingMarkers()
		{
			try
			{
				shouldUpdateMetrics = false;

				var lastMarker = new Marker()
				{
					Previous = MarkerList.Last(),
					Position = MarkerList.First().Position,
				};

				Vector3 initialPos = MarkerList.Last().Position;
				Vector3 finalPos = MarkerList.First().Position;
				CreateLineRenderer(initialPos, finalPos);

				MarkerList.Add(lastMarker);
				CreateMesh();
			}
			catch (Exception ex)
			{
				debugText.text = ex.Message;
				Debug.Log(ex);
			}
		}

		private void CreateMesh()
		{
			var vectorList = new List<Vector2>();

			foreach (Marker marker in MarkerList)
			{
				vectorList.Add(new Vector2(marker.Position.x, marker.Position.y));
			}

			Vector2[] vertices2D = vectorList.ToArray();

			//Vector2[] vertices2D = new Vector2[]
			//{
			//	new Vector2(0,0),
			//	new Vector2(0,50),
			//	new Vector2(50,50),
			//	new Vector2(50,100),
			//	new Vector2(0,100),
			//	new Vector2(0,150),
			//	new Vector2(150,150),
			//	new Vector2(150,100),
			//	new Vector2(100,100),
			//	new Vector2(100,50),
			//	new Vector2(150,50),
			//	new Vector2(150,0),
			//};

			Triangulator.Instance.SetPoints(vertices2D);
			int[] indices = Triangulator.Instance.Triangulate();

			Vector3[] vertices = new Vector3[vertices2D.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, MarkerList[i].Position.z);
			}

			Mesh msh = new Mesh();
			msh.vertices = vertices;
			msh.triangles = indices;
			msh.RecalculateNormals();
			msh.RecalculateBounds();

			GameObject emptyGo = new GameObject();
			emptyGo.name = "Area";
			emptyGo.transform.parent = groundPlane.transform;
			emptyGo.transform.localPosition = new Vector3(MarkerList.First().Position.x, 0.06f, MarkerList.First().Position.z);

			MeshRenderer meshRenderer = emptyGo.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
			meshRenderer.enabled = true;

			MeshFilter filter = emptyGo.AddComponent(typeof(MeshFilter)) as MeshFilter;
			filter.mesh = msh;

			debugText.text = "CREATED MESH!";
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
				ShowFinishButton();
			}
			catch (Exception) { }
		}

		private void ShowFinishButton()
		{
			try
			{
				var button = GameObject.Find("FinishButton");
				button.GetComponent<Animator>().SetBool("isShown", true);
			}
			catch (Exception) { }
		}

		private void HideFinishButton()
		{
			try
			{
				var button = GameObject.Find("FinishButton");
				button.GetComponent<Animator>().SetBool("isShown", false);
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
				CurrentMarker.transform.parent = groundPlane.transform;
				CurrentMarker.transform.position = planeFinder.GetComponent<PlaneFinderBehaviour>().PlaneIndicator.transform.localPosition;
				CurrentMarker.transform.localScale = CurrentMarker.transform.lossyScale;
			}
			catch (Exception ex) { }
		}
	}
}
