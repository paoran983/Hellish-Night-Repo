using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour {
    [SerializeField] private string sceneToLoad;
    [SerializeField] private Vector2 playerPos;
    [SerializeField] private UnitSceneLocation playerScenePosData;

    public void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Player") && collision.isTrigger!) {
            playerScenePosData.initalValue = playerPos;
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public string SceneToLoad { get { return sceneToLoad; } set { sceneToLoad = value; } }
    public Vector2 PlayerPos { get { return playerPos; } set { playerPos = value; } }

    public UnitSceneLocation PlayerScenePosData { get { return playerScenePosData; } set { playerScenePosData = value; } }
}
