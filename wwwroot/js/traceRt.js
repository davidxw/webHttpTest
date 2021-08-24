"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/traceRtHub").build();

//Disable send button until connection is established
document.getElementById("startTrace").disabled = true;

connection.on("Notify", function (message) {

    var pingResult = JSON.parse(message);

    var $div = $("<div>", { "class": "row" });

    $div.append("<code class='col-sm-1'>" + pingResult.hop + "</code>");
    $div.append("<code class='col-sm-1'>" + pingResult.FormattedEt1 + "</code>");
    $div.append("<code class='col-sm-1'>" + pingResult.FormattedEt2 + "</code>");
    $div.append("<code class='col-sm-1'>" + pingResult.FormattedEt3 + "</code>");
    $div.append("<code class='col-sm-8'>" + pingResult.FormattedHostName + "</code>");

    $("#results").append($div);
});

connection.start().then(function () {
    document.getElementById("startTrace").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("startTrace").addEventListener("click", function (event) {
    var hostName = document.getElementById("host").value;
    connection.invoke("StartTrace", hostName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

