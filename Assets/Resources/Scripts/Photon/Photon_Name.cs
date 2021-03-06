using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class Photon_Name : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField nameInputField = null;
    [SerializeField] private InputField roomNameInputField = null;
    [SerializeField] private Button continueButton = null;
    private string Name = "";
    private const string PlayerPrefsNameKey = "PlayerName";

    void Start()
    {
        SetUpInputField();
    }

    private void SetUpInputField()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey)) { return; }
        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);
        nameInputField.text = defaultName;
        SetButtonInteractibility(continueButton, defaultName);
        SetButtonInteractibility(GameObject.Find("Canvas_Menu").transform.Find("Panel_FindOpponent").transform.Find("Button_CreateNewRoom").GetComponent<Button>(), StaticData.myRoomName);
    }

    public void SetButtonInteractibility(Button buttonToChange, string name)
    {
        buttonToChange.interactable = !string.IsNullOrEmpty(name);
    }

    public void SaveName()
    {
        if (GameObject.Find("Canvas_Menu").transform.Find("Panel_NameInput").gameObject.activeInHierarchy){
            Name = nameInputField.text;
            PhotonNetwork.NickName = Name;
            PlayerPrefs.SetString(PlayerPrefsNameKey, Name);
            SetButtonInteractibility(continueButton, Name);
        }
        else{
            StaticData.myRoomName = roomNameInputField.text;
            SetButtonInteractibility(GameObject.Find("Canvas_Menu").transform.Find("Panel_FindOpponent").transform.Find("Button_CreateNewRoom").GetComponent<Button>(), StaticData.myRoomName);
        }
    }
}
