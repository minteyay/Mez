using UnityEngine;

public class EndPoint : MonoBehaviour
{
	void OnTriggerEnter(Collider other)
	{
		if (other.transform.CompareTag("Player"))
			GameManager.Instance.ResetLevel();
	}
}
