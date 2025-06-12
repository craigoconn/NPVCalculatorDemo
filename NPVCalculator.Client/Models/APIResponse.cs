namespace NPVCalculator.Client.Models
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}