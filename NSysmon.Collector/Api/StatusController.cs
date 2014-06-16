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
using NSysmon.Collector.WMI;
using NWhisper;

namespace NSysmon.Collector.Api
{
    public class StatusController : ApiController
    {
        [Route("api/counters")]
        public CounterListViewModel GetCounterList()
        {
            var viewModel = new CounterListViewModel();
            var dictionary = PerfMon.CounterDictionary.ReadFromFile(@"C:\temp\Counters\index.dat");
            foreach (var counter in dictionary)
            {
                var path = string.Format(@"c:\temp\counters\{0}.dat", counter.CounterId);
                var header = NWhisper.Whipser.Info(path);
                viewModel.Counters.Add(new CounterListViewModel.Counter()
                {
                    CounterId = counter.CounterId,
                    Hostname = counter.Hostname,
                    PerformanceCounterCategory = counter.PerformanceCounterCategory,
                    PerformanceCounterName = counter.PerformanceCounterName,
                    PerformanceCounterInstance = counter.PerformanceCounterInstance,
                    Path = path,
                    MaxRetention = header.MaxRetention,
                });
            }
            viewModel.Counters =
                viewModel.Counters
                    .OrderBy(x => x.Hostname)
                    .ThenBy(x => x.PerformanceCounterCategory)
                    .ThenBy(x => x.PerformanceCounterInstance)
                    .ThenBy(x => x.PerformanceCounterName)
                    .ToList();
            return viewModel;
        }

        [Route("api/counter/{id}")]
        public HttpResponseMessage GetCounterGraph(Guid id)
        {
            var path = string.Format(@"c:\temp\counters\{0}.dat", id);
            var header = NWhisper.Whipser.Info(path);
            var from = DateTime.UtcNow.AddMinutes(-30).ToUnixTime();
            var data = NWhisper.Whipser.Fetch(path, from).Value;

            var chart = GetChart();
            var area = GetRouteChartArea(true, data.ValueList.Max(x => x.value) * 1.25);
            var series = GetSparkSeries("Values");
            chart.Series.Add(series);
            foreach (var item in data.ValueList.Where(x => x.Timestamp >= from).OrderBy(x => x.Timestamp))
            {
                series.Points.Add(new DataPoint(item.Timestamp.FromUnixTime().ToLocalTime().ToOADate(), item.value));
            }
            chart.ChartAreas.Add(area);

            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(ToByteArray(chart));
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("image/png");
            return result;
        }

        [Route("api/nodes")]
        public NodeListViewModel GetNodeList(bool? includeData = false)
        {
            return new NodeListViewModel
            {
                Nodes = PollingEngine.AllPollNodes
                .OrderBy(x => x.NodeType)
                .ThenBy(x => x.UniqueKey)
                .Select(node => GetNodeStatusViewModel(node, includeData)).ToList(),
            };
        }

        [Route("api/node/{nodeType}/{nodeKey}/status")]
        public NodeStatusViewModel GetNodeStatus(string nodeType, string nodeKey, bool? includeData = false)
        {
            var node = PollingEngine.GetNode(HttpUtility.UrlDecode(nodeType), HttpUtility.UrlDecode(nodeKey));
            return GetNodeStatusViewModel(node, includeData);
        }

        private static NodeStatusViewModel GetNodeStatusViewModel(PollNode node, bool? includeData = false)
        {
            return new NodeStatusViewModel
            {
                NodeType = node.NodeType,
                UniqueKey = node.UniqueKey,
                LastPoll = node.LastPoll,
                LastPollDuration = node.LastPollDuration,
                MinSecondsBetweenPolls = node.MinSecondsBetweenPolls,
                PollTaskStatus = node.PollTaskStatus,
                PollerCount = node.DataCaches.Count(),
                DataCaches = node.DataCaches.Select(dataCache => new NodeDataCacheViewModel
                {
                    Name = dataCache.ParentMemberName,
                    Type = dataCache.CachedDataType,
                    UniqueId = dataCache.UniqueId,
                    LastPoll = dataCache.LastRefresh,
                    LastPollDuration = dataCache.LastRefreshDuration,
                    LastPollStatus = dataCache.LastRefreshStatus,
                    LastSuccess = dataCache.LastRefreshSuccess,
                    NextPoll = dataCache.CacheExpiration,
                    PollsSuccessful = dataCache.RefreshCountSuccessful,
                    PollsTotal = dataCache.RefreshCountTotal,
                    CacheFailureForSeconds = dataCache.CacheFailureForSeconds,
                    CacheForSecond = dataCache.CacheForSeconds,
                    CachedDataCount = dataCache.ContainsCachedData() ? (dataCache.CachedData is IList ? ((IList)dataCache.CachedData).Count : dataCache.CachedData != null ? 1 : 0) : 0,
                    CachedData = includeData == true ? ToObjectArray(dataCache.ContainsCachedData(), dataCache.CachedData) : new object[0],
                    //CachedTrendData = dataCache.CachedTrendData.Select(d => new
                    //{
                    //    DateTime = d.Item1,
                    //    Data = d.Item2,
                    //}).ToList(),
                }).ToList(),
            };
        }

        private static object[] ToObjectArray(bool containsCachedData, object cachedData)
        {
            var objects = new List<object>();
            if (!containsCachedData || cachedData == null)
            {
                return objects.ToArray();
            }
            if (cachedData is IList)
            {
                foreach (var item in ((IList)cachedData))
                {
                    objects.Add(item);
                }
            }
            else
            {
                objects.Add(cachedData);
            }
            return objects.ToArray();
        }

        [Route("api/graphs")]
        public HttpResponseMessage GetGraphs()
        {
            var node = PollingEngine.AllPollNodes.ToArray()[0] as WindowsServerNode;

            var chart = GetChart();

            var area = GetRouteChartArea(true, 100);
            var avgCPU = GetSparkSeries("Avg Load");
            chart.Series.Add(avgCPU);

            //foreach (var mp in node.PerfOSProcessor.TrendData)
            //{
            //    var totalCpu = mp.Item2.Single(x => x.Name == "Total");
            //    avgCPU.Points.Add(new DataPoint(mp.Item1.ToOADate(), totalCpu.Utilization));
            //}

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
                    LineColor = Color.White,
                    LabelStyle = { Font = new Font("Trebuchet MS", 2.25f) },
                    MajorGrid = { LineColor = ColorTranslator.FromHtml("#e6e6e6") },
                    MinorGrid = { Enabled = false, LineColor = ColorTranslator.FromHtml("#e6e6e6") },
                    //Minimum = 0,
                    //MaximumAutoSize = 100,
                    //LabelStyle = { Enabled = true },
                    //Interval = 10,
                    //IntervalAutoMode = IntervalAutoMode.FixedCount,
                    //MajorGrid = { Enabled = false },
                    //MajorTickMark = { Enabled = false },
                    //LineWidth = 0,
                    //LineDashStyle = ChartDashStyle.Dot,
                },
                AxisX =
                {
                    LineColor = Color.White,
                    LabelStyle = { Font = new Font("Trebuchet MS", 8.25f), Angle = -90, Format = "HH:mm:ss" },
                    MajorGrid = { LineColor = ColorTranslator.FromHtml("#e6e6e6") },
                    MinorGrid = { Enabled = false, LineColor = ColorTranslator.FromHtml("#e6e6e6") },
                    //IsMarginVisible = false,
                    //LabelAutoFitStyle = LabelAutoFitStyles.DecreaseFont,
                    //MaximumAutoSize = 100,
                    //LabelStyle = { Enabled = true, Format = "HH:mm:ss" },
                    //LineWidth = 0,
                    //MajorTickMark = { Enabled = false },
                    ////Maximum = DateTime.UtcNow.ToOADate(),
                    ////Minimum = DateTime.UtcNow.AddDays(-NodeStatus.GetDaysFromView(ViewRange.Summary)).ToOADate(),
                    //MajorGrid = { Enabled = false }
                }
            };

            //if (max.HasValue)
            //    area.AxisY.Maximum = max.Value;

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
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                //Color = ColorTranslator.FromHtml("#c6d5e2"),
                //EmptyPointStyle = { Color = Color.Transparent, BackSecondaryColor = Color.Transparent },
                //IsValueShownAsLabel = false,
                //IsVisibleInLegend = false,
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
                //BackColor = Color.Transparent,
                Height = Unit.Pixel(height ?? 100),
                Width = Unit.Pixel(width ?? 540),
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High,
                //Palette = ChartColorPalette.None
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
