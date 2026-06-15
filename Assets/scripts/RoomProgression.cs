using System.Collections;
using UnityEngine;
using TMPro;

public class RoomProgression : MonoBehaviour
{
public SimpleScroller scroller;
public SoulCombatController combatController;

[Header("Stage")]
public int stageNumber = 1;
public int roomNumber = 1;
public int roomsPerStage = 10;
public int killsPerAdvance = 10;
public float advanceSeconds = 1.2f;
public int GetRunProgressScore()
{
    return ((stageNumber - 1) * 100) + roomNumber;
}

[Header("Stage UI")]
public TextMeshProUGUI stageText;
public TextMeshProUGUI enemyCounterText;

private int kills;

private bool isAdvancingRoom;
    void Start()
    {
        RefreshStageUI();

        if (combatController != null)
            combatController.ApplyEnemyScaling(stageNumber, roomNumber);
    }
    public void ResetProgression()
    {
        StopAllCoroutines();

        kills = 0;
        isAdvancingRoom = false;

        stageNumber = 1;
        roomNumber = 1;

        RefreshStageUI();

        if (combatController != null)
        {
            combatController.ApplyEnemyScaling(stageNumber, roomNumber);
        }

        Debug.Log("[RoomProgression] Progress reset to Stage 1-1.");
    }
    public void RegisterKill()
    {
        if (isAdvancingRoom) return;

        kills++;

        if (kills >= killsPerAdvance)
        {
            kills = 0;
            AdvanceStageRoom();

            if (combatController != null)
                combatController.ApplyEnemyScaling(stageNumber, roomNumber);

            RefreshStageUI();
            StartCoroutine(AdvanceToNextRoom());
            return;
        }

        RefreshStageUI();
    }

    IEnumerator AdvanceToNextRoom()
    {
        if (isAdvancingRoom) yield break;

        isAdvancingRoom = true;

        if (scroller != null)
        {
            yield return StartCoroutine(scroller.MoveForSeconds(advanceSeconds));
        }
        else
        {
            yield return new WaitForSeconds(advanceSeconds);
        }

        isAdvancingRoom = false;
    }

    void AdvanceStageRoom()
    {
        roomNumber++;

        if (roomNumber > roomsPerStage)
        {
            stageNumber++;
            roomNumber = 1;
        }

        Debug.Log($"[RoomProgression] Advanced to Stage {stageNumber}-{roomNumber}");
    }

    void RefreshStageUI()
    {
        if (stageText != null)
            stageText.text = $"Stage {stageNumber}-{roomNumber}";

        if (enemyCounterText != null)
            enemyCounterText.text = $"{kills}/{killsPerAdvance}";
    }
}