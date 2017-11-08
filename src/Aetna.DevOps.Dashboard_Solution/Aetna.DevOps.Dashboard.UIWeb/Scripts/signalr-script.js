$().ready(function () {
    var $deployAction = $.connection.deployAction;
    $deployAction.client.onAction = onAction;
    $.connection.hub.start();
    console.log("hi");
});
function onAction(currentState) {
    $("#startTime").text(currentState.StartTime);

    $("#requestsProcessed").text(currentState.RequestsProcessed);
    $("#bytesProcessed").text(currentState.BytesProcessed);
}