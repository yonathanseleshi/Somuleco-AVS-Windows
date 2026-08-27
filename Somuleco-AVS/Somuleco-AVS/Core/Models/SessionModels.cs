using System.Collections.Generic;

namespace Somuleco_AVS.Core.Models;

public sealed record LiveExperience(string Id, string Title, string ScheduleLabel, string StatusLabel);
public sealed record SessionParticipant(string DisplayName, string Role, bool IsSpeaking);
public sealed record AVSSession(string Id, string Title, IReadOnlyList<SessionParticipant> Participants);