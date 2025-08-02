namespace EventHub.Models.DTOs
{
    public class TransferOwnershipDto
    {
        public int NewOwnerId { get; set; }
        public string VerificationCode { get; set; } = string.Empty;
    }

    public class TransferOwnershipRequestDto
    {
        public int NewOwnerId { get; set; }
    }

    public class TransferOwnershipResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
    }
} 