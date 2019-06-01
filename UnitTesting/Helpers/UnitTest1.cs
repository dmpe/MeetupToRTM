using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeetupToRTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetupToRTM.Tests
{
    [TestClass()]
    public class UnitTest1
    {
        AuthKeys ak = new AuthKeys();

        [TestMethod()]
        public void AuthKeysTest()
        {
            ak.MyRTMkey = "16516584665";
            Assert.AreEqual(ak.MyRTMkey, "16516584665");
        }

        [TestMethod()]
        public void AuthKeysTest1()
        {
            ak.MyMeetupKey = "16516584665";
            Assert.AreEqual(ak.MyMeetupKey, "16516584665");
        }

        [TestMethod()]
        public void AuthKeysTest2()
        {

        }

        [TestMethod()]
        public void AuthKeysTest3()
        {

        }
    }
}