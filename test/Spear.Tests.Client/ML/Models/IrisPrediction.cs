using Microsoft.ML.Runtime.Api;

namespace Spear.Tests.Client.ML
{
    public class IrisPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabels;
    }
}
