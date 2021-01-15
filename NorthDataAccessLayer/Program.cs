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
            orderRepository.GetOrders();

            Console.ReadLine();
        }
    }
}
