using System.Collections.Generic;
using FluentAssertions;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.Library.Tests.TestData.OnlineExecutor.NREOnOnline;
using RequirementsScheduler.Library.Worker;
using Xunit;

namespace RequirementsScheduler.Library.Tests
{
    public class OnlineExecutorTests
    {
        private readonly OnlineExecutor _onlineExecutor;

        public OnlineExecutorTests()
        {
            _onlineExecutor = new OnlineExecutor();
        }

        [Theory]
        [ClassData(typeof(OnlineExecutionsWithNRE))]
        public void Execute_ShouldBeFinishedWithoutAnyExceptions(OnlineChain onlineChainOnFirst,
            OnlineChain onlineChainOnSecond, HashSet<int> processedOnFirst, HashSet<int> processedOnSecond)
        {
            // Arrange + Act
            var context = _onlineExecutor.Execute(onlineChainOnFirst, onlineChainOnSecond, processedOnFirst,
                processedOnSecond);

            // Assert
            context.Should().NotBeNull();
        }
    }
}
