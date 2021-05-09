using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInQueueItem : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    string id;
    int sr;
    string _name;

    public void SetPlayer(string _name, int sr, string id)
    {
        text.text = _name + "(" + sr + ")";
        this._name = _name;
        this.id = id;
        this.sr = sr;
    }

    public string GetID()
    {
        return id;
    }
}
