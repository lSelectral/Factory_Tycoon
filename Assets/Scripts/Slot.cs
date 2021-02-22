using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerDownHandler
{
    public ScriptableArtifact artifact;
    [SerializeField] Sprite defaultSprite;
    [SerializeField] bool isCharacterSlot;
    public ArtifactPart artifactPart; // Shows what type of artifact part contains

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isCharacterSlot)
        {
            Debug.Log("Selected character slot is this");
            SlotManager.Instance.selectedCharacterSlot = this;
        }

        if (isCharacterSlot && artifact == null) // Empty character slot. Open inventory
        {
            Debug.Log("Showing item panel from character slot");
            SlotManager.Instance.ShowItemPanel(artifactPart);
        }
        else if (!isCharacterSlot && artifact != null) // Standard slot and there is item in it
        {
            Debug.Log("Showing equip panel from standard slot");
            SlotManager.Instance.SetEquipPanel(artifact, () =>
            {
                ChangeSlotInfos(this, SlotManager.Instance.selectedCharacterSlot);
                SlotManager.Instance.instantiatedSlots.Remove(this);
            });
        }

        else if (isCharacterSlot && artifact != null) // Unequip item from character slot.
        {
            Debug.Log("Showing unequip panel from character slot");
            SlotManager.Instance.SetEquipPanel(artifact, () =>
            {
                Debug.Log(artifact);
                var slot = SlotManager.Instance.InstantiateSlot(artifact);
                ChangeSlotInfos(this, slot);
                SlotManager.Instance.instantiatedSlots.Add(slot);
                slot.gameObject.SetActive(false);
            }, "Unequip");
        }
    }

    void ChangeSlotInfos(Slot from, Slot to) // Change slot infos between them
    {
        to.SetSlot(from.artifact);
        from.SetSlot(null);
    }

    public void SetSlot(ScriptableArtifact _artifact = null) // Set slot state according to own values
    {
        if (_artifact == null && !isCharacterSlot) // Standard slot
        {
            Destroy(gameObject);
            return;
        }
        if (_artifact == null) // Character slot
        {
            artifact = null;
            SetColor(transform.GetChild(0).GetComponent<Image>());
            transform.GetChild(0).GetComponent<Image>().sprite = defaultSprite;
            return;
        }

        artifact = _artifact;
        SetColor(transform.GetChild(0).GetComponent<Image>());
        transform.GetChild(0).GetComponent<Image>().sprite = artifact.icon;
    }

    void SetColor(Image image) // Set color according to artifact state
    {
        if (artifact != null)
            image.color = Color.white;
        else
            image.color = Color.black;
    }
}