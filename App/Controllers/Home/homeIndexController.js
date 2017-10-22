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
            
            var builds = response.data.split(",");
            
            var started = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var success = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var failed = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            for (var i = 0; i < builds.length; i ++) {
                var build = builds[i].split(":");
                if (build[0] === "DeploymentSucceeded")
                    success[build[1]]++;
                else if (build[0] === "DeploymentStarted")
                    started[build[1]]++;
                else if (build[0] === "DeploymentFailed")
                    failed[build[1]]++;
            }
            var ctx = document.getElementById("canvas");

            var myChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: ["00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00"],
                    datasets: [{
                        label: 'Deployments Started',
                        data: [started[0], started[1], started[2], started[3], started[4], started[5], started[6], started[7], started[8], started[9], started[10], started[11], started[12], started[13], started[14], started[15], started[16], started[17], started[18], started[19], started[20], started[21], started[22], started[23]],
                        backgroundColor: [
                            'rgba(255, 0, 0, 0.2)'
                            
                        ],
                        borderColor: [
                            'rgba(0,0,0,1)'
                        ],
                        borderWidth: 1
                    },
                        {
                            label: 'Deployments Successful',
                            data: [success[0], success[1], success[2], success[3], success[4], success[5], success[6], success[7], success[8], success[9], success[10], success[11], success[12], success[13], success[14], success[15], success[16], success[17], success[18], success[19], success[20], success[21], success[22], success[23]],
                            backgroundColor: [
                                
                                'rgba(0, 0, 235, 0.2)'
                                
                            ],
                            borderColor: [
                                'rgba(0,0,0,1)'  
                            ],
                            borderWidth: 1
                        },
                        {
                            label: 'Deployments Failed',
                            data: [failed[0], failed[1], failed[2], failed[3], failed[4], failed[5], failed[6], failed[7], failed[8], failed[9], failed[10], failed[11], failed[12], failed[13], failed[14], failed[15], failed[16], failed[17], failed[18], failed[19], failed[20], failed[21], failed[22], failed[23]],
                            backgroundColor: [

                                'rgba(0, 255, 0, 0.2)'

                            ],
                            borderColor: [
                                'rgba(0,0,0,1)'
                            ],
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