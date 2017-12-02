$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = true; //debugging
    $.connection.hub.start();
});
function onChange(currentStateJson) {

    var currentState = JSON.parse(currentStateJson);
    var scope = angular.element($('.octopus-column')).scope();


    //console.log(currentState); //debugging

    if (currentState.isChanged.projectGroups) {
        //console.log(currentState.projectGroups);
        scope.projectGroups = currentState.projectGroups;
        // Display an indicator
    }

    if (currentState.isChanged.projects) {
        //console.log(currentState.projects);
        scope.projects = currentState.projects;
        // Display an indicator
    }

    if (currentState.isChanged.lifecycles) {
        //console.log(currentState.lifecycles);
        scope.lifecycles = currentState.lifecycles; // .Count if Lifecycles object is added
        // Display an indicator
    }

    if (currentState.isChanged.environments) {
        //console.log(currentState.environments);
        scope.environments = currentState.environments;
        // Display an indicator
    }

    if (currentState.isChanged.deploys) {
        //console.log(currentState.deploys);

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