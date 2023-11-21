using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
	public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
	{
		private readonly ApplicationDbContext _context;

		public OrderHeaderRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
		public void update(OrderHeader orderHeader)
		{
			_context.Update(orderHeader);
		}

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			//throw new NotImplementedException();
			var orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
			if (orderHeader != null) { 
				orderHeader.OrderStatus = orderStatus;
				if (!(string.IsNullOrEmpty(paymentStatus)))
				{
					orderHeader.PaymentStatus = paymentStatus;
				}
			}
		}

		public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
		{
			//throw new NotImplementedException();
			var orderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
			if (orderHeader != null)
			{
				if (!(string.IsNullOrEmpty(sessionId))) {
				orderHeader.SessionId = sessionId;
				}
				if(!(string.IsNullOrEmpty(paymentIntentId)))
				{
					orderHeader.PaymentIntentId = paymentIntentId;
					orderHeader.PaymentDate = DateTime.Now;
				}
			}
		}
	}
}
