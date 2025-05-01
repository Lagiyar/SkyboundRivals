using UnityEngine;

public class Elevator : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float topY = 5f;
    public float bottomY = 0f;
    public float waitTime = 5f;

    private bool goingUp = true;
    private bool isWaiting = false;

    private void Update()
    {
        if (isWaiting) return;

        Vector3 pos = transform.position;

        if (goingUp)
        {
            pos.y += moveSpeed * Time.deltaTime;
            if (pos.y >= topY)
            {
                pos.y = topY;
                goingUp = false;
                StartCoroutine(WaitAtTop());
            }
        }
        else
        {
            pos.y -= moveSpeed * Time.deltaTime;
            if (pos.y <= bottomY)
            {
                pos.y = bottomY;
                goingUp = true;
            }
        }

        transform.position = pos;
    }

    private System.Collections.IEnumerator WaitAtTop()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
    }
}
