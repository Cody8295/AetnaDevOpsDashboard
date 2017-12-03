$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = true; //debugging
    $.connection.hub.start();
});
function onChange(currentStateJson) {
    var currentState = JSON.parse(currentStateJson);
    var scope = angular.element($('.octopus-column')).scope();


    if ($.connection.hub.logging) console.log(currentState);

    if (currentState.isChanged.projectGroups) {
        if ($.connection.hub.logging) console.log("ProjectGroups Update");
        scope.projectGroups = currentState.projectGroups;
        // Display an indicator
    }
    
    if (currentState.isChanged.projects) {
        if ($.connection.hub.logging) console.log("Projects Update");
        $(".project").tooltip("hide");
        scope.projects = currentState.projects;
        // Display an indicator
    }
    
    if (currentState.isChanged.lifecycles) {
        if ($.connection.hub.logging) console.log("ProjectGroups Update");
        scope.lifecycles = currentState.lifecycles; // .Count if Lifecycles object is added
        // Display an indicator
    }

    if (currentState.isChanged.environments) {
        if ($.connection.hub.logging) console.log("Environments Update");
        scope.environments = currentState.environments;
        // Display an indicator
    }

    if (currentState.isChanged.deploys) {
        if ($.connection.hub.logging) console.log("Deploys Update");
        scope.deploys = currentState.deploys;
        scope.liveDeploys = currentState.liveDeploys;
        //Display an indicator
    }

    if (currentState.isChanged.liveDeploys) {
        if ($.connection.hub.logging) console.log("Live Deploys Update");
        scope.liveDeploys = currentState.liveDeploys;
        //Display an indicator
    }

    scope.$apply();
}