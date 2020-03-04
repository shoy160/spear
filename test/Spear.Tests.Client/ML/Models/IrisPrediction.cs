using Microsoft.ML.Data;

namespace Spear.Tests.Client.ML
{
    public class IrisPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabels;
    }
}
