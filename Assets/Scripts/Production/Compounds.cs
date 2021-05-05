using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class Compounds : ProductionBase
{
    public ScriptableCompound scriptableCompound;

    protected override void Start()
    {
        // Events
        ResourceManager.Instance.OnPricePerProductChanged += OnPricePerProductChanged;
        UpgradeSystem.Instance.OnProductionEfficiencyChanged += OnProductionEfficiencyChanged;
        UpgradeSystem.Instance.OnProductionSpeedChanged += OnProductionSpeedChanged;
        UpgradeSystem.Instance.OnProductionYieldChanged += OnProductionYieldChanged;

        base.Start();

        workModeBtn.onClick.AddListener(() => ChangeWorkingMode());
        tempResourceList = currentRecipe.inputResources.ToList();
        if (resourceBoard != null)
        {
            // Destroy unnecessary objs from prefab
            if (resourceBoard.childCount > currentRecipe.inputResources.Length)
            {
                for (int i = 1; i < resourceBoard.childCount + 1 - currentRecipe.inputResources.Length; i++)
                {
                    Destroy(resourceBoard.GetChild(resourceBoard.childCount - i).gameObject);
                }
            }

            for (int i = 0; i < currentRecipe.inputResources.Length; i++)
            {
                resourceBoard.GetChild(i).GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(currentRecipe.inputResources[i]);
                resourceBoard.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = currentRecipe.inputAmounts[i].ToString();
                resourceBoard.GetChild(i).gameObject.SetActive(true);
                resourceIconListForCompounds.Add(resourceBoard.GetChild(i).GetChild(0).gameObject);
            }
        }
    }

    #region Event Methods
    private void OnPricePerProductChanged(object sender, ResourceManager.OnPricePerProductChangedEventArgs e)
    {
        PricePerProduct = ProductionManager.Instance.GET_OPTIMAL_PRICE_PER_PRODUCT(this) + scriptableCompound.basePricePerProduct;
    }

    private void OnProductionYieldChanged(object sender, UpgradeSystem.OnProductionYieldChangedEventArgs e)
    {
    }

    private void OnProductionSpeedChanged(object sender, UpgradeSystem.OnProductionSpeedChangedEventArgs e)
    {
    }

    private void OnProductionEfficiencyChanged(object sender, UpgradeSystem.OnProductionEfficiencyChangedEventArgs e)
    {
    }

    #endregion
}