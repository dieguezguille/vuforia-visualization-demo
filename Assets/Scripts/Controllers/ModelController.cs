using UnityEngine;

public class ModelController : MonoBehaviour
{
	public GameObject model;
	public Light directionalLight;

	public void OnUpButtonPressed()
	{
		model.transform.localPosition = new Vector3(model.transform.localPosition.x, model.transform.localPosition.y, model.transform.localPosition.z + 0.05f);
	}

	public void OnDownButtonPressed()
	{
		model.transform.localPosition = new Vector3(model.transform.localPosition.x, model.transform.localPosition.y, model.transform.localPosition.z - 0.05f);
	}
	public void OnLeftButtonPressed()
	{
		model.transform.localPosition = new Vector3(model.transform.localPosition.x - 0.05f, model.transform.localPosition.y, model.transform.localPosition.z);
	}

	public void OnRightButtonPressed()
	{
		model.transform.localPosition = new Vector3(model.transform.localPosition.x + 0.05f, model.transform.localPosition.y, model.transform.localPosition.z);
	}

	public void OnPlusLightButtonPressed()
	{
		directionalLight.intensity += 0.1f;
	}

	public void OnMinusLightButtonPressed()
	{
		directionalLight.intensity -= 0.1f;
	}

	public void OnPlusHeightButtonPressed()
	{
		model.transform.localPosition = new Vector3(model.transform.localPosition.x, model.transform.localPosition.y + 0.05f, model.transform.localPosition.z);
	}

	public void OnMinusHeightButtonPressed()
	{
		model.transform.localPosition = new Vector3(model.transform.localPosition.x, model.transform.localPosition.y - 0.05f, model.transform.localPosition.z);
	}
}
