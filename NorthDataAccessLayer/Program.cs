using DALNorthWind.Entities;
using DALNorthWind.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthDataAccessLayer
{
    class Program
    {
        static void Main(string[] args)
        {
            OrderRepository orderRepository = new OrderRepository(@"Data Source=(localdb)\ProjectsV13;Initial Catalog=Northwind;Integrated Security=True", "System.Data.SqlClient");
            //   orderRepository.GetOrders();
            Order o = orderRepository.GetOrderById(10255);  // 10251);
            Console.WriteLine("custid:" +o.CustomerID);
            o.ShipCity = "Brno";

            orderRepository.CreateNewOrder(o);
          //  orderRepository.Update(o);
          // orderRepository.CreateNewOrder(o);

            // orderRepository.Delete(11008);
            // orderRepository.MoveToInProgress(10251, DateTime.Now);
            //orderRepository.CallCustOrderHist("VINET"); 10253
            // orderRepository.CallCustOrdersDetail(10253);
            Console.ReadLine();
        }
    }
}
