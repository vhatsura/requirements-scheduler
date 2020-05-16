using System.Collections.Generic;
using FluentAssertions;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.Library.Tests.TestData.OnlineExecutor.NREOnOnline;
using RequirementsScheduler.Library.Worker;
using Xunit;
using Xunit.Abstractions;

namespace RequirementsScheduler.Library.Tests
{
    public class OnlineExecutorTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly OnlineExecutor _onlineExecutor;

        public OnlineExecutorTests(ITestOutputHelper testOutputHelper)
        {
            _outputHelper = testOutputHelper;
            _onlineExecutor = new OnlineExecutor();
        }

        [Theory]
        [ClassData(typeof(OnlineExecutionsWithNRE))]
        public void Execute_ShouldBeFinishedWithoutAnyExceptions(string path,
            OnlineChain onlineChainOnFirst, OnlineChain onlineChainOnSecond, HashSet<int> processedOnFirst,
            HashSet<int> processedOnSecond)
        {
            // Arrange
            _outputHelper.WriteLine($"Running test with data from '{path}'");
            
            // Act
            var context = _onlineExecutor.Execute(onlineChainOnFirst, onlineChainOnSecond, processedOnFirst,
                processedOnSecond);

            // Assert
            context.Should().NotBeNull();
        }
    }
}
