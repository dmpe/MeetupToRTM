using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MeetupToRTM.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class UnitTest2
    {
        MeetupHelpers.MeetUp mc = new MeetupHelpers.MeetUp();

        /// <summary>
        /// Test Method 1
        /// </summary>
        [TestMethod()]
        public void testingBodyURL()
        {
            string body_req = mc.Authorize_return_Token("123", "456", "789");
            string compare = "client_id=123&client_secret=456&grant_type=authorization_code&redirect_uri=https%3A%2F%2Fdmpe.github.io%2FMeetupToRTM%2F&code=789";
            Console.WriteLine(body_req);
            Console.WriteLine(compare);
            Assert.AreEqual(compare, body_req);
        }
    }
}
