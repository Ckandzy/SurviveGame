using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BossArea : MonoBehaviour
{
    public GameObject Boss;
    public bool hasActive = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasActive)
        {
            Boss.SetActive(true);
            hasActive = true;
            //Boss.GetComponent<BossBehaviour>().SetEnd(FragmenMgr.Instance.GetEnd());
        }
    }
}
