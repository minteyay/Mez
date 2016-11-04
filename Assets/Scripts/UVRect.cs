using UnityEngine;

[ExecuteInEditMode]
public class UVRect : MonoBehaviour
{
	public Vector2 start;
	public Vector2 dim;

	public Mesh sourceMesh = null;
	private Mesh mesh = null;

	void Start()
	{
		mesh = Instantiate(sourceMesh);
		mesh.name = sourceMesh.name;
		GetComponent<MeshFilter>().sharedMesh = mesh;

		UpdateUV();
	}

#if UNITY_EDITOR
	void OnValidate()
	{
		UpdateUV();
	}
#endif

	private void UpdateUV()
	{
		if (mesh)
		{
			if (mesh.vertexCount != 4)
			{
				Debug.Log("UVRect can only be used with Planes.");
				return;
			}

			Vector2[] uvs = new Vector2[mesh.vertexCount];

			Vector2 fixedStart = new Vector2(start.x, -start.y);

			uvs[0] = new Vector2(dim.x, -dim.y) + fixedStart;
			uvs[1] = new Vector2(0.0f, -dim.y) + fixedStart;
			uvs[2] = new Vector2(0.0f, 0.0f) + fixedStart;
			uvs[3] = new Vector2(dim.x, 0.0f) + fixedStart;

			mesh.uv = uvs;
		}
	}
}
