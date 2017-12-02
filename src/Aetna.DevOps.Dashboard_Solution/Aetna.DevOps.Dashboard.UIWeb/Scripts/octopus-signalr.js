$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = true; //debugging
    $.connection.hub.start();
});
function onChange(currentState) {

var scope = angular.element($('.octopus-column')).scope();


    console.log("UDPATING"); //debugging

    if (currentState.IsChanged["ProjectGroups"]) {
        //console.log(currentState.ProjectGroups);
        scope.projectGroups = currentState.ProjectGroups;
        // Display an indicator
    }

    if (currentState.IsChanged["Projects"]) {
        //console.log(currentState.Projects);
        scope.projects = currentState.Projects;
        // Display an indicator
    }

    if (currentState.IsChanged["Lifecycles"]) {
        //console.log(currentState.Lifecycles);
        scope.lifecycles = currentState.Lifecycles; // .Count if Lifecycles object is added
        // Display an indicator
    }

    if (currentState.IsChanged["Environments"]) {
        //console.log(currentState.Environments);
        scope.environments = currentState.Environments;
        // Display an indicator
    }

    if (currentState.IsChanged["Deploys"]) {
        //console.log(currentState.Deploys);

        /*
        var active = 0;
        currentState.Deploys.forEach(function(deploy) {
            if (deploy != null && deploy.Category === "DeploymentStarted") active++; //Not correct (shows all started in past 24 hours, not just currently "started")
        }); 
        scope.numActiveDeploys = active;*/

        scope.deploys = currentState.Deploys;
        //Display an indicator
    }

    scope.$apply();
}