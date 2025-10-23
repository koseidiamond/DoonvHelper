namespace Celeste.Mod.DoonvHelper;

public class DoonvHelperModuleSettings : EverestModuleSettings
{
    [SettingSubText("Whether gore particles from NPCs should be disabled.")]
    public bool Gore { get; set; } = true;
}