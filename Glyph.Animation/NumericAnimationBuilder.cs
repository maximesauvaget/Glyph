using System.Collections.Generic;
using System.Linq;

namespace Glyph.Animation
{
    public class NumericAnimationBuilder<T> : IAnimationBuilder<T>
    {
        private readonly List<AnimationKeyPoint<T>> _list;
        public T StartValue { get; set; }
        public bool Loop { get; set; }

        public T this[float time, ValueEasingDelegate<T> easing = null]
        {
            set
            {
                var step = new AnimationKeyPoint<T>
                {
                    Time = time,
                    Value = value,
                    Easing = easing ?? ((start, end, advance) => advance < 1f ? start : end) 
                };

                _list.Add(step);
                _list.Sort();
            }
        }

        public NumericAnimationBuilder()
        {
            _list = new List<AnimationKeyPoint<T>>();
        }

        public IAnimation<T> Create()
        {
            var builder = new AnimationBuilder<T>
            {
                Loop = Loop
            };

            if (!_list.Any())
            {
                builder[0, 0] = (ref T animatable, float advance) => animatable = StartValue;
                return builder.Create();
            }

            float currentTime = 0;
            T currentValue = StartValue;
            
            foreach (AnimationKeyPoint<T> step in _list)
            {
                T previousValue = currentValue;

                builder[currentTime, step.Time] = (ref T animatable, float advance)
                    => animatable = step.Easing(previousValue, step.Value, advance);

                currentTime = step.Time;
                currentValue = step.Value;
            }

            return builder.Create();
        }
    }
}