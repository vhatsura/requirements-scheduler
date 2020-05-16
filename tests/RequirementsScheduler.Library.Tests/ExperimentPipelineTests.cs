using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Configuration.Conventions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RequirementsScheduler.BLL;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.DAL;
using RequirementsScheduler.DAL.Model;
using RequirementsScheduler.Library.Tests.TestData;
using RequirementsScheduler.Library.Worker;
using Xunit;
using Xunit.Abstractions;
using Experiment = RequirementsScheduler.BLL.Model.Experiment;

namespace RequirementsScheduler.Library.Tests
{
    public class ExperimentPipelineTests
    {
        public ExperimentPipelineTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        private readonly ITestOutputHelper _outputHelper;

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

            var experiment = new Experiment
            {
                Id = Guid.NewGuid(),
                TestsAmount = 1
            };

            var generatorMock = new Mock<IExperimentGenerator>();

            generatorMock.Setup(g =>
                    g.GenerateDataForTest(It.Is<Experiment>(ex => ex.Id == experiment.Id), It.IsAny<int>()))
                .Returns(() => experimentInfo);

            var originalGenerator = new ExperimentGenerator(new RandomizeService());

            generatorMock.Setup(g => g.GenerateP(It.IsAny<IOnlineChainNode>()))
                .Callback<IOnlineChainNode>(node => originalGenerator.GenerateP(node));

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
                Mock.Of<IOptions<DbSettings>>(),
                new OnlineExecutor());

            // Act
            await experimentPipeline.Run(Enumerable.Empty<Experiment>().Append(experiment));

            // Assert
            Assert.NotNull(resultInfo);
        }

        public static IEnumerable<object[]> DataTest()
        {
            var time = new ProcessingTime(1, 5, Distribution.Uniform);
            yield return new object[]
            {
                /*J1*/Enumerable.Empty<Detail>(),
                /*J2*/Enumerable.Empty<Detail>(),
                /*J12*/Enumerable.Range(1, 4).Select(i => new LaboriousDetail(time, time, i)),
                /*J21*/Enumerable.Empty<LaboriousDetail>()
            }; // should on second 0-1-2-3-4
            yield return new object[]
            {
                /*J1*/Enumerable.Empty<Detail>(),
                /*J2*/Enumerable.Empty<Detail>(),
                /*J12*/Enumerable.Range(1, 1).Select(i => new LaboriousDetail(time, time, i)),
                /*J21*/Enumerable.Range(2, 2).Select(i => new LaboriousDetail(time, time, i))
            }; // should be run without exceptions
            yield return new object[]
            {
                /*J1*/Enumerable.Empty<Detail>(),
                /*J2*/Enumerable.Empty<Detail>(),
                /*J12*/Enumerable.Range(1, 2).Select(i => new LaboriousDetail(time, time, i)),
                /*J21*/Enumerable.Range(3, 1).Select(i => new LaboriousDetail(time, time, i))
            }; // should be run without exceptions
        }

        [Theory]
        [ClassData(typeof(ExperimentsWithUnExpectedFailures))]
        public async Task Experiment_ShouldBeFinishedWithoutAnyExceptions(string path, ExperimentInfo experimentInfo)
        {
            // Arrange
            _outputHelper.WriteLine($"Running test with data from '{path}'");

            var experiment = new Experiment
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

            ExperimentReport experimentReport = null;
            var reportServiceMock = new Mock<IReportsService>();
            reportServiceMock.Setup(x => x.Save(It.Is<ExperimentReport>(r => r.ExperimentId == experiment.Id)))
                .Callback<ExperimentReport>((report) => experimentReport = report);

            var experimentPipeline = new ExperimentPipeline(
                generatorMock.Object,
                Mock.Of<IWorkerExperimentService>(),
                experimentTestResultService.Object,
                reportServiceMock.Object,
                Mock.Of<ILogger<ExperimentPipeline>>(),
                Mock.Of<IOptions<DbSettings>>(),
                new OnlineExecutor());

            // Act
            await experimentPipeline.Run(new[]
            {
                experiment
            }, false, true);

            // Assert
            resultInfo.Should().NotBeNull();
            experimentReport.Should().NotBeNull();
        }

        [Fact]
        public async Task RunDirectTest()
        {
            var experimentPipeline = new ExperimentPipeline(
                new ExperimentGenerator(new RandomizeService()),
                Mock.Of<IWorkerExperimentService>(),
                Mock.Of<IExperimentTestResultService>(),
                Mock.Of<IReportsService>(),
                Mock.Of<ILogger<ExperimentPipeline>>(),
                Mock.Of<IOptions<DbSettings>>(),
                new OnlineExecutor());

            var experiments = new List<Experiment>
            {
                new Experiment
                {
                    Id = Guid.NewGuid(),
                    N1 = 10, N2 = 40, N12 = 10, N21 = 40,
                    RequirementsAmount = 10000,
                    TestsAmount = 100,
                    BorderGenerationType = Distribution.Uniform, PGenerationType = Distribution.Uniform,
                    MinPercentageFromA = 50, MaxPercentageFromA = 50,
                    MinBoundaryRange = 10, MaxBoundaryRange = 1000
                }
            };

            await experimentPipeline.Run(experiments, false, true);
        }
    }
}
