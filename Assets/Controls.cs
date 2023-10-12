using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Controls : MonoBehaviour
{
    enum Prog
    {
        nothing, cup, water, tea, milk
    }
    Prog Progress
    {
        get => _progress;
        set
        {
            if ((int)value <= (int)_progress || (int)value - (int)_progress > 1)
                return;
            _progress = value;
        }
    } Prog _progress = Prog.nothing;
    
    void RefreshSprite()
    {
        int i = lastDir switch
        {
            {x: 0, y: 1} => 0,
            {x: 1, y: 0} => 1,
            {x: -1, y: 0} => 2,
            {x: 0, y: -1} => 3
        };
        SR.sprite = Progress switch
        {
            Prog.nothing => Resources.LoadAll<Sprite>("Girl_Nothing")[i],
            Prog.cup => Resources.LoadAll<Sprite>("Girl_Cup")[i],
            Prog.water => Resources.LoadAll<Sprite>("Girl_Water")[i],
            Prog.tea => Resources.LoadAll<Sprite>("Girl_Tea")[i],
            Prog.milk => Resources.LoadAll<Sprite>("Girl_Milk")[i],
        };
    }

    Tilemap Floors;
    Tilemap Objects;
    Tilemap Items;
    SpriteRenderer SR;
    void Awake()
    {
        Floors = GameObject.Find("Map").transform.Find("Floors").GetComponent<Tilemap>();
        Objects = GameObject.Find("Map").transform.Find("Objects").GetComponent<Tilemap>();
        Items = GameObject.Find("Map").transform.Find("Items").GetComponent<Tilemap>();
        SR = GetComponent<SpriteRenderer>();
    }

    Vector3? targetPos
    {
        get => _targetPos;
        set
        {
            _targetPos = value;
            if (value != null)
                lastDir = (Vector3)(value - transform.position);
        }
    } Vector3? _targetPos = null;
    Vector3 lastDir = new Vector3(0, 1);
    
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            AttemptPickup();
        
        // New Input
        if (targetPos == null)
        {
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                targetPos = transform.position + new Vector3(1, 0);
            }
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                targetPos = transform.position + new Vector3(-1, 0);
            }
            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                targetPos = transform.position + new Vector3(0, 1);
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                targetPos = transform.position + new Vector3(0, -1);
            }
            
            RefreshSprite();
            
            if (targetPos != null && CheckCollision((Vector3)targetPos))
                targetPos = null;
        }
        else
        {
            Vector3 step = ((Vector3)targetPos - transform.position).normalized * (3f * Time.deltaTime);
            transform.position += step;
            
            // Arrived
            if ((transform.position - (Vector3)targetPos).magnitude < step.magnitude)
            {
                transform.position = (Vector3)targetPos;
                targetPos = null;
                ArrivedAt(transform.position);
            }
        }
    }

    void AttemptPickup()
    {
        Vector3 pos = transform.position;
        
        foreach (Vector3 c in new[]
                 {
                     pos + new Vector3(1, 0), pos + new Vector3(-1, 0),
                     pos + new Vector3(0, 1), pos + new Vector3(0, -1)
                 })
        {
            Vector3Int cellPos = Items.WorldToCell(c);
            if (!Items.HasTile(cellPos)) continue;
            
            Enum.TryParse(Items.GetTile(cellPos).name.ToLower(), out Prog p);
            Progress = p;
            
            if (Progress == p)
                Items.SetTile(cellPos, null);

            RefreshSprite();
            break;
        }
    }
    void ArrivedAt(Vector3 pos)
    {
        Vector3Int v = Objects.WorldToCell(pos);
        if (Objects.HasTile(v))
        {
            TileBase gt = Objects.GetTile(v);

            if (gt.name.StartsWith("E_Open"))
            {
                Objects.SetTile(v, Resources.Load<TileBase>("E_Closed"));
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("woodhit"), transform.position);
            }
            else if (gt.name.StartsWith("M_Open"))
            {
                Objects.SetTile(v, Resources.Load<TileBase>("M_Closed"));
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("woodhit"), transform.position);
            }
            else if (gt.name.StartsWith("K_Open"))
            {
                Objects.SetTile(v, Resources.Load<TileBase>("K_Closed"));
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("woodhit"), transform.position);
            }
        }

        // Finish
        if (Progress == Prog.milk && Objects.GetTile(Objects.WorldToCell(pos))?.name == "queen")
        {
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("clap"), transform.position);

            StartCoroutine(wait());
            IEnumerator wait()
            {
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
    bool CheckCollision(Vector3 pos)
    {
        if (!Floors.HasTile(Floors.WorldToCell(pos)))
            return true;
        if (Objects.GetTile(Objects.WorldToCell(pos))?.name is "Table" or "E_Closed" or "K_Closed" or "M_Closed")
            return true;
        return false;
    }
}