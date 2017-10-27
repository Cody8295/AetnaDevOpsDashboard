(function () {

    var app = angular.module("app");

    var homeIndexController = function ($scope, $http) {

        $http.get("/api/Octo/projectGroups").then(function (response) {
            $(".projectGroups").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });
        $http.get("/api/Octo/projects").then(function (response) {
            $(".projects").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });
        $http.get("/api/Octo/lifecycles").then(function (response) {
            $(".lifecycles").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });
        $http.get("/api/Octo/environments").then(function (response) {
            var replace = "<div id=\"environments\" class=\"environment panel-collapse collapse environment\">";
            var count = 0;
            response.data.forEach(function (d) {
                replace += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + d.name + "<span class=\"pull-right\">" + d.numMachines + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
                count += 1;
            })
            replace += "</div>";
            $(".environments").replaceWith(replace);
            $(".numEnvironments").replaceWith("<span class=\"pull-right\">" + count + "</span>");

        });

        $http.get("/api/Octo/deploys").then(function (response) {
            var hr = new Date();
            var startTime = hr.getHours();
            var times = [];
            var failed = []; failed.length = 24; failed.fill(0);
            var succeeded = []; succeeded.length = 24; succeeded.fill(0);
            var queued = []; queued.length = 24; queued.fill(0);
            var started = []; started.length = 24; started.fill(0);

            var lastHour = 1;
            for (var hs = startTime-24; hs < startTime; hs++)
            {
                var s = (hs < 0 ? 24 + hs : (hs == 0 ? 12 : hs));
                if (lastHour == 11 || lastHour == 23) {
                    if (lastHour == 11) { times.push("Noon"); }
                    if (lastHour == 23) { times.push("Midnight"); }
                    lastHour = s;
                    continue;
                }
                times.push((s <= 11 ? s + "AM" : (s == 12 ? 12 : s % 12) + "PM"));
                lastHour = s;
            }
            
            response.data.forEach(function (d) {
                var timeString = new Date(d.timeAndDate);
                var hour = timeString.getHours();
                hour = hour + (24 - startTime);
                if (d.category === "DeploymentFailed") {
                    failed[hour] = (failed[hour] !== undefined ? failed[hour] + 1 : 1)
                };
                if (d.category === "DeploymentSucceeded") {
                    succeeded[hour] = (succeeded[hour] !== undefined ? succeeded[hour] + 1 : 1)
                };
                if (d.category === "DeploymentQueued") {
                    queued[hour] = (queued[hour] !== undefined ? queued[hour] + 1 : 1)
                };
                if (d.category === "DeploymentStarted") {
                    started[hour] = (started[hour] !== undefined ? started[hour] + 1 : 1)
                };
            });

            var failedCounts = $.map(failed, function (k, v) {
                return v;
            });

            var startedCounts = $.map(started, function (k, v) {
                return v;
            });
            var succeededCounts = $.map(succeeded, function (k, v) {
                return v;
            });
            var queuedCounts = $.map(queued, function (k, v) {
                return v;
            });
            console.log(queued);
            
            var ctx = document.getElementById("canvas");
            ctx.height = 300;
            var myChart = new Chart(ctx, {
                type: 'line',

                data: {
                    labels: times,
                    datasets: [{

                        label: 'Deployments Started',
                        data: started,
                        backgroundColor: 'rgba(0, 0, 255, 0.2)',
                        borderColor: 'rgba(0,0,255,1)',
                        borderWidth: 1
                    },
                        {

                            label: 'Deployments Queued',
                            data: queued,
                            backgroundColor: 'rgba(53, 53, 53, 0.2)',
                            borderColor: 'rgba(0,0,0,1)',
                            borderWidth: 1
                        },
                        {

                            label: 'Deployments Succeeded',
                            data: succeeded,
                            backgroundColor: 'rgba(0, 255, 0, 0.2)',
                            borderColor: 'rgba(0,0,0,1)',
                            borderWidth: 1
                        },
                        {

                            label: 'Deployments Failed',
                            data: failed,
                            backgroundColor: 'rgba(255, 0, 0, 0.2)',
                            borderColor: 'rgba(0,0,0,1)',
                            borderWidth: 1
                        }
                    ]
                },
                options: {
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: true,
                                stepSize: 1
                            }
                        }],
                        xAxes: [{
                            ticks: {
                                stepSize: 1,
                                autoSkip: false
                            }
                        }]
                    }
                }
            });
            
        });
        
    };

    app.controller("homeIndexController", homeIndexController);

}());