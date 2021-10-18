using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossbowController : MonoBehaviour, IPulsable
{
    public string direction;
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

    // Update is called once per frame
    void Update()
    {

    }

    public void pulse()
    {
        Vector2 checkVector;
        Vector3 lineStartVector;
        switch (direction)
        {
            case "right":
                checkVector = Vector2.right;
                lineStartVector = new Vector3(transform.position.x + 0.3f, transform.position.y);
                break;
            case "left":
                checkVector = Vector2.left;
                lineStartVector = new Vector3(transform.position.x - 0.3f, transform.position.y);
                break;
            case "up":
                checkVector = Vector2.up;
                lineStartVector = new Vector3(transform.position.x, transform.position.y + 0.3f);
                break;
            case "down":
                checkVector = Vector2.down;
                lineStartVector = new Vector3(transform.position.x, transform.position.y - 0.3f);
                break;
            default:
                checkVector = Vector2.right;
                lineStartVector = new Vector3(transform.position.x + 0.3f, transform.position.y);
                break;
        }
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
