using Microsoft.VisualStudio.TestTools.UnitTesting;
using RememberTheMeetup;

namespace MeetupToRTM.Tests
{
    /// <summary>
    /// Test Class
    /// </summary>
    [TestClass()]
    public class UnitTest1
    {
        AuthKeys ak = new AuthKeys();

        /// <summary>
        /// Test Method 1
        /// </summary>
        [TestMethod()]
        public void testingRTM_key()
        {
            ak.MyRTMkey = "16516584665";
            Assert.AreEqual(ak.MyRTMkey, "16516584665");
        }

        /// <summary>
        /// Test Method 2
        /// </summary>
        [TestMethod()]
        public void testingMeetup_key()
        {
            ak.MyMeetupKey = "16516584665";
            Assert.AreEqual(ak.MyMeetupKey, "16516584665");
        }
    }
}




