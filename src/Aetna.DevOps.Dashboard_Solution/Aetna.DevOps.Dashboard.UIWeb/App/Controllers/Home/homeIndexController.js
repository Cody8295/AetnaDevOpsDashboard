(function () {

    var app = angular.module("app");

    app.controller('projectGroupListController', function ($scope, $http) {
        $http.get("/api/Octo/ProjectList").then(function (response) {
            $scope.projectGroupList = response.data;
        });
    });

    app.controller('environmentListController', function ($scope, $http) {
        $http.get("/api/Octo/environmentList").then(function (response) {
            $scope.environmentList = response.data;
        });
    });

    app.controller('projectController', function ($scope, $http) {
        $http.get("/api/Octo/projects").then(function (response) {
            $scope.projects = response.data;
        });
    });

    app.controller('projectGroupController', function ($scope, $http) {
        $http.get("/api/Octo/projectGroups").then(function (response) {
            $scope.projectGroups = response.data;
        });
    });

    app.controller('environmentController', function ($scope, $http) {
        $http.get("/api/Octo/environments").then(function (response) {
            $scope.environments = response.data;
        });
    });

    app.controller('lifecyclesController', function ($scope, $http) {
        $http.get("/api/Octo/lifecycles").then(function (response) {
            $scope.lifecycles = response.data;
        });
    });
    
    var homeIndexController = function ($scope, $http) {
        /*
        $http.get("/api/Octo/projectGroups").then(function (response) {
            $(".projectGroups").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });
        $http.get("/api/Octo/projects").then(function (response) {
            $(".projects").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });
        */
        function getReleases(projectName) {
            var releases = [];
            $http.get("/api/Octo/projectProgression?project=" + projectName).then(function (response) {
                response.data.forEach(function (rel) {
                    releases.push(rel);
                });
            });
            return releases;
        }

        $http.get("/api/Octo/projectsInfo").then(function (response) {
            function makeTimeLine(projects, pName) {
                var proj;
                projects.forEach(function (p) {
                    if (p.name == pName) { proj = p; }
                });
                if (proj === undefined) { return; }
                var releases = getReleases(proj.id);
                var dates = [];
                $("#tl").html("<div id='timeline-embed'></div>");
                setTimeout(function() {
                    for (var x = 0; x < releases.length; x++) {
                        var r = releases[x];
                        var date = {
                            "startDate": r.assembled,
                            "endDate": r.assembled,
                            "headline": r.version,
                            "text": (r.releasenotes === undefined ? "No description" : r.releasenotes)
                        };
                        dates.push(date);
                    }
                    var dataObj = {
                        "timeline":
                        {
                            "headline": "Progression timeline for " + pName,
                            "type": "default",
                            "text": "<p>A brief history of the projects releases</p>",
                            "date": dates
                        }
                    };
                    
                    
                    createStoryJS({
                        width: '100%',
                        height: '1000',
                        source: dataObj,
                        embed_id: 'timeline-embed'
                    });
                }, 1000);   
            }

            var htmlProjects = "";
            var projects = [];
            response.data.forEach(function (p) {
                projects.push(p);
                htmlProjects += "<a href=\"javascript:void(0)\" onclick=\"" +
                    "\" class=\"list-group-item list-group-item-info\" data-toggle=\"tooltip\" data-original-title=\"" + p.lifecycle +
                    "\" style=\"display:block;overflow: hidden; height:70px; padding: 3px 10px;\">" +
                    "<h4 class=\"list-group-item-heading\">" + p.name + "</h4>" +
                    "<p class=\"list-group-item-text\">" + p.groupId + "</p></a> ";
            });
            $(".projectsInfo").replaceWith(htmlProjects);
            $(".projectsInfo").show();
            $('.list-group-item').on('click', function (e) {
                //var previous = $(this).closest(".list-group").children(".active");
                $(".projectsInfo").each(function (pr) {
                    pr.removeClass('active');
                });
                $(e.target).addClass('active'); // activated list-item
                var projName = $(this)[0].getElementsByTagName("h4")[0].innerHTML;
                console.log(projName);
                makeTimeLine(projects, projName);
                $('#projectModal').modal('show');
            });
        });

        /*
        $http.get("/api/Octo/lifecycles").then(function (response) {
            $(".lifecycles").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });

        $http.get("/api/Octo/environments").then(function (response) {
            $(".numEnvironments").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });

        $http.get("/api/Octo/environmentList").then(function (response) {
            var replace = "<div id=\"environments\" class=\"collapsible panel-collapse collapse\"><ul class=\"list-group\">";
            response.data.forEach(function (d) {
                replace += "<li class=\"list-group-item\">" + d.name + "<span class=\"badge badge-default badge-pill\">" + d.description + "</span></li>";
            })
            replace += "</ul></div>";
            $(".environments").replaceWith(replace);
        });
        */

        /*
        $http.get("/api/Octo/ProjectList").then(function(response) {
            var replace = "<div id=\"projectGroupList\" class=\"collapsible panel-collapse collapse\"><ul class=\"list-group\">";
            response.data.forEach(function (d) {
                replace += "<li class=\"list-group-item\">" + d.groupName + "<span class=\"badge badge-default badge-pill\">" + d.projectList.count + "</span></li>";
            })
            replace += "</ul></div>";
            $(".projectGroupList").replaceWith(replace);
        }); */
        
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

            function parseISOLocal(s) {
                var b = s.split(/\D/);
                return new Date(b[0], b[1] - 1, b[2], b[3], b[4], b[5]);
            }

            response.data.forEach(function (d) {
                //console.log(d);
                var timeString = moment(d.timeAndDate);
                var rightNow = moment();
                var hour = timeString.hour();
                var timeDiff = timeString.diff(rightNow, 'hours');
                hour = 23 - (timeDiff<0?timeDiff*-1:timeDiff);
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
                //console.log($(".btn-group").find(".active").attr("id"));
            }

            function setupLineGraph() {
                document.getElementById("canvas").onclick = function (e) {
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
                        var dt = moment(d.dateTime);
                        var timePassed = dt.fromNow();

                        //console.log(msg + "," + timePassed);
                        var environData = "";

                        // Triply nested, double terminating quotations are really fun
                        // -> onclick="element.html('\\"someText\\"')"
                        

                        d.environs.forEach(function (e) {
                            function formatEnvironment(msg, dt, id, name, description) {
                                var machineList = "";
                                e.machines.machines.forEach(function (machine) {
                                        var isInProcessStr = "<i class=\\'fa fa-cog faa-spin animated fa-5x\\'></i>";
                                        machineList += "<li class=\\'list-group-item\\' ><h4 class=\\'list-group-item-header\\'>" +
                                            machine.name + "<span class=\\'pull-right\\'>" + (machine.isInProcess==="true"?isInProcessStr:"") + "<small>" + machine.status + "</small></span></h4><p class=\\'list-group-item-text\\'>" +
                                            machine.statusSummary + "</p></li>";
                                });
                                return "<div class=\\'card text-center\\'><div class=\\'card-header\\'>" + id +
                                    "</div><div class=\\'card-block\\'><h4 class=\\'card-title\\'>" + name + "</h4>" +
                                    "<p class=\\'card-text\\'>" + (description === undefined ? "No description" : description) + "</p></div>" +
                                    "<ul class=\\'list-group list-group-flush\\' style=\\'overflow-y:auto;\\'>" +
                                    machineList +
                                    "</ul>" +
                                    "<div class=\\'card-footer text-muted\\'>" + dt + "</div></div>";
                            }
                            environData += formatEnvironment(msg, timePassed, e.id, e.name, e.description);
                        });
                        htmlDeploys += "<a href=\"javascript:void(0)\" onclick=\"$('.deployData').html('" +
                            environData + "'); $('.deployData').show();\" class=\"list-group-item " + coloredListElement(cat) +
                            "\" style=\"display:block;overflow: hidden; height:100px; padding: 3px 10px;\">" +
                            "<h4 class=\"list-group-item-heading\">" + d.environs[0].name +
                            "<div class='pull-right'><small>" + timePassed + "</small></div></h4>" +
                            "<p class=\"list-group-item-text\">"  + msg + "</p></a>";
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