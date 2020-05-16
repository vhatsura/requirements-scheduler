using System.Diagnostics;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.Extensions.Options;

namespace RequirementsScheduler.DAL
{
    public sealed class Database
    {
        public Database(IOptions<DbSettings> settings)
        {
            //it writes generated sql to the debug console
#if DEBUG
            DataConnection.TurnTraceSwitchOn();
            DataConnection.WriteTraceLine = (s, s1) => Debug.WriteLine(s, s1);
            Configuration.Linq.GenerateExpressionTest = true;
#endif
            Settings = settings.Value;
        }

        private DbSettings Settings { get; }

        public DataConnection Open() =>
            SqlServerTools.CreateDataConnection(Settings.ConnectionString, SqlServerVersion.v2012);
    }
}
