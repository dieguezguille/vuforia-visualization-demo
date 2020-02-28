using Assets.Scripts.Support;
using UnityEngine;

namespace Assets.Scripts.Controllers
{

	public class PolygonTester : MonoBehaviour
	{
		void Start()
		{
			// Create Vector2 vertices
			Vector2[] vertices2D = new Vector2[] {
			new Vector2(0,0),
			new Vector2(0,50),
			new Vector2(50,50),
			new Vector2(50,100),
			new Vector2(0,100),
			new Vector2(0,150),
			new Vector2(150,150),
			new Vector2(150,100),
			new Vector2(100,100),
			new Vector2(100,50),
			new Vector2(150,50),
			new Vector2(150,0),
		};

			// Use the triangulator to get indices for creating triangles
			Triangulator.Instance.SetPoints(vertices2D);
			int[] indices = Triangulator.Instance.Triangulate();

			// Create the Vector3 vertices
			Vector3[] vertices = new Vector3[vertices2D.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
			}

			// Create the mesh
			Mesh msh = new Mesh();
			msh.vertices = vertices;
			msh.triangles = indices;
			msh.RecalculateNormals();
			msh.RecalculateBounds();

			// Set up game object with mesh;
			MeshRenderer meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
			meshRenderer.enabled = true;

			MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
			filter.mesh = msh;
		}
	}
}