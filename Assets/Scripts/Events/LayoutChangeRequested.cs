public readonly struct LayoutChangeRequested
{
    public readonly LayoutPresetSO Preset;

    public LayoutChangeRequested(LayoutPresetSO preset)
    {
        Preset = preset;
    }
}
