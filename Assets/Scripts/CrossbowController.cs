using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossbowController : MonoBehaviour, IPulsable
{
    public MoveDirection direction;
    public Material lineMaterial;

    private GameObject killLine;

    // Start is called before the first frame update
    void Start()
    {

    }

    void DrawKillLine(Vector3 start, Vector3 end, Color color)
    {
        killLine = new GameObject();
        killLine.transform.position = start;
        killLine.AddComponent<LineRenderer>();
        LineRenderer lr = killLine.GetComponent<LineRenderer>();
        lr.sortingLayerName = "GridObject";
        lr.material = lineMaterial;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    void EraseKillLine()
    {
        GameObject.Destroy(killLine);
        killLine = null;
    }

    void Update()
    {

    }

    public void pulse()
    {
        Vector3 checkVector = Global.moveVectors[direction];
        Vector3 lineStartVector = transform.position + Global.moveVectors[direction] * 0.3f;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkVector);
        PlayerController playerController = hit.collider.gameObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            DrawKillLine(lineStartVector, hit.collider.transform.position, Color.red);
            playerController.killWithCrossbow();
        }
        else if (killLine != null)
        {
            EraseKillLine();
        }
    }

    public int pulsePriority()
    { 
        return 1;
    }

    public bool canPulseDeadPlayer()
    {
        return true;
    }
}
