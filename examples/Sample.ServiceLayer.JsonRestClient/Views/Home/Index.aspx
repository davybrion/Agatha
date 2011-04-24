<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>
<asp:Content ID="Scripts" ContentPlaceHolderID="Scripts" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/Examples.js")%>" type="text/javascript"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Using HTTP GET AND POST with JSON/JSONP</h2>
    <p>
        The following demos show the use of the following urls</p>
    <p>
        <ul>
            <li>http://localhost/Sample.ServiceLayer.Host/Service.svc/json/post</li>
            <li>http://localhost/Sample.ServiceLayer.Host/Service.svc/json/post/oneway</li>
            <li>http://localhost/Sample.ServiceLayer.Host/Service.svc/jsonp</li>
        </ul>
    </p>
    <ul>
    </ul>
    <fieldset>
        <legend>Single Request</legend>
        <table width="100%">
            <tr>
                <td>
                    POST
                </td>
                <td>
                    GET (jsonp)
                </td>
            </tr>
            <tr>
                <td>
                    <p>
                        Please enter your date of birth in the format yyyy/mm/dd.</p>
                    <p>
                        <input type="text" name="dateofbirth" id="dateofbirth_POST" /></p>
                    <p>
                        <input type="button" value="Get Age" id="dateofbirth_execute_POST" /></p>
                    <p id="dateofbirth_result_POST">
                    </p>
                </td>
                <td>
                    <p>
                        Please enter your date of birth in the format yyyy/mm/dd.</p>
                    <p>
                        <input type="text" name="dateofbirth" id="dateofbirth_GET" /></p>
                    <p>
                        <input type="button" value="Get Age" id="dateofbirth_execute_GET" /></p>
                    <p id="dateofbirth_result_GET">
                    </p>
                </td>
            </tr>
        </table>
    </fieldset>
    <fieldset>
        <legend>Multiple Requests</legend>
        <table width="100%">
            <tr>
                <td>
                    POST
                </td>
                <td>
                    GET (jsonp)
                </td>
            </tr>
            <tr>
                <td>
                    <p>
                        Execute Multiple Requests</p>
                    <p>
                        <input type="button" id="executeMultiple_POST" value="Execute Multiple Requests" /></p>
                    <p>
                        <strong>String To Reverse</strong>
                        <input type="text" value="text to reverse" id="textToReverse_POST" /></p>
                    <ul>
                        <li><strong>HelloWorldResponse : </strong><span id="MultiResponse1_POST">No Response
                            Yet</span></li>
                        <li><strong>GetServerDateResponse : </strong><span id="MultiResponse2_POST">No Response
                            Yet</span></li>
                        <li><strong>ReverseStringResponse : </strong><span id="MultiResponse3_POST">No Response
                            Yet</span></li>
                    </ul>
                </td>
                <td>
                    <p>
                        Execute Multiple Requests</p>
                    <p>
                        <input type="button" id="executeMultiple_GET" value="Execute Multiple Requests" /></p>
                    <p>
                        <strong>String To Reverse</strong>
                        <input type="text" value="text to reverse" id="textToReverse_GET" /></p>
                    <ul>
                        <li><strong>HelloWorldResponse : </strong><span id="MultiResponse1_GET">No Response
                            Yet</span></li>
                        <li><strong>GetServerDateResponse : </strong><span id="MultiResponse2_GET">No Response
                            Yet</span></li>
                        <li><strong>ReverseStringResponse : </strong><span id="MultiResponse3_GET">No Response
                            Yet</span></li>
                    </ul>
                </td>
            </tr>
        </table>
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
