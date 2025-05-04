using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{


    public static PlayerInteract Instance { get; private set; }
    public event EventHandler<OnSelectedInteractableChangedEventArgs> OnSelectedInteractableChanged;
    public class OnSelectedInteractableChangedEventArgs: EventArgs {
        public Interactable selectedInteractable;
    }

    private void Awake()
    {
        if (Instance != null) {
            Debug.LogError("There is more than one player instance.");
        }
        Instance = this;
    }
    private void Update()
    {
        // refactor to new input system later
        float interactRange = 2f;
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);

        foreach (Collider collider in colliderArray) {
            if (collider.TryGetComponent(out Interactable interactable)) {
                OnSelectedInteractableChanged.Invoke(this, new OnSelectedInteractableChangedEventArgs {selectedInteractable = interactable});
                if (Input.GetKeyDown(KeyCode.E)) {
                    interactable.Interact(transform);
                }
            }
        }
    }
}
