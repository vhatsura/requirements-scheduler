using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public sealed class ExperimentTestResultFileService : IExperimentTestResultService
    {
        private static string ServiceFolder =>
            Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "experiments-results");

        public async Task SaveExperimentTestResult(Guid experimentId, ExperimentInfo experimentInfo)
        {
            var experimentPath = Path.Combine(ServiceFolder, experimentId.ToString());
            if (!Directory.Exists(experimentPath))
            {
                Directory.CreateDirectory(experimentPath);
            }

            var fileName = Path.Combine(experimentPath, $"{experimentInfo.TestNumber}.json");
            var fileStream = File.Create(fileName);

            using (var writer = new StreamWriter(fileStream))
            {
                await writer.WriteAsync(JsonConvert.SerializeObject(experimentInfo, SerializerSettings));
            }
        }

        private JsonSerializerSettings SerializerSettings => new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,

#if DEBUG
            Formatting = Formatting.Indented
#endif
        };

        public async Task<ExperimentInfo> GetExperimentTestResult(Guid experimentId, int testNumber)
        {
            var fileName = Path.Combine(ServiceFolder, experimentId.ToString(), $"{testNumber}.json");
            if (!File.Exists(fileName))
            {
                throw new ArgumentException();
            }

            var fileStream = File.OpenRead(fileName);
            using (var reader = new StreamReader(fileStream))
            {
                var text = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<ExperimentInfo>(text, SerializerSettings);
            }
        }

        public async Task SaveAggregatedResult(Guid experimentId, IDictionary<int, ResultInfo> aggregatedResult)
        {
            var fileName = Path.Combine(ServiceFolder, experimentId.ToString(), "aggregated.json");

            var fileStream = File.Create(fileName);

            using (var writer = new StreamWriter(fileStream))
            {
                await writer.WriteAsync(JsonConvert.SerializeObject(aggregatedResult, SerializerSettings));
            }
        }

        public async Task<IDictionary<int, ResultInfo>> GetAggregatedResult(Guid experimentId)
        {
            var fileName = Path.Combine(ServiceFolder, experimentId.ToString(), "aggregated.json");

            if (!File.Exists(fileName))
            {
                return new Dictionary<int, ResultInfo>();
            }
            
            var fileStream = File.OpenRead(fileName);
            using (var reader = new StreamReader(fileStream))
            {
                var text = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<IDictionary<int, ResultInfo>>(text, SerializerSettings);
            }
        }
    }
}