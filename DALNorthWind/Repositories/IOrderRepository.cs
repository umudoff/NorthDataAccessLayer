using DALNorthWind.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALNorthWind.Repositories
{
   public interface IOrderRepository
    {
        IEnumerable<Order> GetOrders();
        Order GetOrderById(int id);
        int CreateNewOrder(Order order);
        int Update(Order o);
        int Delete(int id);
         int MoveToInProgress(int id, DateTime newOrderDate);
        int MoveToCompleted(int orderId, DateTime shippedDate);
        List<CustOrderHist> CallCustOrderHist(string custID);
        List<CustOrderDetails> CallCustOrdersDetail(int OrderID);




    }
}
