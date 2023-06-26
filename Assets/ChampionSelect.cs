using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChampionSelect : MonoBehaviour
{
    public SelectManager selectManager;
    public ChampionImage champion;
    public Text championText;
    public RectTransform rt;

    public int startInt;
    public string chosenChampion;

    public List<ChampionImage> kids = new List<ChampionImage>();
    public const float gap = 1.5f;
    public float mouseVelocity;
    float lastPos;
    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        lastPos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0, 0)).x;

        for(int i = 0; i < selectManager.champions.Length; i++)
        {
            string piece = selectManager.champions[i].name;

            ChampionImage newChamp = Instantiate(champion, transform);
            newChamp.image.sprite = Chessman.NameToSprite(piece, "white");
            newChamp.transform.position = new Vector2((i + startInt)*gap, transform.position.y);

            kids.Add(newChamp);
        }

        ChangeKidsPos();
    }

    void Update()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        if (Input.GetMouseButton(0) && Input.mousePosition.y <= Screen.height/2 + rt.anchoredPosition.y + rt.sizeDelta.y/2 && Input.mousePosition.y >= Screen.height/2 + rt.anchoredPosition.y - rt.sizeDelta.y/2)
        {
            mouseVelocity = ( (mousePos.x - lastPos) / Time.deltaTime + mouseVelocity*2 ) / 3;
        }
        else
        {
            float kidPos = kids[ClosestChild()].transform.position.x;

            mouseVelocity = -kidPos * Time.deltaTime * 1000;

            //if ( (kidPos < 0 && mouseVelocity < 0) || (kidPos > 0 && mouseVelocity > 0))
            //{
            //    mouseVelocity = mouseVelocity - kidPos * Time.deltaTime * 5;
            //}
            //else
            //{
            //    mouseVelocity = mouseVelocity + kidPos * Time.deltaTime;
            //}
        }

        if(mouseVelocity > 0)
        {
            if (kids[0].transform.position.x + (mouseVelocity * Time.deltaTime) <= 0)
            {
                ChangeKidsPos();
            }
        }
        else if(mouseVelocity < 0)
        {
            if (kids[kids.Count-1].transform.position.x + (mouseVelocity * Time.deltaTime) >= 0)
            {
                ChangeKidsPos();
            }
        }

        lastPos = mousePos.x;
    }


    void ChangeKidsPos()
    {
        string closestChamp = null;

        for(int i = 0; i < kids.Count; i++)
        {
            ChampionImage champ = kids[i];

            champ.transform.position = new Vector2(champ.transform.position.x + (mouseVelocity * Time.deltaTime), transform.position.y);
        }

        closestChamp = selectManager.champions[ClosestChild()].name;
        chosenChampion = closestChamp;
        championText.text = chosenChampion.ToUpper();
    }

    int ClosestChild()
    {
        float closestPos = 999f;
        int closestChamp = 0;

        for (int i = 0; i < kids.Count; i++)
        {
            ChampionImage champ = kids[i];

            float pos = Mathf.Abs(champ.transform.position.x);

            if (pos < closestPos && champ.usable)
            {
                closestPos = pos;
                closestChamp = i;
            }
        }

        return closestChamp;
    }
}
