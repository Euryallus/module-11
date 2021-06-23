using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private string sceneToLoadName;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            SaveLoadManager.Instance.LoadScene(sceneToLoadName);
        }
    }
}
