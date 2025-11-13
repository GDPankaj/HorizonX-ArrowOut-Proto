using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Level : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private List<Arrow> arrowsInLevel;
    [SerializeField] private float levelTimer = 300f;
    [SerializeField] Vector3 cameraPos = new Vector3(0f, 0f, -10f);
    public Vector3 CameraPos => cameraPos;
    private float remainingTime;
    private void Start()
    {
        foreach (Arrow arrow in arrowsInLevel)
        {
            arrow.Init(this);
        }
        counterCo = StartCoroutine(GameCounter());
    }
    Coroutine counterCo;
    public void Init(GameManager manager)
    {
        remainingTime = levelTimer;
        gameManager = manager;
        gameManager.SetTimer(remainingTime);
    }
    private void LevelFailed()
    {
        gameManager.DisplayLevelFailed(this);
        StopCoroutine(counterCo);
    }
    private void LevelSuccess()
    {
        gameManager.DisplayLevelSuccess(this);
        StopCoroutine(counterCo);
    }
    public void RemoveArrow(Arrow arrow)
    {
        if (arrowsInLevel.Contains(arrow))
        {
            arrowsInLevel.Remove(arrow);
            if (arrowsInLevel.Count == 0)
            {
                Invoke(nameof(LevelSuccess), 1f);
            }
        }
    }
    IEnumerator GameCounter()
    {
        while (remainingTime > 0)
        {
            yield return new WaitForSeconds(1f);
            remainingTime -= 1;
            gameManager.SetTimer(remainingTime);
        }
        if (remainingTime <= 0)
        {
            LevelFailed();
        }
    }
}
