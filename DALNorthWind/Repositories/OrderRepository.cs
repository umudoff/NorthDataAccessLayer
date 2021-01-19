using DALNorthWind.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DALNorthWind.Entities.Order;

namespace DALNorthWind.Repositories
{
    public class OrderRepository : IOrderRepository
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
                    cmd.CommandType = CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new Order();
                            order.OrderID = reader.GetInt32(0);
                            order.CustomerID = reader.GetString(1);
                            if (!reader.IsDBNull(2)) {
                                order.EmployeeID = reader.GetInt32(2);
                            }
                            if (!reader.IsDBNull(3))
                            {
                               order.OrderDate = reader.GetDateTime(3);
                            }
                            if (!reader.IsDBNull(4)) {
                                order.RequiredDate = reader.GetDateTime(4);
                            }
                            if (!reader.IsDBNull(5))
                            {
                                order.ShippedDate = reader.GetDateTime(5);
                            }
                            if (!reader.IsDBNull(6))
                            {
                                order.ShipVia = reader.GetInt32(6);
                            }
                            if (!reader.IsDBNull(7))
                            {
                                order.Freight = reader.GetDecimal(7);
                            }
                            if (!reader.IsDBNull(8))
                            {
                                order.ShipName = reader.GetString(8);
                            }
                            if (!reader.IsDBNull(14))
                            {
                               order.OrderStatus = (Status)Enum.Parse(typeof(Status), (string)reader["OrderStatus"], true);
                            }
                            resultOrders.Add(order);
                        }
                    }
                }
            }

            return resultOrders;

        }




        public Order GetOrderById(int id)
        {

            string cmdtext = "SELECT *, CASE WHEN OrderDate is NULL THEN 'NEW' " +
                                      "      WHEN ShippedDate is NULL THEN 'IN_PROGRESS'" +
                                      "      WHEN ShippedDate is not NULL THEN 'COMPLETED'" +
                                      "      END as OrderStatus FROM Northwind.Orders WHERE OrderID=@id; " +
                                      "SELECT od.OrderID, od.ProductID, od.UnitPrice," +
                                      "od.Quantity, od.Discount, p.ProductName, p.SupplierID, p.CategoryID " +
                                      " FROM Northwind.[Order Details] od, Northwind.Products p " +
                                      "WHERE  od.OrderID=@id and  p.ProductID= od.ProductID ;";

            var order = new Order();
            List<Product> prodList = new List<Product>();
            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            using (var conn = ProviderFactory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = cmdtext;
                    cmd.CommandType = CommandType.Text;
                    var Orderid = cmd.CreateParameter();
                    Orderid.ParameterName = "@id";
                    Orderid.Value = id;
                    cmd.Parameters.Add(Orderid);

                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        order.OrderID = reader.GetInt32(0);
                        order.CustomerID = reader.GetString(1);
                        order.EmployeeID = reader.GetInt32(2);
                        if (!reader.IsDBNull(3))
                        {
                            order.OrderDate = reader.GetDateTime(3);
                        }
                        order.RequiredDate = reader.GetDateTime(4);
                        if (!reader.IsDBNull(5))
                        {
                            order.ShippedDate = reader.GetDateTime(5);
                        }
                        order.ShipVia = reader.GetInt32(6);
                        order.Freight = reader.GetDecimal(7);
                        order.ShipName = reader.GetString(8);
                        order.OrderStatus = (Status)Enum.Parse(typeof(Status), (string)reader["OrderStatus"], true);

                        reader.NextResult();
                        while (reader.Read())
                        {
                            var prod = new Product();
                            var orderDetail = new OrderDetails();
                            orderDetail.ProductID = reader.GetInt32(1);
                            orderDetail.UnitPrice = reader.GetDecimal(2);
                            orderDetail.Quantity = reader.GetInt16(3);
                            orderDetail.Discount = reader.GetFloat(4);

                            prod.ProductName = reader.GetString(5);
                            prod.SupplierID = reader.GetInt32(6);
                            prod.CategoryID = reader.GetInt32(7);

                            orderDetailsList.Add(orderDetail);
                            prodList.Add(prod);
                        }

                        order.OrderDetailList = orderDetailsList;
                        order.ProductList = prodList;

                    }

                }
            }

            return order;
        }





        public int CreateNewOrder(Order order)
        {
            string cmdText = "INSERT INTO Northwind.Orders(CustomerID " +
                   ", EmployeeID, OrderDate, RequiredDate, " +
                   " ShippedDate, ShipVia,  Freight, ShipName, ShipAddress, ShipCity,ShipRegion,ShipPostalCode, ShipCountry ) " +
                   " VALUES (@custId, " +
                  " @empId, @orderDate, @reqDate, " +
                 " @shippedDate, @shipVia, @freight, @shipName, @shipAddress, @shipCity, @shipRegion, @shipPostalCode, @shipCountry);";
            int res;

            using (var conn = ProviderFactory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.CommandType = CommandType.Text;

                    var custId = cmd.CreateParameter();
                    custId.ParameterName = "@custId";
                    if ( order.CustomerID==null)
                    {
                        custId.Value = DBNull.Value;
                    }
                    else {
                        custId.Value = order.CustomerID;
                    }
                    cmd.Parameters.Add(custId);

                    var empId = cmd.CreateParameter();
                    empId.ParameterName = "@empId";
                    empId.Value = order.EmployeeID;
                    cmd.Parameters.Add(empId);

                    var orderDate = cmd.CreateParameter();
                    orderDate.ParameterName = "@orderDate";
                    if (order.OrderDate == null)
                    {
                        orderDate.Value = DBNull.Value;
                    }
                    else
                    {
                        orderDate.Value = order.OrderDate;
                    }
                    cmd.Parameters.Add(orderDate);
 

                    var reqDate = cmd.CreateParameter();
                    reqDate.ParameterName = "@reqDate";
                    if (order.RequiredDate == null)
                    {
                        reqDate.Value = DBNull.Value;
                    }
                    else
                    {
                        reqDate.Value = order.RequiredDate;
                    }
                    cmd.Parameters.Add(reqDate);


                    var shippedDate = cmd.CreateParameter();
                    shippedDate.ParameterName = "@shippedDate";
                    if (order.ShippedDate == null)
                    {
                        shippedDate.Value = DBNull.Value;
                    }
                    else
                    {
                        shippedDate.Value = order.ShippedDate;
                    }
                    cmd.Parameters.Add(shippedDate);

                    var shipVia = cmd.CreateParameter();
                    shipVia.ParameterName = "@shipVia";
                    shipVia.Value = order.ShipVia;
                    cmd.Parameters.Add(shipVia);

                    var freight = cmd.CreateParameter();
                    freight.ParameterName = "@freight";
                    freight.Value = order.Freight;
                    cmd.Parameters.Add(freight);

                    var shipName = cmd.CreateParameter();
                    shipName.ParameterName = "@shipName";
                    if (order.ShipName == null)
                    {
                        shipName.Value = DBNull.Value;
                    }
                    else
                    {
                        shipName.Value = order.ShipName;
                    }
                    cmd.Parameters.Add(shipName);

                    var shipAddress = cmd.CreateParameter();
                    shipAddress.ParameterName = "@shipAddress";
                    if (order.ShipAddress == null)
                    {
                        shipAddress.Value = DBNull.Value;
                    }
                    else
                    {
                        shipAddress.Value = order.ShipAddress;
                    }
                    cmd.Parameters.Add(shipAddress);

                    var shipCity = cmd.CreateParameter();
                    shipCity.ParameterName = "@shipCity";
                      if (order.ShipCity == null)
                    {
                        shipCity.Value = DBNull.Value;
                    }
                    else
                    {
                        shipCity.Value = order.ShipCity;
                    }
                    cmd.Parameters.Add(shipCity);

                    var shipRegion = cmd.CreateParameter();
                    shipRegion.ParameterName = "@shipRegion";
                    if (order.ShipRegion == null)
                    {
                        shipRegion.Value = DBNull.Value;
                    }
                    else
                    {
                        shipRegion.Value = order.ShipRegion;
                    }
                    cmd.Parameters.Add(shipRegion);

                    var shipPostalCode = cmd.CreateParameter();
                    shipPostalCode.ParameterName = "@shipPostalCode";
                    if (order.ShipPostalCode == null)
                    {
                        shipPostalCode.Value = DBNull.Value;
                    }
                    else
                    {
                        shipPostalCode.Value = order.ShipPostalCode;
                    }
                    cmd.Parameters.Add(shipPostalCode);

                    var shipCountry = cmd.CreateParameter();
                    shipCountry.ParameterName = "@shipCountry";
                    if (order.ShipCountry == null)
                    {
                        shipCountry.Value = DBNull.Value;
                    }
                    else
                    {
                        shipCountry.Value = order.ShipCountry;
                    }
                    cmd.Parameters.Add(shipCountry);

                    res = cmd.ExecuteNonQuery();

                    //Console.WriteLine(res + "  row was added.");

                }
            }

            return res;

        }

   

        public int Update(Order o)
        {
            string cmdText = "UPDATE Northwind.Orders " +
                " SET EmployeeID = @empid, RequiredDate = @reqdate, " +
                "  ShipVia = @shipvia, Freight = @freight, " +
                "  ShipName = @shipname, ShipAddress = @shipadress," +
                " ShipCity = @shipcity, ShipRegion = @shipregion, " +
                " ShipPostalCode = @shippostcode, ShipCountry = @shipcountry " +
                "    WHERE OrderID = @orderId; ";
            int res=0;

            Order origOrder = GetOrderById(o.OrderID);
            if (origOrder.OrderStatus == Order.Status.IN_PROGRESS || origOrder.OrderStatus == Order.Status.COMPLETED)
            {
                Console.WriteLine("Cannot update object in COMPLETED or IN_PROGRESS state");
            }
            else
            {

                using (var conn = ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = ConnectionString;
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = cmdText;
                        cmd.CommandType = CommandType.Text;

                        var orderId = cmd.CreateParameter();
                        orderId.ParameterName = "@orderId";
                        orderId.Value = o.OrderID;
                        cmd.Parameters.Add(orderId);

                        var empID = cmd.CreateParameter();
                        empID.ParameterName = "@empid";
                        empID.Value = o.EmployeeID;
                        cmd.Parameters.Add(empID);

                        var reqdate = cmd.CreateParameter();
                        reqdate.ParameterName = "@reqdate";
                        reqdate.Value = o.RequiredDate;
                        cmd.Parameters.Add(reqdate);

                        var shipvia = cmd.CreateParameter();
                        shipvia.ParameterName = "@shipvia";
                        shipvia.Value = o.ShipVia;
                        cmd.Parameters.Add(shipvia);

                        var freight = cmd.CreateParameter();
                        freight.ParameterName = "@freight";
                        freight.Value = o.Freight;
                        cmd.Parameters.Add(freight);

                        var shipname = cmd.CreateParameter();
                        shipname.ParameterName = "@shipname";
                        shipname.Value = o.ShipName;
                        cmd.Parameters.Add(shipname);

                        var shipadress = cmd.CreateParameter();
                        shipadress.ParameterName = "@shipadress";
                        shipadress.Value = (object)o.ShipAddress ?? DBNull.Value;
                        cmd.Parameters.Add(shipadress);

                        var shipcity = cmd.CreateParameter();
                        shipcity.ParameterName = "@shipcity";
                        shipcity.Value = (object)o.ShipCity ?? DBNull.Value; ;
                        cmd.Parameters.Add(shipcity);

                        var shipregion = cmd.CreateParameter();
                        shipregion.ParameterName = "@shipregion";
                        shipregion.Value = (object)o.ShipRegion ?? DBNull.Value;
                        cmd.Parameters.Add(shipregion);


                        var shippostcode = cmd.CreateParameter();
                        shippostcode.ParameterName = "@shippostcode";
                        shippostcode.Value = (object)o.ShipPostalCode ?? DBNull.Value;
                        cmd.Parameters.Add(shippostcode);

                        var shipcountry = cmd.CreateParameter();
                        shipcountry.ParameterName = "@shipcountry";
                        shipcountry.Value = (object)o.ShipCountry ?? DBNull.Value;
                        cmd.Parameters.Add(shipcountry);

                        res = cmd.ExecuteNonQuery();

                       // Console.WriteLine($"{res} lines were updated ");
                    }
                }

            }
            return res;
        }

        public int Delete(int id)
        {

            string cmdSelectText = "SELECT count(*) FROM Northwind.Orders " +
                                   " WHERE OrderID=@id and (OrderDate IS NULL or ShippedDate IS NULL); ";
            string cmdDetailText = " DELETE FROM [Northwind].[Order Details] " +
                                    " WHERE OrderID = @id";
            string cmdDeleteOrderText = "DELETE FROM Northwind.Orders " +
                                        "WHERE OrderID=@id and (OrderDate IS NULL or ShippedDate IS NULL)";

            int resCount = 0, res1=0, res2=0;
            using (var conn = ProviderFactory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = cmdSelectText;
                    cmd.CommandType = CommandType.Text;

                    var orderId = cmd.CreateParameter();
                    orderId.ParameterName = "@id";
                    orderId.Value = id;
                    cmd.Parameters.Add(orderId);
                    resCount = (int)cmd.ExecuteScalar();
                }
            }

            if (resCount == 0)
            {
                Console.WriteLine("Order doesn't satisfy condition");
            }
            else
            {

                using (var conn = ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = ConnectionString;
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = cmdDetailText;
                        cmd.CommandType = CommandType.Text;
                        var orderId = cmd.CreateParameter();
                        orderId.ParameterName = "@id";
                        orderId.Value = id;
                        cmd.Parameters.Add(orderId);
                        res1 = cmd.ExecuteNonQuery();
                     }
                }

                using (var conn = ProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = ConnectionString;
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = cmdDeleteOrderText;
                        cmd.CommandType = CommandType.Text;

                        var orderId = cmd.CreateParameter();
                        orderId.ParameterName = "@id";
                        orderId.Value = id;
                        cmd.Parameters.Add(orderId);

                        res2 = cmd.ExecuteNonQuery();
                     }
                }

            }

            return res1 + res2;
        }




        public int MoveToInProgress(int id, DateTime newOrderDate)
        {
            string cmdText = "UPDATE Northwind.Orders " +
                " SET OrderDate=@date WHERE OrderID=@orderId";
            int res=0;
            using (var conn = ProviderFactory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.CommandType = CommandType.Text;

                    var orderId = cmd.CreateParameter();
                    orderId.ParameterName = "@orderId";
                    orderId.Value = id;
                    cmd.Parameters.Add(orderId);

                    var orderDate = cmd.CreateParameter();
                    orderDate.ParameterName = "@date";
                    orderDate.Value = newOrderDate.ToString("yyyy-MM-dd hh:mm:ss");
                    cmd.Parameters.Add(orderDate);

                    res = cmd.ExecuteNonQuery();
                   // Console.WriteLine(res + " moved to In Progress");
                }
            }
            return res;

        }

        public int  MoveToCompleted(int orderId, DateTime shippedDate)
        {
            string cmdText = "UPDATE Northwind.Orders " +
                            " SET ShippedDate=@date WHERE OrderID=@orderId";

            int res = 0;
            using (var conn = ProviderFactory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = cmdText;
                    cmd.CommandType = CommandType.Text;

                    var newOrderId = cmd.CreateParameter();
                    newOrderId.ParameterName = "@orderId";
                    newOrderId.Value = orderId;
                    cmd.Parameters.Add(newOrderId);

                    var orderDate = cmd.CreateParameter();
                    orderDate.ParameterName = "@date";
                    orderDate.Value = shippedDate.ToString("yyyy-MM-dd hh:mm:ss");
                    cmd.Parameters.Add(orderDate);

                    res = cmd.ExecuteNonQuery();
                    Console.WriteLine(res + " moved to Done");
                }
            }
            return res;

        }

        public List<CustOrderHist> CallCustOrderHist(string custID)
        {
            List<CustOrderHist> result = new List<CustOrderHist>();
            using (var conn = ProviderFactory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "Northwind.CustOrderHist";
                    cmd.CommandType = CommandType.StoredProcedure;
                    var customer = cmd.CreateParameter();
                    customer.ParameterName = "@CustomerID";
                    customer.Value = custID;
                    cmd.Parameters.Add(customer);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CustOrderHist c = new CustOrderHist();
                            c.ProductName = reader.GetString(0);
                            c.Total = reader.GetInt32(1);
                            result.Add(c);

                        }
                    }
                }
            }
            return result;
        }

        public List<CustOrderDetails> CallCustOrdersDetail(int OrderID)
        {
            List<CustOrderDetails> result = new List<CustOrderDetails>();

            using (var conn = ProviderFactory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "Northwind.CustOrdersDetail";
                    cmd.CommandType = CommandType.StoredProcedure;
                    var order = cmd.CreateParameter();
                    order.ParameterName = "@OrderID";
                    order.Value = OrderID;
                    cmd.Parameters.Add(order);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CustOrderDetails c = new CustOrderDetails();
                            c.ProductName = reader.GetString(0);
                            c.UnitPrice = reader.GetDecimal(1);
                            c.Quantity = reader.GetInt16(2);
                            c.Discount = reader.GetInt32(3);
                            c.ExtendedPrice = reader.GetDecimal(4);
                            result.Add(c);
                        }
                    }
                }
            }
            return result;

        }







    }
}
