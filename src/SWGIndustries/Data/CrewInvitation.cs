using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

public enum InvitationStatus
{
    /// <summary>
    /// Invitation is pending
    /// </summary>
    Pending,

    /// <summary>
    /// Invitation was accepted
    /// </summary>
    Accepted,

    /// <summary>
    /// Invitation was rejected
    /// </summary>
    Rejected
}

public class CrewInvitation
{
    /// <summary>
    /// Database Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Status of the invitation
    /// </summary>
    public InvitationStatus Status { get; set; }

    /// <summary>
    /// Account that sent the invitation
    /// </summary>
    public ApplicationUser FromUser { get; set; }

    /// <summary>
    /// Account that received the invitation
    /// </summary>
    public ApplicationUser ToUser { get; set; }

    /// <summary>
    /// <c>true</c> the invitation was sent from a crew leader to a member.
    /// <c>false</c> the invitation was sent from a member to a leader of the crew to join
    /// </summary>
    public bool InviteOrRequestToJoin { get; set; }
}