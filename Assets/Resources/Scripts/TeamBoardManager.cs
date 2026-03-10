using TMPro;
using UnityEngine;

public class TeamBoardManager : MonoBehaviour
{
    public enum Team
    {
        None,
        Blue,
        Red
    }

    public enum Role
    {
        None,
        Chef,
        Butcher
    }

    [System.Serializable]
    public class TeamUI
    {
        public TMP_Text titleText;
        public TMP_Text chefSlotText;
        public TMP_Text butcherSlotText;
    }

    [Header("Blue Team UI")]
    public TeamUI blueUI;

    [Header("Red Team UI")]
    public TeamUI redUI;

    [Header("Optional Status UI")]
    public TMP_Text statusText;

    [Header("Settings")]
    public string emptyChefText = "[Empty Chef]";
    public string emptyButcherText = "[Empty Butcher]";
    public string localPlayerId = "User1";

    private string blueChefPlayer = null;
    private string blueButcherPlayer = null;

    private string redChefPlayer = null;
    private string redButcherPlayer = null;

    private void Start()
    {
        RefreshUI();
    }

    // =========================
    // Public Button Functions
    // =========================

    public void SelectBlueChef()
    {
        SelectRole(localPlayerId, Team.Blue, Role.Chef);
    }

    public void SelectBlueButcher()
    {
        SelectRole(localPlayerId, Team.Blue, Role.Butcher);
    }

    public void SelectRedChef()
    {
        SelectRole(localPlayerId, Team.Red, Role.Chef);
    }

    public void SelectRedButcher()
    {
        SelectRole(localPlayerId, Team.Red, Role.Butcher);
    }

    public void CancelCurrentSelection()
    {
        Team currentTeam = GetPlayerTeam(localPlayerId);
        Role currentRole = GetPlayerRole(localPlayerId);

        if (currentTeam == Team.None || currentRole == Role.None)
        {
            SetStatus($"{localPlayerId} is not assigned to any role.");
            return;
        }

        RemovePlayerFromAllSlots(localPlayerId);
        RefreshUI();
        SetStatus($"{localPlayerId} left {currentTeam} {currentRole}.");
    }

    // 可选：如果你还想保留“按队伍取消”的按钮，也可以加这两个
    public void CancelBlue()
    {
        CancelIfInTeam(localPlayerId, Team.Blue);
    }

    public void CancelRed()
    {
        CancelIfInTeam(localPlayerId, Team.Red);
    }

    // =========================
    // Core Logic
    // =========================

    public void SelectRole(string playerId, Team targetTeam, Role targetRole)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            SetStatus("Player ID is empty.");
            return;
        }

        Team currentTeam = GetPlayerTeam(playerId);
        Role currentRole = GetPlayerRole(playerId);

        // 已经在目标位置，不重复处理
        if (currentTeam == targetTeam && currentRole == targetRole)
        {
            SetStatus($"{playerId} is already {targetRole} in {targetTeam} team.");
            RefreshUI();
            return;
        }

        // 目标坑位是否已被别人占用
        string occupant = GetOccupant(targetTeam, targetRole);
        if (!string.IsNullOrEmpty(occupant) && occupant != playerId)
        {
            SetStatus($"{targetTeam} {targetRole} is already taken by {occupant}.");
            RefreshUI();
            return;
        }

        // 先从所有位置移除当前玩家（支持切队 / 切职业）
        RemovePlayerFromAllSlots(playerId);

        // 放进目标坑位
        SetOccupant(targetTeam, targetRole, playerId);

        RefreshUI();

        if (currentTeam == Team.None || currentRole == Role.None)
        {
            SetStatus($"{playerId} joined {targetTeam} team as {targetRole}.");
        }
        else
        {
            SetStatus($"{playerId} switched from {currentTeam} {currentRole} to {targetTeam} {targetRole}.");
        }
    }

    private void CancelIfInTeam(string playerId, Team team)
    {
        if (GetPlayerTeam(playerId) != team)
        {
            SetStatus($"{playerId} is not in {team} team.");
            return;
        }

        RemovePlayerFromAllSlots(playerId);
        RefreshUI();
        SetStatus($"{playerId} left {team} team.");
    }

    // =========================
    // Query Helpers
    // =========================

    public Team GetPlayerTeam(string playerId)
    {
        if (blueChefPlayer == playerId || blueButcherPlayer == playerId)
            return Team.Blue;

        if (redChefPlayer == playerId || redButcherPlayer == playerId)
            return Team.Red;

        return Team.None;
    }

    public Role GetPlayerRole(string playerId)
    {
        if (blueChefPlayer == playerId || redChefPlayer == playerId)
            return Role.Chef;

        if (blueButcherPlayer == playerId || redButcherPlayer == playerId)
            return Role.Butcher;

        return Role.None;
    }

    private string GetOccupant(Team team, Role role)
    {
        switch (team)
        {
            case Team.Blue:
                return role == Role.Chef ? blueChefPlayer : blueButcherPlayer;

            case Team.Red:
                return role == Role.Chef ? redChefPlayer : redButcherPlayer;

            default:
                return null;
        }
    }

    private void SetOccupant(Team team, Role role, string playerId)
    {
        switch (team)
        {
            case Team.Blue:
                if (role == Role.Chef)
                    blueChefPlayer = playerId;
                else if (role == Role.Butcher)
                    blueButcherPlayer = playerId;
                break;

            case Team.Red:
                if (role == Role.Chef)
                    redChefPlayer = playerId;
                else if (role == Role.Butcher)
                    redButcherPlayer = playerId;
                break;
        }
    }

    private void RemovePlayerFromAllSlots(string playerId)
    {
        if (blueChefPlayer == playerId) blueChefPlayer = null;
        if (blueButcherPlayer == playerId) blueButcherPlayer = null;
        if (redChefPlayer == playerId) redChefPlayer = null;
        if (redButcherPlayer == playerId) redButcherPlayer = null;
    }

    // =========================
    // UI
    // =========================

    private void RefreshUI()
    {
        RefreshTeamUI(
            blueUI,
            "Team Blue",
            blueChefPlayer,
            blueButcherPlayer
        );

        RefreshTeamUI(
            redUI,
            "Team Red",
            redChefPlayer,
            redButcherPlayer
        );
    }

    private void RefreshTeamUI(TeamUI teamUI, string teamName, string chefPlayer, string butcherPlayer)
    {
        if (teamUI == null)
            return;

        if (teamUI.titleText != null)
            teamUI.titleText.text = teamName;

        if (teamUI.chefSlotText != null)
            teamUI.chefSlotText.text = string.IsNullOrEmpty(chefPlayer)
                ? emptyChefText
                : $"{chefPlayer} (Chef)";

        if (teamUI.butcherSlotText != null)
            teamUI.butcherSlotText.text = string.IsNullOrEmpty(butcherPlayer)
                ? emptyButcherText
                : $"{butcherPlayer} (Butcher)";
    }

    private void SetStatus(string message)
    {
        Debug.Log("[TeamBoardManager] " + message);

        if (statusText != null)
            statusText.text = message;
    }

    // =========================
    // Optional Debug
    // =========================

    [ContextMenu("Debug/Blue Chef")]
    private void DebugBlueChef()
    {
        SelectBlueChef();
    }

    [ContextMenu("Debug/Blue Butcher")]
    private void DebugBlueButcher()
    {
        SelectBlueButcher();
    }

    [ContextMenu("Debug/Red Chef")]
    private void DebugRedChef()
    {
        SelectRedChef();
    }

    [ContextMenu("Debug/Red Butcher")]
    private void DebugRedButcher()
    {
        SelectRedButcher();
    }

    [ContextMenu("Debug/Cancel Current")]
    private void DebugCancel()
    {
        CancelCurrentSelection();
    }
}