using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour // Changer création d'une nouvelle forme
{
    public static GameManager instance;

    [Header("GameDimensions")]
    public Transform cornerBottomLeftTransform;
    public int playGroundWidth;
    public int playGroundHeight;
    public float stepBetweenBlocksDistance;
    public int spawnCenterPosX;
    public int spawnCenterPosY;

    [Header("Prefabs")]
    public GameObject[] blocksPrefabArray;

    [Header("ShapeData")]
    public ShapeData[] shapesData;

    [Header("GameData")]
    private ShapeData currentShapeMoving;

    public Transform movingBlocksHierarchie;

    public GameObject[] movingBlocksArray;
    public List<GameObject> idlingBlocksArray;

    public int[,] movingDataArray;
    public int[,] idlingDataArray;

    public int[] lineFullTracker;

    [Header("Variables")]
    public float currentScore = 0;
    public int scorePerBlocks = 50;
    public int scorePerLine = 150;
    public float baseMultiplier = 2f;

    public float timeBetweenGoDown = 1.5f;
    private float timeRemainingBeforeNestGoDown = 1.5f;
    private bool canAddMoreDelay = true;

    private int nextChoosedShape;
    private int nextChoosedColor;
    private bool nextReverse;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ResetGame(); 
        int choosedShape = Random.Range(0, 7);
        int choosedColor = Random.Range(0, 6);
        SpawnNewShape(shapesData[choosedShape], choosedColor, Random.Range(0, 2) == 1);
        nextChoosedShape = Random.Range(0, 7);
        nextChoosedColor = Random.Range(0, 6);
        nextReverse = Random.Range(0, 2) == 1;
    }

    private void Update()
    {
        // Mouvements automatiques
        timeRemainingBeforeNestGoDown -= Time.deltaTime;
        if (timeRemainingBeforeNestGoDown <= 0)
        {
            timeRemainingBeforeNestGoDown = timeBetweenGoDown;
            canAddMoreDelay = true;
            if (CanMove(Vector3.down))
            {
                Move(Vector3.down);
            }
            else
            {
                // création nouvelle pièce
                SetIdleMovingShape();

                SpawnNewShape(shapesData[nextChoosedShape], nextChoosedColor, nextReverse);
                if (!CanMove(Vector3.zero))
                {
                    // Perdu...
                }
                nextChoosedShape = Random.Range(0, 7);
                nextChoosedColor = Random.Range(0, 6);
                nextReverse = Random.Range(0, 2) == 1;
            }
        }

        // Mouvements demandés par le joueur
        Vector3 move = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.S))
        {
            MoveDownCompletely();
            if (canAddMoreDelay)
            {
                canAddMoreDelay = false;
                timeRemainingBeforeNestGoDown = 0.5f;
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            move = Vector3.left;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            move = Vector3.right;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            var canRotate = CanRotate(Rotation.Left);
            if (canRotate.Item1)
            {
                Rotate(Rotation.Left, canRotate.Item2);
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            var canRotate = CanRotate(Rotation.Right);
            if (canRotate.Item1)
            {
                Rotate(Rotation.Right, canRotate.Item2);
            }
        }

        if (move != Vector3.zero && CanMove(move))
        {
            Move(move);
        }
    }

    private void ShowNextShape()
    {

    }

    private void ResetGame()
    {
        movingDataArray = new int[playGroundWidth, playGroundHeight];
        idlingDataArray = new int[playGroundWidth, playGroundHeight];
        ResetArray(movingDataArray);
        ResetArray(idlingDataArray);

        lineFullTracker = new int[playGroundHeight];
        for (int i = 0; i < lineFullTracker.Length; i++)
        {
            lineFullTracker[i] = 0;
        }

        foreach (GameObject gameObject in movingBlocksArray)
        {
            Destroy(gameObject);
        }

        foreach (GameObject gameObject in idlingBlocksArray)
        {
            Destroy(gameObject);
        }
        idlingBlocksArray.Clear();
    }

    public void SpawnNewShape(ShapeData choosedShape, int choosedColor, bool reverse)
    {
        Vector3[] shapeVectors = choosedShape.GetShape();
        GameObject choosedBlock = blocksPrefabArray[choosedColor];

        ResetArray(movingDataArray);
        movingBlocksArray = new GameObject[shapeVectors.Length];

        for (int i = 0; i < shapeVectors.Length; i++)
        {
            Vector3 vector = shapeVectors[i];
            if (reverse && choosedShape.canBeReversed)
            {
                vector = new Vector3(-vector.x, vector.y, 0);
            }

            // Créer block
            GameObject newBlock = GameObject.Instantiate(choosedBlock, movingBlocksHierarchie);
            newBlock.transform.position = cornerBottomLeftTransform.position + new Vector3(spawnCenterPosX, spawnCenterPosY, 0) * stepBetweenBlocksDistance + vector * stepBetweenBlocksDistance;
            // Définir données du block
            BlockScript blockScript = newBlock.GetComponent<BlockScript>();
            int posX = spawnCenterPosX + (int)vector.x;
            int posY = spawnCenterPosY + (int)vector.y;
            blockScript.SetHeightPos(posX, posY);
            blockScript.SetPosFromCenter(vector);
            blockScript.ResetTargetedPos();

            movingDataArray[posX, posY] = 1;
            movingBlocksArray[i] = newBlock;
        }

        currentShapeMoving = choosedShape;
    }

    public void SetIdleMovingShape()
    {
        for (int i = 0; i < movingBlocksArray.Length; i++)
        {
            BlockScript blockScript = movingBlocksArray[i].GetComponent<BlockScript>();
            blockScript.SetIdleState();
            lineFullTracker[blockScript.currentHeightPos]++;

            idlingBlocksArray.Add(movingBlocksArray[i]);
            idlingDataArray[blockScript.currentWidthPos, blockScript.currentHeightPos] = 1;

            currentScore += scorePerBlocks;
        }

        int nbLineFull = 0;
        for (int i = 0; i < playGroundHeight; i++)
        {
            if (lineFullTracker[i] == playGroundWidth)
            {
                nbLineFull++;
                lineFullTracker[i] = 0;
                RemoveLineAndMoveBlockDown(i);
                i--;
            }
        }

        currentScore += nbLineFull * scorePerLine * Mathf.Pow(baseMultiplier, nbLineFull - 1);
    }
     
    public void MoveDownCompletely()
    {
        int time = 1;
        while (CanMove(Vector3.down * time))
        {
            time++;
        }
        time--;

        Move(Vector3.down * time);
    }

    public (bool, Vector3) CanRotate(Rotation rotation)
    {
        if (!currentShapeMoving.canRotate)
        {
            return (false, Vector3.zero);
        }

        bool DoTest(Rotation rotation, Vector3 move)
        {
            for (int i = 0; i < movingBlocksArray.Length; i++)
            {
                BlockScript blockScript = movingBlocksArray[i].GetComponent<BlockScript>();
                Vector3 vector = blockScript.GetPosFromCenter();
                int x = rotation == Rotation.Left ? -(int)vector.y : (int)vector.y;
                int y = rotation == Rotation.Left ? (int)vector.x : -(int)vector.x;

                int newX = x + (int)blockScript.GetCenterPosDataArray().x + (int)move.x;
                int newY = y + (int)blockScript.GetCenterPosDataArray().y + (int)move.y;

                if (newX < 0 || newX > playGroundWidth - 1 || newY < 0 || newY > playGroundHeight || idlingDataArray[newX, newY] == 1)
                {
                    return false;
                }
            }
            return true;
        }

        // Test sans délacer
        bool hasFound = DoTest(rotation, Vector3.zero);
        if (hasFound)
        {
            return (true, Vector3.zero);
        }

        // Test en déplaçant
        int[] posToTest = new int[] { -1, 1, -2, 2 };
        foreach (int pos in posToTest)
        {
            int num = rotation == Rotation.Left ? pos : -pos;

            hasFound = DoTest(rotation, new Vector3(num, 0, 0));
            if (hasFound)
            {
                return (true, new Vector3(num, 0, 0));
            }
        }

        return (false, Vector3.zero);
    }

    public void Rotate(Rotation rotation, Vector3 move)
    {
        for (int i = 0; i < movingBlocksArray.Length; i++)
        {
            BlockScript blockScript = movingBlocksArray[i].GetComponent<BlockScript>();
            movingDataArray[blockScript.currentWidthPos, blockScript.currentHeightPos] = 0;
        }

        for (int i = 0; i < movingBlocksArray.Length; i++)
        {
            GameObject block = movingBlocksArray[i];
            BlockScript blockScript = block.GetComponent<BlockScript>();

            blockScript.Rotate(rotation);
            if (move != Vector3.zero)
            {
                blockScript.Move(move);
            }

            movingDataArray[blockScript.currentWidthPos, blockScript.currentHeightPos] = 1;
        }
    }

    public bool CanMove(Vector3 move)
    {
        int moveX = (int)move.x;
        int moveY = (int)move.y;

        for (int ia = 0; ia < playGroundWidth; ia++)
        {
            int x = moveX > 0 ? playGroundWidth - 1 - ia : ia;
            for (int ib = 0; ib < playGroundHeight; ib++)
            {
                int y = moveY > 0 ? playGroundHeight - 1 - ib : ib;

                int currentCase = movingDataArray[x, y];
                if (currentCase == 1)
                {
                    int newPosX = x + moveX;
                    int newPosY = y + moveY;
                    if (newPosX < 0 || newPosY < 0 || newPosX > playGroundWidth - 1 || newPosY > playGroundHeight - 1 || idlingDataArray[newPosX, newPosY] == 1)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void Move(Vector3 move)
    {
        int moveX = (int)move.x;
        int moveY = (int)move.y;

        // Déplacer les blocks dans la liste
        for (int ia = 0; ia < playGroundWidth; ia++)
        {
            int x = moveX > 0 ? playGroundWidth - 1 - ia : ia;
            for (int ib = 0; ib < playGroundHeight; ib++)
            {
                int y = moveY > 0 ? playGroundHeight - 1 - ib : ib;

                int currentCase = movingDataArray[x, y];
                if (currentCase == 1)
                {
                    movingDataArray[x, y] = 0;

                    int newPosX = x + moveX;
                    int newPosY = y + moveY;
                    movingDataArray[newPosX, newPosY] = 1;
                }
            }
        }

        foreach (GameObject block in movingBlocksArray)
        {
            BlockScript blockScript = block.GetComponent<BlockScript>();
            blockScript.Move(move);
        }
    }

    public void RemoveLineAndMoveBlockDown(int height)
    {
        for (int i = height; i < playGroundHeight - 1; i++)
        {
            lineFullTracker[i] = lineFullTracker[i + 1];
        }
        lineFullTracker[playGroundHeight - 1] = 0;

        GameObject[] objectsToRemove = new GameObject[playGroundWidth];

        int time = 0;
        foreach(GameObject gameObject1 in idlingBlocksArray)
        {
            BlockScript blockScript = gameObject1.GetComponent<BlockScript>();

            if (blockScript.currentHeightPos == height)
            {
                objectsToRemove[time] = gameObject1;
                time++;
            }
        }

        foreach (GameObject gameObject1 in objectsToRemove)
        {
            idlingBlocksArray.Remove(gameObject1);
            Destroy(gameObject1);
        }

        for (int i = 0; i < playGroundWidth; i++)
        {
            idlingDataArray[i, height] = 0;
        }

        for (int ib = height + 1; ib < playGroundHeight; ib++)
        {
            for (int ia = 0; ia < playGroundWidth; ia++)
            {
                int num = idlingDataArray[ia, ib];

                if (num == 1)
                {
                    idlingDataArray[ia, ib] = 0;
                    idlingDataArray[ia, ib - 1] = 1;

                    foreach (GameObject gameObject in idlingBlocksArray)
                    {
                        BlockScript blockScript = gameObject.GetComponent<BlockScript>();
                        if (blockScript.currentWidthPos == ia && blockScript.currentHeightPos == ib)
                        {
                            blockScript.Move(Vector3.down, true);
                        }
                    }
                }
            }
        }
    }

    public void ResetArray(int[,] array)
    {
        for (int ia = 0; ia < playGroundWidth; ia++)
        {
            for (int ib = 0; ib < playGroundHeight; ib++)
            {
                array[ia, ib] = 0;
            }
        }
    }
}

[System.Serializable]
public class ShapeData
{
    public string name;
    public bool canRotate = true;
    public bool canBeReversed = true;
    public Vector2[] shape;

    public Vector3[] GetShape()
    {
        Vector3[] newShape = new Vector3[shape.Length];

        for (int i = 0; i < shape.Length; i++)
        {
            newShape[i] = shape[i];
        }

        return newShape;
    }
}

public enum Rotation
{
    Right,
    Left
}