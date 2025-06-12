namespace NPVCalculator.Domain.Entities
{
    public class NpvValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; private set; } = [];
        public List<string> Warnings { get; private set; } = [];

        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
                Errors.Add(error);
        }

        public void AddErrors(IEnumerable<string> errors)
        {
            if (errors != null)
            {
                foreach (var error in errors.Where(e => !string.IsNullOrWhiteSpace(e)))
                    Errors.Add(error);
            }
        }

        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
                Warnings.Add(warning);
        }

        public string GetSummary()
        {
            var parts = new List<string>();

            if (Errors.Any())
                parts.Add($"Errors: {string.Join(", ", Errors)}");

            if (Warnings.Any())
                parts.Add($"Warnings: {string.Join(", ", Warnings)}");

            return parts.Any() ? string.Join("; ", parts) : "No validation issues";
        }
    }
}