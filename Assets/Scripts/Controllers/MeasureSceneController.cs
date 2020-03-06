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
		#region Variables
		// WORLD GO's
		public Text debugText;
		public Camera arCamera;
		// UI
		private GameObject loading;
		private GameObject toolTip;
		// VUFORIA
		private GameObject currentMarker;
		private GameObject PreviousMarker;
		private GameObject groundPlane;
		private GameObject planeFinder;
		private GameObject segmentLine;
		// VARS
		private bool shouldUpdateMetrics = false;
		private Material surfaceAreaMaterial;
		private float surfaceArea;
		#endregion

		#region Properties
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
		#endregion

		void Start()
		{
			try
			{
				loading = GameObject.Find("Loading");
				toolTip = GameObject.Find("TapScreenToolTip");
				groundPlane = GameObject.Find("Ground Plane Stage");
				planeFinder = GameObject.Find("Plane Finder");
				surfaceAreaMaterial = Resources.Load("Materials/SurfaceArea") as Material;
				segmentLine = Resources.Load("Prefabs/SegmentLine") as GameObject;
				MarkerList = new List<Marker>();
			}
			catch (Exception ex) { debugText.text = $"{ex.Message}"; }
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
				// create line
				var line = Instantiate(segmentLine);
				line.transform.parent = groundPlane.transform;

				Vector3 between = finalPos - initialPos;
				float distance = between.magnitude;
				line.transform.localScale = new Vector3(0.01f, 0.01f, distance);
				var pos = initialPos + (between / 2);
				line.transform.position = pos;
				line.transform.LookAt(finalPos);

				// 3create 3d text
				GameObject textGo = new GameObject();
				textGo.name = "SegmentLineText";
				textGo.transform.parent = groundPlane.transform;
				textGo.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
				textGo.transform.position = pos;

				TextMesh textMesh = textGo.AddComponent(typeof(TextMesh)) as TextMesh;
				textMesh.text = distance > 1 ? $"{Math.Round(distance, 2)} mts." : $"{Math.Round(distance, 2) * 100} cms.";
				textMesh.fontSize = 14;
				textMesh.alignment = TextAlignment.Center;
				textMesh.anchor = TextAnchor.MiddleCenter;
				textMesh.transform.position = new Vector3(pos.x, pos.y + 0.05f, pos.z);

				FaceCameraBehaviour faceCameraBehaviour = textGo.AddComponent(typeof(FaceCameraBehaviour)) as FaceCameraBehaviour;
			}
			catch (Exception ex)
			{
				debugText.text = $"{ex.Message}";
			}
		}

		IEnumerator UpdateMetrics()
		{
			// total distance
			var perimeterDistance = 0.0f;
			foreach (var marker in MarkerList)
			{
				var dist = marker.LastSegmentDistance;
				perimeterDistance += dist;
			}

			debugText.text = perimeterDistance > 1 ? $"TOTAL: {Math.Round(perimeterDistance, 2)} mts." : $"TOTAL: {Math.Round(perimeterDistance, 2) * 100} cms.";

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
			}
		}

		private void CreateMesh()
		{
			Vector2[] points2D = new Vector2[MarkerList.Count - 1];

			for (int i = 0; i < MarkerList.Count - 1; i++)
			{
				points2D[i] = new Vector2(MarkerList[i].Position.x, MarkerList[i].Position.z);
			}

			Triangulator.Instance.SetPoints(points2D);
			int[] triangles = Triangulator.Instance.Triangulate();

			Vector3[] vertices3D = new Vector3[points2D.Length];

			for (int i = 0; i < vertices3D.Length; i++)
			{
				vertices3D[i] = new Vector3(points2D[i].x, MarkerList[i].Position.y + 0.005f, points2D[i].y);
			}

			Mesh msh = new Mesh();
			msh.vertices = vertices3D;
			msh.triangles = triangles;
			msh.RecalculateNormals();
			msh.RecalculateBounds();

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

			surfaceArea = CalculateSurfaceArea(msh);
			debugText.text += surfaceArea > 1 ? $"SURFACE: {Math.Round(surfaceArea, 2)} sq. mts." : $"SURFACE: {Math.Round(surfaceArea, 2) * 100} sq. cms.";
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
			ShowSurfaceLoading();
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
			catch (Exception ex) { debugText.text = $"{ex.Message}"; }
		}

		private void ShowFinishButton()
		{
			var button = GameObject.Find("FinishButton");
			button.GetComponent<Animator>().SetBool("isShown", true);
		}

		private void HideFinishButton()
		{
			var button = GameObject.Find("FinishButton");
			button.GetComponent<Animator>().SetBool("isShown", false);
		}

		public void ProcessHitTestResult(HitTestResult result)
		{
			HideSurfaceLoading();
			ShowTapScreenToolTip();
		}

		private void DisableStagePlacement()
		{
			planeFinder.GetComponent<ContentPositioningBehaviour>().AnchorStage = null;
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

		public void ShowAddMarkerButton()
		{
			var button = GameObject.Find("AddMarkerButton");
			button.GetComponent<Animator>().SetBool("isShown", true);
		}

		public void HideAddMarkerButton()
		{
			var button = GameObject.Find("AddMarkerButton");
			button.GetComponent<Animator>().SetBool("isShown", false);
		}

		private void SetUpCurrentItem()
		{
			CurrentMarker.transform.parent = groundPlane.transform;
			CurrentMarker.transform.position = planeFinder.GetComponent<PlaneFinderBehaviour>().PlaneIndicator.transform.localPosition;
			CurrentMarker.transform.localScale = CurrentMarker.transform.lossyScale;
		}
	}
}
