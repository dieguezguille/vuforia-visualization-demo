using UnityEngine;

public class FaceCameraBehaviour : MonoBehaviour
{
	private Camera ARCamera;

	private void Start()
	{
		var cameraGo = GameObject.Find("ARCamera");
		ARCamera = cameraGo.GetComponent<Camera>();
	}
	private void LateUpdate()
	{
		transform.LookAt(transform.position + ARCamera.transform.rotation * Vector3.forward, ARCamera.transform.rotation * Vector3.up);
	}
}
