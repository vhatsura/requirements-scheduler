using System.Diagnostics;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.Extensions.Options;

namespace RequirementsScheduler.DAL
{
    public sealed class Database
    {
        private DbSettings Settings { get; }

        public Database(IOptions<DbSettings> settings)
        {
            //it writes generated sql to the debug console
#if DEBUG
            DataConnection.TurnTraceSwitchOn();
            DataConnection.WriteTraceLine = (s, s1) => Debug.WriteLine(s, s1);
            LinqToDB.Common.Configuration.Linq.GenerateExpressionTest = true;
#endif
            Settings = settings.Value;
        }

        public DataConnection Open()
        {
            return SqlServerTools.CreateDataConnection(Settings.ConnectionString, SqlServerVersion.v2012);
        }
    }
}
