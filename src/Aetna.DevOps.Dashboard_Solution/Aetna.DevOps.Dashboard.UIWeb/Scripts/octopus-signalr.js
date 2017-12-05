$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = true; //debugging
    $.connection.hub.start();
});
function onChange(projectGroups, projects, lifecycles, environments, deployEvents, deploys, liveDeploys) {
    var scope = angular.element($('.octopus-column')).scope();
    
    if (projectGroups !== "noChange") {
        if ($.connection.hub.logging) console.log("ProjectGroups Update");
        scope.projectGroups = JSON.parse(projectGroups);
        // Display an indicator
    }
    
    if (projects !== "noChange") {
        if ($.connection.hub.logging) console.log("Projects Update");
        scope.projects = JSON.parse(projects);
        // Display an indicator
    }
    
    if (lifecycles !== "noChange") {
        if ($.connection.hub.logging) console.log("ProjectGroups Update");
        scope.lifecycles = JSON.parse(lifecycles);
        // Display an indicator
    }

    if (environments !== "noChange") {
        if ($.connection.hub.logging) console.log("Environments Update");
        scope.environments = JSON.parse(environments);
        // Display an indicator
    }

    if (deploys !== "noChange") {
        if ($.connection.hub.logging) console.log("Deploys Update");
        scope.deploys = JSON.parse(deploys);
        //Display an indicator
    }

    if (liveDeploys !== "noChange") {
        if ($.connection.hub.logging) console.log("Live Deploys Update");
        console.log(liveDeploys)
        scope.liveDeploys = JSON.parse(liveDeploys);
        //Display an indicator
    }

    if (deployEvents !== "noChange") {
        if ($.connection.hub.logging) console.log("Deploy Events Update");
        scope.deployEvents = JSON.parse(deployEvents);

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