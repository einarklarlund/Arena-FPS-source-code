using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text upgradeText;
    [SerializeField] private Text numPickupsText = null;
    private PlayerUpgradeManager _playerUpgradeManager = null;
    [SerializeField] private float _textTime = 4f;

    private float textActivatedTime;

    // Start is called before the first frame update
    private void OnEnable()
    {
        _playerUpgradeManager = GameObject.Find("Player").GetComponent<PlayerUpgradeManager>();  
        textActivatedTime = 0f;

        numPickupsText.text = "    x 0";
        upgradeText.text = "";

        Debug.Log(numPickupsText.text);
        upgradeText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        numPickupsText.text = "    x " + _playerUpgradeManager.numBasicPickups;
        
        if(textActivatedTime > 0 && Time.time - textActivatedTime > _textTime)
        {
            upgradeText.gameObject.SetActive(false);
            textActivatedTime = 0f;
        }
    }

    public void DisplayMessage(string message)
    {
        upgradeText.gameObject.SetActive(true);
        upgradeText.text = message;
        textActivatedTime = Time.time;
    }

    // private void HandleGameStateChanged(GameManager.GameState currentState, GameManager.GameState previousState)
    // {
    //     if(currentState == GameManager.GameState.RUNNING && previousState != GameManager.GameState.PAUSED)
    //     {

    //     }
    // }
}
