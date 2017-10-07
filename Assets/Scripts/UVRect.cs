using UnityEngine;

/// <summary>
/// Sets the UVs of a MeshFilter with 4 vertices (e.g. a Plane) to a specified region of a bigger texture.
/// </summary>
[ExecuteInEditMode]
public class UVRect : MonoBehaviour
{
	[SerializeField] private Vector2 _offset;
    /// UV offset in the source texture.
	public Vector2 offset
	{
		get { return _offset; }
		set
		{
			_offset = value;
			UpdateUV();
		}
	}
	
	[SerializeField] private Vector2 _dimensions;
    /// Dimensions of the area of the source texture to use.
	public Vector2 dimensions
	{
		get { return _dimensions; }
		set
		{
			_dimensions = value;
			UpdateUV();
		}
	}

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
		// Update the UV in the editor.
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
			Vector2 fixedOffset = new Vector2(_offset.x, -_offset.y);

			uvs[0] = new Vector2(_dimensions.x, -_dimensions.y) + fixedOffset;
			uvs[1] = new Vector2(0.0f, -_dimensions.y) + fixedOffset;
			uvs[2] = new Vector2(0.0f, 0.0f) + fixedOffset;
			uvs[3] = new Vector2(_dimensions.x, 0.0f) + fixedOffset;

			mesh.uv = uvs;
		}
	}
}
