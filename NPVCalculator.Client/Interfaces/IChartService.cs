using NPVCalculator.Shared.Models;

namespace NPVCalculator.Client.Interfaces
{
    public interface IChartService
    {
        Task RenderNpvChart(string canvasId, List<NpvResult> results);
    }
}