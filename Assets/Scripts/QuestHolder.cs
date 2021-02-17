using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestHolder : MonoBehaviour
{
    public QuestBase questBase;
    public List<int> completedIntervals;
}