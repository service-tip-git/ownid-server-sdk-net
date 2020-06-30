using Serilog.Formatting.Elasticsearch;

namespace OwnIdSdk.NetCore3.Server.Gigya
{
    public class OwnIdFormatter : ElasticsearchJsonFormatter
    {
        // TODO: scope decoupling
        public OwnIdFormatter() : base(renderMessageTemplate: false, inlineFields: true)
        {
        }
    }
}