using System;
using NUnit.Framework;

namespace Konata.Test.Simple
{
    /// <summary>
    /// ����������׼ģ��ο�
    /// </summary>
    [TestFixture(Description = "����ģ��")]
    public class SimpleTest : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            Console.WriteLine($"��ʼ���й��ܲ���");
        }

        [Test(Description ="test2")]
        [Category("�����ȡ��������")]
        public void Test2()
        {

        }

        [TearDown]
        public void Dispose()
        {
            Console.WriteLine("�ͷ���Դ");
        }
    }
}