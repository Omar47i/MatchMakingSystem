using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
public class UIController : MonoBehaviour
{
    [SerializeField] UIFrame frame;
    // Start is called before the first frame update
    void Start()
    {
        ProfileData data = new ProfileData();
        data.Level = 33;
        data.Wins = 131;
        data.NextLevelTarget = 375;
        frame.ShowPanel(ScreenId.ProfilePanel, data);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
