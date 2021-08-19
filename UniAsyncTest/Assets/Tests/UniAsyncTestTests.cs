using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UniAsyncTest.Tests
{
    public class UniAsyncTestTests
    {
        private Thread unityThread;
        private bool testIsRunning;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            this.unityThread = Thread.CurrentThread;
        }

        [SetUp]
        public void SetUp()
        {
            this.testIsRunning = true;
        }

        [TearDown]
        public void TearDown()
        {
            Assert.IsFalse(this.testIsRunning);
        }

        [AsyncTest]
        public async Task SynchronousTest_Finishes()
        {
            await Task.FromResult(0);
            this.AssertOnUnityThread();
            this.TestFinished();
        }

        [AsyncTest]
        public async Task AsynchronousTest_Finishes()
        {
            await Task.Delay(1);
            this.AssertOnUnityThread();
            this.TestFinished();
        }

        [AsyncTest]
        public async Task NestedAsynchronousTest_Finishes()
        {
            await Task.Delay(1);
            await this.WaitAndAssertUnityThread();
            this.AssertOnUnityThread();
            this.TestFinished();
        }

        private async Task WaitAndAssertUnityThread()
        {
            await Task.Delay(1);
            this.AssertOnUnityThread();
        }

        private void AssertOnUnityThread() => Assert.AreEqual(this.unityThread, Thread.CurrentThread);

        private void TestFinished() => this.testIsRunning = false;
    }
}
