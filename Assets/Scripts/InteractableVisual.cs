using UnityEditor.Search;
using UnityEngine;

public class SelectedInteractableVisual : MonoBehaviour
{


    [SerializeField] private Interactable interactable;
    [SerializeField] private GameObject[] visualGameObjectArray;

    private void Awake()
    {
        foreach (GameObject visualGameObject in visualGameObjectArray) {
            visualGameObject.SetActive(false);
        }
    }
    private void Start()
    {
        PlayerInteract.Instance.OnSelectedInteractableChanged += Player_OnSelectedInteractableChanged;

    }

    private void Player_OnSelectedInteractableChanged(object sender, PlayerInteract.OnSelectedInteractableChangedEventArgs e) {
        if (e.selectedInteractable == interactable) {
            Show();
        } else {
            Hide();
        }
    }

    private void Show() {
        foreach (GameObject visualGameObject in visualGameObjectArray) {
            visualGameObject.SetActive(true);
        }
    }

    private void Hide() {
        foreach (GameObject visualGameObject in visualGameObjectArray) {
            visualGameObject.SetActive(false);
        }
    }
}
