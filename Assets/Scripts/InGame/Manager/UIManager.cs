using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonPun<UIManager>
{
    [SerializeField] private UIPlayerStatus redPlayerStatus;
    [SerializeField] private UIPlayerStatus bluePlayerStatus;

    [SerializeField] private TextMeshProUGUI turnCountText;
    [SerializeField] private TextMeshProUGUI turnOwnerText;

    [SerializeField] private RectTransform turnActParent;
    
    [Header("Result")]
    [SerializeField] private RectTransform gameEndParent;

    [SerializeField] private TextMeshProUGUI winText;


    public void PlayerSetting(Player player)
    {
        if (player.team == Team.RED)
            redPlayerStatus.SetPlayerName(player.photonView.Owner.NickName);
        else
            bluePlayerStatus.SetPlayerName(player.photonView.Owner.NickName);
    }

    public void TurnSetting()
    {
        photonView.RPC(nameof(TurnSettingRPC), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void TurnSettingRPC()
    {
        turnCountText.text = InGameManager.Instance.turnCount.ToString();
        turnOwnerText.text = (InGameManager.Instance.player.team == InGameManager.Instance.turnOwner
            ? InGameManager.Instance.player.photonView.Owner.NickName
            : InGameManager.Instance.enemyPlayer.photonView.Owner.NickName) + "의 턴";
        turnActParent.gameObject.SetActive(InGameManager.Instance.player.team == InGameManager.Instance.turnOwner);
    }

    public void PlayerNotActSetting(Team team, bool notAct)
    {
        if (team == Team.RED)
            redPlayerStatus.SetNotAct(notAct);
        else
            bluePlayerStatus.SetNotAct(notAct);
    }

    public void UpdateCard(Team team, List<int> numberCards, bool secretSkip = false)
    {
        bool secret = secretSkip || team == InGameManager.Instance.player.team;
        if (team == Team.RED)
            redPlayerStatus.SetNumberCard(secret, numberCards);
        else
            bluePlayerStatus.SetNumberCard(secret, numberCards);
    }

    public void GameEnd(Player winner)
    {
        gameEndParent.gameObject.SetActive(true);
        turnActParent.gameObject.SetActive(false);
        winText.text = (winner.team == Team.RED ? "<#ff0000>" : "<#0000ff>") + winner.photonView.Owner.NickName + "</color>의 승리!";
    }
}