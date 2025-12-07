using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;
    
    public Tube firstTube;
    public Tube secondTube;
    
    [SerializeField] protected Tube[] allTubes;

    protected void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one GameManager in scene!");
            return;
        }

        instance = this;
    }


    protected void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider is not null)
            {
                if (hit.collider.TryGetComponent(out Tube tube))
                {
                    if(firstTube is null)
                        firstTube = tube;
                    else
                    {
                        if(firstTube == tube)
                            firstTube = null;
                        else
                        {
                            secondTube = tube;
                            firstTube.tubeRef = secondTube;
                            
                            firstTube.UpdateTopWaterColor();
                            secondTube.UpdateTopWaterColor();
                            if (secondTube.FillTubeCheck(firstTube.topColor))
                            {
                                firstTube.StartWaterPouring();
                                firstTube = null;
                                secondTube = null;
                                CheckWinCondition();
                            }
                            else
                            {
                                firstTube = null;
                                secondTube = null;
                            }
                        }
                    }
                }
            }
        }
    }
    
    protected void CheckWinCondition()
    {
        foreach (var tube in allTubes)
        {
            // Tube phải hoặc rỗng hoặc đầy 4 layers cùng màu
            if (tube.numberOfWaters > 0)
            {
                if (tube.numberOfWaters != 4 || tube.numberOfTopColorLayers != 4)
                    return;
            }
        }

        Debug.Log("WIN!");
        UIManager.Instance?.ShowWinPanel();
    }

    protected void CheckLoseCondition()
    {
        for (int i = 0; i < allTubes.Length; i++)
        {
            if (allTubes[i].numberOfWaters == 0)
                continue;

            allTubes[i].UpdateTopWaterColor();

            for (int j = 0; j < allTubes.Length; j++)
            {
                if (i == j)
                    continue;

                allTubes[j].UpdateTopWaterColor();

                if (allTubes[j].FillTubeCheck(allTubes[i].topColor))
                {
                    return;
                }
            }
        }

        Debug.Log("LOSE! No more valid moves!");
        UIManager.Instance?.ShowLosePanel();
    }

    public void ResetLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}