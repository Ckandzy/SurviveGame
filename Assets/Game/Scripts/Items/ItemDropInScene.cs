using System.Text;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class ItemDropInScene : MonoBehaviour, ICollectable
{
    [Header("Reference")]
    public Item item;
    public int amount = 1;

    //缓存动画状态机
    [Header("Reference")]
    [SerializeField] Animator m_Animator;
    [SerializeField] Rigidbody2D m_Rigidbody;
    [SerializeField] Canvas tooltipCanvas;
    public SpriteRenderer spriteRenderer;


    [Space]
    [Header("Setting")]
    [SerializeField] Vector2 startDropVelocity = new Vector2(0, 0);

    protected readonly int m_HashDropPara = Animator.StringToHash("Drop");

    private void OnValidate()
    {
        if (tooltipCanvas == null)
            tooltipCanvas = transform.parent.GetComponentInChildren<Canvas>();
        if (tooltipCanvas != null)
            tooltipCanvas.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = item.ItemName;
        if (m_Animator == null)
            m_Animator = GetComponentInParent<Animator>();
        if (m_Rigidbody == null)
            m_Rigidbody = GetComponentInParent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInParent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (m_Animator != null)
            m_Animator.SetTrigger(m_HashDropPara);
        if (m_Rigidbody != null)
        {
            m_Rigidbody.velocity = startDropVelocity;
        }
    }

    private void Start()
    {
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (OnCollect(collision.GetComponent<Character>().inventory))
                {
                    Destroy(gameObject.GetComponentInParent<Rigidbody2D>().gameObject);
                }
            }
        }
    }
    public bool OnCollect(IItemContainer itemContainer)
    {
        if (itemContainer.CanAddItem(item, amount))
        {
            itemContainer.AddItem(item, amount);
            return true;
        }
        return false;
    }
}
