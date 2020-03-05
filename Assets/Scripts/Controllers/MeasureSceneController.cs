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
		//
		public Text debugText;
		public Camera arCamera;
		//
		private GameObject loading;
		private GameObject toolTip;
		//
		private GameObject currentMarker;
		private GameObject PreviousMarker;
		private GameObject groundPlane;
		private GameObject planeFinder;
		private GameObject segmentLine;
		//
		private bool shouldUpdateMetrics = false;
		private Color lineRendererMaterial;
		private Material surfaceAreaMaterial;
		private float surfaceArea;

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
				surfaceAreaMaterial = Resources.Load("Materials/SurfaceArea") as Material;
				segmentLine = Resources.Load("Prefabs/SegmentLine") as GameObject;
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

		private void CreateSegmentLine(Vector3 initialPos, Vector3 finalPos)
		{
			try
			{
				var line = Instantiate(segmentLine);
				line.transform.parent = groundPlane.transform;

				Vector3 between = finalPos - initialPos;
				float distance = between.magnitude;
				line.transform.localScale = new Vector3(0.01f, 0.01f, distance);
				Vector3 pos = initialPos + (between / 2);
				line.transform.position = pos;
				line.transform.LookAt(finalPos);

				// TODO: CREATE TEXT OVER LINE

				//

				//

				//
			}
			catch (Exception ex)
			{
				debugText.text = $"{ex.Message}";
			}
		}

		IEnumerator UpdateMetrics()
		{
			// last segment distance
			//var lastSegmentDistance = MarkerList.Last().LastSegmentDistance;
			//debugText.text = lastSegmentDistance > 1 ? $"LAST: {Math.Round(lastSegmentDistance, 2)} mts." : $"LAST: {Math.Round(lastSegmentDistance, 2) * 100} cms.";

			// total distance
			var perimeterDistance = 0.0f;
			foreach (var marker in MarkerList)
			{
				var dist = marker.LastSegmentDistance;
				perimeterDistance += dist;
			}

			debugText.text = perimeterDistance > 1 ? $"TOTAL: {Math.Round(perimeterDistance, 2)} mts." : $" TOTAL: {Math.Round(perimeterDistance, 2) * 100} cms.";

			//angle
			//var from = CurrentMarker.transform.position - PreviousMarker.transform.position;
			//var to = Vector3.ProjectOnPlane(from, -CurrentMarker.transform.up);
			//var angle = Vector3.Angle(from, to);

			//debugText.text += $" ANGLE: {Math.Round(angle, 2)} dgs.";

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

				MarkerList.Add(newMarker);

				if (MarkerList != null && MarkerList.Count > 1)
				{
					CreateSegmentLine(PreviousMarker.transform.position, CurrentMarker.transform.position);
				}

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
				CreateSegmentLine(initialPos, finalPos);

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
			// create points in 2d space
			Vector2[] points2D = new Vector2[MarkerList.Count - 1];

			for (int i = 0; i < MarkerList.Count - 1; i++)
			{
				points2D[i] = new Vector2(MarkerList[i].Position.x, MarkerList[i].Position.z);
			}

			// set points and triangulate
			Triangulator.Instance.SetPoints(points2D);
			int[] triangles = Triangulator.Instance.Triangulate();

			// create vertices in 3d space
			Vector3[] vertices3D = new Vector3[points2D.Length];

			for (int i = 0; i < vertices3D.Length; i++)
			{
				vertices3D[i] = new Vector3(points2D[i].x, MarkerList[i].Position.y + 0.005f, points2D[i].y);
			}

			// create mesh and assign props
			Mesh msh = new Mesh();
			msh.vertices = vertices3D;
			msh.triangles = triangles;
			msh.RecalculateNormals();
			msh.RecalculateBounds();

			// create container gameobject
			GameObject emptyGo = new GameObject();
			emptyGo.name = "Area";
			emptyGo.transform.parent = groundPlane.transform;

			MeshRenderer meshRenderer = emptyGo.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
			meshRenderer.enabled = true;
			meshRenderer.material = surfaceAreaMaterial;
			meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			meshRenderer.receiveShadows = false;

			MeshFilter filter = emptyGo.AddComponent(typeof(MeshFilter)) as MeshFilter;
			filter.mesh = msh;

			// calculate area
			surfaceArea = CalculateSurfaceArea(msh);
			debugText.text += surfaceArea > 1 ? $" SURFACE: {Math.Round(surfaceArea, 2)} sq. mts." : $" SURFACE: {Math.Round(surfaceArea, 2) * 100} sq. cms.";
		}

		private float CalculateSurfaceArea(Mesh m)
		{
			Vector3[] mVertices = m.vertices;
			Vector3 result = Vector3.zero;

			for (int p = mVertices.Length - 1, q = 0; q < mVertices.Length; p = q++)
			{
				result += Vector3.Cross(mVertices[q], mVertices[p]);
			}

			result *= 0.5f;
			return result.magnitude;
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
