namespace Somuleco_AVS.Core.Models;

public enum MediaAssetType { Video, Audio, Image, Presentation, Graphic, Other }
public enum MediaAvailability { Available, Offline, Missing, Processing }
public sealed record MediaAsset(string Id, string Name, MediaAssetType Type, MediaAvailability Availability, string DurationLabel);