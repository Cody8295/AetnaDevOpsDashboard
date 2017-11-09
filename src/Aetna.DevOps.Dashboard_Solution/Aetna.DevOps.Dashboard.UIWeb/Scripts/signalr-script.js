$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.start();
});
function onAction(currentState) {
    $("#startTime").text(currentState.StartTime);

    $("#requestsProcessed").text(currentState.RequestsProcessed);
    $("#bytesProcessed").text(currentState.BytesProcessed);
}