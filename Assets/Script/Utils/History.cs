using System;
using System.Collections.Generic;

namespace CakeEngineering
{
    public class History<T> where T : ICloneable
    {
        private readonly List<T> _history = new List<T>();

        private int _index = -1;

        public T Current => _history[_index];

        public bool TryUndo()
        {
            if (_index > 0)
            {
                _index--;
                return true;
            }
            return false;
        }

        public bool TryRedo()
        {
            if (_index < _history.Count)
            {
                _index++;
                return true;
            }
            return false;
        }

        public void CreateNext(T next)
        {
            if (_index + 1 < _history.Count)
            {
                _history[_index + 1] = next;
                _index++;
                if (_index + 1 < _history.Count)
                {
                    var remainingCount = _history.Count - _index - 1;
                    _history.RemoveRange(_index + 1, remainingCount);
                }
            }
            else
            {
                _index++;
                _history.Add(next);
            }
        }

        internal void CreateNext(object v)
        {
            throw new NotImplementedException();
        }
    }
}