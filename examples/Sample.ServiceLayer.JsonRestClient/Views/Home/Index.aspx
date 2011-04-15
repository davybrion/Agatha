<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>
<asp:Content ID="Scripts" ContentPlaceHolderID="Scripts" runat="server">
    <script type="text/javascript">
        window.addEvent("domready", function () {
            $("execute").addEvent("click", function () {
                var dateOfBirth = $("dateofbirth").get("value");
                var jsonRequest =
                    new Request.JSONP({
                        callBackKey: 'callback',
                        url: "http://localhost/Sample.ServiceLayer.Host/Service.svc/jsonp/",
                        data: {
                            "request": "GetAgeRequest",
                            "DateOfBirth": dateOfBirth
                        },
                        onComplete: function (responses) {
                            $("result").set("html", "You are " + responses.ProcessJsonRequestsResult[0].Age);
                        }
                    }).send();
            });

            $("executeMultiple").addEvent("click", function () {
                var textToReverse = $("textToReverse").get("value");
                var jsonRequest =
                    new Request.JSONP({
                        callBackKey: 'callback',
                        url: "http://localhost/Sample.ServiceLayer.Host/Service.svc/jsonp/",
                        data: {
                            "request[0]": "HelloWorldRequest",
                            "request[1]": "GetServerDateRequest",
                            "request[2]": "ReverseStringRequest",
                            "StringToReverse[2]": textToReverse
                        },
                        onComplete: function (responses) {
                            $("MultiResponse1").set("html", responses.ProcessJsonRequestsResult[0].Message);
                            $("MultiResponse2").set("html", responses.ProcessJsonRequestsResult[1].Date.toString());
                            $("MultiResponse3").set("html", responses.ProcessJsonRequestsResult[2].ReversedString);
                        }
                    }).send();
            });

            $("executeOneWayCommand").addEvent("click", function () {
                var cacheKey = $("cacheKey").get("value");
                var cacheValue = $("cacheValue").get("value");

                $("cacheKey").set("value", "");
                $("cacheValue").set("value", "");

                var jsonRequest =
                    new Request.JSONP({
                        callBackKey: 'callback',
                        url: "http://localhost/Sample.ServiceLayer.Host/Service.svc/jsonp/oneway",
                        data: {
                            "request": "SetCacheCommand",
                            "CacheKey": cacheKey,
                            "CacheValue": cacheValue
                        },
                        onComplete: function (responses) {
                            $("onewaysent").set("html", "One way request has been sent").fade(0);
                        }
                    }).send();
            });

            $("getCacheKeyExecute").addEvent("click", function () {
                var cacheKey = $("getCacheKey").get("value");
                var jsonRequest =
                    new Request.JSONP({
                        callBackKey: 'callback',
                        url: "http://localhost/Sample.ServiceLayer.Host/Service.svc/jsonp/",
                        data: {
                            "request": "GetCacheRequest",
                            "CacheKey": cacheKey
                        },
                        onComplete: function (responses) {
                            $("retrievedCacheValue").set("html", responses.ProcessJsonRequestsResult[0].CacheValue);
                        }
                    }).send();
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        <%: ViewData["Message"] %></h2>
    <p>
        The following demos show the use of the following urls</p>
    <p>
        <ul>
            <li>http://localhost/Sample.ServiceLayer.Host/Service.svc/jsonp/</li>
            <li>http://localhost/Sample.ServiceLayer.Host/Service.svc/jsonp/oneway</li>
        </ul>
    </p>
    <ul>
    
    </ul>
    <fieldset>
        <legend>Single Request</legend>
        <p>
            Please enter your date of birth in the format yyyy/mm/dd.</p>
        <p>
            <input type="text" name="dateofbirth" id="dateofbirth" /></p>
        <p>
            <input type="button" value="Get Age" id="execute" /></p>
        <p id="result">
        </p>
    </fieldset>
    <fieldset>
        <legend>Multiple Requests</legend>
        <p>
            Execute Multiple Requests</p>
        <p>
            <input type="button" id="executeMultiple" value="Execute Multiple Requests" /></p>
        <p>
            <strong>String To Reverse</strong>
            <input type="text" value="text to reverse" id="textToReverse" /></p>
        <ul>
            <li><strong>HelloWorldResponse : </strong><span id="MultiResponse1">No Response Yet</span></li>
            <li><strong>GetServerDateResponse : </strong><span id="MultiResponse2">No Response Yet</span></li>
            <li><strong>ReverseStringResponse : </strong><span id="MultiResponse3">No Response Yet</span></li>
        </ul>
    </fieldset>
    <fieldset>
        <legend>One Way Request</legend>
        <p>
            Use the following to set a server cache item</p>
        <table>
            <tr>
                <td>
                    Cache Key
                </td>
                <td>
                    Cache Value
                </td>
            </tr>
            <tr>
                <td>
                    <input type="text" id="cacheKey" />
                </td>
                <td>
                    <input type="text" id="cacheValue" />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <input type="button" id="executeOneWayCommand" value="Send Cache Key & Value" />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <span id="onewaysent"></span>
                </td>
            </tr>
        </table>
        <p>
            Use the following to input a key to retrieve the value from the server cache you
            added using the OneWayRequest</p>
        <p>
            <strong>Cache Key : </strong>
            <input type="text" id="getCacheKey" /><input type="button" id="getCacheKeyExecute"
                value="Get Cache Value" /></p>
        <p>
            <strong>Retrieved Cache Value : </strong><span id="retrievedCacheValue">No Value</span></p>
    </fieldset>
</asp:Content>
