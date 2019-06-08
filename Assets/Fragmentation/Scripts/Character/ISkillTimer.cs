using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragmentation.Skill
{
    public enum EnumSkillMode
    {
        /// <summary>
        /// 一次性技能
        /// </summary>
        Disposable = 0,
        /// <summary>
        /// 持续技能
        /// </summary>
        Persistent,
    }
    /// <summary>
    /// 一次性技能释放后即开始计算冷却时间
    /// 持续技能持续时间结束后开始计算冷却时间
    /// </summary>
    interface ISkillTimer
    {
        EnumSkillMode SkillMode { get; set; }
        float CoolingTime { get; set; }
        //float CoolingTimer { get; set; }
        float DurationTime { get; set; }
        //float DurationTimer { get; set; }
        bool IsReady { get; set; }
        bool IsRunning { get; set; }
        void Tick();
    }
}
