using UnityEngine;

/// <summary>
/// Sets the UVs of a MeshFilter with 4 vertices (e.g. a Plane) to a specified region of a bigger texture.
/// </summary>
[ExecuteInEditMode]
public class UVRect : MonoBehaviour
{
    /// <summary>
    /// UV offset in the source texture.
    /// </summary>
	public Vector2 start;
    /// <summary>
    /// Dimensions of the area of the source texture to use.
    /// </summary>
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
    // Update the UV in the editor.
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
				Debug.Log("UVRect can only be used with MeshFilters with 4 vertices (such as a Plane).");
				return;
			}

			Vector2[] uvs = new Vector2[mesh.vertexCount];

            // The Y coordinate is reversed in UV coords.
			Vector2 fixedStart = new Vector2(start.x, -start.y);

			uvs[0] = new Vector2(dim.x, -dim.y) + fixedStart;
			uvs[1] = new Vector2(0.0f, -dim.y) + fixedStart;
			uvs[2] = new Vector2(0.0f, 0.0f) + fixedStart;
			uvs[3] = new Vector2(dim.x, 0.0f) + fixedStart;

			mesh.uv = uvs;
		}
	}
}
