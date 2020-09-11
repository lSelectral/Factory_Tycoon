using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

public class Compounds : MonoBehaviour
{
    public ScriptableCompound scriptableCompound;

    [SerializeField] private bool isAutomated;

    private BaseResources[] inputResources;
    private int[] inputAmounts;
    private List<BaseResources> tempResourceList;
    private string partName;
    private int outputAmount;
    private float buildTime;
    private float remainedBuildTime;
    private BaseResources product;
    private bool isCharging;
    private WorkingMode workingMode;
    private bool isLockedByContract;

    int compoundLevel;
    float compoundUpgradeAmount;

    public List<BaseResources> RemainedResources
    {
        get { return tempResourceList; }
        set { tempResourceList = value; }
    }

    public float RemainedCollectTime
    {
        get { return remainedBuildTime; }
        set { remainedBuildTime = value; }
    }

    public int CompoundLevel
    {
        get { return compoundLevel; }
        set { compoundLevel = value; }
    }

    public float UpgradeAmount
    {
        get { return compoundUpgradeAmount; }
        set { compoundUpgradeAmount = value; }
    }

    public bool IsAutomated
    {
        get { return isAutomated; }
        set { isAutomated = value; }
    }

    public bool IsLockedByContract
    {
        get { return isLockedByContract; }
        set { isLockedByContract = value; }
    }

    public WorkingMode WorkingMode
    {
        get { return workingMode; }
        set { workingMode = value; }
    }

    Image fillBar;
    Image icon;
    TextMeshProUGUI mineNameText;
    Button btn;
    Transform subResourceIcons;
    Button upgradeBtn;

    void Start()
    {
        IsAutomated = false;
        GameManager.Instance.OnLevelUp += OnLevelUp;
        UpgradeSystem.Instance.OnProductionEfficiencyChanged += OnProductionEfficiencyChanged;
        UpgradeSystem.Instance.OnProductionSpeedChanged += OnProductionSpeedChanged;
        UpgradeSystem.Instance.OnProductionYieldChanged += OnProductionYieldChanged;

        if (scriptableCompound.unlockLevel > GameManager.Instance.CurrentLevel)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT LEVEL " + scriptableCompound.unlockLevel.ToString();
        }
        else if (scriptableCompound.isLockedByContract)
        {
            var lockText = Instantiate(GameManager.Instance.levelLock, transform);
            lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + scriptableCompound.lockedByContract.contractName + " Contract";
        }

        inputResources = scriptableCompound.inputResources;
        tempResourceList = scriptableCompound.inputResources.ToList();
        inputAmounts = scriptableCompound.inputAmounts;
        partName = scriptableCompound.partName;
        outputAmount = scriptableCompound.outputValue;
        buildTime = scriptableCompound.buildTime;
        product = scriptableCompound.product;
        isLockedByContract = scriptableCompound.isLockedByContract;

        fillBar = transform.Find("Main_Panel").transform.Find("FillBar").transform.Find("Fill").GetComponent<Image>();
        mineNameText = transform.Find("Main_Panel").transform.Find("Mine_Name").GetComponent<TextMeshProUGUI>();
        icon = transform.Find("Main_Panel").Find("Icon").GetComponent<Image>();
        btn = transform.Find("Button").GetComponent<Button>();
        upgradeBtn = transform.Find("Upgrade_Btn").GetComponent<Button>();

        icon.sprite = ResourceManager.Instance.GetSpriteFromResource(product);

        subResourceIcons = transform.Find("Main_Panel").Find("Sub_Resource_Icons");

        if (subResourceIcons != null)
        {
            
            for (int i = 0; i < inputResources.Length; i++)
            {
                var icon = Instantiate(ResourceManager.Instance.iconPrefab, subResourceIcons);
                icon.transform.Find("Image").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(inputResources[i]);
                icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = inputAmounts[i].ToString();
            }
        }

        mineNameText.text = partName;
        btn.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("+{0} {1}", outputAmount, partName);
        btn.onClick.AddListener(() => Produce());
    }

    private void OnProductionYieldChanged(object sender, UpgradeSystem.OnProductionYieldChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnProductionSpeedChanged(object sender, UpgradeSystem.OnProductionSpeedChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnProductionEfficiencyChanged(object sender, UpgradeSystem.OnProductionEfficiencyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnLevelUp(object sender, GameManager.OnLevelUpEventArgs e)
    {
        if (transform.Find("Level_Lock(Clone)") != null && scriptableCompound.unlockLevel == e.currentLevel)
        {
            Destroy(transform.Find("Level_Lock(Clone)").gameObject);
            if (scriptableCompound.isLockedByContract)
            {
                var lockText = Instantiate(GameManager.Instance.levelLock, transform);
                lockText.GetComponentInChildren<TextMeshProUGUI>().text = "UNLOCKED AT COMPLETION OF " + scriptableCompound.lockedByContract.contractName + " Contract";
                scriptableCompound.isLockedByContract = false;
            }
        }
    }

    void Update()
    {
        if (isAutomated)
        {
            Produce();
        }

        if (isCharging)
        {
            if (remainedBuildTime > 0)
            {
                remainedBuildTime -= Time.deltaTime * UpgradeSystem.Instance.ProductionSpeedMultiplier;
                fillBar.fillAmount = ((buildTime - remainedBuildTime) / buildTime);
            }
            else
            {
                isCharging = false;
                ResourceManager.Instance.AddResource(product, (long)(outputAmount * UpgradeSystem.Instance.ProductionYieldMultiplier));
                tempResourceList = inputResources.ToList();
                GameManager.Instance.AddXP(scriptableCompound.xpAmount);
                remainedBuildTime = 0;
                fillBar.fillAmount = 0;
            }
        }
    }

    void Produce()
    { 
        if (!isCharging)
        {
            var inputs = inputResources.Zip(inputAmounts, (resource, amount) => (Resource: resource, Amount: amount));
            foreach (var input in inputs)
            {
                if (ResourceManager.Instance.GetResourceAmount(input.Resource) >= input.Amount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier && tempResourceList.Contains(input.Resource))
                {
                    //Debug.Log(input.Resource + " added to recipe");
                    tempResourceList.Remove(input.Resource);
                    ResourceManager.Instance.ConsumeResource(input.Resource, (long)(input.Amount / UpgradeSystem.Instance.ProductionEfficiencyMultiplier));

                    if (subResourceIcons != null)
                        subResourceIcons.GetChild(Array.IndexOf(inputResources, input.Resource)).GetChild(0).GetChild(0).gameObject.SetActive(true);
                }
                else if (ResourceManager.Instance.GetResourceAmount(input.Resource) < input.Amount)
                {
                    //Debug.Log("Not enough " + input.Resource    );
                }
            }

            if (tempResourceList.Count == 0)
            {   
                //Debug.Log(partName + " recipe completed");
                isCharging = true;
                remainedBuildTime = buildTime;

                foreach (var input in inputs)
                {
                    subResourceIcons.GetChild(Array.IndexOf(inputResources, input.Resource)).GetChild(0).GetChild(0).gameObject.SetActive(false);
                }
            }
        }
    }

    void Upgrade()
    {

    }
}
