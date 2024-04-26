using System;
using NUnit.Framework;
using ScoutDomains.Common;
using ScoutDomains;
using ScoutUtilities.Common;
using ScoutUtilities.Events;
using ScoutUtilities.Enums;

namespace ScoutModelsTests.MessageHubModel
{
    [TestFixture]
    public class MessageHubTests
    {
        [Test]
        public void TestMessageHub()
        {
            MessageHub msgHub = new MessageHub();
            publishMessage("Warning", MessageType.Warning);

            loginUser(msgHub);
            addMessage(msgHub, "Information", MessageType.Normal);
            addMessage(msgHub, "Error", MessageType.System);
            publishMessage("Critical", MessageType.System);

            Assert.IsTrue(msgHub.Count == 4);
            logoutUser(msgHub);
            Assert.IsTrue(msgHub.Count == 3);

            loginUser(msgHub);
            Assert.IsTrue(msgHub.Count == 3);
            logoutUser(msgHub);

            loginBciService(msgHub);
            Assert.IsTrue(msgHub.Count == 3);
            logoutUser(msgHub);

            Assert.IsTrue(msgHub.Count == 0);

            loginUser(msgHub);
            Assert.IsTrue(msgHub.Count == 0);
            logoutUser(msgHub);
        }

        private void publishMessage(string msg, MessageType msgType)
        {
            var sysMsg = new SystemMessageDomain
            {
                IsMessageActive = true,
                Message = msg,
                MessageType = msgType
            };

            MessageBus.Default.Publish(sysMsg);
        }

        private void addMessage(MessageHub msgHub, string msg, MessageType msgType)
        {
            HubMessage hubMsg = new HubMessage(DateTime.Now, msgType, msg);
            msgHub.AddMessage(hubMsg);
        }

        private void loginUser(MessageHub msgHub)
        {
            msgHub.OnUserLogin(false);
        }

        private void loginBciService(MessageHub msgHub)
        {
            msgHub.OnUserLogin(true);
        }

        private void logoutUser(MessageHub msgHub)
        {
            msgHub.OnUserLogout();
        }
    }
}
