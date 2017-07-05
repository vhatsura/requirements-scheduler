using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.Core.Worker;
using RequirementsScheduler.Library.Worker;
using Xunit;

namespace RequirementsScheduler.Library.Tests
{
    public class ExperimentPipelineTests
    {
        [Theory]
        [MemberData(nameof(DataTest))]
        public async void Test1()
        {
            // Arrange
            var experimentInfo = new ExperimentInfo();

            var experiment = new Experiment() { Id = Guid.NewGuid() };

            var generatorMock = new Mock<IExperimentGenerator>();

            generatorMock.Setup(g => g.GenerateDataForTest(It.Is<Experiment>(ex => ex.Id == experiment.Id)))
                .Returns(() => experimentInfo);

            var experimentPipeline = new ExperimentPipeline(
                generatorMock.Object,
                Mock.Of<IWorkerExperimentService>(),
                Mock.Of<IExperimentTestResultService>(),
                Mock.Of<IReportsService>(),
                Mock.Of<ILogger<ExperimentPipeline>>());

            // Act
            await experimentPipeline.Run(Enumerable.Empty<Experiment>().Append(experiment));

            // Assert
        }

        public static IEnumerable<object[]> DataTest()
        {
            yield return new object[]{};
        }
    }
}
