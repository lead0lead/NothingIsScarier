using System;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable {
    

    [SerializeField] private String interactText;
    public void Interact(Transform interactorTransform) {
        Debug.Log(GetInteractText());
    }

    public String GetInteractText() {
        return interactText;
    }
}
