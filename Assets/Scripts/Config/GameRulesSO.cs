using UnityEngine;

[CreateAssetMenu(menuName = "Game/CardMatch/Game Rules", fileName = "GameRules")]
public sealed class GameRulesSO : ScriptableObject
{
    [Header("Attempts / Tries")]
    [Min(0)] public int maxTries = 10;

    [Header("Preview (start memorization)")]
    public bool enablePreview = true;
    [Min(0f)] public float previewSeconds = 1.0f;

    [Header("End Screens Delay")]
    [Min(0f)] public float winRestartDelaySeconds = 3f;
    [Min(0f)] public float defeatRestartDelaySeconds = 3f;
}
