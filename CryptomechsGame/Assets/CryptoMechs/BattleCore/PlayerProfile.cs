using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    public PlayerCaravan playerCaravan;
    public List<string> completedStoryIDs;

    public NodePath currentJourneyPath = null;
    public MapNodeBase currentLocation = null;

    public bool IsGoingToHajj = true;
    public bool IsGoingHome = false;

    public float TimeToHajj;
    public float TimeToHome;
    public float InGameTmeMultiplier = 10;

    public float TotalTimeToCompleteHajjInDays = 30 * 6;
    public float TotalTimeToGetHomeInDays = 30 * 6;

    public float DaysToEpochTime(float days)
    {
        return days * 86400;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateGameCriticalTimers()
    {
        if (IsGoingToHajj)
            TimeToHajj -= Time.deltaTime * InGameTmeMultiplier;
        else if (IsGoingHome)
            TimeToHome -= Time.deltaTime * InGameTmeMultiplier;
    }

    public void InitilizeHajjTimerFirstTime()
    {
        TimeToHajj = DaysToEpochTime(TotalTimeToCompleteHajjInDays);
        IsGoingToHajj = true;
        IsGoingHome = false;
    }

    public void InitilizeHomeTimerFirstTime()
    {
        TimeToHome = DaysToEpochTime(TotalTimeToGetHomeInDays);
        IsGoingToHajj = false;
        IsGoingHome = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGameCriticalTimers();
    }
}
