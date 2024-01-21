using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverCanvas;
    public GameObject gameStartCanvas;
    bool gameStarted;
    bool gameOver;

    // Start is called before the first frame update
    public void Start()
    {
        gameStarted = false;
        gameOver = false;
        gameOverCanvas.SetActive(false);
        Time.timeScale = 0;
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(!gameStarted)
            {
                gameStartCanvas.SetActive(false);
                Time.timeScale = 1;
                gameStarted = true;
            }
            if(gameOver)
            {
                gameOverCanvas.SetActive(false);
                gameOver = false;
                Replay();
            }
            
        }
    }

    public void GameOver()
    {
        gameOverCanvas.SetActive(true);
        gameOver = true;
        Time.timeScale = 0;
    }

    private void Replay()
    {
        SceneManager.LoadScene("GameScene");
    }
}
