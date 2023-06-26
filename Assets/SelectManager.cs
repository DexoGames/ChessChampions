using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    public Setting gameMode;
    readonly Vector2Int negative1 = new Vector2Int(-1, -1);

    static readonly string[] queen = { "queen" };
    static readonly string[] rook = { "rook" };
    static readonly string[] king = { "king" };
    static readonly string[] knight = { "knight" };
    static readonly string[] bishop = { "bishop" };
    static readonly string[] main4 = { "queen", "knight", "rook", "bishop" };

    public champion[] champions =
        {
        new champion { name = "jesus", replacements = queen },
        new champion { name = "sniper", replacements = bishop },
        new champion { name = "imposter", replacements = queen },
        new champion { name = "ninja", replacements = knight },
        new champion { name = "mimic", replacements = main4 },
        new champion { name = "hikaru", replacements = king },
        //new champion { name = "wizard", replacements = rook },
        new champion { name = "necromancer", replacements = queen },
        new champion { name = "pheonix", replacements = queen },
        new champion { name = "pirate", replacements = queen }
        };

    public struct champion
    {
        public string name;
        public string[] replacements;
    }

    public int state;
    [SerializeField] GameObject[] menus;

    public RectTransform board;
    public ChampionPosition piece1;
    public ChampionPosition piece2;

    public ChampionSelect select1;
    public ChampionSelect select2;

    public string champ1;
    public string champ2;

    public Vector2Int pos1;
    public Vector2Int pos2;

    public Text timer;
    public Button finishButton;

    int ChampToInt(string champ)
    {
        for(int i = 0; i < champions.Length; i++)
        {
            if (champions[i].name == champ) return i;
        }
        Debug.LogError("ERROR: No Champion with that Name");
        return 0;
    }

    void Start()
    {
        if(gameMode.currentOption == 1) FindObjectOfType<OnlineSelect>().Setup();
    }

    void Update()
    {
        if(champ1 != select1.chosenChampion)
        {
            champ1 = select1.chosenChampion;
            int champInt = ChampToInt(champ1);

            for(int i = 0; i < champions.Length; i++)
            {
                bool enable = false;

                foreach(string replacement in champions[i].replacements)
                {
                    foreach (string champ1replacement in champions[champInt].replacements)
                    {
                        if(replacement != champ1replacement)
                        {
                            enable = true;
                        }
                    }
                }

                select2.kids[i].usable = enable;
            }
        }

        if (champ2 != select2.chosenChampion)
        {
            champ2 = select2.chosenChampion;
            int champInt = ChampToInt(champ2);

            for (int i = 0; i < champions.Length; i++)
            {
                bool enable = false;

                foreach (string replacement in champions[i].replacements)
                {
                    foreach (string champ2replacement in champions[champInt].replacements)
                    {
                        if (replacement != champ2replacement)
                        {
                            enable = true;
                        }
                    }
                }

                select1.kids[i].usable = enable;
            }
        }
    }

    public Vector2 IntToPos(int x, int y)
    {
        float posX = (x-3.5f) * board.rect.width/8 + board.anchoredPosition.x;

        float posY = (y - 1.5f) * board.rect.height/8 + board.anchoredPosition.y;

        return new Vector2(posX, posY);
    }

    public Vector2Int PosToInt(float x, float y)
    {
        float posX = ((x - board.anchoredPosition.x) / board.rect.width * 8) + 3.5f;
        float posY = ((y - board.anchoredPosition.y) / board.rect.height * 8) + 1.5f;

        return new Vector2Int(Mathf.RoundToInt(posX), Mathf.RoundToInt(posY));
    }

    public void ChangeStatus()
    {
        if(state == 0)
        {
            state = 1;
        }
        else
        {
            state = 0;
        }

        menus[state].SetActive(true);
        menus[menus.Length-state-1].SetActive(false);

        if(state == 1)
        {
            piece1.GetComponent<Image>().sprite = Chessman.NameToSprite(champ1, "white");
            piece2.GetComponent<Image>().sprite = Chessman.NameToSprite(champ2, "white");
            piece1.champion = champions[ChampToInt(champ1)];
            piece2.champion = champions[ChampToInt(champ2)];
            piece1.matrix = negative1;
            piece2.matrix = negative1;
            pos1 = negative1;
            pos2 = negative1;
        }
    }
    
    public void ConfirmSelection()
    {
        if (pos1 == negative1 || pos2 == negative1) return;

        if(gameMode.currentOption == 1)
        {
            FindObjectOfType<OnlineSelect>().EndSelection();
        }
    }
}