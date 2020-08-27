using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Contract_Btn : MonoBehaviour
{
    [SerializeField] private ContractBase contract;

    private string contractName;
    private string contractDescription;
    private long contractReward;
    private BaseResources[] requiredResources;
    private int[] requiredResourceAmounts;
    private int unlockLevel;

    private List<BaseResources> tempResourceList;

    private Transform iconHolder;
    Image fillBar;

    private void Awake()
    {
        contractName = contract.contractName;
        contractDescription = contract.description;
        contractReward = contract.contractReward;
        requiredResources = contract.requiredResources;
        requiredResourceAmounts = contract.requiredResourceAmounts;
        tempResourceList = contract.requiredResources.ToList();
        unlockLevel = contract.unlockLevel;

        fillBar = transform.Find("Outline").transform.Find("Fill").GetComponent<Image>();
        iconHolder = transform.Find("Required_Resource");

        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged;
    }

    private void Start()
    {
        transform.Find("Texts").Find("Header").GetComponent<TextMeshProUGUI>().text = contractName;
        transform.Find("Texts").Find("Description").GetComponent<TextMeshProUGUI>().text = contractDescription;
        transform.Find("Texts").Find("Reward").GetComponent<TextMeshProUGUI>().text = contractReward.ToString();
        transform.Find("Icon").GetComponent<Image>().sprite = contract.icon;

        for (int i = 0; i < requiredResources.Length; i++)
        {
            var icon = Instantiate(ResourceManager.Instance.iconPrefab, iconHolder);
            icon.transform.Find("Image").GetComponent<Image>().sprite = ResourceManager.Instance.GetSpriteFromResource(requiredResources[i]);
            icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = requiredResourceAmounts[i].ToString();
        }

        GameObject contactAgreePrefab = Instantiate(ContractManager.Instance.contactAgreePrefab);
        contactAgreePrefab.SetActive(false);
        GetComponent<Button>().onClick.AddListener(() => SetContractPrefab(contactAgreePrefab));
        contactAgreePrefab.GetComponent<Button>().onClick.AddListener(() => ContractManager.Instance.ActivateContract(contract));
    }

    private void SetContractPrefab(GameObject contactAgreePrefab)
    {
        contactAgreePrefab.transform.SetParent(transform.parent);
        contactAgreePrefab.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0.5f);

        contactAgreePrefab.SetActive(true);
    }

    private void OnResourceAmountChanged(object sender, ResourceManager.OnResourceAmountChangedEventArgs e)
    {
        var inputs = requiredResources.Zip(requiredResourceAmounts, (resource, amount) => (Resource: resource, Amount: amount));
        foreach (var input in inputs)
        {
            if (ResourceManager.Instance.GetResourceAmount(input.Resource) >= input.Amount && tempResourceList.Contains(input.Resource))
            {
                Debug.Log(input.Resource + " added to contract");
                tempResourceList.Remove(input.Resource);
                ResourceManager.Instance.ConsumeResource(input.Resource, input.Amount);

                iconHolder.GetChild(Array.IndexOf(requiredResources, input.Resource)).GetChild(0).GetChild(0).gameObject.SetActive(true);
            }
            else if (ResourceManager.Instance.GetResourceAmount(input.Resource) < input.Amount)
            {
                Debug.Log("Not enough " + input.Resource);
            }
        }

        if (tempResourceList.Count == 0)
        {
            Debug.Log(contractName + " contract completed");
            Debug.Log(contractReward + " granted to player as reward");
            ResourceManager.Instance.Currency += contractReward;
            foreach (var input in inputs)
            {
                iconHolder.GetChild(Array.IndexOf(requiredResources, input.Resource)).GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
            tempResourceList = requiredResources.ToList();
        }
    }
}
