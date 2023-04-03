using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EOMCanvasManager : NetworkBehaviour
{

    [SerializeField] private List<string> preyStatStrings, predatorStatStrings;
    [SerializeField] private FadingCanvasGroup eomScreenParent;
    [SerializeField] private GameObject exitMatchBtn;

    [SerializeField] private GameObject winBoard, lossBoard;

    [SerializeField] private TextMeshProUGUI mvpPlayerName;
    [SerializeField] private Image mvpCharacterImage;
    [SerializeField] private Sprite[] characterSprites;
    [SerializeField] private List<TextMeshProUGUI> mvpStatTexts;
    

    [SerializeField] private List<TextMeshProUGUI> statTexts;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("SubscribeToEvents", 1.0f);

        eomScreenParent.FadeOut(0);
    }

    void SubscribeToEvents()
    {
        GameManager.Instance.event_EndOfMatch += OnEndOfMatch;
    }

    public override void OnNetworkSpawn()
    {
        exitMatchBtn.SetActive(NetworkManager.Singleton.IsHost);
    }

    private void OnEndOfMatch(bool localClientWon, ClientStatus localClientStatus, ClientStatus mvp)
    {

        if(IsHost)


        winBoard.SetActive(localClientWon);
        lossBoard.SetActive(!localClientWon);

        /*
        //Change the text of stats on mvp
        mvpPlayerName.text = mvp.playerName;
        mvpCharacterImage.sprite = characterSprites[mvp.character + (3 * mvp.role)];

        if (mvp.role == 0)
        {
            mvpStatTexts[0].text = preyStatStrings[0] + mvp.metrics[0];
            mvpStatTexts[1].text = preyStatStrings[1] + mvp.metrics[1];
        }
        else
        {
            mvpStatTexts[0].text = predatorStatStrings[2] + mvp.metrics[2];
            mvpStatTexts[1].text = predatorStatStrings[3] + mvp.metrics[3];
        }
        */

        //Change the text of stats depending on Prey or Predator
        ApplyStatString(localClientStatus.role == 0 ? preyStatStrings : predatorStatStrings, localClientStatus);
        eomScreenParent.FadeIn();
    }

    [ClientRpc]
    private void OnEndOfMatchClientRpc(bool b)
    {

    }

    private void ApplyStatString(List<string> statLabels, ClientStatus clientStatus)
    {
        for(int i = 0; i < statTexts.Count; i++)
        {
            try
            {
                statTexts[i].text = statLabels[i] + ""
                    + clientStatus.metrics[i + (Enum.GetNames(typeof(ClientStatus.StatIndex)).Length/2 * clientStatus.role)];
            } catch (ArgumentOutOfRangeException e)
            {
                statTexts[i].gameObject.SetActive(false);
            }   
        }
    }

    public void ExitMatchClicked()
    {

        winBoard.SetActive(false);
        lossBoard.SetActive(false);
        eomScreenParent.FadeOut();

        if(NetworkManager.Singleton.IsHost)
            GameManager.Instance.TryExitMatch();
    }
}
