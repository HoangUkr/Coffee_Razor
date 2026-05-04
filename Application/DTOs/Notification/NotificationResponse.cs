namespace Application.DTOs.Notification
{
    public record NotificationResponse
    {
        public int Id { get; init; }
        public string Where { get; init; } = string.Empty;
        public string WhatHappen { get; init; } = string.Empty;
        public string DisplayMessage => $"{Where}: {WhatHappen}";
        public string? TargetUrl { get; init; }
        public bool IsRead { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
    }
}
