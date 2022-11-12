using System.Collections;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    public BlockStates state = BlockStates.Moving;
    private bool forceMove = false;
    public Vector3 targetedPos;
    public int currentWidthPos;
    public int currentHeightPos;
    public float speed;

    public Vector3 posFromCenter;

    private void Update()
    {
        if (state == BlockStates.Moving || forceMove)
        {
            Vector3 dir = targetedPos - transform.position;
            transform.Translate(dir * speed * Time.deltaTime);
        }
    }

    public void ResetTargetedPos()
    {
        targetedPos = transform.position;
    }

    public void SetHeightPos(int width, int height)
    {
        currentWidthPos = width;
        currentHeightPos = height;
    }

    public void SetPosFromCenter(Vector3 vector3)
    {
        posFromCenter = vector3;
    }

    public Vector3 GetPosFromCenter()
    {
        return posFromCenter;
    }

    public void SetIdleState()
    {
        StartCoroutine(SetIdleStateDelay());
    }

    IEnumerator SetIdleStateDelay()
    {
        yield return new WaitForSeconds(0.8f);
        state = BlockStates.Idling;
        transform.position = targetedPos;
    }

    public void Move(Vector3 move, bool lineFull = false)
    {
        targetedPos += move * GameManager.instance.stepBetweenBlocksDistance;
        currentWidthPos += (int)move.x;
        currentHeightPos += (int)move.y;

        if (lineFull)
        {
            StartCoroutine(ForceMove());
        }
    }

    IEnumerator ForceMove()
    {
        forceMove = true;
        yield return new WaitForSeconds(1);
        forceMove = false;
        transform.position = targetedPos;
    }

    public void Rotate(Rotation rotation)
    {
        int x = rotation == Rotation.Left ? -(int)posFromCenter.y : (int)posFromCenter.y;
        int y = rotation == Rotation.Left ? (int)posFromCenter.x : -(int)posFromCenter.x;
        int centerShapeX = currentWidthPos - (int)posFromCenter.x;
        int centerShapeY = currentHeightPos - (int)posFromCenter.y;
        
        posFromCenter = new Vector3(x, y, 0);

        currentWidthPos = x + centerShapeX;
        currentHeightPos = y + centerShapeY;
        targetedPos = new Vector3(currentWidthPos, currentHeightPos, 0) * GameManager.instance.stepBetweenBlocksDistance + GameManager.instance.cornerBottomLeftTransform.position;
    }
}

public enum BlockStates
{
    Idling,
    Moving
}