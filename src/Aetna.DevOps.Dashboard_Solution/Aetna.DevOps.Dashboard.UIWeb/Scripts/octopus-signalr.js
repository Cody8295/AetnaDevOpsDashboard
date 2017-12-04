$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = false; //debugging
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
        if ($.connection.hub.logging) console.log("Live Deploys Update");
        scope.deploys = currentState.deploys;
        scope.liveDeploys = currentState.liveDeploys;
        //Display an indicator
    }


    if (currentState.isChanged.deployEvents) {
        if ($.connection.hub.logging) console.log("Deploy Events Update");
        scope.deployEvents = currentState.deployEvents;

        // These 5 arrays hold information about deployments over the past 24 hours
        var failed = [];
        failed.length = 24;
        failed.fill(0);
        var succeeded = [];
        succeeded.length = 24;
        succeeded.fill(0);
        var queued = [];
        queued.length = 24;
        queued.fill(0);
        var started = [];
        started.length = 24;
        started.fill(0);
        var allDeploys = [];
        allDeploys.length = 24;
        allDeploys.fill([]);

        // total 24 hour counts for each deploy type
        var failedCount = 0;
        var succeededCount = 0;
        var queuedCount = 0;
        var startedCount = 0;

        for (var index in scope.deployEvents) {
            d = scope.deployEvents[index];
            var timeString = moment(d.timeAndDate);
            var rightNow = moment();
            var hour = timeString.hour();
            var timeDiff = timeString.diff(rightNow, 'hours');
            hour = 23 - (timeDiff < 0 ? timeDiff * -1 : timeDiff);
            allDeploys[hour] = (allDeploys[hour] === undefined ? [] : allDeploys[hour]).concat(
                {
                    "message": d.message,
                    "category": d.category,
                    "dateTime": timeString,
                    "environs": d.environs,
                    "webUrl": d.webUrl
                }
            );
            if (d.category === "DeploymentFailed") {
                failed[hour] = (failed[hour] !== undefined ? failed[hour] + 1 : 1)
                failedCount++;
            };
            if (d.category === "DeploymentSucceeded") {
                succeeded[hour] = (succeeded[hour] !== undefined ? succeeded[hour] + 1 : 1)
                succeededCount++;
            };
            if (d.category === "DeploymentQueued") {
                queued[hour] = (queued[hour] !== undefined ? queued[hour] + 1 : 1)
                queuedCount++;
            };
            if (d.category === "DeploymentStarted") {
                started[hour] = (started[hour] !== undefined ? started[hour] + 1 : 1)
                startedCount++;
            };
        }

        scope.octoChartData = [started, queued, succeeded, failed];

        scope.octoPieData = [startedCount, queuedCount, succeededCount, failedCount];

        //Display an indicator
    }

    scope.$apply();
}