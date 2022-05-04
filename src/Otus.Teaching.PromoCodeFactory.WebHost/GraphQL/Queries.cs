using HotChocolate;
using HotChocolate.Data;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.DataAccess;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.Teaching.PromoCodeFactory.WebHost.GraphQL
{
    public class Queries
    {
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<CustomerShortResponse> Customers([Service] DataContext ctx)
        {
            var response = ctx.Customers.Select(x => new CustomerShortResponse()
            {
                Id = x.Id,
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName
            });

            return response;
        }

        [UseProjection]
        public CustomerResponse CustomesByIds([Service] DataContext ctx,
            Guid ids)
        {
            var customer = ctx.Customers
                .FirstOrDefault(x => x.Id == ids);

            return new CustomerResponse(customer);
        }
    }
}
