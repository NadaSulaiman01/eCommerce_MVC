using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
	[Area(areaName: "Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailSender _emailSender;

		[BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }

		public CartController(IUnitOfWork unitOfWork,
			IEmailSender emailSender)
		{
			_unitOfWork = unitOfWork;
			_emailSender = emailSender;
		}
		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			ShoppingCartVM = new ShoppingCartVM()
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == userId, includeProperties: "Product"),
				OrderHeader = new OrderHeader()

			};

			foreach (var cartItem in ShoppingCartVM.ShoppingCartList)
			{
				cartItem.Price = GetPriceBasedOnQuantity(cartItem);
				ShoppingCartVM.OrderHeader.OrderTotal += cartItem.Price * cartItem.Count;
			}

			return View(ShoppingCartVM);
		}

		public IActionResult Plus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
			cart.Count++;
			_unitOfWork.ShoppingCart.update(cart);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId, flag:true);
			if (cart.Count <= 1)
			{
				
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == cart.ApplicationUserId).Count()-1);
                _unitOfWork.ShoppingCart.Remove(cart);
            }
			else
			{
				cart.Count--;
				_unitOfWork.ShoppingCart.update(cart);
			}

			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId, flag:true);

			
			HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == cart.ApplicationUserId).Count()-1);
            _unitOfWork.ShoppingCart.Remove(cart);

            _unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
			ShoppingCartVM = new ShoppingCartVM()
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == userId, includeProperties: "Product"),
				OrderHeader = new OrderHeader
				{
					ApplicationUser = user,
					City = user.City,
					StreetAddress = user.StreetAddress,
					State = user.State,
					PostalCode = user.PostalCode,
					Name = user.Name,
					PhoneNumber = user.PhoneNumber
				}

			};

			foreach (var cartItem in ShoppingCartVM.ShoppingCartList)
			{
				cartItem.Price = GetPriceBasedOnQuantity(cartItem);
				ShoppingCartVM.OrderHeader.OrderTotal += cartItem.Price * cartItem.Count;
			}



			return View(ShoppingCartVM);
		}

		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
			ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
			ShoppingCartVM.OrderHeader.OrderDate = DateTime.UtcNow;
			ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == userId, includeProperties: "Product");

			foreach (var cartItem in ShoppingCartVM.ShoppingCartList)
			{
				cartItem.Price = GetPriceBasedOnQuantity(cartItem);
				ShoppingCartVM.OrderHeader.OrderTotal += cartItem.Price * cartItem.Count;
			}

			if (user.CompanyId.GetValueOrDefault() == 0)
			{
				//regular customer
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;

			}
			else
			{
				//company customer
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;

			}

			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();

			foreach (var item in ShoppingCartVM.ShoppingCartList)
			{
				var orderDetail = new OrderDetail
				{
					OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
					ProductId = item.ProductId,
					Count = item.Count,
					Price = item.Price
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
			}

			_unitOfWork.Save();

			if (user.CompanyId.GetValueOrDefault() == 0)
			{
				//regular customer
				var domain = Request.Scheme +"://" + Request.Host.Value + "/";
				var options = new SessionCreateOptions
				{
					SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl = domain + "Customer/Cart/Index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach(var item in ShoppingCartVM.ShoppingCartList)
				{
					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title
							}
						},
						Quantity = item.Count
					};

					options.LineItems.Add(sessionLineItem);
				}


				var service = new SessionService();
				Session session = service.Create(options);
				_unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_unitOfWork.Save();

				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);




			}


			return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
		}

		public IActionResult OrderConfirmation(int id)
		{
			var orderHeader = _unitOfWork.OrderHeader.Get(o=> o.Id == id, includeProperties:"ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);

				if(session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}
				HttpContext.Session.Clear();
                
            }

			_emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Book Shop",
				$"<p> New Order Created - {orderHeader.Id} </p>");

			var shoppingCartItems = _unitOfWork.ShoppingCart
				.GetAll(c => c.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

			_unitOfWork.ShoppingCart.RemoveRange(shoppingCartItems);
			_unitOfWork.Save();

            return View(id);
		}

		private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
		{
			if (shoppingCart.Count <= 50)
			{
				return shoppingCart.Product.Price;
			}
			else if (shoppingCart.Count <= 100)
			{
				return shoppingCart.Product.Price50;
			}
			else
			{
				return shoppingCart.Product.Price100;
			}

		}
	}
}
