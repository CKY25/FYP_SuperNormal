using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerDisplay : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private GameObject gameTimer;
    private string timer = "GameTimer";

    private void Start()
    {
        gameTimer = GameObject.FindGameObjectWithTag(timer);
        gameTimer.GetComponent<GameTimer>().enabled = true;
    }

    void Update()
    {
        if (gameTimer != null && !transform.GetChild(1).GetChild(3).gameObject.activeSelf)
        {
            UpdateTimerDisplay(gameTimer.GetComponent<GameTimer>().networkCurrentTime.Value);
            if (gameTimer.GetComponent<GameTimer>().networkCurrentTime.Value <= 0)
            {
                Debug.Log("Timer reached 0");
                gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                timerText.text = null;
            }
            
        }
        else if (transform.GetChild(1).GetChild(3).gameObject.activeSelf)
        {
            timerText.text = null;
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
