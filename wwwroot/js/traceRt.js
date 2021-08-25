"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/traceRtHub").build();

//Disable send button until connection is established
document.getElementById("startTrace").disabled = true;

connection.on("Notify", function (messageType, message) {

    switch (messageType) {
        case "hop":
            // message contains hop details
            var pingResult = JSON.parse(message);

            var $div = $("<div>", { "class": "row" });

            $div.append("<code class='col-sm-1'>" + pingResult.hop + "</code>");
            $div.append("<code class='col-sm-1'>" + pingResult.FormattedEt1 + "</code>");
            $div.append("<code class='col-sm-1'>" + pingResult.FormattedEt2 + "</code>");
            $div.append("<code class='col-sm-1'>" + pingResult.FormattedEt3 + "</code>");
            $div.append("<code class='col-sm-8'>" + pingResult.FormattedHostName + "</code>");

            $("#results").append($div);
            break;
        case "end":
            // no more messages expected
            $('#startTrace').toggleClass('animate');
            break;
        case "error":
            // an error occured, message contains details
            $('#startTrace').toggleClass('animate');
            $('#error').append(message);
            $('#error').show();
            break;
        default:
        // code block
    }
});

connection.start().then(function () {
    document.getElementById("startTrace").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("startTrace").addEventListener("click", function (event) {
    var hostName = document.getElementById("host").value;
    $('#startTrace').toggleClass('animate');
    $('#error').hide();
    $('#error').empty();
    $('#results').empty();
    connection.invoke("StartTrace", hostName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

