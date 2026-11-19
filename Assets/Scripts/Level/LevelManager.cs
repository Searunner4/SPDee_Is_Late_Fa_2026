using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One per level scene. Handles respawning the player, checkpoints, and finishing the level.
/// Ethan's timer/UI can subscribe to the static events below without referencing this script's internals.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // --- Events for the timer / UI (Ethan) ---
    public static event Action OnLevelStarted;
    public static event Action OnLevelCompleted;
    public static event Action<int> OnPlayerDied;   // passes total deaths this level

    [Header("Player")]
    [Tooltip("Leave empty to find the object tagged \"Player\".")]
    [SerializeField] Transform player;
    [SerializeField] Transform startPoint;
    [SerializeField] float invulnerableTime = 1f;

    [Header("Level Flow")]
    [Tooltip("Scene to load after the goal. Leave empty to load the next scene in Build Settings.")]
    [SerializeField] string nextSceneName = "";
    [SerializeField] float delayBeforeNextLevel = 1.5f;

    Vector3 respawnPoint;
    float lastHitTime = -999f;
    bool levelFinished;
    public int Deaths { get; private set; }

    void Awake()
    {
        Instance = this;
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Start()
    {
        respawnPoint = startPoint != null ? startPoint.position : player.position;
        OnLevelStarted?.Invoke();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SetCheckpoint(Vector3 position)
    {
        respawnPoint = position;
    }

    /// <summary>Called by enemies, hazards, and kill zones.</summary>
    public void PlayerHit()
    {
        if (levelFinished || Time.time - lastHitTime < invulnerableTime) return;
        lastHitTime = Time.time;
        Deaths++;

        player.position = respawnPoint;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        OnPlayerDied?.Invoke(Deaths);
    }

    public void CompleteLevel()
    {
        if (levelFinished) return;
        levelFinished = true;
        GameSession.AddLevel(Time.timeSinceLevelLoad, Deaths);
        OnLevelCompleted?.Invoke();
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(delayBeforeNextLevel);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            Debug.Log("Last level finished. Hook up the end/results screen here.");
    }
}
