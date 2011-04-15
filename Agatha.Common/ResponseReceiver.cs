using System;
using System.Collections.Generic;
using System.Linq;
using Agatha.Common.Caching;

namespace Agatha.Common
{
    /// <summary>
    /// Originally written by Tom Ceulemans
    /// </summary>
    public class ResponseReceiver
    {
        private readonly Action<ReceivedResponses> responseReceivedCallback;
        private readonly Action<ExceptionInfo, ExceptionType> exceptionAndTypeOccuredCallback;
        private readonly Action<ExceptionInfo> exceptionOccurredCallback;
        private readonly Dictionary<string, int> keyToResultPositions;
        private readonly ICacheManager cacheManager;


        public ResponseReceiver(Action<ReceivedResponses> responseReceivedCallback, Action<ExceptionInfo> exceptionOccurredCallback,
            Dictionary<string, int> keyToResultPositions, ICacheManager cacheManager)
        {
            if (responseReceivedCallback == null) throw new ArgumentNullException("responseReceivedCallback");
            if (exceptionOccurredCallback == null) throw new ArgumentNullException("exceptionOccurredCallback");

            this.responseReceivedCallback = responseReceivedCallback;
            this.exceptionOccurredCallback = exceptionOccurredCallback;
            this.keyToResultPositions = keyToResultPositions;
            this.cacheManager = cacheManager;
        }

        public ResponseReceiver(
            Action<ReceivedResponses> responseReceivedCallback,
            Action<ExceptionInfo, ExceptionType> exceptionAndTypeOccuredCallback,
            Dictionary<string, int> keyToResultPositions, ICacheManager cacheManager)
        {
            if (responseReceivedCallback == null) throw new ArgumentNullException("responseReceivedCallback");
            if (exceptionAndTypeOccuredCallback == null) throw new ArgumentNullException("exceptionAndTypeOccuredCallback");

            this.responseReceivedCallback = responseReceivedCallback;
            this.exceptionAndTypeOccuredCallback = exceptionAndTypeOccuredCallback;
            this.keyToResultPositions = keyToResultPositions;
            this.cacheManager = cacheManager;
        }

        public void ReceiveResponses(ProcessRequestsAsyncCompletedArgs args, Response[] tempResponseArray, Request[] requestsToSendAsArray)
        {
            if (args == null)
            {
                responseReceivedCallback(new ReceivedResponses(tempResponseArray, keyToResultPositions));
            }
            else
            {
                if (HasException(args))
                {
                    HandleException(args);
                }
                else
                {
                    var disposable = responseReceivedCallback.Target as Disposable;

                    if (disposable == null || !disposable.IsDisposed)
                    {
                        var receivedResponses = args.Result;
                        AddCacheableResponsesToCache(receivedResponses, requestsToSendAsArray);
                        PutReceivedResponsesInTempResponseArray(tempResponseArray, receivedResponses);

                        responseReceivedCallback(new ReceivedResponses(tempResponseArray, keyToResultPositions));
                    }
                }
            }
        }

        private void AddCacheableResponsesToCache(Response[] receivedResponses, Request[] requestsToSend)
        {
            if (cacheManager == null) return; //cacheManager should only be null during unit tests!
            
            for (int i = 0; i < receivedResponses.Length; i++)
            {
                if (receivedResponses[i].ExceptionType == ExceptionType.None && cacheManager.IsCachingEnabledFor(requestsToSend[i].GetType()))
                {
                    cacheManager.StoreInCache(requestsToSend[i], receivedResponses[i]);
                }
            }
        }

        private void PutReceivedResponsesInTempResponseArray(Response[] tempResponseArray, Response[] receivedResponses)
        {
            int takeIndex = 0;

            for (int i = 0; i < tempResponseArray.Length; i++)
            {
                if (tempResponseArray[i] == null)
                {
                    tempResponseArray[i] = receivedResponses[takeIndex++];
                }
            }
        }

        private void HandleException(ProcessRequestsAsyncCompletedArgs args)
        {
            var disposable = responseReceivedCallback.Target as Disposable;

            if (disposable == null || !disposable.IsDisposed)
            {
                var exception = GetException(args);

                if (exceptionOccurredCallback != null)
                {
                    exceptionOccurredCallback(exception);
                }
                else if (exceptionAndTypeOccuredCallback != null)
                {
                    var exceptionType = GetExceptionType(args);

                    exceptionAndTypeOccuredCallback(exception, exceptionType);
                }
                else
                {
                    responseReceivedCallback(new ReceivedResponses(args.Result, keyToResultPositions));
                }
            }
        }

        private static bool HasException(ProcessRequestsAsyncCompletedArgs args)
        {
            if (args.Error == null)
            {
                return args.Result.Any(r => r.Exception != null);
            }

            return true;
        }

        private static ExceptionInfo GetException(ProcessRequestsAsyncCompletedArgs args)
        {
            if (args.Error == null)
            {
                var responseWithException = GetFirstException(args.Result);
                if (responseWithException != null)
                {
                    return responseWithException.Exception;
                }

                return null;
            }

            return new ExceptionInfo(args.Error);
        }

        private static ExceptionType GetExceptionType(ProcessRequestsAsyncCompletedArgs args)
        {
            if (args.Error == null)
            {
                var responseWithException = GetFirstException(args.Result);
                if (responseWithException != null)
                {
                    return responseWithException.ExceptionType;
                }

                return ExceptionType.Unknown;
            }

            return ExceptionType.Unknown;
        }

        private static Response GetFirstException(IEnumerable<Response> responsesToCheck)
        {
            return responsesToCheck.FirstOrDefault(r => r.Exception != null);
        }
    }
}