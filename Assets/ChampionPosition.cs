using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChampionPosition : MonoBehaviour
{
    public int id;
    public SelectManager manager;
    public SelectManager.champion champion;
    public ChampionPosition otherPiece;
    [HideInInspector] public Vector2Int matrix;

    public RectTransform startLocation;
    public Image champPos;
    List<Image> allPos = new List<Image>();

    [HideInInspector] public RectTransform rt;
    OnClickDetector detector;
    public bool dragging;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        detector = GetComponent<OnClickDetector>();

        rt.anchoredPosition = startLocation.anchoredPosition;
    }

    void Update()
    {
        if (detector.pressed)
        {
            detector.pressed = false;
            StartDrag();
        }

        if (Input.GetMouseButtonUp(0) && dragging)
        {
            EndDrag();
        }

        if (dragging)
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x - Screen.width/2, Input.mousePosition.y - Screen.height/2);
            rt.anchoredPosition = mousePos;
        }
    }

    void StartDrag()
    {
        dragging = true;

        foreach(string champ in champion.replacements)
        {
            Vector2Int[] locations = PieceToPos(champ);
            
            foreach(Vector2Int _location in locations)
            {
                if (otherPiece.matrix == _location) continue;

                Image newPos = Instantiate(champPos, manager.board);
                newPos.rectTransform.anchoredPosition = manager.IntToPos(_location.x, _location.y);
                allPos.Add(newPos);
            }
        }
    }

    void EndDrag()
    {
        dragging = false;

        RectTransform closest = startLocation;
        float closestDistance = DistanceBetweenVectors(rt.anchoredPosition, startLocation.anchoredPosition);
        bool onBoard = false;

        foreach (Image p in allPos)
        {
            float i = DistanceBetweenVectors(p.rectTransform.anchoredPosition, rt.anchoredPosition);

            if (i < closestDistance)
            {
                closestDistance = i;
                closest = p.rectTransform;
                onBoard = true;
            }
        }

        rt.anchoredPosition = closest.anchoredPosition;
        if(onBoard) matrix = manager.PosToInt(rt.anchoredPosition.x, rt.anchoredPosition.y);
        else matrix = new Vector2Int(-1, -1);

        if (id == 1) manager.pos1 = matrix;
        if (id == 2) manager.pos2 = matrix;

        foreach (Image p in allPos)
        {
            Destroy(p.gameObject);
        }
        allPos.Clear();
    }

    Vector2Int[] PieceToPos(string piece)
    {
        switch (piece)
        {
            case "queen":
                return new Vector2Int[] { new Vector2Int(3, 0) };
            case "king":
                return new Vector2Int[] { new Vector2Int(4, 0) };
            case "bishop":
                return new Vector2Int[] { new Vector2Int(2, 0), new Vector2Int(5, 0) };
            case "knight":
                return new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(6, 0) };
            case "rook":
                return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(7, 0) };
            case "pawn":
                return new Vector2Int[] { new Vector2Int(3, 1), new Vector2Int(4, 1) };
        }
        return null;
    }

    float DistanceBetweenVectors(Vector2 a, Vector2 b)
    {
        return Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
    }
}
