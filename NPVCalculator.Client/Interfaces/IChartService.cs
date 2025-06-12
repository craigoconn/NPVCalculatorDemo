using Microsoft.JSInterop;
using NPVCalculator.Shared.Models;
using System.Text.Json;

namespace NPVCalculator.Client.Interfaces
{
    public interface IChartService
    {
        Task RenderNpvChart(string canvasId, List<NpvResult> results);
    }
}