using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    Dictionary<BaseResources, long> resourceDict;


    long stone, berry, stick, leaf, hut, spear, torch, axe, fire, leaf_cloth, pickaxe, rope, bonefire, hammer, animal_trap, pouch, leather_cloth, wheel, arrow, bow;

    void Start()
    {
        resourceDict = new Dictionary<BaseResources, long>()
        {
            {BaseResources._0_berry, berry},
            {BaseResources._0_stick, stick },
            {BaseResources._0_leaf, leaf },
            {BaseResources._0_hut, hut },
            {BaseResources._0_stone, stone },
            {BaseResources._0_spear, spear },
            {BaseResources._0_torch, torch },
            {BaseResources._0_axe, axe },
            {BaseResources._0_fire, fire },
            {BaseResources._0_leaf_cloth, leaf_cloth },
            {BaseResources._0_pickaxe, pickaxe },
            {BaseResources._0_rope, rope },
            {BaseResources._0_bonefire, bonefire },
            {BaseResources._0_arrow, arrow },
            {BaseResources._0_bow, bow },
            {BaseResources._0_hammer, hammer },
            {BaseResources._0_animal_trap, animal_trap },
            {BaseResources._0_pouch, pouch },
            {BaseResources._0_leather_cloth, leather_cloth },
            {BaseResources._0_wheel, wheel },
        };
        
    }

    void Update()
    {
        //Debug.Log("Stick callback is: " + resourceDict[BaseResources._0_stick]);
        ResourceManager.Instance.AddResource(BaseResources._0_stick, 100);
    }

    public void AddResource(BaseResources res, long amount)
    {
        resourceDict[res] += amount;
    }
}
