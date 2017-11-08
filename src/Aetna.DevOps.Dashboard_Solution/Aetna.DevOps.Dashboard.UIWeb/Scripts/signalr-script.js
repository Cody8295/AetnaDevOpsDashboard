$().ready(function () {
    var $deployAction = $.connection.deployAction;
    $deployAction.client.onAction = onAction;
    $.connection.hub.start();
});
function onAction(currentState) {
    $("#startTime").text(currentState.StartTime);

    $("#requestsProcessed").text(currentState.RequestsProcessed);
    $("#bytesProcessed").text(currentState.BytesProcessed);
}