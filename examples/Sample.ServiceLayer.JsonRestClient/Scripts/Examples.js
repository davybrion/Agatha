function Request(name, namespace) {
    this.__type = name + ":#" + namespace;
}

function Agatha() {
    this.post = function (requests, sender) {

    }
}

function JQuerySender(serviceUrl) {
    var self = this;
    this.serviceUrl = serviceUrl;
    this.getResponses = function (requests) {
        $.ajax({
            url: "",
            type: 'post',
            contentType: "application/json",
            data: '{"requests":[{"__type":"GetAgeRequest:#Sample.Common.RequestsAndResponses","DateOfBirth":"/Date(' + dateOfBirth.getTime() + ')/"}]}',
            success: function (data, status) {
                $("#result").html("You are " + data.ProcessJsonRequestsPostResult[0].Age);
            },
            error: function (xhr, desc, err) {
                console.log(xhr);
                console.log("Desc: " + desc + "\nErr:" + err);
            }
        });
    }
}

$(document).ready(function () {
    $("#dateofbirth_execute_POST").click(function () {
        var dateOfBirth = new Date($("#dateofbirth_POST").val());
        $.ajax({
            url: "http://localhost/Sample.ServiceLayer.Host/Service.svc/json/post",
            type: 'post',
            contentType: "application/json",
            data: '{"requests":[{"__type":"GetAgeRequest:#Sample.Common.RequestsAndResponses","DateOfBirth":"/Date(' + dateOfBirth.getTime() + ')/"}]}',
            success: function (data, status) {
                $("#dateofbirth_result_POST").html("You are " + data.ProcessJsonRequestsPostResult[0].Age);
            },
            error: function (xhr, desc, err) {
                console.log(xhr);
                console.log("Desc: " + desc + "\nErr:" + err);
            }
        });
    });

    $("#dateofbirth_execute_GET").click(function () {
        $.ajax({
            url: "http://localhost/Sample.ServiceLayer.Host/Service.svc/jsonp",
            dataType: 'jsonp',
            contentType: "application/json",
            data: {
                "request": "GetAgeRequest",
                "DateOfBirth": $("#dateofbirth_GET").val()
            },
            success: function (data, status) {
                $("#dateofbirth_result_GET").html("You are " + data.ProcessJsonRequestsGetResult[0].Age);
            },
            error: function (xhr, desc, err) {
                console.log(xhr);
                console.log("Desc: " + desc + "\nErr:" + err);
            }
        });
    });

    $("#executeMultiple_POST").click(function () {
        var textToReverse = $("#textToReverse_POST").val();
        $.ajax({
            url: "http://localhost/Sample.ServiceLayer.Host/Service.svc/json/post",
            type: 'post',
            contentType: "application/json",
            data: '{"requests":[' +
                    '{"__type":"HelloWorldRequest:#Sample.Common.RequestsAndResponses"},' +
                    '{"__type":"GetServerDateRequest:#Sample.Common.RequestsAndResponses"},' +
                    '{"__type":"ReverseStringRequest:#Sample.Common.RequestsAndResponses","StringToReverse":"' + textToReverse + '"}]}',
            success: function (data, status) {
                $("#MultiResponse1_POST").html(data.ProcessJsonRequestsPostResult[0].Message);
                $("#MultiResponse2_POST").html(data.ProcessJsonRequestsPostResult[1].Date.toString());
                $("#MultiResponse3_POST").html(data.ProcessJsonRequestsPostResult[2].ReversedString);
            },
            error: function (xhr, desc, err) {
                console.log(xhr);
                console.log("Desc: " + desc + "\nErr:" + err);
            }
        });
    });

    $("#executeMultiple_GET").click(function () {
        var textToReverse = $("#textToReverse_GET").val();
        $.ajax({
            url: "http://localhost/Sample.ServiceLayer.Host/Service.svc/jsonp",
            dataType: 'jsonp',
            contentType: "application/json",
            data: {
                "request[0]": "HelloWorldRequest",
                "request[1]": "GetServerDateRequest",
                "request[2]": "ReverseStringRequest",
                "StringToReverse[2]": textToReverse
            },
            success: function (data, status) {
                $("#MultiResponse1_GET").html(data.ProcessJsonRequestsGetResult[0].Message);
                $("#MultiResponse2_GET").html(data.ProcessJsonRequestsGetResult[1].Date.toString());
                $("#MultiResponse3_GET").html(data.ProcessJsonRequestsGetResult[2].ReversedString);
            },
            error: function (xhr, desc, err) {
                console.log(xhr);
                console.log("Desc: " + desc + "\nErr:" + err);
            }
        });
    });

    $("#executeOneWayCommand").click(function () {

        var cacheKey = $("#cacheKey").val();
        var cacheValue = $("#cacheValue").val();

        $("#cacheKey").val("");
        $("#cacheValue").val("");

        $.ajax({
            url: "http://localhost/Sample.ServiceLayer.Host/Service.svc/json/post/oneway",
            type: 'post',
            contentType: "application/json",
            data: '{"requests":[{"__type":"SetCacheCommand:#Sample.Common.RequestsAndResponses","CacheKey":"' + cacheKey + '", "CacheValue":"' + cacheValue + '"}]}',
            success: function (data, status) {
                $("#onewaysent").html("One way request has been sent").fadeOut(3000);
            },
            error: function (xhr, desc, err) {
                console.log(xhr);
                console.log("Desc: " + desc + "\nErr:" + err);
            }
        });
    });

    $("#getCacheKeyExecute").click(function () {

        var cacheKey = $("#getCacheKey").val();

        $("#getCacheKey").val("");

        $.ajax({
            url: "http://localhost/Sample.ServiceLayer.Host/Service.svc/json/post",
            type: 'post',
            contentType: "application/json",
            data: '{"requests":[{"__type":"GetCacheRequest:#Sample.Common.RequestsAndResponses","CacheKey":"' + cacheKey + '"}]}',
            success: function (data, status) {
                $("#retrievedCacheValue").html(data.ProcessJsonRequestsPostResult[0].CacheValue);
            },
            error: function (xhr, desc, err) {
                console.log(xhr);
                console.log("Desc: " + desc + "\nErr:" + err);
            }
        });
    });

});

