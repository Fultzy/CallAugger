using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Controllers.Generators.Add_ons
{
    static class WeekGraphAddon
    {
        public static Worksheet AddGraph(Worksheet worksheet)
        {
            // add the graph
            ChartObjects chartObjects = (ChartObjects)worksheet.ChartObjects(Type.Missing);
            ChartObject chartObject = chartObjects.Add(10, 80, 300, 300);
            Chart chart = chartObject.Chart;

            // set the chart type
            chart.ChartType = XlChartType.xlColumnClustered;

            // set the chart data
            Range chartRange = worksheet.get_Range("A1", "B3");
            chart.SetSourceData(chartRange, Type.Missing);

            // set the chart title
            chart.HasTitle = true;
            chart.ChartTitle.Text = "Call Duration by Week";

            // set the chart axes
            Axis xAxis = (Axis)chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary);
            xAxis.HasTitle = true;
            xAxis.AxisTitle.Text = "Week";

            Axis yAxis = (Axis)chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary);
            yAxis.HasTitle = true;
            yAxis.AxisTitle.Text = "Duration";

            return worksheet;
        }
    }
}
