using UnityEngine;

public class Portal : MonoBehaviour, ISavePoint
{
    [SerializeField] private string     sceneToLoadName;
    [SerializeField] private Transform  respawnTransform;

    public Vector3 GetRespawnPosition()
    {
        return respawnTransform.position;
    }

    public string GetSavePointId()
    {
        return "portal_" + transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            AudioManager.Instance.StopAllLoopingSoundEffects();

            WorldSave.Instance.UsedSavePointId = GetSavePointId();

            SaveLoadManager.Instance.SaveGameData(sceneToLoadName);

            SaveLoadManager.Instance.LoadGameScene(sceneToLoadName);
        }
    }
}