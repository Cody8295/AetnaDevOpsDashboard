$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = true; //debugging
    $.connection.hub.start();
});
function onChange(currentState) {
    console.log("UDPATING"); //debugging
    if (currentState.isChanged["NumProjectGroups"]) {
        console.log("PG");
        $(".projectGroups").text(currentState.NumProjectGroups);
        // Display an indicator
    }
    if (currentState.isChanged["NumProjects"]) {
        console.log("Pj"); //debugging
        $(".projects").text(currentState.NumProjects);
        // Display an indicator
    }
    if (currentState.isChanged["NumLifecycles"]) {
        console.log("Lf"); //debugging
        $(".lifecycles").text(currentState.NumLifecycles);
        // Display an indicator
    }
    if (currentState.isChanged["NumEnvironments"]) {
        console.log("nE"); //debugging
        $(".numEnvironments").text(currentState.NumEnvironments);
        // Display an indicator
    }

    var inside = "";
    for (var env in currentState.Environments) {
        inside += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + env.name + "<span class=\"pull-right\">" + env.description + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
    }
    $(".environments").html(inside);

    inside = "";
    for (var pg in currentState.ProjectGroups) {
        inside += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + pg.groupName + "<span class=\"pull-right\">" + pg.projectList.count + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
    }
    $(".projectGroupList").html(inside);
    
}