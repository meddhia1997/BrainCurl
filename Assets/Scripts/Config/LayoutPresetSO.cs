using UnityEngine;

[CreateAssetMenu(menuName = "UI/Layout Preset")]
public class LayoutPresetSO : ScriptableObject
{
    [Min(2)] public int Rows = 4;
    [Min(2)] public int Cols = 4;

    public Vector2 Spacing = new Vector2(10f, 10f);
    public Vector2 Padding = new Vector2(20f, 20f);
}
