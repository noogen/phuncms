namespace Phun.Tests.Templating
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Phun.Templating;

    /// <summary>
    /// Trace tests.
    /// </summary>
    [TestClass]
    public class TraceTests
    {
        /// <summary>
        /// Tests the trace to diagnostic trace.
        /// </summary>
        [TestMethod]
        public void TestTraceToDiagnosticTrace()
        {
            // Arrange
            var t = new Trace();
            var myListener = new MockTraceListener();
            System.Diagnostics.Trace.Listeners.Clear();
            System.Diagnostics.Trace.Listeners.Add(myListener);
            var expected = 1;

            // Act
            t.log("test");

            // Assert
            Assert.AreEqual(expected, myListener.logEntries.Count);
        }

        /// <summary>
        /// Mock trace listener.
        /// </summary>
        public class MockTraceListener : System.Diagnostics.TraceListener
        {
            /// <summary>
            /// The log entries
            /// </summary>
            internal protected readonly List<string> logEntries = new List<string>();

            /// <summary>
            /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
            /// </summary>
            /// <param name="message">A message to write.</param>
            public override void Write(string message)
            {
                this.logEntries.Add(message);
            }

            /// <summary>
            /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
            /// </summary>
            /// <param name="message">A message to write.</param>
            public override void WriteLine(string message)
            {
                this.logEntries.Add(message);
            }
        }
    }
}
