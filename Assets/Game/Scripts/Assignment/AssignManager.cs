using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignManager : MonoBehaviour
{
    static AssignManager s_Instance;
    public static AssignManager Instance
    {
        get
        {
            if (s_Instance != null)
                return s_Instance;

            s_Instance = FindObjectOfType<AssignManager>();

            if (s_Instance != null)
                return s_Instance;

            Create();

            return s_Instance;
        }
        set { s_Instance = value; }
    }

    public List<Assignment> assList;

    static void Create()
    {
        GameObject assignManager = new GameObject("AssignManager");
        s_Instance = assignManager.AddComponent<AssignManager>();
    }
    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        //DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        
    }
}
