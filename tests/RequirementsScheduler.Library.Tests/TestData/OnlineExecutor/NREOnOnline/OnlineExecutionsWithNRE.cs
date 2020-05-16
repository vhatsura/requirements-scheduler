using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.Library.Tests.TestData.OnlineExecutor.NREOnOnline
{
    public class OnlineExecutionsWithNRE : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var filePath in Directory.GetFiles("./TestData/OnlineExecutor/NREOnOnline", "*.json"))
            {
                var text = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeAnonymousType(text,
                    new
                    {
                        OnlineChainOnFirst = new OnlineChain(), OnlineChainOnSecond = new OnlineChain(),
                        ProcessedDetailsOnFirst = new HashSet<int>(),
                        ProcessedDetailsOnSecond = new HashSet<int>()
                    },
                    new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

                yield return new object[]
                {
                    filePath,
                    data.OnlineChainOnFirst, data.OnlineChainOnSecond,
                    data.ProcessedDetailsOnFirst, data.ProcessedDetailsOnSecond
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
