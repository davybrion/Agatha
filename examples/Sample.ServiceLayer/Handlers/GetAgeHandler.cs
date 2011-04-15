using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agatha.ServiceLayer;
using Sample.Common.RequestsAndResponses;

namespace Sample.ServiceLayer.Handlers
{
    public class GetAgeHandler : RequestHandler<GetAgeRequest,GetAgeResponse>
    {

        public override Agatha.Common.Response Handle(GetAgeRequest request)
        {
            var response = new GetAgeResponse();
            DateTime now = DateTime.Today;
            int age = now.Year - request.DateOfBirth.Year;
            if (request.DateOfBirth > now.AddYears(-age)) age--;
            response.Age = age;
            return response;
        }
    }
}
