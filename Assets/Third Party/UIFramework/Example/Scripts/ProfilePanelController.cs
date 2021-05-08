using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
public class ProfilePanelController : APanelController<ProfileData>
{
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI NextLeveLTargetText;
    public Slider Progress;

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        LevelText.text = "Lvl "+Properties.Level;
        NextLeveLTargetText.text = Properties.Wins+"/"+Properties.NextLevelTarget;
        Progress.value = Properties.Wins * 1.0f / Properties.NextLevelTarget;
    }
}
[System.Serializable]
public class ProfileData: PanelProperties
{
    public int Level { get; set; }
    public int Wins { get; set; }
    public int NextLevelTarget { get; set; }
}
