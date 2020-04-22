using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.ReportDesigner.Services;

namespace xrefcoredemo.Services {
    public class CustomPreviewReportCustomizationService : PreviewReportCustomizationService {
        readonly IObjectDataSourceInjector objectDataSourceInjector;
        public CustomPreviewReportCustomizationService(IObjectDataSourceInjector objectDataSourceInjector) {
            this.objectDataSourceInjector = objectDataSourceInjector;

        }
        public override void CustomizeReport(XtraReport report) {
            objectDataSourceInjector.Process(report);
        }
    }
}
