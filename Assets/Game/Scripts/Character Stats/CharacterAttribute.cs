using System;
using UnityEngine;

namespace GameLogic.CharacterStats
{
    [Serializable]
    public class CharacterAttribute
    {
        [SerializeField] private float _curValue;
        [SerializeField] private float _maxValue;
        [SerializeField] private float _changeRate;

        public bool isTick;

        public Func<float> cacuValueFunc;
        public Func<float> cacuValueRateFunc;

        public float MaxValue
        {
            get
            {
                _maxValue = cacuValueFunc();
                if (_curValue > _maxValue)
                    _curValue = _maxValue;
                return _maxValue;
            }
        }
        public float CurValue
        {
            get => _curValue;
            set
            {
                if (value < 0 || value > MaxValue)
                    return;
                else
                    _curValue = value;
            }
        }
        public float ChangeRate
        {
            get
            {
                _changeRate = cacuValueRateFunc();
                return _changeRate;
            }
            set => _changeRate = value;
        }

        public void Init()
        {
            _maxValue = cacuValueFunc();
            _changeRate = cacuValueRateFunc();
        }

        public void Tick(float deltaTime)
        {
            if (isTick)
                CurValue += deltaTime * _changeRate;
        }
    }
}
