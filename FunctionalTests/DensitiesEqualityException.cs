using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit.Sdk;

namespace FunctionalTests
{
    public class DensitiesEqualityException : XunitException
    {
        private string _message;
        private readonly AssertionInfo[] _assertionsInfo;

        public DensitiesEqualityException(IEnumerable<AssertionInfo> assertionsInfo)
            : base("Densities are not equal")
        {
            _assertionsInfo = assertionsInfo.ToArray();
        }

        public override string Message
        {
            get
            {
                if (_message == null)
                    _message = CreateMessage();

                return _message;
            }
        }

        string CreateMessage()
        {
            var sb = new StringBuilder(_assertionsInfo.Length);
            foreach (var assertion in _assertionsInfo)
            {
                sb.AppendFormat(
                    CultureInfo.CurrentCulture,
                    "    SegmentSize = {0}: expected = {1}, actual = {2}, index = {3}{4}",
                    assertion.SegmentSize,
                    assertion.Expected,
                    assertion.Actual,
                    assertion.Index,
                    Environment.NewLine
                );
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                "{1}{0}{2}",
                Environment.NewLine,
                UserMessage,
                sb.ToString()
            );
        }
    }
}
