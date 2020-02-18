using UnityEngine;
using Vuforia;

namespace Assets.Scripts.Interfaces
{
	public interface ISceneController
	{
		GameObject CurrentItem { get; set; }
		void OnContentPlaced();
		void InitializeDefaultScene();
		void ProcessHitTestResult(HitTestResult result);
		void ShowSurfaceLoading();
		void HideSurfaceLoading();
	}
}
