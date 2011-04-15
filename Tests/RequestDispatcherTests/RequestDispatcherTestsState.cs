using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common;
using QuickGenerate;
using QuickGenerate.Meta;
using QuickGenerate.Uber;
using QuickNet.Types;
using Tests.NewRequestProcessorTests;
using Tests.RequestProcessorTests;
using xunit.extensions.quicknet;
using QuickGenerate.Primitives;

namespace Tests.RequestDispatcherTests
{
    public class RequestDispatcherTestsState : AbstractState
    {
        public RequestDispatcher RequestDispatcher;

        public GeneratorRepository GeneratorRepository;

        private List<string> keysUsed;
        private List<Type> requestTypesUsed;
        private List<Type> keyRequestTypesUsed;

        public RequestDispatcherTestsState()
        {
            GeneratorRepository =
                new GeneratorRepository()
                    .With<string>(mi => true, new StringGenerator(1, 1))
                    .With<int>(mi => true, new IntGenerator(0, 5))
                    .With<Request, FirstRequest>()
                    .With<Request, SecondRequest>()
                    .With<Request, ThirdRequest>()
                    .With<Request, FourthRequest>()
                    .With<Request, FifthRequest>()
                    .With<Request, FirstCachedRequest>()
                    .With<Request, SecondCachedRequest>()
                    .With<DescribedAction>(
                            new DescribedAction { Exception = null, Description = "Does nothing" },
                            new DescribedAction { Exception = new BusinessException(), Description = "Throws BusinessException" },
                            new DescribedAction { Exception = new SecurityException(), Description = "Throws SecurityException" },
                            new DescribedAction { Exception = new UnknownException(), Description = "Throws UnknownException" },
                            new DescribedAction { Exception = new AnotherUnknownException(), Description = "Throws AnotherUnknownException" })
                    .With<Type>(
                            typeof(FirstRequest),
                            typeof(SecondRequest),
                            typeof(ThirdRequest),
                            typeof(FourthRequest),
                            typeof(FifthRequest),
                            typeof(FirstCachedRequest),
                            typeof(SecondCachedRequest));
        }

        public override void Setup()
        {
            RequestDispatcher = new RequestDispatcher(null, null);
            ClearUsedTypes();
        }

        public void ClearUsedTypes()
        {
            keysUsed = new List<string>();
            requestTypesUsed = new List<Type>();
            keyRequestTypesUsed = new List<Type>();
        }

        public bool RequestTypeAllreadyUsed(params Request[] input)
        {
            return 
                   requestTypesUsed.Any(type => input.Any(request => type == request.GetType()))
                || keyRequestTypesUsed.Any(type => input.Any(request => type == request.GetType()));
        }

        public void RegisterRequestTypeUsed(params Type[] type)
        {
            requestTypesUsed.AddRange(type);
        }

        public void RegisterRequestTypeUsedUpToTheFirstAllreadyUsed(params Request[] requests)
        {
            foreach (var request in requests)
            {
                if(RequestTypeAllreadyUsed(request))
                    break;
                RegisterRequestTypeUsed(request.GetType());
            }
        }

        public bool KeyAllreadyUsed(string key)
        {
            return keysUsed.Any(el => el == key);
        }

        public bool RequestTypeAllreadyUsedWithoutKey(Request request)
        {
            return requestTypesUsed.Any(type => type == request.GetType());
        }

        public void RegisterRequestTypeKeyCombinationUsed(QuickNet.Types.Tuple<string, Request> combination)
        {
            keysUsed.Add(combination.First);
            keyRequestTypesUsed.Add(combination.Second.GetType());
        }
    }

    
    //TODO
    //void Add<TRequest>(Action<TRequest> action) where TRequest : Request, new();
    //void Send(params OneWayRequest[] oneWayRequests);
}
