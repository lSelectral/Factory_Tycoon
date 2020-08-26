using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Manager", menuName = "Manager")]
public class ScriptableManager : ScriptableObject
{
    public string managerName;
    public string description;
    public long cost;

    public BaseResources automateResource;

    public Sprite managerSprite;
}
