using Konata.Resource.Localization;
using NUnit.Framework;
using System;
using System.Threading;

namespace Konata.Test.Simple
{
    /// <summary>
    /// ����������׼ģ��ο�
    /// </summary>
    [TestFixture(Description ="����ģ��")]
    public class SimpleTest
    {
        [SetUp]
        public void Setup()
        {
            Console.WriteLine($"��ʼ���й��ܲ���");
        }

        [Test(Description ="test1")]
        [Category("�����Բ���")]
        public void Test1()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            Console.WriteLine(Localization.TestString);
            Assert.AreEqual(Localization.TestString, "TestString");
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
            Console.WriteLine(Localization.TestString);
            Assert.AreEqual(Localization.TestString, "�����ַ���");
        }


        [TearDown]
        public void Dispose()
        {
            Console.WriteLine("�ͷ���Դ");
        }
    }
}