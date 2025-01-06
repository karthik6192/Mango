using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService cartService;
        private readonly IOrderService orderService;

        public CartController(ICartService cartService,
                              IOrderService orderService)
        {
            this.cartService = cartService;
            this.orderService = orderService;
        }

        [Authorize]
        public async  Task<IActionResult> CartIndex()
        {
            return View(await LoadCartBasedOnLoggedInUser());
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            return View(await LoadCartBasedOnLoggedInUser());
        }

        [HttpPost]
        [ActionName("Checkout")]
        public async Task<IActionResult> Checkout(CartDTO cartDTO)
        {
            CartDTO cart = await LoadCartBasedOnLoggedInUser();

            cart.CartHeader.Phone = cartDTO.CartHeader.Phone;
            cart.CartHeader.Email = cartDTO.CartHeader.Email;
            cart.CartHeader.Name = cartDTO.CartHeader.Name;

            var response = await orderService.CreateOrder(cart);

            OrderHeaderDTO orderHeaderDTO = JsonConvert.DeserializeObject<OrderHeaderDTO>(Convert.ToString(response.Result));

            if(response!=null && response.IsSuccess)
            {
                // get stripe session and redirect to stripe to place an order.


                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                StripeRequestDTO stripeRequestDTO = new()
                {
                    ApprovedUrl = domain + "cart/confirmation?orderId="+ orderHeaderDTO.OrderHeaderId,
                    CancelUrl = domain + "cart/checkout",
                    OrderHeader = orderHeaderDTO
                };

                var stripeResponse = await orderService.CreateStripeSession(stripeRequestDTO);
                StripeRequestDTO stripeResponseResult = JsonConvert.DeserializeObject<StripeRequestDTO>
                                                    (Convert.ToString(stripeResponse.Result));
                Response.Headers.Add("Location", stripeResponseResult.StripeSessionUrl);
                return new StatusCodeResult(303);
            }

            return View();


        }

 
        public async Task<IActionResult> Confirmation(int orderId)
        {
            ResponseDTO? response = await orderService.ValidateStripeSession(orderId);

            if (response != null && response.IsSuccess)
            {

                OrderHeaderDTO orderHeader = JsonConvert.DeserializeObject<OrderHeaderDTO>(Convert.ToString(response.Result));
                
                if(orderHeader.Status == SD.Status_Approved)
                {

                    return View(orderId);
                }
                
                
            }

            //redirect to other error page based on status.
            return View(orderId);

        }

        public async Task<IActionResult> Remove(int cartDetailsId)
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;

            ResponseDTO? response = await cartService.RemoveFromCartAsync(cartDetailsId);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart Updated Successfully";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDTO cartDTO)
        {


            ResponseDTO? response = await cartService.ApplyCouponAsync(cartDTO);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart Updated Successfully";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDTO cartDTO)
        {
            cartDTO.CartHeader.CouponCode = string.Empty;
            ResponseDTO? response = await cartService.ApplyCouponAsync(cartDTO);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart Updated Successfully";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDTO cartDTO)
        {
            CartDTO cart = await LoadCartBasedOnLoggedInUser();
            cart.CartHeader.Email =  User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault().Value;
            ResponseDTO? response = await cartService.EmailCart(cart);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Email will be processed and sent shortly!!";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();

        }



        private async Task<CartDTO> LoadCartBasedOnLoggedInUser()
        {


            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;

            ResponseDTO? response = await cartService.GetCartByUserIdAsync(userId);

            if(response != null && response.IsSuccess) 
            {
                CartDTO cartDTO = JsonConvert.DeserializeObject<CartDTO>(Convert.ToString(response.Result));
                return cartDTO;
            }

            return new CartDTO();

        }


    }
}
