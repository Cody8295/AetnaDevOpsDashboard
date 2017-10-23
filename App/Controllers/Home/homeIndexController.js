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
    
            var builds = response.data.split(";");

            var startTime = builds[0];
            
            var started = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var times = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            for (var i = 0; i < 24; i++) {
                var occ = builds[i].split(",");
                times[i] = occ[0];
                started[i] = occ[1];
            }
            
            var ctx = document.getElementById("canvas");

            var myChart = new Chart(ctx, {
                type: 'line',

                data: {
                    labels: [times[0], times[1], times[2], times[3], times[4], times[5], times[6], times[7], times[8], times[9], times[10], times[11], times[12], times[13], times[14], times[15], times[16], times[17], times[18], times[19], times[20], times[21], times[22], times[23]],
                    datasets: [{

                        label: 'Deployments Started',
                        data: [started[0], started[1], started[2], started[3], started[4], started[5], started[6], started[7], started[8], started[9], started[10], started[11], started[12], started[13], started[14], started[15], started[16], started[17], started[18], started[19], started[20], started[21], started[22], started[23]],
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