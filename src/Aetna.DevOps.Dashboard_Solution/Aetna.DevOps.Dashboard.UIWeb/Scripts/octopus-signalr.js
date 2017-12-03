$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = true; //debugging
    $.connection.hub.start();
});
function onChange(currentStateJson) {
    var debugging = false;
    var currentState = JSON.parse(currentStateJson);
    var scope = angular.element($('.octopus-column')).scope();


    if (debugging) console.log(currentState);

    if (currentState.isChanged.projectGroups) {
        if (debugging) console.log("ProjectGroups Update");
        scope.projectGroups = currentState.projectGroups;
        // Display an indicator
    }
    
    if (currentState.isChanged.projects) {
        if (debugging) console.log("Projects Update");
        $(".project").tooltip("hide");
        scope.projects = currentState.projects;
        // Display an indicator
    }
    
    if (currentState.isChanged.lifecycles) {
        if (debugging) console.log("ProjectGroups Update");
        scope.lifecycles = currentState.lifecycles; // .Count if Lifecycles object is added
        // Display an indicator
    }

    if (currentState.isChanged.environments) {
        if (debugging) console.log("Environments Update");
        scope.environments = currentState.environments;
        // Display an indicator
    }

    if (currentState.isChanged.deploys) {
        if (debugging) console.log("Deploys Update");

        /*
        var active = 0;
        currentState.deploys.forEach(function(deploy) {
            if (deploy != null && deploy.Category === "DeploymentStarted") active++; //Not correct (shows all started in past 24 hours, not just currently "started")
        }); 
        scope.numActiveDeploys = active;*/

        scope.deploys = currentState.deploys;
        //Display an indicator
    }

    scope.$apply();
}