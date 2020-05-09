using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.Library.Tests.TestData
{
    public class ExperimentsWithUnExpectedFailures : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var filePath in Directory.GetFiles("./TestData", "*.json"))
            {
                var text = File.ReadAllText(filePath);
                var experimentInfo = JsonConvert.DeserializeObject<ExperimentInfo>(text);

                yield return new object[] {filePath, experimentInfo};
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
