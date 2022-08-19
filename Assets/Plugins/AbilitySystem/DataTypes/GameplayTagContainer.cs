using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// struct for gameplay tag containers
[Serializable]
public struct GameplayTagContainer
{
    [SerializeField]
    private List<GameplayTag> gameplayTags;

    public List<GameplayTag> GetGameplayTags()
    {
        return gameplayTags;
    }

    public bool IsEmpty()
    {
        return gameplayTags.Count == 0 ? true : false;
    }

    public void AddTag(GameplayTag tag)
    {
        gameplayTags.Add(tag);
    }

    public void AddAllTags(List<GameplayTag> tags)
    {
        foreach (GameplayTag tag in tags)
        {
            gameplayTags.Add(tag);
        }
    }

    public void RemoveTag(GameplayTag tag)
    {
        gameplayTags.Remove(tag);
    }

    public void RemoveAllTags(List<GameplayTag> tags)
    {
        foreach (GameplayTag tag in tags)
        {
            gameplayTags.Remove(tag);
        }
    }

    public void ClearAllTags()
    {
        gameplayTags.Clear();
    }

    public bool ContainsTag(GameplayTag tag)
    {
        return gameplayTags.Contains(tag);
    }

    public bool ContainsAllTags(List<GameplayTag> tags)
    {
        if (tags.Count == 0)
        {
            return true;
        }

        foreach (GameplayTag tag in tags)
        {
            if (!gameplayTags.Contains(tag))
            {
                return false;
            }
        }

        return true;
    }

    public bool ContainsAnyTag(List<GameplayTag> tags)
    {
        if (tags.Count == 0)
        {
            return false;
        }

        foreach (GameplayTag tag in tags)
        {
            if (gameplayTags.Contains(tag))
            {
                return true;
            }
        }

        return false;
    }
}
