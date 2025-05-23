﻿using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public sealed class WeightRandom
{
    private readonly int[] _weights;
    private readonly bool _canRepeatValues;
    private readonly int _totalWeights;
    
    private int _lastValue = -1;

    public WeightRandom(IReadOnlyList<IWeightable> weigtables, bool canRepeatValues = false)
    {
        _canRepeatValues = canRepeatValues;
        
        _weights = new int[weigtables.Count];
        var currentTotalWeight = 0;
        for (var i = 0; i < weigtables.Count; i++)
        {
            currentTotalWeight += weigtables[i].Weight;
            _weights[i] = currentTotalWeight;
        }

        _totalWeights = currentTotalWeight;

        if (_totalWeights == 0 && weigtables.Count != 0)
        {
            throw new Exception("Total weights of multiple objects is 0, impossible to pick different outcomes");
        }
    }

    public WeightRandom(IReadOnlyList<int> weights, bool canRepeatValues = false)
    {
        _canRepeatValues = canRepeatValues;

        _weights = new int[weights.Count];
        
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
            int r = Random.Range(0, _totalWeights);
            blockID = Array.BinarySearch(_weights, r);
            blockID = blockID < 0 ? ~blockID : (blockID + 1) % _weights.Length;
        } while (blockID == _lastValue && !_canRepeatValues);

        _lastValue = blockID;
        return blockID;
    }
}