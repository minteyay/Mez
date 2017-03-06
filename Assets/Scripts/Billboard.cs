using UnityEngine;

/// <summary>
/// Billboards the GameObject this script is attached to, making it face the camera at all times (using only Y rotation).
/// </summary>
public class Billboard : MonoBehaviour
{
	void Update()
	{
		if (Camera.main == null)
			return;
        // Get a look rotation towards the main camera.
		Quaternion towardsCamera = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);

        // Only use the Y euler rotation from the look rotation.
		Vector3 lookEulers = towardsCamera.eulerAngles;
		lookEulers.x = lookEulers.z = 0.0f;
		transform.rotation = Quaternion.Euler(lookEulers);
	}
}
