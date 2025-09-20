using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using UnityEngine;
using TMPro;

[Serializable]
public class ObjectiveStage
{
    public string header = "NEW SPECIES UNLOCKED";
    public string title;
    [TextArea(2, 4)] public string body;
    [TextArea(1, 3)] public string hint;
    public string targetTag;                 // Fish / Shark / Dolphin
    public int requiredCount = 1;
    public Sprite backgroundSprite;
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    public ObjectiveOverlay overlay;

    [Header("Stages in order")]
    public List<ObjectiveStage> stages = new List<ObjectiveStage>();

    [Header("Completion screen")]
    public Sprite finalSprite;
    [TextArea(1, 3)] public string finalTitle = "GOOD JOB";
    [TextArea(2, 4)]
    public string finalMessage =
        "I hope you enjoyed exploring and that you learnt something about these native species.";

    int currentStageIndex = -1;
    int currentCount = 0;

    // Only pick up fish if it's an objective, and clear inventory for new missions
    public bool isObjective = false;
    public bool clearInventory = false;

    // Display mission and progress on player screen
    [Header("HUD")]
    public TMP_Text objectiveHUDText;
    public TMP_Text progressHUDText;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (overlay == null)
        {
            overlay = FindObjectOfType<ObjectiveOverlay>(true);
        }

        if (overlay == null)
        {
            UnityEngine.Debug.LogError("[ObjectiveManager] No ObjectiveOverlay found in scene.");
            return;
        }

        StartCoroutine(ShowFirstStageNextFrame());
    }

    IEnumerator ShowFirstStageNextFrame()
    {
        yield return null;
        AdvanceToNextStage();
    }

    ObjectiveStage Current =>
        (currentStageIndex >= 0 && currentStageIndex < stages.Count) ? stages[currentStageIndex] : null;

    public void NotifyCollected(string collectedTag)
    {
        isObjective = false;
        clearInventory = false;
        var stage = Current;
        if (stage == null) return;

        if (string.Equals(collectedTag, stage.targetTag, StringComparison.OrdinalIgnoreCase))
        {
            isObjective = true;
            currentCount++;
            UpdateHUDProgress();
            UnityEngine.Debug.Log($"[ObjectiveManager] Collected {currentCount}/{stage.requiredCount} {stage.targetTag}");
            if (currentCount >= stage.requiredCount)
            {
                clearInventory = true;
                AdvanceToNextStage();
            }
        }
    }

    void AdvanceToNextStage()
    {
        currentStageIndex++;
        currentCount = 0;

        if (currentStageIndex < stages.Count)
        {
            var s = stages[currentStageIndex];
            string objLine = $"OBJECTIVE: Find {s.requiredCount} {Pluralize(s.title, s.requiredCount)}";

            overlay.Show(
                s.backgroundSprite,
                s.header,
                s.title,
                s.body,
                objLine,
                $"Hint - {s.hint}",
                true
            );

            if (currentStageIndex == stages.Count - 1 && s.backgroundSprite != null && finalSprite == null)
                finalSprite = s.backgroundSprite;
            UpdateHUDProgress();
            UnityEngine.Debug.Log($"[ObjectiveManager] Stage {currentStageIndex + 1}/{stages.Count} shown: {s.title}");
        }
        else
        {
            overlay.ShowMessage(finalSprite, finalTitle, finalMessage);
            UnityEngine.Debug.Log("[ObjectiveManager] All stages complete — showing final message.");
        }
    }

    string Pluralize(string name, int count)
    {
        if (count == 1) return name;
        if (name.EndsWith("s")) return name;
        return name + "'s";
    }

    // Increase oxygen when player picks up litter
    [Header("Litter popup")]
    public Sprite litterSprite;
    [TextArea(2, 4)]
    public string litterMessage =
        "Marine debris can injure or kill marine wildlife and degrade their habitats. Thank you for being a tidy Kiwi and keeping our oceans clean. Lung capacity increased by 10 seconds.";
    public string litterTitle = "LITTER COLLECTED!";

    public void ShowLitterPopup()
    {
        if (overlay == null) overlay = FindObjectOfType<ObjectiveOverlay>(true);
        if (overlay != null)
        {
            overlay.ShowMessage(litterSprite, litterTitle, litterMessage);
        }
    }

    // Take player back to spawn point when they run out of air
    [Header("Out-of-breath popup")]
    public Sprite outOfBreathSprite;
    [TextArea(2, 4)]
    public string outOfBreathMessage =
        "You ran out of breath while diving! Remember to surface for air, or look for litter to get bonus oxygen. Go back to land to take a small break to recover your breath.";
    public string outOfBreathTitle = "OUT OF BREATH";

    public void ShowOutOfBreathPopup()
    {
        if (overlay == null) overlay = FindObjectOfType<ObjectiveOverlay>(true);
        if (overlay != null)
        {
            overlay.ShowMessage(outOfBreathSprite, outOfBreathTitle, outOfBreathMessage);
        }
    }

    // Display objective on player screen
    void UpdateHUDStage(ObjectiveStage s)
    {
        if (objectiveHUDText)
            objectiveHUDText.text = $"OBJECTIVE: Find {s.requiredCount} {Pluralize(s.title, s.requiredCount)}";
        if (progressHUDText)
            progressHUDText.text = $"0/{s.requiredCount}";
    }

    // Display player progress
    void UpdateHUDProgress()
    {
        var s = Current;
        if (s == null)
        {
            ClearHUD();
            return;
        }
        if (progressHUDText)
            progressHUDText.text = $"{currentCount}/{s.requiredCount}";
    }

    // Reset
    void ClearHUD()
    {
        if (objectiveHUDText) objectiveHUDText.text = "";
        if (progressHUDText) progressHUDText.text = "";
    }
}
