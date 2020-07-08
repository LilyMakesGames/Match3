using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        GameStart,
        Idle,
        Select,
        CheckEndGame,
        GameEnd
    }

    public static GameManager instance;

    public Board nodeBoard;

    public GameState gameState;
    Node currentSelectedNode;

    public int comboCount;
    int score = 0;
    int scoreToBeat;
    int currentStage = 1;
    int timer;
    public int timerMax;
    int highScore;

    public int scoreToBeatMultiplier;

    [SerializeField]
    TextMeshProUGUI scoreText;
    [SerializeField]
    TextMeshProUGUI scoreToBeatText;
    [SerializeField]
    TextMeshProUGUI timerText;
    [SerializeField]
    Image timerBarImage;
    [SerializeField]
    TextMeshProUGUI highScoreText;

    [SerializeField]
    GameObject startPanel, gameOverPanel, gameUI;

    public AudioClip clearSound, selectSound, swapSound;

    [SerializeField]
    AudioSource audioSourceSounds;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        gameState = GameState.Idle;
    }

    public void Start()
    {
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        SetGameDefinitions();
    }

    void SetGameDefinitions()
    {
        timer = timerMax;
        UpdateScore();
        SetStageMaxScore();
        StartCoroutine(Timer());
        nodeBoard.CreateBoard();
        nodeBoard.FirstReorganize();
        gameState = GameState.Idle;
        gameUI.SetActive(true);
        UpdateTimeUI();
    }

    public void RestartGame()
    {
        foreach (var node in nodeBoard.nodeBoard)
        {
            Destroy(node.gameObject);
        }
        score = 0;
        currentStage = 1;
        gameOverPanel.SetActive(false);
        SetGameDefinitions();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(nodeBoard.CheckForPossibleCombinations());
        }
    }

    public void StageBeat()
    {
        StopAllCoroutines();
        currentStage++;
        SetStageMaxScore();
        timer = timerMax;
        StartCoroutine(Timer());
        UpdateTimeUI();
        gameState = GameState.Idle;
    }

    public void InteractNode(Node node)
    {
        switch (gameState)
        {
            case GameState.Idle:
                gameState = GameState.Select;
                node.spriteRenderer.color = Color.gray;
                currentSelectedNode = node;
                PlaySound(selectSound);
                break;
            case GameState.Select:
                if (nodeBoard.GetAllAdjacentNodes(nodeBoard.GetNodeBoardPosition(currentSelectedNode)).Contains(node))
                {
                    nodeBoard.SwapNodes(node, currentSelectedNode);
                    currentSelectedNode.spriteRenderer.color = Color.white;
                    if (nodeBoard.CheckForCombination().Count > 0)
                    {
                        PlaySound(swapSound);
                        nodeBoard.ReorganizeLoop();
                    }
                    else
                    {
                        nodeBoard.SwapNodes(node, currentSelectedNode);
                    }
                    gameState = GameState.Idle;
                    currentSelectedNode = null;
                }
                else
                {
                    currentSelectedNode.spriteRenderer.color = Color.white;
                    node.spriteRenderer.color = Color.gray;
                    PlaySound(selectSound);
                    currentSelectedNode = node;
                }
                break;
        }
    }

    public void AddScore(int nodesDeleted)
    {
        score += nodesDeleted * comboCount * 100;
        UpdateScore();
    }

    public void UpdateScore()
    {
        scoreText.text = $"Score: \n {score}";
    }

    void SetStageMaxScore()
    {
        scoreToBeat = currentStage * scoreToBeatMultiplier + score;
        scoreToBeatText.text = $"Target: \n {scoreToBeat.ToString()}";
    }

    IEnumerator Timer()
    {
        if (timer <= 0)
        {
            gameState = GameState.CheckEndGame;
            if (score >= scoreToBeat)
            {
                StageBeat();
            }
            else
            {
                gameOverPanel.SetActive(true);
                gameUI.SetActive(false);
                if (highScore < score)
                {
                    highScore = score;
                    highScoreText.text = $"HighScore: \n{highScore.ToString()}";
                }
                gameState = GameState.GameEnd;
            }
        }
        else
        {
            yield return new WaitForSeconds(1);
            timer--;
            StartCoroutine(Timer());
        }
        UpdateTimeUI();
    }

    private void UpdateTimeUI()
    {
        timerText.text = timer.ToString();
        timerBarImage.rectTransform.sizeDelta = new Vector2(15 + 315 * ((float)timer / timerMax), 100);
    }

    public void PlaySound(AudioClip sound)
    {
        audioSourceSounds.clip = sound;
        audioSourceSounds.Play();
    }

}
