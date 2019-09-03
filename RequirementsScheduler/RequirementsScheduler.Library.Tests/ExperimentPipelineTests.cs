using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RequirementsScheduler.BLL;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.DAL;
using RequirementsScheduler.Library.Extensions;
using RequirementsScheduler.Library.Worker;
using Xunit;

namespace RequirementsScheduler.Library.Tests
{
    public class ExperimentPipelineTests
    {
        [Fact]
        public async Task RunDirectTest()
        {
            var experimentPipeline = new ExperimentPipeline(
                new ExperimentGenerator(),
                Mock.Of<IWorkerExperimentService>(),
                Mock.Of<IExperimentTestResultService>(),
                Mock.Of<IReportsService>(),
                Mock.Of<ILogger<ExperimentPipeline>>(),
                Mock.Of<IOptions<DbSettings>>());

            await experimentPipeline.Run(Enumerable.Empty<Experiment>().Append(new Experiment
            {
                Id = Guid.NewGuid(),
                N1 = 5, N2 = 5, N12 = 5, N21 = 85,
                RequirementsAmount = 10_000,
                TestsAmount = 100,
                BorderGenerationType = "uniform", PGenerationType = "uniform",
                MinPercentageFromA = 15, MaxPercentageFromA = 15,
                MinBoundaryRange = 10, MaxBoundaryRange = 1000
            }));
        }

        [Theory]
        [MemberData(nameof(DataTest))]
        public async Task Test1(
            IEnumerable<Detail> j1,
            IEnumerable<Detail> j2,
            IEnumerable<LaboriousDetail> j12,
            IEnumerable<LaboriousDetail> j21)
        {
            // Arrange
            var experimentInfo = new ExperimentInfo();

            experimentInfo.J1.AddRange(j1);
            experimentInfo.J2.AddRange(j2);
            experimentInfo.J12.AddRange(j12);
            experimentInfo.J21.AddRange(j21);

            var experiment = new Experiment()
            {
                Id = Guid.NewGuid(),
                TestsAmount = 1
            };

            var generatorMock = new Mock<IExperimentGenerator>();

            generatorMock.Setup(g =>
                    g.GenerateDataForTest(It.Is<Experiment>(ex => ex.Id == experiment.Id), It.IsAny<int>()))
                .Returns(() => experimentInfo);

            ExperimentInfo resultInfo = null;

            var experimentTestResultService = new Mock<IExperimentTestResultService>();
            experimentTestResultService
                .Setup(e => e.SaveExperimentTestResult(It.Is<Guid>(id => id == experiment.Id),
                    It.IsAny<ExperimentInfo>()))
                .Callback<Guid, ExperimentInfo>((id, info) => resultInfo = info);

            var experimentPipeline = new ExperimentPipeline(
                generatorMock.Object,
                Mock.Of<IWorkerExperimentService>(),
                experimentTestResultService.Object,
                Mock.Of<IReportsService>(),
                Mock.Of<ILogger<ExperimentPipeline>>(),
                Mock.Of<IOptions<DbSettings>>());

            // Act
            await experimentPipeline.Run(Enumerable.Empty<Experiment>().Append(experiment));

            // Assert
            Assert.NotNull(resultInfo);
        }

        public static IEnumerable<object[]> DataTest()
        {
            var time = new ProcessingTime(1, 5);
            yield return new object[]
            {
                Enumerable.Empty<Detail>(),
                Enumerable.Empty<Detail>(),
                Enumerable.Range(1, 4).Select(i => new LaboriousDetail(time, time, i)),
                Enumerable.Empty<LaboriousDetail>()
            }; // should on second 0-1-2-3-4
            yield return new object[]
            {
                Enumerable.Empty<Detail>(),
                Enumerable.Empty<Detail>(),
                Enumerable.Range(1, 1).Select(i => new LaboriousDetail(time, time, i)),
                Enumerable.Range(2, 2).Select(i => new LaboriousDetail(time, time, i))
            }; // should be run without exceptions
            yield return new object[]
            {
                Enumerable.Empty<Detail>(),
                Enumerable.Empty<Detail>(),
                Enumerable.Range(1, 2).Select(i => new LaboriousDetail(time, time, i)),
                Enumerable.Range(3, 1).Select(i => new LaboriousDetail(time, time, i))
            }; // should be run without exceptions
        }
    }
}
