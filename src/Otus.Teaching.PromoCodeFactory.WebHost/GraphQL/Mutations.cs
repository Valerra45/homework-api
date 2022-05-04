using HotChocolate;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.DataAccess;
using Otus.Teaching.PromoCodeFactory.WebHost.Mappers;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.Teaching.PromoCodeFactory.WebHost.GraphQL
{
    public class Mutations
    {
        private readonly IRepository<Customer> _customerRepository;

        public Mutations(IRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public Customer CreateCustomer(CreateOrEditCustomerRequest request,
            [Service] DataContext ctx)
        {
            var preferences = ctx.Preferences
                .Where(x => request.PreferenceIds.Contains(x.Id));

            var customer = CustomerMapper.MapFromModel(request, preferences);

            ctx.Add(customer);
            ctx.SaveChanges();

            return customer;
        }

        public Customer UpdateCustomer(Guid id,
            CreateOrEditCustomerRequest request,
            [Service] DataContext ctx)
        {
            var customer = ctx.Customers.FirstOrDefault(x => x.Id == id);

            customer.Preferences.Clear();

            var preferences = ctx.Preferences
                .Where(x => request.PreferenceIds.Contains(x.Id));

            CustomerMapper.MapFromModel(request, preferences, customer);

            ctx.Update(customer);
            ctx.SaveChanges();

            return customer;
        }

        public async Task<int> DeleteCustomer(Guid id, [Service] DataContext ctx)
        {
            var customer = ctx.Customers.FirstOrDefault(x => x.Id == id);

            ctx.Remove(customer);

            return await ctx.SaveChangesAsync();
        }
    }
}
