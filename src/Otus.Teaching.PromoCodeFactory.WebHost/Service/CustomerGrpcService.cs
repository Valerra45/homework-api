using Grpc.Core;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.Teaching.PromoCodeFactory.WebHost.Service
{
    public class CustomerGrpcService : GrpcService.CustomerGrpcService.CustomerGrpcServiceBase
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Preference> _preferenceRepository;

        public CustomerGrpcService(IRepository<Customer> customerRepository,
            IRepository<Preference> preferenceRepository
            )
        {
            _customerRepository = customerRepository;
            _preferenceRepository = preferenceRepository;
        }

        public override async Task<GetCustomersResponse> GetCustomers(GetCustomersRequest request, ServerCallContext context)
        {
            var customers = await _customerRepository.GetAllAsync();

            var customersShot = customers.Select(x => new CustomerShortResponse()
            {
                Id = x.Id.ToString(),
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToList();

            var response = new GetCustomersResponse();

            response.Customers.AddRange(customersShot);

            return response;
        }

        public override async Task<GetCustomerResponse> GetCustomer(GetCustomerRequest request, ServerCallContext context)
        {
            var customer = await _customerRepository.GetByIdAsync(Guid.Parse(request.Id));

            var response = new GetCustomerResponse()
            {
                Id = customer.Id.ToString(),
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email
            };

            var peferences = customer.Preferences.Select(x => new PreferenceResponse()
            {
                Id = x.PreferenceId.ToString(),
                Name = x.Preference.Name
            }).ToList();

            response.Preferences.AddRange(peferences);

            return response;
        }

        public override async Task<GetCustomerResponse> CreateCustomer(CreateCustomerRequest request, ServerCallContext context)
        {
            var preferenceIds = request.PreferenceIds.Select(x => Guid.Parse(x)).ToList();

            var preferences = await _preferenceRepository
                .GetRangeByIdsAsync(preferenceIds);

            var customer = new Customer()
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            customer.Preferences = preferences.Select(x => new CustomerPreference()
            {
                CustomerId = customer.Id,
                Preference = x,
                PreferenceId = x.Id
            }).ToList();
            
            await _customerRepository.AddAsync(customer);

            var createRequest = new GetCustomerRequest()
            {
                Id = customer.Id.ToString()
            };

            return await GetCustomer(createRequest, context);
        }

        public override async Task<CreateOrEditCustomerResponse> EditCustomersAsync(EditCustomerRequest request, ServerCallContext context)
        {
            var response = new CreateOrEditCustomerResponse();

            var customer = await _customerRepository.GetByIdAsync(Guid.Parse(request.Id));

            if (customer is null)
            {
                response.Result = "Not found";
               
                return response;
            }

            var preferenceIds = request.Request.PreferenceIds.Select(x => Guid.Parse(x)).ToList();

            var preferences = await _preferenceRepository
                .GetRangeByIdsAsync(preferenceIds);

            customer.FirstName = request.Request.FirstName;
            customer.LastName = request.Request.LastName;
            customer.Email = request.Request.Email;

            customer.Preferences = preferences.Select(x => new CustomerPreference()
            {
                CustomerId = customer.Id,
                Preference = x,
                PreferenceId = x.Id
            }).ToList();

            await _customerRepository.UpdateAsync(customer);
            response.Result = "Ok";

            return response;
        }

        public override async  Task<CreateOrEditCustomerResponse> DeleteCustomer(DeleteCustomerRequest request, ServerCallContext context)
        {
            var response = new CreateOrEditCustomerResponse();

            var customer = await _customerRepository.GetByIdAsync(Guid.Parse(request.Id));

            if (customer is null)
            {
                response.Result = "Not found";
                return response;
            }

            await _customerRepository.DeleteAsync(customer);

            response.Result = "Ok";

            return response;
        }
    }
}
