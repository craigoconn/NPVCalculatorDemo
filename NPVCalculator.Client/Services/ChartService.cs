using Microsoft.JSInterop;
using NPVCalculator.Client.Interfaces;
using NPVCalculator.Shared.Models;
using System.Text.Json;

namespace NPVCalculator.Client.Services
{
    public class ChartService : IChartService
    {
        private readonly IJSRuntime _jsRuntime;

        public ChartService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task RenderNpvChart(string canvasId, List<NpvResult> results)
        {
            try
            {
                var chartData = new
                {
                    labels = results.Select(r => r.Rate.ToString("F2") + "%").ToArray(),
                    datasets = new[]
                    {
                        new
                        {
                            label = "NPV",
                            data = results.Select(r => (double)r.Value).ToArray(),
                            borderColor = "rgb(75, 192, 192)",
                            backgroundColor = "rgba(75, 192, 192, 0.2)",
                            tension = 0.1,
                            fill = false
                        }
                    }
                };

                var options = new
                {
                    responsive = true,
                    maintainAspectRatio = false,
                    plugins = new
                    {
                        title = new { display = true, text = "NPV vs Discount Rate" },
                        legend = new { display = false }
                    },
                    scales = new
                    {
                        y = new
                        {
                            beginAtZero = true,
                            title = new { display = true, text = "Net Present Value ($)" }
                        },
                        x = new
                        {
                            title = new { display = true, text = "Discount Rate (%)" }
                        }
                    }
                };

                // Use Chart.js directly without any helper file
                var script = $@"
                (function() {{
                    try {{
                        const ctx = document.getElementById('{canvasId}');
                        if (!ctx) {{
                            console.error('Canvas not found: {canvasId}');
                            return;
                        }}
                        
                        if (typeof Chart === 'undefined') {{
                            console.error('Chart.js not loaded');
                            return;
                        }}
                        
                        if (window.npvCharts && window.npvCharts['{canvasId}']) {{
                            window.npvCharts['{canvasId}'].destroy();
                        }}
                        
                        if (!window.npvCharts) window.npvCharts = {{}};
                        
                        window.npvCharts['{canvasId}'] = new Chart(ctx, {{
                            type: 'line',
                            data: {JsonSerializer.Serialize(chartData)},
                            options: {JsonSerializer.Serialize(options)}
                        }});
                        
                        console.log('Chart created: {canvasId}');
                    }} catch (e) {{
                        console.error('Chart error:', e);
                    }}
                }})();
                ";

                await _jsRuntime.InvokeVoidAsync("eval", script);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chart rendering error: {ex.Message}");
            }

        }
    }
}