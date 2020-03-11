using UnityEngine;

public class RippleState : MonoBehaviour
{
	// Sonar mode (directional or spherical)
	public enum SonarMode { Directional, Spherical }
	[SerializeField] SonarMode _mode = SonarMode.Directional;
	public SonarMode mode { get { return _mode; } set { _mode = value; } }

	// Wave direction (used only in the directional mode)
	[SerializeField] Vector3 _direction = Vector3.forward;
	public Vector3 direction { get { return _direction; } set { _direction = value; } }

	// Wave origin (used only in the spherical mode)
	[SerializeField] Vector3 _origin = Vector3.zero;
	public Vector3 origin { get { return _origin; } set { _origin = value; } }

	// Base color (albedo)
	[SerializeField] Color _baseColor = new Color(0.2f, 0.2f, 0.2f, 0);
	public Color baseColor { get { return _baseColor; } set { _baseColor = value; } }

	// Wave color
	[SerializeField] Color _waveColor = new Color(1.0f, 0.2f, 0.2f, 0);
	public Color waveColor { get { return _waveColor; } set { _waveColor = value; } }

	// Wave color amplitude
	[SerializeField] float _waveAmplitude = 2.0f;
	public float waveAmplitude { get { return _waveAmplitude; } set { _waveAmplitude = value; } }

	// Exponent for wave color
	[SerializeField] float _waveExponent = 22.0f;
	public float waveExponent { get { return _waveExponent; } set { _waveExponent = value; } }

	// Interval between waves
	[SerializeField] float _waveInterval = 20.0f;
	public float waveInterval { get { return _waveInterval; } set { _waveInterval = value; } }

	// Wave speed
	[SerializeField] float _waveSpeed = 10.0f;
	public float waveSpeed { get { return _waveSpeed; } set { _waveSpeed = value; } }

	// Additional color (emission)
	[SerializeField] Color _addColor = Color.black;
	public Color addColor { get { return _addColor; } set { _addColor = value; } }

	// Reference to the shader.
	[SerializeField] Shader shader;

	// Private shader variables
	int baseColorID;
	int waveColorID;
	int waveParamsID;
	int waveVectorID;
	int addColorID;

	void Awake()
	{
		baseColorID = Shader.PropertyToID("_SonarBaseColor");
		waveColorID = Shader.PropertyToID("_SonarWaveColor");
		waveParamsID = Shader.PropertyToID("_SonarWaveParams");
		waveVectorID = Shader.PropertyToID("_SonarWaveVector");
		addColorID = Shader.PropertyToID("_SonarAddColor");
	}

	void Update()
	{
		Shader.SetGlobalColor(baseColorID, _baseColor);
		Shader.SetGlobalColor(waveColorID, _waveColor);
		Shader.SetGlobalColor(addColorID, _addColor);

		var param = new Vector4(_waveAmplitude, _waveExponent, _waveInterval, _waveSpeed);
		Shader.SetGlobalVector(waveParamsID, param);

		if (_mode == SonarMode.Directional)
		{
			Shader.DisableKeyword("SONAR_SPHERICAL");
			Shader.SetGlobalVector(waveVectorID, _direction.normalized);
		}
		else
		{
			Shader.EnableKeyword("SONAR_SPHERICAL");
			Shader.SetGlobalVector(waveVectorID, _origin);
		}
	}

}
