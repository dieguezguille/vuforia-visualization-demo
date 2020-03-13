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
		[SerializeField]
		private GameObject loading;
		[SerializeField]
		private GameObject toolTip;
		[SerializeField]
		private CanvasGroup uiWrapper;
		[SerializeField]
		private Toggle cameraOnlyToggle;
		[SerializeField]
		private GameObject uiCanvas;
		// VUFORIA
		private GameObject currentMarker;
		private GameObject PreviousMarker;
		private GameObject groundPlane;
		private GameObject planeFinder;
		private GameObject segmentLinePrefab;
		private GameObject movingSegmentLine;
		// VARS
		private bool shouldUpdateMetrics = false;
		private Material surfaceAreaMaterial;
		private float surfaceArea;
		private float perimeterDistance;
		private bool shouldUpdateMovingSegmentLine = false;
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
			Screen.orientation = ScreenOrientation.Portrait;

			try
			{
				groundPlane = GameObject.Find("Ground Plane Stage");
				planeFinder = GameObject.Find("Plane Finder");
				surfaceAreaMaterial = Resources.Load("Materials/SurfaceArea") as Material;
				segmentLinePrefab = Resources.Load("Prefabs/SegmentLine") as GameObject;
				MarkerList = new List<Marker>();
				NativeToolkit.OnScreenshotSaved += NativeToolkit_OnScreenshotSaved;

				// moving segment line
				movingSegmentLine = Instantiate(segmentLinePrefab);
				movingSegmentLine.transform.parent = groundPlane.transform;
				movingSegmentLine.SetActive(false);
			}
			catch (Exception ex) { debugText.text = $"{ex.Message}"; }
		}

		void Update()
		{
			if (shouldUpdateMetrics)
			{
				StartCoroutine(UpdateMetrics());
			}
			else
			{
				StopCoroutine(UpdateMetrics());
			}

			if (shouldUpdateMovingSegmentLine)
			{
				movingSegmentLine.SetActive(true);
				StartCoroutine(UpdateMovingSegmentLine());
			}
			else
			{
				movingSegmentLine.SetActive(false);
				StopCoroutine(UpdateMovingSegmentLine());
			}
		}

		private void BuildSegmentLine(Vector3 initialPos, Vector3 finalPos)
		{
			try
			{
				// segment line
				var line = Instantiate(segmentLinePrefab);
				line.transform.parent = groundPlane.transform;

				Vector3 between = finalPos - initialPos;
				float distance = between.magnitude;
				line.transform.localScale = new Vector3(0.01f, 0.01f, distance);
				var pos = initialPos + (between / 2);
				line.transform.position = pos;
				line.transform.LookAt(finalPos);

				// text over line
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
			perimeterDistance = 0.0f;

			if (MarkerList != null && MarkerList.Count > 1)
			{
				foreach (var marker in MarkerList)
				{
					var dist = marker.LastSegmentDistance;
					perimeterDistance += dist;
				}
			}

			debugText.text = perimeterDistance > 1 ? $"\n TOTAL: {Math.Round(perimeterDistance, 2)} mts." : $"\n TOTAL: {Math.Round(perimeterDistance, 2) * 100} cms.";

			// last segment distance
			if (MarkerList != null && MarkerList.Count > 0)
			{
				var dist = Vector3.Distance(planeFinder.GetComponent<PlaneFinderBehaviour>().PlaneIndicator.transform.localPosition, MarkerList.Last().Position);
				debugText.text += dist > 1 ? $"\n CURRENT: {Math.Round(dist, 2)} mts." : $"\n CURRENT: {Math.Round(dist, 2) * 100} cms.";
			}

			//angle
			//var from = CurrentMarker.transform.position - PreviousMarker.transform.position;
			//var to = Vector3.ProjectOnPlane(from, -CurrentMarker.transform.up);
			//var angle = Vector3.Angle(from, to);

			//debugText.text += $" ANGLE: {Math.Round(angle, 2)} dgs.";

			yield return new WaitForSeconds(.1f);
		}

		IEnumerator UpdateMovingSegmentLine()
		{
			// segment line
			var finalPos = planeFinder.GetComponent<PlaneFinderBehaviour>().PlaneIndicator.transform.localPosition;
			var initialPos = MarkerList.Last().Position;

			Vector3 between = finalPos - initialPos;
			float distance = between.magnitude;
			movingSegmentLine.transform.localScale = new Vector3(0.01f, 0.01f, distance);
			var pos = initialPos + (between / 2);
			movingSegmentLine.transform.position = pos;
			movingSegmentLine.transform.LookAt(finalPos);

			yield return new WaitForSeconds(.1f);
		}

		public void ResetScene()
		{
			SceneManager.LoadScene("MeasureScene", LoadSceneMode.Single);
		}

		public void GoMainMenu()
		{
			SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
		}

		public void GoGalleryScene()
		{
			SceneManager.LoadScene("GalleryScene", LoadSceneMode.Single);
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
				shouldUpdateMovingSegmentLine = true;

				if (MarkerList.Count > 1)
				{
					BuildSegmentLine(PreviousMarker.transform.position, CurrentMarker.transform.position);
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
				shouldUpdateMovingSegmentLine = false;

				var lastMarker = new Marker()
				{
					Previous = MarkerList.Last(),
					Position = MarkerList.First().Position,
				};

				Vector3 initialPos = MarkerList.Last().Position;
				Vector3 finalPos = MarkerList.First().Position;

				perimeterDistance += Vector3.Distance(finalPos, initialPos);
				debugText.text = perimeterDistance > 1 ? $"\n TOTAL: {Math.Round(perimeterDistance, 2)} mts." : $"\n TOTAL: {Math.Round(perimeterDistance, 2) * 100} cms.";

				BuildSegmentLine(initialPos, finalPos);
				movingSegmentLine.SetActive(false);

				MarkerList.Add(lastMarker);
				CreateMesh();
			}
			catch (Exception ex)
			{
				debugText.text = ex.Message;
			}
		}

		public void TakeScreenshot()
		{
			try
			{
				uiWrapper.alpha = 0;
				groundPlane.SetActive(!cameraOnlyToggle.isOn);
				uiCanvas.SetActive(!cameraOnlyToggle.isOn);
				planeFinder.SetActive(false);

				NativeToolkit.SaveScreenshot($"Screenshot");
			}
			catch (Exception e)
			{
				debugText.text = e.Message;
			}
		}

		private void NativeToolkit_OnScreenshotSaved(string path)
		{
			uiWrapper.alpha = 1;
			groundPlane.SetActive(true);
			uiCanvas.SetActive(true);
			planeFinder.SetActive(true);
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

			RippleShaderBehaviour rippleBehaviour = emptyGo.AddComponent(typeof(RippleShaderBehaviour)) as RippleShaderBehaviour;

			MeshFilter filter = emptyGo.AddComponent(typeof(MeshFilter)) as MeshFilter;
			filter.mesh = msh;

			surfaceArea = CalculateSurfaceArea(msh);
			debugText.text += surfaceArea > 1 ? $"\n SURFACE: {Math.Round(surfaceArea, 2)} sq. mts." : $"\n SURFACE: {Math.Round(surfaceArea, 2) * 100} sq. cms.";
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

		public void ProcessHitTestResult(HitTestResult result)
		{
			HideSurfaceLoading();
			ShowTapScreenToolTip();
		}

		private void SetUpCurrentItem()
		{
			CurrentMarker.transform.parent = groundPlane.transform;
			CurrentMarker.transform.position = planeFinder.GetComponent<PlaneFinderBehaviour>().PlaneIndicator.transform.localPosition;
			CurrentMarker.transform.localScale = CurrentMarker.transform.lossyScale;
		}

		#region UI Methods
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
		#endregion
	}
}