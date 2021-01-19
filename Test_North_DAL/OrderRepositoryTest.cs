using DALNorthWind.Entities;
using DALNorthWind.Repositories;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Test_North_DAL
{
    [TestFixture]
    public class OrderRepositoryTest
    {
        public OrderRepository orderRepository;
        [SetUp]
        public void Init()
        {
         orderRepository = new OrderRepository(ConfigurationManager.ConnectionStrings["NorthWindDAL"].ConnectionString, ConfigurationManager.ConnectionStrings["NorthWindDAL"].ProviderName);
        }

        [Test]
        public void Test_GetOrders()
        {
            int i = 0;
            var listOrders= orderRepository.GetOrders();
            using (IEnumerator<Order> enumerator = listOrders.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    i++;
            }
            
            Assert.Greater(i,0);
        }

        [Test]
        public void Test_GetOrderById()
        {
            int id = 10275;
            Order o= orderRepository.GetOrderById(id);

            Assert.AreEqual(o.OrderID,id);
            Assert.Greater(o.ProductList.Count,1);
            Assert.Greater(o.OrderDetailList.Count,1);  
        }
        [Test]
        public void Test_CreateNewOrder()
        {
            int id = 10285;
            Order o = orderRepository.GetOrderById(id);
            o.ShipCity = "Brno";
            o.ShipAddress = "Gran Vía, 1";
            o.OrderDate = null;
            o.ShippedDate = null;
            Assert.IsTrue( orderRepository.CreateNewOrder(o)>=1);
        }

        [Test]
        public void Test_Update()
        {
            int id = 11086;
            Order o = orderRepository.GetOrderById(id);
            o.ShipCity = "Brno";
            o.ShipAddress = "8 Johnstown Road";
            o.ShipName = "Hungry Owl All-Night Grocers";

            Assert.AreEqual(orderRepository.Update(o), 1);

        }

        [Test]
        public void Test_Delete()
        {
            int id = 11059;
            Assert.IsTrue(orderRepository.Delete(id)>=1);
        }

        [Test]
        public void Test_CallCustOrderHist()
        {
            List<CustOrderHist> procResult=  orderRepository.CallCustOrderHist("QUEEN");
            Assert.Greater(procResult.Count,1);
        }

        [Test]
        public void Test_CallCustOrdersDetail()
        {
           List<CustOrderDetails> procResult= orderRepository.CallCustOrdersDetail(11076);
           Assert.Greater(procResult.Count, 1);
        }

        [Test]
        public void Test_MoveToInProgress()
        {
            int id = 11071;
            Assert.AreEqual(orderRepository.MoveToInProgress(id,DateTime.Now),1);
        } 
        
        [Test]
        public void Test_MoveToCompleted()
        {
            int id = 11062;
            Assert.AreEqual(orderRepository.MoveToCompleted(id,DateTime.Now),1);
        }
         
    }
}
