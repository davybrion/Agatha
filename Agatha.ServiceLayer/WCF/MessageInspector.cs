using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using Common.Logging;

namespace Agatha.ServiceLayer.WCF
{
    public class MessageInspector : IDispatchMessageInspector
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(MessageInspector));
        private readonly ILog messageLogger = LogManager.GetLogger("WCF.Messages");

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (logger.IsInfoEnabled)
            {
                var bufferedCopy = request.CreateBufferedCopy(int.MaxValue);

                LogMessage("request", bufferedCopy.CreateMessage());

                request = bufferedCopy.CreateMessage();
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (logger.IsInfoEnabled && reply != null)
            {
                var bufferedCopy = reply.CreateBufferedCopy(int.MaxValue);

                LogMessage("response", bufferedCopy.CreateMessage());

                reply = bufferedCopy.CreateMessage();
            }
        }

        private void LogMessage(string messageType, Message message)
        {
            var writerSettings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = false };

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = XmlDictionaryWriter.Create(memoryStream, writerSettings))
                {
                    message.WriteMessage(writer);
                    writer.Flush();
                    var size = Math.Round(memoryStream.Position/1024d, 2);
                    logger.InfoFormat("{0} message size: ~{1} KB", messageType, size);
                }

                if (messageLogger.IsDebugEnabled)
                {
                    memoryStream.Position = 0;
                    using (var reader = new StreamReader(memoryStream))
                    {
                        messageLogger.Debug(reader.ReadToEnd());
                    }
                }
            }
            
        }
    }
}