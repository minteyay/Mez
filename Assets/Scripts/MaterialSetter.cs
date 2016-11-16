using UnityEngine;

public class MaterialSetter : MonoBehaviour
{
	public void SetMaterial(Material material)
	{
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		if (renderer != null)
			renderer.material = material;

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
