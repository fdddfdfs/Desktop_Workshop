﻿using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public sealed class Achievements : ResourcesSingleton<Achievements, Achievements.AchievementsResourceName>
{
    [SerializeField] private List<AchievementData> _achievementsData;

    private DataStringArray _achievements;
    private HashSet<string> _givenAchievements;
    private Dictionary<string, DataInt> _achievementsProgress;
    private Dictionary<string, AchievementData> _achievementByName;

    private void Awake()
    {
        _achievements = new DataStringArray(nameof(Achievements));

        _givenAchievements = new HashSet<string>(_achievements.Value);

        _achievementsProgress = new Dictionary<string, DataInt>();

        _achievementByName = new Dictionary<string, AchievementData>();
        for (var i = 0; i < _achievementsData.Count; i++)
        {
            _achievementByName[_achievementsData[i].Name] = _achievementsData[i];
        }
    }

    private void Start()
    {
        if (Instance == this)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogWarning("Steam manager don`t initialized, cannot grant previous stored achievements");
                return;
            }
            
            foreach (string achievement in _achievements.Value)
            {
                SteamUserStats.GetAchievement(achievement, out bool achieved);
                if (!achieved)
                {
                    SteamUserStats.SetAchievement(achievement);
                }
            }

            SteamUserStats.StoreStats();
        }
    }

    public void GetAchievement(string achievementName)
    {
        if (_givenAchievements.Contains(achievementName)) return;

        _givenAchievements.Add(achievementName);
        _achievements.AddElement(achievementName);

        if (!SteamManager.Initialized)
        {
            Debug.LogWarning(
                "Steam Manager don`t initialized, achievement stored and will be granted on next session");
            return;
        }
        
        SteamUserStats.GetAchievement(achievementName, out bool achieved);
        if (!achieved)
        {
            SteamUserStats.SetAchievement(achievementName);
            SteamUserStats.StoreStats();
        }
    }
    
    public void GetAchievement(AchievementData achievementData)
    {
        if (achievementData == null) return;
        
        GetAchievement(achievementData.Name);
    }

    public void IncreaseProgress(string achievementName, int progress)
    {
        if (_givenAchievements.Contains(achievementName)) return;

        if (!_achievementByName.ContainsKey(achievementName))
        {
            throw new Exception($"{nameof(Achievements)} doesnt have data for {achievementName}");
        }

        int requiredProgress = _achievementByName[achievementName].RequiredProgress;

        DataInt currentProgress = _achievementsProgress.GetValueOrDefault(
            achievementName,
            new DataInt(achievementName, 0));

        if (currentProgress.Value + progress >= requiredProgress)
        {
            GetAchievement(achievementName);
        }
        else
        {
            currentProgress.Value += progress;
            _achievementsProgress[achievementName] = currentProgress;
        }
    }

    public void ResetProgress(string achievementName)
    {
        if (_givenAchievements.Contains(achievementName)) return;

        if (!_achievementByName.ContainsKey(achievementName))
        {
            throw new Exception($"{nameof(Achievements)} doesnt have data for {achievementName}");
        }
        
        DataInt currentProgress = _achievementsProgress.GetValueOrDefault(
            achievementName,
            new DataInt(achievementName, 0));

        currentProgress.Value = 0;
        _achievementsProgress[achievementName] = currentProgress;
    }
    
    public int GetAchievementProgress(string achievementName)
    {
        if (_achievementsProgress.TryGetValue(achievementName, out DataInt progress))
        {
            return progress.Value;
        }

        throw new Exception($"{nameof(Achievements)} doesnt have data for {achievementName}");
    }
    
    public class AchievementsResourceName : ResourceName
    {
        public override string Name => "Achievements/Achievements";
    }
}