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
        scope.projectGroups = currentState.ProjectGroups.length;

        scope.projectGroupList = currentState.ProjectGroups;
        // Display an indicator
    }

    if (currentState.IsChanged["Projects"]) {
        scope.projects = currentState.Projects.length;
        scope.projectList = currentState.Projects;
        // Display an indicator
    }

    if (currentState.IsChanged["Lifecycles"]) {
        scope.lifecycles = currentState.Lifecycles; // .Count if Lifecycles object is added
        // Display an indicator
    }

    if (currentState.IsChanged["Environments"]) {
        scope.environments = currentState.Environments.length;

        scope.environmentList = currentState.Environments;
        // Display an indicator
    }

    if (currentState.IsChanged["Deploys"]) {
        //Update Deploys
        //Display an indicator
    }

    scope.$apply();
}