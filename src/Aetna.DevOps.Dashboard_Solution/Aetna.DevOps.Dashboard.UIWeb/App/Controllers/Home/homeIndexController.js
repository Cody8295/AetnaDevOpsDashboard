(function () {

    var app = angular.module("app");

    var homeIndexController = function ($scope, $http) {

        $http.get("/api/Octo/projectGroups").then(function (response) {
            $(".projectGroups").text(response.data);
        });
        $http.get("/api/Octo/projects").then(function (response) {
            $(".projects").text(response.data);
        });
        $http.get("/api/Octo/lifecycles").then(function (response) {
            $(".lifecycles").text(response.data);
        });

        $http.get("/api/Octo/environments").then(function (response) {
            $(".numEnvironments").text(response.data);
        });

        $http.get("/api/Octo/environmentList").then(function (response) {
            var inside = "";
            response.data.forEach(function (d) {
                inside += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + d.name + "<span class=\"pull-right\">" + d.description + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
            })
            $(".environments").html(inside);
        });

        $http.get("/api/Octo/ProjectList").then(function(response) {
            var inside = "";
            response.data.forEach(function (d) {
                inside += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + d.groupName + "<span class=\"pull-right\">" + d.projectList.count + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
            })
            $(".projectGroupList").html(inside);
        });
        
        $http.get("/api/Octo/deploys").then(function (response) {
            $(document).ready(function () { // via https://stackoverflow.com/questions/9446318/bootstrap-tooltips-not-working
                $("body").tooltip({ selector: '[data-toggle=tooltip]' });
            }); // tooltip fix

            var theLineGraph, thePieChart, theBarGraph; // global vars for each graph
            var hr = new Date(); // the date and time right now
            var startTime = hr.getHours(); // the "start" time (really the time the graph ends at)
            var times = []; // array of hours for the graphs labels

            // These 5 arrays hold information about deployments over the past 24 hours
            var failed = []; failed.length = 24; failed.fill(0);
            var succeeded = []; succeeded.length = 24; succeeded.fill(0);
            var queued = []; queued.length = 24; queued.fill(0);
            var started = []; started.length = 24; started.fill(0);
            var allDeploys = []; allDeploys.length = 24; allDeploys.fill([]);

            // total 24 hour counts for each deploy type
            var failedCount = 0;
            var succeededCount = 0;
            var queuedCount = 0;
            var startedCount = 0;

            var lastHour = 1; // used to determine if 12 o'clock is noon or midnight

            for (var hs = startTime - 23; hs <= startTime; hs++) {
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
                console.log(d);
                var timeString = new Date(0);
                timeString.setUTCSeconds(d.timeAndDate);
                var hour = timeString.getHours();
                hour = hour + (23 - startTime);
                //console.log(timeString + ",");
                allDeploys[hour] = (allDeploys[hour]===undefined?[]:allDeploys[hour]).concat({"message":d.message, "category":d.category, "dateTime":timeString, "environs":d.environs});
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
            });
            
            function lineGraph() {
                var ctx = document.getElementById("canvas");
                ctx.height = 300;
                theLineGraph = new Chart(ctx, {
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
                        }]
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
            }

            

            function pieChart() {
                var ctx = document.getElementById("canvas");
                ctx.height = 300;
                const genLabelsDef = Chart.defaults.pie.legend.labels.generateLabels;
                const genLabelsNew = function (chart) {
                    if (genLabelsNew['firstCall']) {
                        const meta = chart.getDatasetMeta(0);
                        meta.data[0].hidden = true; // hide Started deploys
                        meta.data[1].hidden = true; // hide Queued deploys
                        genLabelsNew['firstCall'] = false;
                    }
                    return genLabelsDef(chart);
                }
                genLabelsNew['firstCall'] = true;
                thePieChart = new Chart(ctx, {
                    type: 'pie',
                    options: {
                        legend: {
                            labels: {
                                generateLabels: genLabelsNew
                            }
                        }
                    },
                    data: {
                        datasets: [{
                            data: [startedCount, queuedCount, failedCount, succeededCount],
                            backgroundColor: [
                                'rgba(0, 0, 255, 0.2)',
                                'rgba(255, 255, 0, 0.2)',
                                'rgba(255, 0, 0, 0.2)',
                                'rgba(0, 255, 0, 0.2)'
                            ]
                        }],
                        labels: ["Started","Queued","Failed","Succeeded"]
                    }
                });
            }

            function barGraph() {
                var ctx = document.getElementById("canvas");
                ctx.height = 300;
                theBarGraph = new Chart(ctx, {
                    type: 'bar',
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
                        }]
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
            }

            function coloredListElement(c) {
                if (c == "DeploymentStarted") { return "list-group-item-info"; }
                if (c == "DeploymentQueued") { return "list-group-item-warning"; }
                if (c == "DeploymentFailed") { return "list-group-item-danger"; }
                if (c == "DeploymentSucceeded") { return "list-group-item-success"; }
            }

            function checkRadio() {
                console.log($(".btn-group").find(".active").attr("id"));
            }

            function setupLineGraph() {
                document.getElementById("canvas").onclick = function (e) {
                    console.log("Line graph clicked");
                    var htmlDeploys = "";
                    var points = theLineGraph.getElementsAtEvent(e);
                    if (points[0] === undefined) { // user didn't click on a point
                        $(".list-group").replaceWith("");
                        //$(".list-group").hide();
                        return;
                    }
                    allDeploys[points[0]._index].forEach(function (d) {
                        var msg = d.message;
                        var cat = d.category;
                        var dt = d.dateTime;
                        console.log(msg + "," + cat);
                        var environData = "";
                        d.environs.forEach(function (e) {
                            environData += msg + "<br>" + dt + "<br>" + e.id + "<br>" + e.name + (e.description === undefined ? "" : "(" + e.description + ")<br><br>");
                            if (e.machines !== undefined) {
                                environData += "Machines for " + e.name + ":<br>";
                                e.machines.forEach(function (m) {
                                    environData += m.id + "<br>" + m.name + "<br>" + m.url + "<br>" + m.status + "<br>" + m.statusSummary + "<br>Is running: " + m.isInProcess + "<br><br>";
                                });
                            }

                        });
                        htmlDeploys += "<a href=\"javascript:void(0)\" onclick=\"$('.deployData').html('" + environData +"'); $('.deployData').show();\" class=\"list-group-item " + coloredListElement(cat) + "\" data-toggle=\"tooltip\" data-original-title=\"" + dt + "\" style=\"display:block;overflow: hidden; height:70px; padding: 3px 10px;\">" + msg + "</a>";
                    });
                    if (htmlDeploys == "") { return; }
                    $("#octoModal").modal("show");
                    $(".list-group").replaceWith(htmlDeploys);
                    $(".list-group").show();
                };
            }

            
            lineGraph(); // default representation
            setupLineGraph();

            $(document).ready(function () {
                $('.btn-group .btn').mouseup(function (e) {
                    setTimeout(function () {
                        var btnId = $(".btn-group").find(".active").attr("id");
                        if (btnId == "opt1") {
                            if (theLineGraph !== undefined) { return; }
                            if (theBarGraph !== undefined) { theBarGraph.destroy(); theBarGraph = undefined; }
                            if (thePieChart !== undefined) { thePieChart.destroy(); thePieChart = undefined; }
                            lineGraph();
                            setupLineGraph();
                        }
                        if (btnId == "opt2") {
                            if (thePieChart !== undefined) { return; }
                            if (theLineGraph !== undefined) { theLineGraph.destroy(); theLineGraph = undefined; }
                            if (theBarGraph !== undefined) { theBarGraph.destroy(); theBarGraph = undefined; }
                            pieChart();
                        }
                        if (btnId == "opt3") {
                            if (theBarGraph !== undefined) { return; }
                            if (theLineGraph !== undefined) { theLineGraph.destroy(); theLineGraph = undefined; }
                            if (thePieChart !== undefined) { thePieChart.destroy(); thePieChart = undefined; }
                            barGraph();
                        }
                    }, 300);
                    
                });
            });

            
        });
    };
    

    app.controller("homeIndexController", homeIndexController);

}());