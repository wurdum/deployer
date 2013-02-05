using System;

namespace Deployer.Service.Core.Helpers
{
    public static class DescriptionsFactory
    {
        public static DescriptionMessage Action(string actionSource) {
            return new DescriptionMessage(actionSource);
        }

        public static DescriptionMessage Start(this DescriptionMessage dm, DateTime startTime) {
            return TimeOperation(dm, startTime, "started");
        }

        public static DescriptionMessage Complete(this DescriptionMessage dm, DateTime startTime) {
            return TimeOperation(dm, startTime, "completed");
        }

        private static DescriptionMessage TimeOperation(DescriptionMessage dm, DateTime time, string operation) {
            dm.Message += string.Format("{0} {1} at: {2}", dm.Message.Trim(), operation, time.ToUniversalTime());
            return dm;
        }
    }

    public class DescriptionMessage
    {
        public DescriptionMessage(string message) {
            Message = message;
        }

        public string Message { get; set; }

        public override string ToString() {
            return Message;
        }

        public static implicit operator string(DescriptionMessage dm) {
            return dm.Message;
        }
    }
}