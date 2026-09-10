namespace RVS_Aplikacija.ViewModels
{
    public class GreskaViewModel
    {
        public string? RequestId { get; set; }
        public bool PrikaziRequestId => !string.IsNullOrWhiteSpace(RequestId);
    }
}
