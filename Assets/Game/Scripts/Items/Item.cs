using System.Text;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Items/Item")]
public class Item : ScriptableObject
{
    [Header("Reference")]
    public GameObject itemDropInSceneOBJ;

    [SerializeField] string id;
	public string ID { get { return id; } }
	public string ItemName;
    public string ItemIntroduction;
	public Sprite Icon;
	[Range(1,999)]
	public int MaximumStacks = 1;

	protected static readonly StringBuilder sb = new StringBuilder();

	#if UNITY_EDITOR
	protected virtual void OnValidate()
	{
		string path = AssetDatabase.GetAssetPath(this);
		id = AssetDatabase.AssetPathToGUID(path);
        
        if (true/*itemDropInSceneOBJ == null*/)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { Setting.ItemDropInScenePrefabSavePath });
                foreach(var guid in guids)
                {
                    if (AssetDatabase.GUIDToAssetPath(guid).Contains(fileName))
                    { 
                        itemDropInSceneOBJ = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(GameObject)) as GameObject;
                        itemDropInSceneOBJ.GetComponentInChildren<ItemDropInScene>().item = this;
                    }
                }
            }
        }
        if (true)
        {
            //Icon = itemDropInSceneOBJ.GetComponentInChildren<ItemDropInScene>().spriteRenderer.sprite;
        }
    }
	#endif

	public virtual Item GetCopy()
	{
		return this;
	}

	public virtual void Destroy()
	{

	}

	public virtual string GetItemType()
	{
		return "";
	}

	public virtual string GetDescription()
	{
		return "";
	}
}
