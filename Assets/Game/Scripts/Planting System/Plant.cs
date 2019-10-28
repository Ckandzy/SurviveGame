using System;
using System.Collections;
using System.Collections.Generic;
using GameLogic.CharacterStats;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class Plant : MonoBehaviour, ICollectable
{
    [Header("Reference")]
    [SerializeField] Item item;
    [SerializeField] int amount;
    [SerializeField] Canvas tooltipCanvas;
    [SerializeField] Sprite[] sprites;

    [SerializeField] private Animator m_Animator;
    [SerializeField] private SpriteRenderer m_spriteRender;

    public CharacterAttribute age;
    public float maturingTime;

    protected readonly int m_HashDropPara = Animator.StringToHash("Drop");

    [SerializeField] Vector2 startDropVelocity = new Vector2(0, 0);

    private void Awake()
    {
        age.cacuValueFunc = () => 100;
        age.cacuValueRateFunc = () => 100 / maturingTime;
        age.Init();
    }

    private void OnValidate()
    {
        if (tooltipCanvas == null)
            tooltipCanvas = GetComponentInChildren<Canvas>();
        tooltipCanvas.gameObject.SetActive(false);

        tooltipCanvas.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = item.ItemName;

        if (m_Animator == null)
        {
            m_Animator = GetComponent<Animator>();
        }
        if (m_spriteRender == null)
        {
            m_spriteRender = GetComponent<SpriteRenderer>();
        }

#if UNITY_EDITOR
        if (item == null)
        {
            string[] guids = AssetDatabase.FindAssets(String.Format("name:{0} t:{1}", gameObject.name, typeof(ScriptableObject).ToString()), new string[] { Setting.PlantScriptableSavePath });
            Debug.Log(guids.Length);
            if (guids.Length > 0)
                item = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(ScriptableObject)) as Item;
        }
#endif
        try
        {
            item.itemDropInSceneOBJ.GetComponentInChildren<ItemDropInScene>().spriteRenderer.sprite = item.Icon;
        }
        catch (Exception e)
        {

        }
    }

    private void Update()
    {
        if (age.isTick)
            age.Tick(Time.deltaTime);
        if (sprites.Length > 0)
            m_spriteRender.sprite = sprites[Mathf.CeilToInt(age.CurValue / age.MaxValue * (sprites.Length - 1))];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //如果是玩家显示植物名称并提示可收集
        if (collision.CompareTag("Player"))
        {
            tooltipCanvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E) && (age.CurValue / age.MaxValue > 0.8f))
            {
                if (OnCollect(collision.GetComponent<Character>().inventory))
                {
                    Destroy(gameObject);
                }
                else
                {
                    OnDrop();
                }
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                OnDrop();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //如果是玩家显示植物名称并提示可收集
        if (collision.CompareTag("Player"))
        {
            tooltipCanvas.gameObject.SetActive(false);
        }
    }

    public void OnDrop()
    {
        //生成物品
        if (age.CurValue / age.MaxValue > 0.8f)
        {
            ItemDropInScene itemDropInScene = Instantiate(item.itemDropInSceneOBJ, transform.position + new Vector3(0, 1, 0), Quaternion.identity).GetComponentInChildren<ItemDropInScene>();
            itemDropInScene.item = item;
            itemDropInScene.amount = amount;
        }
        //播放动画
        if (m_Animator != null)
        {
            m_Animator.SetTrigger(m_HashDropPara);
        }
        //销毁植物
        Destroy(gameObject);
    }

    public bool OnCollect(IItemContainer itemContainer)
    {
        Debug.Log(item.ItemName);
        if (itemContainer.CanAddItem(item, amount))
        {
            itemContainer.AddItem(item, amount);
            return true;
        }
        return false;
    }
}
