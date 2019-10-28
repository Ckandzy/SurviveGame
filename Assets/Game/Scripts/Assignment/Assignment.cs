using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Assignment
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string assName;
    /// <summary>
    /// 任务ID
    /// </summary>
    public int assID;
    /// <summary>
    /// 任务概览
    /// </summary>
    [TextArea(3, 10)]
    public string assContext;
    /// <summary>
    /// 任务提示
    /// </summary>
    [TextArea(1, 1)]
    public string hint;
    /// <summary>
    /// 任务对话
    /// </summary>
    public Dialogue dialogue;

    public bool hasTriggered;
    public bool hasCompleted;
}
