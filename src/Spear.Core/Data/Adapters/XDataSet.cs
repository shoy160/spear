using System.Data;

namespace Spear.Core.Data.Adapters
{
    public class XDataSet : DataSet
    {
        public override void Load(IDataReader reader, LoadOption loadOption, FillErrorEventHandler errorHandler, params DataTable[] tables)
        {
            var adapter = new XLoadAdapter
            {
                FillLoadOption = loadOption,
                MissingSchemaAction = MissingSchemaAction.AddWithKey
            };
            if (errorHandler != null)
            {
                adapter.FillError += errorHandler;
            }
            adapter.FillFromReader(this, reader, 0, 0);
            if (!reader.IsClosed && !reader.NextResult())
            {
                reader.Close();
            }
        }
    }
}
