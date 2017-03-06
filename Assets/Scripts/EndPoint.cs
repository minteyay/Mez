using UnityEngine;

/// <summary>
/// Specifies a GameObject that marks the end of a maze.
/// </summary>
public class EndPoint : MonoBehaviour
{
	void OnTriggerEnter(Collider other)
	{
        // Reset the level when the Player hits this GameObject.
		if (other.transform.CompareTag("Player"))
			GameManager.Instance.ResetLevel();
	}
}
