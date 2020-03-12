using UnityEngine;

[RequireComponent(typeof(RippleShaderBehaviour))]
public class RippleFxSwitcher : MonoBehaviour
{
	public Gradient baseAlbedo;
	public Gradient baseEmission;
	public Gradient waveColor;
	public float switchSpeed = 5;

	RippleShaderBehaviour rippleState;
	float parameter;
	float target;

	public bool state
	{
		get { return target > 0.0f; }
		set { target = state ? 1.0f : 0.0f; }
	}

	void Awake()
	{
		rippleState = GetComponent<RippleShaderBehaviour>();
	}

	void Update()
	{
		if (parameter < target)
		{
			parameter = Mathf.Min(1.0f, parameter + switchSpeed * Time.deltaTime);
			rippleState.enabled = true;
		}
		else if (parameter > target)
		{
			parameter = Mathf.Max(0.0f, parameter - switchSpeed * Time.deltaTime);
			if (parameter == 0.0f) rippleState.enabled = false;
		}

		if (parameter > 0.0f)
		{
			rippleState.baseColor = baseAlbedo.Evaluate(parameter);
			rippleState.addColor = baseEmission.Evaluate(parameter);
			rippleState.waveColor = waveColor.Evaluate(parameter);
		}
	}

	public void Toggle()
	{
		target = target > 0.0f ? 0.0f : 1.0f;
	}
}
