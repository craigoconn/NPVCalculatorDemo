namespace NPVCalculator.Client.Models
{
    public class InputValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = [];
    }
}
