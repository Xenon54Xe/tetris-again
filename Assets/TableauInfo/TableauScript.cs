using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableauScript : MonoBehaviour
{
    [Header("Structure")]
    public TextMeshProUGUI textName;
    public Transform startingPoint;
    public Transform cellListHierarchie;
    public GameObject cellPrefab;

    public List<GameObject> cellsArray = new List<GameObject>();

    private float time = 0;
    private KeyCode lastKey;

    [Header("TableauInfo")]
    public string tableauName;
    public int width;
    public int height;
    private int[,] tableau = null;



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            lastKey = KeyCode.M;
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            lastKey = KeyCode.I;
        }

        time -= Time.deltaTime;
        if (time <= 0)
        {
            time = 0.5f;
            if (lastKey == KeyCode.M)
            {
                SetTableau(GameManager.instance.movingDataArray, GameManager.instance.playGroundWidth, GameManager.instance.playGroundHeight, "MovingBlocks");
            }
            else if (lastKey == KeyCode.I)
            {
                SetTableau(GameManager.instance.idlingDataArray, GameManager.instance.playGroundWidth, GameManager.instance.playGroundHeight, "IdlingBlocks");
            }

            Show();
        }
    }

    public void SetTableau(int[,] newTableau, int newWidth, int newHeight, string newName)
    {
        tableau = newTableau;
        width = newWidth;
        height = newHeight;
        tableauName = newName;
    }

    public void Show()
    {
        ResetTableau();

        textName.text = tableauName;

        for (int ia = 0; ia < width; ia++)
        {
            int x = ia;
            for (int ib = 0; ib < height; ib++)
            {
                int y = ib;
                int num = tableau[x, y];

                GameObject newCell = GameObject.Instantiate(cellPrefab, cellListHierarchie);

                Image image = newCell.GetComponent<Image>();
                if (num == 0)
                {
                    image.color = Color.gray;
                }
                else
                {
                    image.color = Color.red;
                }

                cellsArray.Add(newCell);
            }
        }
    }

    void ResetTableau()
    {
        if (cellsArray.Count != 0)
        {
            foreach (GameObject gameObject in cellsArray)
            {
                Destroy(gameObject);
            }
        }

        cellsArray.Clear();
    }
}
