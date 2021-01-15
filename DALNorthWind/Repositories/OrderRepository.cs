using DALNorthWind.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DALNorthWind.Entities.Order;

namespace DALNorthWind.Repositories
{
   public class OrderRepository: IOrderRepository
    {
        private DbProviderFactory ProviderFactory;
        private string ConnectionString;
        public OrderRepository(string connString, string provider)
        {
            ProviderFactory = DbProviderFactories.GetFactory(provider);
            ConnectionString = connString;
        }
        public IEnumerable<Order> GetOrders()
        {
            var resultOrders = new List<Order>();
            using (var conn = ProviderFactory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT OrderID, CustomerID, EmployeeID, OrderDate, RequiredDate, "
                                    + " ShippedDate, ShipVia, Freight, ShipName, ShipAddress, "
                                    + "  ShipCity, ShipRegion, ShipPostalCode, ShipCountry, "
                                    + " CASE WHEN OrderDate is NULL THEN 'NEW' "
                                    + "      WHEN ShippedDate is NULL THEN 'IN_PROGRESS'"
                                    + "      WHEN ShippedDate is not NULL THEN 'COMPLETED'"
                                    + "      END as OrderStatus FROM Northwind.Orders";
                    cmd.CommandType = System.Data.CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new Order();
                            order.OrderID = reader.GetInt32(0);
                            order.CustomerID = reader.GetString(1);
                            order.EmployeeID = reader.GetInt32(2);
                            order.OrderDate = reader.GetDateTime(3);
                            order.RequiredDate = reader.GetDateTime(4);
                            if (!reader.IsDBNull(5))
                            {
                                order.ShippedDate = reader.GetDateTime(5);
                            }
                            order.ShipVia = reader.GetInt32(6);
                            order.Freight = reader.GetDecimal(7);
                            order.ShipName = reader.GetString(8);
                            order.OrderStatus = (Status)Enum.Parse(typeof(Status), (string)reader["OrderStatus"], true);
                            Console.WriteLine(order.OrderStatus);
                            resultOrders.Add(order);
                        }
                    }
                }
            }

            return resultOrders;

        }


    }
}
