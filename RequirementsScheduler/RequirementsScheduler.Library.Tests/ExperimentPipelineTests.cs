using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.Core.Worker;
using RequirementsScheduler.Library.Extensions;
using RequirementsScheduler.Library.Worker;
using Xunit;

namespace RequirementsScheduler.Library.Tests
{
    public class ExperimentPipelineTests
    {
        [Theory]
        [MemberData(nameof(DataTest))]
        public async void Test1(
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

            generatorMock.Setup(g => g.GenerateDataForTest(It.Is<Experiment>(ex => ex.Id == experiment.Id)))
                .Returns(() => experimentInfo);

            ExperimentInfo resultInfo = null;

            var experimentTestResultService = new Mock<IExperimentTestResultService>();
            experimentTestResultService
                .Setup(e => e.SaveExperimentTestResult(It.Is<Guid>(id => id == experiment.Id), It.IsAny<ExperimentInfo>()))
                .Callback<Guid, ExperimentInfo>((id, info) => resultInfo = info);

            var experimentPipeline = new ExperimentPipeline(
                generatorMock.Object,
                Mock.Of<IWorkerExperimentService>(),
                experimentTestResultService.Object,
                Mock.Of<IReportsService>(),
                Mock.Of<ILogger<ExperimentPipeline>>());

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
            };
            yield return new object[] { DetailList.Empty, DetailList.Empty, new LaboriousDetailList(), new LaboriousDetailList() };
        }
    }
}
