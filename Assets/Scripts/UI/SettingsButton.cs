using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] private GameObject ScoreBoardContainer;
    [SerializeField] private GameObject ScoreBoard_turnOFF;
    private float ButtonCoolDown = 0.25f;
    private float CurrentCoolDown = 0;

    private void Update()
    {
        if (CurrentCoolDown > 0) 
        {
            CurrentCoolDown -= Time.deltaTime;
        }
    }
    public void OpenScoreBoard()
    {
        if (ScoreBoardContainer.transform.localScale == Vector3.zero && CurrentCoolDown <= 0 && ScoreBoard_turnOFF.activeSelf == false) 
        { 
            ScoreBoardContainer.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            CurrentCoolDown = ButtonCoolDown;
            ScoreBoard_turnOFF.SetActive(true);
        }

        if (ScoreBoardContainer.transform.localScale == new Vector3(0.65f, 0.65f, 0.65f) && CurrentCoolDown <= 0 && ScoreBoard_turnOFF.activeSelf == true)
        {
            ScoreBoardContainer.transform.localScale = Vector3.zero;
            CurrentCoolDown = ButtonCoolDown;
            ScoreBoard_turnOFF.SetActive(false);
        }
    }
}
