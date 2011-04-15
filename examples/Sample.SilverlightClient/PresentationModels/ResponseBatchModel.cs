using System.Collections.Generic;
namespace Sample.SilverlightClient.PresentationModels
{
    public class ResponseBatchModel
    {
        public ResponseBatchModel(string title, params string[] responseTexts)
        {
            Title = title;
            ResponseTexts = responseTexts;
        }

        public string Title { get; private set; }
        public IEnumerable<string> ResponseTexts { get; private set; }
    }
}
