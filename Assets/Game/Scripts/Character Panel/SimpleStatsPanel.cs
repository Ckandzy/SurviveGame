using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimpleStatsPanel : MonoBehaviour
{
    public TextMeshProUGUI healthTextMesh;
    public TextMeshProUGUI magicTextMesh;
    public TextMeshProUGUI PowerTextMesh;
    public TextMeshProUGUI HungerTextMesh;

    [SerializeField] Character character;

    private void OnValidate()
    {
        if (character == null)
            character = FindObjectOfType<Character>();
    }
    void Update()
    {
        healthTextMesh.text = "Health: " + character.health.CurValue.ToString("0") + "/" + character.health.MaxValue;
        magicTextMesh.text = "Magic" + character.magic.CurValue.ToString("0") + "/" + character.magic.MaxValue;
        PowerTextMesh.text = "Power: " + character.power.CurValue.ToString("0") + "/" + character.power.MaxValue;
        HungerTextMesh.text = "Hunger: " + character.hunger.CurValue.ToString("0") + "/" + character.hunger.MaxValue;
    }
}
