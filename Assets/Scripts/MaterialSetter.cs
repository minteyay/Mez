using UnityEngine;

/// <summary>
/// Sets a material to this GameObject's MeshRenderer as well as to all child GameObjects that have a MeshRenderer attached.
/// </summary>
public class MaterialSetter : MonoBehaviour
{
	public void SetMaterial(Material material)
	{
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		if (renderer != null)
			renderer.material = material;

        // Go through child objects and set their materials as well.
		foreach (Transform child in transform)
		{
			MaterialSetter setter = child.GetComponent<MaterialSetter>();
			if (setter != null)
				setter.SetMaterial(material);

			renderer = child.GetComponent<MeshRenderer>();
			if (renderer != null)
				renderer.material = material;
		}
	}
}
