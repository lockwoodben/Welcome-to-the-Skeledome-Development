using UnityEngine;

public class RespawnManager
{

    private static RespawnManager instance;
    private RespawnManager() {}

    public static RespawnManager Instance() {
        if (instance == null) {
            instance = new RespawnManager();
        }

        return instance;
    }
    // Start is called before the first frame update
    private Vector4 respawnPoint = Vector3.zero;

    public void SetRespawnPoint(Vector3 newRespawnPoint) => respawnPoint = newRespawnPoint;

    public Vector3 getRespawnPoint() => respawnPoint;

    public void Respawn(Transform respawnable) =>
        respawnable.transform.SetPositionAndRotation(respawnPoint, Quaternion.identity);
}
