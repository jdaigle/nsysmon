using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;
using NSysmon.Core.WMI;

namespace NSysmon.Core.Api
{
    public class StatusController : ApiController
    {
        [Route("api/nodes")]
        public NodeListViewModel GetNodeList()
        {
            return new NodeListViewModel
            {
                Nodes = PollingEngine.AllPollNodes
                .OrderBy(x => x.NodeType)
                .ThenBy(x => x.UniqueKey)
                .Select(node => GetNodeStatusViewModel(node)).ToList(),
            };
        }

        [Route("api/node/{nodeType}/{nodeKey}/status")]
        public NodeStatusViewModel GetNodeStatus(string nodeType, string nodeKey)
        {
            var node = PollingEngine.GetNode(HttpUtility.UrlDecode(nodeType), HttpUtility.UrlDecode(nodeKey));
            return GetNodeStatusViewModel(node);
        }

        private static NodeStatusViewModel GetNodeStatusViewModel(PollNode node)
        {
            return new NodeStatusViewModel
            {
                NodeType = node.NodeType,
                UniqueKey = node.UniqueKey,
                LastPoll = node.LastPoll,
                LastPollDuration = node.LastPollDuration,
                MinSecondsBetweenPolls = node.MinSecondsBetweenPolls,
                MonitorStatus = node.MonitorStatus,
                MonitorStatusReason = node.MonitorStatusReason,
                PollTaskStatus = node.PollTaskStatus,
                PollerCount = node.DataCaches.Count(),
                DataCaches = node.DataCaches.Select(dataCache => new NodeDataCacheViewModel
                {
                    Name = dataCache.ParentMemberName,
                    Type = dataCache.Type,
                    UniqueId = dataCache.UniqueId,
                    LastPoll = dataCache.LastPoll,
                    LastPollDuration = dataCache.LastPollDuration,
                    LastPollStatus = dataCache.LastPollStatus,
                    LastSuccess = dataCache.LastSuccess,
                    MonitorStatus = dataCache.GetCachedDataMonitorStatus(),
                    MonitorStatusReason = dataCache.GetCachedDataMonitorStatusReason(),
                    NextPoll = dataCache.NextPoll,
                    PollsSuccessful = dataCache.PollsSuccessful,
                    PollsTotal = dataCache.PollsTotal,
                    CacheFailureForSeconds = dataCache.CacheFailureForSeconds,
                    CacheForSecond = dataCache.CacheForSeconds,
                    CachedDataCount = dataCache.ContainsCachedData ? (dataCache.CachedData is IList ? ((IList)dataCache.CachedData).Count : dataCache.CachedData != null ? 1 : 0) : 0,
                    CachedData = ToCachedDataDictionary(dataCache.ContainsCachedData, dataCache.CachedData),
                    //CachedTrendData = dataCache.CachedTrendData.Select(d => new
                    //{
                    //    DateTime = d.Item1,
                    //    Data = d.Item2,
                    //}).ToList(),
                }).ToList(),
            };
        }

        private static List<IDictionary<string, object>> ToCachedDataDictionary(bool containsCachedData, object cachedData)
        {
            var dictionary = new List<IDictionary<string, object>>();
            if (!containsCachedData || cachedData == null)
            {
                return dictionary;
            }
            if (cachedData is IList)
            {
                foreach (var item in ((IList)cachedData))
                {
                    dictionary.Add(item.ToExpando());
                }
            }
            else
            {
                dictionary.Add(cachedData.ToExpando());
            }
            return dictionary;
        }

        [Route("api/graphs")]
        public HttpResponseMessage GetGraphs()
        {
            var node = PollingEngine.AllPollNodes.ToArray()[0] as WindowsServerNode;

            var chart = GetChart();

            var area = GetRouteChartArea(true, 100);
            var avgCPU = GetSparkSeries("Avg Load");
            chart.Series.Add(avgCPU);

            foreach (var mp in node.PerfOSProcessor.TrendData)
            {
                var totalCpu = mp.Item2.Single(x => x.Name == "Total");
                avgCPU.Points.Add(new DataPoint(mp.Item1.ToOADate(), totalCpu.Utilization));
            }

            //foreach (var mp in node.PerfOSMemory.TrendData)
            //{
            //    avgCPU.Points.Add(new DataPoint(mp.Item1.ToOADate(), mp.Item2.AvailableMBytes));
            //}

            chart.ChartAreas.Add(area);

            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(ToByteArray(chart));
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("image/png");
            return result;
        }

        const long _gb = 1024 * 1024 * 1024;

        private static ChartArea GetRouteChartArea(bool alt, double? max = null)
        {
            var area = new ChartArea("area")
            {
                //BackColor = alt ? ColorTranslator.FromHtml("#fafafa") : Color.White,
                //Position = new ElementPosition(0, 0, 100, 100),
                //InnerPlotPosition = new ElementPosition(0, 0, 100, 100),
                AxisY =
                {
                    Minimum = 0,
                    MaximumAutoSize = 100,
                    LabelStyle = { Enabled = true },
                    Interval = 10,
                    IntervalAutoMode = IntervalAutoMode.VariableCount,
                    MajorGrid = { Enabled = false },
                    MajorTickMark = { Enabled = false },
                    LineWidth = 0,
                    LineDashStyle = ChartDashStyle.Dot,
                },
                AxisX =
                {
                    MaximumAutoSize = 100,
                    LabelStyle = { Enabled = true, Format = "HH:mm:ss" },
                    LineWidth = 0,
                    MajorTickMark = { Enabled = false },
                    //Maximum = DateTime.UtcNow.ToOADate(),
                    //Minimum = DateTime.UtcNow.AddDays(-NodeStatus.GetDaysFromView(ViewRange.Summary)).ToOADate(),
                    MajorGrid = { Enabled = false }
                }
            };

            if (max.HasValue)
                area.AxisY.Maximum = max.Value;

            return area;
        }

        private static ChartArea GetSparkChartArea(double? max = null, int? daysAgo = null, bool noLine = false)
        {
            var area = new ChartArea("area")
            {
                BackColor = Color.Transparent,
                Position = new ElementPosition(0, 0, 100, 100),
                InnerPlotPosition = new ElementPosition(0, 0, 100, 100),
                AxisY =
                {
                    MaximumAutoSize = 100,
                    LabelStyle = { Enabled = false },
                    MajorGrid = { Enabled = false },
                    MajorTickMark = { Enabled = false },
                    LineColor = Color.Transparent,
                    LineDashStyle = ChartDashStyle.Dot,
                },
                AxisX =
                {
                    MaximumAutoSize = 100,
                    LabelStyle = { Enabled = false },
                    Maximum = DateTime.UtcNow.ToOADate(),
                    //Minimum = DateTime.UtcNow.AddDays(-(daysAgo ?? 1)).ToOADate(),
                    Minimum = DateTime.UtcNow.AddMinutes(-1).ToOADate(),
                    MajorGrid = { Enabled = false },
                    LineColor = ColorTranslator.FromHtml("#a3c0d7")
                }
            };

            if (max.HasValue)
                area.AxisY.Maximum = max.Value;
            if (noLine)
                area.AxisX.LineColor = Color.Transparent;

            return area;
        }

        private static Series GetSparkSeries(string name, Color? color = null)
        {
            color = color ?? Color.SteelBlue;
            return new Series(name)
            {
                ChartType = SeriesChartType.Area,
                XValueType = ChartValueType.DateTime,
                Color = ColorTranslator.FromHtml("#c6d5e2"),
                EmptyPointStyle = { Color = Color.Transparent, BackSecondaryColor = Color.Transparent },
                IsValueShownAsLabel = true,
                IsVisibleInLegend = true,
            };
        }

        private Chart GetSparkChart(int? height = null, int? width = null)
        {
            height = height.GetValueOrDefault(20);
            width = width.GetValueOrDefault(200);
            //if (Current.IsHighDPI)
            //{
            //    height *= 2;
            //    width *= 2;
            //}
            return GetChart(height, width);
        }

        private static Chart GetChart(int? height = null, int? width = null)
        {
            return new Chart
            {
                BackColor = Color.Transparent,
                Height = Unit.Pixel(height ?? 250),
                Width = Unit.Pixel(width ?? 500),
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High,
                Palette = ChartColorPalette.None
            };
        }

        public static byte[] ToByteArray(Chart chart)
        {
            var width = (int)chart.Width.Value;
            var height = (int)chart.Height.Value;

            using (var bmp = new Bitmap(width, height))
            {
                //bmp.SetResolution(326, 326); // retina max
                using (var g = Graphics.FromImage(bmp))
                {
                    chart.Paint(g, new Rectangle(new Point(0, 0), new Size(width, height)));
                }
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms.ToArray();
                }
            }
        }
    }
}
