using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fragmentation.Skill
{
    [Serializable]
    public class SkillBase : ISkillTimer
    {
        [SerializeField]
        private EnumSkillMode _skillMode;
        [SerializeField]
        private float _coolingTime;
        [SerializeField]
        private bool _isReady;
        [SerializeField]
        private float _durationTime;

        //Debug Serialize
        [SerializeField]
        private float _coolingTimer;
        [SerializeField]
        private float _durationTimer;
        [SerializeField]
        private bool _isRunning;

        public float CoolingTime { get => _coolingTime; set => _coolingTime = value; }
        public float CoolingTimer { get => _coolingTimer; set => _coolingTimer = value; }
        public bool IsReady { get => _isReady; set => _isReady = value; }
        public EnumSkillMode SkillMode { get => _skillMode; set => _skillMode = value; }
        public float DurationTimer { get => _durationTimer; set => _durationTimer = value; }
        public float DurationTime { get => _durationTime; set => _durationTime = value; }
        public bool IsRunning { get => _isRunning; set => _isRunning = value; }

        public virtual void EndSkill()
        {
            //当提前结束持续性技能时，直接将其置于冷却状态
            if (SkillMode == EnumSkillMode.Persistent)
                if (!IsReady && DurationTimer > 0f)
                {
                    DurationTimer = -1f;
                    IsRunning = false;
                }
        }

        public virtual void StartSkill()
        {
            IsReady = false;
            if (SkillMode == EnumSkillMode.Persistent)
            {
                IsRunning = true;
            }
        }

        public SkillBase()
        {
        }

        public void Tick()
        {
            if (!IsReady)
            {
                switch (SkillMode)
                {
                    case EnumSkillMode.Disposable:
                        {
                            if ((CoolingTimer -= Time.deltaTime) < 0f)
                            {
                                IsReady = true;
                                CoolingTimer = CoolingTime;
                            }
                        }
                        break;
                    case EnumSkillMode.Persistent:
                        {
                            if (DurationTimer < 0f)
                            {
                                IsRunning = false;
                                if ((CoolingTimer -= Time.deltaTime) < 0f)
                                {
                                    IsReady = true;
                                    CoolingTimer = CoolingTime;
                                    DurationTimer = DurationTime;
                                }
                            }
                            else
                                DurationTimer -= Time.deltaTime;
                        }
                        break;
                }
            }
        }
    }
}
