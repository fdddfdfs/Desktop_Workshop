using System;
using System.Collections.Generic;

namespace Utils.WeightRandom
{
    public class EditableWeightRandom
    {
        private readonly List<int> _weights;
        private readonly bool _canRepeatValues;
        
        private int _totalWeights;
    
        private int _lastValue = -1;
        
        public EditableWeightRandom(IReadOnlyList<int> weights, bool canRepeatValues = false)
        {
            _canRepeatValues = canRepeatValues;

            _weights = new List<int>(weights.Count);

            var currentTotalWeight = 0;
            for (var i = 0; i < weights.Count; i++)
            {
                currentTotalWeight += weights[i] + 1;
                _weights[i] = currentTotalWeight;
            }

            _totalWeights = currentTotalWeight;
        
            if (_totalWeights == 0 && weights.Count != 0)
            {
                throw new Exception("Total weights of multiple objects is 0, impossible to pick different outcomes");
            }
        }
        
        public int GetRandom()
        {
            int blockID;
            do
            {
                int r = UnityEngine.Random.Range(0, _totalWeights);
                blockID = _weights.BinarySearch(r);
                blockID = blockID < 0 ? ~blockID : (blockID + 1) % _weights.Count;
            } while (blockID == _lastValue && !_canRepeatValues);

            _lastValue = blockID;
            return blockID;
        }
        
        public void AddWeight(int weight)
        {
            _totalWeights += weight + 1;
            _weights.Add(_totalWeights);
        }

        public void RemoveWeight(int index)
        {
            int removedWeight = _weights[index] - (index - 1 < 0 ? 0 : _weights[index - 1]);
            _totalWeights -= removedWeight;
            for (int i = index + 1; i < _weights.Count; i++)
            {
                _weights[i] -= removedWeight;
            }

            _weights.Remove(index);
        }
    }
}