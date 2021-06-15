using UnityEngine;

public class FireMonumentInteraction : InteractableWithOutline
{
    [Header("Fire Monument")]
    [SerializeField] private FireMonument mainScipt;

    public override void Interact()
    {
        base.Interact();

        mainScipt.OnInteract();
    }
}