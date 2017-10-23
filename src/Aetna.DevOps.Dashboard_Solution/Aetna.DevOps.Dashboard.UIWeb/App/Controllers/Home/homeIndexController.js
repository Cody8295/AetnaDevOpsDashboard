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
            $(".environments").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });

        $http.get("/api/Octo/builds").then(function (response) {

            var buildGroups = response.data.split("|");
            //var builds = response.split(";");

            var startTime = buildGroups[0].split(";")[0];
            
            var started = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var times = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var failed = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var queued = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var succeeded = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            for (var c = 0; c <= 3; c++) {
                for (var i = 0; i < 24; i++) {
                    var occ = buildGroups[c].split(";")[i].split(",");
                    times[i] = occ[0]; //(occ[0]>12 ? 24-occ[0] : occ[0]) // 12 hour format
                    if (c == 0) { started[i] = occ[1]; }
                    if (c == 1) { succeeded[i] = occ[1]; }
                    if (c == 2) { queued[i] = occ[1]; }
                    if (c == 3) { failed[i] = occ[1]; }
                }
            }
            
            
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
                                beginAtZero: true
                            }
                        }]
                    }
                }
            });
            
        });
        
    };

    app.controller("homeIndexController", homeIndexController);

}());