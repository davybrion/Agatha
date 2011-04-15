using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Sample.SilverlightClient.PresentationModels
{
    public class ResponsesModel
    {
        public ResponsesModel()
        {
            Responses = new ObservableCollection<ResponseBatchModel>();
        }

        public ObservableCollection<ResponseBatchModel> Responses { get; private set; }

    }
}
