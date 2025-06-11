namespace NPVCalculator.Domain.Entities
{
    /// <summary>
    /// Domain entity representing validation results
    /// Follows Single Responsibility Principle - only handles validation state
    /// </summary>
    public class NpvValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; private set; } = new List<string>();
        public List<string> Warnings { get; private set; } = new List<string>();

        /// <summary>
        /// Add a validation error
        /// </summary>
        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                Errors.Add(error);
            }
        }

        /// <summary>
        /// Add multiple validation errors
        /// </summary>
        public void AddErrors(IEnumerable<string> errors)
        {
            if (errors != null)
            {
                foreach (var error in errors.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    Errors.Add(error);
                }
            }
        }

        /// <summary>
        /// Add a validation warning (doesn't affect IsValid)
        /// </summary>
        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
            {
                Warnings.Add(warning);
            }
        }

        /// <summary>
        /// Get a summary of all validation issues
        /// </summary>
        public string GetSummary()
        {
            var summary = new List<string>();

            if (Errors.Any())
            {
                summary.Add($"Errors: {string.Join(", ", Errors)}");
            }

            if (Warnings.Any())
            {
                summary.Add($"Warnings: {string.Join(", ", Warnings)}");
            }

            return summary.Any() ? string.Join("; ", summary) : "No validation issues";
        }

        /// <summary>
        /// Reset validation state
        /// </summary>
        public void Clear()
        {
            Errors.Clear();
            Warnings.Clear();
        }
    }
}