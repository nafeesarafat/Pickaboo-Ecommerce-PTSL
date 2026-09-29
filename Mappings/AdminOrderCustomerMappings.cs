using System.Linq.Expressions;
using Ecommerce.Api.DTOs.Admin;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.Mappings;

public static class AdminOrderCustomerMappings
{
    public static Expression<Func<Order, AdminOrderViewDto>> OrderView =>
        order => new AdminOrderViewDto
        {
            OrderId = order.OrderId,
            UserName = order.User.UserName,
            OrderStatus = order.OrderStatus,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount
        };

    public static Expression<Func<Order, AdminCustomerOrderDto>> CustomerOrder =>
        order => new AdminCustomerOrderDto
        {
            UserId = order.UserId,
            OrderId = order.OrderId,
            OrderStatus = order.OrderStatus,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount
        };

    public static Expression<Func<UserInformation, AdminCustomerDto>> Customer =>
        user => new AdminCustomerDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            City = user.City,
            Country = user.Country,
            IsActive = user.IsActive,
            RegistrationDate = user.RegistrationDate
        };

    public static Expression<Func<UserInformation, AdminCustomerAddressDto>> CustomerAddress =>
        user => new AdminCustomerAddressDto
        {
            UserId = user.UserId,
            Address = user.Address,
            City = user.City,
            Country = user.Country
        };

    public static Expression<Func<UserInformation, AdminCustomerStatusDto>> CustomerStatus =>
        user => new AdminCustomerStatusDto
        {
            UserId = user.UserId,
            IsActive = user.IsActive
        };
}
