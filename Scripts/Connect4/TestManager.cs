using ConnectFour;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestManager : MonoBehaviour
{
    public static TestManager Instance;
    public int _games = 50;

    private int yellowWins = 0;
    private int redWins = 0;
    private int draws = 0;
    private int games = 1;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (games <= _games)
        {
            // if there is a scene
            if (SceneManager.GetActiveScene() != null)
            {
                GameObject game = GameObject.Find("GameController");
                if (game != null)
                {
                    GameController controller = game.GetComponent<GameController>();
                    if (controller != null)
                    {
                        // if game is over
                        Connect4State.Result result = controller.GetGameResult();
                        if (result != Connect4State.Result.Undecided)
                        {
                            // tally scores
                            if (result == Connect4State.Result.YellowWin)
                            {
                                yellowWins++;
                            }
                            else if (result == Connect4State.Result.RedWin)
                            {
                                redWins++;
                            }
                            else
                            {
                                draws++;
                            }

                            Debug.Log("//SCORE// " + games + " games\nYellow: " + yellowWins + "(" + (yellowWins*100)/(float)games + "%)    Red: " + redWins + "(" + (redWins*100)/(float)games + "%)    Draws: " + draws);

                            games++;

                            // new scene
                            SceneManager.LoadScene("Connect4");
                        }
                    }
                }
            }
            else
            {
                SceneManager.LoadScene("Connect4");
            }
        }
    }
}
