using UnityEngine;
using UnityEngine.UI;

public class ModelController : MonoBehaviour
{
	public GameObject Model;
	public Light DirectionalLight;
	public Text DebugText;
	public Camera ARCamera;

	public void OnUpButtonPressed()
	{
		Model.transform.localPosition = new Vector3(Model.transform.localPosition.x, Model.transform.localPosition.y, Model.transform.localPosition.z + 0.2f);
	}

	public void OnDownButtonPressed()
	{
		Model.transform.localPosition = new Vector3(Model.transform.localPosition.x, Model.transform.localPosition.y, Model.transform.localPosition.z - 0.2f);
	}
	public void OnLeftButtonPressed()
	{
		Model.transform.localPosition = new Vector3(Model.transform.localPosition.x - 0.2f, Model.transform.localPosition.y, Model.transform.localPosition.z);
	}

	public void OnRightButtonPressed()
	{
		Model.transform.localPosition = new Vector3(Model.transform.localPosition.x + 0.2f, Model.transform.localPosition.y, Model.transform.localPosition.z);
	}

	public void OnPlusLightButtonPressed()
	{
		DirectionalLight.intensity += 0.1f;
	}

	public void OnMinusLightButtonPressed()
	{
		DirectionalLight.intensity -= 0.1f;
	}
}
