using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace UniAsyncTest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class AsyncTestAttribute : NUnitAttribute, ISimpleTestBuilder, IImplyFixture
    {
        private static readonly NUnitTestCaseBuilder Builder = new NUnitTestCaseBuilder();

        public TestMethod BuildFrom(IMethodInfo method, Test suite)
        {
            var parameters = new TestCaseParameters(new object[] { method, suite })
            {
                ExpectedResult = null,
                HasExpectedResult = true,
            };

            var proxyMethod = typeof(AsyncTestAttribute)
                .GetMethod(nameof(AsyncMethodProxy), BindingFlags.Static | BindingFlags.Public);
            var wrapper = new MethodWrapper(proxyMethod.DeclaringType, proxyMethod);
            suite.Method = wrapper;

            var testMethod = Builder.BuildTestMethod(wrapper, suite, parameters);

            // Overwrite name to original name
            testMethod.Name = method.Name;

            return testMethod;
        }

        // Must be public for NUnit
        public static IEnumerator AsyncMethodProxy(IMethodInfo method, Test suite)
        {
            return EnumeratorSynchronizationContext.RunTask(() => (Task)method.Invoke(suite.Fixture));
        }
    }
}
